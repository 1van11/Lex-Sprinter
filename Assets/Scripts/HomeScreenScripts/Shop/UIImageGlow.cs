using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Outline))]
public class UIImageGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = Color.cyan;
    public float glowThickness = 5f;
    public float pulseSpeed = 2f;
    public bool enableGlow = true;

    [Header("Fade Settings")]
    public float fadeSpeed = 0.3f;

    private Outline outline;
    private Coroutine glowRoutine;
    private Coroutine fadeRoutine;

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
        StopAllRunningCoroutines();
        ClearGlow();
    }

    public void StartGlow()
    {
        if (!gameObject.activeInHierarchy) return;
        
        StopAllRunningCoroutines();
        glowRoutine = StartCoroutine(PulseGlow());
    }

    public void StopGlow()
    {
        if (!gameObject.activeInHierarchy)
        {
            ClearGlow();
            return;
        }
        
        StopAllRunningCoroutines();
        fadeRoutine = StartCoroutine(FadeGlowOut());
    }

    private IEnumerator PulseGlow()
    {
        float angle = 0f;
        while (true)
        {
            angle += Time.deltaTime * 100f;
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

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
        fadeRoutine = null;  // ADD THIS LINE HERE
    }

    private void StopAllRunningCoroutines()
    {
        if (glowRoutine != null)
        {
            StopCoroutine(glowRoutine);
            glowRoutine = null;
        }
        
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }

    private void ClearGlow()
    {
        if (outline != null)
        {
            outline.effectColor = Color.clear;
            outline.effectDistance = Vector2.zero;
        }
    }
}