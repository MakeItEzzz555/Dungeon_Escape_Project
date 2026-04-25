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

    private int coinsCollected = 0;
    private int totalCoins = 0;
    private int keysCollected = 0;
    private int totalKeys = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Automatically count how many coins and keys are in the scene at the start
        totalCoins = GameObject.FindGameObjectsWithTag("Coin").Length;
        totalKeys = GameObject.FindGameObjectsWithTag("Key").Length;
        
        UpdateUI();
    }

    public void RegisterCoinCollection()
    {
        coinsCollected++;
        UpdateUI();
    }

    public void RegisterKeyCollection()
    {
        keysCollected++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = $"Coins: {coinsCollected} / {totalCoins}";
        }

        if (keyText != null)
        {
            keyText.text = $"Keys: {keysCollected} / {totalKeys}";
        }
    }
}
