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
    [SerializeField] private int idleThreshold = 5; // Time in seconds before standby triggers
    [Header("Footsteps")]
    [SerializeField] private float footstepInterval = 0.4f;

    private float footstepTimer;
    private bool isMoving;
    
    private bool isDead = false;
    private Rigidbody2D rb;
    public PlayerAnimator playerAnimator;
    private Vector2 moveInput;
    private int lastTimeMove = 0;
    private float idleTimer = 0f;
    private bool hasTriggeredStandBy = false;
    private bool wasMovingLastFrame = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Ensure the player doesn't fall due to gravity or spin from collisions
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (isDead) return;
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

        // 3. Update Idle Timer and Trigger Standby
        if (moveInput.sqrMagnitude < inputDeadzone * inputDeadzone)
        {
            idleTimer += Time.deltaTime;
            lastTimeMove = Mathf.FloorToInt(idleTimer);

            if (lastTimeMove >= idleThreshold)
            {
                if (!hasTriggeredStandBy)
                {
                    Debug.Log($"[DEBUG_LOG] PlayerController: Idle threshold {idleThreshold} reached. Enabling StandBy bool.");
                    hasTriggeredStandBy = true;
                    
                    if (playerAnimator != null)
                    {
                        playerAnimator.SetStandByStatus(true);
                    }
                }
            }
        }
        else
        {
            if (hasTriggeredStandBy)
            {
                Debug.Log("[DEBUG_LOG] PlayerController: Movement detected. Resetting idle status.");
                if (playerAnimator != null)
                {
                    playerAnimator.SetStandByStatus(false);
                }
            }
            idleTimer = 0f;
            lastTimeMove = 0;
            hasTriggeredStandBy = false;
        }

        // 4. Update Animations
        if (playerAnimator != null)
        {
            playerAnimator.UpdateAnimations(moveInput.normalized, lastTimeMove);
        }

        // 5. Trigger Walk Sound
        HandleFootsteps();
    }

    public void ResetIdleTimer()
    {
        Debug.Log("[DEBUG_LOG] PlayerController: Resetting idle timers via ResetIdleTimer().");
        idleTimer = 0f;
        lastTimeMove = 0;
        hasTriggeredStandBy = false;
    }

    public bool IsDead => isDead;

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDeath();
        }

        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // Stop all physics movement immediately

        // Ensure IsMoving is set to false in the animator to prevent animation direction changes
        if (playerAnimator != null && playerAnimator.animator != null)
        {
            playerAnimator.animator.SetBool(PlayerAnimator.IsMoving, false);
            playerAnimator.animator.applyRootMotion = false; // Prevent animation from moving the player
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        // 4. Apply Physics Movement
        // Note: Using velocity for compatibility. In Unity 6, rb.linearVelocity is preferred.
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
    private void HandleFootsteps()
    {
        isMoving = moveInput.sqrMagnitude > inputDeadzone * inputDeadzone;

        if (!isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWalk();
            }

            footstepTimer = footstepInterval;
        }
    }
}
