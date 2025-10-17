using System.Collections;
using UnityEngine;

public class AnswerCheck : MonoBehaviour
{
    [Header("Answer Settings")]
    public bool isCorrect = false;

    [Header("Feedback / IFrame Settings")]
    public float iFrameDuration = 2f;
    public float flashInterval = 0.2f;

    [Header("Score (optional)")]
    public int scoreReward = 10;
    public int scorePenalty = -5;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Get or add the iFrame handler on the player so coroutine lives on the player object
        PlayerIFramesHandler handler = other.GetComponent<PlayerIFramesHandler>();
        if (handler == null)
        {
            handler = other.gameObject.AddComponent<PlayerIFramesHandler>();
        }

        if (isCorrect)
        {
            Debug.Log("✅ Correct Answer! +" + scoreReward + " points");
            // TODO: Hook into your score manager here if you have one, e.g.
            // ScoreManager.Instance.Add(scoreReward);
        }
        else
        {
            Debug.Log("❌ Wrong Answer! " + scorePenalty + " penalty");
            handler.StartIFrames(iFrameDuration, flashInterval);
            // TODO: apply score penalty if you use a score system
        }

        // Destroy the whole question prefab (parent). This is safe because iFrames run on player.
        if (transform.parent != null)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }
}

/// <summary>
/// This component runs the iFrame coroutine on the player GameObject.
/// It's designed to be added dynamically by AnswerOption (or you can attach it to player manually).
/// </summary>
public class PlayerIFramesHandler : MonoBehaviour
{
    private bool isInvincible = false;
    private Coroutine runningCoroutine = null;
    private Renderer[] renderersCache;

    /// <summary>
    /// Start invincibility frames: flashing + invincible flag.
    /// Safe to call even if component was just added at runtime.
    /// </summary>
    public void StartIFrames(float duration, float flashInterval)
    {
        if (isInvincible) return; // already invincible
        if (runningCoroutine != null) StopCoroutine(runningCoroutine);

        // cache the renderers (all child renderers - MeshRenderer, SkinnedMeshRenderer, etc.)
        if (renderersCache == null || renderersCache.Length == 0)
            renderersCache = GetComponentsInChildren<Renderer>(true);

        runningCoroutine = StartCoroutine(IFramesRoutine(duration, flashInterval));
    }

    private IEnumerator IFramesRoutine(float duration, float flashInterval)
    {
        isInvincible = true;

        float elapsed = 0f;
        bool visible = true;

        // ensure renderers start visible
        SetRenderersEnabled(true);

        while (elapsed < duration)
        {
            // toggle visibility for flash effect
            visible = !visible;
            SetRenderersEnabled(visible);

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // restore
        SetRenderersEnabled(true);
        isInvincible = false;
        runningCoroutine = null;
    }

    private void SetRenderersEnabled(bool enabled)
    {
        if (renderersCache == null) return;
        for (int i = 0; i < renderersCache.Length; i++)
        {
            if (renderersCache[i] != null)
                renderersCache[i].enabled = enabled;
        }
    }

    // Optional: expose a property if other scripts need to check invincibility
    public bool IsInvincible => isInvincible;
}
