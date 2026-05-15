using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyRangeTrigger : MonoBehaviour
{
    public enum RangeType { Aggro, Attack }

    [SerializeField] private RangeType rangeType;
    [SerializeField] private EnemyAI enemyAI;

    private Collider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        if (enemyAI == null)
            enemyAI = GetComponentInParent<EnemyAI>();

        if (enemyAI == null)
            Debug.LogWarning($"[EnemyRangeTrigger] {name} could not find EnemyAI in parent.");
    }

    private void OnEnable()
    {
        Debug.Log($"[EnemyRangeTrigger] Enabled {name}. Type={rangeType}, ColliderEnabled={triggerCollider.enabled}, IsTrigger={triggerCollider.isTrigger}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[EnemyRangeTrigger] ENTER {rangeType}: {other.name}, tag={other.tag}");

        if (!IsPlayer(other)) return;
        SetRange(true);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log($"[EnemyRangeTrigger] STAY {rangeType}: {other.name}, tag={other.tag}");

        if (!IsPlayer(other)) return;
        SetRange(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[EnemyRangeTrigger] EXIT {rangeType}: {other.name}, tag={other.tag}");

        if (!IsPlayer(other)) return;
        SetRange(false);
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player")
            || other.transform.root.CompareTag("Player")
            || other.GetComponentInParent<PlayerController>() != null;
    }

    private void SetRange(bool value)
    {
        if (enemyAI == null) return;

        if (rangeType == RangeType.Aggro)
            enemyAI.SetPlayerInAggroRange(value);
        else
            enemyAI.SetPlayerInAttackRange(value);
    }
}