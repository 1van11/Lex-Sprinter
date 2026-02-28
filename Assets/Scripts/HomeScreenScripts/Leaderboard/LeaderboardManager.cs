using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Leaderboard UI References")]
    public Transform contentParent;
    public GameObject leaderboardItemPrefab;

    [Header("Player Info")]
    public string currentPlayerName = "Player";
    public int currentWordCount;

    private DatabaseReference dbReference;

    private void Start()
    {

        currentPlayerName = PlayerPrefs.GetString("PlayerName", "Player");
        currentWordCount = PlayerPrefs.GetInt("LatestWordCount", 0);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("✅ Firebase Connected Successfully!");

                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                UploadScore();
            }
            else
            {
                Debug.LogError("❌ Firebase NOT Ready: " + task.Result);
            }
        });
    }
    // Call this when opening the leaderboard panel
    public void OnLeaderboardPanelOpen()
    {
        if (dbReference != null)
            LoadLeaderboard();
    }

    // 🔥 Upload or Update Score (Option 2)
    public void UploadScore()
    {
        DatabaseReference leaderboardRef = dbReference.Child("Leaderboard");

        leaderboardRef.OrderByChild("username").EqualTo(currentPlayerName)
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompleted)
                    return;

                if (task.Result.Exists)
                {
                    // Player already exists
                    foreach (DataSnapshot child in task.Result.Children)
                    {
                        int oldScore = 0;

                        if (child.Child("score").Exists)
                            oldScore = int.Parse(child.Child("score").Value.ToString());

                        if (currentWordCount > oldScore)
                        {
                            child.Reference.Child("score").SetValueAsync(currentWordCount);
                            Debug.Log("High score updated!");
                        }
                        else
                        {
                            Debug.Log("Score not higher than existing high score.");
                        }
                    }
                }
                else
                {
                    // Player does NOT exist → create new entry
                    Dictionary<string, object> newData = new Dictionary<string, object>();
                    newData["username"] = currentPlayerName;
                    newData["score"] = currentWordCount;

                    leaderboardRef.Push().SetValueAsync(newData);
                    Debug.Log("New player added to leaderboard.");
                }

                // After upload/update → reload leaderboard
                LoadLeaderboard();
            });
    }


    // 🔥 Load Top 10 Scores
    private void LoadLeaderboard()
    {
        dbReference.Child("Leaderboard")
            .OrderByChild("score")
            .LimitToLast(10)
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompleted)
                    return;

                foreach (Transform child in contentParent)
                    Destroy(child.gameObject);

                DataSnapshot snapshot = task.Result;

                List<DataSnapshot> entries = new List<DataSnapshot>();

                foreach (DataSnapshot child in snapshot.Children)
                    entries.Add(child);

                // Reverse so highest score appears first
                entries.Reverse();

                int rank = 1;

                foreach (DataSnapshot entry in entries)
                {
                    if (!entry.Child("username").Exists || !entry.Child("score").Exists)
                        continue;

                    GameObject item = Instantiate(leaderboardItemPrefab, contentParent);
                    TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();

                    string username = entry.Child("username").Value.ToString();
                    string score = entry.Child("score").Value.ToString();

                    texts[0].text = rank.ToString();
                    texts[1].text = username;
                    texts[2].text = score + " Words";

                    if (username == currentPlayerName)
                        texts[1].color = Color.yellow;

                    rank++;
                }
            });
    }
}