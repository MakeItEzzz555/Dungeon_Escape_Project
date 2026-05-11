using UnityEngine;
using Scripts.Managers;

namespace Scripts.Managers
{
    /// <summary>
    /// Level-specific initializer that defines the requirements for the current scene.
    /// Place this in each level scene.
    /// </summary>
    public class LevelInitializer : MonoBehaviour
    {
        [Header("Level Requirements")]
        [SerializeField] private int requiredKeys;

        private void Start()
        {
            InitializeLevel();
        }

        private void InitializeLevel()
        {
            if (GlobalQuestManager.Instance != null)
            {
                GlobalQuestManager.Instance.ResetLevelProgress(requiredKeys);
            }
            else
            {
                Debug.LogWarning("GlobalQuestManager Instance not found during level initialization!");
            }
        }
    }
}
