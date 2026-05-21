using UnityEngine;

/// <summary>
/// Animation-event bridge for FinalBoss visuals.
/// </summary>
public class FinalBossCombatBridge : MonoBehaviour
{
    [Header("Hitboxes")]
    [SerializeField] private Hitbox attackHitbox;
    [SerializeField] private Hitbox chargeHitbox;

    [Header("Behavior")]
    [SerializeField] private FinalBossBehavior finalBossBehavior;

    private void Awake()
    {
        if (finalBossBehavior == null)
        {
            finalBossBehavior = GetComponentInParent<FinalBossBehavior>();
        }

        ResolveMissingHitboxes();
        WarnForMissingHitboxes();
    }

    public void EnableHitbox()
    {
        if (attackHitbox == null)
        {
            Debug.LogWarning($"[FinalBossCombatBridge] EnableHitbox called on {gameObject.name} but attackHitbox is null.");
            return;
        }

        attackHitbox.EnableHitbox();
    }

    public void DisableHitbox()
    {
        attackHitbox?.DisableHitbox();
    }

    public void EnableChargeHitbox()
    {
        if (chargeHitbox == null)
        {
            Debug.LogWarning($"[FinalBossCombatBridge] EnableChargeHitbox called on {gameObject.name} but chargeHitbox is null.");
            return;
        }

        chargeHitbox.EnableHitbox();
    }

    public void DisableChargeHitbox()
    {
        chargeHitbox?.DisableHitbox();
    }

    public void StartChargeDash()
    {
        if (finalBossBehavior == null)
        {
            Debug.LogWarning($"[FinalBossCombatBridge] StartChargeDash called on {gameObject.name} but finalBossBehavior is null.");
            return;
        }

        finalBossBehavior.StartChargeDash();
    }

    public void BeginChargeDash()
    {
        StartChargeDash();
    }

    public void InvokeChargeDash()
    {
        StartChargeDash();
    }

    public void DisableAllHitboxes()
    {
        attackHitbox?.DisableHitbox();
        chargeHitbox?.DisableHitbox();
    }

    public void PlayAttack1SFX()
    {
        AudioManager.Instance?.PlayEnemySwordSwing1();
    }

    public void PlayAttack2SFX()
    {
        AudioManager.Instance?.PlayEnemySwordSwing2();
    }

    public void PlayAttack3SFX()
    {
        AudioManager.Instance?.PlayEnemySwordSwing2();
    }

    private void ResolveMissingHitboxes()
    {
        if (attackHitbox != null && chargeHitbox != null) return;

        Transform searchRoot = transform.parent != null ? transform.parent : transform;
        Hitbox[] hitboxes = searchRoot.GetComponentsInChildren<Hitbox>(true);

        for (int i = 0; i < hitboxes.Length; i++)
        {
            Hitbox hitbox = hitboxes[i];
            if (hitbox == null) continue;

            string hitboxName = hitbox.gameObject.name;
            if (attackHitbox == null && hitboxName.IndexOf("Attack", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                attackHitbox = hitbox;
                continue;
            }

            if (chargeHitbox == null && hitboxName.IndexOf("Charge", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                chargeHitbox = hitbox;
                continue;
            }
        }

        for (int i = 0; i < hitboxes.Length; i++)
        {
            Hitbox hitbox = hitboxes[i];
            if (hitbox == null) continue;

            if (attackHitbox == null)
            {
                attackHitbox = hitbox;
                continue;
            }

            if (chargeHitbox == null && hitbox != attackHitbox)
            {
                chargeHitbox = hitbox;
                return;
            }
        }
    }

    private void WarnForMissingHitboxes()
    {
        if (attackHitbox == null)
        {
            Debug.LogWarning($"[FinalBossCombatBridge] {gameObject.name} could not find an attack Hitbox in parent children.");
        }

        if (chargeHitbox == null)
        {
            Debug.LogWarning($"[FinalBossCombatBridge] {gameObject.name} could not find a charge Hitbox in parent children.");
        }
    }
}
