using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Outline))]
public class UIImageGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = Color.cyan;   // Glow color
    public float glowThickness = 5f;       // How wide the glow appears
    public float pulseSpeed = 2f;          // Speed of pulsing glow
    public bool enableGlow = true;         // Toggle glow on/off

    [Header("Fade Settings")]
    public float fadeSpeed = 0.3f;         // Fade in/out speed

    private Outline outline;
    private Coroutine glowRoutine;

    void Awake()
    {
        outline = GetComponent<Outline>();
        outline.effectColor = Color.clear;
        outline.effectDistance = Vector2.zero;
    }

    void OnEnable()
    {
        if (enableGlow)
        {
            StartGlow();
        }
    }

    void OnDisable()
    {
        StopGlow();
    }

    /// <summary>
    /// Starts the pulsing corner glow.
    /// </summary>
    public void StartGlow()
    {
        if (glowRoutine != null) StopCoroutine(glowRoutine);
        glowRoutine = StartCoroutine(PulseGlow());
    }

    /// <summary>
    /// Stops the glow effect and fades out smoothly.
    /// </summary>
    public void StopGlow()
    {
        if (glowRoutine != null) StopCoroutine(glowRoutine);
        glowRoutine = StartCoroutine(FadeGlowOut());
    }

    private IEnumerator PulseGlow()
    {
        float angle = 0f;
        while (true)
        {
            angle += Time.deltaTime * 100f;
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

            // Make the outline "move" diagonally, giving a soft glow at corners
            float offset = Mathf.Lerp(0f, glowThickness, pulse);
            outline.effectColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0.8f);
            outline.effectDistance = new Vector2(offset, offset);

            yield return null;
        }
    }

    private IEnumerator FadeGlowOut()
    {
        Color startColor = outline.effectColor;
        Vector2 startDist = outline.effectDistance;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeSpeed;
            outline.effectColor = Color.Lerp(startColor, Color.clear, t);
            outline.effectDistance = Vector2.Lerp(startDist, Vector2.zero, t);
            yield return null;
        }

        outline.effectColor = Color.clear;
        outline.effectDistance = Vector2.zero;
    }
}
