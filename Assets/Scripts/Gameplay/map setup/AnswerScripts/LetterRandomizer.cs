using UnityEngine;
using TMPro;

public class LetterRandomizer : MonoBehaviour
{
    public TMP_Text displayText;    // Shows letter on the object (optional)
    public TMP_Text collectedText;  // Shows collected letters in UI

    private char letter;

    void Start()
    {
        letter = GetRandomLetter();

        // Show the letter on the object (uppercase)
        if (displayText != null)
            displayText.text = letter.ToString();
    }

    char GetRandomLetter()
    {
        int letterIndex = Random.Range(65, 91); // A-Z
        return (char)letterIndex;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add this letter in lowercase to the collected text
            if (collectedText != null)
                collectedText.text += char.ToLower(letter);

            // Optionally clear display text (letter disappears from object)
            if (displayText != null)
                displayText.text = "";

            // Deactivate the object so it can't be collected again
            gameObject.SetActive(false);
        }
    }
}
