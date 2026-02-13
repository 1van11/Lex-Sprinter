using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject OtherThingsCanvas;
    [SerializeField] TMP_Text countdownText;
    [SerializeField] float slowMotionTimescale = 0.1f;
    [SerializeField] float slowMotionDuration = 3f;

    [Header("Revive System")]
    public GameObject playerObject;
    public GameObject revivePanel;
    public GameObject[] uiToDisable;
    public TMP_Text revivePriceText;
    private int revivePrice = 250;

    private PlayerFunctions playerFunctions;
    private bool isResuming = false;
    private Coroutine resumeCoroutine;
    private float originalFixedDeltaTime;
    
    // Caching components
    private Animator[] allAnimators;
    private ParticleSystem[] allParticleSystems;
    private AudioSource[] allAudioSources;

    void Start()
    {
        if (playerObject != null)
            playerFunctions = playerObject.GetComponent<PlayerFunctions>();
        else
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

        UpdateRevivePriceUI();
    }

    void CacheAllAnimationComponents()
    {
        allAnimators = FindObjectsOfType<Animator>(true);
        allParticleSystems = FindObjectsOfType<ParticleSystem>(true);
        allAudioSources = FindObjectsOfType<AudioSource>(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale > slowMotionTimescale && !isResuming)
                Pause();
        }
    }

    #region Pause/Resume Logic
    public void Pause()
    {
        if (resumeCoroutine != null)
        {
            StopCoroutine(resumeCoroutine);
            resumeCoroutine = null;
            isResuming = false;
        }

        pauseMenu.SetActive(true);
        if (OtherThingsCanvas != null) OtherThingsCanvas.SetActive(false);
        
        Time.timeScale = 0;

        PauseAllAnimations();
        PauseAllParticleSystems();
        PauseAllAudio();

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
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
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
        while (timer > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.CeilToInt(timer).ToString();
                StartCoroutine(ScaleCountdownNumber(countdownText.transform));
            }
            yield return new WaitForSecondsRealtime(1f);
            timer -= 1f;
        }

        if (countdownText != null)
        {
            countdownText.text = "go";
            StartCoroutine(ScaleCountdownNumber(countdownText.transform, 1.8f));
        }

        yield return new WaitForSecondsRealtime(1f);

        if (countdownText != null) countdownText.gameObject.SetActive(false);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        RestoreAllAnimationsToNormal();
        RestoreAllParticleSystemsToNormal();
        RestoreAllAudioToNormal();

        pauseMenu.SetActive(false);
        if (OtherThingsCanvas != null) OtherThingsCanvas.SetActive(true);

        isResuming = false;
        resumeCoroutine = null;
    }
    #endregion

    #region Revive System
    public void ShowRevivePanel()
    {
        if (revivePanel != null)
        {
            revivePanel.SetActive(true);
            Time.timeScale = 0;

            foreach (GameObject ui in uiToDisable)
                if (ui != null) ui.SetActive(false);

            PauseAllAnimations();
            PauseAllParticleSystems();
            PauseAllAudio();
        }
        UpdateRevivePriceUI();
    }

    public void RevivePlayer()
    {
        if (playerFunctions != null && playerFunctions.SpendCoins(revivePrice))
        {
            ReviveImmediate();
        }
    }

    private void ReviveImmediate()
    {
        if (revivePanel != null) revivePanel.SetActive(false);

        foreach (GameObject ui in uiToDisable)
            if (ui != null) ui.SetActive(true);

        playerFunctions.ReviveFromDeath();

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        RestoreAllAnimationsToNormal();
        RestoreAllParticleSystemsToNormal();
        RestoreAllAudioToNormal();
        ResumeAllAnimations();
        ResumeAllParticleSystems();
        ResumeAllAudio();

        revivePrice = Mathf.RoundToInt(revivePrice * 1.5f);
        UpdateRevivePriceUI();
    }

    public void CancelRevive()
    {
        if (revivePanel != null) revivePanel.SetActive(false);
        foreach (GameObject ui in uiToDisable)
            if (ui != null) ui.SetActive(true);

        Time.timeScale = 1f;
        ResumeAllAnimations();
        ResumeAllParticleSystems();
        ResumeAllAudio();
    }

    private void UpdateRevivePriceUI()
    {
        if (revivePriceText != null) revivePriceText.text = revivePrice.ToString();
    }
    #endregion

    #region Scene Transitions (The Fix)
    // Use this to reset time scale without triggering UI errors
    private void PrepareForSceneChange()
    {
        if (resumeCoroutine != null) StopCoroutine(resumeCoroutine);
        isResuming = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }

    public void Home()
    {
        PrepareForSceneChange();
        SaveCoins();
        SceneManager.LoadScene("HomeScreen");
    }

    public void Restart()
    {
        PrepareForSceneChange();
        SaveCoins();
        DeductEnergyFromHome();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    #region Internal Helpers
    private IEnumerator ScaleCountdownNumber(Transform target, float targetScale = 1.5f)
    {
        Vector3 originalScale = Vector3.one; 
        Vector3 goalScale = originalScale * targetScale;
        float t = 0f;
        while (t < 1f) {
            t += Time.unscaledDeltaTime * 5f;
            target.localScale = Vector3.Lerp(originalScale, goalScale, t);
            yield return null;
        }
        t = 0f;
        while (t < 1f) {
            t += Time.unscaledDeltaTime * 5f;
            target.localScale = Vector3.Lerp(goalScale, originalScale, t);
            yield return null;
        }
    }

    private void SaveCoins() {
        if (playerFunctions != null) playerFunctions.SaveTotalCoins();
    }

    private void DeductEnergyFromHome() {
        int currentPower = PlayerPrefs.GetInt("currentPower", 5);
        if (currentPower > 0) {
            currentPower--;
            PlayerPrefs.SetInt("currentPower", currentPower);
            PlayerPrefs.Save();
        }
    }

    void OnDestroy() {
        // Ensure time is normal if this script is destroyed
        Time.timeScale = 1f;
    }
    #endregion

    #region Component Controls (Wrapped for Safety)
    void PauseAllAnimations() { foreach (var a in allAnimators) if (a) a.speed = 0; }
    void ResumeAllAnimations() { foreach (var a in allAnimators) if (a) a.speed = 1; }
    void SetAllAnimationsSpeed(float s) { foreach (var a in allAnimators) if (a) a.speed = s; }
    void RestoreAllAnimationsToNormal() { foreach (var a in allAnimators) if (a) a.speed = 1; }
    
    void PauseAllParticleSystems() { foreach (var p in allParticleSystems) if (p) p.Pause(); }
    void ResumeAllParticleSystems() { foreach (var p in allParticleSystems) if (p) p.Play(); }
    void SetAllParticleSystemSpeed(float s) { foreach (var p in allParticleSystems) { if (p) { var m = p.main; m.simulationSpeed = s; } } }
    void RestoreAllParticleSystemsToNormal() { foreach (var p in allParticleSystems) { if (p) { var m = p.main; m.simulationSpeed = 1; } } }

    void PauseAllAudio() { foreach (var a in allAudioSources) if (a && a.isPlaying) a.Pause(); }
    void ResumeAllAudio() { foreach (var a in allAudioSources) if (a) a.UnPause(); }
    void SetAllAudioPitch(float p) { foreach (var a in allAudioSources) if (a) a.pitch = p; }
    void RestoreAllAudioToNormal() { foreach (var a in allAudioSources) if (a) a.pitch = 1; }
    #endregion
}