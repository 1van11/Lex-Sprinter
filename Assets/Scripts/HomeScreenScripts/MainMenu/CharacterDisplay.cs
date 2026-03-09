using UnityEngine;

public class CharacterDisplay : MonoBehaviour
{
    [Header("Girl Costumes")]
    public GameObject defaultGirl;
    public GameObject rainyGirl;
    public GameObject christmasGirl;

    [Header("Boy Costumes")]
    public GameObject defaultBoy;
    public GameObject rainyBoy;
    public GameObject christmasBoy;

    public static bool isGirl; // other scripts can read this

    void Start()
    {
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 1);
        isGirl = (selected == 2);

        // Hide ALL first
        defaultGirl.SetActive(false);
        rainyGirl.SetActive(false);
        christmasGirl.SetActive(false);
        defaultBoy.SetActive(false);
        rainyBoy.SetActive(false);
        christmasBoy.SetActive(false);

        // Show correct DEFAULT based on selection
        if (isGirl)
            defaultGirl.SetActive(true);
        else
            defaultBoy.SetActive(true);
    }
}