using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WordUnlockManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField searchInput;
    public Transform wordButtonContainer;
    public GameObject wordButtonPrefab;

    [Header("Sprites")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    public Sprite clickedSprite;

    [Header("Word Data - ALL WORDS IN GAME")]
    public List<string> allWords = new List<string>();

    private HashSet<string> unlockedWords = new HashSet<string>();
    private HashSet<string> clickedWords = new HashSet<string>();
    private Dictionary<string, Button> wordButtonDict = new Dictionary<string, Button>();

    void Awake()
    {
        Debug.Log("🔍 WordUnlockManager Awake() called");

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        allWords.Clear();

        // Pull words directly from QuestionRandomizer
        if (sceneName == "HomeScreen" || sceneName == "MainMenu")
        {
            // If we're in HomeScreen, load from both Easy and Medium
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.easySpellingPairs));
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.easySentencePairs));
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.mediumSpellingPairs));
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.mediumSentencePairs));
            Debug.Log("📚 Loaded ALL words (Easy + Medium) for Dictionary");
        }
        else if (sceneName == "EasyMode")
        {
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.easySpellingPairs));
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.easySentencePairs));
            Debug.Log("📚 Loaded EASY MODE words from QuestionRandomizer");
        }
        else if (sceneName == "MediumMode")
        {
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.mediumSpellingPairs));
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.mediumSentencePairs));
            Debug.Log("📚 Loaded MEDIUM MODE words from QuestionRandomizer");
        }
        else
        {
            // Default to Easy
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.easySpellingPairs));
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.easySentencePairs));
            Debug.LogWarning("Unknown scene. Defaulting to EASY MODE words.");
        }

        // Remove duplicates and sort alphabetically
        allWords = new List<string>(new HashSet<string>(allWords));
        allWords.Sort();

        Debug.Log($"📚 Total words in dictionary: {allWords.Count}");
    }

    /// <summary>
    /// Helper method to extract correct answers from question pairs
    /// </summary>
    private List<string> GetWordsFromPairs(string[,] pairs)
    {
        List<string> words = new List<string>();
        for (int i = 0; i < pairs.GetLength(0); i++)
        {
            words.Add(pairs[i, 1].ToLower()); // Index 1 is the correct answer
        }
        return words;
    }

    void Start()
    {
        Debug.Log("🎬 WordUnlockManager Start() called");

        // Setup search input
        if (searchInput != null)
        {
            searchInput.characterLimit = 10;
            searchInput.onValueChanged.AddListener(OnSearchValueChanged);
            Debug.Log("✅ Search input configured");
        }

        // Load unlocked words from PlayerPrefs
        LoadUnlockedWords();

        // Check for newly unlocked word from gameplay
        CheckForNewUnlockedWord();

        // Generate ALL word buttons (locked and unlocked)
        GenerateWordButtons();

        Debug.Log($"🎮 Dictionary initialized: {allWords.Count} total words, {unlockedWords.Count} unlocked");
    }

    void OnEnable()
    {
        Debug.Log("👁️ WordUnlockManager OnEnable() called");

        // Check for new unlocked words when panel opens
        CheckForNewUnlockedWord();

        // Refresh all button visuals
        if (wordButtonDict.Count > 0)
        {
            foreach (string word in allWords)
            {
                UpdateButtonVisual(word);
            }
            Debug.Log("🔄 Refreshed button visuals");
        }
    }

    /// <summary>
    /// Checks PlayerPrefs for newly unlocked words from gameplay
    /// Now supports MULTIPLE words separated by commas
    /// </summary>
    void CheckForNewUnlockedWord()
    {
        // Check for multiple newly unlocked words
        string newlyUnlockedWords = PlayerPrefs.GetString("NewlyUnlockedWords", "");

        if (!string.IsNullOrEmpty(newlyUnlockedWords))
        {
            // Split by comma to get all words
            string[] words = newlyUnlockedWords.Split(',');

            int unlockedCount = 0;
            foreach (string word in words)
            {
                string trimmedWord = word.Trim();
                if (!string.IsNullOrEmpty(trimmedWord))
                {
                    UnlockWord(trimmedWord);
                    unlockedCount++;
                    Debug.Log($"🆕 Unlocked word from gameplay: {trimmedWord}");
                }
            }

            // Clear the list after processing
            PlayerPrefs.DeleteKey("NewlyUnlockedWords");
            PlayerPrefs.Save();

            Debug.Log($"✅ Total words unlocked this session: {unlockedCount}");
        }
    }

    /// <summary>
    /// Generates buttons for ALL words (locked and unlocked)
    /// UNLOCKED/CLICKED words appear FIRST (top), then LOCKED words (bottom)
    /// </summary>
    void GenerateWordButtons()
    {
        Debug.Log("🏗️ GenerateWordButtons() called");

        if (wordButtonContainer == null)
        {
            Debug.LogError("❌ Word Button Container is not assigned!");
            return;
        }

        if (wordButtonPrefab == null)
        {
            Debug.LogError("❌ Word Button Prefab is not assigned!");
            return;
        }

        // Clear existing buttons
        int childCount = wordButtonContainer.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(wordButtonContainer.GetChild(i).gameObject);
        }
        wordButtonDict.Clear();

        Debug.Log($"🧹 Cleared {childCount} existing buttons");

        // ✅ SORT: Unlocked/Clicked words FIRST, then Locked words
        List<string> sortedWords = new List<string>();

        // Add clicked words first (top priority)
        foreach (string word in allWords)
        {
            if (clickedWords.Contains(word))
                sortedWords.Add(word);
        }

        // Add unlocked but not clicked words
        foreach (string word in allWords)
        {
            if (unlockedWords.Contains(word) && !clickedWords.Contains(word))
                sortedWords.Add(word);
        }

        // Add locked words at the end
        foreach (string word in allWords)
        {
            if (!unlockedWords.Contains(word))
                sortedWords.Add(word);
        }

        // Create buttons in sorted order
        int successCount = 0;
        foreach (string word in sortedWords)
        {
            GameObject buttonObj = Instantiate(wordButtonPrefab, wordButtonContainer);
            Button btn = buttonObj.GetComponent<Button>();
            TMP_Text btnText = buttonObj.GetComponentInChildren<TMP_Text>();
            Image btnImage = buttonObj.GetComponent<Image>();

            if (btn == null || btnImage == null)
            {
                Debug.LogError($"❌ Prefab missing Button or Image component!");
                Destroy(buttonObj);
                continue;
            }

            // Always show the word text (lowercase)
            if (btnText != null)
            {
                btnText.text = word.ToLower();
            }

            // Set sprite and interactability based on unlock status
            bool isUnlocked = unlockedWords.Contains(word);
            bool isClicked = clickedWords.Contains(word);

            if (isClicked)
            {
                if (clickedSprite != null)
                    btnImage.sprite = clickedSprite;
                btn.interactable = true;
                Debug.Log($"📖 Button created: {word} (CLICKED)");
            }
            else if (isUnlocked)
            {
                if (unlockedSprite != null)
                    btnImage.sprite = unlockedSprite;
                btn.interactable = true;
                Debug.Log($"🔓 Button created: {word} (UNLOCKED)");
            }
            else
            {
                if (lockedSprite != null)
                    btnImage.sprite = lockedSprite;
                btn.interactable = false;
                Debug.Log($"🔒 Button created: {word} (LOCKED)");
            }

            // Add click listener (only works if unlocked)
            string wordCopy = word;
            btn.onClick.AddListener(() => OnWordButtonClick(wordCopy));

            wordButtonDict[word] = btn;
            successCount++;
        }

        Debug.Log($"✅ Generated {successCount}/{allWords.Count} word buttons");

        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(wordButtonContainer as RectTransform);
    }

    /// <summary>
    /// Unlocks a word (called from gameplay or manually)
    /// </summary>
    public void UnlockWord(string word)
    {
        // Normalize the word (lowercase)
        word = word.ToLower();

        if (!allWords.Contains(word))
        {
            Debug.LogWarning($"⚠️ Word '{word}' is not in the dictionary!");
            return;
        }

        if (!unlockedWords.Contains(word))
        {
            unlockedWords.Add(word);
            SaveUnlockedWords();
            Debug.Log($"🔓 Unlocked word: {word}");

            // ✅ Regenerate buttons to re-sort (unlocked words move to top)
            GenerateWordButtons();
        }
        else
        {
            UpdateButtonVisual(word);
        }
    }

    /// <summary>
    /// Called when player clicks an unlocked word button
    /// </summary>
    void OnWordButtonClick(string word)
    {
        if (!unlockedWords.Contains(word))
        {
            Debug.Log($"🔒 Word '{word}' is locked!");
            return;
        }

        // Mark as clicked/viewed
        if (!clickedWords.Contains(word))
        {
            clickedWords.Add(word);
            SaveClickedWords();
            Debug.Log($"📖 Viewed word: {word}");
        }

        UpdateButtonVisual(word);
    }

    /// <summary>
    /// Updates a single button's visual state
    /// </summary>
    void UpdateButtonVisual(string word)
    {
        if (!wordButtonDict.ContainsKey(word)) return;

        Button btn = wordButtonDict[word];
        Image btnImage = btn.GetComponent<Image>();

        bool isUnlocked = unlockedWords.Contains(word);
        bool isClicked = clickedWords.Contains(word);

        // Update sprite and clickability
        if (isClicked)
        {
            btnImage.sprite = clickedSprite;
            btn.interactable = true;
        }
        else if (isUnlocked)
        {
            btnImage.sprite = unlockedSprite;
            btn.interactable = true;
        }
        else
        {
            btnImage.sprite = lockedSprite;
            btn.interactable = false;
        }
    }

    void SaveUnlockedWords()
    {
        PlayerPrefs.SetString("UnlockedWords", string.Join(",", unlockedWords));
        PlayerPrefs.Save();
    }

    void SaveClickedWords()
    {
        PlayerPrefs.SetString("ClickedWords", string.Join(",", clickedWords));
        PlayerPrefs.Save();
    }

    void LoadUnlockedWords()
    {
        // Load unlocked words
        string savedUnlocked = PlayerPrefs.GetString("UnlockedWords", "");
        if (!string.IsNullOrEmpty(savedUnlocked))
        {
            unlockedWords = new HashSet<string>(savedUnlocked.Split(','));
            Debug.Log($"📚 Loaded {unlockedWords.Count} unlocked words");
        }

        // Load clicked words
        string savedClicked = PlayerPrefs.GetString("ClickedWords", "");
        if (!string.IsNullOrEmpty(savedClicked))
        {
            clickedWords = new HashSet<string>(savedClicked.Split(','));
            Debug.Log($"👆 Loaded {clickedWords.Count} clicked words");
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

        // Filter buttons - ONLY show unlocked/clicked words in search
        int visibleCount = 0;
        foreach (var kvp in wordButtonDict)
        {
            string word = kvp.Key;
            GameObject buttonObj = kvp.Value.gameObject;

            bool isUnlocked = unlockedWords.Contains(word);
            bool isClicked = clickedWords.Contains(word);

            // ✅ Only show if: (unlocked OR clicked) AND matches search
            bool matchesSearch = string.IsNullOrEmpty(searchTerm) || word.ToLower().Contains(searchTerm);
            bool isAccessible = isUnlocked || isClicked;

            bool show = matchesSearch && (isAccessible || string.IsNullOrEmpty(searchTerm));

            // If searching, hide locked words completely
            if (!string.IsNullOrEmpty(searchTerm) && !isAccessible)
            {
                show = false;
            }

            buttonObj.SetActive(show);

            if (show) visibleCount++;
        }

        Debug.Log($"🔍 Search: '{searchTerm}' - Showing {visibleCount}/{wordButtonDict.Count} buttons");
    }

    [ContextMenu("Debug: Unlock All Words")]
    void DebugUnlockAll()
    {
        foreach (string word in allWords)
        {
            unlockedWords.Add(word);
        }
        SaveUnlockedWords();

        foreach (string word in allWords)
        {
            UpdateButtonVisual(word);
        }

        Debug.Log("🔓 All words unlocked for testing!");
    }

    [ContextMenu("Debug: Reset All Words")]
    void DebugResetAll()
    {
        unlockedWords.Clear();
        clickedWords.Clear();
        PlayerPrefs.DeleteKey("UnlockedWords");
        PlayerPrefs.DeleteKey("ClickedWords");
        PlayerPrefs.Save();

        if (wordButtonDict.Count > 0)
        {
            foreach (string word in allWords)
            {
                UpdateButtonVisual(word);
            }
        }

        Debug.Log("🔒 All words reset to locked state!");
    }
}