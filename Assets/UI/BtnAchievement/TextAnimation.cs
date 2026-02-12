using UnityEngine;
using TMPro;

public class TextAnimation : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float minScale = 0.9f;    // smallest size
    public float maxScale = 1.2f;    // largest size
    public float pulseSpeed = 2f;    // how fast it pulses

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Smooth pulsing using sine wave
        float scaleFactor = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) / 2f);
        rectTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }
}
