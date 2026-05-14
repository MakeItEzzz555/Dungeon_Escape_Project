using UnityEngine;
using TMPro;
using System.Collections;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("State")]
    public bool hudRevealed;
    public bool coinsDiscovered;
    public bool keysDiscovered;

    [Header("UI References")]
    [SerializeField] private RectTransform hudPanelRect;
    [SerializeField] private GameObject coinsObject;
    [SerializeField] private GameObject keysObject;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI keysText;

    [Header("Animation Settings")]
    [SerializeField] private float hiddenX = -1000f;
    [SerializeField] private float shownX = 0f;
    [SerializeField] private float slideDuration = 0.5f;

    [Header("Collection Tracking")]
    [Tooltip("If set to 0, it will automatically count objects with the 'Coin' tag in the scene.")]
    public int manualMaxCoins = 0;

    private int coinsCollected = 0;
    private int totalCoins = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Use manual values if provided, otherwise count how many coins in the scene
        totalCoins = manualMaxCoins > 0 ? manualMaxCoins : GameObject.FindGameObjectsWithTag("Coin").Length;

        // On Start(): set HUD panel to hiddenX, disable coinsObject, disable keysObject
        if (hudPanelRect != null)
        {
            Vector2 pos = hudPanelRect.anchoredPosition;
            pos.x = hiddenX;
            hudPanelRect.anchoredPosition = pos;
        }

        if (coinsObject != null) coinsObject.SetActive(false);
        if (keysObject != null) keysObject.SetActive(false);
        
        // Ensure counters are up to date but they stay hidden until discovered
        UpdateCoinsText();
    }

    public void UpdateCoins(int current, int total)
    {
        coinsCollected = current;
        totalCoins = total;
        
        RevealHUDIfNeeded();

        if (!coinsDiscovered && current > 0)
        {
            coinsDiscovered = true;
            if (coinsObject != null) coinsObject.SetActive(true);
        }

        UpdateCoinsText();
    }

    public void UpdateKeys(int current, int total)
    {
        RevealHUDIfNeeded();

        if (!keysDiscovered && current > 0)
        {
            keysDiscovered = true;
            if (keysObject != null) keysObject.SetActive(true);
        }

        if (keysText != null)
        {
            keysText.text = $"Keys: {current} / {total}";
        }
    }

    public void RevealHUDIfNeeded()
    {
        if (!hudRevealed)
        {
            hudRevealed = true;
            StartCoroutine(AnimateHUDSlideIn());
        }
    }

    private IEnumerator AnimateHUDSlideIn()
    {
        if (hudPanelRect == null) yield break;

        float elapsed = 0f;
        Vector2 startPos = hudPanelRect.anchoredPosition;
        startPos.x = hiddenX;
        Vector2 endPos = startPos;
        endPos.x = shownX;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            
            // Back ease-out for slight overshoot feel
            // formula: 1 - (1 - t)^3 * (1 - t * (1.70158 * t - 1.70158)) -- roughly
            // Standard back ease-out:
            float s = 1.70158f;
            float t1 = t - 1;
            float curvedT = t1 * t1 * ((s + 1) * t1 + s) + 1;
            
            hudPanelRect.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, curvedT);
            yield return null;
        }

        hudPanelRect.anchoredPosition = endPos;
    }

    // Support for existing Collect_coins.cs
    public void RegisterCoinCollection()
    {
        UpdateCoins(coinsCollected + 1, totalCoins);
    }

    private void UpdateCoinsText()
    {
        if (coinsText != null)
        {
            coinsText.text = $"Coins: {coinsCollected} / {totalCoins}";
        }
    }
}
