using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject OtherThingsCanvas;

    private PlayerFunctions playerFunctions;

    void Start()
    {
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
        SaveCoins();
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
        // ✅ 1. Save coins before restart
        SaveCoins();

        // ✅ 2. Subtract one energy (affects HomeScreen Power)
        DeductEnergyFromHome();

        // ✅ 3. Restart the level
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SaveCoins()
    {
        if (playerFunctions != null)
        {
            playerFunctions.SaveTotalCoins();
            Debug.Log("💾 Coins saved before restarting or leaving the scene.");
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerFunctions not found! Coins might not be saved.");
        }
    }

    private void DeductEnergyFromHome()
    {
        int maxPower = 5; // default number of energy letters (P, O, W, E, R)
        int currentPower = PlayerPrefs.GetInt("currentPower", maxPower);

        if (currentPower > 0)
        {
            currentPower--;
            PlayerPrefs.SetInt("currentPower", currentPower);

            // Save the next recharge time for the used slot
            int index = maxPower - currentPower - 1;
            DateTime nextRecharge = DateTime.Now.AddMinutes(15);
            PlayerPrefs.SetString($"rechargeTime_{index}", nextRecharge.ToBinary().ToString());

            PlayerPrefs.Save();
            Debug.Log($"⚡ Restart pressed → 1 energy used. Remaining: {currentPower}");
        }
        else
        {
            Debug.Log("⚠️ No energy left to restart!");
        }
    }
}
