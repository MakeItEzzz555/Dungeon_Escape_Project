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
    [Tooltip("If set to 0, it will automatically count objects with the 'Key' tag in the scene.")]
    public int manualMaxKeys = 0;

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
        // Use manual values if provided, otherwise count how many coins and keys are in the scene
        totalCoins = manualMaxCoins > 0 ? manualMaxCoins : GameObject.FindGameObjectsWithTag("Coin").Length;
        totalKeys = manualMaxKeys > 0 ? manualMaxKeys : GameObject.FindGameObjectsWithTag("Key").Length;
        
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
