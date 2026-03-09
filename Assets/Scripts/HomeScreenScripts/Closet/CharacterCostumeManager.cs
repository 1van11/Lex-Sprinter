using UnityEngine;

public class CharacterCostumeManager : MonoBehaviour
{
    [Header("Girl Costumes")]
    public GameObject[] girlCostumeModels;  // DefaultGirl, RainyGirl, ChristmasGirl

    [Header("Boy Costumes")]
    public GameObject[] boyCostumeModels;   // DefaultBoy, RainyBoy, ChristmasBoy

    int lastCostumeIndex = -1;
    bool isGirl;

    void Start()
    {
        // Read which character was selected
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 1);
        isGirl = (selected == 2);
    }

    void OnEnable()
    {
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 1);
        isGirl = (selected == 2);
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
        // Pick the correct array based on gender
        GameObject[] activeSet  = isGirl ? girlCostumeModels : boyCostumeModels;
        GameObject[] inactiveSet = isGirl ? boyCostumeModels : girlCostumeModels;

        // Hide ALL of the opposite gender
        foreach (var obj in inactiveSet)
            if (obj != null) obj.SetActive(false);

        // Show only selected costume of current gender
        for (int i = 0; i < activeSet.Length; i++)
            if (activeSet[i] != null)
                activeSet[i].SetActive(i == index);
    }
}