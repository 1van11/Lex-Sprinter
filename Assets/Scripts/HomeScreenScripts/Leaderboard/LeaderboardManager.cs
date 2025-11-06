using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Leaderboard UI References")]
    public Transform contentParent;              // Content inside ScrollView
    public GameObject leaderboardItemPrefab;     // Prefab: RankText, NameText, DistanceText

    [Header("Player Info")]
    public string currentPlayerName = "Player";  // Temporary name, can be set from another script
    public float currentDistance;                // Current distance from your gameplay script

    private List<PlayerData> playerList = new List<PlayerData>();

    void Start()
    {
        LoadLeaderboard();

        // Get the actual player name from PlayerPrefs (saved by HomeScreenName)
        currentPlayerName = PlayerPrefs.GetString("PlayerName", "Player");

        // Get the current distance from PlayerFunctions
        currentDistance = PlayerPrefs.GetFloat("LatestDistance", 0);

        SavePlayerScore(currentPlayerName, currentDistance);
        DisplayLeaderboard();
    }

    // 🧩 Save or Update Player's Distance
    public void SavePlayerScore(string playerName, float distance)
    {
        bool found = false;

        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].playerName == playerName)
            {
                found = true;
                if (distance > playerList[i].distance)
                {
                    playerList[i].distance = distance;
                    PlayerPrefs.SetFloat("Distance_" + playerName, distance);
                    PlayerPrefs.Save();
                }
                break;
            }
        }

        // New player
        if (!found)
        {
            playerList.Add(new PlayerData(playerName, distance));

            // Save player name to tracking list
            int playerCount = PlayerPrefs.GetInt("LeaderboardPlayerCount", 0);
            PlayerPrefs.SetString("LeaderboardPlayer_" + playerCount, playerName);
            PlayerPrefs.SetInt("LeaderboardPlayerCount", playerCount + 1);

            PlayerPrefs.SetString("Name_" + playerName, playerName);
            PlayerPrefs.SetFloat("Distance_" + playerName, distance);
            PlayerPrefs.Save();
        }

        // Sort and refresh display
        playerList.Sort((a, b) => b.distance.CompareTo(a.distance));
        DisplayLeaderboard();
    }

    // 🧩 Display Leaderboard
    void DisplayLeaderboard()
    {
        Debug.Log($"📊 DisplayLeaderboard called! Player count: {playerList.Count}");

        // Clear old entries
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Create new rows
        for (int i = 0; i < playerList.Count; i++)
        {
            Debug.Log($"Creating row {i}: {playerList[i].playerName} - {playerList[i].distance}m");

            GameObject item = Instantiate(leaderboardItemPrefab, contentParent);
            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();

            Debug.Log($"Found {texts.Length} TextMeshPro components");

            if (texts.Length >= 3)
            {
                texts[0].text = (i + 1).ToString();                          // Rank
                texts[1].text = playerList[i].playerName;                    // Name
                texts[2].text = playerList[i].distance.ToString("F1") + "m"; // Distance

                Debug.Log($"✅ Set text - Rank: {texts[0].text}, Name: {texts[1].text}, Distance: {texts[2].text}");
            }
            else
            {
                Debug.LogError($"❌ Not enough TextMeshPro components! Found: {texts.Length}, Need: 3");
            }
        }
    }

    // 🧩 Load from PlayerPrefs
    void LoadLeaderboard()
    {
        playerList.Clear();

        // Get the count of players saved
        int playerCount = PlayerPrefs.GetInt("LeaderboardPlayerCount", 0);

        // Load each saved player
        for (int i = 0; i < playerCount; i++)
        {
            string playerKey = "LeaderboardPlayer_" + i;
            if (PlayerPrefs.HasKey(playerKey))
            {
                string playerName = PlayerPrefs.GetString(playerKey);
                if (PlayerPrefs.HasKey("Distance_" + playerName))
                {
                    float dist = PlayerPrefs.GetFloat("Distance_" + playerName);
                    playerList.Add(new PlayerData(playerName, dist));
                }
            }
        }

        // Sort descending
        playerList.Sort((a, b) => b.distance.CompareTo(a.distance));
    }
}

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public float distance;

    public PlayerData(string name, float dist)
    {
        playerName = name;
        distance = dist;
    }
}