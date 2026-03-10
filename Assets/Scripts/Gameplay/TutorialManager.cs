using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        [Header("Step Settings")]
        [Tooltip("Seconds to wait before this step starts.")]
        public float delayBeforeStep = 0f;

        [Tooltip("How long the panel stays on screen.")]
        public float displayDuration = 3f;

        [Range(0f, 1f)]
        [Tooltip("Time scale while this step is active.")]
        public float slowMoScale = 0.2f;

        [TextArea(2, 4)]
        public string message = "Swipe to move!";
    }

    [Header("Tutorial Panel")]
    public GameObject tutorialPanel;
    public TMP_Text tutorialBodyText;
    public TMP_Text tapPrompt;

    [Header("Sequence")]
    public TutorialStep[] steps;

    [Header("Initial Delay")]
    [Tooltip("Seconds after game start before the first step appears.")]
    public float initialDelay = 2f;

    [Header("Messages")]
    public string tapMessage = "Tap to continue →";

    [Header("Developer")]
    public bool resetTutorialFlag = false;

    private const string TUTORIAL_DONE_KEY = "TutorialDone";
    private int currentStepIndex = -1;
    private bool tutorialActive = false;
    private float originalTimeScale = 1f;

    void Awake()
    {
        if (resetTutorialFlag)
        {
            PlayerPrefs.DeleteKey(TUTORIAL_DONE_KEY);
            PlayerPrefs.Save();
            resetTutorialFlag = false;
            Debug.Log("Tutorial flag reset.");
        }
    }

    void Start()
    {
        // Hide panel at start
        if (tutorialPanel != null && tutorialPanel.activeSelf)
            tutorialPanel.SetActive(false);

        // If already done, disable
        if (PlayerPrefs.HasKey(TUTORIAL_DONE_KEY) || steps.Length == 0)
        {
            enabled = false;
            return;
        }

        // Start the sequence after initial delay
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSecondsRealtime(initialDelay);
        StartNextStep();
    }

    void StartNextStep()
    {
        currentStepIndex++;
        if (currentStepIndex >= steps.Length)
        {
            FinishTutorial();
            return;
        }

        TutorialStep step = steps[currentStepIndex];
        StartCoroutine(RunStep(step));
    }

    IEnumerator RunStep(TutorialStep step)
    {
        // Wait for the step's own delay
        if (step.delayBeforeStep > 0f)
            yield return new WaitForSecondsRealtime(step.delayBeforeStep);

        tutorialActive = true;

        // Update text
        if (tutorialBodyText != null)
            tutorialBodyText.text = step.message;
        if (tapPrompt != null)
            tapPrompt.text = tapMessage;

        // Show panel
        tutorialPanel.SetActive(true);

        // Apply slow motion
        originalTimeScale = Time.timeScale;
        Time.timeScale = step.slowMoScale;

        Debug.Log($"Step {currentStepIndex + 1}: {step.message} (delay {step.delayBeforeStep}s, duration {step.displayDuration}s, slow‑mo {step.slowMoScale})");

        // Wait for either tap or timeout
        float timer = 0f;
        while (timer < step.displayDuration && tutorialActive)
        {
            if (Input.anyKeyDown)
            {
                AdvanceToNextStep();
                yield break;
            }
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (tutorialActive)
            AdvanceToNextStep();
    }

    void AdvanceToNextStep()
    {
        if (!tutorialActive) return;

        tutorialPanel.SetActive(false);
        tutorialActive = false;
        Time.timeScale = originalTimeScale;

        // Immediately proceed to next step (its own delay will be applied)
        StartNextStep();
    }

    void FinishTutorial()
    {
        tutorialActive = false;
        tutorialPanel.SetActive(false);
        Time.timeScale = originalTimeScale;
        PlayerPrefs.SetInt(TUTORIAL_DONE_KEY, 1);
        PlayerPrefs.Save();
        Debug.Log("Tutorial complete.");
    }

    public void SkipTutorial()
    {
        StopAllCoroutines();
        FinishTutorial();
    }
}