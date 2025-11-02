using System.Collections;            // ✅ Required for IEnumerator
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScreenZoom : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingScreenParent; // Parent GameObject containing loading UI
    public Slider loadingSlider;           // UI Slider for progress
    public Text progressText;              // Optional: display percentage text

    [Header("Scene Settings")]
    public string nextSceneName = "HomeScreen"; // Scene to load
    public float fakeLoadSpeed = 0.5f;          // Speed multiplier for smooth animation

    private void Start()
    {
        // Show the loading screen immediately
        loadingScreenParent.SetActive(true);

        // Start the loading coroutine
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        // Start asynchronous loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        operation.allowSceneActivation = false; // Wait until slider reaches 100%

        float progress = 0f;

        while (!operation.isDone)
        {
            // Target progress (Unity's loading progress goes up to 0.9f)
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Smoothly interpolate slider
            progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime * fakeLoadSpeed);

            // Update slider & text
            if (loadingSlider != null)
                loadingSlider.value = progress;

            if (progressText != null)
                progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

            // Once loading reaches 100%, activate the next scene
            if (progress >= 1f)
            {
                yield return new WaitForSeconds(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
