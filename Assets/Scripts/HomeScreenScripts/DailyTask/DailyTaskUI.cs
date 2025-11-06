using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyTaskUI : MonoBehaviour
{
    [Header("Task 1 UI")]
    public TMP_Text task1Description;
    public TMP_Text task1Reward;
    public Button task1ClaimButton;
    public TMP_Text task1ClaimText;
    public GameObject task1CompletedIcon;

    [Header("Task 2 UI")]
    public TMP_Text task2Description;
    public TMP_Text task2Reward;
    public Button task2ClaimButton;
    public TMP_Text task2ClaimText;
    public GameObject task2CompletedIcon;

    [Header("Task 3 UI")]
    public TMP_Text task3Description;
    public TMP_Text task3Reward;
    public Button task3ClaimButton;
    public TMP_Text task3ClaimText;
    public GameObject task3CompletedIcon;

    [Header("Reset Timer UI")]
    public TMP_Text resetTimerText;

    [Header("Coin Display Reference")]
    public CoinsDisplay coinsDisplay;

    [Header("Button Text")]
    public string claimText = "CLAIM";
    public string claimedText = "CLAIMED";

    [Header("Timer Settings")]
    public bool updateTimerRealtime = true;

    void Start()
    {
        RefreshTaskUI();

        // Add button listeners for CLAIMING rewards
        if (task1ClaimButton != null)
            task1ClaimButton.onClick.AddListener(() => ClaimTaskReward(0));
        if (task2ClaimButton != null)
            task2ClaimButton.onClick.AddListener(() => ClaimTaskReward(1));
        if (task3ClaimButton != null)
            task3ClaimButton.onClick.AddListener(() => ClaimTaskReward(2));

        if (updateTimerRealtime)
            InvokeRepeating(nameof(UpdateResetTimer), 0f, 1f);
    }

    void OnEnable()
    {
        RefreshTaskUI();
    }

    void OnDisable()
    {
        CancelInvoke(nameof(UpdateResetTimer));
    }

    void UpdateResetTimer()
    {
        if (resetTimerText == null || DailyTaskManager.Instance == null)
            return;

        string formattedTime = DailyTaskManager.Instance.GetFormattedTimeUntilReset();
        resetTimerText.text = $"Resets in: {formattedTime}";
    }

    public void RefreshTaskUI()
    {
        if (DailyTaskManager.Instance == null)
        {
            Debug.LogWarning("⚠️ DailyTaskManager not found!");
            return;
        }

        UpdateTaskSlot(0, task1Description, task1Reward, task1ClaimButton, task1ClaimText, task1CompletedIcon);
        UpdateTaskSlot(1, task2Description, task2Reward, task2ClaimButton, task2ClaimText, task2CompletedIcon);
        UpdateTaskSlot(2, task3Description, task3Reward, task3ClaimButton, task3ClaimText, task3CompletedIcon);

        UpdateResetTimer();

        Debug.Log("🔄 Daily Task UI refreshed");
    }

    void UpdateTaskSlot(int taskID, TMP_Text description, TMP_Text reward, Button claimButton, TMP_Text claimButtonText, GameObject completedIcon)
    {
        DailyTask task = DailyTaskManager.Instance.GetTask(taskID);

        if (task == null)
        {
            Debug.LogWarning($"⚠️ Task {taskID} not found!");
            return;
        }

        // Update description - shows "Get the word: dog"
        if (description != null)
            description.text = task.taskDescription;

        // Update reward
        if (reward != null)
            reward.text = $"+{task.coinReward}";

        // Update button state based on completion and claim status
        if (task.isClaimed)
        {
            // Task claimed - button disabled, shows "CLAIMED"
            if (claimButton != null)
                claimButton.interactable = false;

            if (claimButtonText != null)
                claimButtonText.text = claimedText;

            if (completedIcon != null)
                completedIcon.SetActive(true);
        }
        else if (task.isCompleted)
        {
            // Task completed but not claimed - button ENABLED, shows "CLAIM"
            if (claimButton != null)
                claimButton.interactable = true;

            if (claimButtonText != null)
                claimButtonText.text = claimText;

            if (completedIcon != null)
                completedIcon.SetActive(false);
        }
        else
        {
            // Task not completed - button DISABLED, shows "CLAIM"
            if (claimButton != null)
                claimButton.interactable = false;

            if (claimButtonText != null)
                claimButtonText.text = claimText;

            if (completedIcon != null)
                completedIcon.SetActive(false);
        }
    }

    /// <summary>
    /// Claims the reward for a completed task (does NOT load gameplay)
    /// </summary>
    void ClaimTaskReward(int taskID)
    {
        if (DailyTaskManager.Instance == null)
            return;

        bool success = DailyTaskManager.Instance.ClaimTaskReward(taskID);

        if (success)
        {
            // Refresh UI to show claimed state
            RefreshTaskUI();

            // Update coin display
            if (coinsDisplay != null)
                coinsDisplay.RefreshCoinDisplay();

            Debug.Log($"✅ Task {taskID} reward claimed!");
        }
    }

    /// <summary>
    /// Manual refresh button (optional)
    /// </summary>
    public void ManualRefresh()
    {
        if (DailyTaskManager.Instance != null)
        {
            string lastResetDate = PlayerPrefs.GetString("LastTaskResetDate", "");
            string todayDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            if (lastResetDate != todayDate)
            {
                Debug.Log("🔄 New day detected on manual refresh!");
                DailyTaskManager.Instance.ForceResetTasks();
            }
        }

        RefreshTaskUI();
    }

    /// <summary>
    /// TESTING: Force clear all task data and regenerate
    /// </summary>
    [ContextMenu("Clear and Regenerate Tasks")]
    public void ClearAndRegenerateTasks()
    {
        PlayerPrefs.DeleteKey("LastTaskResetDate");
        PlayerPrefs.DeleteKey("DailyTaskData");
        PlayerPrefs.Save();

        if (DailyTaskManager.Instance != null)
        {
            DailyTaskManager.Instance.ForceResetTasks();
        }

        RefreshTaskUI();
        Debug.Log("🔄 Tasks cleared and regenerated!");
    }
}