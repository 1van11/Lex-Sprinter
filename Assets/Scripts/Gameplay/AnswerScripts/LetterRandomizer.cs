using UnityEngine;
using TMPro;

public class LetterRandomizer : MonoBehaviour
{
    public TMP_Text displayText;            // Shows letter on the object (optional)
    public TMP_Text collectedText;          // Shows collected letters in UI (formatted "d _ _")
    public TMP_Text targetWordText;         // Reference to the target word UI text
    public float correctLetterChance = 0.4f; // 40% chance to spawn needed letter

    private char letter;

    void OnEnable()
    {
        letter = GetSpawnedLetter();

        if (displayText != null)
            displayText.text = letter.ToString();
    }

    // ── Strips spaces and underscores, returns only real letter characters in lower-case.
    // Mirrors QuestionRandomizer.ExtractRawLetters() so both scripts agree on what "collected" means.
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

    char GetSpawnedLetter()
    {
        correctLetterChance = Mathf.Clamp01(correctLetterChance);

        if (targetWordText != null && collectedText != null && targetWordText.text.Contains(":"))
        {
            // Extract real target word from "Spell: dog" → "dog"
            string fullTarget = targetWordText.text.Split(':')[1].Trim().ToLower();

            // Extract raw collected letters from the formatted "d _ _" display
            string rawCollected = ExtractRawLetters(collectedText.text);

            // Determine next needed letter
            if (rawCollected.Length < fullTarget.Length)
            {
                char nextNeededLetter = fullTarget[rawCollected.Length];

                float boostedChance = Mathf.Clamp01(correctLetterChance + 0.4f);

                if (Random.value <= boostedChance)
                    return char.ToUpper(nextNeededLetter);
            }
        }

        // Fallback: random letter
        return (char)Random.Range('A', 'Z' + 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (collectedText != null)
        {
            // Extract current raw progress from the formatted display ("d _ _" → "d")
            string rawCollected = ExtractRawLetters(collectedText.text);

            // Append the newly collected letter as raw text.
            // QuestionRandomizer.Update() will detect the change on the next frame
            // and reformat it back to "d o _" style automatically.
            collectedText.text = rawCollected + char.ToLower(letter);
        }

        // Return to pool
        gameObject.SetActive(false);
    }
}