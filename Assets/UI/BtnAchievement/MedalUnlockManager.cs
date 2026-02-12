using UnityEngine;
using TMPro;

public class MedalUnlockManager : MonoBehaviour
{
    [Header("Unlock Requirements")]
    public int easyRequired = 25;
    public int mediumRequired = 50;
    public int hardRequired = 100;

    [Header("Reward Coins")]
    public int easyReward = 500;
    public int mediumReward = 750;
    public int hardReward = 1000;

    private string currentMedalKey = "";

    [Header("UI")]
    public GameObject unlockedPanel;
    public TMP_Text centerMessageText;

    [Header("Unlocked Panel UI")]
    public GameObject claimButton;


    private int totalWords;

    void Start()
    {
        totalWords = PlayerPrefs.GetInt("TotalWordCount", 0);

        if (unlockedPanel != null)
            unlockedPanel.SetActive(false);

        if (centerMessageText != null)
            centerMessageText.gameObject.SetActive(false);
    }

    public void CheckEasyMedal()
    {
        CheckMedal(easyRequired, "easy medal");
    }

    public void CheckMediumMedal()
    {
        CheckMedal(mediumRequired, "medium medal");
    }

    public void CheckHardMedal()
    {
        CheckMedal(hardRequired, "hard medal");
    }

    void CheckMedal(int requiredWords, string medalName)
    {
        totalWords = PlayerPrefs.GetInt("TotalWordCount", 0);

        string claimedKey = medalName + "_claimed";
        bool alreadyClaimed = PlayerPrefs.GetInt(claimedKey, 0) == 1;

        if (totalWords >= requiredWords)
        {
            if (alreadyClaimed)
            {
                // 🏅 Already claimed → NO PANEL
                unlockedPanel.SetActive(false);
                claimButton.SetActive(false);

                centerMessageText.gameObject.SetActive(true);
                centerMessageText.text =
                    medalName + " already claimed!\n\nCongratulations!";
            }
            else
            {
                // ✅ Unlocked and NOT claimed
                unlockedPanel.SetActive(true);
                claimButton.SetActive(true);

                centerMessageText.gameObject.SetActive(false);

                currentMedalKey = medalName;
            }
        }
        else
        {
            // 🔒 Not unlocked yet
            unlockedPanel.SetActive(false);
            claimButton.SetActive(false);

            centerMessageText.gameObject.SetActive(true);
            centerMessageText.text =
                "You need to reach " + requiredWords +
                " words to unlock this medal.\n\nCurrent Words: " + totalWords;
        }
    }


    public void ClaimReward()
    {
        if (string.IsNullOrEmpty(currentMedalKey))
            return;

        string claimedKey = currentMedalKey + "_claimed";

        if (PlayerPrefs.GetInt(claimedKey, 0) == 1)
            return;

        int rewardAmount = 0;

        if (currentMedalKey == "easy medal")
            rewardAmount = easyReward;
        else if (currentMedalKey == "medium medal")
            rewardAmount = mediumReward;
        else if (currentMedalKey == "hard medal")
            rewardAmount = hardReward;

        CoinsDisplay.Instance.AddCoins(rewardAmount);

        PlayerPrefs.SetInt(claimedKey, 1);
        PlayerPrefs.Save();

        unlockedPanel.SetActive(false);
        claimButton.SetActive(false);
    }


}
