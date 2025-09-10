using UnityEngine;
using TMPro;
using System.Collections;

public class AnswerOption : MonoBehaviour
{
    [Header("Answer Settings")]
    public bool isCorrect;
    public string answerText;

    [Header("3D Text Reference")]
    public TextMeshPro answerTMP; // drag your 3D TMP here

    [Header("Player References")]
    public Renderer[] playerRenderers; // drag ALL player parts here (body, head, etc.)

    [Header("UI Reference")]
    public GameObject bgTopQuestions; // drag your BGTopQuestions UI here in Inspector

    private bool isInvincible = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ✅ Hide the background UI when colliding with this question
            if (bgTopQuestions != null)
                bgTopQuestions.SetActive(false);

            if (isCorrect)
            {
                Debug.Log("✅ Correct Answer: " + answerText);

                if (answerTMP != null)
                {
                    answerTMP.color = Color.green;
                    answerTMP.fontStyle = FontStyles.Bold;
                }
            }
            else
            {
                Debug.Log("❌ Wrong Answer: " + answerText);

                if (answerTMP != null)
                {
                    answerTMP.color = Color.red;
                    answerTMP.fontStyle = FontStyles.Italic;
                }

                if (!isInvincible && playerRenderers.Length > 0)
                    StartCoroutine(BlinkPlayer(3f));
            }
        }
    }

    private IEnumerator BlinkPlayer(float duration)
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Hide all renderers
            foreach (Renderer rend in playerRenderers)
                rend.enabled = false;

            yield return new WaitForSeconds(0.2f);

            // Show all renderers
            foreach (Renderer rend in playerRenderers)
                rend.enabled = true;

            yield return new WaitForSeconds(0.2f);

            elapsed += 0.4f;
        }

        // Make sure everything is visible
        foreach (Renderer rend in playerRenderers)
            rend.enabled = true;

        isInvincible = false;
    }
}
