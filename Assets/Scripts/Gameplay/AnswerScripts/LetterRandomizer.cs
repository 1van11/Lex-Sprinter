using UnityEngine;
using TMPro;

public class LetterRandomizer : MonoBehaviour
{
    public TMP_Text displayText;       // Shows letter on the object (optional)
    public TMP_Text collectedText;     // Shows collected letters in UI
    public TMP_Text targetWordText;    // Reference to the target word UI text
    public float correctLetterChance = 0.4f; // 40% chance to spawn needed letter

    private char letter;

    void OnEnable()
    {
        // Pick a letter based on chance - runs every time object is enabled
        letter = GetSpawnedLetter();

        if (displayText != null)
            displayText.text = letter.ToString();
    }

    char GetSpawnedLetter()
    {
        correctLetterChance = Mathf.Clamp01(correctLetterChance);

        if (targetWordText != null && Random.value <= correctLetterChance)
        {
            // Extract the word from "Spell: WORD" format
            string displayedText = targetWordText.text;
            if (displayedText.Contains(":"))
            {
                string word = displayedText.Split(':')[1].Trim().ToLower();
                
                if (!string.IsNullOrEmpty(word))
                {
                    // Pick random letter from the current target word
                    int index = Random.Range(0, word.Length);
                    return char.ToUpper(word[index]);
                }
            }
        }

        // Return completely random letter
        return (char)Random.Range('A', 'Z' + 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add collected letter
            if (collectedText != null)
                collectedText.text += char.ToLower(letter);

            // Return object to pool by disabling (spawner handles pooling)
            gameObject.SetActive(false);
        }
    }
}