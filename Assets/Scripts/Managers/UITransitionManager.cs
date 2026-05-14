using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Scripts.Managers
{
    public class UITransitionManager : MonoBehaviour
    {
        public static UITransitionManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Canvas fadeCanvas;
        [SerializeField] private Image fadeOverlay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void FadeToBlack(float duration, System.Action onComplete = null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(1f, duration, onComplete));
        }

        public void FadeFromBlack(float duration, System.Action onComplete = null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(0f, duration, onComplete));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, System.Action onComplete)
        {
            if (fadeOverlay == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            float startAlpha = fadeOverlay.color.a;
            float elapsed = 0f;
            Color c = fadeOverlay.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float a = Mathf.Lerp(startAlpha, targetAlpha, t);
                
                c.a = a;
                fadeOverlay.color = c;
                yield return null;
            }

            c.a = targetAlpha;
            fadeOverlay.color = c;
            onComplete?.Invoke();
        }
    }
}
