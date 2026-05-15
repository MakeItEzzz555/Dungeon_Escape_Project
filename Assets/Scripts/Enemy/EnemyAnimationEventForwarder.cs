using UnityEngine;

/// <summary>
/// Place this on the Visuals GameObject (same object that has the Animator).
/// Forwards animation events to EnemyCombat on the parent.
/// </summary>
public class EnemyAnimationEventForwarder : MonoBehaviour
{
    [SerializeField] private EnemyCombat enemyCombat;

    private void Awake()
    {
        if (enemyCombat == null)
        {
            enemyCombat = GetComponentInParent<EnemyCombat>();
        }

        if (enemyCombat == null)
        {
            Debug.LogWarning($"[EnemyAnimationEventForwarder] {gameObject.name} could not find EnemyCombat in parent!");
        }
    }

    // Called by animation events
    public void EnableHitbox()
    {
        Debug.Log($"[EnemyAnimationEventForwarder] EnableHitbox called on {gameObject.name}");
        if (enemyCombat != null) enemyCombat.EnableHitbox();
    }

    public void DisableHitbox()
    {
        Debug.Log($"[EnemyAnimationEventForwarder] DisableHitbox called on {gameObject.name}");
        if (enemyCombat != null) enemyCombat.DisableHitbox();
    }

    public void PlayEnemySwing1SFX()
    {
        AudioManager.Instance?.PlayEnemySwordSwing1();
    }

    public void PlayEnemySwing2SFX()
    {
        AudioManager.Instance?.PlayEnemySwordSwing2();
    }
}
