using UnityEngine;

public class door_interaction : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";
    public string openTrigger = "Open";
    public string closeTrigger = "Close";
    
    [Header("Components")]
    public Collider2D physicalCollider; // The collider that blocks movement

    private Animator animator;
    private bool isPlayerInRange = false;
    private bool isOpened = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Toggle door state when E is pressed and player is in range
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isOpened)
            {
                OpenDoor();
            }
            else
            {
                CloseDoor();
            }
        }
    }

    private void OpenDoor()
    {
        isOpened = true;
        if (animator != null)
        {
            animator.SetTrigger(openTrigger);
        }

        // Disable physical collision so player can pass
        if (physicalCollider != null)
        {
            physicalCollider.enabled = false;
        }

        Debug.Log("Door Opened!");
    }

    private void CloseDoor()
    {
        isOpened = false;
        if (animator != null)
        {
            animator.SetTrigger(closeTrigger);
        }

        // Enable physical collision to block the way
        if (physicalCollider != null)
        {
            physicalCollider.enabled = true;
        }

        Debug.Log("Door Closed!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
        }
    }
}
