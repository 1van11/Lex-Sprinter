using UnityEngine;
using TMPro;
using System.Collections;

public class CoinsDisplay : MonoBehaviour
{
    public static CoinsDisplay Instance; // Singleton for easy access
    
    [Header("UI Reference")]
    public TMP_Text coinText; // Assign your TextMeshProUGUI here

    [Header("Coin Settings")]
    public string coinSaveKey = "PlayerTotalCoins"; // Must match PlayerFunctions
    public float animationDuration = 1.2f; // How long the number animates
    public bool animateOnStart = true;

    [Header("Inspector Debug Tools")]
    public int inspectorCoinInput = 20000; // Enter value in Inspector

    [Header("Auto-Save Settings")]
    public bool enableAutoSave = true;
    public float autoSaveInterval = 5f; // Save every 5 seconds

    private int totalCoins;
    private float autoSaveTimer = 0f;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
{
    LoadCoins();

    // If no coins saved yet AND inspector has a value, use inspector value
    if (totalCoins == 0 && inspectorCoinInput > 0)
    {
        Debug.Log($"🆕 First launch! Setting starting coins to {inspectorCoinInput}");
        totalCoins = inspectorCoinInput;
        SaveCoins();
    }

    if (animateOnStart)
        StartCoroutine(AnimateCoinCount(0, totalCoins));
    else
        UpdateCoinUI(totalCoins);
    
    Debug.Log($"💰 CoinsDisplay loaded: {totalCoins} coins");
}


    /// <summary>
    /// Loads saved total coins from PlayerPrefs.
    /// </summary>
    void LoadCoins()
    {
        totalCoins = PlayerPrefs.GetInt(coinSaveKey, 0);
        
    }

    /// <summary>
    /// Saves current coins to PlayerPrefs.
    /// </summary>
    void SaveCoins()
    {
        PlayerPrefs.SetInt(coinSaveKey, totalCoins);
        PlayerPrefs.Save(); // Force immediate save to disk
        
    }
    
    /// <summary>
    /// Save coins when application quits or pauses (for mobile).
    /// </summary>
    void OnApplicationQuit()
    {
        SaveCoins();
        
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCoins();
            
        }
    }
    
    /// <summary>
    /// Save coins when disabled (safety backup).
    /// </summary>
    void OnDisable()
    {
        SaveCoins();
        
    }
    // DISABLED FOR PRODUCTION - Causes coins to constantly reset
/*
// DISABLED - Only use inspector value on FIRST launch, not every frame
// This was causing constant resets!
/*
void OnValidate()
{
    if (Application.isPlaying && inspectorCoinInput >= 0)
    {
        totalCoins = inspectorCoinInput;
        UpdateCoinUI(totalCoins);
        SaveCoins();
    }
}
*/


    /// <summary>
    /// Add coins and save immediately.
    /// </summary>
    public void AddCoins(int amount)
    {
        int oldCoins = totalCoins;
        totalCoins += amount;
        SaveCoins();
        
        StopAllCoroutines();
        StartCoroutine(AnimateCoinCount(oldCoins, totalCoins));
        
        
    }

    /// <summary>
    /// Adds the value entered in the Inspector.
    /// </summary>
    public void AddInspectorCoins()
    {
        AddCoins(inspectorCoinInput);
        Debug.Log($"🟢 Added {inspectorCoinInput} coins from Inspector.");
    }
    /// <summary>
    /// Sets coins to the value entered in the Inspector.
    /// </summary>
    public void SetInspectorCoins()
    {
        SetCoins(inspectorCoinInput);
        Debug.Log($"🔵 Coins set to {inspectorCoinInput} from Inspector.");
    }

    /// <summary>
    /// Spend coins if player has enough. Returns true if successful.
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            int oldCoins = totalCoins;
            totalCoins -= amount;
            SaveCoins();
            
            StopAllCoroutines();
            StartCoroutine(AnimateCoinCount(oldCoins, totalCoins));
            
            Debug.Log($"💸 Spent {amount} coins. Remaining: {totalCoins}");
            return true;
        }
        else
        {
            Debug.Log($"❌ Not enough coins! Need: {amount}, Have: {totalCoins}");
            return false;
        }
    }

    /// <summary>
    /// Check if player has enough coins.
    /// </summary>
    public bool HasEnoughCoins(int amount)
    {
        return totalCoins >= amount;
    }

    /// <summary>
    /// Get current coin count.
    /// </summary>
    public int GetCoins()
    {
        return totalCoins;
    }

    /// <summary>
    /// Set coins to a specific amount.
    /// </summary>
    public void SetCoins(int amount)
    {
        int oldCoins = totalCoins;
        totalCoins = amount;
        SaveCoins();
        
        StopAllCoroutines();
        StartCoroutine(AnimateCoinCount(oldCoins, totalCoins));
    }

    /// <summary>
    /// Reset coins to 0.
    /// </summary>
    public void ResetCoins()
    {
        SetCoins(0);
    
    }

    /// <summary>
    /// Instantly updates the UI to show the given amount.
    /// </summary>
    void UpdateCoinUI(int value)
    {
        if (coinText != null)
            coinText.text = FormatNumber(value);
    }

    /// <summary>
    /// Refreshes the coin count and animates from the old to the new value.
    /// </summary>
    public void RefreshCoinDisplay()
    {
        int oldCoins = totalCoins;
        LoadCoins();
        StopAllCoroutines();
        StartCoroutine(AnimateCoinCount(oldCoins, totalCoins));
    }

    /// <summary>
    /// Smoothly animates the coin counter from start to end.
    /// </summary>
    IEnumerator AnimateCoinCount(int startValue, int endValue)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            UpdateCoinUI(currentValue);
            yield return null;
        }

        UpdateCoinUI(endValue);
    }

    /// <summary>
    /// Formats numbers like 1000 → 1k, 1500 → 1.5k, 1000000 → 1M, etc.
    /// </summary>
    string FormatNumber(int number)
    {
        if (number < 1000)
            return number.ToString();
        else if (number < 1000000)
        {
            float thousands = number / 1000f;
            return $"{thousands:F1}k".Replace(".0", "");
        }
        else if (number < 1000000000)
        {
            float millions = number / 1000000f;
            return $"{millions:F1}M".Replace(".0", "");
        }
        else
        {
            float billions = number / 1000000000f;
            return $"{billions:F1}B".Replace(".0", "");
        }
    }

    // TESTING - Remove this in production
    void Update()
    {
        // Auto-save coins periodically
        if (enableAutoSave)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                SaveCoins();
                autoSaveTimer = 0f;
            }
        }
    }
}