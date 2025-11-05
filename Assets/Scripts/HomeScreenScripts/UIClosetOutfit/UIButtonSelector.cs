using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIButtonSelector : MonoBehaviour
{
    [Header("Button Settings")]
    public Image targetImage;           // The image of this button
    public Sprite defaultSprite;        // Normal sprite
    public Sprite selectedSprite;       // Sprite when selected
    public bool isDefaultSelected = false; // ✅ Set this true for default selection

    [Header("Shop Lock Settings")]
    public string purchaseKey = "";     // Unique key from BuyButtonController (e.g. "Item1Purchased")
    public GameObject lockUI;           // 🔒 The lock icon in your hierarchy
    public Button button;               // The actual Button component to disable/enable

    private static List<UIButtonSelector> allButtons = new List<UIButtonSelector>();
    private static UIButtonSelector activeButton;

    void Awake()
    {
        // Register this button
        allButtons.Add(this);

        // Try to get Button automatically if not assigned
        if (button == null)
            button = GetComponent<Button>();
    }

    void Start()
    {
        CheckPurchaseStatus();

        // Set the default selected button
        if (isDefaultSelected && button != null && button.interactable)
        {
            Select();
            activeButton = this;
        }
        else
        {
            Deselect();
        }
    }

    /// <summary>
    /// Checks if the item has been purchased and updates lock state
    /// </summary>
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
            // ✅ Unlock the button
            if (lockUI != null) lockUI.SetActive(false);
            if (button != null) button.interactable = true;
        }
        else
        {
            // 🔒 Locked (not yet purchased)
            if (lockUI != null) lockUI.SetActive(true);
            if (button != null) button.interactable = false;
        }
    }

    /// <summary>
    /// Called by Button OnClick event
    /// </summary>
    public void OnButtonClick()
    {
        if (button == null || !button.interactable)
            return;

        if (activeButton == this)
            return;

        // Deselect all other buttons
        foreach (UIButtonSelector btn in allButtons)
        {
            btn.Deselect();
        }

        // Select this button
        Select();
        activeButton = this;
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

    void OnDestroy()
    {
        allButtons.Remove(this);
    }
}
