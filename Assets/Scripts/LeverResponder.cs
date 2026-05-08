using UnityEngine;

public class LeverResponder : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Animation Control")]
    [SerializeField] private string parameterName = "Open";
    [SerializeField] private bool isTrigger = true;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError($"[LeverResponder] Missing Animator on {gameObject.name}");
        }
    }

    public void Respond(bool signal)
    {
        if (animator == null)
            return;

        if (isTrigger)
        {
            if (signal)
            {
                animator.SetTrigger(parameterName);
            }
        }
        else
        {
            animator.SetBool(parameterName, signal);
        }

        Debug.Log($"[LeverResponder] {gameObject.name} received signal: {signal}");
    }
}