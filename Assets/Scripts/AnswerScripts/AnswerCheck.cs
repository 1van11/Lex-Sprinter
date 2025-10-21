using System.Collections;
using UnityEngine;

public class AnswerCheck : MonoBehaviour
{
    [Header("Answer Settings")]
    public string answerWord;         // Word shown on this option
    public int scoreReward = 10;
    public int scorePenalty = -5;

    [Header("Feedback / IFrame Settings")]
    public float iFrameDuration = 2f;
    public float flashInterval = 0.2f;

    // Hardcoded correct answers
    private string[] correctAnswers = new string[]
    {
        "CAT", "CUP", "NOSE", "BOY", "FAST", "DOG", "HAT", "LIP", "GIRL", "SLOW",
        "FISH", "BED", "LEG", "KID", "GOOD", "BIRD", "BOX", "ARM", "BABY", "BAD",
        "COW", "PEN", "HAND", "AUNT", "HAPPY", "PIG", "BOOK", "FOOT", "UNCLE",
        "SAD", "HEN", "CHAIR", "HAIR", "FRIEND", "TOY", "DUCK", "DESK", "TOOTH",
        "RUN", "MAP", "GOAT", "BAG", "RICE", "HOP", "CAR", "MOUSE", "SUN", "EGG",
        "SIT", "BUS", "RED", "MOON", "MILK", "STAND", "DOOR", "BLUE", "STAR",
        "MEAT", "WALK", "BELL", "GREEN", "RAIN", "CORN", "JUMP", "ME", "PINK",
        "SNOW", "PEAR", "SWIM", "YOU", "YELLOW", "TREE", "CAKE", "READ", "HIM",
        "BLACK", "LEAF", "BREAD", "WRITE", "HER", "WHITE", "WIND", "JAM", "SING",
        "BROWN", "CLOUD", "SOUP", "HOT", "GRAY", "ROCK", "MOM", "COLD", "ORANGE",
        "EYE", "DAD", "BIG", "BALL", "EAR", "MAN", "SMALL"
    };

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        bool isCorrect = System.Array.Exists(correctAnswers, word => word == answerWord);

        PlayerIFramesHandler handler = other.GetComponent<PlayerIFramesHandler>();
        if (handler == null)
        {
            handler = other.gameObject.AddComponent<PlayerIFramesHandler>();
        }

        if (isCorrect)
        {
            Debug.Log($"✅ Correct Answer! +{scoreReward} points ({answerWord}) supper galing");
            
        }
        else
        {
            Debug.Log($"❌ right Answer! {scorePenalty} penalty ({answerWord})");
            handler.StartIFrames(iFrameDuration, flashInterval);
            // TODO: deduct score
        }

        Destroy(gameObject); // Remove option after hit
    }
}

public class PlayerIFramesHandler : MonoBehaviour
{
    private bool isInvincible = false;
    private Coroutine runningCoroutine = null;
    private Renderer[] renderersCache;

    public void StartIFrames(float duration, float flashInterval)
    {
        if (isInvincible) return;
        if (runningCoroutine != null) StopCoroutine(runningCoroutine);

        if (renderersCache == null || renderersCache.Length == 0)
            renderersCache = GetComponentsInChildren<Renderer>(true);

        runningCoroutine = StartCoroutine(IFramesRoutine(duration, flashInterval));
    }

    private IEnumerator IFramesRoutine(float duration, float flashInterval)
    {
        isInvincible = true;
        float elapsed = 0f;
        bool visible = true;

        SetRenderersEnabled(true);

        while (elapsed < duration)
        {
            visible = !visible;
            SetRenderersEnabled(visible);

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

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

    public bool IsInvincible => isInvincible;
}
