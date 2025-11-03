using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LetterHurdleManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text wordToSpellText;   // Shows the word player should spell
    public TMP_Text playerInputText;   // Shows letters player has collected

    [Header("Word List")]
    public string[] wordList = new string[]
    {
        "dog","hat","pink","sun","leg","meat","cup","pair","tree","black",
        "fast","swim","you","bed","hand","bird","milk","jump","bread","cake",
        "foot","girl","nose","blue","car","corn","aunt","cow","happy","red",
        "arm","friend","box","bag","chair","hop","dad","kid","hen","rice",
        "slow","book","pig","baby","him","bell","toy","mom","sad","leaf",
        "write","white","cat","goat","desk","moon","rain","snow","lip","jam",
        "boy","uncle","good","bad","walk","stand","sing","bus","door","map",
        "egg","pear","duck","mouse","wind"
    };

    private string currentWord;
    private int currentLetterIndex = 0;

    void Start()
    {
        NextWord();
    }

    void NextWord()
    {
        int index = Random.Range(0, wordList.Length);
        currentWord = wordList[index];

        wordToSpellText.text = currentWord;
        playerInputText.text = "";
        currentLetterIndex = 0;
    }

    // Call this whenever the player collects a letter
    public void AddLetter(char letter)
    {
        // Check if collected letter matches the next letter in the word
        if (currentLetterIndex < currentWord.Length && letter == currentWord[currentLetterIndex])
        {
            playerInputText.text += letter;
            currentLetterIndex++;

            // Word completed
            if (currentLetterIndex >= currentWord.Length)
            {
                Debug.Log("Correct! Word completed.");
                NextWord();
            }
        }
        else
        {
            Debug.Log("Wrong letter! Word failed.");
            NextWord(); // reset to a new word
        }

        // Limit the input to the word length
        if (playerInputText.text.Length >= currentWord.Length)
        {
            if (playerInputText.text != currentWord)
            {
                Debug.Log("Failed to spell the word within allowed letters. Picking a new word.");
                NextWord();
            }
        }
    }
}
