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

    [Header("Reference")]
    public GameplayCostumeManager costumeManager; // may be null if in different scene

    public static bool isGirl;

    void Start()
    {
        int selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 1);
        int costumeIndex = PlayerPrefs.GetInt("EquippedCostume", 0);

        isGirl = (selectedCharacter == 2);

        // If the manager is in the same scene, update it directly
        if (costumeManager != null)
        {
            costumeManager.SetCharacter(isGirl, costumeIndex);
        }
        // Otherwise, we rely on PlayerPrefs – already saved, so nothing else needed

        // Hide all display models
        defaultGirl.SetActive(false);
        rainyGirl.SetActive(false);
        christmasGirl.SetActive(false);
        defaultBoy.SetActive(false);
        rainyBoy.SetActive(false);
        christmasBoy.SetActive(false);

        // Activate correct display model
        if (isGirl)
        {
            if (costumeIndex == 0) defaultGirl.SetActive(true);
            else if (costumeIndex == 1) rainyGirl.SetActive(true);
            else if (costumeIndex == 2) christmasGirl.SetActive(true);
        }
        else
        {
            if (costumeIndex == 0) defaultBoy.SetActive(true);
            else if (costumeIndex == 1) rainyBoy.SetActive(true);
            else if (costumeIndex == 2) christmasBoy.SetActive(true);
        }
    }
}