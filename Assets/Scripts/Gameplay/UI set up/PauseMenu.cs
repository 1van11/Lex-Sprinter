using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject OtherThingsCanvas;

    private PlayerFunctions playerFunctions; // Reference to access SaveTotalCoins

    void Start()
    {
        // Find the PlayerFunctions script in your scene
        playerFunctions = FindObjectOfType<PlayerFunctions>();
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        OtherThingsCanvas.SetActive(false);
        Time.timeScale = 0;
    }

    public void Home()
    {
        // ✅ Save total coins before going back to HomeScreen
        if (playerFunctions != null)
        {
            playerFunctions.SaveTotalCoins();
            Debug.Log("💾 Coins saved before returning to HomeScreen.");
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerFunctions not found! Coins might not be saved.");
        }

        // Resume time and load HomeScreen
        Time.timeScale = 1;
        SceneManager.LoadScene("HomeScreen");
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
