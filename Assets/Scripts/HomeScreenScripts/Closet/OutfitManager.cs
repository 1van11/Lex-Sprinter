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
    public Transform characterModel; // Your character's body in current scene
    
    [Header("Auto-Find Character")]
    public string characterTag = "Player"; // Tag to find character automatically
    public bool autoFindCharacter = true;
    
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
            return;
        }
    }
    
    void Start()
    {
        LoadOutfit();
        ApplyOutfitToCharacter();
    }
    
    void Update()
    {
        // Auto-find character if not set or if we changed scenes
        if (autoFindCharacter && characterModel == null)
        {
            FindCharacter();
        }
    }
    
    /// <summary>
    /// Automatically find the character in the current scene
    /// </summary>
    void FindCharacter()
    {
        GameObject player = GameObject.FindGameObjectWithTag(characterTag);
        if (player != null)
        {
            characterModel = player.transform;
            Debug.Log("✅ Character found: " + player.name);
            
            // Apply outfit immediately after finding character
            if (currentOutfit != null)
            {
                ApplyOutfitToCharacter();
            }
        }
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
        
        // Set new outfit
        currentOutfit = outfit;
        EnableOutfit(outfit);
        
        // Auto-save after equipping
        SaveOutfit();
        
        // Apply to current character in scene
        ApplyOutfitToCharacter();
        
        Debug.Log("✅ Equipped outfit: " + outfit.outfitName);
    }
    
    /// <summary>
    /// Apply the current outfit to the character in the scene
    /// </summary>
    public void ApplyOutfitToCharacter()
    {
        if (currentOutfit == null)
        {
            Debug.LogWarning("No outfit to apply!");
            return;
        }
        
        if (characterModel == null)
        {
            Debug.LogWarning("Character not found! Looking for character...");
            FindCharacter();
            return;
        }
        
        // Find all clothing items on the character
        // This searches for clothing GameObjects by their names
        ApplyClothingPiece(currentOutfit.topClothing, "Top", "Shirt", "Upperbody");
        ApplyClothingPiece(currentOutfit.bottomClothing, "Bottom", "Pants", "Skirt", "Lowerbody");
        ApplyClothingPiece(currentOutfit.shoesClothing, "Shoes", "Footwear");
        ApplyClothingPiece(currentOutfit.accessory, "Accessory", "Hat", "Hair");
        
        Debug.Log("👕 Outfit applied to character: " + currentOutfit.outfitName);
    }
    
    /// <summary>
    /// Apply a single clothing piece by finding matching GameObject on character
    /// </summary>
    void ApplyClothingPiece(GameObject clothingPrefab, params string[] searchNames)
    {
        if (clothingPrefab == null) return;
        
        // First, disable all clothing items that match the search names
        foreach (string searchName in searchNames)
        {
            Transform[] allChildren = characterModel.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.name.Contains(searchName))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        
        // Now enable the specific clothing piece
        string clothingName = clothingPrefab.name;
        Transform[] children = characterModel.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform child in children)
        {
            if (child.name.Contains(clothingName) || child.name == clothingName)
            {
                child.gameObject.SetActive(true);
                Debug.Log("👕 Enabled: " + child.name);
                return;
            }
        }
        
        Debug.LogWarning("⚠️ Could not find clothing: " + clothingName + " on character");
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
        
        Debug.Log("💾 Outfit saved: " + currentOutfit.outfitName);
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
                currentOutfit = outfit;
                Debug.Log("📂 Outfit loaded: " + outfit.outfitName);
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
            currentOutfit = allOutfits[0];
            SaveOutfit();
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
            Debug.Log("🔓 Unlocked outfit: " + outfit.outfitName);
        }
    }
    
    // Reset to default outfit
    public void ResetOutfit()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        LoadDefaultOutfit();
        ApplyOutfitToCharacter();
        Debug.Log("🔄 Outfit reset to default");
    }
    
    /// <summary>
    /// Called when this object is enabled
    /// </summary>
    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    /// <summary>
    /// Called when this object is disabled
    /// </summary>
    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Called when scene changes - reapply outfit to new character
    /// </summary>
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        characterModel = null; // Reset character reference
        FindCharacter(); // Find character in new scene
    }
}