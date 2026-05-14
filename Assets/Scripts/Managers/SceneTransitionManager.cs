using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Managers
{
    public enum TransitionState
    {
        None,
        ZoomingIn,
        FadingToBlack,
        LoadingScene,
        FadingFromBlack,
        ZoomingOut
    }
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        private TransitionState currentState = TransitionState.None;

        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.8f;

        public bool IsTransitioning => currentState != TransitionState.None;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.Log($"[DEBUG_LOG] SceneTransitionManager: Duplicate detected on {gameObject.name}, destroying.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void BeginTransition(string targetScene, Transform zoomTarget)
        {
            if (currentState != TransitionState.None) return;

            StartCoroutine(TransitionRoutine(targetScene, zoomTarget));
        }

        private IEnumerator TransitionRoutine(string targetScene, Transform zoomTarget)
        {
            Debug.Log($"[DEBUG_LOG] SceneTransitionManager: BeginTransition to {targetScene}");
            currentState = TransitionState.ZoomingIn;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: Disabling player control");
            playerObj?.GetComponent<PlayerController>()?.SetControlEnabled(false);

            bool zoomInDone = false;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: Calling StartZoomIn");
            if (CameraTransitionSystem.Instance != null)
            {
                CameraTransitionSystem.Instance.StartZoomIn(zoomTarget, () => {
                    Debug.Log("[DEBUG_LOG] SceneTransitionManager: ZoomIn callback received");
                    zoomInDone = true;
                });
            }
            else
            {
                Debug.LogWarning("[DEBUG_LOG] SceneTransitionManager: CameraTransitionSystem.Instance is null!");
                zoomInDone = true;
            }
            
            float timeout = 5f;
            float timer = 0f;
            while (!zoomInDone && timer < timeout) 
            {
                timer += Time.deltaTime;
                yield return null;
            }
            
            if (timer >= timeout) Debug.LogError("[DEBUG_LOG] SceneTransitionManager: ZoomIn TIMEOUT!");

            currentState = TransitionState.FadingToBlack;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: FadingToBlack");

            bool fadeToBlackDone = false;
            if (UITransitionManager.Instance != null)
            {
                UITransitionManager.Instance.FadeToBlack(fadeInDuration, () => {
                    Debug.Log("[DEBUG_LOG] SceneTransitionManager: FadeToBlack callback received");
                    fadeToBlackDone = true;
                });
            }
            else
            {
                Debug.LogWarning("[DEBUG_LOG] SceneTransitionManager: UITransitionManager.Instance is null!");
                fadeToBlackDone = true;
            }
            
            timer = 0f;
            while (!fadeToBlackDone && timer < timeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            if (timer >= timeout) Debug.LogError("[DEBUG_LOG] SceneTransitionManager: FadeToBlack TIMEOUT!");

            currentState = TransitionState.LoadingScene;
            Debug.Log($"[DEBUG_LOG] SceneTransitionManager: LoadingScene {targetScene}");

            AsyncOperation load = SceneManager.LoadSceneAsync(targetScene);
            if (load != null)
            {
                while (!load.isDone) yield return null;
            }

            Debug.Log("[DEBUG_LOG] SceneTransitionManager: Scene loaded, waiting one frame");
            // Wait one frame to ensure SceneLoaded events fire and Player is spawned/found
            yield return null;

            playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Debug.Log("[DEBUG_LOG] SceneTransitionManager: Player found, resetting state");
                var controller = playerObj.GetComponent<PlayerController>();
                controller?.ResetState();
                controller?.SetControlEnabled(false);
            }

            currentState = TransitionState.FadingFromBlack;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: FadingFromBlack");

            bool fadeFromBlackDone = false;
            if (UITransitionManager.Instance != null)
            {
                UITransitionManager.Instance.FadeFromBlack(fadeOutDuration, () => {
                    Debug.Log("[DEBUG_LOG] SceneTransitionManager: FadeFromBlack callback received");
                    fadeFromBlackDone = true;
                });
            }
            else
            {
                Debug.LogWarning("[DEBUG_LOG] SceneTransitionManager: UITransitionManager.Instance is null!");
                fadeFromBlackDone = true;
            }

            timer = 0f;
            while (!fadeFromBlackDone && timer < timeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            if (timer >= timeout) Debug.LogError("[DEBUG_LOG] SceneTransitionManager: FadeFromBlack TIMEOUT!");

            currentState = TransitionState.ZoomingOut;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: ZoomingOut");

            if (CameraTransitionSystem.Instance != null)
            {
                CameraTransitionSystem.Instance.StartZoomOut(() =>
                {
                    Debug.Log("[DEBUG_LOG] SceneTransitionManager: ZoomOut callback received, enabling player control");
                    playerObj?.GetComponent<PlayerController>()?.SetControlEnabled(true);
                    currentState = TransitionState.None;
                });
            }
            else
            {
                Debug.LogWarning("[DEBUG_LOG] SceneTransitionManager: CameraTransitionSystem.Instance is null in ZoomingOut!");
                playerObj?.GetComponent<PlayerController>()?.SetControlEnabled(true);
                currentState = TransitionState.None;
            }
        }
    }
}