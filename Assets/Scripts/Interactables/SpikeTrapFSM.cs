using UnityEngine;

public class SpikeTrapFSM : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    // STATE MACHINE
    // ─────────────────────────────────────────────────────────────

    private enum TrapState
    {
        Disabled,
        Armed,
        PlayerDetected,
        Active,
        Triggered,
        CoolingDown
    }

    [Header("Startup")]
    [SerializeField] private bool startEnabled = true;
    [SerializeField] private int trapDamageAmount = 5;

    private TrapState state;

    // ─────────────────────────────────────────────────────────────
    // REFERENCES
    // ─────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private Animator animator;
    // ─────────────────────────────────────────────────────────────
    // INTERNAL
    // ─────────────────────────────────────────────────────────────

    private PlayerController detectedPlayer;

    private bool trapEnabled;
    private bool damageWindowOpen;
    private bool damageAppliedThisWindow;

    // ─────────────────────────────────────────────────────────────
    // UNITY
    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogError("[SpikeTrapFSM] Missing Animator.");

        trapEnabled = startEnabled;

        state = trapEnabled
            ? TrapState.Armed
            : TrapState.Disabled;

        if (animator != null)
        {
            animator.speed = trapEnabled ? 1f : 0f;
        }
    }

    private void Update()
    {
        if (!trapEnabled)
            return;

        if (damageWindowOpen && detectedPlayer != null)
        {
            ApplyTrapDamage();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // LEVER CONTROL
    // ─────────────────────────────────────────────────────────────

    public void SetTrapActive(bool isOn)
    {
        trapEnabled = isOn;

        if (!trapEnabled)
        {
            state = TrapState.Disabled;

            detectedPlayer = null;
            damageWindowOpen = false;

            if (animator != null)
            {
                animator.speed = 0f;
            }

            Debug.Log("[SpikeTrapFSM] DISABLED");
            return;
        }

        state = TrapState.Armed;

        if (animator != null)
        {
            animator.speed = 1f;
        }

        Debug.Log("[SpikeTrapFSM] ENABLED");
    }

    // ─────────────────────────────────────────────────────────────
    // PLAYER DETECTION
    // ─────────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!trapEnabled)
            return;

        if (!other.CompareTag("Player"))
            return;

        detectedPlayer = other.GetComponent<PlayerController>();
        if (detectedPlayer == null) detectedPlayer = other.GetComponentInParent<PlayerController>();

        if (detectedPlayer == null)
            return;

        state = TrapState.PlayerDetected;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerController exitingPlayer = other.GetComponent<PlayerController>();
        if (exitingPlayer == null) exitingPlayer = other.GetComponentInParent<PlayerController>();

        if (detectedPlayer != null && exitingPlayer == detectedPlayer)
        {
            detectedPlayer = null;
        }

        if (trapEnabled &&
            state != TrapState.Triggered &&
            state != TrapState.CoolingDown)
        {
            state = TrapState.Armed;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // ANIMATION EVENTS
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Animation Event:
    /// Called when spikes are visually fully extended.
    /// </summary>
    public void EnableDamageWindow()
    {
        if (!trapEnabled)
            return;

        if (state == TrapState.CoolingDown)
            return;

        damageWindowOpen = true;
        damageAppliedThisWindow = false;
        state = TrapState.Active;

        Debug.Log("[SpikeTrapFSM] DAMAGE WINDOW OPEN");
    }

    /// <summary>
    /// Animation Event:
    /// Called when spikes are retracting / safe.
    /// </summary>
    public void DisableDamageWindow()
    {
        damageWindowOpen = false;
        damageAppliedThisWindow = false;

        if (trapEnabled &&
            state != TrapState.CoolingDown)
        {
            state = detectedPlayer != null
                ? TrapState.PlayerDetected
                : TrapState.Armed;
        }

        Debug.Log("[SpikeTrapFSM] DAMAGE WINDOW CLOSED");
    }

    // ─────────────────────────────────────────────────────────────
    // DEATH
    // ─────────────────────────────────────────────────────────────

    private void ApplyTrapDamage()
    {
        if (damageAppliedThisWindow)
            return;

        if (detectedPlayer == null)
            return;

        if (detectedPlayer.IsDead || detectedPlayer.IsFalling)
            return;

        Health health = detectedPlayer.GetComponent<Health>();
        if (health == null) health = detectedPlayer.GetComponentInParent<Health>();

        if (health == null)
        {
            Debug.LogWarning("[SpikeTrapFSM] Player Health missing. Falling back to death sequence.");
            damageAppliedThisWindow = true;
            detectedPlayer.StartDeathSequence();
            return;
        }

        damageAppliedThisWindow = true;
        state = TrapState.Triggered;

        Debug.Log($"[SpikeTrapFSM] Player damaged for {trapDamageAmount}.");

        health.TakeDamage(trapDamageAmount, transform);
    }

    // ─────────────────────────────────────────────────────────────
    // RESET
    // ─────────────────────────────────────────────────────────────

    public void ResetTrap()
    {
        trapEnabled = startEnabled;

        state = trapEnabled
            ? TrapState.Armed
            : TrapState.Disabled;

        detectedPlayer = null;

        damageWindowOpen = false;
        damageAppliedThisWindow = false;

        if (animator != null)
        {
            animator.speed = trapEnabled ? 1f : 0f;
        }

        Time.timeScale = 1f;
    }
}
