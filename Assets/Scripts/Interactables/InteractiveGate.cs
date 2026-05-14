using UnityEngine;

/// <summary>
/// A robust gate controller that handles animation states and physical colliders.
/// Works with LeverResponder and can be used with Animation Events.
/// </summary>
public class InteractiveGate : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("The physical collider that blocks the player.")]
    [SerializeField] private Collider2D physicalCollider;
    
    [Tooltip("The animator controlling the gate.")]
    [SerializeField] private Animator animator;

    [Header("Animation Parameters")]
    [Tooltip("The name of the parameter to open the gate (Trigger or Bool).")]
    [SerializeField] private string openParameter = "Open";
    
    [Tooltip("The name of the parameter to close the gate (Trigger or Bool).")]
    [SerializeField] private string closeParameter = "Close";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (physicalCollider == null)
            physicalCollider = GetComponent<Collider2D>();
    }

    // --- Interaction Methods (Called by LeverResponder or Scripts) ---

    /// <summary>
    /// Starts the opening process. Sets Open parameter and resets Close parameter.
    /// </summary>
    public void Open()
    {
        if (animator == null)
        {
            DisableCollider();
            return;
        }

        // Set Open state
        SetParam(openParameter, true);
        
        // Ensure Close state is cleared to prevent looping/conflict
        SetParam(closeParameter, false);
        
        HUDManager.Instance?.ShowUXMessage("Gate Opened");

        Debug.Log($"[InteractiveGate] {gameObject.name} -> Open() called. Parameter '{openParameter}' set, '{closeParameter}' reset.");
    }

    /// <summary>
    /// Starts the closing process. Sets Close parameter and resets Open parameter.
    /// </summary>
    public void Close()
    {
        if (animator == null)
        {
            EnableCollider();
            return;
        }

        // Set Close state
        SetParam(closeParameter, true);
        
        // Ensure Open state is cleared to prevent looping/conflict
        SetParam(openParameter, false);
        
        HUDManager.Instance?.ShowUXMessage("Gate Closed");

        Debug.Log($"[InteractiveGate] {gameObject.name} -> Close() called. Parameter '{closeParameter}' set, '{openParameter}' reset.");
    }

    /// <summary>
    /// Convenience method for toggling state via a single boolean.
    /// </summary>
    public void SetState(bool isOpen)
    {
        if (isOpen) Open();
        else Close();
    }

    // --- Animation Event Callbacks (Called by Animator) ---

    /// <summary>
    /// Call this from an Animation Event when the gate is open/high enough to pass.
    /// </summary>
    public void DisableCollider()
    {
        if (physicalCollider != null)
        {
            physicalCollider.enabled = false;
            Debug.Log($"[Gate] {gameObject.name} collider DISABLED (Open)");
        }
        
        // Optionally reset the parameter at the end of animation to prevent looping if it's a trigger
        if (animator != null)
        {
            animator.ResetTrigger(openParameter);
        }
    }

    /// <summary>
    /// Call this from an Animation Event when the gate is closed/hits the floor.
    /// </summary>
    public void EnableCollider()
    {
        if (physicalCollider != null)
        {
            physicalCollider.enabled = true;
            Debug.Log($"[Gate] {gameObject.name} collider ENABLED (Closed)");
        }
        
        // Optionally reset the parameter at the end of animation to prevent looping if it's a trigger
        if (animator != null)
        {
            animator.ResetTrigger(closeParameter);
        }
    }

    // --- Private Helpers ---

    /// <summary>
    /// Robustly sets a parameter regardless of whether it is a Trigger or a Bool.
    /// </summary>
    private void SetParam(string paramName, bool value)
    {
        if (animator == null || string.IsNullOrEmpty(paramName)) return;

        // Check the type of the parameter
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == paramName)
            {
                if (p.type == AnimatorControllerParameterType.Trigger)
                {
                    if (value) animator.SetTrigger(paramName);
                    else animator.ResetTrigger(paramName);
                }
                else if (p.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(paramName, value);
                }
                return;
            }
        }
    }
}
