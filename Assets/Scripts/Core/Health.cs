using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Animations")]
    [SerializeField] private string hurtTrigger = "Hurt";
    [SerializeField] private string hurtXParam = "HurtX";
    [SerializeField] private string hurtYParam = "HurtY";
    [SerializeField] private string isDeadParam = "IsDead";
    [SerializeField] private string deathTrigger = "Die";

    private Animator animator;
    private bool isDead = false;

    public bool IsDead => isDead;
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
        NotifyHealthChanged();
    }

    public void TakeDamage(int amount, Transform attacker)
    {
        if (isDead) return;
        if (amount <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        Debug.Log($"[DEBUG_LOG] {gameObject.name} took {amount} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            NotifyHealthChanged();
            TriggerHurt(attacker);
        }
    }

    public void DepleteHealth()
    {
        if (isDead && currentHealth <= 0)
        {
            NotifyHealthChanged();
            return;
        }

        isDead = true;
        currentHealth = 0;
        Debug.Log($"[DEBUG_LOG] {gameObject.name} HP depleted.");
        NotifyHealthChanged();
    }

    private void TriggerHurt(Transform attacker)
    {
        if (animator == null) return;

        if (attacker != null)
        {
            Vector2 direction = (transform.position - attacker.position).normalized;

            // Convert to cardinal direction
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                direction = direction.x > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                direction = direction.y > 0 ? Vector2.up : Vector2.down;
            }
            
            // Some animators might use these parameters for directional hurt animations
            SetAnimatorFloatIfExists(hurtXParam, direction.x);
            SetAnimatorFloatIfExists(hurtYParam, direction.y);
        }

        animator.SetTrigger(hurtTrigger);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        currentHealth = 0;
        NotifyHealthChanged();

        Debug.Log($"[DEBUG_LOG] {gameObject.name} has died.");

        if (animator != null)
        {
            SetAnimatorBoolIfExists(isDeadParam, true);
            SetAnimatorTriggerIfExists(deathTrigger);
        }

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController == null) playerController = GetComponentInParent<PlayerController>();

        if (playerController != null)
        {
            playerController.StartDeathSequence();
            return;
        }

        SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void SetAnimatorFloatIfExists(string paramName, float value)
    {
        if (string.IsNullOrEmpty(paramName)) return;
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName)
            {
                animator.SetFloat(paramName, value);
                return;
            }
        }
    }

    private void SetAnimatorBoolIfExists(string paramName, bool value)
    {
        if (string.IsNullOrEmpty(paramName)) return;
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName)
            {
                animator.SetBool(paramName, value);
                return;
            }
        }
    }

    private void SetAnimatorTriggerIfExists(string paramName)
    {
        if (string.IsNullOrEmpty(paramName)) return;
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(paramName);
                return;
            }
        }
    }
}
