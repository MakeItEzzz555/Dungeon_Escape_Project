using Scripts.Managers;
using UnityEngine;

namespace Scripts.Interactables
{
    public class FallZone : MonoBehaviour
    {
        private bool hasTriggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            // 🔥 BLOCK DURING TRANSITIONS
            if (SceneTransitionManager.Instance != null &&
                SceneTransitionManager.Instance.IsTransitioning)
            {
                return;
            }

            // 🔥 BLOCK SPAWN FRAME BUG
            if (player.IsSpawnProtected())
            {
                return;
            }

            if (hasTriggered) return;

            hasTriggered = true;

            Debug.Log($"FallZone triggered: {gameObject.name}");

            player.StartFallSequence();
        }

        private void OnEnable()
        {
            hasTriggered = false;
        }
    }
}