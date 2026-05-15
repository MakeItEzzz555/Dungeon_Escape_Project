using UnityEngine;

/// <summary>
/// Pure animation bridge layer.
/// No gameplay logic. No state inference.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("Animator Setup")]
    public Animator animator;

    private static readonly string MoveX = "MoveX";
    private static readonly string MoveY = "MoveY";
    private static readonly string LastMoveX = "LastMoveX";
    private static readonly string LastMoveY = "LastMoveY";
    public static readonly string IsMoving = "IsMoving";
    public static readonly string DieTrigger = "Die";
    private static readonly string LastTimeMove = "LastTimeMove";
    private static readonly string IsInStandBy = "IsInStandBy";
    private static readonly string IsFalling = "IsFalling";
    private static readonly string AttackTrigger = "Attack";
    private static readonly string IsDead = "IsDead";

    public bool IsInStandByActive => animator != null && animator.GetBool(IsInStandBy);

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void UpdateAnimations(Vector2 moveInput, int lastTimeMove, bool isLocked)
    {
        if (animator == null) return;

        // HARD LOCK: absolute override state
        if (isLocked)
        {
            animator.SetBool(IsMoving, false);
            animator.SetFloat(MoveX, 0);
            animator.SetFloat(MoveY, 0);
            animator.SetInteger(LastTimeMove, 0);
            
            // Critical fix: force IsInStandBy to false while locked
            // This prevents the animator from getting stuck in the StandBy clip
            // if a transition starts exactly when it was playing.
            if (animator.GetBool(IsInStandBy))
            {
                Debug.Log("[DEBUG_LOG] PlayerAnimator: Forcing StandBy FALSE because of isLocked.");
                animator.SetBool(IsInStandBy, false);
            }
            return;
        }

        bool moving = moveInput.sqrMagnitude > 0.01f;

        animator.SetBool(IsMoving, moving);
        
        // Always pass LastTimeMove to ensure the animator reflects the current timer
        animator.SetInteger(LastTimeMove, lastTimeMove);

        if (moving)
        {
            Vector2 dir = moveInput.normalized;
            animator.SetFloat(MoveX, dir.x);
            animator.SetFloat(MoveY, dir.y);

            animator.SetFloat(LastMoveX, dir.x);
            animator.SetFloat(LastMoveY, dir.y);
        }
    }

    public void SetStandByStatus(bool value)
    {
        if (animator != null)
        {
            if (animator.GetBool(IsInStandBy) != value)
            {
                Debug.Log($"[DEBUG_LOG] PlayerAnimator: SetStandByStatus CHANGED to {value}.");
                animator.SetBool(IsInStandBy, value);
            }
        }
    }

    public void SetFallingStatus(bool value)
    {
        animator?.SetBool(IsFalling, value);
    }

    public void TriggerDie()
    {
        animator?.SetTrigger(DieTrigger);
    }
    public void PlayAttack()
    {
        if (animator == null) return;

        float lx = animator.GetFloat(LastMoveX);
        float ly = animator.GetFloat(LastMoveY);

        if (Mathf.Approximately(lx, 0f) && Mathf.Approximately(ly, 0f))
        {
            animator.SetFloat(LastMoveX, 0f);
            animator.SetFloat(LastMoveY, -1f);
        }

        animator.SetTrigger(AttackTrigger);
    }

    public void SetDeadStatus(bool value)
    {
        if (animator != null)
            animator.SetBool(IsDead, value);
    }
    public void OnStandByAnimationEnd()
    {
        Debug.Log("[DEBUG_LOG] PlayerAnimator: OnStandByAnimationEnd called (Redirecting to PlayerController).");
        
        PlayerController pc = GetComponent<PlayerController>();
        if (pc == null) pc = GetComponentInParent<PlayerController>();
        
        if (pc != null)
        {
            pc.OnStandByAnimationEnd();
        }
        else
        {
            // Fallback if PC not found
            animator?.SetBool(IsInStandBy, false);
        }
    }
}