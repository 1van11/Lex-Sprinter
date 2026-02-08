using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using TMPro;
using UnityEngine.Animations;

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
        
        // Cache all animators, particle systems, and audio sources in the scene
        CacheAllAnimationComponents();
        
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
            countdownText.raycastTarget = false; // Ensure numbers are not tappable
        }

        // Ensure pause menu starts fully visible and interactable
        CanvasGroup pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null)
            pauseCanvasGroup = pauseMenu.AddComponent<CanvasGroup>();

        pauseCanvasGroup.alpha = 1f;
        pauseCanvasGroup.interactable = true;
        pauseCanvasGroup.blocksRaycasts = true;
    }

    void CacheAllAnimationComponents()
    {
        // Find all animators in the scene
        allAnimators = FindObjectsOfType<Animator>(true); // Include inactive
        allParticleSystems = FindObjectsOfType<ParticleSystem>(true);
        allAudioSources = FindObjectsOfType<AudioSource>(true);
        
        Debug.Log($"📊 Cached {allAnimators.Length} animators, {allParticleSystems.Length} particle systems, {allAudioSources.Length} audio sources");
    }

    void Update()
    {
        // Optional: Escape key to pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale > slowMotionTimescale) // Not already in slow motion
                Pause();
        }
    }

    public void Pause()
    {
        // Stop any ongoing resume coroutine
        if (resumeCoroutine != null)
        {
            StopCoroutine(resumeCoroutine);
            resumeCoroutine = null;
            isResuming = false;
            Time.timeScale = 0; // Force back to paused state
            Time.fixedDeltaTime = originalFixedDeltaTime;
            RestoreAllAnimationsToNormal(); // Make sure animations are paused
        }

        pauseMenu.SetActive(true);
        OtherThingsCanvas.SetActive(false);
        Time.timeScale = 0;

        // Pause all animations manually
        PauseAllAnimations();
        PauseAllParticleSystems();
        PauseAllAudio();

        // Ensure pause menu is fully visible and tappable when paused
        CanvasGroup pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }

        // Hide countdown text when pausing
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

        // 1️⃣ Hide pause menu buttons but keep the menu visible for countdown
        CanvasGroup pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null)
            pauseCanvasGroup = pauseMenu.AddComponent<CanvasGroup>();

        // Make pause menu semi-transparent & untappable
        pauseCanvasGroup.alpha = 0;
        pauseCanvasGroup.interactable = false; 
        pauseCanvasGroup.blocksRaycasts = false;

        // 2️⃣ Show countdown text on the pause menu
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = Mathf.CeilToInt(slowMotionDuration).ToString();
            countdownText.color = new Color(countdownText.color.r, countdownText.color.g, countdownText.color.b, 1f);
        }

        // 3️⃣ Start ultra slow motion
        Time.timeScale = slowMotionTimescale;
        Time.fixedDeltaTime = originalFixedDeltaTime * slowMotionTimescale;
        
        Debug.Log($"🚀 Starting ultra slow motion: Time.timeScale = {Time.timeScale}");

        // MANUALLY control all animations for slow motion
        SetAllAnimationsSpeed(slowMotionTimescale);
        SetAllParticleSystemSpeed(slowMotionTimescale);
        SetAllAudioPitch(slowMotionTimescale);
        
        // Resume all animations and particles
        ResumeAllAnimations();
        ResumeAllParticleSystems();
        ResumeAllAudio();

        // 4️⃣ Countdown in REAL TIME (unaffected by Time.timeScale)
        float timer = slowMotionDuration;

        while (timer > 0)
        {
            // Update countdown text
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(timer).ToString();

            // Wait in REAL TIME (using unscaled time - NOT affected by Time.timeScale)
            yield return new WaitForSecondsRealtime(1f);
            
            timer -= 1f;
        }

        // 5️⃣ Hide countdown text
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        // 6️⃣ Restore normal time scale
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        Debug.Log($"✅ Restored normal time: Time.timeScale = {Time.timeScale}");

        // Restore all animations to normal speed
        RestoreAllAnimationsToNormal();
        RestoreAllParticleSystemsToNormal();
        RestoreAllAudioToNormal();

        // 7️⃣ Hide pause menu completely and re-enable game UI
        pauseMenu.SetActive(false);
        OtherThingsCanvas.SetActive(true);

        // 8️⃣ Reset menu state for next time
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }

        isResuming = false;
        resumeCoroutine = null;
    }

    #region Animation Control Methods

    void PauseAllAnimations()
    {
        foreach (var animator in allAnimators)
        {
            if (animator != null && animator.isActiveAndEnabled)
            {
                animator.speed = 0f; // Pause animation
            }
        }
    }

    void ResumeAllAnimations()
    {
        foreach (var animator in allAnimators)
        {
            if (animator != null && animator.isActiveAndEnabled)
            {
                animator.speed = 1f; // Resume at normal speed initially
            }
        }
    }

    void SetAllAnimationsSpeed(float speed)
    {
        foreach (var animator in allAnimators)
        {
            if (animator != null && animator.isActiveAndEnabled)
            {
                animator.speed = speed; // Set custom speed
            }
        }
    }

    void RestoreAllAnimationsToNormal()
    {
        foreach (var animator in allAnimators)
        {
            if (animator != null && animator.isActiveAndEnabled)
            {
                animator.speed = 1f; // Restore to normal speed
            }
        }
    }

    void PauseAllParticleSystems()
    {
        foreach (var ps in allParticleSystems)
        {
            if (ps != null && ps.isPlaying)
            {
                ps.Pause();
            }
        }
    }

    void ResumeAllParticleSystems()
    {
        foreach (var ps in allParticleSystems)
        {
            if (ps != null)
            {
                ps.Play();
            }
        }
    }

    void SetAllParticleSystemSpeed(float speed)
    {
        foreach (var ps in allParticleSystems)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.simulationSpeed = speed;
            }
        }
    }

    void RestoreAllParticleSystemsToNormal()
    {
        foreach (var ps in allParticleSystems)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.simulationSpeed = 1f;
            }
        }
    }

    void PauseAllAudio()
    {
        foreach (var audioSource in allAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }

    void ResumeAllAudio()
    {
        foreach (var audioSource in allAudioSources)
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.UnPause();
            }
        }
    }

    void SetAllAudioPitch(float pitch)
    {
        foreach (var audioSource in allAudioSources)
        {
            if (audioSource != null)
            {
                audioSource.pitch = pitch;
            }
        }
    }

    void RestoreAllAudioToNormal()
    {
        foreach (var audioSource in allAudioSources)
        {
            if (audioSource != null)
            {
                audioSource.pitch = 1f;
            }
        }
    }

    #endregion

    // Method to check if we're currently in slow motion
    public bool IsInSlowMotion()
    {
        return isResuming && Time.timeScale == slowMotionTimescale;
    }

    // Method to manually cancel slow motion (for emergencies)
    public void CancelSlowMotion()
    {
        if (resumeCoroutine != null)
        {
            StopCoroutine(resumeCoroutine);
            resumeCoroutine = null;
        }
        
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        isResuming = false;
        
        // Restore all animations to normal
        RestoreAllAnimationsToNormal();
        RestoreAllParticleSystemsToNormal();
        RestoreAllAudioToNormal();
        
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
            
        pauseMenu.SetActive(false);
        OtherThingsCanvas.SetActive(true);
    }

    public void Home()
    {
        // Stop slow motion if resuming
        CancelSlowMotion();
        SaveCoins();
        Time.timeScale = 1;
        SceneManager.LoadScene("HomeScreen");
    }

    public void Restart()
    {
        // Stop slow motion if resuming
        CancelSlowMotion();
        SaveCoins();
        DeductEnergyFromHome();
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Clean up when destroyed
    void OnDestroy()
    {
        // Always restore time scale when this object is destroyed
        if (Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
        }
        
        // Restore animations just in case
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

    #region DeductEnergy
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
    #endregion
}
//super slow on resume
