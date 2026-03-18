using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// WordUnlockManager  (updated)
/// ─────────────────────────────────────────────────────────────────────────────
/// Change from original:
///   • Every word button — locked, unlocked, OR clicked — now opens the
///     Dictionary Panel via DictionaryWordViewer.ShowWord(word).
///   • Locked buttons remain visually locked (grey sprite, non-interactive
///     for gameplay) but are still clickable for dictionary look-up.
///   • Assign dictionaryViewer in the Inspector.
/// </summary>
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

    [Header("Dictionary Viewer")]
    [Tooltip("Drag the object that holds DictionaryWordViewer here.")]
    public DictionaryWordViewer dictionaryViewer;

    [Header("Word Data - ALL WORDS IN GAME")]
    public List<string> allWords = new List<string>();

    private HashSet<string> unlockedWords = new HashSet<string>();
    private HashSet<string> clickedWords = new HashSet<string>();
    private Dictionary<string, Button> wordButtonDict = new Dictionary<string, Button>();

    bool isFiltering = false;



    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        Debug.Log("🔍 WordUnlockManager Awake() called");

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        allWords.Clear();

        if (sceneName == "HomeScreen" || sceneName == "MainMenu")
        {
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.easySpellingPairs));
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.mediumSpellingPairs));
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.hardSpellingPairs));

        }
        else if (sceneName == "EasyMode" || sceneName == "GAMEMODE")
        {
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.easySpellingPairs));
            
        }
        else if (sceneName == "MediumMode" || sceneName == "GAMEMODE 1")
        {
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.mediumSpellingPairs));
            
        }
        else if (sceneName == "GAMEMODE 2")
        {
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.hardSpellingPairs));
            
        }
        else
        {
            allWords.AddRange(GetWordsFromPairs(QuestionRandomizer.easySpellingPairs));
            Debug.LogWarning("Unknown scene. Defaulting to EASY MODE words.");
        }

        // De-duplicate and sort alphabetically
        allWords = new List<string>(new HashSet<string>(allWords));
        allWords.Sort();

        Debug.Log($"📚 Total words in dictionary: {allWords.Count}");
    }

    private List<string> GetWordsFromPairs(string[,] pairs)
    {
        var words = new List<string>();
        for (int i = 0; i < pairs.GetLength(0); i++)
            words.Add(pairs[i, 1].ToLower());
        return words;
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        if (searchInput != null)
        {
            searchInput.characterLimit = 10;
            searchInput.onValueChanged.RemoveAllListeners();
            searchInput.onValueChanged.AddListener(OnSearchValueChanged);
            
        }

        LoadUnlockedWords();
        CheckForNewUnlockedWord();
        GenerateWordButtons();
    }

    void OnEnable()
    {
        CheckForNewUnlockedWord();

        if (wordButtonDict.Count > 0)
        {
            foreach (string word in allWords)
                UpdateButtonVisual(word);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CheckForNewUnlockedWord()
    {
        string newlyUnlockedWords = PlayerPrefs.GetString("NewlyUnlockedWords", "");
        if (string.IsNullOrEmpty(newlyUnlockedWords)) return;

        foreach (string word in newlyUnlockedWords.Split(','))
        {
            string w = word.Trim();
            if (!string.IsNullOrEmpty(w))
            {
                UnlockWord(w);
                Debug.Log($"🆕 Unlocked word from gameplay: {w}");
            }
        }

        PlayerPrefs.DeleteKey("NewlyUnlockedWords");
        PlayerPrefs.Save();
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Generates all word buttons.
    /// Order: clicked → unlocked → locked.
    /// ALL buttons are now interactable so the dictionary panel can open.
    /// </summary>
    void GenerateWordButtons()
    {
        if (wordButtonContainer == null || wordButtonPrefab == null)
        {
            Debug.LogError("❌ WordButtonContainer or WordButtonPrefab not assigned!");
            return;
        }

        // Clear existing
        for (int i = wordButtonContainer.childCount - 1; i >= 0; i--)
            Destroy(wordButtonContainer.GetChild(i).gameObject);
        wordButtonDict.Clear();

        // Sort: clicked first, then unlocked, then locked
        var sorted = new List<string>();
        foreach (string w in allWords) if (clickedWords.Contains(w)) sorted.Add(w);
        foreach (string w in allWords) if (unlockedWords.Contains(w) && !clickedWords.Contains(w)) sorted.Add(w);
        foreach (string w in allWords) if (!unlockedWords.Contains(w)) sorted.Add(w);

        foreach (string word in sorted)
        {
            GameObject btnObj = Instantiate(wordButtonPrefab, wordButtonContainer);
            Button btn = btnObj.GetComponent<Button>();
            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();
            Image img = btnObj.GetComponent<Image>();

            if (btn == null || img == null)
            {
                Destroy(btnObj);
                continue;
            }

            if (label != null)
                label.text = word.ToLower();

            bool isUnlocked = unlockedWords.Contains(word);
            bool isClicked = clickedWords.Contains(word);

            // Set sprite
            if (isClicked && clickedSprite != null)
                img.sprite = clickedSprite;
            else if (isUnlocked && unlockedSprite != null)
                img.sprite = unlockedSprite;
            else if (lockedSprite != null)
                img.sprite = lockedSprite;

            // ── KEY CHANGE: ALL buttons are interactable for dictionary lookup ──
            btn.interactable = true;

            // Click → open dictionary panel regardless of locked/unlocked state
            string wordCopy = word;
            btn.onClick.AddListener(() => OnWordButtonClick(wordCopy));

            wordButtonDict[word] = btn;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(wordButtonContainer as RectTransform);
        Debug.Log($"✅ Generated {wordButtonDict.Count} word buttons");
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Called when any word button is tapped.
    /// Opens the dictionary panel for ALL words (locked or not).
    /// Only marks the word as "clicked/viewed" if it is already unlocked.
    /// </summary>
    void OnWordButtonClick(string word)
    {
        // Block dictionary access if word is locked
        if (!unlockedWords.Contains(word))
        {
            Debug.Log($"🔒 Word '{word}' is locked. Unlock it by playing the game first!");
            return;
        }

        // Show the dictionary panel only for unlocked words
        if (dictionaryViewer != null)
            dictionaryViewer.ShowWord(word);
        else
            Debug.LogWarning("[WordUnlockManager] DictionaryWordViewer reference not assigned!");

        // Mark as viewed/clicked
        if (!clickedWords.Contains(word))
        {
            clickedWords.Add(word);
            SaveClickedWords();
            UpdateButtonVisual(word);
        }

        Debug.Log($"📖 Opened dictionary for: {word}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void UnlockWord(string word)
    {
        word = word.ToLower();
        if (!allWords.Contains(word))
        {
            Debug.LogWarning($"⚠️ Word '{word}' not in dictionary!");
            return;
        }

        if (!unlockedWords.Contains(word))
        {
            unlockedWords.Add(word);
            SaveUnlockedWords();
            GenerateWordButtons();   // re-sort so unlocked word floats to top
        }
        else
        {
            UpdateButtonVisual(word);
        }
    }

    void UpdateButtonVisual(string word)
    {
        if (!wordButtonDict.ContainsKey(word)) return;

        Button btn = wordButtonDict[word];
        Image img = btn.GetComponent<Image>();
        bool isUnlocked = unlockedWords.Contains(word);
        bool isClicked = clickedWords.Contains(word);

        if (isClicked && clickedSprite != null)
            img.sprite = clickedSprite;
        else if (isUnlocked && unlockedSprite != null)
            img.sprite = unlockedSprite;
        else if (lockedSprite != null)
            img.sprite = lockedSprite;

        btn.interactable = true;   // always keep interactive
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PERSISTENCE
    // ─────────────────────────────────────────────────────────────────────────
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
        string savedUnlocked = PlayerPrefs.GetString("UnlockedWords", "");
        if (!string.IsNullOrEmpty(savedUnlocked))
            unlockedWords = new HashSet<string>(savedUnlocked.Split(','));

        string savedClicked = PlayerPrefs.GetString("ClickedWords", "");
        if (!string.IsNullOrEmpty(savedClicked))
            clickedWords = new HashSet<string>(savedClicked.Split(','));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SEARCH
    // ─────────────────────────────────────────────────────────────────────────
    public void OnSearchValueChanged(string input)
    {
        if (isFiltering) return;

        isFiltering = true;

        // Remove non-letter characters
        string filtered = "";
        foreach (char c in input)
        {
            if (char.IsLetter(c))
                filtered += c;
        }

        // Update field ONLY if changed
        if (filtered != input)
        {
            searchInput.SetTextWithoutNotify(filtered);
        }

        string searchTerm = filtered.ToLower().Trim();

        foreach (var kvp in wordButtonDict)
        {
            string word = kvp.Key.ToLower();
            bool matches = string.IsNullOrEmpty(searchTerm) || word.StartsWith(searchTerm);

            kvp.Value.gameObject.SetActive(matches);
        }

        isFiltering = false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DEBUG
    // ─────────────────────────────────────────────────────────────────────────
    [ContextMenu("Debug: Unlock All Words")]
    void DebugUnlockAll()
    {
        foreach (string w in allWords) unlockedWords.Add(w);
        SaveUnlockedWords();
        foreach (string w in allWords) UpdateButtonVisual(w);
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
        foreach (string w in allWords) UpdateButtonVisual(w);
        Debug.Log("🔒 All words reset to locked!");
    }
}