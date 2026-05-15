using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    [Header("Settings")]
    public int damageAmount = 1;
    public Transform owner;

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
        Health health = other.GetComponent<Health>();
        if (health == null) health = other.GetComponentInParent<Health>();

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
}
