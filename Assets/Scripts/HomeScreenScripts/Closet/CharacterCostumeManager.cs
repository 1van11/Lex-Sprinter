using UnityEngine;

public class CharacterCostumeManager : MonoBehaviour
{
    public GameObject[] costumeModels;
    
    void Start()
    {
        int currentCostumeIndex = PlayerPrefs.GetInt("EquippedCostume", 0);
        UpdateCostume(currentCostumeIndex);
    }

    public void SetCostume(int index)
    {
        // Save the costume index
        PlayerPrefs.SetInt("EquippedCostume", index);
        PlayerPrefs.Save();
        
        // Swap the model in the closet scene
        UpdateCostume(index);
    }

    public void UpdateCostume(int index)
    {
        for (int i = 0; i < costumeModels.Length; i++)
        {
            if (costumeModels[i] != null)
            {
                costumeModels[i].SetActive(i == index);
            }
        }
    }
}