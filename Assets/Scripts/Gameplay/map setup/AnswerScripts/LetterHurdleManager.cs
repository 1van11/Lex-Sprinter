using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LetterHurdleManager : MonoBehaviour
{
    public TMP_Text collectedText;
    public TMP_Text targetWordText;
    public TMP_Text feedbackText;
    public TMP_Text scoreText;

    [Header("Boss Reference")]
    public EventTimingManager bossManager;
    
    [Header("Player Reference")]
    public PlayerFunctions playerFunctions; // Assign in Inspector
    
    private string[] wordList = {
        "dog","hat","pink","sun","leg","meat","cup","pair","tree","black",
        "fast","swim","you","bed","hand","bird","milk","jump","bread","cake",
        "foot","girl","nose","blue","car","corn","aunt","cow","happy","red",
        "arm","friend","box","bag","chair","hop","dad","kid","hen","rice",
        "slow","book","pig","baby","him","bell","toy","mom","sad","leaf",
        "write","white","cat","goat","desk","moon","rain","snow","lip","jam",
        "boy","uncle","good","bad","walk","stand","sing","bus","door","map",
        "egg","pear","duck","mouse","wind"
    };

    private string currentTargetWord;
    private List<string> shuffledWords;
    private int currentWordIndex = 0;
    private string previousCollectedText = "";

    void Start()
    {
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
                CheckSpelling(currentCollected);
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

    void CheckSpelling(string collected)
    {
        if (string.IsNullOrEmpty(collected))
            return;

        int collectedLength = collected.Length;

        if (collectedLength <= currentTargetWord.Length)
        {
            char expectedLetter = currentTargetWord[collectedLength - 1];
            char collectedLetter = collected[collectedLength - 1];

            if (collectedLetter == expectedLetter)
            {
                // Correct letter
                if (collectedLength == currentTargetWord.Length)
                {
                    // Word completed!
                    if (feedbackText != null)
                    {
                        feedbackText.text = "Correct!";
                        feedbackText.color = Color.green;
                    }

                    // AWARD 25 POINTS THROUGH PLAYERFUNCTIONS
                    if (playerFunctions != null)
                    {
                        playerFunctions.AddCoins(25);
                    }

                    CheckBossSpell();

                    currentWordIndex++;
                    Invoke("SetNewTargetWord", 2f);
                }
                else
                {
                    if (feedbackText != null)
                        feedbackText.text = "";
                }
            }
            else
            {
                // Wrong letter collected! DAMAGE PLAYER
                if (feedbackText != null)
                {
                    feedbackText.text = "Wrong Letter!";
                    feedbackText.color = Color.red;
                }

                if (playerFunctions != null)
                    playerFunctions.TakeDamageFromWrongLetter();

                // Remove the wrong letter
                collectedText.text = collected.Substring(0, collectedLength - 1);
                previousCollectedText = collectedText.text;

                Invoke("ClearFeedback", 1f);
            }
        }
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
