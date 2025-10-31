using UnityEngine;
using UnityEngine.UI;

public class SpriteButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Image targetImage;     // The UI Image in the Canvas
    public Sprite defaultSprite;  // Default sprite
    public Sprite clickedSprite;  // Sprite when button is clicked

    private static SpriteButton activeButton; // tracks the last clicked button
    private bool isClicked = false;

    // Called when this button is clicked
    public void OnButtonClick()
    {
        if (targetImage == null) return;

        // Reset the previous active button
        if (activeButton != null && activeButton != this)
        {
            activeButton.ResetToDefault();
        }

        // Always select this button (no self-toggle off)
        targetImage.sprite = clickedSprite;
        isClicked = true;
        activeButton = this;
    }

    // Resets this button to default
    public void ResetToDefault()
    {
        if (targetImage == null) return;

        targetImage.sprite = defaultSprite;
        isClicked = false;
    }

    // Called by your UI Back Button
    public static void ResetAllButtons()
    {
        if (activeButton != null)
        {
            activeButton.ResetToDefault();
            activeButton = null;
        }
    }
}
