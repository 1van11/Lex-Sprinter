using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScreenZoom : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingScreenParent;
    public Slider loadingSlider;
    public Text progressText;

    [Header("Scene Settings")]
    public string setupSceneName = "Name&Character";
    public string homeSceneName = "HomeScreen";
    public float fakeLoadSpeed = 0.5f;

    [Header("Smooth Fill Settings")]
    public float smoothSpeed = 2f;        // How fast the slider fills smoothly
    public float minLoadingTime = 3f;     // ✅ Minimum time the loading will last

    private const string HasCompletedSetupKey = "HasCompletedSetup";

    private void Start()
    {
        // Show the loading screen
        if (loadingScreenParent != null)
            loadingScreenParent.SetActive(true);

        // Reset the loading bar
        if (loadingSlider != null)
            loadingSlider.value = 0f;

        // Determine which scene to load
        bool hasCompletedSetup = PlayerPrefs.GetInt(HasCompletedSetupKey, 0) == 1;
        string targetScene = hasCompletedSetup ? homeSceneName : setupSceneName;

        // Start loading coroutine
        StartCoroutine(LoadSceneAsync(targetScene));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float displayProgress = 0f;
        float elapsedTime = 0f;

        while (!operation.isDone)
        {
            // Real loading progress (0 to 1)
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Fake loading delay timer
            elapsedTime += Time.deltaTime;
            float timeProgress = Mathf.Clamp01(elapsedTime / minLoadingTime);

            // Combine both (take whichever is smaller to ensure smooth animation)
            float combinedProgress = Mathf.Min(targetProgress, timeProgress);

            // Smoothly animate slider
            displayProgress = Mathf.Lerp(displayProgress, combinedProgress, Time.deltaTime * smoothSpeed);

            if (loadingSlider != null)
                loadingSlider.value = displayProgress;

            if (progressText != null)
                progressText.text = Mathf.RoundToInt(displayProgress * 100f) + "%";

            // When loading is done AND fake delay is done
            if (displayProgress >= 0.99f && elapsedTime >= minLoadingTime)
            {
                yield return new WaitForSeconds(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
