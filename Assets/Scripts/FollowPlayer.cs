using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10f);
    public float smoothSpeed = 8f;
    public bool snapOnStart = true;

    private Camera cam;
    private static FollowPlayer _instance;

    [Header("Menu Settings")]
    public MainMenu pauseMenu;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.Log($"[DEBUG_LOG] FollowPlayer: Duplicate detected on {gameObject.name}, destroying.");
            Destroy(gameObject);
            return;
        }

        _instance = this;
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            SetTarget(playerObj.transform);
        }
        else
        {
            target = null;
        }
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }

        if (snapOnStart && target != null)
        {
            transform.position = target.position + offset;
        }

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

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null && snapOnStart)
        {
            transform.position = target.position + offset;
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