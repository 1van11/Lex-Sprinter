using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Validation : MonoBehaviour
{
    [Header("Directional Lights")]
    public Light directionalLight1;
    public Light directionalLight2;

    [Header("UI Buttons")]
    public Button button1;
    public Button button2;
    public Button confirmButton;

    [Header("Name Input")]
    public TMP_InputField nameInputField;
    public TextMeshProUGUI feedbackText;

    private bool hasNameInput = false;
    private bool hasSelectedLight = false;

    private const string HasCompletedSetupKey = "HasCompletedSetup";
    private const string PlayerNameKey = "PlayerName";
    private const string SelectedCharacterKey = "SelectedCharacter";

    void Start()
    {
        // Set initial state - Both lights off
        directionalLight1.enabled = false;
        directionalLight2.enabled = false;

        // Disable confirm button initially
        confirmButton.interactable = false;

        // Add button listeners
        button1.onClick.AddListener(ShowLight1);
        button2.onClick.AddListener(ShowLight2);
        confirmButton.onClick.AddListener(SaveAndLoadHomeScreen);

        // Name input setup
        nameInputField.characterLimit = 10;
        nameInputField.onValueChanged.AddListener(ValidateInput);
        feedbackText.text = "";
    }

    void ShowLight1()
    {
        directionalLight1.enabled = true;
        directionalLight2.enabled = false;
        hasSelectedLight = true;
        CheckConfirmButton();
    }

    void ShowLight2()
    {
        directionalLight1.enabled = false;
        directionalLight2.enabled = true;
        hasSelectedLight = true;
        CheckConfirmButton();
    }

    void ValidateInput(string input)
    {
        // --- Check 1: Empty input ---
        if (string.IsNullOrEmpty(input))
        {
            hasNameInput = false;
            CheckConfirmButton();
            return;
        }

        string cleanText = "";
        bool hasInvalid = false;

        // --- Check 2: First character must be a letter ---
        if (!char.IsLetter(input[0]))
        {
            feedbackText.text = "\u2718 First character must be a letter (a–z).";
            feedbackText.color = Color.red;
            hasNameInput = false;
            CheckConfirmButton();
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
                    hasInvalid = true;
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
                hasInvalid = true;
            }
        }

        if (hasInvalid)
        {
            feedbackText.text = "\u2718 Only lowercase letters (a–z) and numbers are allowed.";
            feedbackText.color = Color.red;
            nameInputField.text = cleanText;
            nameInputField.caretPosition = nameInputField.text.Length;
            hasNameInput = !string.IsNullOrEmpty(cleanText);
            CheckConfirmButton();
            return;
        }

        // --- Check 4: Maximum 10 characters ---
        if (cleanText.Length > 10)
        {
            feedbackText.text = "\u2718 Name cannot exceed 10 characters.";
            feedbackText.color = Color.red;
            nameInputField.text = cleanText.Substring(0, 10);
            nameInputField.caretPosition = nameInputField.text.Length;
            hasNameInput = true;
            CheckConfirmButton();
            return;
        }

        // ✅ All checks passed
        feedbackText.text = "\u2714 Valid name.";
        feedbackText.color = Color.green;
        nameInputField.text = cleanText;
        hasNameInput = true;
        CheckConfirmButton();
    }

    void CheckConfirmButton()
    {
        // Enable confirm button only if BOTH conditions are met
        confirmButton.interactable = hasNameInput && hasSelectedLight;
    }

    void SaveAndLoadHomeScreen()
    {
        // Save the player's name
        PlayerPrefs.SetString(PlayerNameKey, nameInputField.text);

        // Save which character was selected (1 or 2)
        int selectedCharacter = directionalLight1.enabled ? 1 : 2;
        PlayerPrefs.SetInt(SelectedCharacterKey, selectedCharacter);

        // Mark that setup is complete
        PlayerPrefs.SetInt(HasCompletedSetupKey, 1);
        PlayerPrefs.Save();

        // Load HomeScreen
        SceneManager.LoadScene("HomeScreen");
    }

    IEnumerator ClearInvalidInput()
    {
        yield return null;
        nameInputField.text = "";
    }

    void OnDestroy()
    {
        button1.onClick.RemoveListener(ShowLight1);
        button2.onClick.RemoveListener(ShowLight2);
        confirmButton.onClick.RemoveListener(SaveAndLoadHomeScreen);
    }
}