using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CostumeButtonsSelector : MonoBehaviour
{
    // Optional: Show which costume is selected
    public TMP_Text selectedDisplayText;

    // --- BUTTON FUNCTIONS ---
    // Each button calls one of these functions
    // Button 0 should call SelectCostume0(), Button 1 calls SelectCostume1(), etc.

    public void SelectCostume0()
    {
        SendCostumeIndex(0);
    }

    public void SelectCostume1()
    {
        SendCostumeIndex(1);
    }

    public void SelectCostume2()
    {
        SendCostumeIndex(2);
    }

    public void SelectCostume3()
    {
        SendCostumeIndex(3);
    }

    public void SelectCostume4()
    {
        SendCostumeIndex(4);
    }

    public void SelectCostume5()
    {
        SendCostumeIndex(5);
    }

    // Add more if you need more costumes
    public void SelectCostume6()
    {
        SendCostumeIndex(6);
    }

    public void SelectCostume7()
    {
        SendCostumeIndex(7);
    }

    // --- CORE FUNCTION ---
    // This is what actually sends/saves the index
    private void SendCostumeIndex(int index)
    {
        // Save the index to PlayerPrefs
        PlayerPrefs.SetInt("SelectedCostume", index);
        PlayerPrefs.Save();

        // Update display if available
        UpdateDisplay(index);

        // Debug log to verify it's working
        Debug.Log($"Costume index {index} saved to PlayerPrefs");
    }

    // Optional: Update UI display
    private void UpdateDisplay(int index)
    {
        if (selectedDisplayText != null)
        {
            selectedDisplayText.text = $"Costume {index + 1} Selected";
        }
    }

    // Optional: Start button function
    public void StartGame(string sceneName)
    {
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}