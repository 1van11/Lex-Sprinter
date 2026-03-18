using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class DailyTask
{
    public int taskID;
    public string taskDescription;
    public string correctAnswer;
    public int coinReward;
    public bool isCompleted;
    public bool isClaimed;
    public int questionIndex;
    public bool isSpellingQuestion;
    public string difficulty; // "easy", "medium", "hard"
}

public class DailyTaskManager : MonoBehaviour
{
    public static DailyTaskManager Instance { get; private set; }

    [Header("Task Settings")]
    public int numberOfDailyTasks = 3;
    public int[] taskRewards = new int[] { 50, 60, 70 };

    [Header("Save Keys")]
    private const string LAST_RESET_DATE_KEY = "LastTaskResetDate";
    private const string TASK_DATA_KEY = "DailyTaskData";

    public List<DailyTask> dailyTasks = new List<DailyTask>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CheckAndResetDailyTasks();
    }

    void CheckAndResetDailyTasks()
    {
        string lastResetDate = PlayerPrefs.GetString(LAST_RESET_DATE_KEY, "");
        string todayDate = DateTime.Now.ToString("yyyy-MM-dd");

        if (lastResetDate != todayDate)
        {
            Debug.Log("🔄 New day detected! Generating new daily tasks...");
            GenerateNewDailyTasks();
            PlayerPrefs.SetString(LAST_RESET_DATE_KEY, todayDate);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.Log("📋 Loading existing daily tasks...");
            LoadDailyTasks();
        }
    }

    public TimeSpan GetTimeUntilReset()
    {
        DateTime now = DateTime.Now;
        DateTime nextMidnight = now.Date.AddDays(1);
        return nextMidnight - now;
    }

    public string GetFormattedTimeUntilReset()
    {
        TimeSpan t = GetTimeUntilReset();
        return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
    }

    void GenerateNewDailyTasks()
    {
        dailyTasks.Clear();

        // Task 0 → Easy, Task 1 → Medium, Task 2 → Hard
        string[] difficulties = new string[] { "easy", "medium", "hard" };
        string[][,] pools = new string[][,]
        {
            QuestionRandomizer.easySpellingPairs,
            QuestionRandomizer.mediumSpellingPairs,
            QuestionRandomizer.hardSpellingPairs
        };

        for (int i = 0; i < numberOfDailyTasks; i++)
        {
            string[,] pool = pools[i];
            string diff = difficulties[i];

            int randomIndex = UnityEngine.Random.Range(0, pool.GetLength(0));
            string correctAnswer = pool[randomIndex, 1];

            DailyTask newTask = new DailyTask();
            newTask.taskID = i;
            newTask.coinReward = taskRewards[i];
            newTask.isCompleted = false;
            newTask.isClaimed = false;
            newTask.isSpellingQuestion = true;
            newTask.questionIndex = randomIndex;
            newTask.correctAnswer = correctAnswer;
            newTask.difficulty = diff;
            newTask.taskDescription = "Get the word: \"" + correctAnswer + "\"";

            dailyTasks.Add(newTask);
        }

        SaveDailyTasks();
        Debug.Log("✅ Generated " + dailyTasks.Count + " new daily tasks!");
    }

    void SaveDailyTasks()
    {
        string json = JsonUtility.ToJson(new TaskListWrapper { tasks = dailyTasks });
        PlayerPrefs.SetString(TASK_DATA_KEY, json);
        PlayerPrefs.Save();
    }

    void LoadDailyTasks()
    {
        string json = PlayerPrefs.GetString(TASK_DATA_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            TaskListWrapper wrapper = JsonUtility.FromJson<TaskListWrapper>(json);
            dailyTasks = wrapper.tasks;
            Debug.Log("📋 Loaded " + dailyTasks.Count + " daily tasks");
        }
        else
        {
            Debug.LogWarning("⚠️ No saved tasks found, generating new ones...");
            GenerateNewDailyTasks();
        }
    }

    public bool CheckAndCompleteTask(string correctAnswer)
    {
        foreach (DailyTask task in dailyTasks)
        {
            if (task.correctAnswer == correctAnswer && !task.isCompleted)
            {
                task.isCompleted = true;
                SaveDailyTasks();
                Debug.Log("🎯 Daily Task completed: \"" + correctAnswer + "\" - Can now claim " + task.coinReward + " coins!");
                return true;
            }
        }
        return false;
    }

    public bool ClaimTaskReward(int taskID)
    {
        DailyTask task = GetTask(taskID);

        if (task == null)
        {
            Debug.LogWarning("⚠️ Task " + taskID + " not found!");
            return false;
        }
        if (!task.isCompleted)
        {
            Debug.LogWarning("⚠️ Task " + taskID + " not completed yet!");
            return false;
        }
        if (task.isClaimed)
        {
            Debug.LogWarning("⚠️ Task " + taskID + " already claimed!");
            return false;
        }

        int currentCoins = PlayerPrefs.GetInt("PlayerTotalCoins", 0);
        currentCoins += task.coinReward;
        PlayerPrefs.SetInt("PlayerTotalCoins", currentCoins);
        PlayerPrefs.Save();

        task.isClaimed = true;
        SaveDailyTasks();

        Debug.Log("💰 Task " + taskID + " claimed! Awarded " + task.coinReward + " coins. Total: " + currentCoins);
        return true;
    }

    public DailyTask GetTask(int taskID)
    {
        if (taskID >= 0 && taskID < dailyTasks.Count)
            return dailyTasks[taskID];
        return null;
    }

    public List<DailyTask> GetAllTasks()
    {
        return dailyTasks;
    }

    public bool AreAllTasksCompleted()
    {
        foreach (DailyTask task in dailyTasks)
            if (!task.isCompleted) return false;
        return true;
    }

    [ContextMenu("Force Reset Tasks")]
    public void ForceResetTasks()
    {
        PlayerPrefs.DeleteKey(LAST_RESET_DATE_KEY);
        PlayerPrefs.DeleteKey(TASK_DATA_KEY);
        GenerateNewDailyTasks();
        Debug.Log("🔄 Tasks force reset!");
    }
}

[System.Serializable]
public class TaskListWrapper
{
    public List<DailyTask> tasks;
}