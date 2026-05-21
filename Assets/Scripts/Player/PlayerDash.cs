using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Dash Settings")]
    [SerializeField, Min(0f)] private float dashSpeed = 20f;
    [SerializeField, Min(0f)] private float dashDuration = 0.18f;
    [SerializeField, Min(0f)] private float dashDistance = 3.6f;
    [SerializeField, Min(0f)] private float dashCooldown = 1f;

    [Header("Collision")]
    [SerializeField] private LayerMask solidLayerMask;
    [SerializeField, Min(0f)] private float obstacleSkinWidth = 0.03f;
    [SerializeField, Min(0f)] private float stuckDistanceThreshold = 0.01f;
    [SerializeField, Min(0f)] private float stuckTimeThreshold = 0.08f;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private Health health;
    [SerializeField] private ChargeTrail dashTrail;

    [Header("Trail")]
    [SerializeField] private bool clearDashTrailOnStart = true;
    [SerializeField] private bool clearDashTrailOnStop;

    private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];
    private Rigidbody2D rb;
    private Vector2 dashDirection = Vector2.down;
    private float dashElapsed;
    private float dashDistanceTravelled;
    private float nextDashReadyTime;
    private float stuckTimer;
    private bool isDashing;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();
        if (playerAnimator == null && playerController != null) playerAnimator = playerController.playerAnimator;
        if (health == null) health = GetComponent<Health>();
        if (dashTrail == null) dashTrail = GetComponentInChildren<ChargeTrail>(true);

        if (solidLayerMask.value == 0)
        {
            solidLayerMask = LayerMask.GetMask("Default", "Water");
        }

        dashTrail?.Stop(true);
    }

    private void OnDisable()
    {
        CancelDash();
    }

    private void Update()
    {
        if (ShouldCancelForPlayerState())
        {
            CancelDash();
            return;
        }

        if (Input.GetKeyDown(dashKey))
        {
            TryStartDash();
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing) return;

        ExecuteDashMovement();
    }

    public void CancelDash()
    {
        EndDash(false, true);
    }

    private void TryStartDash()
    {
        if (isDashing) return;
        if (Time.time < nextDashReadyTime) return;
        if (playerController == null || playerController.IsControlLocked) return;
        if (health != null && health.IsDead) return;
        if (dashSpeed <= 0f || dashDuration <= 0f || dashDistance <= 0f) return;

        dashDirection = GetDashDirection();
        dashElapsed = 0f;
        dashDistanceTravelled = 0f;
        stuckTimer = 0f;
        isDashing = true;

        playerController.SetAbilityMovementLock(true);
        playerAnimator?.SetLastMoveDirection(dashDirection);
        playerAnimator?.SetDashingStatus(true);
        StartDashTrail();
    }

    private Vector2 GetDashDirection()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(x) > Mathf.Abs(y) && Mathf.Abs(x) > 0f)
        {
            return x > 0f ? Vector2.right : Vector2.left;
        }

        if (Mathf.Abs(y) > 0f)
        {
            return y > 0f ? Vector2.up : Vector2.down;
        }

        return playerController != null ? playerController.LastMoveDirection : Vector2.down;
    }

    private bool ShouldCancelForPlayerState()
    {
        if (!isDashing) return false;
        if (health != null && health.IsDead) return true;

        return playerController != null && playerController.IsFalling;
    }

    private void ExecuteDashMovement()
    {
        dashElapsed += Time.fixedDeltaTime;

        float requestedDistance = dashSpeed * Time.fixedDeltaTime;
        float remainingDistance = Mathf.Max(0f, dashDistance - dashDistanceTravelled);
        float moveDistance = Mathf.Min(requestedDistance, remainingDistance);
        bool obstacleHit = TryClampDistanceForObstacle(ref moveDistance);

        if (moveDistance > 0f)
        {
            rb.MovePosition(rb.position + dashDirection * moveDistance);
            dashDistanceTravelled += moveDistance;
        }

        UpdateStuckGuard(moveDistance);

        if (obstacleHit ||
            dashElapsed >= dashDuration ||
            dashDistanceTravelled >= dashDistance ||
            (stuckTimeThreshold > 0f && stuckTimer >= stuckTimeThreshold))
        {
            FinishDash();
        }
    }

    private bool TryClampDistanceForObstacle(ref float moveDistance)
    {
        if (moveDistance <= 0f || solidLayerMask.value == 0) return false;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(solidLayerMask);
        filter.useTriggers = false;

        int hitCount = rb.Cast(dashDirection, filter, castHits, moveDistance + obstacleSkinWidth);
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

    private void UpdateStuckGuard(float moveDistance)
    {
        if (stuckTimeThreshold <= 0f || stuckDistanceThreshold <= 0f)
        {
            stuckTimer = 0f;
            return;
        }

        if (moveDistance <= stuckDistanceThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;
            return;
        }

        stuckTimer = 0f;
    }

    private void FinishDash()
    {
        EndDash(true, false);
    }

    private void EndDash(bool applyCooldown, bool forceClearTrail)
    {
        if (!isDashing) return;

        isDashing = false;
        dashElapsed = 0f;
        dashDistanceTravelled = 0f;
        stuckTimer = 0f;

        StopDashTrail(forceClearTrail);
        playerAnimator?.SetDashingStatus(false);
        playerController?.SetAbilityMovementLock(false);
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (applyCooldown)
        {
            nextDashReadyTime = Time.time + dashCooldown;
        }
    }

    private void StartDashTrail()
    {
        if (dashTrail == null) return;

        if (clearDashTrailOnStart)
        {
            dashTrail.Clear();
        }

        dashTrail.Play();
    }

    private void StopDashTrail(bool forceClear)
    {
        dashTrail?.Stop(forceClear || clearDashTrailOnStop);
    }
}
