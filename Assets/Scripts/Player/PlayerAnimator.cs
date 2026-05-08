using UnityEngine;

/// <summary>
/// A simple, student-friendly 2D top-down player animator script.
/// Bridges movement input with Unity Animator parameters.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("Animator Setup")]
    public Animator animator;

    // These parameters must match the names in your Animator Controller exactly
    private static readonly string MoveX = "MoveX";
    private static readonly string MoveY = "MoveY";
    private static readonly string LastMoveX = "LastMoveX";
    private static readonly string LastMoveY = "LastMoveY";
    public static string IsMoving = "IsMoving";
    public static readonly string DieTrigger = "Die";
    private static readonly string LastTimeMove = "LastTimeMove";
    private static readonly string StandByIdleDown = "StandBy_idle_down";
    private static readonly string IsInStandBy = "IsInStandBy";

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError($"PlayerAnimator on {gameObject.name} is missing an Animator component!");
        }
    }

    /// <summary>
    /// Call this method from your movement script each frame.
    /// </summary>
    /// <param name="moveInput">The movement vector (e.g., from Horizontal and Vertical axes).</param>
    /// <param name="lastTimeMove">The current idle time count.</param>
    public void UpdateAnimations(Vector2 moveInput, int lastTimeMove)
    {
        if (animator == null) return;

        // Check if the player is currently moving
        bool moving = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool(IsMoving, moving);
        animator.SetInteger(LastTimeMove, lastTimeMove);

        if (moving)
        {
            // Normalize input so speed doesn't affect which animation frame shows (unless you want that)
            Vector2 normalizedInput = moveInput.normalized;

            // Update movement parameters for the "Run" Blend Tree
            animator.SetFloat(MoveX, normalizedInput.x);
            animator.SetFloat(MoveY, normalizedInput.y);

            // Update "Last" parameters to keep facing direction when idle
            animator.SetFloat(LastMoveX, normalizedInput.x);
            animator.SetFloat(LastMoveY, normalizedInput.y);
        }
    }

    /// <summary>
    /// Updates the standby status in the animator.
    /// </summary>
    /// <param name="inStandBy">True if the player should be in standby.</param>
    public void SetStandByStatus(bool inStandBy)
    {
        if (animator != null)
        {
            animator.SetBool(IsInStandBy, inStandBy);
        }
    }

    /// <summary>
    /// Can be called via an Animation Event at the end of the StandBy animation
    /// to signal that the state should transition back to normal idle.
    /// </summary>
    public void OnStandByAnimationEnd()
    {
        if (animator != null)
        {
            animator.SetBool(IsInStandBy, false);
            Debug.Log("[DEBUG_LOG] PlayerAnimator: StandBy Animation Event triggered. Setting IsInStandBy to false.");
            
            // Notify the PlayerController to reset its timers
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ResetIdleTimer();
            }
        }
    }

    /// <summary>
    /// Checks if the animator is currently in the StandBy_idle_down state.
    /// </summary>
    /// <returns>True if in StandBy_idle_down state, false otherwise.</returns>
    public bool IsInStandByState()
    {
        if (animator == null) return false;
        
        // Get information about the current state on the base layer (layer 0)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isInState = stateInfo.IsName(StandByIdleDown);
        
        // Detailed logging to help debug transitions
        if (isInState)
        {
            Debug.Log("[DEBUG_LOG] PlayerAnimator: Currently in StandBy_idle_down state.");
        }
        
        return isInState;
    }

    public void TriggerDie()
    {
        if (animator != null)
        {
            animator.SetTrigger(DieTrigger);
        }
    }
}
