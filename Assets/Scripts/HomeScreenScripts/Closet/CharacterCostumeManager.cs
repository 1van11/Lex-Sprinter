using UnityEngine;
using UnityEngine.UI;

public class CharacterCostumeManager : MonoBehaviour
{
    [Header("Girl Costumes")]
    public GameObject[] girlCostumeModels;  // DefaultGirl, RainyGirl, ChristmasGirl

    [Header("Boy Costumes")]
    public GameObject[] boyCostumeModels;   // DefaultBoy, RainyBoy, ChristmasBoy

    [Header("Closet UI Images")]
    public Image[] outfitSlots;             // Drag your 3 lock/outfit UI images here

    [Header("Girl Outfit Sprites")]
    public Sprite[] girlSprites;            // 3 girl outfit sprites

    [Header("Boy Outfit Sprites")]
    public Sprite[] boySprites;             // 3 boy outfit sprites

    int lastCostumeIndex = -1;
    bool isGirl;

    void Start()
    {
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 1);
        isGirl = (selected == 2);
    }

    void OnEnable()
    {
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 1);
        isGirl = (selected == 2);

        // Swap UI images when panel opens
        UpdateClosetUI();
        ApplySavedCostume();
    }

    void Update()
    {
        int currentIndex = PlayerPrefs.GetInt("EquippedCostume", 0);
        if (currentIndex != lastCostumeIndex)
        {
            UpdateCostume(currentIndex);
            lastCostumeIndex = currentIndex;
        }
    }

    public void SetCostume(int index)
    {
        PlayerPrefs.SetInt("EquippedCostume", index);
        PlayerPrefs.Save();
        UpdateCostume(index);
        lastCostumeIndex = index;
    }

    void ApplySavedCostume()
    {
        int index = PlayerPrefs.GetInt("EquippedCostume", 0);
        UpdateCostume(index);
        lastCostumeIndex = index;
    }

    void UpdateCostume(int index)
    {
        GameObject[] activeSet   = isGirl ? girlCostumeModels : boyCostumeModels;
        GameObject[] inactiveSet = isGirl ? boyCostumeModels : girlCostumeModels;

        foreach (var obj in inactiveSet)
            if (obj != null) obj.SetActive(false);

        for (int i = 0; i < activeSet.Length; i++)
            if (activeSet[i] != null)
                activeSet[i].SetActive(i == index);
    }

    // NEW: swaps the UI images based on gender
    void UpdateClosetUI()
    {
        Sprite[] sprites = isGirl ? girlSprites : boySprites;

        for (int i = 0; i < outfitSlots.Length; i++)
        {
            if (outfitSlots[i] != null && i < sprites.Length)
                outfitSlots[i].sprite = sprites[i];
        }
    }
}