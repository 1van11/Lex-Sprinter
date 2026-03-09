using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class LetterHurdleManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text collectedText;          // Shows "d _ _" progress
    public TMP_Text targetWordText;
    public Image letterHurdleClueImage;      // Uses difficulty‑based clue images
    public TMP_Text letterHurdleFeedbackText;
    public TMP_Text letterHurdleScoreText;

    [Header("Clue Images (Difficulty Based)")]
    public Sprite[] easyClueImages;
    public Sprite[] mediumClueImages;
    public Sprite[] hardClueImages;
    private Sprite[] currentClueImages;

    [Header("Word Lists (Difficulty Based)")]
    private string[] easyWordList = {
        "dog","hat","pink","sun","leg","meat","cup","pair","tree","black",
        "fast","swim","you","bed","hand","bird","milk","jump","bread","cake",
        "foot","girl","nose","blue","car","corn","aunt","cow","happy","red",
        "arm","friend","box","bag","chair","hop","dad","kid","hen","rice",
        "slow","book","pig","baby","him","bell","toy","mom","sad","leaf",
        "write","white","cat","goat","desk","moon","rain","snow","lip","jam",
        "boy","uncle","good","bad","walk","stand","sing","bus","door","map",
        "egg","pear","duck","mouse","wind"
    };

    private string[] mediumWordList = {
        "Rabbit","Monkey","Tiger","Zebra","Eagle","Panther","Giraffe","Alligator","Octopus","Penguin",
        "Forest","Desert","Basket","Ladder","Bottle","Pillow","Blanket","Lantern","Magnet","Cactus",
        "Banana","Cookie","Cheese","Tomato","Melon","Market","Garden","School","Street","Castle",
        "Statue","Crown","Bridge","Anchor","Volcano","Hurricane","Glacier","Blizzard","Compass",
        "Scissors","Telescope","Microscope","Helmet","Jewelry","Instrument","Armor","Chocolate",
        "Spaghetti","Lighthouse","Windmill"
    };

    private string[] hardWordList = {
        "Aardvark", "Amphitheater", "Armadillo", "Astrolabe", "Axolotl", "Ballista", "Battlement", "Carousel", "Catapult", "Centaur",
        "Chameleon", "Chandelier", "Chrysalis", "Cockatoo", "Colosseum", "Drawbridge", "Gargoyle", "Gladiator", "Guillotine", "Harpoon",
        "Hieroglyph", "Kaleidoscope", "Labyrinth", "Marquee", "Menagerie", "Minotaur", "Monolith", "Narwhal", "Obelisk", "Obsidian",
        "Oubliette", "Parthenon", "Periscope", "Pharaoh", "Platypus", "Portcullis", "Pyramid", "Quokka", "Samurai", "Sarcophagus",
        "Scorpion", "Sextant", "Sphinx", "Spyglass", "Tarantula", "Trebuchet", "Trident", "Viking", "Xylophone", "Ziggurat",
    };
    private string[] currentWordList;

    [Header("References")]
    public PlayerFunctions playerFunctions;
    public ObstacleSpawner obstacleSpawner;
    public EventTimingManager bossManager;   // optional

    [Header("Letter Prefab")]
    public GameObject letterPrefab;
    public Transform letterSpawnParent;
    public float letterSpacing = 1f;

    // Private state
    private List<string> shuffledWords;
    private int currentWordIndex = 0;
    private string currentTargetWord;
    private string previousRawCollected = "";

    private Dictionary<string, Sprite> wordToImageMap = new Dictionary<string, Sprite>();
    private List<GameObject> spawnedLetters = new List<GameObject>();

    void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GAMEMODE")
        {
            currentWordList = easyWordList;
            currentClueImages = easyClueImages;
        }
        else if (sceneName == "GAMEMODE 1")
        {
            currentWordList = mediumWordList;
            currentClueImages = mediumClueImages;
        }
        else if (sceneName == "GAMEMODE 2")
        {
            currentWordList = hardWordList;
            currentClueImages = hardClueImages;
        }
        else
        {
            currentWordList = easyWordList;
            currentClueImages = easyClueImages;
            Debug.LogWarning("Unknown scene name. Defaulting to EASY MODE for letter hurdle.");
        }
    }

    void Start()
    {
        if (letterHurdleClueImage != null)
            letterHurdleClueImage.gameObject.SetActive(false);

        InitializeLetterHurdle();
    }

    void Update()
    {
        if (collectedText == null) return;

        string rawNow = ExtractRawLetters(collectedText.text);
        if (rawNow != previousRawCollected)
        {
            CheckSpellingFast(rawNow);
        }
    }

    // ------------------------------------------------------------------
    // Initialization
    // ------------------------------------------------------------------
    void InitializeLetterHurdle()
    {
        // Convert word list to lower case for consistency
        currentWordList = currentWordList.Select(w => w.ToLower()).ToArray();

        BuildWordToImageMap();

        shuffledWords = currentWordList.OrderBy(x => Random.value).ToList();

        if (letterHurdleFeedbackText != null) letterHurdleFeedbackText.text = "";
        UpdateScoreText();
        SetNewTargetWord();
    }

    void BuildWordToImageMap()
    {
        wordToImageMap.Clear();
        if (currentClueImages == null || currentClueImages.Length == 0)
        {
            Debug.LogWarning("No clue images assigned for current difficulty");
            return;
        }

        int count = Mathf.Min(currentWordList.Length, currentClueImages.Length);
        for (int i = 0; i < count; i++)
        {
            string word = currentWordList[i];
            if (!wordToImageMap.ContainsKey(word) && currentClueImages[i] != null)
                wordToImageMap.Add(word, currentClueImages[i]);
        }
        Debug.Log($"Word-to-image map built: {wordToImageMap.Count} words mapped");
    }

    void SetNewTargetWord()
    {
        foreach (var letter in spawnedLetters) Destroy(letter);
        spawnedLetters.Clear();

        if (currentWordIndex >= shuffledWords.Count)
        {
            shuffledWords = currentWordList.OrderBy(x => Random.value).ToList();
            currentWordIndex = 0;
        }

        currentTargetWord = shuffledWords[currentWordIndex];

        if (targetWordText != null)
            targetWordText.text = "Spell: " + currentTargetWord.ToLower();

        SpawnLetters(currentTargetWord);

        previousRawCollected = "";
        UpdateCollectedDisplay("");

        if (letterHurdleFeedbackText != null) letterHurdleFeedbackText.text = "";

        UpdateClueImage();
    }

    void SpawnLetters(string word)
    {
        if (letterPrefab == null || letterSpawnParent == null) return;

        for (int i = 0; i < word.Length; i++)
        {
            GameObject letterObj = Instantiate(letterPrefab, letterSpawnParent);
            letterObj.transform.localPosition = new Vector3(i * letterSpacing, 0, 0);
            TMP_Text lt = letterObj.GetComponent<TMP_Text>();
            if (lt != null) lt.text = word[i].ToString().ToUpper();
            spawnedLetters.Add(letterObj);
        }
    }

    void UpdateClueImage()
    {
        if (letterHurdleClueImage == null) return;

        string targetWord = currentTargetWord.ToLower();
        if (wordToImageMap.TryGetValue(targetWord, out Sprite sprite) && sprite != null)
        {
            letterHurdleClueImage.sprite = sprite;
            letterHurdleClueImage.gameObject.SetActive(true);
        }
        else
        {
            letterHurdleClueImage.gameObject.SetActive(false);
            Debug.LogWarning($"No clue image found for word: {currentTargetWord}");
        }
    }

    // ------------------------------------------------------------------
    // Spelling Check
    // ------------------------------------------------------------------
    private string ExtractRawLetters(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in text)
        {
            if (c != '_' && c != ' ')
                sb.Append(char.ToLower(c));
        }
        return sb.ToString();
    }

    private void UpdateCollectedDisplay(string rawCollected)
    {
        if (collectedText == null || string.IsNullOrEmpty(currentTargetWord)) return;

        string target = currentTargetWord.ToLower();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < target.Length; i++)
        {
            if (i > 0) sb.Append(' ');

            if (i < rawCollected.Length)
                sb.Append(rawCollected[i]);
            else
                sb.Append('_');
        }

        collectedText.text = sb.ToString();
    }

    private void CheckSpellingFast(string collected)
    {
        if (string.IsNullOrEmpty(collected))
        {
            previousRawCollected = "";
            UpdateCollectedDisplay("");
            return;
        }

        string target = currentTargetWord.ToLower();

        if (collected == target)
        {
            UpdateCollectedDisplay(collected);
            previousRawCollected = collected;

            if (letterHurdleFeedbackText != null)
            {
                letterHurdleFeedbackText.text = "Correct!";
                letterHurdleFeedbackText.color = Color.green;
            }

            // Reward coins based on difficulty
            if (playerFunctions != null)
            {
                string scene = SceneManager.GetActiveScene().name;
                int coinReward = scene switch
                {
                    "GAMEMODE 2" => 200,
                    "GAMEMODE 1" => 100,
                    _            => 25
                };
                playerFunctions.AddCoins(coinReward);
            }

            // Notify obstacle spawner about success
            if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
            {
                obstacleSpawner.OnLetterHurdleSuccess();
                if (letterHurdleClueImage != null)
                    letterHurdleClueImage.gameObject.SetActive(false);
            }

            // Optional boss finish
            if (bossManager != null) bossManager.FinishBoss();

            // Move to next word
            currentWordIndex++;
            SetNewTargetWord();
            return;
        }

        int minLen = Mathf.Min(collected.Length, target.Length);
        for (int i = 0; i < minLen; i++)
        {
            if (collected[i] != target[i])
            {
                if (letterHurdleFeedbackText != null)
                {
                    letterHurdleFeedbackText.text = "Wrong Letter!";
                    letterHurdleFeedbackText.color = Color.red;
                }

                if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
                    obstacleSpawner.OnLetterHurdleFailed();

                if (playerFunctions != null)
                    playerFunctions.TakeDamageFromWrongLetter();

                string trimmed = collected.Substring(0, i);
                previousRawCollected = trimmed;
                UpdateCollectedDisplay(trimmed);

                if (letterHurdleFeedbackText != null)
                    Invoke(nameof(ClearFeedback), 1f);

                return;
            }
        }

        // All characters matched so far – continue
        previousRawCollected = collected;
        UpdateCollectedDisplay(collected);

        if (letterHurdleFeedbackText != null) letterHurdleFeedbackText.text = "";
    }

    private void ClearFeedback()
    {
        if (letterHurdleFeedbackText != null) letterHurdleFeedbackText.text = "";
    }

    // ------------------------------------------------------------------
    // Public Methods (called by other scripts, e.g. LetterCollectible)
    // ------------------------------------------------------------------
    public string GetCurrentWord() => currentTargetWord;

    public void ClearCollectedLetters()
    {
        previousRawCollected = "";
        UpdateCollectedDisplay("");
        if (letterHurdleFeedbackText != null) letterHurdleFeedbackText.text = "";
    }

    public void SkipWord()
    {
        currentWordIndex++;
        SetNewTargetWord();
    }

    public void CheckBossSpell()
    {
        if (collectedText == null) return;

        string typed = ExtractRawLetters(collectedText.text);
        if (typed == currentTargetWord.ToLower())
        {
            if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
            {
                obstacleSpawner.OnLetterHurdleSuccess();
                if (letterHurdleClueImage != null)
                    letterHurdleClueImage.gameObject.SetActive(false);
            }
        }
    }

    public void ShowClueImage(bool show)
    {
        if (letterHurdleClueImage != null)
            letterHurdleClueImage.gameObject.SetActive(show);
    }

    public void RefreshClueImage() => UpdateClueImage();

    private void UpdateScoreText()
    {
        if (letterHurdleScoreText != null && playerFunctions != null)
            letterHurdleScoreText.text = "Score: " + playerFunctions.score;
    }
}