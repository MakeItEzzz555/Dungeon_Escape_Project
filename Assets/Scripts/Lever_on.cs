using UnityEngine;
using UnityEngine.Events;

public class Lever_on : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";
    public UnityEvent<bool> onToggle;

    private bool inRange = false;
    private bool isOn = false;
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            isOn = !isOn;
            
            // 1. Animate the lever itself
            if (anim != null) anim.SetBool("ON", isOn);
            
            // 2. Send signal to others
            onToggle?.Invoke(isOn);
            
            Debug.Log($"[LEVER] State: {isOn}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) inRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) inRange = false;
    }
}
