using UnityEngine;

public class CharacterCostumeManager : MonoBehaviour
{
    public GameObject[] costumeModels;

    int lastCostumeIndex = -1;

    void OnEnable()
    {
        ApplySavedCostume();
    }

    void Update()
    {
        // Detect change and update instantly
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
        for (int i = 0; i < costumeModels.Length; i++)
        {
            if (costumeModels[i] != null)
                costumeModels[i].SetActive(i == index);
        }
    }
}
