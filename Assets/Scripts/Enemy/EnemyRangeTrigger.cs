using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider2D))]
public class EnemyRangeTrigger : MonoBehaviour
{
    public enum RangeType { Aggro, Attack }

    [SerializeField] private RangeType rangeType;
    [FormerlySerializedAs("enemyAI")]
    [SerializeField] private MonoBehaviour rangeReceiverOverride;

    private Collider2D triggerCollider;
    private IEnemyRangeReceiver rangeReceiver;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        if (rangeReceiverOverride != null)
        {
            rangeReceiver = rangeReceiverOverride as IEnemyRangeReceiver;

            if (rangeReceiver == null)
            {
                Debug.LogWarning($"[EnemyRangeTrigger] {name} override does not implement IEnemyRangeReceiver.");
            }
        }

        if (rangeReceiver == null)
            rangeReceiver = GetComponentInParent<IEnemyRangeReceiver>();

        if (rangeReceiver == null)
            Debug.LogWarning($"[EnemyRangeTrigger] {name} could not find IEnemyRangeReceiver in parent.");
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
        if (rangeReceiver == null) return;

        if (rangeType == RangeType.Aggro)
            rangeReceiver.SetPlayerInAggroRange(value);
        else
            rangeReceiver.SetPlayerInAttackRange(value);
    }
}
