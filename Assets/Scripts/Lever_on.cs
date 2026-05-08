using UnityEngine;
using UnityEngine.Events;

public class Lever_On : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";

    [System.Serializable]
    public class LeverEvent : UnityEvent<bool> { }

    public LeverEvent onToggle;

    private bool inRange;
    private bool isOn;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!inRange)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            Toggle();
    }

    private void Toggle()
    {
        isOn = !isOn;

        if (anim != null)
            anim.SetBool("ON", isOn);

        onToggle?.Invoke(isOn);

        Debug.Log($"[LEVER] State: {isOn}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            inRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            inRange = false;
    }
}