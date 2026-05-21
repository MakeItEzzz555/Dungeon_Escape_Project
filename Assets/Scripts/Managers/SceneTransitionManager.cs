using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Scripts.Managers
{
    public enum TransitionState
    {
        None,
        FailureAnimation,
        ZoomingIn,
        FadingToBlack,
        ResultsVisible,
        LoadingScene,
        ReloadingScene,
        FadingFromBlack,
        ZoomingOut,
        RestoringControl,
        ExitingToMenu
    }
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        private TransitionState currentState = TransitionState.None;

        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.8f;
        [SerializeField] private string mainMenuSceneName = "Main Menu";

        public bool IsTransitioning => currentState != TransitionState.None;
        public bool IsRunResultsVisible => currentState == TransitionState.ResultsVisible;

        private string pendingTargetScene;
        private RunResultsMode pendingResultsMode;
        private float timeScaleBeforeRunResults = 1f;
        private bool hasTimeScaleBeforeRunResults;

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
            BeginLevelCompletionTransition(targetScene, zoomTarget);
        }

        public void BeginLevelCompletionTransition(string targetScene, Transform zoomTarget)
        {
            if (currentState != TransitionState.None) return;

            StartCoroutine(LevelCompletionResultsRoutine(targetScene, zoomTarget));
        }

        public void BeginLevelCompletionTransitionWithPresentationWindow(string targetScene, Transform zoomTarget, float presentationDuration)
        {
            if (currentState != TransitionState.None) return;

            StartCoroutine(LevelCompletionResultsRoutine(targetScene, zoomTarget, Mathf.Max(0f, presentationDuration), true));
        }

        public void BeginRespawnTransition(Transform zoomTarget, float failureAnimationDuration)
        {
            if (currentState != TransitionState.None) return;

            StartCoroutine(RespawnTransitionRoutine(zoomTarget, failureAnimationDuration));
        }

        private IEnumerator LevelCompletionResultsRoutine(string targetScene, Transform zoomTarget)
        {
            yield return LevelCompletionResultsRoutine(targetScene, zoomTarget, 0f, false);
        }

        private IEnumerator LevelCompletionResultsRoutine(string targetScene, Transform zoomTarget, float presentationDuration, bool holdZoomTarget)
        {
            Debug.Log($"[DEBUG_LOG] SceneTransitionManager: BeginLevelCompletionTransition to {targetScene}");
            currentState = TransitionState.ZoomingIn;
            pendingResultsMode = RunResultsMode.Completion;
            pendingTargetScene = targetScene;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: Disabling player control");
            playerObj?.GetComponent<PlayerController>()?.SetControlEnabled(false);

            if (presentationDuration > 0f)
            {
                float fadeStartDelay = Mathf.Max(0f, presentationDuration - fadeInDuration);
                yield return ZoomInDuringPresentationWindow(zoomTarget, fadeStartDelay, holdZoomTarget);
            }
            else
            {
                yield return ZoomIn(zoomTarget);
            }

            currentState = TransitionState.FadingToBlack;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: FadingToBlack");

            yield return FadeToBlack();
            CameraTransitionSystem.Instance?.ReleaseHeldTarget();

            ShowRunResults(RunResultsMode.Completion);
        }

        public void ContinueFromRunResults()
        {
            if (currentState != TransitionState.ResultsVisible ||
                pendingResultsMode != RunResultsMode.Completion ||
                string.IsNullOrEmpty(pendingTargetScene))
            {
                Debug.LogWarning($"[DEBUG_LOG] SceneTransitionManager: Ignored Continue. State={currentState}, PendingMode={pendingResultsMode}, Target='{pendingTargetScene}'.");
                return;
            }

            StartCoroutine(ContinueFromRunResultsRoutine());
        }

        public void RespawnFromRunResults()
        {
            if (currentState != TransitionState.ResultsVisible ||
                pendingResultsMode != RunResultsMode.Failure)
            {
                Debug.LogWarning($"[DEBUG_LOG] SceneTransitionManager: Ignored Respawn. State={currentState}, PendingMode={pendingResultsMode}.");
                return;
            }

            StartCoroutine(RespawnFromRunResultsRoutine());
        }

        public void ExitRunResultsToMainMenu()
        {
            if (currentState != TransitionState.ResultsVisible)
            {
                Debug.LogWarning($"[DEBUG_LOG] SceneTransitionManager: Ignored Exit. State={currentState}.");
                return;
            }

            StartCoroutine(ExitRunResultsToMainMenuRoutine());
        }

        public void ExitToMainMenuFromGameplay()
        {
            if (currentState != TransitionState.None)
            {
                Debug.LogWarning($"[DEBUG_LOG] SceneTransitionManager: Ignored gameplay menu exit. State={currentState}.");
                return;
            }

            StartCoroutine(ExitToMainMenuFromGameplayRoutine());
        }

        public void StartGameFromMainMenu(string targetScene)
        {
            if (currentState != TransitionState.None)
            {
                Debug.LogWarning($"[DEBUG_LOG] SceneTransitionManager: Ignored menu play. State={currentState}.");
                return;
            }

            if (string.IsNullOrEmpty(targetScene))
            {
                Debug.LogWarning("[DEBUG_LOG] SceneTransitionManager: Ignored menu play with empty target scene.");
                return;
            }

            StartCoroutine(StartGameFromMainMenuRoutine(targetScene));
        }

        private IEnumerator ContinueFromRunResultsRoutine()
        {
            RestoreTimeScaleAfterRunResults();
            HUDManager.Instance?.HideRunResultsImmediate();

            if (IsMainMenuScene(pendingTargetScene))
            {
                currentState = TransitionState.ExitingToMenu;
                Debug.Log($"[DEBUG_LOG] SceneTransitionManager: Continue target is {mainMenuSceneName}");

                yield return LoadMainMenuWhileBlack();
                ClearPendingResults();
                yield break;
            }

            currentState = TransitionState.LoadingScene;
            Debug.Log($"[DEBUG_LOG] SceneTransitionManager: LoadingScene {pendingTargetScene}");

            yield return LoadSceneAndLockPlayer(pendingTargetScene);

            yield return RestoreSceneAfterBlack();
        }

        private IEnumerator RespawnFromRunResultsRoutine()
        {
            RestoreTimeScaleAfterRunResults();
            HUDManager.Instance?.HideRunResultsImmediate();

            currentState = TransitionState.ReloadingScene;
            string activeScene = SceneManager.GetActiveScene().name;
            Debug.Log($"[DEBUG_LOG] SceneTransitionManager: Respawn reloading active scene {activeScene}");

            yield return LoadSceneAndLockPlayer(activeScene);

            yield return RestoreSceneAfterBlack();
        }

        private IEnumerator ExitRunResultsToMainMenuRoutine()
        {
            RestoreTimeScaleAfterRunResults();
            HUDManager.Instance?.HideRunResultsImmediate();

            currentState = TransitionState.ExitingToMenu;
            Debug.Log($"[DEBUG_LOG] SceneTransitionManager: Exiting to {mainMenuSceneName}");

            yield return LoadMainMenuWhileBlack();
            ClearPendingResults();
        }

        private IEnumerator ExitToMainMenuFromGameplayRoutine()
        {
            currentState = TransitionState.ExitingToMenu;
            Time.timeScale = 1f;
            HUDManager.Instance?.SetGameplayHudSuppressed(false);
            HUDManager.Instance?.HideRunResultsImmediate();

            yield return FadeToBlack();
            yield return LoadMainMenuWhileBlack();
        }

        private IEnumerator StartGameFromMainMenuRoutine(string targetScene)
        {
            currentState = TransitionState.LoadingScene;
            Time.timeScale = 1f;
            UnlockCursorForMenu();

            yield return FadeToBlack();
            AudioManager.Instance?.PlayGameplayMusic();

            yield return LoadSceneAndLockPlayer(targetScene);
            CameraTransitionSystem cameraTransitionSystem = CameraTransitionSystem.Instance;
            cameraTransitionSystem?.SetInstantZoom(cameraTransitionSystem.TransitionZoom);

            yield return RestoreSceneAfterBlack();
        }

        private IEnumerator RestoreSceneAfterBlack()
        {
            currentState = TransitionState.FadingFromBlack;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: FadingFromBlack");

            yield return FadeFromBlack();

            currentState = TransitionState.ZoomingOut;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: ZoomingOut");

            yield return ZoomOut();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            playerObj?.GetComponent<PlayerController>()?.SetControlEnabled(true);
            currentState = TransitionState.None;
            ClearPendingResults();
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

            pendingResultsMode = RunResultsMode.Failure;
            pendingTargetScene = SceneManager.GetActiveScene().name;
            ShowRunResults(RunResultsMode.Failure);
        }

        private void ShowRunResults(RunResultsMode mode)
        {
            currentState = TransitionState.ResultsVisible;
            RunStatsSnapshot snapshot = HUDManager.Instance != null
                ? HUDManager.Instance.CreateRunStatsSnapshot()
                : new RunStatsSnapshot(SceneManager.GetActiveScene().name, 0, 0, 0f, 0, 0);

            HUDManager.Instance?.ShowRunResults(mode, snapshot);
            FreezeTimeForRunResults();
        }

        private void FreezeTimeForRunResults()
        {
            if (!hasTimeScaleBeforeRunResults)
            {
                timeScaleBeforeRunResults = Time.timeScale;
                hasTimeScaleBeforeRunResults = true;
            }

            EnsureUIInputUsesDynamicUpdate();
            Time.timeScale = 0f;
            Debug.Log("[DEBUG_LOG] SceneTransitionManager: Time frozen while RunResultsPanel is visible.");
        }

        private void RestoreTimeScaleAfterRunResults()
        {
            if (!hasTimeScaleBeforeRunResults) return;

            Time.timeScale = timeScaleBeforeRunResults > 0f ? timeScaleBeforeRunResults : 1f;
            hasTimeScaleBeforeRunResults = false;
            Debug.Log($"[DEBUG_LOG] SceneTransitionManager: Time restored after RunResultsPanel. timeScale={Time.timeScale}");
        }

        private void EnsureUIInputUsesDynamicUpdate()
        {
#if ENABLE_INPUT_SYSTEM
            if (InputSystem.settings != null &&
                InputSystem.settings.updateMode != InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
            {
                InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            }
#endif
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

        private IEnumerator ZoomInDuringPresentationWindow(Transform zoomTarget, float presentationDuration, bool holdZoomTarget)
        {
            bool zoomInDone = false;
            float elapsed = 0f;

            Debug.Log("[DEBUG_LOG] SceneTransitionManager: Calling StartZoomIn for presentation window");
            if (CameraTransitionSystem.Instance != null)
            {
                if (holdZoomTarget)
                {
                    CameraTransitionSystem.Instance.StartZoomInAndHoldTarget(zoomTarget, () => {
                        Debug.Log("[DEBUG_LOG] SceneTransitionManager: Presentation ZoomIn callback received");
                        zoomInDone = true;
                    });
                }
                else
                {
                    CameraTransitionSystem.Instance.StartZoomIn(zoomTarget, () => {
                        Debug.Log("[DEBUG_LOG] SceneTransitionManager: Presentation ZoomIn callback received");
                        zoomInDone = true;
                    });
                }
            }
            else
            {
                Debug.LogWarning("[DEBUG_LOG] SceneTransitionManager: CameraTransitionSystem.Instance is null!");
                zoomInDone = true;
            }

            while ((!zoomInDone || elapsed < presentationDuration) && elapsed < 5f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= 5f && !zoomInDone)
            {
                Debug.LogError("[DEBUG_LOG] SceneTransitionManager: Presentation ZoomIn TIMEOUT!");
            }
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

        private IEnumerator LoadSceneWithoutPlayerLock(string sceneName)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
            if (load != null)
            {
                while (!load.isDone) yield return null;
            }

            yield return null;
        }

        private IEnumerator LoadMainMenuWhileBlack()
        {
            yield return LoadSceneWithoutPlayerLock(mainMenuSceneName);
            AudioManager.Instance?.PlayMainMenuMusic();
            UnlockCursorForMenu();
            CameraTransitionSystem cameraTransitionSystem = CameraTransitionSystem.Instance;
            cameraTransitionSystem?.SetInstantZoom(cameraTransitionSystem.GameplayZoom);

            yield return FadeFromBlack();
            currentState = TransitionState.None;
        }

        private void ClearPendingResults()
        {
            pendingTargetScene = null;
            pendingResultsMode = RunResultsMode.Completion;
        }

        private bool IsMainMenuScene(string sceneName)
        {
            return string.Equals(sceneName, mainMenuSceneName, System.StringComparison.Ordinal);
        }

        private void UnlockCursorForMenu()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
