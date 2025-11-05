using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIButtonSelector : MonoBehaviour
{
    [Header("Button Settings")]
    public Image targetImage;              // The image of this button
    public Sprite defaultSprite;           // Normal sprite
    public Sprite selectedSprite;          // Sprite when selected
    public bool isDefaultSelected = false; // ✅ Default selected

    [Header("Shop Lock Settings")]
    public string purchaseKey = "";        // Purchase key
    public GameObject lockUI;              // 🔒 Lock icon
    public Button button;                  // Button reference

    [Header("Character Material Settings")]
    public SkinnedMeshRenderer torsoRenderer;      // Torso renderer
    public SkinnedMeshRenderer lowerTorsoRenderer; // Lower torso renderer

    [Tooltip("Assign materials for torso and lower torso here")]
    public Material[] torsoMaterials;      // Materials for torso
    public Material[] lowerTorsoMaterials; // Materials for lower torso (2 elements)

    private static List<UIButtonSelector> allButtons = new List<UIButtonSelector>();
    private static UIButtonSelector activeButton;

    private const string EQUIPPED_KEY = "EquippedOutfit"; // 🔑 Key for saving selection

    void Awake()
    {
        allButtons.Add(this);
        if (button == null)
            button = GetComponent<Button>();
    }

    void Start()
    {
        CheckPurchaseStatus();

        // 🔹 Check if this button is the saved outfit
        string equippedName = PlayerPrefs.GetString(EQUIPPED_KEY, "");

        if (!string.IsNullOrEmpty(equippedName) && equippedName == name)
        {
            Select();
            activeButton = this;
            ApplyCharacterMaterials();
        }
        else if (isDefaultSelected && button != null && button.interactable && string.IsNullOrEmpty(equippedName))
        {
            Select();
            activeButton = this;
            ApplyCharacterMaterials();
            SaveEquippedOutfit(); // Save default
        }
        else
        {
            Deselect();
        }
    }

    public void CheckPurchaseStatus()
    {
        if (string.IsNullOrEmpty(purchaseKey))
        {
            Debug.LogWarning($"⚠️ No purchase key set for {name}.");
            return;
        }

        bool purchased = PlayerPrefs.GetInt(purchaseKey, 0) == 1;

        if (purchased)
        {
            if (lockUI != null) lockUI.SetActive(false);
            if (button != null) button.interactable = true;
        }
        else
        {
            if (lockUI != null) lockUI.SetActive(true);
            if (button != null) button.interactable = false;
        }
    }

    public void OnButtonClick()
    {
        if (button == null || !button.interactable)
            return;

        if (activeButton == this)
            return;

        foreach (UIButtonSelector btn in allButtons)
        {
            btn.Deselect();
        }

        Select();
        activeButton = this;

        // ✅ Apply the materials for this button
        ApplyCharacterMaterials();

        // 💾 Save selection
        SaveEquippedOutfit();
    }

    private void Select()
    {
        if (targetImage != null && selectedSprite != null)
        {
            targetImage.sprite = selectedSprite;
        }
    }

    private void Deselect()
    {
        if (targetImage != null && defaultSprite != null)
        {
            targetImage.sprite = defaultSprite;
        }
    }

    /// <summary>
    /// Apply the assigned materials to torso and lower torso renderers.
    /// </summary>
    private void ApplyCharacterMaterials()
    {
        if (torsoRenderer != null && torsoMaterials != null && torsoMaterials.Length > 0)
        {
            torsoRenderer.materials = torsoMaterials;
        }

        if (lowerTorsoRenderer != null && lowerTorsoMaterials != null && lowerTorsoMaterials.Length > 0)
        {
            lowerTorsoRenderer.materials = lowerTorsoMaterials;
        }

        Debug.Log($"✅ Applied materials for {name}");
    }

    /// <summary>
    /// 💾 Save the currently equipped outfit
    /// </summary>
    private void SaveEquippedOutfit()
    {
        PlayerPrefs.SetString(EQUIPPED_KEY, name);
        PlayerPrefs.Save();
        Debug.Log($"💾 Saved equipped outfit: {name}");
    }

    void OnDestroy()
    {
        allButtons.Remove(this);
    }
}
