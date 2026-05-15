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
    private bool deathRunning;

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
        if (deathRunning || !trapEnabled)
            return;

        if (damageWindowOpen && detectedPlayer != null)
        {
            TriggerDeath();
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

        if (state == TrapState.Triggered ||
            state == TrapState.CoolingDown)
            return;

        damageWindowOpen = true;
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

        if (trapEnabled &&
            state != TrapState.Triggered &&
            state != TrapState.CoolingDown)
        {
            state = TrapState.Armed;
        }

        Debug.Log("[SpikeTrapFSM] DAMAGE WINDOW CLOSED");
    }

    // ─────────────────────────────────────────────────────────────
    // DEATH
    // ─────────────────────────────────────────────────────────────

    private void TriggerDeath()
    {
        if (deathRunning)
            return;

        if (detectedPlayer == null)
            return;

        if (detectedPlayer.IsDead)
            return;

        deathRunning = true;
        state = TrapState.Triggered;

        Debug.Log("[SpikeTrapFSM] Player killed.");

        detectedPlayer.StartDeathSequence();
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
        deathRunning = false;

        if (animator != null)
        {
            animator.speed = trapEnabled ? 1f : 0f;
        }

        Time.timeScale = 1f;
    }
}
