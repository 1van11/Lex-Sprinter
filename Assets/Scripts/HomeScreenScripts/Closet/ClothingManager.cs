using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Single Outfit Bundle (Top + Bottom + Shoes as one outfit)
[System.Serializable]
public class OutfitBundle
{
    public string outfitID;
    public string outfitName;
    public GameObject topClothing;      // Shirt/Top
    public GameObject bottomClothing;   // Pants/Skirt
    public GameObject shoesClothing;    // Shoes (optional)
    public GameObject accessory;        // Hat/Accessory (optional)
    public Sprite outfitIcon;           // Icon for UI
    public bool isUnlocked = true;
    public int price = 0;
}

// Save data for currently equipped outfit
[System.Serializable]
public class OutfitSaveData
{
    public string currentOutfitID;
}

public class OutfitManager : MonoBehaviour
{
    public static OutfitManager Instance;
    
    [Header("Available Outfits")]
    public List<OutfitBundle> allOutfits = new List<OutfitBundle>();
    
    [Header("Currently Equipped")]
    public OutfitBundle currentOutfit;
    
    [Header("Character Reference")]
    public Transform characterModel; // Your character's body
    
    private const string SAVE_KEY = "CurrentOutfit";
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadOutfit();
    }
    
    // Equip a complete outfit bundle
    public void EquipOutfit(OutfitBundle outfit)
    {
        if (outfit == null)
        {
            Debug.LogWarning("Outfit is null!");
            return;
        }
        
        // Disable current outfit first
        if (currentOutfit != null)
        {
            DisableOutfit(currentOutfit);
        }
        
        // Enable new outfit
        currentOutfit = outfit;
        EnableOutfit(outfit);
        
        // Auto-save after equipping
        SaveOutfit();
        
        Debug.Log("Equipped outfit: " + outfit.outfitName);
    }
    
    // Enable all pieces of an outfit
    private void EnableOutfit(OutfitBundle outfit)
    {
        if (outfit.topClothing != null)
            outfit.topClothing.SetActive(true);
        
        if (outfit.bottomClothing != null)
            outfit.bottomClothing.SetActive(true);
        
        if (outfit.shoesClothing != null)
            outfit.shoesClothing.SetActive(true);
        
        if (outfit.accessory != null)
            outfit.accessory.SetActive(true);
    }
    
    // Disable all pieces of an outfit
    private void DisableOutfit(OutfitBundle outfit)
    {
        if (outfit.topClothing != null)
            outfit.topClothing.SetActive(false);
        
        if (outfit.bottomClothing != null)
            outfit.bottomClothing.SetActive(false);
        
        if (outfit.shoesClothing != null)
            outfit.shoesClothing.SetActive(false);
        
        if (outfit.accessory != null)
            outfit.accessory.SetActive(false);
    }
    
    // Save currently equipped outfit
    public void SaveOutfit()
    {
        if (currentOutfit == null)
        {
            Debug.LogWarning("No outfit to save!");
            return;
        }
        
        OutfitSaveData saveData = new OutfitSaveData();
        saveData.currentOutfitID = currentOutfit.outfitID;
        
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        
        Debug.Log("Outfit saved: " + currentOutfit.outfitName);
    }
    
    // Load saved outfit
    public void LoadOutfit()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            OutfitSaveData saveData = JsonUtility.FromJson<OutfitSaveData>(json);
            
            // Find the outfit by ID
            OutfitBundle outfit = allOutfits.Find(x => x.outfitID == saveData.currentOutfitID);
            
            if (outfit != null)
            {
                EquipOutfit(outfit);
                Debug.Log("Outfit loaded: " + outfit.outfitName);
            }
            else
            {
                Debug.LogWarning("Saved outfit not found! Loading default.");
                LoadDefaultOutfit();
            }
        }
        else
        {
            Debug.Log("No saved outfit. Loading default.");
            LoadDefaultOutfit();
        }
    }
    
    // Load the first available outfit as default
    private void LoadDefaultOutfit()
    {
        if (allOutfits.Count > 0)
        {
            EquipOutfit(allOutfits[0]);
        }
    }
    
    // Get outfit by ID
    public OutfitBundle GetOutfitByID(string outfitID)
    {
        return allOutfits.Find(x => x.outfitID == outfitID);
    }
    
    // Check if outfit is unlocked
    public bool IsOutfitUnlocked(string outfitID)
    {
        OutfitBundle outfit = GetOutfitByID(outfitID);
        return outfit != null && outfit.isUnlocked;
    }
    
    // Unlock an outfit (for shop/achievements)
    public void UnlockOutfit(string outfitID)
    {
        OutfitBundle outfit = GetOutfitByID(outfitID);
        if (outfit != null)
        {
            outfit.isUnlocked = true;
            Debug.Log("Unlocked outfit: " + outfit.outfitName);
        }
    }
    
    // Reset to default outfit
    public void ResetOutfit()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        LoadDefaultOutfit();
        Debug.Log("Outfit reset to default");
    }
}