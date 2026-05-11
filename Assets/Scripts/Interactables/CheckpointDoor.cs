using UnityEngine;
using UnityEngine.SceneManagement;
using Scripts.Managers;

namespace Scripts.Interactables
{
    /// <summary>
    /// A door that checks for level completion and transitions to the next scene.
    /// </summary>
    public class CheckpointDoor : MonoBehaviour
    {
        [Header("Scene Transition Settings")]
        [SerializeField] private string nextSceneName;

        private bool playerInRange;
        private bool isTransitioning;

        private void Update()
        {
            // Detect player interaction
            if (playerInRange && !isTransitioning && Input.GetKeyDown(KeyCode.E))
            {
                TryTransition();
            }
        }

        private void TryTransition()
        {
            // Verify quest completion via GlobalQuestManager
            if (GlobalQuestManager.Instance != null && GlobalQuestManager.Instance.IsQuestComplete)
            {
                TransitionToNextLevel();
            }
            else
            {
                DenyInteraction();
            }
        }

        private void TransitionToNextLevel()
        {
            isTransitioning = true;
            Debug.Log($"Quest complete. Transitioning to {nextSceneName}...");
            
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError("Next Scene Name is not set on CheckpointDoor!");
                isTransitioning = false;
            }
        }

        private void DenyInteraction()
        {
            Debug.Log("Collect all keys first.");
            // You could also trigger a sound or UI feedback here
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }
    }
}
