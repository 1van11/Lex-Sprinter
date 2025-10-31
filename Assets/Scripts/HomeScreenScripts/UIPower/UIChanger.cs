using UnityEngine;
using UnityEngine.UI;

public class UIChanger : MonoBehaviour
{
    [Header("Letter Images (R, E, W, O, P order)")]
    public Image[] letterImages;   // The Images in your Hierarchy

    [Header("Power Sprites (R, E, W, O, P order)")]
    public Sprite[] powerSprites;  // The new sprites (different colors/designs)

    [Header("Controls")]
    public Button playButton;

    private int currentIndex = 0;

    void Start()
    {
        playButton.onClick.AddListener(ChangeNextLetter);
    }

    void ChangeNextLetter()
    {
        if (currentIndex >= letterImages.Length) return; // stop if finished

        // Swap the sprite of the current letter
        if (letterImages[currentIndex] != null && powerSprites[currentIndex] != null)
        {
            letterImages[currentIndex].sprite = powerSprites[currentIndex];
        }

        currentIndex++; // move to next letter
    }
}
