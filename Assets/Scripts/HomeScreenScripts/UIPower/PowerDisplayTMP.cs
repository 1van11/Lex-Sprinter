using UnityEngine;
using TMPro;

public class PowerDisplayTMP : MonoBehaviour
{
    public PowerManager powerManager;
    public TextMeshProUGUI powerText;

    public Color partialColor = Color.blue;
    public Color emptyColor = Color.red;

    private void OnEnable()
    {
        powerManager.OnPowerChanged += UpdateDisplay;
    }

    private void OnDisable()
    {
        powerManager.OnPowerChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int current, int max)
    {
        string coloredCurrent;

        if (current == 0)
        {
            coloredCurrent = $"<color=#{ColorUtility.ToHtmlStringRGB(emptyColor)}>{current}</color>";
        }
        else if (current < max)
        {
            coloredCurrent = $"<color=#{ColorUtility.ToHtmlStringRGB(partialColor)}>{current}</color>";
        }
        else
        {
            coloredCurrent = current.ToString();
        }

        powerText.text = $"Remaining Power:\n{coloredCurrent}/{max}";
    }
}