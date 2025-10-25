using UnityEngine;
using TMPro;
using System.Collections;

public class AnswerOption : MonoBehaviour
{
    [Header("Answer Settings")]
    public bool isCorrect;
    public string answerText;

    [Header("3D Text Reference")]
    public TextMeshPro answerTMP;

    [Header("Player References")]
    public Renderer[] playerRenderers;

    [Header("UI Reference")]
    public GameObject bgTopQuestions;

    [Header("Sound Settings")]
    public AudioSource audioSource;
    public AudioClip correctSound1;   // 🎵 First correct sound
    public AudioClip correctSound2;   // 🎵 Second correct sound
    public AudioClip wrongSound;

    private bool isInvincible = false;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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

                // 🔊 Play both correct sounds
                if (correctSound1 != null)
                    audioSource.PlayOneShot(correctSound1);

                if (correctSound2 != null)
                    audioSource.PlayOneShot(correctSound2);
            }
            else
            {
                Debug.Log("❌ Wrong Answer: " + answerText);

                if (answerTMP != null)
                {
                    answerTMP.color = Color.red;
                    answerTMP.fontStyle = FontStyles.Italic;
                }

                if (wrongSound != null)
                    audioSource.PlayOneShot(wrongSound);

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
            foreach (Renderer rend in playerRenderers)
                rend.enabled = false;

            yield return new WaitForSeconds(0.2f);

            foreach (Renderer rend in playerRenderers)
                rend.enabled = true;

            yield return new WaitForSeconds(0.2f);

            elapsed += 0.4f;
        }

        foreach (Renderer rend in playerRenderers)
            rend.enabled = true;

        isInvincible = false;
    }
}
