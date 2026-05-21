using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SwordConsumable : MonoBehaviour
{
    [Header("Collection")]
    [SerializeField] private string collectionStateName = "Collect";
    [SerializeField, Min(0f)] private float destroyDelay = 0.15f;
    [SerializeField] private string unlockMessage = "Charge Attack Unlocked";

    private Animator animator;
    private Collider2D triggerCollider;
    private bool isCollected;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        triggerCollider = GetComponent<Collider2D>();

        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[SwordConsumable] {name} needs its Collider2D set to Is Trigger to be collected.");
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

        PlayerChargeAttack chargeAttack = ResolvePlayerChargeAttack(other);
        if (chargeAttack == null) return;

        chargeAttack.UnlockChargeAttack();
        Collect();
    }

    private PlayerChargeAttack ResolvePlayerChargeAttack(Collider2D other)
    {
        PlayerController playerController = other.GetComponentInParent<PlayerController>();
        if (playerController == null)
        {
            if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player"))
            {
                return null;
            }

            playerController = other.GetComponentInParent<PlayerController>();
        }

        if (playerController == null) return null;

        return playerController.GetComponent<PlayerChargeAttack>();
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

        AudioManager.Instance?.PlayInteract();
        HUDManager.Instance?.ShowUXMessage(unlockMessage);
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
