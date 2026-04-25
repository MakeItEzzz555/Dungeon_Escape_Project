using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A simple Unity C# editor tool to paint a room on Tilemaps.
/// This script can be attached to any GameObject in the scene (ideally the Grid).
/// </summary>
public class RoomGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap background;
    public Tilemap midground;
    public Tilemap foreground;
    public Tilemap fences;

    [Header("Tiles")]
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase cornerTile;

    [Header("Room Settings")]
    public Vector2Int origin = Vector2Int.zero;
    public int width = 10;
    public int height = 8;

    /// <summary>
    /// Generates a rectangular room with a doorway gap.
    /// Can be triggered from the Inspector context menu.
    /// </summary>
    [ContextMenu("Generate Room")]
    public void GenerateRoom()
    {
        if (background == null || midground == null)
        {
            Debug.LogError("Please assign Background and Midground Tilemaps!");
            return;
        }

        if (floorTile == null || wallTile == null || cornerTile == null)
        {
            Debug.LogError("Please assign Floor, Wall, and Corner Tiles!");
            return;
        }

        // 1. Fill the interior on Background (Floor)
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                background.SetTile(new Vector3Int(origin.x + x, origin.y + y, 0), floorTile);
            }
        }

        // 2. Build the perimeter on Midground (Walls and Corners)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isCorner = (x == 0 && y == 0) || (x == 0 && y == height - 1) || 
                                (x == width - 1 && y == 0) || (x == width - 1 && y == height - 1);
                bool isEdge = (x == 0 || x == width - 1 || y == 0 || y == height - 1);

                if (isCorner)
                {
                    midground.SetTile(new Vector3Int(origin.x + x, origin.y + y, 0), cornerTile);
                }
                else if (isEdge)
                {
                    // 3. Leave one 2-tile doorway gap at the bottom center
                    // We only want a gap at y=0 (bottom edge)
                    if (y == 0)
                    {
                        int midX = width / 2;
                        if (x == midX || x == midX - 1)
                        {
                            // It's the doorway - leave it empty
                            midground.SetTile(new Vector3Int(origin.x + x, origin.y + y, 0), null);
                            continue;
                        }
                    }

                    midground.SetTile(new Vector3Int(origin.x + x, origin.y + y, 0), wallTile);
                }
            }
        }

        Debug.Log($"Room of {width}x{height} generated successfully!");
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom Inspector button to make it more user-friendly.
    /// </summary>
    [CustomEditor(typeof(RoomGenerator))]
    public class RoomGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            RoomGenerator generator = (RoomGenerator)target;
            if (GUILayout.Button("Generate Room"))
            {
                // Register undo for the tile changes
                Undo.RegisterFullObjectHierarchyUndo(generator.background.gameObject, "Generate Room");
                Undo.RegisterFullObjectHierarchyUndo(generator.midground.gameObject, "Generate Room");
                
                generator.GenerateRoom();
            }
        }
    }
#endif
}
