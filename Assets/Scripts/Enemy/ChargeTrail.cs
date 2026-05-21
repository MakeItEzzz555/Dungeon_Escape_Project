using UnityEngine;

[DisallowMultipleComponent]
public class ChargeTrail : MonoBehaviour
{
    private struct Afterimage
    {
        public GameObject GameObject;
        public SpriteRenderer Renderer;
        public float Age;
        public bool Active;
    }

    [Header("Source")]
    [SerializeField] private SpriteRenderer sourceRenderer;

    [Header("Sampling")]
    [SerializeField, Min(1)] private int afterimageCount = 4;
    [SerializeField, Min(0.01f)] private float sampleInterval = 0.04f;
    [SerializeField, Min(0.01f)] private float lifetime = 0.18f;
    [SerializeField] private bool clearOnPlay = true;

    [Header("Look")]
    [SerializeField] private Color startTint = new Color(1f, 0.35f, 0.1f, 0.55f);
    [SerializeField] private Color endTint = new Color(1f, 0.05f, 0.02f, 0f);
    [SerializeField] private int sortingOrderOffset = 1;

    private Afterimage[] afterimages;
    private Transform poolRoot;
    private bool emitting;
    private float sampleTimer;
    private int nextIndex;

    private void Awake()
    {
        ResolveSourceIfMissing();
        EnsurePool();
        Clear();
    }

    private void OnDestroy()
    {
        if (poolRoot != null)
        {
            Destroy(poolRoot.gameObject);
        }
    }

    private void LateUpdate()
    {
        UpdateAfterimages(Time.deltaTime);

        if (!emitting) return;

        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval)
        {
            sampleTimer = 0f;
            SpawnAfterimage();
        }
    }

    public void Play()
    {
        ResolveSourceIfMissing();
        EnsurePool();

        if (clearOnPlay)
        {
            Clear();
        }

        emitting = true;
        sampleTimer = 0f;
        SpawnAfterimage();
    }

    public void Stop(bool clear)
    {
        emitting = false;

        if (clear)
        {
            Clear();
        }
    }

    public void Clear()
    {
        if (afterimages == null) return;

        for (int i = 0; i < afterimages.Length; i++)
        {
            afterimages[i].Age = 0f;
            afterimages[i].Active = false;

            if (afterimages[i].GameObject != null)
            {
                afterimages[i].GameObject.SetActive(false);
            }
        }
    }

    private void ResolveSourceIfMissing()
    {
        if (sourceRenderer != null) return;

        FinalBossAnimationBridge animationBridge = GetComponentInChildren<FinalBossAnimationBridge>(true);
        if (animationBridge != null)
        {
            sourceRenderer = animationBridge.GetComponent<SpriteRenderer>();
        }

        if (sourceRenderer == null)
        {
            sourceRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }
    }

    private void EnsurePool()
    {
        int desiredCount = Mathf.Max(1, afterimageCount);
        if (afterimages != null && afterimages.Length == desiredCount)
        {
            if (nextIndex < 0 || nextIndex >= afterimages.Length)
            {
                nextIndex = 0;
            }

            return;
        }

        DestroyPoolRoot();

        poolRoot = new GameObject($"{name}_ChargeTrailPool").transform;
        poolRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        afterimages = new Afterimage[desiredCount];
        nextIndex = 0;
        sampleTimer = 0f;

        for (int i = 0; i < afterimages.Length; i++)
        {
            GameObject afterimageObject = new GameObject($"ChargeTrailAfterimage_{i + 1}");
            afterimageObject.transform.SetParent(poolRoot, false);

            SpriteRenderer renderer = afterimageObject.AddComponent<SpriteRenderer>();
            renderer.enabled = true;

            afterimages[i] = new Afterimage
            {
                GameObject = afterimageObject,
                Renderer = renderer,
                Age = 0f,
                Active = false
            };

            afterimageObject.SetActive(false);
        }
    }

    private void DestroyPoolRoot()
    {
        if (poolRoot == null) return;

        if (Application.isPlaying)
        {
            Destroy(poolRoot.gameObject);
        }
        else
        {
            DestroyImmediate(poolRoot.gameObject);
        }

        poolRoot = null;
    }

    private void SpawnAfterimage()
    {
        if (sourceRenderer == null || sourceRenderer.sprite == null || afterimages == null || afterimages.Length == 0)
        {
            return;
        }

        if (nextIndex < 0 || nextIndex >= afterimages.Length)
        {
            nextIndex = 0;
        }

        int spawnIndex = nextIndex;
        Afterimage afterimage = afterimages[spawnIndex];
        nextIndex = (nextIndex + 1) % afterimages.Length;

        Transform sourceTransform = sourceRenderer.transform;
        afterimage.GameObject.transform.SetPositionAndRotation(sourceTransform.position, sourceTransform.rotation);
        afterimage.GameObject.transform.localScale = sourceTransform.lossyScale;

        afterimage.Renderer.sprite = sourceRenderer.sprite;
        afterimage.Renderer.flipX = sourceRenderer.flipX;
        afterimage.Renderer.flipY = sourceRenderer.flipY;
        afterimage.Renderer.sortingLayerID = sourceRenderer.sortingLayerID;
        afterimage.Renderer.sortingOrder = sourceRenderer.sortingOrder + sortingOrderOffset;
        afterimage.Renderer.color = startTint;

        afterimage.Age = 0f;
        afterimage.Active = true;
        afterimage.GameObject.SetActive(true);
        afterimages[spawnIndex] = afterimage;
    }

    private void UpdateAfterimages(float deltaTime)
    {
        if (afterimages == null) return;

        for (int i = 0; i < afterimages.Length; i++)
        {
            Afterimage afterimage = afterimages[i];
            if (!afterimage.Active) continue;

            afterimage.Age += deltaTime;
            float t = Mathf.Clamp01(afterimage.Age / lifetime);
            afterimage.Renderer.color = Color.Lerp(startTint, endTint, t);

            if (t >= 1f)
            {
                afterimage.Active = false;
                afterimage.GameObject.SetActive(false);
            }

            afterimages[i] = afterimage;
        }
    }
}
