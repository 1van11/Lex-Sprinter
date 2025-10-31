using UnityEngine;
using UnityEngine.UI;

public class UIButtonSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class ButtonData
    {
        public Button button;            // The actual UI Button
        public Image buttonImage;        // The Image of that Button
        public Sprite defaultSprite;     // Default UI
        public Sprite selectedSprite;    // Selected UI
    }

    public ButtonData[] buttons;

    void Start()
    {
        // Assign click listeners dynamically
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // local copy for the lambda
            buttons[i].button.onClick.AddListener(() => OnButtonPressed(index));
        }

        // Make first button selected at start
        OnButtonPressed(0);
    }

    public void OnButtonPressed(int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == index)
            {
                buttons[i].buttonImage.sprite = buttons[i].selectedSprite;
            }
            else
            {
                buttons[i].buttonImage.sprite = buttons[i].defaultSprite;
            }
        }
    }
}
