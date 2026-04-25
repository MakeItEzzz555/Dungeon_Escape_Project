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
            Debug.LogWarning("[DEBUG_LOG] FollowPlayer: pauseMenu reference is missing! Please assign it in the Inspector.");
            return;
        }

        Debug.Log("[DEBUG_LOG] FollowPlayer: ESC pressed. Current menu state: " + pauseMenu.gameObject.activeSelf);

        if (pauseMenu.gameObject.activeSelf)
        {
            pauseMenu.ResumeGame();
        }
        else
        {
            pauseMenu.PauseGame();
        }
    }
}