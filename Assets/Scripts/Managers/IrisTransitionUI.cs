using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Managers
{
    public class IrisTransitionUI : MonoBehaviour
    {
        public static IrisTransitionUI Instance { get; private set; }

        [SerializeField] private Image irisImage;
        [SerializeField] private float defaultDuration = 0.5f;

        private Material irisMaterial;
        private static readonly int RadiusID = Shader.PropertyToID("_Radius");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (irisImage == null)
                irisImage = GetComponentInChildren<Image>();

            if (irisImage != null)
            {
                irisMaterial = irisImage.material;
                // Start opened
                SetRadius(1.5f);
                irisImage.enabled = false;
            }
        }

        public void Close(float duration, System.Action onComplete)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateIris(1.5f, 0f, duration, onComplete));
        }

        public void Open(float duration, System.Action onComplete)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateIris(0f, 1.5f, duration, onComplete));
        }

        private IEnumerator AnimateIris(float start, float end, float duration, System.Action onComplete)
        {
            if (irisImage == null || irisMaterial == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            irisImage.enabled = true;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = Mathf.SmoothStep(0, 1, t);
                SetRadius(Mathf.Lerp(start, end, t));
                yield return null;
            }

            SetRadius(end);
            if (end >= 1.5f) irisImage.enabled = false;
            
            onComplete?.Invoke();
        }

        private void SetRadius(float radius)
        {
            if (irisMaterial != null)
                irisMaterial.SetFloat(RadiusID, radius);
        }
    }
}
