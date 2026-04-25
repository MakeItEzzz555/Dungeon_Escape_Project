using UnityEngine;

/// <summary>
/// A simple, student-friendly top-down 2D player controller.
/// Handles movement with Rigidbody2D and connects to the PlayerAnimator system.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float inputDeadzone = 0.1f;

    private Rigidbody2D rb;
    private PlayerAnimator playerAnimator;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<PlayerAnimator>();

        // Ensure the player doesn't fall due to gravity or spin from collisions
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // 1. Read Raw Input (WASD or Arrow Keys)
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        // 2. Lock to 4-direction movement (Horizontal priority)
        if (Mathf.Abs(inputX) > inputDeadzone)
        {
            moveInput = new Vector2(inputX, 0);
        }
        else if (Mathf.Abs(inputY) > inputDeadzone)
        {
            moveInput = new Vector2(0, inputY);
        }
        else
        {
            moveInput = Vector2.zero;
        }

        // 3. Update Animations (normalize to ensure consistent direction values)
        if (playerAnimator != null)
        {
            playerAnimator.UpdateAnimations(moveInput.normalized);
        }
    }

    private void FixedUpdate()
    {
        // 4. Apply Physics Movement
        // Note: Using velocity for compatibility. In Unity 6, rb.linearVelocity is preferred.
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}
