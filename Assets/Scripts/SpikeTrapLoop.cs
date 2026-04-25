using UnityEngine;

/// <summary>
/// A spike trap animated by an Animator that kills the player during specific frames.
/// Frames 4-13 are deadly; others (1-3 and the end) are safe.
/// </summary>
public class SpikeTrapLoop : MonoBehaviour
{
    [Header("Animator Settings")]
    [Tooltip("The Animator controlling the trap animation.")]
    public Animator animator;

    [Tooltip("The name of the animation state or clip to check for frames.")]
    public string animationStateName = "SpikeTrap_Anim";

    [Header("Deadly Frames (1-indexed)")]
    public int startDeadlyFrame = 4;
    public int endDeadlyFrame = 13;

    private bool playerIsInside = false;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError($"SpikeTrapLoop on {gameObject.name} has no Animator assigned!");
        }
    }

    private void Update()
    {
        if (playerIsInside && IsTrapDeadly())
        {
            KillPlayer();
        }
    }

    private bool IsTrapDeadly()
    {
        if (animator == null) return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // Ensure we are in the correct state
        if (!stateInfo.IsName(animationStateName)) return false;

        // Get current clip info to find total frames
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length == 0) return false;

        AnimationClip clip = clipInfo[0].clip;
        float totalFrames = clip.frameRate * clip.length;
        
        // Calculate current frame (1-indexed based on user description)
        // stateInfo.normalizedTime % 1 gives the progress within the loop [0, 1]
        float currentNormalizedTime = stateInfo.normalizedTime % 1f;
        int currentFrame = Mathf.FloorToInt(currentNormalizedTime * totalFrames) + 1;

        return currentFrame >= startDeadlyFrame && currentFrame <= endDeadlyFrame;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInside = true;
            if (IsTrapDeadly())
            {
                KillPlayer();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInside = false;
        }
    }

    private void KillPlayer()
    {
        Debug.Log("PLAYER KILLED BY SPIKE TRAP");
        
        // Stop the game
        Time.timeScale = 0f;
        
        // Additional game-over logic can be added here
    }
}
