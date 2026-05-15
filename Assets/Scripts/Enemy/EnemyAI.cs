using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { IdleAtHome, Chasing, Attacking, ReturningHome, Dead }

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float returnStopDistance = 0.05f;
    [SerializeField] private float attackDistanceFallback = 1.5f;

    [Header("Animator Params")]
    [SerializeField] private string walkingBool = "IsWalking";
    [SerializeField] private string attackTrigger = "Attack";

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform visuals;
    [SerializeField] private Animator animator;
    [SerializeField] private Health health;

    private Vector3 homePosition;
    private float nextAttackTime;
    private bool playerInAggroRange;
    private bool playerInAttackRange;
    private EnemyState currentState = EnemyState.IdleAtHome;
    private EnemyState previousState;
    private Rigidbody2D rb;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (health == null) health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        homePosition = transform.position;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        Debug.Log($"[EnemyAI] Awake on {name}. Player={(player ? player.name : "NULL")}, Animator={(animator ? animator.name : "NULL")}, Health={(health ? health.name : "NULL")}");
    }

    private void Update()
    {
        if (health != null && health.IsDead)
        {
            ChangeState(EnemyState.Dead);
            SetWalking(false);
            return;
        }

        UpdateState();
        ExecuteState();
    }

    private void UpdateState()
    {
        if (playerInAggroRange)
        {
            bool canAttack = playerInAttackRange || IsPlayerCloseEnoughToAttack();
            ChangeState(canAttack ? EnemyState.Attacking : EnemyState.Chasing);
        }
        else
        {
            bool awayFromHome = Vector3.Distance(transform.position, homePosition) > returnStopDistance;
            ChangeState(awayFromHome ? EnemyState.ReturningHome : EnemyState.IdleAtHome);
        }
    }

    private bool IsPlayerCloseEnoughToAttack()
    {
        if (player == null) return false;

        return Vector2.Distance(transform.position, player.position) <= attackDistanceFallback;
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        currentState = newState;

        Debug.Log($"[EnemyAI] State changed: {previousState} -> {currentState}. Aggro={playerInAggroRange}, AttackRange={playerInAttackRange}");
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case EnemyState.IdleAtHome:
                SetWalking(false);
                break;

            case EnemyState.Chasing:
                ChasePlayer();
                if (player != null) FlipVisuals(player.position.x - transform.position.x);
                break;

            case EnemyState.Attacking:
                StopAndAttack();
                if (player != null) FlipVisuals(player.position.x - transform.position.x);
                break;

            case EnemyState.ReturningHome:
                ReturnHome();
                FlipVisuals(homePosition.x - transform.position.x);
                break;

            case EnemyState.Dead:
                SetWalking(false);
                break;
        }
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        SetWalking(true);

        Vector2 nextPosition = Vector2.MoveTowards(
            rb != null ? rb.position : (Vector2)transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );

        MoveTo(nextPosition);
    }

    private void StopAndAttack()
    {
        SetWalking(false);

        if (Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void ReturnHome()
    {
        SetWalking(true);

        Vector2 currentPosition = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            homePosition,
            moveSpeed * Time.deltaTime
        );

        MoveTo(nextPosition);

        if (Vector2.Distance(nextPosition, homePosition) <= returnStopDistance)
        {
            MoveTo(homePosition);
            SetWalking(false);
        }
    }

    private void MoveTo(Vector2 position)
    {
        if (rb != null)
            rb.MovePosition(position);
        else
            transform.position = position;
    }

    private void Attack()
    {
        Debug.Log($"[EnemyAI] Attack() called");
        Debug.Log($"[EnemyAI] Attack() called on {name}. Trigger={attackTrigger}");

        if (animator == null)
        {
            Debug.LogWarning("[EnemyAI] Cannot attack because animator is null.");
            return;
        }

        animator.SetTrigger(attackTrigger);
    }

    private void SetWalking(bool value)
    {
        if (animator == null) return;

        animator.SetBool(walkingBool, value);
    }

    private void FlipVisuals(float directionX)
    {
        if (visuals == null) return;

        if (directionX < -0.01f)
            visuals.localScale = new Vector3(-1, 1, 1);
        else if (directionX > 0.01f)
            visuals.localScale = new Vector3(1, 1, 1);
    }

    public void SetPlayerInAggroRange(bool value)
    {
        playerInAggroRange = value;
        Debug.Log($"[EnemyAI] playerInAggroRange = {value}");
    }

    public void SetPlayerInAttackRange(bool value)
    {
        playerInAttackRange = value;
        Debug.Log($"[EnemyAI] playerInAttackRange = {value}");
    }

    private void OnDeath()
    {
        ChangeState(EnemyState.Dead);
        SetWalking(false);
    }
}