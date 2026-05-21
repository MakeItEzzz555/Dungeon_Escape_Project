using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerChargeAttack : MonoBehaviour
{
    private enum ChargeState
    {
        Ready,
        WindUp,
        Charging,
        Recovery
    }

    [Header("Input")]
    [SerializeField] private KeyCode chargeKey = KeyCode.Q;

    [Header("Charge Settings")]
    [SerializeField, Min(0f)] private float chargeCooldown = 1.5f;
    [SerializeField, Min(0f)] private float chargeSpeed = 8f;
    [SerializeField, Min(0f)] private float chargeDistance = 4f;
    [SerializeField, Min(0f)] private float chargeRecoveryDuration = 0.25f;
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField, Min(0f)] private float obstacleSkinWidth = 0.03f;

    [Header("Animation Fallbacks")]
    [SerializeField, Min(0f)] private float dashEventFallbackDelay = 0f;
    [SerializeField, Min(0.01f)] private float maxChargeDurationFallback = 1.25f;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private Health health;
    [SerializeField] private Hitbox chargeHitbox;
    [SerializeField] private ChargeTrail chargeTrail;

    [Header("Charge Trail")]
    [SerializeField] private bool clearChargeTrailOnDashStart = true;
    [SerializeField] private bool clearChargeTrailOnDashStop;

    private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];
    private Rigidbody2D rb;
    private Vector2 chargeDirection = Vector2.down;
    private float chargeDistanceTravelled;
    private float nextChargeReadyTime;
    private float dashFallbackTime;
    private float maxChargeEndTime;
    private float recoveryEndTime;
    private bool chargeUnlocked;
    private bool dashStarted;
    private ChargeState chargeState = ChargeState.Ready;

    public bool IsUnlocked => chargeUnlocked;
    public bool IsChargeActive => chargeState != ChargeState.Ready;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();
        if (playerAnimator == null && playerController != null) playerAnimator = playerController.playerAnimator;
        if (health == null) health = GetComponent<Health>();
        if (chargeTrail == null) chargeTrail = GetComponentInChildren<ChargeTrail>(true);
        if (chargeHitbox == null) ResolveChargeHitboxIfMissing();

        chargeHitbox?.DisableHitbox();
        chargeTrail?.Stop(true);
    }

    private void OnDisable()
    {
        EndChargeImmediately();
    }

    private void Update()
    {
        if (ShouldCancelForPlayerState())
        {
            EndChargeImmediately();
            return;
        }

        UpdateFallbacks();

        if (Input.GetKeyDown(chargeKey))
        {
            TryStartChargeAttack();
        }
    }

    private void FixedUpdate()
    {
        if (chargeState != ChargeState.Charging) return;

        ExecuteChargeMovement();
    }

    public void UnlockChargeAttack()
    {
        chargeUnlocked = true;
    }

    public void StartChargeDash()
    {
        if (chargeState != ChargeState.WindUp || dashStarted) return;

        dashStarted = true;
        chargeDistanceTravelled = 0f;

        if (clearChargeTrailOnDashStart)
        {
            chargeTrail?.Clear();
        }

        chargeTrail?.Play();
        chargeState = ChargeState.Charging;
    }

    public void StartDash()
    {
        StartChargeDash();
    }

    public void EnableChargeHitbox()
    {
        if (!IsChargeActive) return;

        chargeHitbox?.EnableHitbox();
    }

    public void DisableChargeHitbox()
    {
        chargeHitbox?.DisableHitbox();
    }

    public void EndChargeAttack()
    {
        if (!IsChargeActive) return;

        FinishChargeAttack();
    }

    public void CancelChargeAttack()
    {
        EndChargeImmediately();
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

    private void TryStartChargeAttack()
    {
        if (!chargeUnlocked) return;
        if (IsChargeActive) return;
        if (Time.time < nextChargeReadyTime) return;
        if (playerController == null || playerController.IsControlLocked) return;
        if (health != null && health.IsDead) return;

        chargeDirection = playerController.LastMoveDirection;
        chargeDistanceTravelled = 0f;
        dashStarted = false;
        dashFallbackTime = dashEventFallbackDelay > 0f
            ? Time.time + dashEventFallbackDelay
            : float.PositiveInfinity;
        maxChargeEndTime = Time.time + maxChargeDurationFallback;
        recoveryEndTime = 0f;

        playerController.SetAbilityMovementLock(true);
        playerAnimator?.SetLastMoveDirection(chargeDirection);
        playerAnimator?.PlayChargeAttack();
        chargeState = ChargeState.WindUp;
    }

    private bool ShouldCancelForPlayerState()
    {
        if (!IsChargeActive) return false;
        if (health != null && health.IsDead) return true;

        return playerController != null && playerController.IsFalling;
    }

    private void UpdateFallbacks()
    {
        if (!IsChargeActive) return;

        if (dashEventFallbackDelay > 0f && chargeState == ChargeState.WindUp && Time.time >= dashFallbackTime)
        {
            StartChargeDash();
        }

        if (chargeState == ChargeState.Recovery && Time.time >= recoveryEndTime)
        {
            FinishChargeAttack();
            return;
        }

        if (Time.time >= maxChargeEndTime)
        {
            FinishChargeAttack();
        }
    }

    private void ExecuteChargeMovement()
    {
        if (chargeSpeed <= 0f || chargeDistance <= 0f)
        {
            BeginChargeRecovery();
            return;
        }

        float requestedDistance = chargeSpeed * Time.fixedDeltaTime;
        float remainingDistance = Mathf.Max(0f, chargeDistance - chargeDistanceTravelled);
        float moveDistance = Mathf.Min(requestedDistance, remainingDistance);
        bool obstacleHit = TryClampDistanceForObstacle(ref moveDistance);

        if (moveDistance > 0f)
        {
            rb.MovePosition(rb.position + chargeDirection * moveDistance);
            chargeDistanceTravelled += moveDistance;
        }

        if (obstacleHit || chargeDistanceTravelled >= chargeDistance)
        {
            BeginChargeRecovery();
        }
    }

    private bool TryClampDistanceForObstacle(ref float moveDistance)
    {
        if (moveDistance <= 0f || obstacleMask.value == 0) return false;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(obstacleMask);
        filter.useTriggers = false;

        int hitCount = rb.Cast(chargeDirection, filter, castHits, moveDistance + obstacleSkinWidth);
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCollider = castHits[i].collider;
            if (hitCollider == null) continue;
            if (hitCollider.transform.root == transform.root) continue;

            closestDistance = Mathf.Min(closestDistance, castHits[i].distance);
        }

        if (float.IsPositiveInfinity(closestDistance)) return false;

        moveDistance = Mathf.Max(0f, closestDistance - obstacleSkinWidth);
        return true;
    }

    private void BeginChargeRecovery()
    {
        if (chargeState == ChargeState.Recovery) return;

        chargeHitbox?.DisableHitbox();
        StopChargeTrail(false);
        recoveryEndTime = Time.time + chargeRecoveryDuration;
        chargeState = ChargeState.Recovery;
    }

    private void FinishChargeAttack()
    {
        chargeHitbox?.DisableHitbox();
        StopChargeTrail(false);
        playerController?.SetAbilityMovementLock(false);
        rb.linearVelocity = Vector2.zero;
        nextChargeReadyTime = Time.time + chargeCooldown;
        chargeState = ChargeState.Ready;
        dashStarted = false;
        recoveryEndTime = 0f;
    }

    private void EndChargeImmediately()
    {
        if (!IsChargeActive) return;

        chargeHitbox?.DisableHitbox();
        StopChargeTrail(true);
        playerController?.SetAbilityMovementLock(false);
        if (rb != null) rb.linearVelocity = Vector2.zero;
        chargeState = ChargeState.Ready;
        dashStarted = false;
        recoveryEndTime = 0f;
    }

    private void StopChargeTrail(bool forceClear)
    {
        chargeTrail?.Stop(forceClear || clearChargeTrailOnDashStop);
    }

    private void ResolveChargeHitboxIfMissing()
    {
        Hitbox[] hitboxes = GetComponentsInChildren<Hitbox>(true);
        for (int i = 0; i < hitboxes.Length; i++)
        {
            Hitbox hitbox = hitboxes[i];
            if (hitbox == null) continue;

            if (hitbox.gameObject.name.IndexOf("Charge", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                chargeHitbox = hitbox;
                return;
            }
        }
    }
}
