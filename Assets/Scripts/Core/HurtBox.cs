using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class HurtBox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;

    public Health Health
    {
        get
        {
            if (health == null)
            {
                health = GetComponentInParent<Health>();
            }

            return health;
        }
    }

    private void Awake()
    {
        Collider2D hurtCollider = GetComponent<Collider2D>();
        hurtCollider.isTrigger = true;

        if (health == null)
        {
            health = GetComponentInParent<Health>();
        }

        if (health == null)
        {
            Debug.LogWarning($"[HurtBox] {name} could not find Health in parent hierarchy.");
        }
    }

    public bool TryGetHealth(out Health targetHealth)
    {
        targetHealth = Health;
        return targetHealth != null;
    }
}
