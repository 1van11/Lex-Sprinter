using UnityEngine;
using UnityEngine.UI;

public class HeartbeatAnimation : MonoBehaviour
{
    [Header("Heartbeat Settings")]
    public float minScale = 0.9f;      // smallest size
    public float maxScale = 1.1f;      // largest size
    public float beatSpeed = 1f;       // how fast it beats (1 = 1 beat per second)

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Calculate the scale using sine wave
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * beatSpeed * Mathf.PI) + 1f) / 2f);
        rectTransform.localScale = new Vector3(scale, scale, 1f);
    }
}
