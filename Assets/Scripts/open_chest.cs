using UnityEngine;

public class open_chest : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";
    public string openAnimationTrigger = "Open";

    private Animator animator;
    private bool isPlayerInRange = false;
    private bool isOpened = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Check if player is in range, chest is not already opened, and E key is pressed
        if (isPlayerInRange && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        isOpened = true;
        
        if (animator != null)
        {
            animator.SetTrigger(openAnimationTrigger);
        }
        else
        {
            Debug.LogWarning($"[open_chest] Animator missing on {gameObject.name}");
        }

        Debug.Log("Chest Opened!");
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
