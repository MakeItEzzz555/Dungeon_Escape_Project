using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    [Header("HP UI")]
    [SerializeField] private Transform hudHpPanel;

    [Header("UX Notification UI")]
    [SerializeField] private CanvasGroup uxPanelCanvasGroup;
    [SerializeField] private TextMeshProUGUI uxMessageText;

    [Header("Animation Settings")]
    [SerializeField] private float hiddenX = -1000f;
    [SerializeField] private float shownX = 0f;
    [SerializeField] private float slideDuration = 0.5f;

    [Header("UX Animation Settings")]
    [SerializeField] private float uxFadeInDuration = 0.2f;
    [SerializeField] private float uxHoldDuration = 1.0f;
    [SerializeField] private float uxFadeOutDuration = 0.3f;

    [Header("Collection Tracking")]
    [Tooltip("If set to 0, it will automatically count objects with the 'Coin' tag in the scene.")]
    public int manualMaxCoins = 0;

    private int coinsCollected = 0;
    private int totalCoins = 0;

    private Coroutine uxMessageCoroutine;
    private readonly List<Image> hpImages = new List<Image>();
    private Health subscribedPlayerHealth;
    private int lastDisplayedHealth = -1;
    private int lastDisplayedMaxHealth = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CacheHpImages();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPlayerHealth();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset state for the new scene
        coinsCollected = 0;
        hudRevealed = false;
        coinsDiscovered = false;
        keysDiscovered = false;

        // Recalculate total coins in the new scene
        totalCoins = manualMaxCoins > 0 ? manualMaxCoins : GameObject.FindGameObjectsWithTag("Coin").Length;
        
        // Ensure UI matches the reset state
        ResetHUDUI();
        BindPlayerHealth();
    }

    private void Start()
    {
        // Recalculate if not already done by OnSceneLoaded
        totalCoins = manualMaxCoins > 0 ? manualMaxCoins : GameObject.FindGameObjectsWithTag("Coin").Length;

        ResetHUDUI();
        BindPlayerHealth();
    }

    private void Update()
    {
        if (subscribedPlayerHealth == null)
        {
            BindPlayerHealth();
            return;
        }

        if (subscribedPlayerHealth.currentHealth != lastDisplayedHealth ||
            subscribedPlayerHealth.maxHealth != lastDisplayedMaxHealth)
        {
            UpdatePlayerHp(subscribedPlayerHealth.currentHealth, subscribedPlayerHealth.maxHealth);
        }
    }

    private void ResetHUDUI()
    {
        // On Reset/Start: set HUD panel to hiddenX, disable coinsObject, disable keysObject
        if (hudPanelRect != null)
        {
            Vector2 pos = hudPanelRect.anchoredPosition;
            pos.x = hiddenX;
            hudPanelRect.anchoredPosition = pos;
        }

        if (coinsObject != null) coinsObject.SetActive(false);
        if (keysObject != null) keysObject.SetActive(false);
        
        // Initialize UX panel state
        if (uxPanelCanvasGroup != null)
        {
            uxPanelCanvasGroup.alpha = 0f;
            uxPanelCanvasGroup.interactable = false;
            uxPanelCanvasGroup.blocksRaycasts = false;
        }

        UpdateCoinsText();
        SetHpImagesVisible(hpImages.Count);
        // Keys update is usually handled by GlobalQuestManager calling UpdateKeys
    }

    private void CacheHpImages()
    {
        hpImages.Clear();

        if (hudHpPanel == null || hudHpPanel.name != "HUD_HP")
        {
            if (hudHpPanel != null)
            {
                Debug.LogWarning($"[DEBUG_LOG] HUDManager: hudHpPanel points to '{hudHpPanel.name}', expected HUD_HP. Rebinding by name.");
            }

            hudHpPanel = FindChildByName(transform, "HUD_HP");
        }

        if (hudHpPanel == null)
        {
            Debug.LogWarning("[DEBUG_LOG] HUDManager: HUD_HP panel reference missing. Player HP UI will not update.");
            return;
        }

        Image[] images = hudHpPanel.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (image.transform == hudHpPanel) continue;
            hpImages.Add(image);
        }

        hpImages.Sort(CompareHpImagesByPosition);

        if (hpImages.Count == 0)
        {
            Debug.LogWarning("[DEBUG_LOG] HUDManager: HUD_HP has no child Image components.");
        }
    }

    private Transform FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform found = FindChildByName(child, childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void BindPlayerHealth()
    {
        CacheHpImages();
        UnsubscribeFromPlayerHealth();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogWarning("[DEBUG_LOG] HUDManager: Player not found. HP UI cannot bind.");
            return;
        }

        Health health = playerObject.GetComponent<Health>();
        if (health == null) health = playerObject.GetComponentInParent<Health>();

        if (health == null)
        {
            Debug.LogWarning("[DEBUG_LOG] HUDManager: Player Health component missing. HP UI cannot bind.");
            return;
        }

        subscribedPlayerHealth = health;
        subscribedPlayerHealth.OnHealthChanged += UpdatePlayerHp;
        UpdatePlayerHp(subscribedPlayerHealth.currentHealth, subscribedPlayerHealth.maxHealth);
    }

    private void UnsubscribeFromPlayerHealth()
    {
        if (subscribedPlayerHealth == null) return;

        subscribedPlayerHealth.OnHealthChanged -= UpdatePlayerHp;
        subscribedPlayerHealth = null;
    }

    private void UpdatePlayerHp(int currentHealth, int maxHealth)
    {
        if (hpImages.Count == 0) return;

        if (hpImages.Count < maxHealth)
        {
            Debug.LogWarning($"[DEBUG_LOG] HUDManager: HUD_HP has {hpImages.Count} images for max health {maxHealth}.");
        }

        int visibleCount = Mathf.Clamp(currentHealth, 0, hpImages.Count);
        SetHpImagesVisible(visibleCount);
        lastDisplayedHealth = currentHealth;
        lastDisplayedMaxHealth = maxHealth;
    }

    private void SetHpImagesVisible(int visibleCount)
    {
        for (int i = 0; i < hpImages.Count; i++)
        {
            hpImages[i].enabled = i < visibleCount;
        }
    }

    private int CompareHpImagesByPosition(Image left, Image right)
    {
        RectTransform leftRect = left.rectTransform;
        RectTransform rightRect = right.rectTransform;

        float yDelta = rightRect.anchoredPosition.y - leftRect.anchoredPosition.y;
        if (Mathf.Abs(yDelta) > 0.01f)
        {
            return yDelta > 0f ? 1 : -1;
        }

        float xDelta = leftRect.anchoredPosition.x - rightRect.anchoredPosition.x;
        if (Mathf.Abs(xDelta) > 0.01f)
        {
            return xDelta > 0f ? 1 : -1;
        }

        return left.transform.GetSiblingIndex().CompareTo(right.transform.GetSiblingIndex());
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
            float s = 1.70158f;
            float t1 = t - 1;
            float curvedT = t1 * t1 * ((s + 1) * t1 + s) + 1;
            
            hudPanelRect.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, curvedT);
            yield return null;
        }

        hudPanelRect.anchoredPosition = endPos;
    }

    // --- UX Notification API ---

    public void ShowUXMessage(string message)
    {
        if (uxPanelCanvasGroup == null || uxMessageText == null) return;

        if (uxMessageCoroutine != null)
        {
            StopCoroutine(uxMessageCoroutine);
        }

        uxMessageCoroutine = StartCoroutine(UXMessageRoutine(message));
    }

    private IEnumerator UXMessageRoutine(string message)
    {
        uxMessageText.text = message;
        float startAlpha = uxPanelCanvasGroup.alpha;
        float elapsed = 0f;

        // Fade In (from current alpha to 1)
        while (elapsed < uxFadeInDuration)
        {
            elapsed += Time.deltaTime;
            uxPanelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / uxFadeInDuration);
            yield return null;
        }
        uxPanelCanvasGroup.alpha = 1f;

        // Hold
        yield return new WaitForSeconds(uxHoldDuration);

        // Fade Out
        elapsed = 0f;
        while (elapsed < uxFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            uxPanelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / uxFadeOutDuration);
            yield return null;
        }
        uxPanelCanvasGroup.alpha = 0f;

        uxMessageCoroutine = null;
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
