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

    // Make sure UI references exist and format is correct
    if (targetWordText != null && collectedText != null && targetWordText.text.Contains(":"))
    {
        // Extract real target word
        string fullTarget = targetWordText.text.Split(':')[1].Trim().ToLower();
        string collected = collectedText.text.Trim().ToLower();

        // Determine next letter needed
        if (collected.Length < fullTarget.Length)
        {
            char nextNeededLetter = fullTarget[collected.Length];

            // Increase chance for next needed letter
            float boostedChance = correctLetterChance + 0.4f; // +40% stronger bias
            boostedChance = Mathf.Clamp01(boostedChance);

            if (Random.value <= boostedChance)
            {
                return char.ToUpper(nextNeededLetter);
            }
        }
    }

    // Otherwise, random letter
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