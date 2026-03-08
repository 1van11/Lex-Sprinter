using UnityEngine;

public class CharacterDisplay : MonoBehaviour
{
    public GameObject boyCharacter;   // drag "Idle0 Lexigirl" BOY object here
    public GameObject girlCharacter;  // drag "Idle1 SummerDress" GIRL object here

    void Start()
    {
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 1);

        boyCharacter.SetActive(selected == 1);   // Light1 = 1 = Boy
        girlCharacter.SetActive(selected == 2);  // Light2 = 2 = Girl
    }
}
