using UnityEngine;
using UnityEngine.UI;

public class ButtonSpriteSwitcher : MonoBehaviour
{
    [Header("Buttons")]
    public Button button1;
    public Button button2;

    [Header("Button Images")]
    public Image button1Image;
    public Image button2Image;

    [Header("Button 1 Sprites")]
    public Sprite button1DefaultSprite;
    public Sprite button1ClickedSprite;

    [Header("Button 2 Sprites")]
    public Sprite button2DefaultSprite;
    public Sprite button2ClickedSprite;

    private int activeButton = 1; // 1 = Button1 active, 2 = Button2 active

    void Start()
    {
        // Assign listeners
        button1.onClick.AddListener(() => OnButtonClicked(1));
        button2.onClick.AddListener(() => OnButtonClicked(2));

        // Initialize Button1 as already clicked
        SetActiveButton(1);
    }

    void OnButtonClicked(int buttonNumber)
    {
        if (buttonNumber == activeButton)
            return; // already active, do nothing

        SetActiveButton(buttonNumber);
    }

    void SetActiveButton(int buttonNumber)
    {
        activeButton = buttonNumber;

        if (activeButton == 1)
        {
            // Button1 becomes active, Button2 resets
            button1Image.sprite = button1ClickedSprite;
            button2Image.sprite = button2DefaultSprite;
        }
        else if (activeButton == 2)
        {
            // Button2 becomes active, Button1 resets
            button1Image.sprite = button1DefaultSprite;
            button2Image.sprite = button2ClickedSprite;
        }
    }
}
