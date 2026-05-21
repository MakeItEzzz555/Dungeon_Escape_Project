using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Scripts.Managers;
using UnityEngine.Events;

public enum RunResultsMode
{
    Completion,
    Failure
}

public readonly struct RunStatsSnapshot
{
    public RunStatsSnapshot(
        string levelName,
        int coinsCollected,
        int totalCoins,
        float elapsedSeconds,
        int enemiesKilled,
        int totalActiveEnemies)
    {
        LevelName = levelName;
        CoinsCollected = coinsCollected;
        TotalCoins = totalCoins;
        ElapsedSeconds = elapsedSeconds;
        EnemiesKilled = enemiesKilled;
        TotalActiveEnemies = totalActiveEnemies;
    }

    public string LevelName { get; }
    public int CoinsCollected { get; }
    public int TotalCoins { get; }
    public float ElapsedSeconds { get; }
    public int EnemiesKilled { get; }
    public int TotalActiveEnemies { get; }
}

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }
    private const string PlayerHpPanelName = "HUD_HP";
    private const string FinalBossHpPanelName = "HUD_HP_FinalBoss";
    private const int FinalBossHpImagesPerRow = 5;
    private const float FinalBossHpStartX = 900f;
    private const float FinalBossHpStartY = 475f;
    private const float FinalBossHpSpacing = 100f;
    private const int RuntimeFinalBossHpSortingOffset = 1;

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
    [SerializeField] private Transform finalBossHpPanel;
    [SerializeField] private Sprite finalBossHpSprite;
    [SerializeField] private GameObject hudItemsPanel;

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

    [Header("Run Results UI")]
    [SerializeField] private CanvasGroup runResultsPanelCanvasGroup;
    [SerializeField] private TextMeshProUGUI runResultsTitleText;
    [SerializeField] private TextMeshProUGUI runResultsCoinsText;
    [SerializeField] private TextMeshProUGUI runResultsTimeText;
    [SerializeField] private TextMeshProUGUI runResultsEnemiesText;
    [SerializeField] private GameObject runResultsContentPanel;
    [SerializeField] private GameObject runResultsSettingsPanel;
    [SerializeField] private GameObject summaryBanner;
    [SerializeField] private GameObject failedBanner;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject respawnButton;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private float runResultsFadeDuration = 0.25f;
    [SerializeField] private int runResultsCanvasSortingOrder = 200;

    [Header("Collection Tracking")]
    [Tooltip("If set to 0, it will automatically count objects with the 'Coin' tag in the scene.")]
    public int manualMaxCoins = 0;

    private int coinsCollected = 0;
    private int totalCoins = 0;

    private Coroutine uxMessageCoroutine;
    private readonly List<Image> hpImages = new List<Image>();
    private readonly List<Image> finalBossHpImages = new List<Image>();
    private readonly List<Health> trackedEnemyHealth = new List<Health>();
    private Health subscribedPlayerHealth;
    private Health subscribedFinalBossHealth;
    private int lastDisplayedHealth = -1;
    private int lastDisplayedMaxHealth = -1;
    private int lastDisplayedFinalBossHealth = -1;
    private int lastDisplayedFinalBossMaxHealth = -1;
    private Canvas hudCanvas;
    private int defaultCanvasSortingOrder;
    private bool defaultCanvasOverrideSorting;
    private bool runTimerActive;
    private float runElapsedSeconds;
    private int totalActiveEnemies;
    private int enemiesKilled;
    private Coroutine runTimerStartRoutine;
    private Coroutine runResultsFadeRoutine;
    private CursorLockMode cursorLockStateBeforeRunResults;
    private bool cursorVisibleBeforeRunResults;
    private bool hasCursorStateBeforeRunResults;
    private bool hudItemsWasActiveBeforeModal;
    private bool hudHpWasActiveBeforeModal;
    private bool finalBossHpWasActiveBeforeModal;
    private bool finalBossHpShouldBeVisible;
    private bool hasGameplayHudVisibilityBeforeModal;
    private Button boundContinueButton;
    private Button boundRespawnButton;
    private Button boundExitButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        hudCanvas = GetComponent<Canvas>();
        if (hudCanvas != null)
        {
            defaultCanvasSortingOrder = hudCanvas.sortingOrder;
            defaultCanvasOverrideSorting = hudCanvas.overrideSorting;
        }

        CacheHpImages();
        CacheFinalBossHpImages(false);
        SetFinalBossHpPanelActive(false);
        CacheRunResultsReferences();
        CacheGameplayHudReferences();
        ResetRunStats();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPlayerHealth();
        UnsubscribeFromFinalBossHealth();
        UnsubscribeFromEnemyHealth();
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
        ResetRunStats();
        
        // Ensure UI matches the reset state
        ResetHUDUI();
        BindPlayerHealth();
        ScheduleRunTimerStart();
    }

    private void Start()
    {
        // Recalculate if not already done by OnSceneLoaded
        totalCoins = manualMaxCoins > 0 ? manualMaxCoins : GameObject.FindGameObjectsWithTag("Coin").Length;
        ResetRunStats();

        ResetHUDUI();
        BindPlayerHealth();
        ScheduleRunTimerStart();
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

        if (runTimerActive)
        {
            runElapsedSeconds += Time.deltaTime;
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
        HideFinalBossHp();
        ResetRunResultsUI();
        SetGameplayHudSuppressed(false);
        // Keys update is usually handled by GlobalQuestManager calling UpdateKeys
    }

    private void ResetRunResultsUI()
    {
        CacheRunResultsReferences();

        if (runResultsFadeRoutine != null)
        {
            StopCoroutine(runResultsFadeRoutine);
            runResultsFadeRoutine = null;
        }

        if (runResultsPanelCanvasGroup != null)
        {
            runResultsPanelCanvasGroup.alpha = 0f;
            runResultsPanelCanvasGroup.interactable = false;
            runResultsPanelCanvasGroup.blocksRaycasts = false;
            runResultsPanelCanvasGroup.gameObject.SetActive(false);
        }

        SetRunResultsCanvasPriority(false);
    }

    private void CacheRunResultsReferences()
    {
        if (runResultsPanelCanvasGroup == null)
        {
            Transform panel = FindChildByName(transform, "RunResultsPanel");
            if (panel != null)
            {
                runResultsPanelCanvasGroup = panel.GetComponent<CanvasGroup>();
            }
        }

        Transform runResultsPanel = runResultsPanelCanvasGroup != null
            ? runResultsPanelCanvasGroup.transform
            : FindChildByName(transform, "RunResultsPanel");

        if (runResultsPanel == null) return;

        if (runResultsContentPanel == null)
        {
            Transform contentPanel = FindChildByName(runResultsPanel, "ResultsPanel");
            if (contentPanel != null)
            {
                runResultsContentPanel = contentPanel.gameObject;
            }
        }

        if (runResultsSettingsPanel == null)
        {
            Transform settingsPanel = FindChildByName(runResultsPanel, "settings_panel");
            if (settingsPanel != null)
            {
                runResultsSettingsPanel = settingsPanel.gameObject;
            }
        }

        if (continueButton == null)
        {
            Transform button = FindChildByName(runResultsPanel, "Continue_bttn");
            if (button != null)
            {
                continueButton = button.gameObject;
            }
        }

        if (respawnButton == null)
        {
            Transform button = FindChildByName(runResultsPanel, "Respawn_bttn");
            if (button != null)
            {
                respawnButton = button.gameObject;
            }
        }

        if (exitButton == null)
        {
            Transform button = FindChildByName(runResultsPanel, "Exit_bttn");
            if (button != null)
            {
                exitButton = button.gameObject;
            }
        }

        BindRunResultsButton(continueButton, ref boundContinueButton, OnRunResultsContinuePressed);
        BindRunResultsButton(respawnButton, ref boundRespawnButton, OnRunResultsRespawnPressed);
        BindRunResultsButton(exitButton, ref boundExitButton, OnRunResultsExitPressed);
    }

    private void BindRunResultsButton(GameObject buttonObject, ref Button boundButton, UnityAction action)
    {
        if (buttonObject == null) return;

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"[DEBUG_LOG] HUDManager: {buttonObject.name} is assigned for RunResults but has no Button component.");
            return;
        }

        if (boundButton == button) return;

        if (boundButton != null)
        {
            boundButton.onClick.RemoveListener(action);
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
        boundButton = button;
    }

    private void CacheGameplayHudReferences()
    {
        if (hudItemsPanel == null)
        {
            Transform itemsPanel = FindChildByName(transform, "HUD_Items");
            if (itemsPanel != null)
            {
                hudItemsPanel = itemsPanel.gameObject;
            }
        }

        if (hudHpPanel == null || hudHpPanel.name != PlayerHpPanelName)
        {
            Transform hpPanel = FindChildByName(transform, PlayerHpPanelName);
            if (hpPanel != null)
            {
                hudHpPanel = hpPanel;
            }
        }

        if (finalBossHpPanel == null || finalBossHpPanel.name != FinalBossHpPanelName)
        {
            Transform bossHpPanel = FindChildByName(transform, FinalBossHpPanelName);
            if (bossHpPanel != null)
            {
                finalBossHpPanel = bossHpPanel;
            }
        }
    }

    private void ResetRunStats()
    {
        StopRunTimer();
        runElapsedSeconds = 0f;
        enemiesKilled = 0;
        UnsubscribeFromEnemyHealth();
        CacheActiveEnemies();
    }

    private void CacheActiveEnemies()
    {
        totalActiveEnemies = 0;
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            Health health = enemy.GetComponent<Health>();
            if (health == null) health = enemy.GetComponentInParent<Health>();
            if (health == null || health.IsDead) continue;

            totalActiveEnemies++;
            trackedEnemyHealth.Add(health);
            health.OnDied += OnTrackedEnemyDied;
        }
    }

    private void UnsubscribeFromEnemyHealth()
    {
        foreach (Health health in trackedEnemyHealth)
        {
            if (health != null)
            {
                health.OnDied -= OnTrackedEnemyDied;
            }
        }

        trackedEnemyHealth.Clear();
    }

    private void OnTrackedEnemyDied(Health health)
    {
        if (!trackedEnemyHealth.Contains(health)) return;

        enemiesKilled++;
        health.OnDied -= OnTrackedEnemyDied;
        trackedEnemyHealth.Remove(health);
    }

    private void ScheduleRunTimerStart()
    {
        if (runTimerStartRoutine != null)
        {
            StopCoroutine(runTimerStartRoutine);
        }

        runTimerStartRoutine = StartCoroutine(StartRunTimerWhenGameplayReady());
    }

    private IEnumerator StartRunTimerWhenGameplayReady()
    {
        yield return null;

        while (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning)
        {
            yield return null;
        }

        StartRunTimer();
        runTimerStartRoutine = null;
    }

    public void StartRunTimer()
    {
        runTimerActive = true;
    }

    public void StopRunTimer()
    {
        runTimerActive = false;
    }

    public RunStatsSnapshot CreateRunStatsSnapshot()
    {
        return new RunStatsSnapshot(
            SceneManager.GetActiveScene().name,
            coinsCollected,
            totalCoins,
            runElapsedSeconds,
            enemiesKilled,
            totalActiveEnemies);
    }

    public void ShowRunResults(RunResultsMode mode, RunStatsSnapshot stats)
    {
        StopRunTimer();
        CacheRunResultsReferences();

        if (runResultsPanelCanvasGroup == null)
        {
            Debug.LogWarning("[DEBUG_LOG] HUDManager: RunResultsPanel CanvasGroup missing. Results UI cannot be shown.");
            return;
        }

        SetRunResultsText(mode, stats);
        SetRunResultsButtons(mode);
        SetRunResultsPanelState();
        SetRunResultsCanvasPriority(true);
        SetGameplayHudSuppressed(true);
        UnlockCursorForRunResults();

        runResultsPanelCanvasGroup.gameObject.SetActive(true);
        runResultsPanelCanvasGroup.transform.SetAsLastSibling();
        runResultsPanelCanvasGroup.interactable = false;
        runResultsPanelCanvasGroup.blocksRaycasts = false;

        if (runResultsFadeRoutine != null)
        {
            StopCoroutine(runResultsFadeRoutine);
        }

        runResultsFadeRoutine = StartCoroutine(FadeRunResultsPanel(1f, true));
    }

    public void HideRunResultsImmediate()
    {
        CacheRunResultsReferences();
        if (runResultsFadeRoutine != null)
        {
            StopCoroutine(runResultsFadeRoutine);
            runResultsFadeRoutine = null;
        }

        if (runResultsPanelCanvasGroup != null)
        {
            runResultsPanelCanvasGroup.alpha = 0f;
            runResultsPanelCanvasGroup.interactable = false;
            runResultsPanelCanvasGroup.blocksRaycasts = false;
            runResultsPanelCanvasGroup.gameObject.SetActive(false);
        }

        SetRunResultsCanvasPriority(false);
        SetGameplayHudSuppressed(false);
        RestoreCursorAfterRunResults();
    }

    public void OnRunResultsContinuePressed()
    {
        Debug.Log("[DEBUG_LOG] HUDManager: RunResults Continue pressed.");
        SceneTransitionManager.Instance?.ContinueFromRunResults();
    }

    public void OnRunResultsRespawnPressed()
    {
        Debug.Log("[DEBUG_LOG] HUDManager: RunResults Respawn pressed.");
        SceneTransitionManager.Instance?.RespawnFromRunResults();
    }

    public void OnRunResultsExitPressed()
    {
        Debug.Log("[DEBUG_LOG] HUDManager: RunResults Exit pressed.");
        SceneTransitionManager.Instance?.ExitRunResultsToMainMenu();
    }

    private void SetRunResultsText(RunResultsMode mode, RunStatsSnapshot stats)
    {
        string resultWord = mode == RunResultsMode.Completion ? "Passed" : "Failed";

        if (runResultsTitleText != null)
        {
            runResultsTitleText.text = $"{stats.LevelName} {resultWord}";
        }

        if (runResultsCoinsText != null)
        {
            runResultsCoinsText.text = $"Coins: {stats.CoinsCollected} / {stats.TotalCoins}";
        }

        if (runResultsTimeText != null)
        {
            runResultsTimeText.text = $"Time: {FormatElapsedTime(stats.ElapsedSeconds)}";
        }

        if (runResultsEnemiesText != null)
        {
            runResultsEnemiesText.text = stats.TotalActiveEnemies > 0
                ? $"Enemies Killed: {stats.EnemiesKilled} / {stats.TotalActiveEnemies}"
                : "Enemies Killed: N/A";
        }
    }

    private void SetRunResultsButtons(RunResultsMode mode)
    {
        if (summaryBanner != null)
        {
            summaryBanner.SetActive(mode == RunResultsMode.Completion);
        }

        if (failedBanner != null)
        {
            failedBanner.SetActive(mode == RunResultsMode.Failure);
        }

        if (continueButton != null)
        {
            continueButton.SetActive(mode == RunResultsMode.Completion);
        }

        if (respawnButton != null)
        {
            respawnButton.SetActive(mode == RunResultsMode.Failure);
        }
    }

    private void SetRunResultsPanelState()
    {
        CacheRunResultsReferences();

        if (runResultsContentPanel != null)
        {
            runResultsContentPanel.SetActive(true);
            runResultsContentPanel.transform.SetAsLastSibling();
        }

        if (runResultsSettingsPanel != null)
        {
            runResultsSettingsPanel.SetActive(false);
        }
    }

    private IEnumerator FadeRunResultsPanel(float targetAlpha, bool keepActive)
    {
        float startAlpha = runResultsPanelCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < runResultsFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = runResultsFadeDuration > 0f ? elapsed / runResultsFadeDuration : 1f;
            runResultsPanelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        runResultsPanelCanvasGroup.alpha = targetAlpha;
        runResultsPanelCanvasGroup.interactable = targetAlpha > 0.99f;
        runResultsPanelCanvasGroup.blocksRaycasts = targetAlpha > 0.99f;

        if (!keepActive)
        {
            runResultsPanelCanvasGroup.gameObject.SetActive(false);
            SetRunResultsCanvasPriority(false);
        }

        runResultsFadeRoutine = null;
    }

    public void SetGameplayHudSuppressed(bool suppressed)
    {
        CacheGameplayHudReferences();

        GameObject hpPanelObject = hudHpPanel != null ? hudHpPanel.gameObject : null;
        GameObject finalBossHpPanelObject = finalBossHpPanel != null ? finalBossHpPanel.gameObject : null;

        if (suppressed)
        {
            if (!hasGameplayHudVisibilityBeforeModal)
            {
                hudItemsWasActiveBeforeModal = hudItemsPanel != null && hudItemsPanel.activeSelf;
                hudHpWasActiveBeforeModal = hpPanelObject != null && hpPanelObject.activeSelf;
                finalBossHpWasActiveBeforeModal = finalBossHpPanelObject != null && finalBossHpPanelObject.activeSelf;
                hasGameplayHudVisibilityBeforeModal = true;
            }

            if (hudItemsPanel != null)
            {
                hudItemsPanel.SetActive(false);
            }

            if (hpPanelObject != null)
            {
                hpPanelObject.SetActive(false);
            }

            if (finalBossHpPanelObject != null)
            {
                finalBossHpPanelObject.SetActive(false);
            }

            return;
        }

        if (!hasGameplayHudVisibilityBeforeModal) return;

        if (hudItemsPanel != null)
        {
            hudItemsPanel.SetActive(hudItemsWasActiveBeforeModal);
        }

        if (hpPanelObject != null)
        {
            hpPanelObject.SetActive(hudHpWasActiveBeforeModal);
        }

        if (finalBossHpPanelObject != null)
        {
            finalBossHpPanelObject.SetActive(finalBossHpShouldBeVisible && finalBossHpWasActiveBeforeModal);
        }

        hasGameplayHudVisibilityBeforeModal = false;
    }

    private void SetRunResultsCanvasPriority(bool showAboveFadeOverlay)
    {
        if (hudCanvas == null) return;

        hudCanvas.overrideSorting = showAboveFadeOverlay || defaultCanvasOverrideSorting;
        hudCanvas.sortingOrder = showAboveFadeOverlay ? runResultsCanvasSortingOrder : defaultCanvasSortingOrder;
    }

    private void UnlockCursorForRunResults()
    {
        if (!hasCursorStateBeforeRunResults)
        {
            cursorLockStateBeforeRunResults = Cursor.lockState;
            cursorVisibleBeforeRunResults = Cursor.visible;
            hasCursorStateBeforeRunResults = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestoreCursorAfterRunResults()
    {
        if (!hasCursorStateBeforeRunResults) return;

        Cursor.lockState = cursorLockStateBeforeRunResults;
        Cursor.visible = cursorVisibleBeforeRunResults;
        hasCursorStateBeforeRunResults = false;
    }

    private string FormatElapsedTime(float elapsedSeconds)
    {
        int totalSeconds = Mathf.FloorToInt(Mathf.Max(0f, elapsedSeconds));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    private void CacheHpImages()
    {
        hpImages.Clear();

        if (hudHpPanel == null || hudHpPanel.name != PlayerHpPanelName)
        {
            if (hudHpPanel != null)
            {
                Debug.LogWarning($"[DEBUG_LOG] HUDManager: hudHpPanel points to '{hudHpPanel.name}', expected {PlayerHpPanelName}. Rebinding by name.");
            }

            hudHpPanel = FindChildByName(transform, PlayerHpPanelName);
        }

        if (hudHpPanel == null)
        {
            Debug.LogWarning($"[DEBUG_LOG] HUDManager: {PlayerHpPanelName} panel reference missing. Player HP UI will not update.");
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
            Debug.LogWarning($"[DEBUG_LOG] HUDManager: {PlayerHpPanelName} has no child Image components.");
        }
    }

    private bool CacheFinalBossHpImages(bool createIfMissing)
    {
        finalBossHpImages.Clear();

        if (finalBossHpPanel == null || finalBossHpPanel.name != FinalBossHpPanelName)
        {
            finalBossHpPanel = FindChildByName(transform, FinalBossHpPanelName);
        }

        if (finalBossHpPanel == null && createIfMissing)
        {
            finalBossHpPanel = CreateRuntimeFinalBossHpPanel();
        }

        if (finalBossHpPanel == null)
        {
            return false;
        }

        foreach (Transform child in finalBossHpPanel)
        {
            if (!child.TryGetComponent(out Image image)) continue;

            ApplyFinalBossHpSprite(image);
            finalBossHpImages.Add(image);
        }

        finalBossHpImages.Sort(CompareHpImagesByPosition);
        return true;
    }

    private Transform CreateRuntimeFinalBossHpPanel()
    {
        CacheHpImages();

        if (hudHpPanel == null)
        {
            Debug.LogWarning($"[DEBUG_LOG] HUDManager: Cannot create {FinalBossHpPanelName}; {PlayerHpPanelName} is missing.");
            return null;
        }

        Transform parent = hudHpPanel.parent != null ? hudHpPanel.parent : transform;
        GameObject panelObject = new GameObject(FinalBossHpPanelName, typeof(RectTransform));
        panelObject.layer = hudHpPanel.gameObject.layer;
        panelObject.transform.SetParent(parent, false);
        panelObject.transform.SetSiblingIndex(Mathf.Min(hudHpPanel.GetSiblingIndex() + RuntimeFinalBossHpSortingOffset, parent.childCount - 1));

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        RectTransform sourceRect = hudHpPanel as RectTransform;
        if (sourceRect != null)
        {
            panelRect.anchorMin = sourceRect.anchorMin;
            panelRect.anchorMax = sourceRect.anchorMax;
            panelRect.pivot = sourceRect.pivot;
            panelRect.anchoredPosition = sourceRect.anchoredPosition;
            panelRect.sizeDelta = sourceRect.sizeDelta;
            panelRect.localScale = sourceRect.localScale;
            panelRect.localRotation = sourceRect.localRotation;
        }

        panelObject.SetActive(false);
        return panelObject.transform;
    }

    private void EnsureFinalBossHpImageCount(int maxHealth)
    {
        if (finalBossHpPanel == null) return;
        maxHealth = Mathf.Max(0, maxHealth);

        if (finalBossHpImages.Count != maxHealth)
        {
            RebuildRuntimeFinalBossHpImages(maxHealth);
        }

        LayoutFinalBossHpImages(finalBossHpImages.ToArray());
        finalBossHpImages.Sort(CompareHpImagesByPosition);
    }

    private void RebuildRuntimeFinalBossHpImages(int maxHealth)
    {
        if (finalBossHpPanel == null) return;

        Image template = GetFinalBossHpTemplate();
        if (template == null)
        {
            Debug.LogWarning($"[DEBUG_LOG] HUDManager: Cannot create {FinalBossHpPanelName} images; {PlayerHpPanelName} has no HP image template.");
            return;
        }

        ClearFinalBossHpChildren();

        for (int i = 0; i < maxHealth; i++)
        {
            Image image = CreateFinalBossHpImage(template, i);
            if (image == null) continue;
            finalBossHpImages.Add(image);
        }
    }

    private Image GetFinalBossHpTemplate()
    {
        if (hpImages.Count == 0)
        {
            CacheHpImages();
        }

        return hpImages.Count > 0 ? hpImages[0] : null;
    }

    private void ClearFinalBossHpChildren()
    {
        finalBossHpImages.Clear();

        if (finalBossHpPanel == null) return;

        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in finalBossHpPanel)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            if (child == null) continue;
            Destroy(child);
        }
    }

    private Image CreateFinalBossHpImage(Image template, int index)
    {
        GameObject hpObject = new GameObject($"FinalBoss HP ({index})", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        hpObject.layer = template.gameObject.layer;
        hpObject.transform.SetParent(finalBossHpPanel, false);

        RectTransform sourceRect = template.rectTransform;
        RectTransform rectTransform = hpObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = sourceRect.anchorMin;
        rectTransform.anchorMax = sourceRect.anchorMax;
        rectTransform.pivot = sourceRect.pivot;
        rectTransform.sizeDelta = sourceRect.sizeDelta;
        rectTransform.localScale = sourceRect.localScale;
        rectTransform.localRotation = sourceRect.localRotation;

        Image image = hpObject.GetComponent<Image>();
        image.sprite = finalBossHpSprite != null ? finalBossHpSprite : template.sprite;
        image.color = template.color;
        image.material = template.material;
        image.type = template.type;
        image.preserveAspect = true;
        image.fillCenter = template.fillCenter;
        image.raycastTarget = false;
        image.enabled = false;

        return image;
    }

    private void LayoutFinalBossHpImages(Image[] images)
    {
        List<Image> layoutImages = new List<Image>();
        foreach (Image image in images)
        {
            if (image == null || image.transform == finalBossHpPanel) continue;
            layoutImages.Add(image);
        }

        layoutImages.Sort(CompareHpImagesByPosition);
        for (int i = 0; i < layoutImages.Count; i++)
        {
            RectTransform rectTransform = layoutImages[i].rectTransform;
            int row = i / FinalBossHpImagesPerRow;
            int column = i % FinalBossHpImagesPerRow;
            rectTransform.anchoredPosition = new Vector2(
                FinalBossHpStartX - column * FinalBossHpSpacing,
                FinalBossHpStartY - row * FinalBossHpSpacing);
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

    public void ShowFinalBossHp(Health bossHealth)
    {
        if (bossHealth == null || bossHealth.IsDead)
        {
            HideFinalBossHp();
            return;
        }

        if (!CacheFinalBossHpImages(true)) return;

        if (subscribedFinalBossHealth != bossHealth)
        {
            UnsubscribeFromFinalBossHealth();
            subscribedFinalBossHealth = bossHealth;
            subscribedFinalBossHealth.OnHealthChanged += UpdateFinalBossHp;
            subscribedFinalBossHealth.OnDied += OnFinalBossDied;
        }

        EnsureFinalBossHpImageCount(bossHealth.maxHealth);
        LayoutFinalBossHpImages(finalBossHpImages.ToArray());
        finalBossHpImages.Sort(CompareHpImagesByPosition);
        ApplyFinalBossHpSprites();
        finalBossHpShouldBeVisible = true;
        SetFinalBossHpPanelActive(true);
        UpdateFinalBossHp(bossHealth.currentHealth, bossHealth.maxHealth);
    }

    public void HideFinalBossHp()
    {
        finalBossHpShouldBeVisible = false;
        UnsubscribeFromFinalBossHealth();
        SetFinalBossHpPanelActive(false);
        lastDisplayedFinalBossHealth = -1;
        lastDisplayedFinalBossMaxHealth = -1;
    }

    private void OnFinalBossDied(Health bossHealth)
    {
        HideFinalBossHp();
    }

    private void UnsubscribeFromFinalBossHealth()
    {
        if (subscribedFinalBossHealth == null) return;

        subscribedFinalBossHealth.OnHealthChanged -= UpdateFinalBossHp;
        subscribedFinalBossHealth.OnDied -= OnFinalBossDied;
        subscribedFinalBossHealth = null;
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

    private void UpdateFinalBossHp(int currentHealth, int maxHealth)
    {
        if (!CacheFinalBossHpImages(true)) return;

        EnsureFinalBossHpImageCount(maxHealth);
        LayoutFinalBossHpImages(finalBossHpImages.ToArray());
        finalBossHpImages.Sort(CompareHpImagesByPosition);
        ApplyFinalBossHpSprites();

        if (finalBossHpImages.Count < maxHealth)
        {
            Debug.LogWarning($"[DEBUG_LOG] HUDManager: {FinalBossHpPanelName} has {finalBossHpImages.Count} images for max health {maxHealth}.");
        }

        int visibleCount = Mathf.Clamp(currentHealth, 0, Mathf.Min(maxHealth, finalBossHpImages.Count));
        SetFinalBossHpImagesVisible(visibleCount);
        lastDisplayedFinalBossHealth = currentHealth;
        lastDisplayedFinalBossMaxHealth = maxHealth;
    }

    private void SetHpImagesVisible(int visibleCount)
    {
        for (int i = 0; i < hpImages.Count; i++)
        {
            hpImages[i].enabled = i < visibleCount;
        }
    }

    private void SetFinalBossHpImagesVisible(int visibleCount)
    {
        for (int i = 0; i < finalBossHpImages.Count; i++)
        {
            bool visible = i < visibleCount;
            finalBossHpImages[i].enabled = visible;
            finalBossHpImages[i].gameObject.SetActive(visible);
        }
    }

    private void ApplyFinalBossHpSprites()
    {
        foreach (Image image in finalBossHpImages)
        {
            ApplyFinalBossHpSprite(image);
        }
    }

    private void ApplyFinalBossHpSprite(Image image)
    {
        if (image == null || finalBossHpSprite == null) return;

        image.sprite = finalBossHpSprite;
        image.preserveAspect = true;
    }

    private void SetFinalBossHpPanelActive(bool active)
    {
        if (finalBossHpPanel != null)
        {
            finalBossHpPanel.gameObject.SetActive(active);
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
