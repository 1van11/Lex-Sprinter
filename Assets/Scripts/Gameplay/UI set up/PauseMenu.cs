using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject OtherThingsCanvas;
    [SerializeField] TMP_Text countdownText; // Assign a TMP_Text in the pause menu
    [SerializeField] float resumeDelay = 3f; // seconds

    private PlayerFunctions playerFunctions;
    private bool isResuming = false;

    void Start()
    {
        playerFunctions = FindObjectOfType<PlayerFunctions>();
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
            countdownText.raycastTarget = false; // Ensure numbers are not tappable
        }

        // Ensure pause menu starts fully visible and interactable
        CanvasGroup pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null)
            pauseCanvasGroup = pauseMenu.AddComponent<CanvasGroup>();

        pauseCanvasGroup.alpha = 1f;
        pauseCanvasGroup.interactable = true;
        pauseCanvasGroup.blocksRaycasts = true;
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        OtherThingsCanvas.SetActive(false);
        Time.timeScale = 0;

        // Ensure pause menu is fully visible and tappable when paused
        CanvasGroup pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }
    }

    public void Resume()
    {
        if (!isResuming)
            StartCoroutine(ResumeWithCountdown());
    }

    private IEnumerator ResumeWithCountdown()
    {
        isResuming = true;

        CanvasGroup pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null)
            pauseCanvasGroup = pauseMenu.AddComponent<CanvasGroup>();

        // Reset menu fully visible first
        pauseCanvasGroup.alpha = 1f;
        pauseCanvasGroup.interactable = true;
        pauseCanvasGroup.blocksRaycasts = true;

        // 1️⃣ Make pause menu semi-transparent & untappable
        pauseCanvasGroup.alpha = 0f; // semi-transparent during countdown
        pauseCanvasGroup.interactable = false; 
        pauseCanvasGroup.blocksRaycasts = false;

        // 2️⃣ Show countdown text
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.color = new Color(countdownText.color.r, countdownText.color.g, countdownText.color.b, 1f);
        }

        float timer = resumeDelay;
        while (timer > 0)
        {
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(timer).ToString();

            yield return new WaitForSecondsRealtime(0.5f);

            // Fade out countdown smoothly
            float fadeDuration = 0.5f;
            float fadeTimer = 0f;
            while (fadeTimer < fadeDuration)
            {
                if (countdownText != null)
                {
                    float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
                    countdownText.color = new Color(countdownText.color.r, countdownText.color.g, countdownText.color.b, alpha);
                }
                fadeTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            timer -= 1f;
        }

        // 3️⃣ Hide countdown text
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        // 4️⃣ Fully hide pause menu and re-enable game UI
        pauseCanvasGroup.alpha = 0f; // invisible
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;

        OtherThingsCanvas.SetActive(true);
        Time.timeScale = 1;

        isResuming = false;
    }

    public void Home()
    {
        SaveCoins();
        Time.timeScale = 1;
        SceneManager.LoadScene("HomeScreen");
    }

    public void Restart()
    {
        SaveCoins();
        DeductEnergyFromHome();
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SaveCoins()
    {
        if (playerFunctions != null)
        {
            playerFunctions.SaveTotalCoins();
            Debug.Log("💾 Coins saved.");
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerFunctions not found!");
        }
    }

    #region DeductEnergy
    private void DeductEnergyFromHome()
    {
        int maxPower = 5;
        int currentPower = PlayerPrefs.GetInt("currentPower", maxPower);

        if (currentPower > 0)
        {
            currentPower--;
            PlayerPrefs.SetInt("currentPower", currentPower);

            int index = maxPower - currentPower - 1;
            DateTime nextRecharge = DateTime.Now.AddMinutes(15);
            PlayerPrefs.SetString($"rechargeTime_{index}", nextRecharge.ToBinary().ToString());

            PlayerPrefs.Save();
            Debug.Log($"⚡ Energy used. Remaining: {currentPower}");
        }
        else
        {
            Debug.Log("⚠️ No energy left!");
        }
    }
    #endregion
}

//delayed, pause menu disappear