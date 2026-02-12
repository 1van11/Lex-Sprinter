using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Dan.Main;

namespace LexSprinterLeaderboard
{
    public class LeaderboardManager : MonoBehaviour
    {
        [Header("Leaderboard UI References")]
        public Transform contentParent;
        public GameObject leaderboardItemPrefab;

        [Header("Player Info")]
        public string currentPlayerName = "Player";
        public int currentWordCount;

        private void Start()
        {
            currentPlayerName = PlayerPrefs.GetString("PlayerName", "Player");
            currentWordCount = PlayerPrefs.GetInt("LatestWordCount", 0);

            UploadScore();
            LoadLeaderboard();
        }

        // 🔥 Upload player score
        public void UploadScore()
        {
            Leaderboards.LexSprinterLeaderboard.UploadNewEntry(
                currentPlayerName,
                currentWordCount,
                isSuccessful =>
                {
                    if (isSuccessful)
                        LoadLeaderboard();
                });
        }

        // 🔥 Load Top 10 Entries
        private void LoadLeaderboard()
        {
            Leaderboards.LexSprinterLeaderboard.GetEntries(entries =>
            {
                foreach (Transform child in contentParent)
                    Destroy(child.gameObject);

                int length = Mathf.Min(10, entries.Length);

                for (int i = 0; i < length; i++)
                {
                    GameObject item = Instantiate(leaderboardItemPrefab, contentParent);
                    TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();

                    if (texts.Length >= 3)
                    {
                        texts[0].text = entries[i].Rank.ToString();
                        texts[1].text = entries[i].Username;
                        texts[2].text = entries[i].Score + " Words";

                        if (entries[i].Username == currentPlayerName)
                            texts[1].color = Color.yellow;
                    }
                }
            });
        }
    }
}
