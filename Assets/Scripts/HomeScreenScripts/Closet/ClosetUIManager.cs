using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosetUIManager : MonoBehaviour
{
    [Header("Closet Panel")]
    public GameObject closetPanel;
    
    [Header("Outfit Slots")]
    public Transform outfitSlotsContainer; // Container with your 4 outfit slots
    public GameObject outfitSlotPrefab;     // Prefab for each slot
    
    [Header("Preview")]
    public GameObject characterPreview;     // Optional: preview character
    
    private List<GameObject> outfitSlots = new List<GameObject>();
    
    void Start()
    {
        SetupOutfitSlots();
    }
    
    // Create outfit slots from available outfits
    void SetupOutfitSlots()
    {
        // Clear existing slots
        foreach (Transform child in outfitSlotsContainer)
        {
            Destroy(child.gameObject);
        }
        outfitSlots.Clear();
        
        // Create a slot for each outfit
        foreach (OutfitBundle outfit in OutfitManager.Instance.allOutfits)
        {
            CreateOutfitSlot(outfit);
        }
    }
    
    // Create a single outfit slot button
    void CreateOutfitSlot(OutfitBundle outfit)
    {
        GameObject slotObj;
        
        // Use prefab if available, otherwise create simple button
        if (outfitSlotPrefab != null)
        {
            slotObj = Instantiate(outfitSlotPrefab, outfitSlotsContainer);
        }
        else
        {
            slotObj = new GameObject("OutfitSlot_" + outfit.outfitID);
            slotObj.transform.SetParent(outfitSlotsContainer);
            slotObj.AddComponent<Button>();
            slotObj.AddComponent<Image>();
        }
        
        outfitSlots.Add(slotObj);
        
        // Setup button
        Button button = slotObj.GetComponent<Button>();
        Image slotImage = slotObj.GetComponent<Image>();
        
        // Find child components (if using prefab structure)
        Image iconImage = slotObj.transform.Find("Icon")?.GetComponent<Image>();
        GameObject lockIcon = slotObj.transform.Find("Lock")?.gameObject;
        Text nameText = slotObj.transform.Find("Name")?.GetComponent<Text>();
        GameObject equippedIndicator = slotObj.transform.Find("Equipped")?.gameObject;
        
        // Set outfit icon
        if (iconImage != null && outfit.outfitIcon != null)
        {
            iconImage.sprite = outfit.outfitIcon;
        }
        else if (slotImage != null && outfit.outfitIcon != null)
        {
            slotImage.sprite = outfit.outfitIcon;
        }
        
        // Set outfit name
        if (nameText != null)
        {
            nameText.text = outfit.outfitName;
        }
        
        // Show/hide lock based on unlock status
        if (lockIcon != null)
        {
            lockIcon.SetActive(!outfit.isUnlocked);
        }
        
        // Show equipped indicator if this is current outfit
        if (equippedIndicator != null)
        {
            equippedIndicator.SetActive(OutfitManager.Instance.currentOutfit == outfit);
        }
        
        // Setup button click
        if (outfit.isUnlocked)
        {
            button.onClick.AddListener(() => OnOutfitClicked(outfit));
        }
        else
        {
            button.onClick.AddListener(() => OnLockedOutfitClicked(outfit));
        }
    }
    
    // When player clicks an unlocked outfit
    void OnOutfitClicked(OutfitBundle outfit)
    {
        // Equip the outfit
        OutfitManager.Instance.EquipOutfit(outfit);
        
        // Refresh UI to show which is equipped
        RefreshOutfitSlots();
        
        Debug.Log("Selected outfit: " + outfit.outfitName);
    }
    
    // When player clicks a locked outfit
    void OnLockedOutfitClicked(OutfitBundle outfit)
    {
        Debug.Log("Outfit locked: " + outfit.outfitName + " - Price: " + outfit.price + " coins");
        
        // TODO: Show purchase dialog
        // You can implement shop logic here
    }
    
    // Refresh all outfit slots (update equipped indicator)
    public void RefreshOutfitSlots()
    {
        for (int i = 0; i < outfitSlots.Count; i++)
        {
            if (i < OutfitManager.Instance.allOutfits.Count)
            {
                GameObject slot = outfitSlots[i];
                OutfitBundle outfit = OutfitManager.Instance.allOutfits[i];
                
                // Update equipped indicator
                GameObject equippedIndicator = slot.transform.Find("Equipped")?.gameObject;
                if (equippedIndicator != null)
                {
                    equippedIndicator.SetActive(OutfitManager.Instance.currentOutfit == outfit);
                }
            }
        }
    }
    
    // Open closet panel
    public void OpenCloset()
    {
        closetPanel.SetActive(true);
        RefreshOutfitSlots();
    }
    
    // Close closet panel
    public void CloseCloset()
    {
        closetPanel.SetActive(false);
        // Save happens automatically when outfit is equipped
    }
    
    // Buy/Unlock outfit (call this from shop)
    public void PurchaseOutfit(string outfitID, int playerCoins)
    {
        OutfitBundle outfit = OutfitManager.Instance.GetOutfitByID(outfitID);
        
        if (outfit != null && !outfit.isUnlocked)
        {
            if (playerCoins >= outfit.price)
            {
                // Unlock outfit
                OutfitManager.Instance.UnlockOutfit(outfitID);
                
                // Deduct coins (you need to implement CoinManager)
                // CoinManager.Instance.SpendCoins(outfit.price);
                
                // Refresh UI
                RefreshOutfitSlots();
                
                Debug.Log("Purchased outfit: " + outfit.outfitName);
            }
            else
            {
                Debug.Log("Not enough coins!");
            }
        }
    }
}