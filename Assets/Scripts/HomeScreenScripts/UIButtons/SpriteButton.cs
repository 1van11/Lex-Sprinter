using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Outline))]
public class SpriteButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Image targetImage;
    public Sprite defaultSprite;
    public Sprite clickedSprite;

    [Header("Lock Settings")]
    public bool isLocked = false;
    public GameObject lockIcon; // The UI sprite/image for the lock
    public Sprite lockedSprite; // Optional: different sprite when locked

    [Header("Glow Effect")]
    public Color glowColor = Color.yellow;
    public float glowMaxThickness = 10f;
    public float pulseSpeed = 2f;
    public float fadeSpeed = 0.2f;

    [Header("Animation Settings")]
    public float animationSpeed = 0.25f;
    public float raisedY = 30f;
    public float expandedScale = 1.15f;
    public static float moveDistance = 10f;

    private Vector3 defaultScale;
    private Vector2 defaultAnchoredPos;
    private bool initPosSaved = false;

    private RectTransform rectTransform;
    private Outline outline;

    private static SpriteButton activeButton;
    private static List<SpriteButton> allButtons = new List<SpriteButton>();

    private Coroutine glowRoutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        defaultScale = transform.localScale;
        allButtons.Add(this);

        outline = GetComponent<Outline>();
        outline.effectColor = Color.clear;
        outline.effectDistance = Vector2.zero;

        // Initialize lock icon visibility
        UpdateLockVisual();
    }

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        if (!initPosSaved)
        {
            defaultAnchoredPos = rectTransform.anchoredPosition;
            initPosSaved = true;
        }
    }

    private void UpdateLockVisual()
    {
        if (lockIcon != null)
        {
            lockIcon.SetActive(isLocked);
        }

        // Optional: Change sprite when locked
        if (isLocked && lockedSprite != null)
        {
            targetImage.sprite = lockedSprite;
        }
    }

    public void OnButtonClick()
    {
        // Prevent interaction if locked
        if (isLocked)
        {
            // Optional: Add a shake animation or sound effect here
            StartCoroutine(ShakeButton());
            return;
        }

        if (activeButton == this) return;

        if (activeButton != null)
            activeButton.ResetToDefault();

        targetImage.sprite = clickedSprite;
        activeButton = this;
        AnimateButtons();

        if (glowRoutine != null) StopCoroutine(glowRoutine);
        glowRoutine = StartCoroutine(PulseGlow());
    }

    // Optional: Shake animation when trying to click locked button
    private IEnumerator ShakeButton()
    {
        Vector2 originalPos = rectTransform.anchoredPosition;
        float shakeDuration = 0.3f;
        float shakeMagnitude = 5f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = originalPos.x + Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = originalPos.y + Random.Range(-shakeMagnitude, shakeMagnitude);

            rectTransform.anchoredPosition = new Vector2(x, y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = originalPos;
    }

    // Public method to unlock the button
    public void UnlockButton()
    {
        isLocked = false;
        UpdateLockVisual();

        // Restore default sprite
        if (targetImage != null && defaultSprite != null)
        {
            targetImage.sprite = defaultSprite;
        }
    }

    // Public method to lock the button
    public void LockButton()
    {
        isLocked = true;
        UpdateLockVisual();
    }

    private void ResetToDefault()
    {
        // Don't reset locked buttons
        if (isLocked) return;

        targetImage.sprite = defaultSprite;

        if (glowRoutine != null) StopCoroutine(glowRoutine);
        StartCoroutine(FadeGlowOut());

        rectTransform.anchoredPosition = defaultAnchoredPos;
        transform.localScale = defaultScale;
    }

    private IEnumerator PulseGlow()
    {
        float angle = 0f;

        while (true)
        {
            angle += Time.deltaTime * 100f;
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

            float thickness = Mathf.Lerp(0f, glowMaxThickness, pulse);

            float rad = angle * Mathf.Deg2Rad;
            float offsetX = Mathf.Cos(rad) * thickness;
            float offsetY = Mathf.Sin(rad) * thickness;

            outline.effectColor = glowColor;
            outline.effectDistance = new Vector2(offsetX, offsetY);

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

    private void AnimateButtons()
    {
        foreach (var btn in allButtons)
        {
            // Skip locked buttons in animation
            if (btn.isLocked) continue;

            btn.StopAllCoroutines();

            if (btn == this)
            {
                btn.glowRoutine = StartCoroutine(btn.AnimateTo(
                    btn.defaultAnchoredPos + Vector2.up * raisedY,
                    btn.defaultScale * expandedScale
                ));
            }
            else
            {
                btn.glowRoutine = StartCoroutine(btn.AnimateTo(
                    btn.GetOutwardPos(),
                    btn.defaultScale
                ));
            }
        }
    }

    private Vector2 GetOutwardPos()
    {
        int idx = transform.GetSiblingIndex();
        int actIdx = activeButton.transform.GetSiblingIndex();
        Vector2 pos = defaultAnchoredPos;

        if (idx < actIdx) pos.x -= moveDistance;
        if (idx > actIdx) pos.x += moveDistance;

        return pos;
    }

    private IEnumerator AnimateTo(Vector2 targetPos, Vector3 targetScale)
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector3 startScale = transform.localScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / animationSpeed;
            float smooth = EaseOutCubic(t);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, smooth);
            transform.localScale = Vector3.Lerp(startScale, targetScale, smooth);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
        transform.localScale = targetScale;
    }

    private float EaseOutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);

    void OnDestroy()
    {
        allButtons.Remove(this);
    }
}