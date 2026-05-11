using UnityEngine;

/// <summary>
/// Clean deterministic player controller.
/// Single responsibility: state + movement.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float inputDeadzone = 0.1f;
    [SerializeField] private int idleThreshold = 5;

    [Header("Footsteps")]
    [SerializeField] private float footstepInterval = 0.4f;

    private Rigidbody2D rb;
    public PlayerAnimator playerAnimator;

    private Vector2 moveInput;
    private float idleTimer;
    private int lastTimeMove;
    private float footstepTimer;

    private bool isDead;
    private bool isFalling;
    private bool canMove = true;

    private bool IsLocked => isDead || isFalling || !canMove;

    public bool IsDead => isDead;
    public bool IsFalling => isFalling;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        HandleInput();
        HandleAnimation();
        HandleFootsteps();
    }

    private void HandleInput()
    {
        if (IsLocked)
        {
            moveInput = Vector2.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        moveInput =
            Mathf.Abs(x) > inputDeadzone ? new Vector2(x, 0) :
            Mathf.Abs(y) > inputDeadzone ? new Vector2(0, y) :
            Vector2.zero;

        bool idle = moveInput.sqrMagnitude < inputDeadzone * inputDeadzone;

        if (idle)
        {
            idleTimer += Time.deltaTime;
            lastTimeMove = Mathf.FloorToInt(idleTimer);

            if (lastTimeMove >= idleThreshold)
                playerAnimator?.SetStandByStatus(true);
        }
        else
        {
            idleTimer = 0;
            lastTimeMove = 0;
            playerAnimator?.SetStandByStatus(false);
        }
    }

    private void HandleAnimation()
    {
        if (playerAnimator == null) return;

        if (IsLocked)
        {
            playerAnimator.SetFallingStatus(isFalling);
            playerAnimator.UpdateAnimations(Vector2.zero, lastTimeMove, true);
            return;
        }

        playerAnimator.UpdateAnimations(moveInput, lastTimeMove, false);
    }

    private void FixedUpdate()
    {
        if (IsLocked)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    private void HandleFootsteps()
    {
        if (IsLocked)
        {
            footstepTimer = 0;
            return;
        }

        bool moving = moveInput.sqrMagnitude > inputDeadzone * inputDeadzone;

        if (!moving)
        {
            footstepTimer = 0;
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            AudioManager.Instance?.PlayWalk();
            footstepTimer = footstepInterval;
        }
    }

    public void StartFallSequence()
    {
        if (IsLocked) return;

        isFalling = true;
        canMove = false;

        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        playerAnimator?.SetFallingStatus(true);

        StartCoroutine(FallRoutine());
    }

    private System.Collections.IEnumerator FallRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void Die()
    {
        if (IsLocked) return;

        isDead = true;
        canMove = false;

        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        playerAnimator?.TriggerDie();
    }

    public void ResetIdleTimer()
    {
        idleTimer = 0;
        lastTimeMove = 0;
    }
}