using UnityEngine;

public class open_chest : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";
    public string openAnimationTrigger = "Open";

    [Header("Chest Content")]
    [Tooltip("Item that will be enabled when the chest opens")]
    public GameObject itemInside;

    private Animator animator;
    private bool isPlayerInRange = false;
    private bool isOpened = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Ensure the item is hidden initially if assigned
        if (itemInside != null)
        {
            itemInside.SetActive(false);
        }
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

        // Enable the item inside the chest
        if (itemInside != null)
        {
            itemInside.SetActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayInteract();
        }

        HUDManager.Instance?.ShowUXMessage("Chest Opened");

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
