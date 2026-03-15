using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClosetManager : MonoBehaviour
{
    [Header("Preview UI")]
    public Image previewImage;
    public TMP_Text outfitNameText;
    public TMP_Text statusText;
    public Button equipButton;
    public TMP_Text equipButtonText;

    [Header("Panel")]
    public GameObject closetPanel;

    [Header("Girl Preview Sprites")]
    public Sprite girlDefaultSprite;
    public Sprite girlRainySprite;
    public Sprite girlChristmasSprite;

    [Header("Boy Preview Sprites")]
    public Sprite boyDefaultSprite;
    public Sprite boyRainySprite;
    public Sprite boyChristmasSprite;

    [Header("Currently Selected")]
    private string selectedOutfitID;
    private int selectedCostumeIndex;
    private Sprite selectedSprite;
    private string selectedOutfitName;
    private bool selectedIsPurchased;
    private bool isGirl;

    void Start()
    {
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 1);
        isGirl = (selected == 2);

        if (equipButton != null)
            equipButton.onClick.AddListener(EquipOutfit);

        ClearPreview();
    }

    Sprite GetCharacterSprite(int costumeIndex)
    {
        if (isGirl)
        {
            switch (costumeIndex)
            {
                case 0: return girlDefaultSprite;
                case 1: return girlRainySprite;
                case 2: return girlChristmasSprite;
            }
        }
        else
        {
            switch (costumeIndex)
            {
                case 0: return boyDefaultSprite;
                case 1: return boyRainySprite;
                case 2: return boyChristmasSprite;
            }
        }
        return null;
    }

    public void ShowOutfitPreview(string outfitID, string outfitName, Sprite outfitSprite, int costumeIndex, bool isPurchased)
    {
        if (outfitID == "bundle_1") isPurchased = true;

        selectedOutfitID = outfitID;
        selectedCostumeIndex = costumeIndex;
        selectedSprite = outfitSprite;
        selectedOutfitName = outfitName;
        selectedIsPurchased = isPurchased;

        if (previewImage != null)
        {
            Sprite characterSprite = GetCharacterSprite(costumeIndex);
            if (characterSprite != null)
            {
                previewImage.sprite = characterSprite;
                previewImage.enabled = true;
            }
            else if (outfitSprite != null)
            {
                previewImage.sprite = outfitSprite;
                previewImage.enabled = true;
            }
        }

        if (outfitNameText != null)
            outfitNameText.text = outfitName;

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

        PlayerPrefs.SetInt("EquippedCostume", selectedCostumeIndex);
        PlayerPrefs.SetString("EquippedOutfitID", selectedOutfitID);
        PlayerPrefs.Save();

        CharacterCostumeManager character = FindObjectOfType<CharacterCostumeManager>();
        if (character != null)
            character.SetCostume(selectedCostumeIndex);

        ShowOutfitPreview(selectedOutfitID, selectedOutfitName, selectedSprite, selectedCostumeIndex, selectedIsPurchased);
    }

    void RefreshAllOutfitButtons()
    {
        ClosetOutfitButton[] buttons = FindObjectsOfType<ClosetOutfitButton>();
        foreach (ClosetOutfitButton btn in buttons)
            btn.RefreshPurchaseStatus();
    }

    void ClearPreview()
    {
        if (previewImage != null)
            previewImage.enabled = false;

        if (outfitNameText != null)
            outfitNameText.text = "Select an outfit";

        if (statusText != null)
            statusText.text = "";

        if (equipButton != null)
        {
            equipButton.interactable = false;
            ColorBlock colors = equipButton.colors;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            equipButton.colors = colors;
        }

        if (equipButtonText != null)
        {
            equipButtonText.text = "LOCKED";
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