using System.Collections;            // ✅ Required for IEnumerator
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScreenZoom : MonoBehaviour
{
    [Header("Splash Zoom Settings")]
    public RectTransform splashUI;       // Your splash UI object (Image or Panel)
    public float zoomDuration = 2f;      // Time for zoom animation
    public float zoomScale = 1.2f;       // Final zoom scale

    [Header("References")]
    public GameObject loadingScreenParent;  // Parent GameObject for loading screen (disabled initially)

    private Vector3 initialScale;

    private void Start()
    {
        initialScale = splashUI.localScale;
        loadingScreenParent.SetActive(false); // Make sure loading screen is hidden first
        StartCoroutine(ZoomSplash());
    }

    private IEnumerator ZoomSplash()  // ✅ IEnumerator works now
    {
        float elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            float t = elapsedTime / zoomDuration;
            splashUI.localScale = Vector3.Lerp(initialScale, initialScale * zoomScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // End of splash: hide splash and show loading screen
        splashUI.gameObject.SetActive(false);
        loadingScreenParent.SetActive(true);

        // Call loading screen logic
        LoadingScreen loading = loadingScreenParent.GetComponent<LoadingScreen>();
        if (loading != null)
        {
            loading.StartLoadingNextScene(); // Start loading next scene
        }
    }
}
