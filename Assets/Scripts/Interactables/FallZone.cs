using UnityEngine;

namespace Scripts.Interactables
{
    /// <summary>
    /// A zone that triggers a fall sequence when the player enters it.
    /// Used for water, pits, and void areas.
    /// </summary>
    public class FallZone : MonoBehaviour
    {
        private bool hasTriggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Only trigger if it's the player and we haven't triggered yet
            if (!hasTriggered && other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    hasTriggered = true;
                    Debug.Log($"[DEBUG_LOG] FallZone: Player entered {gameObject.name}. Starting fall sequence.");
                    player.StartFallSequence();
                }
            }
        }
    }
}
