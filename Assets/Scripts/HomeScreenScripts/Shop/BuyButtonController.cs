using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BuyButtonController : MonoBehaviour
{
    [Header("UI References")]
    public Button buyButton;
    public TMP_Text priceText; // Shows the cost (e.g., "500")
    public CoinsDisplay coinsDisplay; // Reference to your CoinsDisplay script

    [Header("Purchase Settings")]
    public int itemCost = 500; // Cost of the item
    public string coinSaveKey = "PlayerTotalCoins"; // Must match CoinsDisplay
    public string purchaseKey = ""; // Unique key to track if this item was purchased (set in Inspector)

    [Header("Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 10f;
    public int shakeCount = 3;

    [Header("Visual Feedback")]
    public Color canAffordColor = Color.green;
    public Color cannotAffordColor = Color.red;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool isShaking = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
    }

    void Start()
    {
        originalPosition = rectTransform.anchoredPosition;
        UpdatePriceDisplay();

        // Check if already purchased
        CheckIfPurchased();

        // Only check affordability if button is still active
        if (buyButton != null && buyButton.interactable)
        {
            CheckAffordability();
        }
    }

    /// <summary>
    /// Checks if this item was already purchased and disables button if so
    /// </summary>
    void CheckIfPurchased()
    {
        if (string.IsNullOrEmpty(purchaseKey))
        {
            Debug.LogWarning("⚠️ Purchase Key is empty! Set a unique key for this item in the Inspector.");
            return;
        }

        bool alreadyPurchased = PlayerPrefs.GetInt(purchaseKey, 0) == 1;

        if (alreadyPurchased)
        {
            DisableBuyButton();
            Debug.Log($"✅ Item already purchased: {purchaseKey}");
        }
    }

    void UpdatePriceDisplay()
    {
        if (priceText != null)
        {
            priceText.text = FormatPrice(itemCost);
        }
    }

    /// <summary>
    /// Checks if player can afford the item and updates button color
    /// </summary>
    public void CheckAffordability()
    {
        int currentCoins = PlayerPrefs.GetInt(coinSaveKey, 0);
        bool canAfford = currentCoins >= itemCost;

        // Optional: Change price text color based on affordability
        if (priceText != null)
        {
            priceText.color = canAfford ? canAffordColor : cannotAffordColor;
        }
    }

    /// <summary>
    /// Called when buy button is clicked
    /// </summary>
    void OnBuyButtonClicked()
    {
        int currentCoins = PlayerPrefs.GetInt(coinSaveKey, 0);

        if (currentCoins >= itemCost)
        {
            // Player can afford - proceed with purchase
            ProcessPurchase(currentCoins);
        }
        else
        {
            // Not enough coins - shake the button
            if (!isShaking)
            {
                StartCoroutine(ShakeButton());
            }
        }
    }

    /// <summary>
    /// Process the purchase and deduct coins
    /// </summary>
    void ProcessPurchase(int currentCoins)
    {
        // Deduct coins
        int newCoins = currentCoins - itemCost;
        PlayerPrefs.SetInt(coinSaveKey, newCoins);

        // Mark item as purchased
        if (!string.IsNullOrEmpty(purchaseKey))
        {
            PlayerPrefs.SetInt(purchaseKey, 1);
        }

        PlayerPrefs.Save();

        Debug.Log($"💰 Purchase successful! Spent {itemCost} coins. Remaining: {newCoins}");

        // Refresh the coin display
        if (coinsDisplay != null)
        {
            coinsDisplay.RefreshCoinDisplay();
        }
        // ✅ Notify UI buttons to refresh their state
        foreach (var selector in FindObjectsOfType<UIButtonSelector>())
        {
            selector.CheckPurchaseStatus();
        }

        // Disable the buy button after purchase
        DisableBuyButton();

        // Add your unlock logic here
        // Example: UnlockItem();
    }

    /// <summary>
    /// Disables the buy button and shows "Purchased"
    /// </summary>
    void DisableBuyButton()
    {
        if (buyButton != null)
        {
            buyButton.interactable = false;
        }

        // Change text to show it's purchased
        if (priceText != null)
        {
            priceText.text = "PURCHASED";
            priceText.color = Color.gray;
        }
    }

    /// <summary>
    /// Shakes the buy button when player can't afford
    /// </summary>
    IEnumerator ShakeButton()
    {
        isShaking = true;

        float shakeDurationPerShake = shakeDuration / shakeCount;

        for (int i = 0; i < shakeCount; i++)
        {
            float shakeElapsed = 0f;

            while (shakeElapsed < shakeDurationPerShake)
            {
                float x = originalPosition.x + Random.Range(-shakeMagnitude, shakeMagnitude);
                float y = originalPosition.y + Random.Range(-shakeMagnitude, shakeMagnitude);

                rectTransform.anchoredPosition = new Vector2(x, y);

                shakeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Return to original position
        rectTransform.anchoredPosition = originalPosition;
        isShaking = false;
    }

    /// <summary>
    /// Formats price with k/M/B suffixes (same as CoinsDisplay)
    /// </summary>
    string FormatPrice(int price)
    {
        if (price < 1000)
            return price.ToString();
        else if (price < 1000000)
        {
            float thousands = price / 1000f;
            return $"{thousands:F1}k".Replace(".0", "");
        }
        else if (price < 1000000000)
        {
            float millions = price / 1000000f;
            return $"{millions:F1}M".Replace(".0", "");
        }
        else
        {
            float billions = price / 1000000000f;
            return $"{billions:F1}B".Replace(".0", "");
        }
    }

    /// <summary>
    /// Public method to set item cost (useful for different items)
    /// </summary>
    public void SetItemCost(int cost)
    {
        itemCost = cost;
        UpdatePriceDisplay();
        CheckAffordability();
    }

    void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnBuyButtonClicked);
        }
    }
}