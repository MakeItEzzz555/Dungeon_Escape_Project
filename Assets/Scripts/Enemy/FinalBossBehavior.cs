using System.Collections;
using UnityEngine;
using Scripts.Managers;

[RequireComponent(typeof(Rigidbody2D))]
public class FinalBossBehavior : MonoBehaviour, IEnemyRangeReceiver
{
    private enum FinalBossState
    {
        IdleAtHome,
        Chasing,
        RegularAttacking,
        ChargeWindUp,
        Charging,
        ChargeRecovery,
        ReturningHome,
        Dead
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float returnStopDistance = 0.05f;
    [SerializeField] private float attackDistanceFallback = 1.5f;

    [Header("Regular Attack")]
    [SerializeField] private float regularAttackCooldown = 2f;
    [SerializeField] private float regularAttackLockDuration = 0.35f;

    [Header("Phase Two")]
    [SerializeField, Range(0f, 1f)] private float phaseTwoHealthThreshold = 0.5f;

    [Header("Charge Attack")]
    [SerializeField] private float chargeAttackCooldown = 6f;
    [SerializeField] private float chargeWindUpDuration = 0.35f;
    [SerializeField] private float chargeAnimationTriggerDelay = 0.25f;
    [SerializeField] private float minimumChargeDashDelayAfterAnimationTrigger = 0.15f;
    [SerializeField] private bool startChargeDashFromAnimationEvent = true;
    [SerializeField] private float chargeDashEventFallbackDuration = 1f;
    [SerializeField] private float chargeSpeed = 8f;
    [SerializeField] private float chargeDistance = 4f;
    [SerializeField] private float chargeRecoveryDuration = 0.75f;
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private float obstacleSkinWidth = 0.03f;

    [Header("Charge Trail")]
    [SerializeField] private ChargeTrail chargeTrail;
    [SerializeField] private bool clearChargeTrailOnDashStart = true;
    [SerializeField] private bool clearChargeTrailOnDashStop;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Health health;
    [SerializeField] private FinalBossAnimationBridge animationBridge;
    [SerializeField] private FinalBossCombatBridge combatBridge;

    [Header("Boss HUD")]
    [SerializeField] private bool showBossHudOnAggro = true;

    [Header("Death Rewards")]
    [SerializeField] private GameObject deathRewardObject;
    [SerializeField] private bool showRunResultsOnDeath;
    [SerializeField] private string resultsTargetScene = "Main Menu";
    [SerializeField] private Transform resultsZoomTarget;
    [SerializeField] private float bossDeathTransitionDelay = 1.68f;

    private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];
    private const float PlayerResolveRetryInterval = 0.5f;

    private Rigidbody2D rb;
    private Vector2 homePosition;
    private Vector2 chargeDirection = Vector2.down;
    private Vector2 lastFacingDirection = Vector2.down;
    private float nextRegularAttackTime;
    private float nextChargeAttackTime;
    private float stateUnlockTime;
    private float chargeAnimationTriggerTime;
    private float earliestChargeDashTime;
    private float chargeDistanceTravelled;
    private float nextPlayerResolveTime;
    private bool playerInAggroRange;
    private bool playerInAttackRange;
    private bool hasAttackRangeTrigger;
    private bool phaseTwoUnlocked;
    private bool deathRewardsProcessed;
    private bool chargeAnimationTriggered;
    private FinalBossState currentState = FinalBossState.IdleAtHome;
    private FinalBossState previousState;

    public bool IsPhaseTwoUnlocked => phaseTwoUnlocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        if (health == null) health = GetComponent<Health>();
        if (animationBridge == null) animationBridge = GetComponentInChildren<FinalBossAnimationBridge>();
        if (combatBridge == null) combatBridge = GetComponentInChildren<FinalBossCombatBridge>();

        ResolveChargeTrailIfMissing();
        StopChargeTrail(true);

        homePosition = rb.position;
        HideDeathRewardAtRuntimeStart();
        hasAttackRangeTrigger = HasAttackRangeTrigger();
        ResolvePlayerIfMissing();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDied += HandleDied;
        }
    }

    private void Start()
    {
        EvaluatePhaseTwo();
        animationBridge?.SetFacingDirection(lastFacingDirection);
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged -= HandleHealthChanged;
            health.OnDied -= HandleDied;
        }

        combatBridge?.DisableAllHitboxes();
        StopChargeTrail(true);
        HideBossHud();
    }

    private void Update()
    {
        if (health != null && health.IsDead)
        {
            ChangeState(FinalBossState.Dead);
        }

        if (currentState == FinalBossState.Dead)
        {
            animationBridge?.SetWalking(false);
            combatBridge?.DisableAllHitboxes();
            StopChargeTrail(true);
            return;
        }

        ResolvePlayerIfMissing();
        ExecuteState();
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case FinalBossState.IdleAtHome:
                animationBridge?.SetWalking(false);
                UpdateNonCommittedState();
                break;

            case FinalBossState.Chasing:
                ChasePlayer();
                UpdateNonCommittedState();
                break;

            case FinalBossState.RegularAttacking:
                animationBridge?.SetWalking(false);
                if (Time.time >= stateUnlockTime)
                {
                    UpdateNonCommittedState();
                }
                break;

            case FinalBossState.ChargeWindUp:
                animationBridge?.SetWalking(false);
                TryTriggerChargeAnimation();
                if (Time.time >= stateUnlockTime)
                {
                    StartChargeDash();
                }
                break;

            case FinalBossState.Charging:
                ExecuteChargeMovement();
                break;

            case FinalBossState.ChargeRecovery:
                animationBridge?.SetWalking(false);
                if (Time.time >= stateUnlockTime)
                {
                    UpdateNonCommittedState();
                }
                break;

            case FinalBossState.ReturningHome:
                ReturnHome();
                UpdateNonCommittedState();
                break;
        }
    }

    private void UpdateNonCommittedState()
    {
        FinalBossState desiredState = GetDesiredNonCommittedState();

        if (desiredState == FinalBossState.RegularAttacking)
        {
            TryStartAttack();
            return;
        }

        ChangeState(desiredState);
    }

    private FinalBossState GetDesiredNonCommittedState()
    {
        if (playerInAggroRange)
        {
            bool canAttack = playerInAttackRange || ShouldUseAttackDistanceFallback();
            return canAttack ? FinalBossState.RegularAttacking : FinalBossState.Chasing;
        }

        bool awayFromHome = Vector2.Distance(rb.position, homePosition) > returnStopDistance;
        return awayFromHome ? FinalBossState.ReturningHome : FinalBossState.IdleAtHome;
    }

    private void TryStartAttack()
    {
        if (phaseTwoUnlocked && Time.time >= nextChargeAttackTime)
        {
            BeginChargeWindUp();
            return;
        }

        if (Time.time < nextRegularAttackTime)
        {
            FacePlayer();
            animationBridge?.SetWalking(false);
            stateUnlockTime = Mathf.Min(nextRegularAttackTime, Time.time + 0.1f);
            ChangeState(FinalBossState.RegularAttacking);
            return;
        }

        FacePlayer();
        animationBridge?.SetWalking(false);
        animationBridge?.PlayAttack();
        nextRegularAttackTime = Time.time + regularAttackCooldown;
        stateUnlockTime = Time.time + regularAttackLockDuration;
        ChangeState(FinalBossState.RegularAttacking);
    }

    private void BeginChargeWindUp()
    {
        FacePlayer();
        chargeDirection = GetDirectionToPlayer();
        SetFacingFromDirection(chargeDirection);
        chargeDistanceTravelled = 0f;
        nextChargeAttackTime = Time.time + chargeAttackCooldown;
        chargeAnimationTriggered = false;
        chargeAnimationTriggerTime = Time.time + Mathf.Max(0f, chargeAnimationTriggerDelay);
        earliestChargeDashTime = chargeAnimationTriggerTime + Mathf.Max(0f, minimumChargeDashDelayAfterAnimationTrigger);
        stateUnlockTime = chargeAnimationTriggerTime + GetChargeDashFallbackDelay();

        animationBridge?.SetWalking(false);
        animationBridge?.SetFacingDirection(chargeDirection);
        ChangeState(FinalBossState.ChargeWindUp);
        TryTriggerChargeAnimation();
    }

    public void StartChargeDash()
    {
        if (currentState != FinalBossState.ChargeWindUp) return;
        if (!chargeAnimationTriggered)
        {
            TryTriggerChargeAnimation();
            return;
        }

        if (Time.time < earliestChargeDashTime) return;

        BeginCharging();
    }

    private void TryTriggerChargeAnimation()
    {
        if (chargeAnimationTriggered) return;
        if (Time.time < chargeAnimationTriggerTime) return;

        animationBridge?.SetWalking(false);
        animationBridge?.SetFacingDirection(chargeDirection);
        animationBridge?.PlayChargeAttack();
        chargeAnimationTriggered = true;
    }

    private float GetChargeDashFallbackDelay()
    {
        if (!startChargeDashFromAnimationEvent) return chargeWindUpDuration;

        return chargeDashEventFallbackDuration > 0f
            ? chargeDashEventFallbackDuration
            : float.PositiveInfinity;
    }

    private void BeginCharging()
    {
        StartChargeTrail();
        combatBridge?.EnableChargeHitbox();
        ChangeState(FinalBossState.Charging);
    }

    private void ExecuteChargeMovement()
    {
        animationBridge?.SetWalking(false);

        float requestedDistance = chargeSpeed * Time.deltaTime;
        float remainingDistance = Mathf.Max(0f, chargeDistance - chargeDistanceTravelled);
        float moveDistance = Mathf.Min(requestedDistance, remainingDistance);
        bool obstacleHit = TryClampDistanceForObstacle(ref moveDistance);

        if (moveDistance > 0f)
        {
            MoveTo(rb.position + chargeDirection * moveDistance);
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
            if (IsPlayerCollider(hitCollider)) continue;

            closestDistance = Mathf.Min(closestDistance, castHits[i].distance);
        }

        if (float.IsPositiveInfinity(closestDistance)) return false;

        moveDistance = Mathf.Max(0f, closestDistance - obstacleSkinWidth);
        return true;
    }

    private bool IsPlayerCollider(Collider2D hitCollider)
    {
        return hitCollider.CompareTag("Player")
            || hitCollider.transform.root.CompareTag("Player")
            || hitCollider.GetComponentInParent<PlayerController>() != null;
    }

    private void BeginChargeRecovery()
    {
        combatBridge?.DisableChargeHitbox();
        StopChargeTrail(false);
        stateUnlockTime = Time.time + chargeRecoveryDuration;
        ChangeState(FinalBossState.ChargeRecovery);
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        SetFacingFromDirection(direction);
        animationBridge?.SetMovement(lastFacingDirection);

        Vector2 nextPosition = Vector2.MoveTowards(rb.position, player.position, moveSpeed * Time.deltaTime);
        MoveTo(nextPosition);
    }

    private void ReturnHome()
    {
        Vector2 direction = (homePosition - rb.position).normalized;
        SetFacingFromDirection(direction);
        animationBridge?.SetMovement(lastFacingDirection);

        Vector2 nextPosition = Vector2.MoveTowards(rb.position, homePosition, moveSpeed * Time.deltaTime);
        MoveTo(nextPosition);

        if (Vector2.Distance(nextPosition, homePosition) <= returnStopDistance)
        {
            MoveTo(homePosition);
            animationBridge?.ClearMovement();
            ChangeState(FinalBossState.IdleAtHome);
        }
    }

    private void MoveTo(Vector2 position)
    {
        rb.MovePosition(position);
    }

    private bool IsPlayerCloseEnoughToAttack()
    {
        if (player == null) return false;

        return Vector2.Distance(rb.position, player.position) <= attackDistanceFallback;
    }

    private bool ShouldUseAttackDistanceFallback()
    {
        if (hasAttackRangeTrigger) return false;

        return IsPlayerCloseEnoughToAttack();
    }

    private bool HasAttackRangeTrigger()
    {
        Transform attackRange = transform.Find("AttackRange");
        if (attackRange == null) return false;
        if (!attackRange.gameObject.activeInHierarchy) return false;
        if (!attackRange.TryGetComponent(out EnemyRangeTrigger _)) return false;
        if (!attackRange.TryGetComponent(out Collider2D rangeCollider)) return false;

        return rangeCollider.enabled;
    }

    private void FacePlayer()
    {
        Vector2 direction = GetDirectionToPlayer();
        SetFacingFromDirection(direction);
        animationBridge?.SetFacingDirection(lastFacingDirection);
    }

    private Vector2 GetDirectionToPlayer()
    {
        if (player == null) return lastFacingDirection;

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        return direction.sqrMagnitude > 0.01f ? direction : lastFacingDirection;
    }

    private void SetFacingFromDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.01f) return;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            lastFacingDirection = direction.x > 0f ? Vector2.right : Vector2.left;
        else
            lastFacingDirection = direction.y > 0f ? Vector2.up : Vector2.down;

        animationBridge?.SetFacingDirection(lastFacingDirection);
    }

    private void ResolvePlayerIfMissing()
    {
        if (player != null) return;
        if (Time.time < nextPlayerResolveTime) return;

        nextPlayerResolveTime = Time.time + PlayerResolveRetryInterval;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        EvaluatePhaseTwo();
    }

    private void EvaluatePhaseTwo()
    {
        if (phaseTwoUnlocked || health == null || health.maxHealth <= 0) return;

        float normalizedHealth = health.currentHealth / (float)health.maxHealth;
        if (normalizedHealth <= phaseTwoHealthThreshold)
        {
            phaseTwoUnlocked = true;
        }
    }

    private void HandleDied(Health deadHealth)
    {
        OnDeath();
    }

    public void OnDeath()
    {
        if (deathRewardsProcessed) return;

        deathRewardsProcessed = true;
        ChangeState(FinalBossState.Dead);
        animationBridge?.SetWalking(false);
        animationBridge?.SetDeadStatus(true);
        AudioManager.Instance?.PlayBossDeath();
        combatBridge?.DisableAllHitboxes();
        StopChargeTrail(true);
        HideBossHud();
        RevealDeathRewardIfAssigned();
        StartCoroutine(ShowResultsAfterDeath());
    }

    private IEnumerator ShowResultsAfterDeath()
    {
        yield return new WaitForSeconds(bossDeathTransitionDelay);
        ShowRunResultsIfEnabled();
    }

    private void HideDeathRewardAtRuntimeStart()
    {
        if (deathRewardObject == null) return;

        deathRewardObject.SetActive(false);
    }

    private void RevealDeathRewardIfAssigned()
    {
        if (deathRewardObject == null) return;

        deathRewardObject.SetActive(true);
    }

    private void ShowRunResultsIfEnabled()
    {
        if (!showRunResultsOnDeath) return;

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning("[FinalBossBehavior] SceneTransitionManager missing. Cannot show RunResultsPanel on FinalBoss death.");
            return;
        }

        if (string.IsNullOrWhiteSpace(resultsTargetScene))
        {
            Debug.LogWarning("[FinalBossBehavior] Results target scene is empty. Cannot show completion results on FinalBoss death.");
            return;
        }

        Transform zoomTarget = resultsZoomTarget != null ? resultsZoomTarget : transform;
        SceneTransitionManager.Instance.BeginLevelCompletionTransition(resultsTargetScene, zoomTarget);
    }

    private void ResolveChargeTrailIfMissing()
    {
        if (chargeTrail != null) return;

        chargeTrail = GetComponentInChildren<ChargeTrail>(true);
        if (chargeTrail == null) chargeTrail = gameObject.AddComponent<ChargeTrail>();
    }

    private void StartChargeTrail()
    {
        if (chargeTrail == null) return;

        if (clearChargeTrailOnDashStart) chargeTrail.Clear();
        chargeTrail.Play();
    }

    private void StopChargeTrail(bool forceClear)
    {
        if (chargeTrail == null) return;

        chargeTrail.Stop(forceClear || clearChargeTrailOnDashStop);
    }

    private void ChangeState(FinalBossState newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        currentState = newState;
        Debug.Log($"[FinalBossBehavior] State changed: {previousState} -> {currentState}.");
    }

    public void SetPlayerInAggroRange(bool value)
    {
        playerInAggroRange = value;

        if (value)
        {
            ShowBossHud();
        }
        else
        {
            HideBossHud();
        }
    }

    public void SetPlayerInAttackRange(bool value)
    {
        playerInAttackRange = value;
    }

    private void ShowBossHud()
    {
        if (!showBossHudOnAggro || health == null || health.IsDead) return;

        HUDManager.Instance?.ShowFinalBossHp(health);
    }

    private void HideBossHud()
    {
        if (!showBossHudOnAggro) return;

        HUDManager.Instance?.HideFinalBossHp();
    }
}
