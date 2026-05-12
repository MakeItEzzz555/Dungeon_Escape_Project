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
    public static readonly string IsMoving = "IsMoving";
    public static readonly string DieTrigger = "Die";
    private static readonly string LastTimeMove = "LastTimeMove";
    private static readonly string IsInStandBy = "IsInStandBy";
    private static readonly string IsFalling = "IsFalling";

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
            animator.SetInteger(LastTimeMove, lastTimeMove);
            return;
        }

        bool moving = moveInput.sqrMagnitude > 0.01f;

        animator.SetBool(IsMoving, moving);
        animator.SetInteger(LastTimeMove, lastTimeMove);

        if (moving)
        {
            Vector2 dir = moveInput.normalized;
            animator.SetFloat(MoveX, dir.x);
            animator.SetFloat(MoveY, dir.y);
        }
    }

    public void SetStandByStatus(bool value)
    {
        animator?.SetBool(IsInStandBy, value);
    }

    public void SetFallingStatus(bool value)
    {
        animator?.SetBool(IsFalling, value);
    }

    public void TriggerDie()
    {
        animator?.SetTrigger(DieTrigger);
    }
}