using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Complete shop system with auto-closing popups.
/// Both BUY PANEL and SUCCESS PANEL close after 3 seconds and return to shop.
/// Attach this to each Bundle.
/// </summary>
public class ShopItemBuyer : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemID = "bundle_1";
    public string itemName = "Bundle 1";
    public int itemPrice = 2500;
    public int costumeIndex = 0;
    public bool isFreeItem = false;
    
    [Header("UI References - Assign These!")]
    public Text priceText;
    public Button buyButton;
    public Text buttonText;
    public GameObject lockIcon;
    public Image priceBackground;
    
    [Header("Popup References")]
    public GameObject buyPopup;              // "Are you sure?" popup (BUYPANEL2)
    public GameObject successPopup;          // "Purchase Complete!" popup (SUCCESSFUL PANEL)
    public Text successMessageText;          // Text in success popup
    public Button successOkButton;           // OK button in success popup
    public GameObject shopPanel;             // Main SHOP PANEL to return to
    
    [Header("Auto-Close Settings")]
    public float autoCloseDelay = 3f;        // Seconds before auto-close (default 3)
    
    // Item state
    private bool isPurchased = false;
    private bool isEquipped = false;
    
    // Coroutines
    private Coroutine autoCloseCoroutine;
    
    void Start()
    {   
        // Setup buy button
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnButtonClicked);
        }
        
        // Setup success OK button (if you have it)
        if (successOkButton != null)
        {
            successOkButton.onClick.AddListener(CloseAllPopups);
        }
        
        LoadItemState();
        UpdateUI();
    }
    
    void LoadItemState()
    {
        if (isFreeItem)
        {
            isPurchased = true;
            PlayerPrefs.SetInt("Item_" + itemID + "_Purchased", 1);
        }
        else
        {
            isPurchased = PlayerPrefs.GetInt("Item_" + itemID + "_Purchased", 0) == 1;
        }
        
        int equippedCostume = PlayerPrefs.GetInt("EquippedCostume", 0);
        isEquipped = (equippedCostume == costumeIndex);
    }
    
    void OnButtonClicked()
    {
        if (isPurchased)
        {
            // Already owned - equip it
            EquipItem();
        }
        else
        {
            // Not owned - show buy popup
            if (buyPopup != null)
            {
                buyPopup.SetActive(true);
            }
            else
            {
                // No popup - buy directly
                TryPurchase();
            }
        }
    }
    
    /// <summary>
    /// PUBLIC method - can be called by popup's BUY button
    /// </summary>
    public void TryPurchase()
    {
        if (CoinManager.Instance == null)
        {
            Debug.LogError("❌ CoinManager.Instance not found!");
            return;
        }

        Debug.Log("🟡 Attempting purchase: " + itemName);
        Debug.Log("💰 Coins BEFORE: " + CoinManager.Instance.GetCoins());

        if (CoinManager.Instance.HasEnoughCoins(itemPrice))
        {
            bool success = CoinManager.Instance.SpendCoins(itemPrice);

            Debug.Log("💸 SpendCoins() returned: " + success);
            Debug.Log("💰 Coins AFTER: " + CoinManager.Instance.GetCoins());

            if (success)
            {
                // Purchase successful!
                isPurchased = true;
                PlayerPrefs.SetInt("Item_" + itemID + "_Purchased", 1);
                PlayerPrefs.Save();

                // Close buy popup IMMEDIATELY
                if (buyPopup != null)
                {
                    buyPopup.SetActive(false);
                }

                // Equip the item
                EquipItem();
                RefreshAllItemsUI();

                // Show SUCCESS popup with AUTO-CLOSE! 🎉
                ShowSuccessPopup();

                Debug.Log($"🎉 Purchase complete: {itemName}!");
            }
        }
        else
        {
            int currentCoins = CoinManager.Instance.GetCoins();
            int coinsNeeded = itemPrice - currentCoins;
            Debug.Log($"❌ Not enough coins! Have {currentCoins}, need {itemPrice}");
        }
    }

    /// <summary>
    /// Show the success popup with auto-close timer
    /// </summary>
    void ShowSuccessPopup()
    {
        if (successPopup != null)
        {
            // Show success popup
            successPopup.SetActive(true);
            
            // Update message text
            if (successMessageText != null)
            {
                successMessageText.text = "PURCHASE COMPLETE!\nYou got: " + itemName;
            }
            
            // Start auto-close timer (3 seconds)
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
            }
            autoCloseCoroutine = StartCoroutine(AutoClosePopups());
            
            Debug.Log($"✅ Success popup shown - will auto-close in {autoCloseDelay} seconds");
        }
    }

    /// <summary>
    /// Automatically close all popups after delay and return to shop
    /// </summary>
    IEnumerator AutoClosePopups()
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(autoCloseDelay);
        
        // Close all popups and return to shop
        CloseAllPopups();
        
        Debug.Log("⏰ Popups auto-closed - Returned to shop");
    }

    /// <summary>
    /// Close all popups and return to shop
    /// Called by OK button OR auto-close timer
    /// </summary>
    void CloseAllPopups()
    {
        // Stop auto-close timer if running
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        // Close BUY popup (just in case)
        if (buyPopup != null)
        {
            buyPopup.SetActive(false);
        }
        
        // Close SUCCESS popup
        if (successPopup != null)
        {
            successPopup.SetActive(false);
        }
        
        // Return to shop
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
        
        Debug.Log("✅ All popups closed - Back to shop!");
    }

    void EquipItem()
    {
        if (!isPurchased)
        {
            Debug.LogWarning("Cannot equip unpurchased item!");
            return;
        }

        ShopItemBuyer[] allItems = FindObjectsOfType<ShopItemBuyer>();
        foreach (ShopItemBuyer item in allItems)
        {
            item.isEquipped = false;
        }

        isEquipped = true;
        PlayerPrefs.SetInt("EquippedCostume", costumeIndex);
        PlayerPrefs.Save();

        RefreshAllItemsUI();

        Debug.Log($"👕 Equipped {itemName}!");
    }
    
    void UpdateUI()
    {
        if (isPurchased)
        {
            UpdateUIForOwnedItem();
        }
        else
        {
            UpdateUIForLockedItem();
        }
    }
    
    void UpdateUIForOwnedItem()
{
    // Show "OWNED" text where the price was
    if (priceText != null)
    {
        priceText.text = "OWNED";
        priceText.color = Color.white;  // White text
        priceText.gameObject.SetActive(true);  // Keep it visible!
    }
    
    // Change background to gray to show it's owned
    if (priceBackground != null)
    {   
        priceBackground.gameObject.SetActive(false);
        Image img = priceBackground.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(0.6f, 0.6f, 0.6f);  // Gray background
        }
        priceBackground.gameObject.SetActive(true);  // Keep it visible!
    }
    
    // Hide lock icon
    if (lockIcon != null)
    {
        lockIcon.SetActive(false);
    }

    // Make button gray, unclickable, and show white text
    if (buyButton != null && buttonText != null)
    {
        buttonText.text = "OWNED";
        buttonText.color = Color.white;  // ← THIS IS IMPORTANT! White text
        buyButton.interactable = false;  // Can't click it
        
        // Gray button
        ColorBlock colors = buyButton.colors;
        colors.normalColor = new Color(0.5f, 0.5f, 0.5f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f);
        colors.pressedColor = new Color(0.5f, 0.5f, 0.5f);
        buyButton.colors = colors;
    }
}

    
    void UpdateUIForLockedItem()
    {
        if (priceText != null)
        {
            priceText.text = itemPrice.ToString();
            priceText.color = Color.black;
            priceText.gameObject.SetActive(true);
        }
        
        if (priceBackground != null)
        {
            priceBackground.gameObject.SetActive(true);
        }
        
        if (lockIcon != null)
        {
            lockIcon.SetActive(true);
        }
        
        if (buyButton != null && buttonText != null)
        {
            buttonText.text = "BUY";
            buyButton.interactable = true;
            
            ColorBlock colors = buyButton.colors;
            colors.normalColor = new Color(0.3f, 0.8f, 0.3f);
            colors.highlightedColor = new Color(0.25f, 0.7f, 0.25f);
            colors.pressedColor = new Color(0.2f, 0.6f, 0.2f);
            buyButton.colors = colors;
        }
    }
    
    public bool IsPurchased()
    {
        return isPurchased;
    }
    
    public bool IsEquipped()
    {
        return isEquipped;
    }
    
    void RefreshAllItemsUI()
    {
        ShopItemBuyer[] allItems = FindObjectsOfType<ShopItemBuyer>();
        foreach (ShopItemBuyer item in allItems)
        {
            item.LoadItemState();
            item.UpdateUI();
        }
    }
    
    // Clean up coroutines when object is destroyed
    void OnDestroy()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
        }
    }
}