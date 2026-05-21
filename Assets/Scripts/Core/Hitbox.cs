using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    public enum TargetMode
    {
        HurtBoxOnly,
        AnyHealthInHierarchy
    }

    [Header("Settings")]
    public int damageAmount = 1;
    public Transform owner;

    [Header("Targeting")]
    [SerializeField] private TargetMode targetMode = TargetMode.HurtBoxOnly;
    [SerializeField] private string legacyHurtBoxName = "HurtBox";

    private Collider2D hitboxCollider;
    private List<Health> hitTargets = new List<Health>();

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false;
        }
    }

    public void EnableHitbox()
    {
        hitTargets.Clear();
        if (hitboxCollider != null) hitboxCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        if (hitboxCollider != null) hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        Health health = ResolveTargetHealth(other);

        if (health != null && !hitTargets.Contains(health))
        {
            // Prevent self-damage and damage to objects in the same hierarchy
            if (owner != null && (health.transform == owner || health.transform.root == owner.root))
            {
                return;
            }

            hitTargets.Add(health);
            health.TakeDamage(damageAmount, owner);
        }
    }

    private Health ResolveTargetHealth(Collider2D other)
    {
        if (other == null) return null;

        if (targetMode == TargetMode.HurtBoxOnly)
        {
            HurtBox hurtBox = other.GetComponent<HurtBox>();
            if (hurtBox != null && hurtBox.TryGetHealth(out Health hurtBoxHealth))
            {
                return hurtBoxHealth;
            }

            if (IsLegacyHurtBox(other))
            {
                return other.GetComponentInParent<Health>();
            }

            return null;
        }

        Health health = other.GetComponent<Health>();
        if (health == null) health = other.GetComponentInParent<Health>();
        return health;
    }

    private bool IsLegacyHurtBox(Collider2D other)
    {
        return !string.IsNullOrWhiteSpace(legacyHurtBoxName)
            && string.Equals(other.gameObject.name, legacyHurtBoxName, System.StringComparison.OrdinalIgnoreCase);
    }
}
