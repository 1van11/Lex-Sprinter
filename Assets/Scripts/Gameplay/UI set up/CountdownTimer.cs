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

    void Start()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        StartCountdown();
    }

    public void StartCountdown()
    {
        StartCoroutine(CountdownSequence());
    }

    private IEnumerator CountdownSequence()
    {
        // Hide other UI elements
        if (OtherThingsCanvas != null)
            OtherThingsCanvas.SetActive(false);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.raycastTarget = false;

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
            Debug.LogWarning("⚠️ countdownText is not assigned!");
        }

        if (OtherThingsCanvas != null)
        {
            OtherThingsCanvas.SetActive(true);
            Debug.Log("✅ OtherThingsCanvas is now visible");
        }
        else
        {
            Debug.LogWarning("⚠️ OtherThingsCanvas is not assigned!");
        }
    }

    // Smooth scale animation for countdown numbers
private IEnumerator ScaleNumber(Transform target, float targetScale = -1f)
{
    if (targetScale < 0f) targetScale = scaleUpAmount; // default scale

    Vector3 originalScale = target.localScale;  // use current scale instead of Vector3.one
    Vector3 goalScale = originalScale * targetScale;

    float t = 0f;
    while (t < 1f)
    {
        t += Time.deltaTime * scaleSpeed;
        target.localScale = Vector3.Lerp(originalScale, goalScale, t);
        yield return null;
    }

    // shrink back to original scale
    t = 0f;
    while (t < 1f)
    {
        t += Time.deltaTime * scaleSpeed;
        target.localScale = Vector3.Lerp(goalScale, originalScale, t);
        yield return null;
    }
}


    // Optional: call externally to trigger countdown
    public void TriggerCountdown()
    {
        StartCoroutine(CountdownSequence());
    }
}
//working