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
    public GameplayCostumeManager costumeManager;

    public static bool isGirl;

    void Start()
    {
        int selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 1);
        int costumeIndex = PlayerPrefs.GetInt("EquippedCostume", 0);

        isGirl = (selectedCharacter == 2);

        // send gender to costume manager
        if (costumeManager != null)
            costumeManager.useGirlCostumes = isGirl;

        // Hide all
        defaultGirl.SetActive(false);
        rainyGirl.SetActive(false);
        christmasGirl.SetActive(false);
        defaultBoy.SetActive(false);
        rainyBoy.SetActive(false);
        christmasBoy.SetActive(false);

        // Activate correct model based on index
        if (isGirl)
        {
            if (costumeIndex == 0) defaultGirl.SetActive(true);
            if (costumeIndex == 1) rainyGirl.SetActive(true);
            if (costumeIndex == 2) christmasGirl.SetActive(true);
        }
        else
        {
            if (costumeIndex == 0) defaultBoy.SetActive(true);
            if (costumeIndex == 1) rainyBoy.SetActive(true);
            if (costumeIndex == 2) christmasBoy.SetActive(true);
        }

        // also tell gameplay manager which costume index to use
        if (costumeManager != null)
            costumeManager.SetCostumeByIndex(costumeIndex);
    }
}