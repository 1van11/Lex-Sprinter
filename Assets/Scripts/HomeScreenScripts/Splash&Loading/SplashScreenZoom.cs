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
    public string setupSceneName = "NameAndCharacter";
    public string homeSceneName = "HomeScreen";
    public float fakeLoadSpeed = 0.5f;

    private const string HasCompletedSetupKey = "HasCompletedSetup";

    private void Start()
    {
        loadingScreenParent.SetActive(true);

        // Check if user has completed setup before
        bool hasCompletedSetup = PlayerPrefs.GetInt(HasCompletedSetupKey, 0) == 1;

        // Load appropriate scene
        string targetScene = hasCompletedSetup ? homeSceneName : setupSceneName;
        StartCoroutine(LoadSceneAsync(targetScene));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float progress = 0f;

        while (!operation.isDone)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime * fakeLoadSpeed);

            if (loadingSlider != null)
                loadingSlider.value = progress;

            if (progressText != null)
                progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

            if (progress >= 1f)
            {
                yield return new WaitForSeconds(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}