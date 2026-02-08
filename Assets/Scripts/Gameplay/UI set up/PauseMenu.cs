using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject OtherThingsCanvas;
    [SerializeField] TMP_Text countdownText; // Assign a TMP_Text in the pause menu
    [SerializeField] float slowMotionTimescale = 0.1f; // Ultra slow motion timescale (10x slower)
    [SerializeField] float slowMotionDuration = 3f; // seconds of slow motion

    private PlayerFunctions playerFunctions;
    private bool isResuming = false;
    private Coroutine resumeCoroutine;
    private float originalFixedDeltaTime;
    private Animator[] allAnimators; // Cache all animators for speed control
    private ParticleSystem[] allParticleSystems; // Cache all particle systems
    private AudioSource[] allAudioSources; // Cache all audio sources for pitch adjustment

    void Start()
    {
        playerFunctions = FindObjectOfType<PlayerFunctions>();
        originalFixedDeltaTime = Time.fixedDeltaTime;

        CacheAllAnimationComponents();

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
            countdownText.raycastTarget = false;
        }

        CanvasGroup pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null)
            pauseCanvasGroup = pauseMenu.AddComponent<CanvasGroup>();

        pauseCanvasGroup.alpha = 1f;
        pauseCanvasGroup.interactable = true;
        pauseCanvasGroup.blocksRaycasts = true;
    }

    void CacheAllAnimationComponents()
    {
        allAnimators = FindObjectsOfType<Animator>(true);
        allParticleSystems = FindObjectsOfType<ParticleSystem>(true);
        allAudioSources = FindObjectsOfType<AudioSource>(true);

        Debug.Log($"📊 Cached {allAnimators.Length} animators, {allParticleSystems.Length} particle systems, {allAudioSources.Length} audio sources");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale > slowMotionTimescale)
                Pause();
        }
    }

    public void Pause()
    {
        if (resumeCoroutine != null)
        {
            StopCoroutine(resumeCoroutine);
            resumeCoroutine = null;
            isResuming = false;
            Time.timeScale = 0;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            RestoreAllAnimationsToNormal();
        }

        pauseMenu.SetActive(true);
        OtherThingsCanvas.SetActive(false);
        Time.timeScale = 0;

        PauseAllAnimations();
        PauseAllParticleSystems();
        PauseAllAudio();

        CanvasGroup pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    public void Resume()
    {
        if (!isResuming && Time.timeScale == 0)
        {
            resumeCoroutine = StartCoroutine(ResumeWithSlowMotion());
        }
    }

private IEnumerator ResumeWithSlowMotion()
{
    isResuming = true;

    CanvasGroup pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
    if (pauseCanvasGroup == null)
        pauseCanvasGroup = pauseMenu.AddComponent<CanvasGroup>();

    pauseCanvasGroup.alpha = 0;
    pauseCanvasGroup.interactable = false;
    pauseCanvasGroup.blocksRaycasts = false;

    if (countdownText != null)
    {
        countdownText.gameObject.SetActive(true);
        countdownText.color = new Color(countdownText.color.r, countdownText.color.g, countdownText.color.b, 1f);
    }

    Time.timeScale = slowMotionTimescale;
    Time.fixedDeltaTime = originalFixedDeltaTime * slowMotionTimescale;

    SetAllAnimationsSpeed(slowMotionTimescale);
    SetAllParticleSystemSpeed(slowMotionTimescale);
    SetAllAudioPitch(slowMotionTimescale);

    ResumeAllAnimations();
    ResumeAllParticleSystems();
    ResumeAllAudio();

    float timer = slowMotionDuration;

    // 3, 2, 1 countdown
    while (timer > 0)
    {
        if (countdownText != null)
        {
            countdownText.text = Mathf.CeilToInt(timer).ToString(); // will show 3,2,1
            StartCoroutine(ScaleCountdownNumber(countdownText.transform));
        }

        yield return new WaitForSecondsRealtime(1f); // unscaled time
        timer -= 1f;
    }

    // Show "go" after 1
    if (countdownText != null)
    {
        countdownText.text = "go"; // small letters
        StartCoroutine(ScaleCountdownNumber(countdownText.transform, 1.8f)); // slightly bigger
    }

    yield return new WaitForSecondsRealtime(1f); // show "go" for 1 second

    // Hide countdown text
    if (countdownText != null)
        countdownText.gameObject.SetActive(false);

    Time.timeScale = 1f;
    Time.fixedDeltaTime = originalFixedDeltaTime;

    RestoreAllAnimationsToNormal();
    RestoreAllParticleSystemsToNormal();
    RestoreAllAudioToNormal();

    pauseMenu.SetActive(false);
    OtherThingsCanvas.SetActive(true);

    if (pauseCanvasGroup != null)
    {
        pauseCanvasGroup.alpha = 1f;
        pauseCanvasGroup.interactable = true;
        pauseCanvasGroup.blocksRaycasts = true;
    }

    isResuming = false;
    resumeCoroutine = null;
}

    #region Scale Animation
    private IEnumerator ScaleCountdownNumber(Transform target, float targetScale = -1f)
    {
        if (targetScale < 0f) targetScale = 1.5f;

        Vector3 originalScale = target.localScale;
        Vector3 goalScale = originalScale * targetScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 5f;
            target.localScale = Vector3.Lerp(originalScale, goalScale, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 5f;
            target.localScale = Vector3.Lerp(goalScale, originalScale, t);
            yield return null;
        }
    }
    #endregion

    #region Animation Control Methods
    void PauseAllAnimations()
    {
        foreach (var animator in allAnimators)
            if (animator != null && animator.isActiveAndEnabled) animator.speed = 0f;
    }

    void ResumeAllAnimations()
    {
        foreach (var animator in allAnimators)
            if (animator != null && animator.isActiveAndEnabled) animator.speed = 1f;
    }

    void SetAllAnimationsSpeed(float speed)
    {
        foreach (var animator in allAnimators)
            if (animator != null && animator.isActiveAndEnabled) animator.speed = speed;
    }

    void RestoreAllAnimationsToNormal()
    {
        foreach (var animator in allAnimators)
            if (animator != null && animator.isActiveAndEnabled) animator.speed = 1f;
    }

    void PauseAllParticleSystems()
    {
        foreach (var ps in allParticleSystems)
            if (ps != null && ps.isPlaying) ps.Pause();
    }

    void ResumeAllParticleSystems()
    {
        foreach (var ps in allParticleSystems)
            if (ps != null) ps.Play();
    }

    void SetAllParticleSystemSpeed(float speed)
    {
        foreach (var ps in allParticleSystems)
            if (ps != null) { var main = ps.main; main.simulationSpeed = speed; }
    }

    void RestoreAllParticleSystemsToNormal()
    {
        foreach (var ps in allParticleSystems)
            if (ps != null) { var main = ps.main; main.simulationSpeed = 1f; }
    }

    void PauseAllAudio()
    {
        foreach (var audioSource in allAudioSources)
            if (audioSource != null && audioSource.isPlaying) audioSource.Pause();
    }

    void ResumeAllAudio()
    {
        foreach (var audioSource in allAudioSources)
            if (audioSource != null && !audioSource.isPlaying) audioSource.UnPause();
    }

    void SetAllAudioPitch(float pitch)
    {
        foreach (var audioSource in allAudioSources)
            if (audioSource != null) audioSource.pitch = pitch;
    }

    void RestoreAllAudioToNormal()
    {
        foreach (var audioSource in allAudioSources)
            if (audioSource != null) audioSource.pitch = 1f;
    }
    #endregion

    public bool IsInSlowMotion() => isResuming && Time.timeScale == slowMotionTimescale;

    public void CancelSlowMotion()
    {
        if (resumeCoroutine != null) StopCoroutine(resumeCoroutine);
        resumeCoroutine = null;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        isResuming = false;

        RestoreAllAnimationsToNormal();
        RestoreAllParticleSystemsToNormal();
        RestoreAllAudioToNormal();

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        pauseMenu.SetActive(false);
        OtherThingsCanvas.SetActive(true);
    }

    public void Home()
    {
        CancelSlowMotion();
        SaveCoins();
        Time.timeScale = 1f;
        SceneManager.LoadScene("HomeScreen");
    }

    public void Restart()
    {
        CancelSlowMotion();
        SaveCoins();
        DeductEnergyFromHome();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnDestroy()
    {
        if (Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
        }

        RestoreAllAnimationsToNormal();
        RestoreAllParticleSystemsToNormal();
        RestoreAllAudioToNormal();
    }

    private void SaveCoins()
    {
        if (playerFunctions != null)
        {
            playerFunctions.SaveTotalCoins();
            Debug.Log("💾 Coins saved.");
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerFunctions not found!");
        }
    }

    private void DeductEnergyFromHome()
    {
        int maxPower = 5;
        int currentPower = PlayerPrefs.GetInt("currentPower", maxPower);

        if (currentPower > 0)
        {
            currentPower--;
            PlayerPrefs.SetInt("currentPower", currentPower);

            int index = maxPower - currentPower - 1;
            DateTime nextRecharge = DateTime.Now.AddMinutes(15);
            PlayerPrefs.SetString($"rechargeTime_{index}", nextRecharge.ToBinary().ToString());

            PlayerPrefs.Save();
            Debug.Log($"⚡ Energy used. Remaining: {currentPower}");
        }
        else
        {
            Debug.Log("⚠️ No energy left!");
        }
    }
}
//working