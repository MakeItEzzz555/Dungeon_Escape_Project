using UnityEngine;

/// <summary>
/// Presentation-only bridge for driving the FinalBoss Animator.
/// </summary>
public class FinalBossAnimationBridge : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Movement Parameters")]
    [SerializeField] private string moveXParameter = "MoveX";
    [SerializeField] private string moveYParameter = "MoveY";
    [SerializeField] private string lastMoveXParameter = "LastMoveX";
    [SerializeField] private string lastMoveYParameter = "LastMoveY";
    [SerializeField] private string isWalkingParameter = "IsWalking";

    [Header("Action Parameters")]
    [SerializeField] private string attackParameter = "Attack";
    [SerializeField] private string chargeAttackParameter = "ChargeAttack";
    [SerializeField] private string hurtParameter = "Hurt";
    [SerializeField] private string isDeadParameter = "IsDead";
    [SerializeField] private string dieParameter = "Die";

    private int moveXHash;
    private int moveYHash;
    private int lastMoveXHash;
    private int lastMoveYHash;
    private int isWalkingHash;
    private int attackHash;
    private int chargeAttackHash;
    private int hurtHash;
    private int isDeadHash;
    private int dieHash;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        CacheParameterHashes();

        if (animator == null)
        {
            Debug.LogWarning($"[FinalBossAnimationBridge] {gameObject.name} could not find an Animator on self or children.");
        }
    }

    public void SetMovement(Vector2 direction, bool isWalking)
    {
        if (animator == null) return;

        Vector2 normalizedDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.zero;

        animator.SetFloat(moveXHash, normalizedDirection.x);
        animator.SetFloat(moveYHash, normalizedDirection.y);
        animator.SetBool(isWalkingHash, isWalking);

        if (normalizedDirection.sqrMagnitude > 0f)
        {
            SetFacingDirection(normalizedDirection);
        }
    }

    public void SetMovement(Vector2 direction)
    {
        SetMovement(direction, direction.sqrMagnitude > 0.01f);
    }

    public void SetWalking(bool isWalking)
    {
        if (animator == null) return;

        animator.SetBool(isWalkingHash, isWalking);

        if (!isWalking)
        {
            animator.SetFloat(moveXHash, 0f);
            animator.SetFloat(moveYHash, 0f);
        }
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (animator == null || direction.sqrMagnitude <= 0f) return;

        Vector2 normalizedDirection = direction.normalized;
        animator.SetFloat(lastMoveXHash, normalizedDirection.x);
        animator.SetFloat(lastMoveYHash, normalizedDirection.y);
    }

    public void TriggerRegularAttack()
    {
        animator?.SetTrigger(attackHash);
    }

    public void PlayAttack()
    {
        TriggerRegularAttack();
    }

    public void TriggerChargeAttack()
    {
        animator?.SetTrigger(chargeAttackHash);
    }

    public void PlayChargeAttack()
    {
        TriggerChargeAttack();
    }

    public void TriggerHurt()
    {
        animator?.SetTrigger(hurtHash);
    }

    public void SetDeadStatus(bool isDead)
    {
        animator?.SetBool(isDeadHash, isDead);
    }

    public void TriggerDie()
    {
        animator?.SetTrigger(dieHash);
    }

    public void ClearMovement()
    {
        if (animator == null) return;

        animator.SetFloat(moveXHash, 0f);
        animator.SetFloat(moveYHash, 0f);
        animator.SetBool(isWalkingHash, false);
    }

    private void CacheParameterHashes()
    {
        moveXHash = Animator.StringToHash(moveXParameter);
        moveYHash = Animator.StringToHash(moveYParameter);
        lastMoveXHash = Animator.StringToHash(lastMoveXParameter);
        lastMoveYHash = Animator.StringToHash(lastMoveYParameter);
        isWalkingHash = Animator.StringToHash(isWalkingParameter);
        attackHash = Animator.StringToHash(attackParameter);
        chargeAttackHash = Animator.StringToHash(chargeAttackParameter);
        hurtHash = Animator.StringToHash(hurtParameter);
        isDeadHash = Animator.StringToHash(isDeadParameter);
        dieHash = Animator.StringToHash(dieParameter);
    }
}
