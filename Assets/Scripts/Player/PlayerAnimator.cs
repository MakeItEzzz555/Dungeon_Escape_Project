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
    private static readonly string IsMoving = "IsMoving";

    private void Start()
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
    public void UpdateAnimations(Vector2 moveInput)
    {
        if (animator == null) return;

        // Check if the player is currently moving
        bool moving = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool(IsMoving, moving);

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
}
