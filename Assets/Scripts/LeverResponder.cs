using UnityEngine;

public class LeverResponder : MonoBehaviour
{
    public string parameterName = "Open";
    public bool isTrigger = true;

    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Respond(bool signal)
    {
        if (anim == null)
        {
            Debug.LogError($"[RESPONDER] No Animator on {gameObject.name}!");
            return;
        }

        if (isTrigger)
        {
            if (signal) anim.SetTrigger(parameterName);
        }
        else
        {
            anim.SetBool(parameterName, signal);
        }
        
        Debug.Log($"[RESPONDER] Received signal {signal}. Triggering '{parameterName}' on {gameObject.name}");
    }
}
