using UnityEngine;

public class CharacterCostumeManager : MonoBehaviour
{
    public GameObject[] costumeModels;
    
    void Start()
    {
        // Only load the saved outfit when the game starts
        int currentCostumeIndex = PlayerPrefs.GetInt("EquippedCostume", 0);
        UpdateCostume(currentCostumeIndex);
    }

    // ADDED THIS: This fixes the "<Missing CharacterCostumeManager.SetCostume>" error in your buttons
    public void SetCostume(int index)
    {
        // Save the index so it remembers the outfit next time you play
        PlayerPrefs.SetInt("EquippedCostume", index);
        PlayerPrefs.Save();
        
        // Actually swap the 3D model
        UpdateCostume(index);
    }

    // This name must stay exactly like this so your ClosetManager script doesn't crash
    public void UpdateCostume(int index)
    {
        for (int i = 0; i < costumeModels.Length; i++)
        {
            if (costumeModels[i] != null)
            {
                // Only the model matching the index stays active
                costumeModels[i].SetActive(i == index);
            }
        }
    }
}