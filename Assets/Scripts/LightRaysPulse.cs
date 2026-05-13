using UnityEngine;

public class LightRaySequencedFX : MonoBehaviour
{
    [Header("Renderers")]
    public SpriteRenderer rayA;
    public SpriteRenderer rayB;

    [Header("Timing")]
    public float fadeInTime = 0.8f;
    public float holdTime = 1.2f;
    public float fadeOutTime = 0.8f;

    [Header("Visual Variation")]
    public float noiseStrength = 0.05f;
    public float driftSpeed = 0.2f;

    [Header("Alpha Range")]
    public float maxAlpha = 0.05f;

    private enum State
    {
        RayA_FadeIn,
        RayA_Hold,
        RayA_FadeOut,
        RayB_FadeIn,
        RayB_Hold,
        RayB_FadeOut
    }

    private State state;
    private float timer;

    private Vector3 basePos;

    void Start()
    {
        basePos = transform.localPosition;
        state = State.RayA_FadeIn;
        timer = 0f;

        SetAlpha(rayA, 0f);
        SetAlpha(rayB, 0f);
    }

    void Update()
    {
        timer += Time.deltaTime;

        ApplyNoiseAndDrift();

        switch (state)
        {
            case State.RayA_FadeIn:
                UpdateFade(rayA, rayB, timer, fadeInTime, true, State.RayA_Hold);
                break;

            case State.RayA_Hold:
                SetAlpha(rayA, maxAlpha);
                SetAlpha(rayB, 0f);
                if (timer >= holdTime) Next(State.RayA_FadeOut);
                break;

            case State.RayA_FadeOut:
                UpdateFade(rayA, rayB, timer, fadeOutTime, false, State.RayB_FadeIn);
                break;

            case State.RayB_FadeIn:
                UpdateFade(rayB, rayA, timer, fadeInTime, true, State.RayB_Hold);
                break;

            case State.RayB_Hold:
                SetAlpha(rayB, maxAlpha);
                SetAlpha(rayA, 0f);
                if (timer >= holdTime) Next(State.RayB_FadeOut);
                break;

            case State.RayB_FadeOut:
                UpdateFade(rayB, rayA, timer, fadeOutTime, false, State.RayA_FadeIn);
                break;
        }
    }

    void UpdateFade(SpriteRenderer activeRay, SpriteRenderer inactiveRay, float t, float duration, bool fadeIn, State nextState)
    {
        float normalized = Mathf.Clamp01(t / duration);
        float alpha = fadeIn
            ? Mathf.Lerp(0f, maxAlpha, normalized)
            : Mathf.Lerp(maxAlpha, 0f, normalized);

        SetAlpha(activeRay, alpha);
        SetAlpha(inactiveRay, 0f);

        if (t >= duration)
        {
            // Enforce final alpha value before transition
            SetAlpha(activeRay, fadeIn ? maxAlpha : 0f);
            Next(nextState);
        }
    }

    void Next(State newState)
    {
        state = newState;
        timer = 0f;
    }

    void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr == null) return;

        Color c = sr.color;
        c.a = Mathf.Clamp(alpha, 0f, maxAlpha);
        sr.color = c;
    }

    void ApplyNoiseAndDrift()
    {
        float noiseX = (Mathf.PerlinNoise(Time.time * 0.5f, 0f) - 0.5f) * noiseStrength;
        float noiseY = (Mathf.PerlinNoise(0f, Time.time * 0.5f) - 0.5f) * noiseStrength;

        float drift = Mathf.Sin(Time.time * driftSpeed) * noiseStrength;

        transform.localPosition = basePos + new Vector3(noiseX, noiseY + drift, 0f);
    }
}