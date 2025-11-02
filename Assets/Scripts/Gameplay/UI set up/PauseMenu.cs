using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject OtherThingsCanvas;

    public void Pause()
    {
        pauseMenu.SetActive(true);
        OtherThingsCanvas.SetActive(false);
        Time.timeScale = 0;
    }

    public void Home()
    {
        SceneManager.LoadScene("HomeScreen");
        Time.timeScale = 1;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        OtherThingsCanvas.SetActive(true);
        Time.timeScale = 1;
    }

    public void Restart()
    {   
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
