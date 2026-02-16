using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;

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

    // Stronger banned list
    private string[] bannedWords = {
        "sex","porn","xxx","fuck","shit",
        "bitch","ass","dick","pussy","nude"
    };

    void Start()
    {
        directionalLight1.enabled = false;
        directionalLight2.enabled = false;

        confirmButton.interactable = false;

        button1.onClick.AddListener(ShowLight1);
        button2.onClick.AddListener(ShowLight2);
        confirmButton.onClick.AddListener(SaveAndLoadHomeScreen);

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
    if (string.IsNullOrEmpty(input))
    {
        feedbackText.text = "⚠ Name cannot be empty.";
        feedbackText.color = Color.red;
        hasNameInput = false;
        CheckConfirmButton();
        return;
    }

    if (!char.IsLetter(input[0]))
    {
        feedbackText.text = "⚠ First character must be a letter.";
        feedbackText.color = Color.red;
        hasNameInput = false;
        CheckConfirmButton();
        return;
    }

    // Allow only letters and numbers
    foreach (char c in input)
    {
        if (!char.IsLetterOrDigit(c))
        {
            feedbackText.text = "⚠ Only letters and numbers allowed.";
            feedbackText.color = Color.red;
            hasNameInput = false;
            CheckConfirmButton();
            return;
        }
    }

    if (input.Length > 10)
    {
        feedbackText.text = "⚠ Name cannot exceed 10 characters.";
        feedbackText.color = Color.red;
        hasNameInput = false;
        CheckConfirmButton();
        return;
    }

    // 🔥 Normalize text to detect leetspeak
    string normalized = input.ToLower();

    normalized = normalized
        .Replace("0", "o")
        .Replace("1", "i")
        .Replace("3", "e")
        .Replace("4", "a")
        .Replace("5", "s")
        .Replace("7", "t");

    // Check banned words
    foreach (string word in bannedWords)
    {
        if (normalized.Contains(word))
        {
            feedbackText.text = "⚠ Name contains inappropriate content.";
            feedbackText.color = Color.red;
            hasNameInput = false;
            CheckConfirmButton();
            return;
        }
    }

    // ✅ Valid
    feedbackText.text = "✔ Name looks good!";
    feedbackText.color = Color.green;
    hasNameInput = true;
    CheckConfirmButton();
}



    void CheckConfirmButton()
    {
        confirmButton.interactable = hasNameInput && hasSelectedLight;
    }

    void SaveAndLoadHomeScreen()
    {
        PlayerPrefs.SetString(PlayerNameKey, nameInputField.text);

        int selectedCharacter = directionalLight1.enabled ? 1 : 2;
        PlayerPrefs.SetInt(SelectedCharacterKey, selectedCharacter);

        PlayerPrefs.SetInt(HasCompletedSetupKey, 1);
        PlayerPrefs.Save();

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
