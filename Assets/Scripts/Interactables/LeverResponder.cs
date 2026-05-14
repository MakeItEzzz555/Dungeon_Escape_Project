using UnityEngine;

public class LeverResponder : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Animation Control")]
    [SerializeField] private string parameterName = "Open";
    [SerializeField] private bool isTrigger = true;
    [SerializeField] private bool useTwoTriggers = false;
    [SerializeField] private string offParameterName = "Close";

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
            if (useTwoTriggers)
            {
                // NEW: If we have an InteractiveGate, use its specialized methods
                InteractiveGate gate = GetComponent<InteractiveGate>();
                if (gate != null)
                {
                    gate.SetState(signal);
                }
                else
                {
                    string targetTrigger = signal ? parameterName : offParameterName;
                    string otherTrigger = signal ? offParameterName : parameterName;
                    
                    animator.ResetTrigger(otherTrigger);
                    animator.SetTrigger(targetTrigger);
                }
                
                Debug.Log($"[LeverResponder] {gameObject.name} handled trigger (Signal: {signal})");
            }
            else if (signal)
            {
                animator.SetTrigger(parameterName);
                Debug.Log($"[LeverResponder] {gameObject.name} set Trigger: {parameterName}");
            }
        }
        else
        {
            InteractiveGate gate = GetComponent<InteractiveGate>();
            if (gate != null)
            {
                gate.SetState(signal);
            }
            else
            {
                animator.SetBool(parameterName, signal);
                if (useTwoTriggers)
                {
                    animator.SetBool(offParameterName, !signal);
                }
            }
            Debug.Log($"[LeverResponder] {gameObject.name} set Bool/State (Signal: {signal})");
        }
    }
}