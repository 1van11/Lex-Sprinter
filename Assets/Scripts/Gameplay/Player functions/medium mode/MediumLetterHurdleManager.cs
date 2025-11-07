using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class MediumLetterHurdleManager : MonoBehaviour
{
    public TMP_Text collectedText;
    public TMP_Text targetWordText;
    public TMP_Text feedbackText;
    public TMP_Text scoreText;

    [Header("Boss Reference")]
    public EventTimingManager bossManager;

    [Header("Player Reference")]
    public PlayerFunctions playerFunctions;

    [Header("Letter Prefab & Spawn")]
    public GameObject letterPrefab;
    public Transform spawnParent;
    public float letterSpacing = 1f;

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
        wordList = mediumWordList;
        shuffledWords = wordList.OrderBy(x => Random.value).ToList();

        SetNewTargetWord();

        if (feedbackText != null)
            feedbackText.text = "";

        UpdateScoreText();

        if (playerFunctions == null)
            playerFunctions = FindObjectOfType<PlayerFunctions>();
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
                playerFunctions.AddCoins(100);

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

    public void CheckBossSpell()
    {
        if (collectedText == null || bossManager == null) return;

        string typed = collectedText.text.ToLower().Trim();
        string target = currentTargetWord.ToLower().Trim();

        if (typed == target)
        {
            bossManager.FinishBoss();
        }
    }
}
