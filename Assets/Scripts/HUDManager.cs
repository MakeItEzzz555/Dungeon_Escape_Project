using UnityEngine;
using UnityEngine.UI; // Added for Image references
using TMPro; 

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("UI Text References")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI keyText;

    [Header("UI Image References")]
    public Image coinIcon;
    public Image keyIcon;

    [Header("Collection Settings")]
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
        
        UpdateUI();
    }

    public void RegisterCoinCollection()
    {
        coinsCollected++;
        UpdateUI();
    }

    public void UpdateKeyText(int current, int required)
    {
        if (keyText != null)
        {
            keyText.text = $"Keys: {current} / {required}";
        }
    }

    // Keep UpdateUI for internal use if needed (e.g. for coins which aren't in GlobalQuestManager yet)
    private void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = $"Coins: {coinsCollected} / {totalCoins}";
        }
    }
}
