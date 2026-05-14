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

        private Camera mainCamera;
        private FollowPlayer followScript;
        private Coroutine zoomCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Reinitialize();
        }

        public void Reinitialize()
        {
            mainCamera = Camera.main;

            if (mainCamera != null)
                followScript = mainCamera.GetComponent<FollowPlayer>();
        }

        public void StartZoomIn(Transform target, System.Action onComplete)
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);

            zoomCoroutine = StartCoroutine(
                ZoomRoutine(target, transitionZoom, zoomInDuration, onComplete)
            );
        }

        public void StartZoomOut(System.Action onComplete)
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);

            zoomCoroutine = StartCoroutine(ZoomOutSafe(onComplete));
        }

        private IEnumerator ZoomOutSafe(System.Action onComplete)
        {
            // WAIT FOR NEW SCENE CAMERA TO EXIST
            while (Camera.main == null)
                yield return null;

            Reinitialize();

            if (mainCamera != null)
            {
                // IMMEDIATELY FORCE transitionZoom as the starting point
                mainCamera.orthographicSize = transitionZoom;
            }

            zoomCoroutine = StartCoroutine(
                ZoomRoutine(null, gameplayZoom, zoomOutDuration, onComplete)
            );
        }

        private IEnumerator ZoomRoutine(Transform target, float targetSize, float duration, System.Action onComplete)
        {
            if (mainCamera == null)
                Reinitialize();

            if (mainCamera == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            float startSize = mainCamera.orthographicSize;
            float elapsed = 0f;

            Transform originalTarget = null;

            if (target != null && followScript != null)
            {
                originalTarget = followScript.target;
                followScript.target = target;
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

                yield return null;
            }

            mainCamera.orthographicSize = targetSize;

            zoomCoroutine = null;
            onComplete?.Invoke();
        }

        public void SetInstantZoom(float size)
        {
            Reinitialize();

            if (mainCamera != null)
                mainCamera.orthographicSize = size;
        }

        public float TransitionZoom => transitionZoom;
        public float GameplayZoom => gameplayZoom;
    }
}