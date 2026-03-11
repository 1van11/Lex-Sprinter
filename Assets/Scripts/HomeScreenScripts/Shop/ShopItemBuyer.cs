using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShopItemBuyer : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemID = "bundle_1";
    public string itemName = "Bundle 1";
    public int itemPrice = 2500;
    public int costumeIndex = 0;
    public bool isFreeItem = false;
    public Text nameText;   

    public enum CharacterGender { Both, GirlOnly, BoyOnly }

    [Header("Gender Filter")]
    public CharacterGender targetGender = CharacterGender.Both;

    [Header("Gender Visuals")]
    public string girlItemName = "";
    public string boyItemName  = "";
    public Sprite girlSprite;
    public Sprite boySprite;
    public Image bundleImage;

    [Header("UI References - Assign These!")]
    public Text priceText;
    public Button buyButton;
    public Text buttonText;
    public GameObject lockIcon;
    public Image priceBackground;
    
    [Header("Popup References")]
    public GameObject buyPopup;
    public GameObject successPopup;
    public Text successMessageText;
    public Button successOkButton;
    public GameObject shopPanel;
    
    [Header("Auto-Close Settings")]
    public float autoCloseDelay = 3f;
    
    private bool isPurchased = false;
    private bool isEquipped = false;
    private bool isGirl = true;
    private Coroutine autoCloseCoroutine;
    
    void Start()
    {   
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 1);
        isGirl = (selected == 2);
        CheckGenderVisibility();
        ApplyGenderVisuals();

        if (buyButton != null)
            buyButton.onClick.AddListener(OnButtonClicked);
        
        if (successOkButton != null)
            successOkButton.onClick.AddListener(CloseAllPopups);
        
        LoadItemState();
        UpdateUI();
    }

    void OnEnable()
    {
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 1);
        isGirl = (selected == 2);
        CheckGenderVisibility();
        ApplyGenderVisuals();
        LoadItemState();
        UpdateUI();
    }

    void CheckGenderVisibility()
    {
        if (targetGender == CharacterGender.GirlOnly && !isGirl)
            gameObject.SetActive(false);
        else if (targetGender == CharacterGender.BoyOnly && isGirl)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    }

    void ApplyGenderVisuals()
    {
        if (bundleImage != null)
        {
            if (isGirl && girlSprite != null)
                bundleImage.sprite = girlSprite;
            else if (!isGirl && boySprite != null)
                bundleImage.sprite = boySprite;
        }

        if (isGirl && !string.IsNullOrEmpty(girlItemName))
            itemName = girlItemName;
        else if (!isGirl && !string.IsNullOrEmpty(boyItemName))
            itemName = boyItemName;
            // ✅ ADD THIS → update the separate name Text
    if (nameText != null)
        nameText.text = itemName;
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
            Debug.Log("Already owned! Equip from the Closet.");
        }
        else
        {
            if (buyPopup != null)
                buyPopup.SetActive(true);
            else
                TryPurchase();
        }
    }
    
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
                isPurchased = true;
                PlayerPrefs.SetInt("Item_" + itemID + "_Purchased", 1);
                PlayerPrefs.Save();

                if (buyPopup != null)
                    buyPopup.SetActive(false);

                RefreshAllItemsUI();
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

    void ShowSuccessPopup()
    {
        if (successPopup != null)
        {
            successPopup.SetActive(true);
            
            if (successMessageText != null)
                successMessageText.text = "PURCHASE COMPLETE!\nYou got: " + itemName;
            
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);

            autoCloseCoroutine = StartCoroutine(AutoClosePopups());
            
            Debug.Log($"✅ Success popup shown - will auto-close in {autoCloseDelay} seconds");
        }
    }

    IEnumerator AutoClosePopups()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseAllPopups();
        Debug.Log("⏰ Popups auto-closed - Returned to shop");
    }

    void CloseAllPopups()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        if (buyPopup != null)     buyPopup.SetActive(false);
        if (successPopup != null) successPopup.SetActive(false);
        if (shopPanel != null)    shopPanel.SetActive(true);
        
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
            item.isEquipped = false;

        isEquipped = true;
        PlayerPrefs.SetInt("EquippedCostume", costumeIndex);
        PlayerPrefs.Save();

        RefreshAllItemsUI();

        Debug.Log($"👕 Equipped {itemName}!");
    }
    
    void UpdateUI()
    {
        if (isPurchased) UpdateUIForOwnedItem();
        else             UpdateUIForLockedItem();
    }
    
    void UpdateUIForOwnedItem()
    {
        if (priceText != null)
        {
            priceText.text = "OWNED";
            priceText.color = Color.white;
            priceText.gameObject.SetActive(true);
        }
        
        if (priceBackground != null)
        {   
            priceBackground.gameObject.SetActive(false);
            Image img = priceBackground.GetComponent<Image>();
            if (img != null)
                img.color = new Color(0.6f, 0.6f, 0.6f);
            priceBackground.gameObject.SetActive(true);
        }
        
        if (lockIcon != null)
            lockIcon.SetActive(false);

        if (buyButton != null && buttonText != null)
        {
            buttonText.text = "OWNED";
            buttonText.color = Color.white;
            buyButton.interactable = false;
            
            ColorBlock colors = buyButton.colors;
            colors.normalColor      = new Color(0.5f, 0.5f, 0.5f);
            colors.disabledColor    = new Color(0.5f, 0.5f, 0.5f);
            colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f);
            colors.pressedColor     = new Color(0.5f, 0.5f, 0.5f);
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
            priceBackground.gameObject.SetActive(true);
        
        if (lockIcon != null)
            lockIcon.SetActive(true);
        
        if (buyButton != null && buttonText != null)
        {
            buttonText.text = "BUY";
            buyButton.interactable = true;
            
            ColorBlock colors = buyButton.colors;
            colors.normalColor      = new Color(0.3f, 0.8f, 0.3f);
            colors.highlightedColor = new Color(0.25f, 0.7f, 0.25f);
            colors.pressedColor     = new Color(0.2f, 0.6f, 0.2f);
            buyButton.colors = colors;
        }
    }
    
    public bool IsPurchased() { return isPurchased; }
    public bool IsEquipped()  { return isEquipped; }
    
    void RefreshAllItemsUI()
    {
        ShopItemBuyer[] allItems = FindObjectsOfType<ShopItemBuyer>();
        foreach (ShopItemBuyer item in allItems)
        {
            item.LoadItemState();
            item.UpdateUI();
        }
    }
    
    void OnDestroy()
    {
        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);
    }
}