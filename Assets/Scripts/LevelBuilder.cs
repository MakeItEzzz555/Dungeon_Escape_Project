using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A student-friendly LevelBuilder for generating dungeon layouts.
/// Handles rooms, corridors, and auto-loading tiles based on keywords.
/// </summary>
public class LevelBuilder : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap background;
    public Tilemap midground;
    public Tilemap foreground;
    public Tilemap fences;

    [System.Serializable]
    public class TileCategory
    {
        public string label;
        public string[] keywords;
        public List<TileBase> tiles = new List<TileBase>();

        public TileCategory(string label, string[] keywords)
        {
            this.label = label;
            this.keywords = keywords;
        }

        public TileBase GetRandomTile()
        {
            if (tiles == null || tiles.Count == 0) return null;
            return tiles[Random.Range(0, tiles.Count)];
        }
    }

    [Header("Tile Categories")]
    public TileCategory floors = new TileCategory("Floors", new[] { "floor", "ground", "T001" });
    public TileCategory walls = new TileCategory("Walls", new[] { "wall", "T002" });
    public TileCategory corners = new TileCategory("Corners", new[] { "corner", "inner", "outer", "T003" });
    public TileCategory stairs = new TileCategory("Stairs", new[] { "stair", "stairs" });
    public TileCategory deco = new TileCategory("Deco", new[] { "deco", "prop" });
    public TileCategory fenceTiles = new TileCategory("Fences", new[] { "fence" });

    [Header("Generation Settings")]
    public int mapWidth = 40;
    public int mapHeight = 30;
    public int roomCount = 6;
    public int minRoomSize = 5;
    public int maxRoomSize = 10;
    public bool useRandomSeed = true;
    public int seed = 42;

    private struct Room
    {
        public RectInt rect;
        public Vector2Int Center => new Vector2Int((int)rect.center.x, (int)rect.center.y);
        public Room(int x, int y, int w, int h) { rect = new RectInt(x, y, w, h); }
        public bool Overlaps(Room other) { return rect.Overlaps(other.rect); }
    }

    [ContextMenu("Auto Load Tiles")]
    public void AutoLoadTiles()
    {
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:TileBase", new[] { "Assets/Tiles" });
        ClearCategories();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
            string name = tile.name.ToLower();

            if (MatchKeywords(name, floors.keywords)) floors.tiles.Add(tile);
            else if (MatchKeywords(name, walls.keywords)) walls.tiles.Add(tile);
            else if (MatchKeywords(name, corners.keywords)) corners.tiles.Add(tile);
            else if (MatchKeywords(name, stairs.keywords)) stairs.tiles.Add(tile);
            else if (MatchKeywords(name, deco.keywords)) deco.tiles.Add(tile);
            else if (MatchKeywords(name, fenceTiles.keywords)) fenceTiles.tiles.Add(tile);
        }

        Debug.Log($"Auto-loaded: {floors.tiles.Count} floors, {walls.tiles.Count} walls, {corners.tiles.Count} corners.");
#endif
    }

    private bool MatchKeywords(string name, string[] keywords)
    {
        return keywords.Any(k => name.Contains(k.ToLower()));
    }

    private void ClearCategories()
    {
        floors.tiles.Clear();
        walls.tiles.Clear();
        corners.tiles.Clear();
        stairs.tiles.Clear();
        deco.tiles.Clear();
        fenceTiles.tiles.Clear();
    }

    [ContextMenu("Clear Level")]
    public void ClearLevel()
    {
        if (background) background.ClearAllTiles();
        if (midground) midground.ClearAllTiles();
        if (foreground) foreground.ClearAllTiles();
        if (fences) fences.ClearAllTiles();
    }

    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        if (!background || !midground)
        {
            Debug.LogError("Assign Background and Midground Tilemaps first!");
            return;
        }

        if (useRandomSeed) seed = Random.Range(0, 100000);
        Random.InitState(seed);

        ClearLevel();

        List<Room> rooms = new List<Room>();
        for (int i = 0; i < roomCount * 2 && rooms.Count < roomCount; i++)
        {
            int w = Random.Range(minRoomSize, maxRoomSize + 1);
            int h = Random.Range(minRoomSize, maxRoomSize + 1);
            int x = Random.Range(1, mapWidth - w - 1);
            int y = Random.Range(1, mapHeight - h - 1);

            Room newRoom = new Room(x, y, w, h);
            if (!rooms.Any(r => r.Overlaps(newRoom)))
            {
                rooms.Add(newRoom);
                PaintRoom(newRoom);
            }
        }

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            ConnectRooms(rooms[i], rooms[i + 1]);
        }

        // Place stairs at start and end
        if (rooms.Count >= 2)
        {
            PlaceTile(rooms[0].Center, stairs.GetRandomTile(), background);
            PlaceTile(rooms[rooms.Count - 1].Center, stairs.GetRandomTile(), background);
        }

        Debug.Log($"Dungeon generated with {rooms.Count} rooms. Seed: {seed}");
    }

    private void PaintRoom(Room room)
    {
        // Floor
        for (int x = room.rect.x; x < room.rect.xMax; x++)
        {
            for (int y = room.rect.y; y < room.rect.yMax; y++)
            {
                background.SetTile(new Vector3Int(x, y, 0), floors.GetRandomTile());
            }
        }

        // Walls and Corners
        for (int x = room.rect.x - 1; x <= room.rect.xMax; x++)
        {
            for (int y = room.rect.y - 1; y <= room.rect.yMax; y++)
            {
                bool isCorner = (x == room.rect.x - 1 || x == room.rect.xMax) && 
                                (y == room.rect.y - 1 || y == room.rect.yMax);
                bool isEdge = x == room.rect.x - 1 || x == room.rect.xMax || 
                              y == room.rect.y - 1 || y == room.rect.yMax;

                if (isEdge)
                {
                    TileBase wallTile = isCorner ? (corners.GetRandomTile() ?? walls.GetRandomTile()) : walls.GetRandomTile();
                    midground.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
            }
        }
    }

    private void ConnectRooms(Room r1, Room r2)
    {
        Vector2Int start = r1.Center;
        Vector2Int end = r2.Center;

        // Horizontal
        int xDir = start.x < end.x ? 1 : -1;
        for (int x = start.x; x != end.x + xDir; x += xDir)
        {
            PaintCorridorTile(new Vector3Int(x, start.y, 0));
        }

        // Vertical
        int yDir = start.y < end.y ? 1 : -1;
        for (int y = start.y; y != end.y + yDir; y += yDir)
        {
            PaintCorridorTile(new Vector3Int(end.x, y, 0));
        }
    }

    private void PaintCorridorTile(Vector3Int pos)
    {
        background.SetTile(pos, floors.GetRandomTile());
        // Clear walls on midground for the corridor path
        midground.SetTile(pos, null);
        
        // Add walls adjacent to corridor if empty
        Vector3Int[] neighbors = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        foreach (var offset in neighbors)
        {
            Vector3Int nPos = pos + offset;
            if (background.GetTile(nPos) == null && midground.GetTile(nPos) == null)
            {
                midground.SetTile(nPos, walls.GetRandomTile());
            }
        }
    }

    private void PlaceTile(Vector2Int pos, TileBase tile, Tilemap map)
    {
        if (tile != null && map != null)
        {
            map.SetTile(new Vector3Int(pos.x, pos.y, 0), tile);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelBuilder))]
public class LevelBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LevelBuilder builder = (LevelBuilder)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Auto Load Tiles"))
        {
            Undo.RecordObject(builder, "Auto Load Tiles");
            builder.AutoLoadTiles();
        }

        if (GUILayout.Button("Generate Level"))
        {
            Undo.RegisterFullObjectHierarchyUndo(builder.background.gameObject.transform.parent.gameObject, "Generate Level");
            builder.GenerateLevel();
        }

        if (GUILayout.Button("Clear Level"))
        {
            Undo.RegisterFullObjectHierarchyUndo(builder.background.gameObject.transform.parent.gameObject, "Clear Level");
            builder.ClearLevel();
        }
    }
}
#endif
