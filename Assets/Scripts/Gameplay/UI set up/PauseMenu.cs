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
    [SerializeField] float freezeFrameDuration = 3f; // Now used as freeze duration

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
    private CanvasGroup pauseCanvasGroup; // Cache the CanvasGroup

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

        // Get or add CanvasGroup and store reference
        pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null)
            pauseCanvasGroup = pauseMenu.AddComponent<CanvasGroup>();

        // Ensure initial state is visible
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
            if (Time.timeScale > 0 && !isResuming) // Changed condition since we're using freeze (0 timeScale)
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

        // Reset canvas group to ensure visibility and interaction
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }

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
            resumeCoroutine = StartCoroutine(ResumeWithFreezeFrame());
        }
    }

    private IEnumerator ResumeWithFreezeFrame()
    {
        isResuming = true;

        // Hide pause menu but keep game frozen
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }

        // Show countdown text
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        // Keep timeScale at 0 (completely frozen) during countdown
        Time.timeScale = 0f;
        Time.fixedDeltaTime = originalFixedDeltaTime * 0f;

        // Ensure all animations and particles remain paused
        PauseAllAnimations();
        PauseAllParticleSystems();
        PauseAllAudio();

        // Countdown with frozen game
        float timer = freezeFrameDuration;
        while (timer > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.CeilToInt(timer).ToString();
                // Start the scale animation but it will be frozen - we'll handle it with unscaled time
                StartCoroutine(ScaleCountdownNumber(countdownText.transform));
            }
            
            // Wait for 1 second in real time (unscaled)
            float waitStartTime = Time.unscaledTime;
            while (Time.unscaledTime - waitStartTime < 1f)
            {
                yield return null; // Wait using unscaled time
            }
            
            timer -= 1f;
        }

        // Show "GO!" text
        if (countdownText != null)
        {
            countdownText.text = "go!";
            StartCoroutine(ScaleCountdownNumber(countdownText.transform, 1.8f));
        }

        // Brief pause before resuming
        yield return new WaitForSecondsRealtime(0.3f);

        // Hide countdown text
        if (countdownText != null) 
            countdownText.gameObject.SetActive(false);

        // Resume the game
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        // Resume all animations and effects
        ResumeAllAnimations();
        ResumeAllParticleSystems();
        ResumeAllAudio();

        // Hide pause menu and show other UI
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
        if (target == null) yield break;
        
        Vector3 originalScale = target.localScale;          // store the actual starting scale
        Vector3 goalScale = originalScale * targetScale;    // scale up from that
        float t = 0f;

        // Scale up - using unscaledDeltaTime since time is frozen
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 5f;
            target.localScale = Vector3.Lerp(originalScale, goalScale, t);
            yield return null;
        }
        target.localScale = goalScale;   // ensure exact final up-scale

        t = 0f;

        // Scale back down - using unscaledDeltaTime since time is frozen
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 5f;
            target.localScale = Vector3.Lerp(goalScale, originalScale, t);
            yield return null;
        }
        target.localScale = originalScale;   // restore exactly
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
        Time.fixedDeltaTime = originalFixedDeltaTime;
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