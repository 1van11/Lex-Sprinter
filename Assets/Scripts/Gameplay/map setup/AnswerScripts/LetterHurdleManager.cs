using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class LetterHurdleManager : MonoBehaviour
{
    public TMP_Text collectedText;      // Reference to the text showing collected letters
    public TMP_Text targetWordText;     // Shows the word player needs to spell
    public TMP_Text feedbackText;       // Shows "Correct!" or "Wrong Letter!"
    public TMP_Text scoreText;          // Shows current score
    
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
    private int score = 0;
    private List<string> shuffledWords;
    private int currentWordIndex = 0;
    private string previousCollectedText = "";
    
    void Start()
    {
        // Shuffle the word list
        shuffledWords = wordList.OrderBy(x => Random.value).ToList();
        
        // Set first target word
        SetNewTargetWord();
        
        // Initialize UI
        if (feedbackText != null)
            feedbackText.text = "";
        
        UpdateScoreText();
    }
    
    void Update()
    {
        // Check spelling only when new letter is added
        if (collectedText != null)
        {
            string currentCollected = collectedText.text.ToLower().Trim();
            
            // Only check if text changed
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
            // All words completed, reshuffle
            shuffledWords = wordList.OrderBy(x => Random.value).ToList();
            currentWordIndex = 0;
        }
        
        currentTargetWord = shuffledWords[currentWordIndex];
        
        if (targetWordText != null)
            targetWordText.text = "Spell: " + currentTargetWord.ToUpper();
        
        // Clear collected text for new word
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
        
        // Check if each letter matches the target word in sequence
        if (collectedLength <= currentTargetWord.Length)
        {
            // Get the expected letter at this position
            char expectedLetter = currentTargetWord[collectedLength - 1];
            char collectedLetter = collected[collectedLength - 1];
            
            if (collectedLetter == expectedLetter)
            {
                // Correct letter in correct position
                if (collectedLength == currentTargetWord.Length)
                {
                    // Word completed correctly!
                    if (feedbackText != null)
                    {
                        feedbackText.text = "Correct!";
                        feedbackText.color = Color.green;
                    }
                    
                    score += 10;
                    UpdateScoreText();
                    
                    // Move to next word after a short delay
                    currentWordIndex++;
                    Invoke("SetNewTargetWord", 2f);
                }
                else
                {
                    // Correct so far, clear feedback
                    if (feedbackText != null)
                        feedbackText.text = "";
                }
            }
            else
            {
                // Wrong letter collected!
                if (feedbackText != null)
                {
                    feedbackText.text = "Wrong Letter!";
                    feedbackText.color = Color.red;
                }
                
                // Remove the wrong letter
                collectedText.text = collected.Substring(0, collectedLength - 1);
                previousCollectedText = collectedText.text;
                
                // Clear feedback after 1 second
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
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
    
    // Public method to clear collected letters (can be called by a button)
    public void ClearCollectedLetters()
    {
        if (collectedText != null)
            collectedText.text = "";
        
        previousCollectedText = "";
        
        if (feedbackText != null)
            feedbackText.text = "";
    }

    // Public method to skip current word (optional)
    public void SkipWord()
    {
        currentWordIndex++;
        SetNewTargetWord();
    }
    
        public string GetCurrentWord()
{
    return currentTargetWord;
}

}