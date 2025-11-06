using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class DailyTask
{
    public int taskID; // 0, 1, 2 for the 3 tasks
    public string taskDescription; // "Get the word: dog"
    public string correctAnswer; // The actual answer like "dog", "cooked"
    public int coinReward;
    public bool isCompleted; // Player answered it correctly
    public bool isClaimed; // Player claimed the reward
    public int questionIndex; // Which question from easySpellingPairs/easySentencePairs
    public bool isSpellingQuestion; // true = spelling, false = sentence
}

public class DailyTaskManager : MonoBehaviour
{
    public static DailyTaskManager Instance { get; private set; }

    [Header("Task Settings")]
    public int numberOfDailyTasks = 3;
    public int[] taskRewards = new int[] { 50, 60, 70 }; // Rewards for each task

    [Header("Save Keys")]
    private const string LAST_RESET_DATE_KEY = "LastTaskResetDate";
    private const string TASK_DATA_KEY = "DailyTaskData";

    public List<DailyTask> dailyTasks = new List<DailyTask>();

    // Question pools - these match your QuestionRandomizer arrays
    private string[,] easySpellingPairs = new string[,]
    {
        { "a common pet that barks", "dog", "dag" },
        { "something you wear on your head", "hat", "hoat" },
        { "a color between red and white", "pink", "ponk" },
        { "the star in the sky during daytime", "sun", "san" },
        { "body part used for walking", "leg", "log" },
        { "food from animals, often eaten cooked", "meat", "met" },
        { "a small container for drinking", "cup", "cawp" },
        { "you listen with this", "pair", "pear" },
        { "a plant with leaves and branches", "tree", "tri" },
        { "opposite of white", "black", "bolck" },
        { "moving quickly", "fast", "fest" },
        { "activity in water", "swim", "swom" },
        { "refers to the person being addressed", "you", "yoo" },
        { "where you sleep at night", "bed", "ved" },
        { "part of your body used to hold things", "hand", "henk" },
        { "a flying animal with feathers", "bird", "burd" },
        { "white drink from cows", "milk", "milx" },
        { "to leap into the air", "jump", "jomp" },
        { "baked food made from flour", "bread", "bredd" },
        { "sweet dessert", "cake", "cak" },
        { "used for walking or running", "foot", "fut" },
        { "female child", "girl", "gurl" },
        { "part of your face used to smell", "nose", "nosh" },
        { "color of the sky on a clear day", "blue", "blu" },
        { "vehicle with wheels", "car", "cor" },
        { "corn", "corn", "curn" },
        { "mother's sister", "aunt", "ant" },
        { "farm animal that gives milk", "cow", "coe" },
        { "feeling of joy", "happy", "hoppy" },
        { "color of an apple", "red", "rad" },
        { "arm", "arm", "arum" },
        { "a companion", "friend", "frend" },
        { "box", "box", "bocs" },
        { "used to carry things", "bag", "bog" },
        { "furniture to sit on", "chair", "chare" },
        { "hop hop", "hop", "hut" },
        { "a male parent", "dad", "did" },
        { "a young child", "kid", "kod" },
        { "female bird", "hen", "han" },
        { "staple food, often eaten with dishes", "rice", "rais" },
        { "slow", "slow", "slew" },
        { "reading material", "book", "buck" },
        { "farm animal that oinks", "pig", "peg" },
        { "baby", "baby", "bebe" },
        { "refers to a male person", "him", "hem" },
        { "bell", "bell", "vel" },
        { "toy you play with", "toy", "toi" },
        { "mother", "mom", "mum" },
        { "feeling of sadness", "sad", "sed" },
        { "leaves from a tree", "leaf", "loaf" },
        { "to form letters on paper", "write", "rite" },
        { "color opposite of black", "white", "whyte" },
        { "cat", "cat", "gat" },
        { "farm animal with horns", "goat", "got" },
        { "desk", "desk", "deks" },
        { "a celestial object at night", "moon", "mun" },
        { "falling water from clouds", "rain", "rein" },
        { "frozen precipitation", "snow", "snou" },
        { "part of the face used for speaking", "lip", "lap" },
        { "sweet spread for bread", "jam", "jem" },
        { "young male child", "boy", "boi" },
        { "relative of parents", "uncle", "unkl" },
        { "good or desirable", "good", "gud" },
        { "opposite of good", "bad", "badd" },
        { "to move on feet at a moderate pace", "walk", "wok" },
        { "to stand upright", "stand", "stend" },
        { "activity of singing", "sing", "sung" },
        { "transport by road", "bus", "bos" },
        { "passage to enter or exit", "door", "dor" },
        { "something that shows location", "map", "mop" },
        { "farm product from birds", "egg", "egh" },
        { "edible fruit", "pear", "per" },
        { "small flying waterbird", "duck", "duc" },
        { "small rodent", "mouse", "mawz" },
        { "air in motion", "wind", "windz" },
    };

    private string[,] easySentencePairs = new string[,]
    {
        { "The chef ____ a delicious meal.", "cooked", "taught" },
        { "The teacher ____ the students about history.", "taught", "slept" },
        { "The cat ____ on the sunny windowsill.", "slept", "flew" },
        { "The pianist ____ a beautiful melody.", "played", "ran" },
        { "The athlete ____ across the finish line.", "ran", "cried" },
        { "The baby ____ when it's hungry.", "cries", "tumbles" },
        { "The photographer ____ pictures of the sunset.", "took", "repaired" },
        { "The mechanic ____ the broken engine.", "repaired", "studied" },
        { "The student ____ for the upcoming exam.", "studied", "painted" },
        { "The artist ____ a portrait on the canvas.", "painted", "wagged" },
        { "The dog ____ its tail happily.", "wagged", "sang" },
        { "The singer ____ in front of the audience.", "sang", "watered" },
        { "The gardener ____ the flowers every morning.", "watered", "wrote" },
        { "The writer ____ a new short story.", "wrote", "examined" },
        { "The doctor ____ the patient carefully.", "examined", "drove" },
        { "The driver ____ the car down the highway.", "drove", "built" },
        { "The carpenter ____ a sturdy table.", "built", "experimented" },
        { "The scientist ____ an experiment in the lab.", "experimented", "served" },
        { "The waiter ____ food to the customers.", "served", "swam" },
        { "The swimmer ____ laps in the pool.", "swam", "cooked" }
    };

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
        TimeSpan timeRemaining = nextMidnight - now;
        return timeRemaining;
    }

    public string GetFormattedTimeUntilReset()
    {
        TimeSpan timeRemaining = GetTimeUntilReset();
        return string.Format("{0:D2}:{1:D2}:{2:D2}",
            timeRemaining.Hours,
            timeRemaining.Minutes,
            timeRemaining.Seconds);
    }

    void GenerateNewDailyTasks()
    {
        dailyTasks.Clear();

        HashSet<int> usedSpellingIndices = new HashSet<int>();
        HashSet<int> usedSentenceIndices = new HashSet<int>();

        for (int i = 0; i < numberOfDailyTasks; i++)
        {
            DailyTask newTask = new DailyTask();
            newTask.taskID = i;
            newTask.coinReward = taskRewards[i];
            newTask.isCompleted = false;
            newTask.isClaimed = false;

            bool isSpelling = UnityEngine.Random.value > 0.5f;
            newTask.isSpellingQuestion = isSpelling;

            if (isSpelling)
            {
                int randomIndex;
                do
                {
                    randomIndex = UnityEngine.Random.Range(0, easySpellingPairs.GetLength(0));
                } while (usedSpellingIndices.Contains(randomIndex));

                usedSpellingIndices.Add(randomIndex);
                newTask.questionIndex = randomIndex;
                newTask.correctAnswer = easySpellingPairs[randomIndex, 1]; // Get correct answer
                newTask.taskDescription = $"Get the word: \"{newTask.correctAnswer}\"";
            }
            else
            {
                int randomIndex;
                do
                {
                    randomIndex = UnityEngine.Random.Range(0, easySentencePairs.GetLength(0));
                } while (usedSentenceIndices.Contains(randomIndex));

                usedSentenceIndices.Add(randomIndex);
                newTask.questionIndex = randomIndex;
                newTask.correctAnswer = easySentencePairs[randomIndex, 1]; // Get correct answer
                newTask.taskDescription = $"Get the word: \"{newTask.correctAnswer}\"";
            }

            dailyTasks.Add(newTask);
        }

        SaveDailyTasks();
        Debug.Log($"✅ Generated {dailyTasks.Count} new daily tasks!");
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
            Debug.Log($"📋 Loaded {dailyTasks.Count} daily tasks");
        }
        else
        {
            Debug.LogWarning("⚠️ No saved tasks found, generating new ones...");
            GenerateNewDailyTasks();
        }
    }

    /// <summary>
    /// Called when player answers a question correctly in gameplay
    /// </summary>
    public bool CheckAndCompleteTask(string correctAnswer)
    {
        foreach (DailyTask task in dailyTasks)
        {
            if (task.correctAnswer == correctAnswer && !task.isCompleted)
            {
                task.isCompleted = true;
                SaveDailyTasks();
                Debug.Log($"🎯 Daily Task completed: \"{correctAnswer}\" - Can now claim {task.coinReward} coins!");
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Claims the reward for a completed task
    /// </summary>
    public bool ClaimTaskReward(int taskID)
    {
        DailyTask task = GetTask(taskID);

        if (task == null)
        {
            Debug.LogWarning($"⚠️ Task {taskID} not found!");
            return false;
        }

        if (!task.isCompleted)
        {
            Debug.LogWarning($"⚠️ Task {taskID} not completed yet!");
            return false;
        }

        if (task.isClaimed)
        {
            Debug.LogWarning($"⚠️ Task {taskID} already claimed!");
            return false;
        }

        // Award coins
        int currentCoins = PlayerPrefs.GetInt("PlayerTotalCoins", 0);
        currentCoins += task.coinReward;
        PlayerPrefs.SetInt("PlayerTotalCoins", currentCoins);
        PlayerPrefs.Save();

        task.isClaimed = true;
        SaveDailyTasks();

        Debug.Log($"💰 Task {taskID} claimed! Awarded {task.coinReward} coins. Total: {currentCoins}");
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
        {
            if (!task.isCompleted)
                return false;
        }
        return true;
    }

    // ✅ Allow other scripts (like WordUnlockManager) to safely access word data
    public string[,] GetEasySpellingPairs()
    {
        return easySpellingPairs;
    }

    public string[,] GetEasySentencePairs()
    {
        return easySentencePairs;
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