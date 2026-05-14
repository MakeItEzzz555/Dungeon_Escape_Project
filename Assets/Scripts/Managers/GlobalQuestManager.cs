using UnityEngine;

namespace Scripts.Managers
{
    /// <summary>
    /// Manages global gameplay progression, tracking keys and level completion.
    /// Persistent across scenes.
    /// </summary>
    public class GlobalQuestManager : MonoBehaviour
    {
        public static GlobalQuestManager Instance { get; private set; }

        [Header("Progression State")]
        [SerializeField] private int requiredKeys;
        private int collectedKeys;

        public bool IsQuestComplete => collectedKeys >= requiredKeys && requiredKeys > 0;

        private void Awake()
        {
            // Singleton pattern with duplicate prevention
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Increments collected keys and updates the HUD.
        /// </summary>
        public void AddKey()
        {
            collectedKeys++;
            UpdateHUD();
            
            if (IsQuestComplete)
            {
                Debug.Log("Quest Complete! All keys collected.");
            }
        }

        /// <summary>
        /// Resets progress for a new level with a specific number of required keys.
        /// </summary>
        /// <param name="newRequiredKeys">Number of keys needed for the level.</param>
        public void ResetKeys(int newRequiredKeys)
        {
            collectedKeys = 0;
            requiredKeys = newRequiredKeys;
            UpdateHUD();
            Debug.Log($"Keys reset. Required keys: {requiredKeys}");
        }

        public int GetCollectedKeys() => collectedKeys;
        public int GetRequiredKeys() => requiredKeys;

        private void UpdateHUD()
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.UpdateKeys(collectedKeys, requiredKeys);
            }
        }
    }
}
