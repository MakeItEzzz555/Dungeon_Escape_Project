using UnityEngine;

public class LeverResponder : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private MonoBehaviour target;

    [Header("Animator (optional fallback)")]
    [SerializeField] private Animator animator;

    [Header("Animation Control")]
    public string parameterName = "Open";
    public bool isTrigger = true;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void Respond(bool signal)
    {
        // ─────────────────────────────
        // 1. PRIORITY: Spike Trap FSM
        // ─────────────────────────────
        if (target is SpikeTrapFSM trap)
        {
            trap.SetTrapActive(signal);
            return;
        }

        // ─────────────────────────────
        // 2. FALLBACK: Animator toggle
        // ─────────────────────────────
        if (animator == null)
            return;

        if (isTrigger)
        {
            if (signal)
                animator.SetTrigger(parameterName);
        }
        else
        {
            animator.SetBool(parameterName, signal);
        }
    }
}