using UnityEngine;
using System.Collections;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TMP_Text countdownText;          // TextMeshPro countdown text
    [SerializeField] GameObject OtherThingsCanvas;    // Canvas to hide during countdown

    [Header("Countdown Settings")]
    [SerializeField] float countdownDuration = 0.7f;  // Base duration per number
    [SerializeField] float scaleUpAmount = 1.5f;     // How much the number scales up
    [SerializeField] float scaleSpeed = 5f;          // Speed of scale animation

    private void Awake()
    {
        // Auto-find references if they're missing (helps in duplicated scenes)
        if (countdownText == null)
            countdownText = GetComponentInChildren<TMP_Text>();

        if (OtherThingsCanvas == null)
        {
            // Try to find by name (adjust if your canvas has a different name)
            OtherThingsCanvas = GameObject.Find("OtherThingsCanvas");
            // If still null, try finding any canvas with that tag or just any canvas
            if (OtherThingsCanvas == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                    OtherThingsCanvas = canvas.gameObject;
            }
        }

        // Ensure text is initially hidden
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
        else
            Debug.LogError("CountdownTimer: countdownText is missing! Please assign it in the Inspector.");
    }

    private void Start()
    {
        // Start countdown automatically (you can disable this if you want to call it externally)
        StartCountdown();
    }

    public void StartCountdown()
    {
        // Only start if we have the text reference
        if (countdownText != null)
            StartCoroutine(CountdownSequence());
        else
            Debug.LogError("Cannot start countdown: countdownText is not assigned!");
    }

    private IEnumerator CountdownSequence()
    {
        // Hide other UI elements if they exist
        if (OtherThingsCanvas != null)
            OtherThingsCanvas.SetActive(false);
        else
            Debug.LogWarning("OtherThingsCanvas is not assigned – continuing without hiding.");

        // Show countdown text
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.raycastTarget = false; // prevent blocking input

            float duration = countdownDuration;

            for (int i = 3; i > 0; i--)
            {
                countdownText.text = i.ToString();
                StartCoroutine(ScaleNumber(countdownText.transform));
                Debug.Log("Countdown: " + i);
                yield return new WaitForSeconds(duration);

                // Make the countdown slightly faster each number
                duration *= 0.5f;
            }

            countdownText.text = "go!";
            StartCoroutine(ScaleNumber(countdownText.transform, 1.8f)); // bigger GO effect
            Debug.Log("Countdown: GO!");
            yield return new WaitForSeconds(0.5f);

            countdownText.gameObject.SetActive(false);
            Debug.Log("Countdown finished - hiding text");
        }
        else
        {
            Debug.LogError("CountdownSequence: countdownText is null – cannot show countdown.");
        }

        // Show other UI again
        if (OtherThingsCanvas != null)
        {
            OtherThingsCanvas.SetActive(true);
            Debug.Log("OtherThingsCanvas is now visible");
        }
    }

    // Smooth scale animation for countdown numbers
    private IEnumerator ScaleNumber(Transform target, float targetScale = -1f)
    {
        if (target == null) yield break;

        if (targetScale < 0f) targetScale = scaleUpAmount; // default scale

        Vector3 originalScale = target.localScale;  // use current scale instead of Vector3.one
        Vector3 goalScale = originalScale * targetScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * scaleSpeed; // use unscaled time to avoid issues with paused game
            target.localScale = Vector3.Lerp(originalScale, goalScale, t);
            yield return null;
        }
        target.localScale = goalScale;   // ensure exact final up-scale

        // shrink back to original scale
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * scaleSpeed;
            target.localScale = Vector3.Lerp(goalScale, originalScale, t);
            yield return null;
        }
        target.localScale = originalScale;   // restore exactly
    }

    // Optional: call externally to trigger countdown
    public void TriggerCountdown()
    {
        StartCountdown();
    }
}