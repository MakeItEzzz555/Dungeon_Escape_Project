using System.Collections;
using UnityEngine;

public class SpikeTrapFSM : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // STATE
    // ─────────────────────────────────────────────

    private enum TrapState
    {
        Disabled,
        Armed,
        PlayerDetected,
        Active,
        Triggered,
        CoolingDown
    }

    private TrapState state = TrapState.Disabled;

    // ─────────────────────────────────────────────
    // REFERENCES
    // ─────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerAnimator playerAnimator;

    private PlayerController detectedPlayer;

    private bool trapEnabled = false;
    private bool damageWindowOpen = false;
    private bool deathRunning = false;

    // ─────────────────────────────────────────────
    // UNITY
    // ─────────────────────────────────────────────

    private void Awake()
    {
        animator ??= GetComponent<Animator>();

        if (animator == null)
            Debug.LogError("[SpikeTrapFSM] Missing Animator.");
    }

    private void Update()
    {
        if (deathRunning || !trapEnabled)
            return;

        if (state == TrapState.Active &&
            damageWindowOpen &&
            detectedPlayer != null)
        {
            TriggerDeath();
        }
    }

    // ─────────────────────────────────────────────
    // LEVER CONTROL
    // ─────────────────────────────────────────────

    public void SetTrapActive(bool isOn)
    {
        trapEnabled = isOn;

        if (!isOn)
        {
            state = TrapState.Disabled;
            detectedPlayer = null;
            damageWindowOpen = false;

            if (animator != null)
                animator.speed = 0f;

            return;
        }

        state = TrapState.Armed;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetBool("Active", true);
        }
    }

    // ─────────────────────────────────────────────
    // TRIGGERS
    // ─────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        detectedPlayer = other.GetComponent<PlayerController>();

        if (trapEnabled && state == TrapState.Armed)
        {
            state = TrapState.PlayerDetected;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        detectedPlayer = null;

        if (trapEnabled && state != TrapState.Disabled)
        {
            state = TrapState.Armed;
        }
    }

    // ─────────────────────────────────────────────
    // ANIMATION EVENTS (CRITICAL)
    // ─────────────────────────────────────────────

    public void EnableDamageWindow()
    {
        if (!trapEnabled) return;

        damageWindowOpen = true;
        state = TrapState.Active;
    }

    public void DisableDamageWindow()
    {
        damageWindowOpen = false;

        if (state == TrapState.Active)
            state = TrapState.Armed;
    }

    // ─────────────────────────────────────────────
    // DEATH
    // ─────────────────────────────────────────────

    private void TriggerDeath()
    {
        if (state == TrapState.Triggered || detectedPlayer == null)
            return;

        state = TrapState.Triggered;
        deathRunning = true;

        detectedPlayer.Die();

        playerAnimator?.TriggerDie();

        StartCoroutine(DeathSequence());
    }

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

        Time.timeScale = 0f;
    }

    public void ResetTrap()
    {
        state = TrapState.Disabled;

        trapEnabled = false;
        detectedPlayer = null;
        damageWindowOpen = false;
        deathRunning = false;

        if (animator != null)
            animator.speed = 1f;

        Time.timeScale = 1f;
    }
}