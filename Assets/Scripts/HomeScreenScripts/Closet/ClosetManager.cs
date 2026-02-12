using UnityEngine;
using UnityEngine.UI;

public class ClosetManager : MonoBehaviour
{
    [Header("Preview UI")]
    public Image previewImage;
    public Text outfitNameText;
    public Text statusText;
    public Button equipButton;
    public Text equipButtonText;

    [Header("Panel")]
    public GameObject closetPanel;

    [Header("Currently Selected")]
    private string selectedOutfitID;
    private int selectedCostumeIndex;
    private Sprite selectedSprite;
    private string selectedOutfitName;
    private bool selectedIsPurchased;

    void Start()
    {
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(EquipOutfit);
        }

        ClearPreview();
    }

    public void ShowOutfitPreview(string outfitID, string outfitName, Sprite outfitSprite, int costumeIndex, bool isPurchased)
{
    // --- ADD THIS LINE TO UNLOCK BUNDLE 1 ---
    if (outfitID == "bundle_1") isPurchased = true;
    // ----------------------------------------

    selectedOutfitID = outfitID;
    selectedCostumeIndex = costumeIndex;
    selectedSprite = outfitSprite;
    selectedOutfitName = outfitName;
    selectedIsPurchased = isPurchased;

    if (previewImage != null && outfitSprite != null)
    {
        previewImage.sprite = outfitSprite;
        previewImage.enabled = true;
    }

    if (outfitNameText != null)
    {
        outfitNameText.text = outfitName;
    }

    string equippedID = PlayerPrefs.GetString("EquippedOutfitID", "");
    bool isEquipped = equippedID == outfitID;

    if (statusText != null)
    {
        if (!isPurchased)
        {
            statusText.text = "LOCKED - Buy in shop!";
            statusText.color = new Color(0.96f, 0.26f, 0.21f);
        }
        else if (isEquipped)
        {
            statusText.text = "EQUIPPED";
            statusText.color = new Color(1f, 0.84f, 0f);
        }
        else
        {
            statusText.text = "OWNED";
            statusText.color = new Color(0.3f, 0.69f, 0.31f);
        }
    }

    UpdateEquipButton(isPurchased, isEquipped);
}

    void UpdateEquipButton(bool isPurchased, bool isEquipped)
{
    if (!isPurchased)
    {
        equipButton.interactable = false;
        equipButtonText.text = "LOCKED";
    }
    else if (isEquipped)
    {
        equipButton.interactable = false;
        equipButtonText.text = "EQUIPPED";
    }
    else
    {
        equipButton.interactable = true;
        equipButtonText.text = "EQUIP";
    }
}



    void EquipOutfit()
{
    if (!selectedIsPurchased) return;

    // 1. SAVE the choice
    PlayerPrefs.SetInt("EquippedCostume", selectedCostumeIndex);
    PlayerPrefs.SetString("EquippedOutfitID", selectedOutfitID);
    PlayerPrefs.Save();

    // 2. TELL the character to change
    CharacterCostumeManager character = FindObjectOfType<CharacterCostumeManager>();
    if (character != null)
    {
        // Change 'UpdateCostume' to 'SetCostume' to match your other script
        character.SetCostume(selectedCostumeIndex);
    }

    // 3. Refresh UI
    ShowOutfitPreview(selectedOutfitID, selectedOutfitName, selectedSprite, selectedCostumeIndex, selectedIsPurchased);
}

    void RefreshAllOutfitButtons()
    {
        ClosetOutfitButton[] buttons = FindObjectsOfType<ClosetOutfitButton>();
        foreach (ClosetOutfitButton btn in buttons)
        {
            btn.RefreshPurchaseStatus();
        }
    }

    void ClearPreview()
{
    if (previewImage != null)
    {
        previewImage.enabled = false;
    }
    
    if (outfitNameText != null)
    {
        outfitNameText.text = "Select an outfit";
    }
    
    if (statusText != null)
    {
        statusText.text = "";
    }
    
    if (equipButton != null)
    {
        equipButton.interactable = false;
        
        // Make button gray when disabled
        ColorBlock colors = equipButton.colors;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
        equipButton.colors = colors;
    }
    
    if (equipButtonText != null)
    {
        equipButtonText.text = "LOCKED";
        equipButtonText.color = new Color(0.7f, 0.7f, 0.7f, 1f); // Gray text
    }
}

    public void OpenCloset()
    {
        if (closetPanel != null) closetPanel.SetActive(true);
        ClearPreview();
        RefreshAllOutfitButtons();
    }

    public void CloseCloset()
    {
        if (closetPanel != null) closetPanel.SetActive(false);
    }
}
