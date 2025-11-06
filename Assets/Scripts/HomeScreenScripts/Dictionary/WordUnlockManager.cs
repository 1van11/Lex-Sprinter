using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WordUnlockManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField searchInput;        // Your search bar input field
    public Button[] wordButtons;              // All your word buttons (SlotsBTN)
    public Sprite lockedSprite;               // Locked state sprite
    public Sprite unlockedSprite;             // Unlocked state sprite
    public Sprite clickedSprite;              // Clicked state sprite

    [Header("Word Data")]
    public List<string> allWords = new List<string>();   // Words available in your dictionary

    private HashSet<string> unlockedWords = new HashSet<string>();
    private HashSet<string> clickedWords = new HashSet<string>();

    void Awake()
    {
        // ✅ Auto-fill all words from DailyTaskManager
        DailyTaskManager dailyTaskManager = FindObjectOfType<DailyTaskManager>();
        if (dailyTaskManager != null)
        {
            // Get words from spelling pairs
            string[,] spellingPairs = dailyTaskManager.GetEasySpellingPairs();
            for (int i = 0; i < spellingPairs.GetLength(0); i++)
            {
                string word = spellingPairs[i, 1];
                if (!allWords.Contains(word))
                    allWords.Add(word);
            }

            // Get words from sentence pairs
            string[,] sentencePairs = dailyTaskManager.GetEasySentencePairs();
            for (int i = 0; i < sentencePairs.GetLength(0); i++)
            {
                string word = sentencePairs[i, 1];
                if (!allWords.Contains(word))
                    allWords.Add(word);
            }

            Debug.Log($"📚 Loaded {allWords.Count} words from DailyTaskManager");
        }
        else
        {
            Debug.LogWarning("⚠️ DailyTaskManager not found in scene!");
        }
    }

    void Start()
    {
        if (searchInput != null)
        {
            searchInput.characterLimit = 10;
            searchInput.onValueChanged.AddListener(OnSearchValueChanged);
        }

        for (int i = 0; i < wordButtons.Length; i++)
        {
            int index = i;
            wordButtons[i].onClick.AddListener(() => OnWordButtonClick(index));
            SetButtonState(wordButtons[i], lockedSprite);
        }

        LoadUnlockedWords();

        string lastUnlockedWord = PlayerPrefs.GetString("LastUnlockedWord", "");
        if (!string.IsNullOrEmpty(lastUnlockedWord))
        {
            UnlockWord(lastUnlockedWord);
            PlayerPrefs.DeleteKey("LastUnlockedWord");
            PlayerPrefs.Save();
        }
    }

    void OnEnable()
    {
        AutoUnlockDailyTaskWords();
    }

    void AutoUnlockDailyTaskWords()
    {
        if (DailyTaskManager.Instance == null)
        {
            Debug.LogWarning("⚠️ DailyTaskManager not found. Make sure it's in the scene or marked DontDestroyOnLoad.");
            return;
        }

        List<DailyTask> tasks = DailyTaskManager.Instance.GetAllTasks();

        foreach (DailyTask task in tasks)
        {
            if (task.isCompleted)
            {
                UnlockWord(task.correctAnswer);
                Debug.Log($"🔓 Auto-unlocked word from daily task: {task.correctAnswer}");
            }
        }
    }

    public void UnlockWord(string word)
    {
        if (!unlockedWords.Contains(word))
        {
            unlockedWords.Add(word);
            SaveUnlockedWords();
        }

        UpdateButtonVisual(word);
    }

    void OnWordButtonClick(int index)
    {
        if (index < 0 || index >= allWords.Count)
            return;

        string word = allWords[index];

        if (!unlockedWords.Contains(word))
        {
            Debug.Log($"Word '{word}' is locked!");
            return;
        }

        clickedWords.Add(word);
        SetButtonState(wordButtons[index], clickedSprite);
        Debug.Log($"Clicked on '{word}'");
    }

    void UpdateButtonVisual(string word)
    {
        int index = allWords.IndexOf(word);
        if (index == -1 || index >= wordButtons.Length) return;

        if (clickedWords.Contains(word))
            SetButtonState(wordButtons[index], clickedSprite);
        else if (unlockedWords.Contains(word))
            SetButtonState(wordButtons[index], unlockedSprite);
        else
            SetButtonState(wordButtons[index], lockedSprite);
    }

    void SetButtonState(Button button, Sprite sprite)
    {
        if (button == null || sprite == null) return;
        button.image.sprite = sprite;
    }

    void SaveUnlockedWords()
    {
        PlayerPrefs.SetString("UnlockedWords", string.Join(",", unlockedWords));
        PlayerPrefs.Save();
    }

    void LoadUnlockedWords()
    {
        string saved = PlayerPrefs.GetString("UnlockedWords", "");
        if (!string.IsNullOrEmpty(saved))
        {
            unlockedWords = new HashSet<string>(saved.Split(','));
            foreach (string word in unlockedWords)
                UpdateButtonVisual(word);
        }
    }

    public void OnSearchValueChanged(string input)
    {
        // Only allow letters
        string filtered = "";
        foreach (char c in input)
        {
            if (char.IsLetter(c))
                filtered += c;
        }

        if (filtered != input)
            searchInput.text = filtered;

        string searchTerm = filtered.ToLower();

        for (int i = 0; i < allWords.Count; i++)
        {
            bool show = allWords[i].ToLower().Contains(searchTerm);
            wordButtons[i].gameObject.SetActive(show);
        }
    }
}
