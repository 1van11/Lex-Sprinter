using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuOnclicks : MonoBehaviour
{
    public PowerManager powerManager;

    public void PlayButton()
    {
        if (powerManager.UsePower())
        {
            SceneManager.LoadScene("EasyMode");
        }
        else
        {
            Debug.Log("Not enough stamina to play!");
        }
    }
}
