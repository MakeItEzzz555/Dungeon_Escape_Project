using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Hitbox attackHitbox;

    public bool HasHitboxAssigned => attackHitbox != null;

    private void Awake()
    {
        if (attackHitbox == null)
        {
            attackHitbox = GetComponentInChildren<Hitbox>(true);
            
            if (attackHitbox == null)
            {
                Debug.LogWarning($"[EnemyCombat] No Hitbox component assigned or found in children of {gameObject.name}. Enemy attacks will not deal damage.");
            }
        }
    }

    // Animation Events
    public void EnableHitbox()
    {
        if (attackHitbox == null)
        {
            Debug.LogWarning($"[EnemyCombat] EnableHitbox called on {gameObject.name} but attackHitbox is null!");
            return;
        }
        
        attackHitbox.EnableHitbox();
    }

    public void DisableHitbox()
    {
        if (attackHitbox == null) return;
        
        attackHitbox.DisableHitbox();
    }
}
