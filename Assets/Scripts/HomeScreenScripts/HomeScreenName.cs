using UnityEngine;
using TMPro;

public class HomeScreenName : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI welcomeText; // Optional: for "Welcome, [Name]!"

    private const string PlayerNameKey = "PlayerName";
    private const string SelectedCharacterKey = "SelectedCharacter";

    void Start()
    {
        LoadAndDisplayPlayerInfo();
    }

    void LoadAndDisplayPlayerInfo()
    {
        // Retrieve the saved player name
        string playerName = PlayerPrefs.GetString(PlayerNameKey, "Player");

        // Display the name
        if (playerNameText != null)
        {
            playerNameText.text = playerName;
        }

        // Optional: Display welcome message
        if (welcomeText != null)
        {
            welcomeText.text = $"Welcome, {playerName}!";
        }

        // Optional: Get selected character if you need it
        int selectedCharacter = PlayerPrefs.GetInt(SelectedCharacterKey, 1);
        Debug.Log($"Player: {playerName}, Selected Character: {selectedCharacter}");
    }

    // Optional: Method to update the display if name changes
    public void RefreshPlayerName()
    {
        LoadAndDisplayPlayerInfo();
    }
}