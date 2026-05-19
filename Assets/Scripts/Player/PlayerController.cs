using System.Collections;
using Scripts.Managers;
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

    [Header("Death")]
    [SerializeField] private float deathRespawnDelay = 1.5f;
    [SerializeField] private float fallRespawnDelay = 1.5f;

    private Rigidbody2D rb;
    public PlayerAnimator playerAnimator;

    private Vector2 moveInput;
    private float idleTimer;
    private int lastTimeMove;
    private float footstepTimer;

    private bool isDead;
    private bool isFalling;
    private bool canMove = true;
    private Coroutine fallSequenceRoutine;
    private Coroutine deathSequenceRoutine;

    // 🔥 NEW: spawn protection
    private bool spawnGracePeriod = true;

    private bool IsLocked => isDead || isFalling || !canMove;

    public bool IsDead => isDead;
    public bool IsFalling => isFalling;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        EnsureHurtBox();
    }

    private void EnsureHurtBox()
    {
        // Check if HurtBox already exists
        Transform hurtBoxTransform = transform.Find("HurtBox");
        if (hurtBoxTransform != null) return;

        // Create HurtBox GameObject
        GameObject hurtBoxObj = new GameObject("HurtBox");
        hurtBoxObj.transform.SetParent(this.transform);
        hurtBoxObj.transform.localPosition = new Vector3(0, 0.3f, 0);
        hurtBoxObj.transform.localRotation = Quaternion.identity;
        hurtBoxObj.transform.localScale = Vector3.one;

        // Add BoxCollider2D
        BoxCollider2D col = hurtBoxObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1.0f);
        col.offset = Vector2.zero; // Local position already handles Y offset
        
        Debug.Log("[PlayerController] Created HurtBox child dynamically.");
    }

    private IEnumerator Start()
    {
        // 🔥 Prevent instant trigger on scene load
        spawnGracePeriod = true;
        yield return new WaitForSeconds(0.2f);
        spawnGracePeriod = false;
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
            {
                // Only trigger if we aren't already in standby.
                if (playerAnimator != null && !playerAnimator.IsInStandByActive)
                {
                    Debug.Log($"[DEBUG_LOG] PlayerController: Triggering StandBy. idleTimer: {idleTimer:F2}, lastTimeMove: {lastTimeMove}");
                    playerAnimator.SetStandByStatus(true);
                    
                    // 🔥 AUTO-EXIT FALLBACK:
                    // If the animation event fails for some reason, we manually reset after a duration.
                    // This is a safety measure to prevent getting stuck.
                    StartCoroutine(AutoExitStandBy(6f)); // Slightly longer than the 5.25s animation
                }
            }
        }
        else
        {
            ResetIdleTimer();
            playerAnimator?.SetStandByStatus(false);
        }
    }

    private void HandleAnimation()
    {
        if (playerAnimator == null) return;

        if (IsLocked)
        {
            playerAnimator.SetFallingStatus(isFalling);
            // Pass lastTimeMove = 0 when locked to prevent StandBy logic from seeing old timer values
            playerAnimator.UpdateAnimations(Vector2.zero, 0, true);
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

        fallSequenceRoutine = StartCoroutine(FallRoutine());
    }

    public void Die()
    {
        StartDeathSequence();
    }

    public void OnDeath()
    {
        StartDeathSequence();
    }

    public void StartDeathSequence()
    {
        if (isDead || isFalling) return;
        if (deathSequenceRoutine != null) return;

        isDead = true;
        canMove = false;

        moveInput = Vector2.zero;
        ResetIdleTimer();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        playerAnimator?.SetFallingStatus(false);
        playerAnimator?.SetStandByStatus(false);
        playerAnimator?.SetDeadStatus(true);
        playerAnimator?.TriggerDie();

        deathSequenceRoutine = StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return StartRespawnTransition(deathRespawnDelay);
    }

    private IEnumerator FallRoutine()
    {
        yield return StartRespawnTransition(fallRespawnDelay);
    }

    private IEnumerator StartRespawnTransition(float failureAnimationDuration)
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.BeginRespawnTransition(transform, failureAnimationDuration);
            yield break;
        }

        Debug.LogError("[DEBUG_LOG] PlayerController: SceneTransitionManager missing. Cannot show RunResultsPanel or respawn safely.");
        yield return new WaitForSeconds(failureAnimationDuration);
    }

    public void SetControlEnabled(bool enabled)
    {
        canMove = enabled;

        if (!enabled)
        {
            moveInput = Vector2.zero;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            
            // Immediately kill standby state when locking
            ResetIdleTimer();
            playerAnimator?.SetStandByStatus(false);
        }
    }

    // 🔥 NEW RESET
    public void ResetState()
    {
        isDead = false;
        isFalling = false;
        canMove = true;
        ResetIdleTimer();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;

        playerAnimator?.SetFallingStatus(false);
        playerAnimator?.SetDeadStatus(false);
        playerAnimator?.SetStandByStatus(false);

        fallSequenceRoutine = null;
        deathSequenceRoutine = null;
    }

    // 🔥 NEW: used by FallZone
    public bool IsSpawnProtected()
    {
        return spawnGracePeriod;
    }

    public void ResetIdleTimer()
    {
        Debug.Log("[DEBUG_LOG] PlayerController: ResetIdleTimer called.");
        idleTimer = 0;
        lastTimeMove = 0;
    }

    public void OnStandByAnimationEnd()
    {
        Debug.Log("[DEBUG_LOG] PlayerController: OnStandByAnimationEnd triggered via Animation Event.");
        ResetIdleTimer();
        playerAnimator?.SetStandByStatus(false);
    }

    private IEnumerator AutoExitStandBy(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // If we are still in standby and haven't moved, force reset
        if (playerAnimator != null && playerAnimator.IsInStandByActive)
        {
            Debug.LogWarning("[DEBUG_LOG] PlayerController: AutoExitStandBy TRIGGERED. Animation event might have failed.");
            ResetIdleTimer();
            playerAnimator.SetStandByStatus(false);
        }
    }
}
