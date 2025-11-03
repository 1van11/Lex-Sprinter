using System.Collections;
using UnityEngine;
using TMPro;

public class NameInput : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_InputField nameInputField;   // The input field for the name
    public TextMeshProUGUI feedbackText;    // The UI Text to display error messages

    private void Start()
    {
        nameInputField.characterLimit = 10; // Limit input to 10 characters
        nameInputField.onValueChanged.AddListener(ValidateInput);
        feedbackText.text = ""; // Clear feedback at start
    }

    void ValidateInput(string input)
    {
        // --- Check 1: Empty input ---
        if (string.IsNullOrEmpty(input))
        {
            // Keep showing last message (do not clear immediately)
            return;
        }

        string cleanText = "";
        bool hasInvalid = false;

        // --- Check 2: First character must be a letter ---
        if (!char.IsLetter(input[0]))
        {
            feedbackText.text = "\u2718 First character must be a letter (a–z)."; // ✘
            feedbackText.color = Color.red;
            StartCoroutine(ClearInvalidInput());
            return;
        }

        // --- Check 3: Only lowercase letters and numbers are allowed ---
        foreach (char c in input)
        {
            if (char.IsLetter(c))
            {
                if (char.IsUpper(c))
                {
                    hasInvalid = true; // uppercase not allowed
                }
                else
                {
                    cleanText += c;
                }
            }
            else if (char.IsDigit(c))
            {
                cleanText += c;
            }
            else
            {
                hasInvalid = true; // special character found
            }
        }

        if (hasInvalid)
        {
            feedbackText.text = "\u2718 Only lowercase letters (a–z) and numbers are allowed."; // ✘
            feedbackText.color = Color.red;
            nameInputField.text = cleanText;
            nameInputField.caretPosition = nameInputField.text.Length;
            return;
        }

        // --- Check 4: Maximum 10 characters ---
        if (cleanText.Length > 10)
        {
            feedbackText.text = "\u2718 Name cannot exceed 10 characters."; // ✘
            feedbackText.color = Color.red;
            nameInputField.text = cleanText.Substring(0, 10);
            nameInputField.caretPosition = nameInputField.text.Length;
            return;
        }

        // ✅ All checks passed
        feedbackText.text = "\u2714 Valid name."; // ✔
        feedbackText.color = Color.green;
        nameInputField.text = cleanText;
    }

    IEnumerator ClearInvalidInput()
    {
        yield return null; // wait a frame so TMP updates UI
        nameInputField.text = "";
    }
}
