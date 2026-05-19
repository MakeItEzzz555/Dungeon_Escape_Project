using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Managers
{
    public class CameraTransitionSystem : MonoBehaviour
    {
        public static CameraTransitionSystem Instance { get; private set; }

        [Header("Zoom Settings")]
        [SerializeField] private float gameplayZoom = 5f;
        [SerializeField] private float transitionZoom = 2f;
        [SerializeField] private float zoomInDuration = 0.5f;
        [SerializeField] private float zoomOutDuration = 0.8f;

        private Camera cam;
        private FollowPlayer followScript;
        private Coroutine zoomCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.Log($"[DEBUG_LOG] CameraTransitionSystem: Duplicate detected on {gameObject.name}, destroying.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            cam = GetComponent<Camera>();
            followScript = GetComponent<FollowPlayer>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // AudioListener cleanup logic
            AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (listeners.Length > 1)
            {
                foreach (var listener in listeners)
                {
                    if (listener.gameObject != gameObject)
                    {
                        Debug.Log($"[DEBUG_LOG] CameraTransitionSystem: Destroying duplicate AudioListener on {listener.gameObject.name}");
                        Destroy(listener);
                    }
                }
            }
        }

        public void StartZoomIn(Transform target, System.Action onComplete)
        {
            Debug.Log("[DEBUG_LOG] CameraTransitionSystem: StartZoomIn called");
            
            // Critical fix: Always capture the previous onComplete if we are stopping a coroutine
            // Actually, we should probably just invoke it before starting the next one if it exists
            // But usually, one transition cancels the other.
            
            if (zoomCoroutine != null)
            {
                Debug.Log("[DEBUG_LOG] CameraTransitionSystem: Stopping existing zoom coroutine.");
                StopCoroutine(zoomCoroutine);
                zoomCoroutine = null;
            }

            zoomCoroutine = StartCoroutine(
                ZoomRoutine(target, transitionZoom, zoomInDuration, onComplete)
            );
        }

        public void StartZoomOut(System.Action onComplete)
        {
            Debug.Log("[DEBUG_LOG] CameraTransitionSystem: StartZoomOut called");
            if (zoomCoroutine != null)
            {
                Debug.Log("[DEBUG_LOG] CameraTransitionSystem: Stopping existing zoom coroutine.");
                StopCoroutine(zoomCoroutine);
                zoomCoroutine = null;
            }

            zoomCoroutine = StartCoroutine(ZoomOutSafe(onComplete));
        }

        private IEnumerator ZoomOutSafe(System.Action onComplete)
        {
            if (cam != null)
            {
                cam.orthographicSize = transitionZoom;
            }

            yield return ZoomRoutine(null, gameplayZoom, zoomOutDuration, onComplete);
        }

        private IEnumerator ZoomRoutine(Transform target, float targetSize, float duration, System.Action onComplete)
        {
            Debug.Log($"[DEBUG_LOG] CameraTransitionSystem: ZoomRoutine started. TargetSize: {targetSize}, Duration: {duration}");
            if (cam == null)
            {
                Debug.LogWarning("[DEBUG_LOG] CameraTransitionSystem: Camera component missing!");
                onComplete?.Invoke();
                yield break;
            }

            float startSize = cam.orthographicSize;
            float elapsed = 0f;

            Transform originalTarget = null;

            if (target != null && followScript != null)
            {
                Debug.Log($"[DEBUG_LOG] CameraTransitionSystem: Switching follow target to {target.name}");
                originalTarget = followScript.target;
                followScript.target = target;
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

                yield return null;
            }

            cam.orthographicSize = targetSize;
            if (target != null && followScript != null)
            {
                followScript.target = originalTarget;
            }

            Debug.Log("[DEBUG_LOG] CameraTransitionSystem: ZoomRoutine finished. Invoking onComplete.");
            zoomCoroutine = null;
            onComplete?.Invoke();
        }

        public void SetInstantZoom(float size)
        {
            if (cam != null)
                cam.orthographicSize = size;
        }

        public float TransitionZoom => transitionZoom;
        public float GameplayZoom => gameplayZoom;
    }
}
