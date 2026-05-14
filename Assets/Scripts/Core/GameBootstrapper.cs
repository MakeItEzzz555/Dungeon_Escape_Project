using UnityEngine;

namespace Scripts.Core
{
    /// <summary>
    /// Ensures that the PersistentSystems prefab is instantiated automatically at runtime.
    /// This allows starting the game from any scene without needing a bootstrap scene.
    /// </summary>
    public static class GameBootstrapper
    {
        private const string PREFAB_PATH = "PersistentSystems";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Bootstrap()
        {
            Debug.Log("[DEBUG_LOG] GameBootstrapper: Initializing...");

            // Check if PersistentSystems already exists in the scene
            GameObject existingSystems = GameObject.Find("PersistentSystems");
            if (existingSystems != null)
            {
                Debug.Log("[DEBUG_LOG] GameBootstrapper: PersistentSystems already exists.");
                return;
            }

            // Load from Resources or assume it's in the scene if we can't find it
            // For now, we will try to load it from Resources/PersistentSystems
            // If the user hasn't put it in Resources, we'll have to create it or rely on it being in the scene.
            // BUT the requirement says: "Instantiate PersistentSystems prefab OR create it programmatically"
            
            GameObject prefab = Resources.Load<GameObject>(PREFAB_PATH);
            if (prefab != null)
            {
                Debug.Log("[DEBUG_LOG] GameBootstrapper: Instantiating PersistentSystems from Resources.");
                GameObject instance = Object.Instantiate(prefab);
                instance.name = "PersistentSystems";
                Object.DontDestroyOnLoad(instance);
            }
            else
            {
                Debug.LogWarning($"[DEBUG_LOG] GameBootstrapper: Prefab '{PREFAB_PATH}' not found in Resources. Searching in scene...");
                // If not in Resources, maybe it's already there but named differently? 
                // We already checked for "PersistentSystems" by name.
            }
            
            // Safety check: Ensure a Camera exists
            EnsureMainCameraExists();
        }

        public static void EnsureMainCameraExists()
        {
            if (Camera.main == null)
            {
                Debug.LogWarning("[DEBUG_LOG] GameBootstrapper: No MainCamera found! Recreating safety camera.");
                GameObject camObj = new GameObject("SafetyCamera");
                Camera cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                // Add essential components if missing, though they should be in PersistentSystems
                Object.DontDestroyOnLoad(camObj);
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void PostBootstrap()
        {
            // Safety check: Ensure a Camera exists after scene load if it was somehow missing or destroyed
            EnsureMainCameraExists();
        }
    }
}
