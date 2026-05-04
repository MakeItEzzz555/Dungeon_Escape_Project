using UnityEngine;

public class auto_gate : MonoBehaviour
{
    [Header("Settings")]
    public Collider2D physicalCollider; // The collider that blocks the player
    
    // You can call these from Animation Events
    // or the script can try to sync with the Animator
    
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Call this from an Animation Event at the frame the gate is high enough to pass.
    /// </summary>
    public void DisableCollider()
    {
        if (physicalCollider != null)
        {
            physicalCollider.enabled = false;
            Debug.Log($"[Gate] {gameObject.name} collider DISABLED (Open)");
        }
    }

    /// <summary>
    /// Call this from an Animation Event at the frame the gate hits the floor.
    /// </summary>
    public void EnableCollider()
    {
        if (physicalCollider != null)
        {
            physicalCollider.enabled = true;
            Debug.Log($"[Gate] {gameObject.name} collider ENABLED (Closed)");
        }
    }
}
