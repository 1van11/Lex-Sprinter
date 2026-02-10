using UnityEngine;
using UnityEngine.UI;

public class ClosetOutfitButton : MonoBehaviour
{
    [Header("Outfit Data")]
    public string itemID = "bundle_1";  // Must match shop itemID!
    public string outfitName = "School Uniform";
    public int costumeIndex = 0;
    
    [Header("UI Elements")]
    public Button button;
    public Image outfitImage;  // The Top/Bot image
    public Text nameText;  // Optional
    
    [Header("Visual Feedback")]
    public GameObject lockIcon;  // Optional lock image
    
    private bool isPurchased = false;
    
    void Start()
    {
        // Add click listener
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        
        // Check purchase status
        CheckPurchaseStatus();
    }
    
    void OnEnable()
    {
        // Refresh when closet opens
        CheckPurchaseStatus();
    }
    
    void CheckPurchaseStatus()
    {
        // Check if this item was purchased in shop
        isPurchased = PlayerPrefs.GetInt("Item_" + itemID + "_Purchased", 0) == 1;
        
        UpdateButtonState();
    }
    
    void UpdateButtonState()
{
    bool isEquipped = PlayerPrefs.GetString("Equipped_Outfit", "") == itemID;

    if (button != null)
    {
        // Button should be clickable ONLY if purchased AND not already equipped
        button.interactable = isPurchased && !isEquipped;
    }

    if (outfitImage != null)
    {
        outfitImage.color = isPurchased ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    if (lockIcon != null)
    {
        lockIcon.SetActive(!isPurchased);
    }

    // Optional: change button text
    Text btnText = button.GetComponentInChildren<Text>();
    if (btnText != null)
    {
        if (!isPurchased)
            btnText.text = "Locked";
        else if (isEquipped)
            btnText.text = "Equipped";
        else
            btnText.text = "Equip";
    }
}

    
    void OnButtonClick()
{
    CheckPurchaseStatus(); // 🔁 Force refresh

    if (!isPurchased)
    {
        Debug.Log("❌ This outfit is locked! Buy it in the shop first.");
        return;
    }

    ClosetManager manager = FindObjectOfType<ClosetManager>();
    if (manager != null)
    {
        Sprite outfitSprite = null;
        if (outfitImage != null)
        {
            outfitSprite = outfitImage.sprite;
        }

        manager.ShowOutfitPreview(itemID, outfitName, outfitSprite, costumeIndex, isPurchased);
    }
}


    
    // Call this after purchasing in shop
    public void RefreshPurchaseStatus()
    {
        CheckPurchaseStatus();
    }
}