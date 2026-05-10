using UnityEngine;
using System.Collections;

/// <summary>
/// A key collection script that plays a collection animation/frame before disappearing.
/// </summary>
public class Collect_keys : MonoBehaviour
{
    [Header("Collection Settings")]
    [Tooltip("The name of the animation state or trigger for the collection frame.")]
    public string collectionStateName = "Collect";

    [Tooltip("How long to wait (in seconds) after collection before destroying the key.")]
    public float destroyDelay = 1.0f;

    private bool isCollected = false;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"Collect_keys on {gameObject.name} has no Animator. It will disappear immediately.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only collect if it's the player and we haven't collected it yet
        if (other.CompareTag("Player") && !isCollected)
        {
            Collect();
        }
    }

    private void Collect()
    {
        isCollected = true;

        // Play collection animation or switch frame if Animator exists
        if (animator != null)
        {
            // We play the collection state (e.g., a frame where it shines or vanishes)
            animator.Play(collectionStateName);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayKey();
        }

        Debug.Log("Key collected!");

        // Update the HUD
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.RegisterKeyCollection();
        }

        // Start the countdown to disappear
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
