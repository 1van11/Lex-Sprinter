using UnityEngine;
using TMPro;
using UnityEngine.UI; // needed for LayoutRebuilder
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Leaderboard UI References")]
    public Transform contentParent;              // Content inside ScrollView
    public GameObject leaderboardItemPrefab;     // Prefab: RankText, NameText, DistanceText

    [Header("Player Info")]
    public string currentPlayerName = "Player";
    public float currentDistance;

    private List<PlayerData> playerList = new List<PlayerData>();

    void Start()
    {
        // Load all saved records
        LoadLeaderboard();

        // Get actual player info from PlayerPrefs
        currentPlayerName = PlayerPrefs.GetString("PlayerName", "Player");
        currentDistance = PlayerPrefs.GetFloat("LatestDistance", 0);

        // Only add the current run if it hasn't been added yet
        if (!HasScoreBeenRecorded(currentPlayerName, currentDistance))
        {
            SavePlayerScore(currentPlayerName, currentDistance);
        }
        else
        {
            // Still refresh leaderboard display
            DisplayLeaderboard();
        }
    }

    // Check if this exact score is already in playerList
    private bool HasScoreBeenRecorded(string playerName, float distance)
    {
        foreach (var pd in playerList)
        {
            if (pd.playerName == playerName && Mathf.Approximately(pd.distance, distance))
                return true;
        }
        return false;
    }

    // Always adds a new score
    public void SavePlayerScore(string playerName, float distance)
    {
        PlayerData newRecord = new PlayerData(playerName, distance);
        playerList.Add(newRecord);

        // Save to PlayerPrefs
        int recordCount = PlayerPrefs.GetInt("LeaderboardRecordCount", 0);
        PlayerPrefs.SetString($"LeaderboardRecord_{recordCount}_Name", playerName);
        PlayerPrefs.SetFloat($"LeaderboardRecord_{recordCount}_Distance", distance);
        PlayerPrefs.SetInt("LeaderboardRecordCount", recordCount + 1);
        PlayerPrefs.Save();

        // Sort descending (highest distance first)
        playerList.Sort((a, b) => b.distance.CompareTo(a.distance));

        // Refresh display
        DisplayLeaderboard();
    }

    // Display only top 10
    void DisplayLeaderboard()
    {
        // Clear previous entries
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        int topCount = Mathf.Min(10, playerList.Count);
        for (int i = 0; i < topCount; i++)
        {
            GameObject item = Instantiate(leaderboardItemPrefab, contentParent);
            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();

            if (texts.Length >= 3)
            {
                texts[0].text = (i + 1).ToString();                          // Rank
                texts[1].text = playerList[i].playerName;                    // Name
                texts[2].text = playerList[i].distance.ToString("F1") + "m"; // Distance

                // Highlight current player's entries
                if (playerList[i].playerName == currentPlayerName)
                    texts[1].color = Color.yellow;
            }
            else
            {
                Debug.LogError($"Leaderboard prefab needs 3 TMP_Text components. Found {texts.Length}");
            }
        }

        // Force layout rebuild so Vertical Layout Group + ContentSizeFitter works at Start
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
    }

    // Load all saved records
    void LoadLeaderboard()
    {
        playerList.Clear();

        int recordCount = PlayerPrefs.GetInt("LeaderboardRecordCount", 0);
        for (int i = 0; i < recordCount; i++)
        {
            string keyName = $"LeaderboardRecord_{i}_Name";
            string keyDistance = $"LeaderboardRecord_{i}_Distance";

            if (PlayerPrefs.HasKey(keyName) && PlayerPrefs.HasKey(keyDistance))
            {
                string playerName = PlayerPrefs.GetString(keyName);
                float distance = PlayerPrefs.GetFloat(keyDistance);
                playerList.Add(new PlayerData(playerName, distance));
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
