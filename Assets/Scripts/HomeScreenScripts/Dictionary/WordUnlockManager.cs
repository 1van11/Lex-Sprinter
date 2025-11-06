using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WordUnlockManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField searchInput;        // Your search bar
    public Transform wordButtonContainer;     // Parent for all word buttons (Content of ScrollView)
    public GameObject wordButtonPrefab;       // Prefab with Button + TMP_Text + Image

    [Header("Sprites")]
    public Sprite lockedSprite;               // Locked state sprite (not clickable)
    public Sprite unlockedSprite;             // Unlocked state sprite (clickable)
    public Sprite clickedSprite;              // Clicked/viewed state sprite

    [Header("Word Data - ALL WORDS IN GAME")]
    // You can manually add all words here, or keep them from DailyTaskManager for reference
    public List<string> allWords = new List<string>();

    private HashSet<string> unlockedWords = new HashSet<string>();
    private HashSet<string> clickedWords = new HashSet<string>();
    private Dictionary<string, Button> wordButtonDict = new Dictionary<string, Button>();

    // ✅ Define all words from both spelling and sentence questions
    private string[] spellingWords = new string[]
    {
        "dog", "hat", "pink", "sun", "leg", "meat", "cup", "pair", "tree", "black",
        "fast", "swim", "you", "bed", "hand", "bird", "milk", "jump", "bread", "cake",
        "foot", "girl", "nose", "blue", "car", "corn", "aunt", "cow", "happy", "red",
        "arm", "friend", "box", "bag", "chair", "hop", "dad", "kid", "hen", "rice",
        "slow", "book", "pig", "baby", "him", "bell", "toy", "mom", "sad", "leaf",
        "write", "white", "cat", "goat", "desk", "moon", "rain", "snow", "lip", "jam",
        "boy", "uncle", "good", "bad", "walk", "stand", "sing", "bus", "door", "map",
        "egg", "pear", "duck", "mouse", "wind"
    };

    private string[] sentenceWords = new string[]
    {
        "cooked", "taught", "slept", "played", "ran", "cries", "took", "repaired",
        "studied", "painted", "wagged", "sang", "watered", "wrote", "examined",
        "drove", "built", "experimented", "served", "swam"
    };

    void Awake()
    {
        Debug.Log("🔍 WordUnlockManager Awake() called");

        // Combine all words from both arrays
        allWords.Clear();
        allWords.AddRange(spellingWords);
        allWords.AddRange(sentenceWords);

        // Remove duplicates and sort
        allWords = new List<string>(new HashSet<string>(allWords));
        allWords.Sort();

        Debug.Log($"📚 Total words in dictionary: {allWords.Count}");
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
    /// </summary>
    void CheckForNewUnlockedWord()
    {
        // Check the key that PlayerFunctions saves when player answers correctly
        string lastUnlockedWord = PlayerPrefs.GetString("LastUnlockedWord", "");

        if (!string.IsNullOrEmpty(lastUnlockedWord))
        {
            UnlockWord(lastUnlockedWord);
            PlayerPrefs.DeleteKey("LastUnlockedWord");
            PlayerPrefs.Save();
            Debug.Log($"🆕 New word unlocked from gameplay: {lastUnlockedWord}");
        }
    }

    /// <summary>
    /// Generates buttons for ALL words (locked and unlocked)
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

        // Create a button for EVERY word
        int successCount = 0;
        foreach (string word in allWords)
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

            // Always show the word text
            if (btnText != null)
            {
                btnText.text = word.ToUpper();
            }

            // Set sprite and interactability based on unlock status
            bool isUnlocked = unlockedWords.Contains(word);
            bool isClicked = clickedWords.Contains(word);

            if (isClicked)
            {
                btnImage.sprite = clickedSprite;
                btn.interactable = true; // Clickable
            }
            else if (isUnlocked)
            {
                btnImage.sprite = unlockedSprite;
                btn.interactable = true; // Clickable
            }
            else
            {
                btnImage.sprite = lockedSprite;
                btn.interactable = false; // NOT clickable when locked
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
        }

        UpdateButtonVisual(word);
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
            btn.interactable = false; // Locked = not clickable
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

        // Filter buttons
        int visibleCount = 0;
        foreach (var kvp in wordButtonDict)
        {
            string word = kvp.Key;
            GameObject buttonObj = kvp.Value.gameObject;

            bool show = string.IsNullOrEmpty(searchTerm) || word.ToLower().Contains(searchTerm);
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