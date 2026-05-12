using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Managers
{
    public enum TransitionState
    {
        None,
        ZoomingIn,
        IrisClosing,
        LoadingScene,
        IrisOpening,
        ZoomingOut
    }
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        private TransitionState currentState = TransitionState.None;
        private GameObject player;

        [SerializeField] private float irisDuration = 0.5f;

        public bool IsTransitioning => currentState != TransitionState.None;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void BeginTransition(string targetScene, Transform zoomTarget)
        {
            if (currentState != TransitionState.None) return;

            StartCoroutine(TransitionRoutine(targetScene, zoomTarget));
        }

        private IEnumerator TransitionRoutine(string targetScene, Transform zoomTarget)
        {
            currentState = TransitionState.ZoomingIn;

            player = GameObject.FindGameObjectWithTag("Player");
            player?.GetComponent<PlayerController>()?.SetControlEnabled(false);

            CameraTransitionSystem.Instance?.StartZoomIn(zoomTarget, null);

            yield return new WaitForSeconds(0.5f);

            currentState = TransitionState.IrisClosing;

            IrisTransitionUI.Instance?.Close(irisDuration, null);

            yield return new WaitForSeconds(irisDuration);

            currentState = TransitionState.LoadingScene;

            AsyncOperation load = SceneManager.LoadSceneAsync(targetScene);

            while (!load.isDone)
                yield return null;

            yield return null;

            player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                var controller = player.GetComponent<PlayerController>();
                controller?.ResetState();
                controller?.SetControlEnabled(false);
            }

            CameraTransitionSystem.Instance?.Reinitialize();

            currentState = TransitionState.IrisOpening;

            IrisTransitionUI.Instance?.Open(irisDuration, null);

            yield return new WaitForSeconds(irisDuration);

            currentState = TransitionState.ZoomingOut;

            CameraTransitionSystem.Instance?.StartZoomOut(null);

            yield return new WaitForSeconds(0.8f);

            player?.GetComponent<PlayerController>()?.SetControlEnabled(true);

            currentState = TransitionState.None;
        }
    }
}