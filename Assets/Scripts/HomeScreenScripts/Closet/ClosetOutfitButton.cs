using UnityEngine;
using UnityEngine.UI;

public class ClosetOutfitButton : MonoBehaviour
{   
    [Header("Outfit Data")]
    public string itemID = "bundle_1"; 
    public string outfitName = "School Uniform";
    public int costumeIndex = 0;
    
    [Header("UI Elements")]
    public Button button;
    public Image outfitImage;  
    public Text nameText;  
    
    [Header("Visual Feedback")]
    public GameObject lockIcon;  
    
    private bool isPurchased = false;
    
    void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        
        CheckPurchaseStatus();
    }

    void OnEnable()
    {
        CheckPurchaseStatus();
    }
    
    public void CheckPurchaseStatus()
    {
        // 1. Check shop purchase
        bool hasBought = PlayerPrefs.GetInt("Item_" + itemID + "_Purchased", 0) == 1;

        // 2. FORCE UNLOCK BUNDLE 1 IMMEDIATELY
        bool isFree = (itemID == "bundle_1");
        
        isPurchased = hasBought || isFree;
        
        UpdateButtonState();
    }
    
    void UpdateButtonState()
    {
        // Check if equipped using the ID used in ClosetManager
        bool isEquipped = PlayerPrefs.GetString("EquippedOutfitID", "") == itemID;

        if (button != null)
        {
            // Small buttons stay interactable so you can always see the preview
            button.interactable = true; 
        }

        if (outfitImage != null)
        {
            outfitImage.color = isPurchased ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        if (lockIcon != null)
        {
            lockIcon.SetActive(!isPurchased);
        }
    }
    
    void OnButtonClick()
    {
        CheckPurchaseStatus(); 

        ClosetManager manager = FindObjectOfType<ClosetManager>();
        if (manager != null)
        {
            Sprite outfitSprite = (outfitImage != null) ? outfitImage.sprite : null;
            
            // This ONLY updates the closet preview UI
            manager.ShowOutfitPreview(itemID, outfitName, outfitSprite, costumeIndex, isPurchased);
        }
    }
    
    // Kept for shop compatibility
    public void RefreshPurchaseStatus()
    {
        CheckPurchaseStatus();
    }
}