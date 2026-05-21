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

    [Header("FallZone Directional Nudge")]
    [SerializeField, Min(0f)] private float fallZoneNudgeDistance = 0.35f;
    [SerializeField, Min(0.01f)] private float fallZoneNudgeDuration = 0.16f;

    private Rigidbody2D rb;
    public PlayerAnimator playerAnimator;
    private PlayerCombat playerCombat;
    private PlayerChargeAttack playerChargeAttack;
    private PlayerDash playerDash;

    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;
    private float idleTimer;
    private int lastTimeMove;
    private float footstepTimer;

    private bool isDead;
    private bool isFalling;
    private bool canMove = true;
    private bool abilityMovementLocked;
    private Coroutine fallSequenceRoutine;
    private Coroutine fallZoneNudgeRoutine;
    private Coroutine deathSequenceRoutine;

    // 🔥 NEW: spawn protection
    private bool spawnGracePeriod = true;

    private bool IsLocked => isDead || isFalling || !canMove || abilityMovementLocked;

    public bool IsDead => isDead;
    public bool IsFalling => isFalling;
    public bool IsControlLocked => IsLocked;
    public Vector2 LastMoveDirection => lastMoveDirection.sqrMagnitude > 0.01f ? lastMoveDirection.normalized : Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<PlayerAnimator>();
            if (playerAnimator == null) playerAnimator = GetComponentInChildren<PlayerAnimator>();
        }

        playerCombat = GetComponent<PlayerCombat>();
        playerChargeAttack = GetComponent<PlayerChargeAttack>();
        playerDash = GetComponent<PlayerDash>();

        EnsureHurtBox();
    }

    private void EnsureHurtBox()
    {
        // Check if HurtBox already exists
        Transform hurtBoxTransform = transform.Find("HurtBox");
        if (hurtBoxTransform != null)
        {
            EnsureHurtBoxComponents(hurtBoxTransform.gameObject);
            return;
        }

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
        EnsureHurtBoxComponents(hurtBoxObj);
        
        Debug.Log("[PlayerController] Created HurtBox child dynamically.");
    }

    private void EnsureHurtBoxComponents(GameObject hurtBoxObj)
    {
        Collider2D hurtCollider = hurtBoxObj.GetComponent<Collider2D>();
        if (hurtCollider == null)
        {
            BoxCollider2D boxCollider = hurtBoxObj.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(0.8f, 1.0f);
            hurtCollider = boxCollider;
        }

        hurtCollider.isTrigger = true;

        if (hurtBoxObj.GetComponent<HurtBox>() == null)
        {
            hurtBoxObj.AddComponent<HurtBox>();
        }
    }

    private IEnumerator Start()
    {
        // 🔥 Prevent instant trigger on scene load
        playerAnimator?.SetLastMoveDirection(lastMoveDirection);
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
            lastMoveDirection = moveInput.normalized;
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
            playerAnimator.SetLastMoveDirection(lastMoveDirection);
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
        if (isDead || isFalling) return;
        if (fallSequenceRoutine != null) return;
        if (!canMove && !abilityMovementLocked) return;

        CancelActiveActionsForFailure();

        isFalling = true;
        canMove = false;
        abilityMovementLocked = false;

        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        playerAnimator?.SetLastMoveDirection(lastMoveDirection);
        playerAnimator?.InterruptToFallDive();

        if (fallZoneNudgeRoutine != null)
        {
            StopFallZoneDirectionalNudge();
        }

        fallZoneNudgeRoutine = StartCoroutine(FallZoneDirectionalNudgeRoutine());

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

        CancelActiveActionsForFailure();

        isDead = true;
        canMove = false;
        abilityMovementLocked = false;
        StopFallZoneDirectionalNudge();

        moveInput = Vector2.zero;
        ResetIdleTimer();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        playerAnimator?.SetLastMoveDirection(lastMoveDirection);
        playerAnimator?.InterruptToDeath();
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

    private IEnumerator FallZoneDirectionalNudgeRoutine()
    {
        if (fallZoneNudgeDistance <= 0f || fallZoneNudgeDuration <= 0f)
        {
            fallZoneNudgeRoutine = null;
            yield break;
        }

        Vector2 startPosition = rb.position;
        Vector2 endPosition = startPosition + GetFallZoneNudgeDirection() * fallZoneNudgeDistance;
        float elapsed = 0f;

        while (elapsed < fallZoneNudgeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallZoneNudgeDuration);
            rb.position = Vector2.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        rb.position = endPosition;
        fallZoneNudgeRoutine = null;
    }

    private Vector2 GetFallZoneNudgeDirection()
    {
        return lastMoveDirection.sqrMagnitude > 0.01f ? lastMoveDirection.normalized : Vector2.down;
    }

    private void StopFallZoneDirectionalNudge()
    {
        if (fallZoneNudgeRoutine == null) return;

        StopCoroutine(fallZoneNudgeRoutine);
        fallZoneNudgeRoutine = null;
    }

    private void CancelActiveActionsForFailure()
    {
        playerCombat?.CancelAttack();
        playerChargeAttack?.CancelChargeAttack();
        playerDash?.CancelDash();
        playerAnimator?.ResetActionTriggers();
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

    public void SetAbilityMovementLock(bool locked)
    {
        abilityMovementLocked = locked;

        if (locked)
        {
            moveInput = Vector2.zero;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            ResetIdleTimer();
            playerAnimator?.SetStandByStatus(false);
            playerAnimator?.UpdateAnimations(Vector2.zero, 0, true);
        }
    }

    // 🔥 NEW RESET
    public void ResetState()
    {
        isDead = false;
        isFalling = false;
        canMove = true;
        abilityMovementLocked = false;
        lastMoveDirection = Vector2.down;
        playerDash?.CancelDash();
        StopFallZoneDirectionalNudge();
        ResetIdleTimer();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;

        playerAnimator?.SetFallingStatus(false);
        playerAnimator?.SetLastMoveDirection(lastMoveDirection);
        playerAnimator?.SetDeadStatus(false);
        playerAnimator?.SetDashingStatus(false);
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
