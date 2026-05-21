using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private Animator animator;
    [SerializeField] private Hitbox swordHitbox;
    [SerializeField] private Health health;

    [Header("Settings")]
    [SerializeField] private string attackTrigger = "Attack";

    private void Awake()
    {
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (health == null) health = GetComponent<Health>();
        
        if (swordHitbox == null) swordHitbox = GetComponentInChildren<Hitbox>(true);

        if (playerAnimator == null) Debug.LogWarning("[DEBUG_LOG] PlayerCombat: PlayerAnimator reference missing!");
        if (animator == null) Debug.LogWarning("[DEBUG_LOG] PlayerCombat: Animator reference missing!");
        if (swordHitbox == null) Debug.LogWarning("[DEBUG_LOG] PlayerCombat: SwordHitbox reference missing!");
    }

    private void Update()
    {
        if (health != null && health.IsDead) return;
        if (playerController != null && playerController.IsControlLocked) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            HandleAttackInput();
        }
    }

    private void HandleAttackInput()
    {
        if (playerAnimator == null) return;

        playerAnimator.PlayAttack();

        // Notify controller to reset idle timer
        playerController?.ResetIdleTimer();
    }

    // Animation Events
    public void EnableHitbox()
    {
        if (swordHitbox != null)
        {
            swordHitbox.EnableHitbox();
        }
        else
        {
            Debug.LogWarning("[DEBUG_LOG] PlayerCombat: EnableHitbox called but swordHitbox is null!");
        }
    }

    public void DisableHitbox()
    {
        if (swordHitbox != null)
        {
            swordHitbox.DisableHitbox();
        }
    }

    public void CancelAttack()
    {
        DisableHitbox();
        ResetAttackTriggerIfExists();
        playerAnimator?.ResetActionTriggers();
    }

    public void PlayAttack1SFX()
    {
        AudioManager.Instance?.PlayPlayerAttack1();
    }

    public void PlayAttack2SFX()
    {
        AudioManager.Instance?.PlayPlayerAttack2();
    }

    public void PlayAttack3SFX()
    {
        AudioManager.Instance?.PlayPlayerAttack3();
    }

    private void ResetAttackTriggerIfExists()
    {
        if (animator == null || string.IsNullOrEmpty(attackTrigger)) return;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == attackTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(attackTrigger);
                return;
            }
        }
    }
}
