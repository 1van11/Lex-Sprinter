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
        // Save selection
        selectedOutfitID = outfitID;
        selectedCostumeIndex = costumeIndex;
        selectedSprite = outfitSprite;
        selectedOutfitName = outfitName;
        selectedIsPurchased = isPurchased;

        // Show image
        if (previewImage != null && outfitSprite != null)
        {
            previewImage.sprite = outfitSprite;
            previewImage.enabled = true;
        }

        // Show name
        if (outfitNameText != null)
        {
            outfitNameText.text = outfitName;
        }

        // Check if equipped using ID (NOT index)
        string equippedID = PlayerPrefs.GetString("EquippedOutfitID", "");
        bool isEquipped = equippedID == outfitID;

        // Update status text
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
        if (!selectedIsPurchased)
        {
            Debug.Log("❌ Cannot equip! Outfit not purchased!");
            return;
        }

        // Save equipped outfit by ID
        PlayerPrefs.SetString("EquippedOutfitID", selectedOutfitID);
        PlayerPrefs.Save();

        Debug.Log("✅ EQUIPPED: " + selectedOutfitName);

        // Refresh preview
        ShowOutfitPreview(selectedOutfitID, selectedOutfitName, selectedSprite, selectedCostumeIndex, selectedIsPurchased);

        // Refresh all outfit buttons
        RefreshAllOutfitButtons();
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
        if (previewImage != null) previewImage.enabled = false;
        if (outfitNameText != null) outfitNameText.text = "Select an outfit";
        if (statusText != null) statusText.text = "";
        if (equipButton != null) equipButton.interactable = false;
        if (equipButtonText != null) equipButtonText.text = "EQUIP";
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
