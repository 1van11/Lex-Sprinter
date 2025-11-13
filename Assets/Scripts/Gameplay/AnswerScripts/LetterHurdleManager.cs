using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class LetterHurdleManager : MonoBehaviour
{
    public TMP_Text collectedText;
    public TMP_Text targetWordText;
    public TMP_Text feedbackText;
    public TMP_Text scoreText;

    [Header("Boss Reference")]
    public EventTimingManager bossManager;

    [Header("Player Reference")]
    public PlayerFunctions playerFunctions;

    [Header("Obstacle Spawner Reference")]
    public ObstacleSpawner obstacleSpawner;

    [Header("Letter Prefab & Spawn")]
    public GameObject letterPrefab;
    public Transform spawnParent;
    public float letterSpacing = 1f;

    [Header("Word Lists")]
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

    private string[] wordList;
    private string currentTargetWord;
    private List<string> shuffledWords;
    private int currentWordIndex = 0;
    private string previousCollectedText = "";

    private List<GameObject> spawnedLetters = new List<GameObject>();

    void Start()
    {
        string scene = SceneManager.GetActiveScene().name;
        wordList = (scene == "MediumMode") ? mediumWordList : easyWordList;
        shuffledWords = wordList.OrderBy(x => Random.value).ToList();

        SetNewTargetWord();

        if (feedbackText != null)
            feedbackText.text = "";

        UpdateScoreText();

        if (playerFunctions == null)
            playerFunctions = FindObjectOfType<PlayerFunctions>();

        // Auto-find ObstacleSpawner if not assigned
        if (obstacleSpawner == null)
            obstacleSpawner = FindObjectOfType<ObstacleSpawner>();
    }

    void Update()
    {
        if (collectedText != null)
        {
            string currentCollected = collectedText.text.ToLower().Trim();
            if (currentCollected != previousCollectedText)
            {
                CheckSpellingFast(currentCollected);
                previousCollectedText = currentCollected;
            }
        }
    }

    void SetNewTargetWord()
    {
        foreach (var letter in spawnedLetters)
            Destroy(letter);
        spawnedLetters.Clear();

        if (currentWordIndex >= shuffledWords.Count)
        {
            shuffledWords = wordList.OrderBy(x => Random.value).ToList();
            currentWordIndex = 0;
        }

        currentTargetWord = shuffledWords[currentWordIndex];

        if (targetWordText != null)
            targetWordText.text = "Spell: " + currentTargetWord.ToLower();

        SpawnLetters(currentTargetWord);

        if (collectedText != null)
            collectedText.text = "";

        previousCollectedText = "";

        if (feedbackText != null)
            feedbackText.text = "";
    }

    void SpawnLetters(string word)
    {
        if (letterPrefab == null || spawnParent == null) return;

        for (int i = 0; i < word.Length; i++)
        {
            GameObject letterObj = Instantiate(letterPrefab, spawnParent);
            letterObj.transform.localPosition = new Vector3(i * letterSpacing, 0, 0);
            TMP_Text letterText = letterObj.GetComponent<TMP_Text>();
            if (letterText != null)
                letterText.text = word[i].ToString();

            spawnedLetters.Add(letterObj);
        }
    }

    void CheckSpellingFast(string collected)
    {
        if (string.IsNullOrEmpty(collected)) return;

        string target = currentTargetWord.ToLower();

        if (collected == target)
        {
            if (feedbackText != null)
            {
                feedbackText.text = "Correct!";
                feedbackText.color = Color.green;
            }

            if (playerFunctions != null)
            {
                string scene = SceneManager.GetActiveScene().name;
                playerFunctions.AddCoins((scene == "MediumMode") ? 100 : 25);
            }

            // NEW: Notify ObstacleSpawner that word was completed
            if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
            {
                obstacleSpawner.OnLetterHurdleSuccess();
                Debug.Log("✅ Word completed! Notified ObstacleSpawner.");
            }

            // Call boss manager if it exists
            if (bossManager != null)
                bossManager.FinishBoss();

            currentWordIndex++;
            SetNewTargetWord();
            return;
        }

        int minLength = Mathf.Min(collected.Length, target.Length);
        for (int i = 0; i < minLength; i++)
        {
            if (collected[i] != target[i])
            {
                if (feedbackText != null)
                {
                    feedbackText.text = "Wrong Letter!";
                    feedbackText.color = Color.red;
                }

                // NEW: Notify ObstacleSpawner of failure
                if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
                {
                    obstacleSpawner.OnLetterHurdleFailed();
                }

                if (playerFunctions != null)
                    playerFunctions.TakeDamageFromWrongLetter();

                collectedText.text = collected.Substring(0, i);
                previousCollectedText = collectedText.text;

                if (feedbackText != null)
                    Invoke("ClearFeedback", 1f);

                return;
            }
        }

        if (feedbackText != null)
            feedbackText.text = "";
    }

    void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = "";
    }

    void UpdateScoreText()
    {
        if (scoreText != null && playerFunctions != null)
            scoreText.text = "Score: " + playerFunctions.score;
    }

    public void ClearCollectedLetters()
    {
        if (collectedText != null)
            collectedText.text = "";

        previousCollectedText = "";

        if (feedbackText != null)
            feedbackText.text = "";
    }

    public void SkipWord()
    {
        currentWordIndex++;
        SetNewTargetWord();
    }

    public string GetCurrentWord()
    {
        return currentTargetWord;
    }

    // NEW: Modified CheckBossSpell to notify ObstacleSpawner
    public void CheckBossSpell()
    {
        if (collectedText == null) return;

        string typed = collectedText.text.ToLower().Trim();
        string target = currentTargetWord.ToLower().Trim();

        if (typed == target)
        {
            // Notify ObstacleSpawner first
            if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
            {
                obstacleSpawner.OnLetterHurdleSuccess();
                Debug.Log("✅ CheckBossSpell: Word completed! Ending letter event.");
            }

            // Then call boss manager if it exists
           // if (bossManager != null)
            //    bossManager.FinishBoss();
        }
    }
}