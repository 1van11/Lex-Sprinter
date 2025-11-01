using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    public Slider loadingBar;   // Assign your slider UI here

    [Header("Settings")]
    public string nextSceneName = "HomeScreen"; // Scene to load
    public float fillSpeed = 0.3f;  // Lower = slower smooth loading

    public void StartLoadingNextScene()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        operation.allowSceneActivation = false;

        float targetProgress = 0f;

        while (!operation.isDone)
        {
            // Calculate the target progress (actual loading progress)
            targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Slowly move the slider toward the target progress
            loadingBar.value = Mathf.MoveTowards(loadingBar.value, targetProgress, fillSpeed * Time.deltaTime);

            // Optional: when almost done, keep it smooth before scene activation
            if (operation.progress >= 0.9f && loadingBar.value >= 0.99f)
            {
                yield return new WaitForSeconds(1f); // Small delay for realism
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
