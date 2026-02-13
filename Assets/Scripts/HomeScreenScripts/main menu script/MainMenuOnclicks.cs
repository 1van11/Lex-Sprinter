using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuOnclicks : MonoBehaviour
{
    public PowerManager powerManager;

    [SerializeField]
    private string SceneMode;

    public void PlayButton()
    {
        if (powerManager.UsePower())
        {
            SceneManager.LoadScene(SceneMode);
        }
        else
        {
            Debug.Log("Not enough stamina to play!");
        }
    }
}
