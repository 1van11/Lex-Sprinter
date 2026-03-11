using UnityEngine;
using UnityEngine.UI;

public class ClosetOutfitButton : MonoBehaviour
{   
    [Header("Outfit Data")]
    public string itemID = "bundle_1"; 
    public string girlOutfitName = "School Uniform";  // shown when girl selected
    public string boyOutfitName  = "School Uniform";  // shown when boy selected
    public int costumeIndex = 0;
    
    [Header("UI Elements")]
    public Button button;
    public Image outfitImage;  
    public Text nameText;  
    
    [Header("Visual Feedback")]
    public GameObject lockIcon;  
    
    private bool isPurchased = false;
    private bool isGirl = true;
    
    void Start()
    {
        // Read gender once
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 1);
        isGirl = (selected == 2);

        if (button != null)
            button.onClick.AddListener(OnButtonClick);

        // Show correct name on the button label
        if (nameText != null)
            nameText.text = isGirl ? girlOutfitName : boyOutfitName;
        
        CheckPurchaseStatus();
    }

    void OnEnable()
    {
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 1);
        isGirl = (selected == 2);

        // Refresh name label whenever panel opens
        if (nameText != null)
            nameText.text = isGirl ? girlOutfitName : boyOutfitName;

        CheckPurchaseStatus();
    }
    
    public void CheckPurchaseStatus()
    {
        bool hasBought = PlayerPrefs.GetInt("Item_" + itemID + "_Purchased", 0) == 1;
        bool isFree    = (itemID == "bundle_1");
        isPurchased    = hasBought || isFree;
        UpdateButtonState();
    }
    
    void UpdateButtonState()
    {
        bool isEquipped = PlayerPrefs.GetString("EquippedOutfitID", "") == itemID;

        if (button != null)
            button.interactable = true; 

        if (outfitImage != null)
            outfitImage.color = isPurchased ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);

        if (lockIcon != null)
            lockIcon.SetActive(!isPurchased);
    }
    
    void OnButtonClick()
    {
        CheckPurchaseStatus(); 

        // Use correct name based on gender
        string displayName = isGirl ? girlOutfitName : boyOutfitName;

        ClosetManager manager = FindObjectOfType<ClosetManager>();
        if (manager != null)
        {
            Sprite outfitSprite = (outfitImage != null) ? outfitImage.sprite : null;
            manager.ShowOutfitPreview(itemID, displayName, outfitSprite, costumeIndex, isPurchased);
        }
    }
    
    public void RefreshPurchaseStatus()
    {
        CheckPurchaseStatus();
    }
}