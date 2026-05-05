using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10f);
    public float smoothSpeed = 8f;
    public bool snapOnStart = true;

    [Header("Zoom Settings")]
    [Tooltip("The size of the camera view. Larger values = zoomed out, Smaller values = zoomed in.")]
    public float targetZoom = 5f;
    private Camera cam;

    [Header("Menu Settings")]
    public MainMenu pauseMenu;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographicSize = targetZoom;
        }

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("FollowPlayer: No target assigned and no GameObject with tag 'Player' found.");
            }
        }

        if (snapOnStart && target != null)
        {
            transform.position = target.position + offset;
        }

        // Automatically hide the pause menu at the start of the level
        if (pauseMenu != null)
        {
            pauseMenu.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

    }

    private void TogglePauseMenu()
    {
        if (pauseMenu == null)
        {
            Debug.LogWarning("[DEBUG_LOG] FollowPlayer: pauseMenu reference is missing! Attempting to find it in the scene...");
            pauseMenu = Object.FindAnyObjectByType<MainMenu>();
            if (pauseMenu == null)
            {
                Debug.LogError("[DEBUG_LOG] FollowPlayer: Could not find MainMenu script in the scene!");
                return;
            }
        }

        Debug.Log("[DEBUG_LOG] FollowPlayer: ESC pressed. Current menu state (gameObject.activeSelf): " + pauseMenu.gameObject.activeSelf);

        // Check both GameObject state and Canvas component if possible
        Canvas canvas = pauseMenu.GetComponent<Canvas>();
        bool isMenuShowing = pauseMenu.gameObject.activeSelf;
        if (canvas != null) isMenuShowing = isMenuShowing && canvas.enabled;

        if (isMenuShowing)
        {
            Debug.Log("[DEBUG_LOG] FollowPlayer: Resuming game...");
            pauseMenu.ResumeGame();
        }
        else
        {
            Debug.Log("[DEBUG_LOG] FollowPlayer: Pausing game...");
            // Ensure any canvas component is enabled before calling PauseGame
            if (canvas != null) canvas.enabled = true;
            pauseMenu.PauseGame();
        }
    }
}