using UnityEngine;
using Scripts.Managers;

namespace Scripts.Interactables
{
/// <summary>
/// Handles level completion interaction and safe scene transition.
/// Fully locked to prevent physics/death systems interfering during transition.
/// </summary>
public class CheckpointDoor : MonoBehaviour
{
[Header("Scene Transition Settings")]
[SerializeField] private string targetScene;
[SerializeField] private Transform zoomTarget;

    private bool playerInRange;
    private bool isTransitioning;

    private void Update()
    {
        if (isTransitioning) return;

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryTransition();
        }
    }

    private void TryTransition()
    {
        if (GlobalQuestManager.Instance == null)
        {
            Debug.LogWarning("GlobalQuestManager missing.");
            return;
        }

        if (GlobalQuestManager.Instance.IsQuestComplete)
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
        if (isTransitioning) return;
        isTransitioning = true;

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("CheckpointDoor: Target scene not set!");
            isTransitioning = false;
            return;
        }

        Debug.Log($"Quest complete. Transitioning to {targetScene}...");

        // ----------------------------------------------------
        // HARD PLAYER LOCK (prevents fall/death triggers)
        // ----------------------------------------------------
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Disable movement
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
                controller.SetControlEnabled(false);

            // Stop physics immediately (VERY IMPORTANT)
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false;
            }

            // OPTIONAL: prevent trigger-based death systems
            Collider2D col = player.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }

        // ----------------------------------------------------
        // SAFETY CHECK: Transition Manager
        // ----------------------------------------------------
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("SceneTransitionManager is missing from scene!");
            return;
        }

        // ----------------------------------------------------
        // START TRANSITION
        // ----------------------------------------------------
        SceneTransitionManager.Instance.BeginTransition(
            targetScene,
            zoomTarget != null ? zoomTarget : transform
        );
    }

    private void DenyInteraction()
    {
        Debug.Log("Collect all keys first.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}


}
