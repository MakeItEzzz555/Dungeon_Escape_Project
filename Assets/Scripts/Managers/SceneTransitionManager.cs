using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Managers
{
    public enum TransitionState
    {
        None,
        FailureAnimation,
        ZoomingIn,
        FadingToBlack,
        LoadingScene,
        ReloadingScene,
        FadingFromBlack,
        ZoomingOut,
        RestoringControl
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

        public void BeginRespawnTransition(Transform zoomTarget, float failureAnimationDuration)
        {
            if (currentState != TransitionState.None) return;

            StartCoroutine(RespawnTransitionRoutine(zoomTarget, failureAnimationDuration));
        }

        private IEnumerator TransitionRoutine(string targetScene, Transform zoomTarget)
        {
            Debug.Log($"[DEBUG_LOG] SceneTransitionManager: BeginTransition to {targetScene}");
            currentState = TransitionState.ZoomingIn;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: Disabling player control");
            playerObj?.GetComponent<PlayerController>()?.SetControlEnabled(false);

            yield return ZoomIn(zoomTarget);

            currentState = TransitionState.FadingToBlack;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: FadingToBlack");

            yield return FadeToBlack();

            currentState = TransitionState.LoadingScene;
            Debug.Log($"[DEBUG_LOG] SceneTransitionManager: LoadingScene {targetScene}");

            yield return LoadSceneAndLockPlayer(targetScene);
            playerObj = GameObject.FindGameObjectWithTag("Player");

            currentState = TransitionState.FadingFromBlack;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: FadingFromBlack");

            yield return FadeFromBlack();

            currentState = TransitionState.ZoomingOut;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: ZoomingOut");

            yield return ZoomOut();

            playerObj?.GetComponent<PlayerController>()?.SetControlEnabled(true);
            currentState = TransitionState.None;
        }

        private IEnumerator RespawnTransitionRoutine(Transform zoomTarget, float failureAnimationDuration)
        {
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: BeginRespawnTransition");
            currentState = TransitionState.FailureAnimation;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            PlayerController controller = playerObj?.GetComponent<PlayerController>();
            controller?.SetControlEnabled(false);

            if (failureAnimationDuration > 0f)
            {
                yield return new WaitForSeconds(failureAnimationDuration);
            }

            currentState = TransitionState.ZoomingIn;
            yield return ZoomIn(zoomTarget != null ? zoomTarget : playerObj?.transform);

            currentState = TransitionState.FadingToBlack;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: Respawn FadingToBlack");
            yield return FadeToBlack();

            currentState = TransitionState.ReloadingScene;
            string activeScene = SceneManager.GetActiveScene().name;
            Debug.Log($"[DEBUG_LOG] SceneTransitionManager: Respawn reloading active scene {activeScene}");
            yield return LoadSceneAndLockPlayer(activeScene);

            currentState = TransitionState.FadingFromBlack;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: Respawn FadingFromBlack");
            yield return FadeFromBlack();

            currentState = TransitionState.ZoomingOut;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: Respawn ZoomingOut");
            yield return ZoomOut();

            currentState = TransitionState.RestoringControl;
            playerObj = GameObject.FindGameObjectWithTag("Player");
            controller = playerObj?.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.SetControlEnabled(true);
            }
            else
            {
                Debug.LogError("[DEBUG_LOG] SceneTransitionManager: Respawn completed without finding a PlayerController.");
            }

            currentState = TransitionState.None;
        }

        private IEnumerator ZoomIn(Transform zoomTarget)
        {
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

            yield return WaitForTransitionStep(() => zoomInDone, "ZoomIn");
        }

        private IEnumerator ZoomOut()
        {
            bool zoomOutDone = false;
            if (CameraTransitionSystem.Instance != null)
            {
                CameraTransitionSystem.Instance.StartZoomOut(() =>
                {
                    Debug.Log("[DEBUG_LOG] SceneTransitionManager: ZoomOut callback received");
                    zoomOutDone = true;
                });
            }
            else
            {
                Debug.LogWarning("[DEBUG_LOG] SceneTransitionManager: CameraTransitionSystem.Instance is null in ZoomingOut!");
                zoomOutDone = true;
            }

            yield return WaitForTransitionStep(() => zoomOutDone, "ZoomOut");
        }

        private IEnumerator FadeToBlack()
        {
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

            yield return WaitForTransitionStep(() => fadeToBlackDone, "FadeToBlack");
        }

        private IEnumerator FadeFromBlack()
        {
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

            yield return WaitForTransitionStep(() => fadeFromBlackDone, "FadeFromBlack");
        }

        private IEnumerator LoadSceneAndLockPlayer(string sceneName)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
            if (load != null)
            {
                while (!load.isDone) yield return null;
            }

            Debug.Log("[DEBUG_LOG] SceneTransitionManager: Scene loaded, waiting one frame");
            yield return null;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Debug.Log("[DEBUG_LOG] SceneTransitionManager: Player found, resetting state");
                var controller = playerObj.GetComponent<PlayerController>();
                controller?.ResetState();
                controller?.SetControlEnabled(false);
            }
            else
            {
                Debug.LogError("[DEBUG_LOG] SceneTransitionManager: Scene loaded without a Player tagged object.");
            }
        }

        private IEnumerator WaitForTransitionStep(System.Func<bool> isDone, string stepName)
        {
            const float timeout = 5f;
            float timer = 0f;
            while (!isDone() && timer < timeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (timer >= timeout)
            {
                Debug.LogError($"[DEBUG_LOG] SceneTransitionManager: {stepName} TIMEOUT!");
            }
        }
    }
}
