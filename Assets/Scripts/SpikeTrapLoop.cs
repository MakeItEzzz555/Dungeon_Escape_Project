using System.Collections;
using UnityEngine;

/// <summary>
/// Production-safe spike trap FSM (event-driven version).
/// Animation events define the damage window.
/// No frame math. No timing guessing.
/// </summary>
public class SpikeTrapFSM : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    // STATE MACHINE
    // ─────────────────────────────────────────────────────────────

    private enum TrapState
    {
        Idle,
        Armed,
        PlayerDetected,
        Active,        // damage window OPEN (controlled by animation event)
        Triggered,
        CoolingDown
    }

    private TrapState state = TrapState.Armed;

    // ─────────────────────────────────────────────────────────────
    // REFERENCES
    // ─────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerAnimator playerAnimator;

    private PlayerController detectedPlayer;
    private bool deathRunning = false;

    // ─────────────────────────────────────────────────────────────
    // UNITY
    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogError("[SpikeTrapFSM] Missing Animator on spike trap.");
    }

    private void Update()
    {
        if (deathRunning)
            return;

        // Only kill when:
        // 1. trap is in ACTIVE window (animation says spikes are up)
        // 2. player is inside trigger
        if (state == TrapState.Active && detectedPlayer != null)
        {
            TriggerDeath();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // COLLISIONS
    // ─────────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        detectedPlayer = other.GetComponent<PlayerController>();

        if (state == TrapState.Armed)
        {
            state = TrapState.PlayerDetected;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        detectedPlayer = null;

        if (state == TrapState.PlayerDetected || state == TrapState.Active)
        {
            state = TrapState.Armed;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 🎯 ANIMATION EVENTS (THIS IS THE CORE SYSTEM NOW)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by animation event when spikes are FULLY UP.
    /// This opens the damage window.
    /// </summary>
    public void EnableDamageWindow()
    {
        if (state != TrapState.PlayerDetected && state != TrapState.Armed)
            return;

        state = TrapState.Active;
    }

    /// <summary>
    /// Called by animation event when spikes are no longer dangerous.
    /// </summary>
    public void DisableDamageWindow()
    {
        if (state == TrapState.Active)
        {
            state = TrapState.Armed;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // DEATH LOGIC
    // ─────────────────────────────────────────────────────────────

    private void TriggerDeath()
    {
        if (state == TrapState.Triggered || detectedPlayer == null)
            return;

        state = TrapState.Triggered;
        deathRunning = true;

        Debug.Log("[SpikeTrapFSM] Player killed by spike trap.");

        detectedPlayer.Die();

        if (playerAnimator != null)
            playerAnimator.TriggerDie();

        StartCoroutine(DeathSequence());
    }

    // ─────────────────────────────────────────────────────────────
    // DEATH SEQUENCE
    // ─────────────────────────────────────────────────────────────

    private IEnumerator DeathSequence()
    {
        yield return null;

        Animator anim = playerAnimator != null
            ? playerAnimator.GetComponent<Animator>()
            : null;

        if (anim != null)
        {
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Die"))
                yield return null;

            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
        }

        if (animator != null)
            animator.speed = 0f;

        state = TrapState.CoolingDown;

        Debug.Log("[SpikeTrapFSM] Trap frozen.");

        Time.timeScale = 0f;
    }

    // ─────────────────────────────────────────────────────────────
    // RESET
    // ─────────────────────────────────────────────────────────────

    public void ResetTrap()
    {
        state = TrapState.Armed;
        detectedPlayer = null;
        deathRunning = false;

        if (animator != null)
            animator.speed = 1f;

        Time.timeScale = 1f;
    }
}