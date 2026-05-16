using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Attempt to use Newtonsoft if installed, otherwise show a descriptive error
#if UNITY_NEWTONSOFT_JSON_INSTALLED
using Newtonsoft.Json;
#endif

namespace UnityArchitect.Editor.Tools
{
    /// <summary>
    /// Architect Kit: Scene Insight
    /// Generates a full scene hierarchy dump for AI analysis or technical debugging.
    /// </summary>
    public class ArchitectKitSceneInsight : EditorWindow
    {
        private const string MenuPath = "Architect Kit/Tools/Dump Scene Insight";

        [MenuItem(MenuPath)]
        public static void DumpScene()
        {
#if !UNITY_NEWTONSOFT_JSON_INSTALLED
            if (!CheckForNewtonsoft())
            {
                EditorUtility.DisplayDialog("Architect Kit Error", 
                    "This component requires 'Newtonsoft JSON'. \n\nPlease install it via the Package Manager: \n'com.unity.nuget.newtonsoft-json'", "Understood");
                return;
            }
#endif

            var sceneObjects = new List<GameObjectData>();
            var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            // Filter only root objects to start recursion
            foreach (var go in allObjects)
            {
                if (go.transform.parent == null)
                {
                    sceneObjects.Add(ProcessObject(go));
                }
            }

            string json = Serialize(sceneObjects);
            string fileName = $"SceneDump_{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
            string path = Path.Combine(Application.dataPath, "..", fileName);
            
            File.WriteAllText(path, json);
            
            Debug.Log($"<b><color=#4CAF50>[Architect Kit]</color></b> Scene exported successfully to: <color=white>{path}</color>");
            EditorUtility.RevealInFinder(path);
        }

        private static GameObjectData ProcessObject(GameObject go)
        {
            var data = new GameObjectData
            {
                Name = go.name,
                InstanceID = go.GetInstanceID(),
                Tag = go.tag,
                Layer = LayerMask.LayerToName(go.layer),
                IsActive = go.activeInHierarchy,
                Position = go.transform.position,
                Rotation = go.transform.eulerAngles,
                Components = go.GetComponents<Component>()
                    .Where(c => c != null)
                    .Select(c => c.GetType().Name)
                    .ToList()
            };

            if (go.transform.childCount > 0)
            {
                data.Children = new List<GameObjectData>();
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    data.Children.Add(ProcessObject(go.transform.GetChild(i).gameObject));
                }
            }

            return data;
        }

        private static string Serialize(object obj)
        {
#if UNITY_NEWTONSOFT_JSON_INSTALLED
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
#else
            // Basic fallback if Newtonsoft is missing (user should ideally install it)
            return JsonUtility.ToJson(obj, true); 
#endif
        }

        private static bool CheckForNewtonsoft()
        {
            // Simple check if the namespace exists (placeholder, 
            // actual compilation logic depends on .asmdef or compiler symbols)
            return false; 
        }

        [System.Serializable]
        public class GameObjectData
        {
            public string Name;
            public int InstanceID;
            public string Tag;
            public string Layer;
            public bool IsActive;
            public Vector3 Position;
            public Vector3 Rotation;
            public List<string> Components;
            public List<GameObjectData> Children;
        }
    }
}
