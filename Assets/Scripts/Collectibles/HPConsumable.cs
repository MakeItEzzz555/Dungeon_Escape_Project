using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HPConsumable : MonoBehaviour
{
    [Header("Healing")]
    [SerializeField, Min(1)] private int healAmount = 2;
    [SerializeField] private bool consumeWhenAtFullHealth;

    [Header("Collection")]
    [SerializeField] private string collectionStateName = "Collect";
    [SerializeField, Min(0f)] private float destroyDelay = 0.15f;

    private Animator animator;
    private Collider2D triggerCollider;
    private bool isCollected;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        triggerCollider = GetComponent<Collider2D>();

        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[HPConsumable] {name} needs its Collider2D set to Is Trigger to be collected.");
        }
    }

    private void Reset()
    {
        Collider2D collider2d = GetComponent<Collider2D>();
        if (collider2d != null)
        {
            collider2d.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        Health playerHealth = ResolvePlayerHealth(other);
        if (playerHealth == null || playerHealth.IsDead) return;

        bool healed = playerHealth.Heal(healAmount);
        if (!healed && !consumeWhenAtFullHealth) return;

        Collect();
    }

    private Health ResolvePlayerHealth(Collider2D other)
    {
        PlayerController playerController = other.GetComponentInParent<PlayerController>();
        if (playerController != null)
        {
            return playerController.GetComponent<Health>();
        }

        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player"))
        {
            return null;
        }

        return other.GetComponentInParent<Health>();
    }

    private void Collect()
    {
        isCollected = true;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        if (animator != null && !string.IsNullOrWhiteSpace(collectionStateName))
        {
            animator.Play(collectionStateName);
        }

        HUDManager.Instance?.ShowUXMessage($"+{healAmount} HP");
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        if (destroyDelay > 0f)
        {
            yield return new WaitForSeconds(destroyDelay);
        }

        Destroy(gameObject);
    }
}
