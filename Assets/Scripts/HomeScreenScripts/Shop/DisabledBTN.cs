using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisabledBTN : MonoBehaviour
{
    [Header("UI References")]
    public Button targetButton;         // The button to disable
    public TMP_Text buttonText;         // The text that shows "PURCHASED"

    void Start()
    {
        // Automatically find references if not assigned
        if (targetButton == null)
            targetButton = GetComponent<Button>();

        if (buttonText == null)
            buttonText = GetComponentInChildren<TMP_Text>();

        CheckIfPurchased();
    }

    void Update()
    {
        // Continuously check (optional but useful if text changes dynamically)
        CheckIfPurchased();
    }

    void CheckIfPurchased()
    {
        if (buttonText != null && targetButton != null)
        {
            // If the text is exactly "PURCHASED", disable the button
            if (buttonText.text.Trim().ToUpper() == "PURCHASED")
            {
                targetButton.interactable = false;
            }
            else
            {
                targetButton.interactable = true;
            }
        }
    }
}
