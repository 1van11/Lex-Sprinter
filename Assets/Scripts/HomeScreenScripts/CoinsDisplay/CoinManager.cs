using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;
    
    [Header("Coin Settings")]
    private int currentCoins = 0;
    
    [Header("References")]
    public CoinsDisplay coinsDisplay; // Link to your CoinsDisplay script
    
    private const string COIN_SAVE_KEY = "PlayerTotalCoins"; // Must match your CoinsDisplay
    
    void Awake()
    {
        // Singleton pattern
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
    }
    
    // Add coins
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        SaveCoins();
        RefreshUI();
        
        Debug.Log("💰 Added " + amount + " coins. Total: " + currentCoins);
    }
    
    // Remove/Spend coins
    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            SaveCoins();
            RefreshUI();
            
            Debug.Log("💸 Spent " + amount + " coins. Remaining: " + currentCoins);
            return true;
        }
        else
        {
            Debug.Log("❌ Not enough coins! Need: " + amount + ", Have: " + currentCoins);
            return false;
        }
    }
    
    // Check if player has enough coins
    public bool HasEnoughCoins(int amount)
    {
        return currentCoins >= amount;
    }
    
    // Get current coin count
    public int GetCoins()
    {
        return currentCoins;
    }
    
    // Set coins to specific amount
    public void SetCoins(int amount)
    {
        currentCoins = amount;
        SaveCoins();
        RefreshUI();
    }
    
    // Save coins to PlayerPrefs
    public void SaveCoins()
    {
        PlayerPrefs.SetInt(COIN_SAVE_KEY, currentCoins);
        PlayerPrefs.Save();
        
        Debug.Log("💾 Coins saved: " + currentCoins);
    }
    
    // Load coins from PlayerPrefs
    public void LoadCoins()
    {
        if (PlayerPrefs.HasKey(COIN_SAVE_KEY))
        {
            currentCoins = PlayerPrefs.GetInt(COIN_SAVE_KEY);
            Debug.Log("📂 Coins loaded: " + currentCoins);
        }
        else
        {
            currentCoins = 0;
            Debug.Log("🆕 No saved coins found. Starting with 0");
        }
        
        RefreshUI();
    }
    
    // Refresh the coin UI display
    private void RefreshUI()
    {
        if (coinsDisplay != null)
        {
            coinsDisplay.RefreshCoinDisplay();
        }
    }
    
    // Reset coins (for testing or new game)
    public void ResetCoins()
    {
        currentCoins = 0;
        SaveCoins();
        RefreshUI();
        
        Debug.Log("🔄 Coins reset to 0");
    }
    
    // TESTING FUNCTION - Add coins by pressing C key
    void Update()
    {
        // Press C to add 10 coins (for testing)
        if (Input.GetKeyDown(KeyCode.C))
        {
            AddCoins(1000);
            Debug.Log("🎮 TEST: Added 10 coins");
        }
        
        // Press R to reset coins (for testing)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCoins();
            Debug.Log("🎮 TEST: Reset coins");
        }
    }
}