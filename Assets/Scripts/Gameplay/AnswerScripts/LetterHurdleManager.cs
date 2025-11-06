using UnityEngine;
using TMPro;
using System.Collections;
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

    // EASY WORDS
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

    // MEDIUM WORDS
    private string[] mediumWordList = {
        "Rabbit","Monkey","Tiger","Zebra","Eagle","Panther","Giraffe","Alligator","Octopus","Penguin",
        "Forest","Desert","Basket","Ladder","Bottle","Pillow","Blanket","Lantern","Magnet","Cactus",
        "Banana","Cookie","Cheese","Tomato","Melon","Market","Garden","School","Street","Castle",
        "Statue","Crown","Bridge","Anchor","Volcano","Hurricane","Glacier","Blizzard","Compass",
        "Scissors","Telescope","Microscope","Helmet","Jewelry","Instrument","Armor","Chocolate",
        "Spaghetti","Lighthouse","Windmill"
    };

    private string[] wordList; // ACTIVE WORD LIST (changes by difficulty)
    private string currentTargetWord;
    private List<string> shuffledWords;
    private int currentWordIndex = 0;
    private string previousCollectedText = "";

    void Start()
    {
        // STRICT SCENE DETECTION
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "MediumMode")
            wordList = mediumWordList;
        else if (scene == "EasyMode")
            wordList = easyWordList;
        else
            wordList = easyWordList; // default fallback

        // Shuffle active list
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
        if (currentWordIndex >= shuffledWords.Count)
        {
            shuffledWords = wordList.OrderBy(x => Random.value).ToList();
            currentWordIndex = 0;
        }

        currentTargetWord = shuffledWords[currentWordIndex];

        if (targetWordText != null)
            targetWordText.text = "Spell: " + currentTargetWord.ToLower();

        if (collectedText != null)
            collectedText.text = "";

        previousCollectedText = "";

        if (feedbackText != null)
            feedbackText.text = "";
    }

   void CheckSpellingFast(string collected)
{
    if (string.IsNullOrEmpty(collected))
        return;

    string target = currentTargetWord.ToLower();

    // Full word correct
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
            if (scene == "MediumMode")
                playerFunctions.AddCoins(100); // 100 points for MediumMode
            else
                playerFunctions.AddCoins(25);  // 25 points for EasyMode
        }

        if (bossManager != null)
            bossManager.FinishBoss();

        currentWordIndex++;
        SetNewTargetWord(); // immediately next word
        return;
    }

    // Check for wrong letters
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

            collectedText.text = collected.Substring(0, i); // remove wrong letter
            previousCollectedText = collectedText.text;

            if (feedbackText != null)
                Invoke("ClearFeedback", 1f);

            return;
        }
    }

    // Clear feedback if all letters so far are correct
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
