using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject OtherThingsCanvas;
    [SerializeField] TMP_Text countdownText;
    [SerializeField] float slowMotionTimescale = 0.1f;
    [SerializeField] float slowMotionDuration = 3f;

    [Header("Revive Panel Settings")]
    [SerializeField] GameObject revivePanel;
    [SerializeField] GameObject[] uiToDisable;       // UI to hide when revive panel is active
    [SerializeField] TMP_Text revivePriceText;
    [SerializeField] int revivePrice = 250;

    [Header("Game Over Panel (must be assigned!)")]
    [SerializeField] GameObject gameOverPanel;        // <-- DRAG YOUR GAME OVER PANEL HERE

    private PlayerFunctions playerFunctions;
    private bool isResuming = false;
    private Coroutine resumeCoroutine;
    private float originalFixedDeltaTime;

    private Animator[] allAnimators;
    private ParticleSystem[] allParticleSystems;
    private AudioSource[] allAudioSources;

    void Awake()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Start()
    {
        if (playerFunctions == null)
            playerFunctions = FindObjectOfType<PlayerFunctions>();

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
        // Block escape when revive panel is active
        if (revivePanel != null && revivePanel.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale > slowMotionTimescale)
                Pause();
        }
    }

    // -------------------- PAUSE MENU METHODS --------------------
    public void Pause()
    {
        StopAllSlowMotion(); // kill any ongoing resume coroutine

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
            resumeCoroutine = StartCoroutine(SlowMotionResumeCoroutine(false)); // false = from pause menu
        }
    }

    // Unified slow‑motion resume coroutine
    private IEnumerator SlowMotionResumeCoroutine(bool fromRevive)
    {
        isResuming = true;

        // If called from pause menu, hide the pause menu UI
        if (!fromRevive)
        {
            CanvasGroup pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
            if (pauseCanvasGroup == null)
                pauseCanvasGroup = pauseMenu.AddComponent<CanvasGroup>();
            pauseCanvasGroup.alpha = 0;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }

        // Show countdown text
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.color = new Color(countdownText.color.r, countdownText.color.g, countdownText.color.b, 1f);
        }

        // Enter slow motion
        Time.timeScale = slowMotionTimescale;
        Time.fixedDeltaTime = originalFixedDeltaTime * slowMotionTimescale;

        SetAllAnimationsSpeed(slowMotionTimescale);
        SetAllParticleSystemSpeed(slowMotionTimescale);
        SetAllAudioPitch(slowMotionTimescale);

        ResumeAllAnimations();
        ResumeAllParticleSystems();
        ResumeAllAudio();

        float timer = slowMotionDuration;

        // Countdown 3,2,1
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

        // Show "go"
        if (countdownText != null)
        {
            countdownText.text = "go";
            StartCoroutine(ScaleCountdownNumber(countdownText.transform, 1.8f));
        }
        yield return new WaitForSecondsRealtime(1f);

        // Hide countdown
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        // Restore normal speed
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        RestoreAllAnimationsToNormal();
        RestoreAllParticleSystemsToNormal();
        RestoreAllAudioToNormal();

        // If called from pause menu, close pause menu and show gameplay canvas
        if (!fromRevive)
        {
            pauseMenu.SetActive(false);
            OtherThingsCanvas.SetActive(true);

            CanvasGroup pauseCanvasGroup = pauseMenu.GetComponent<CanvasGroup>();
            if (pauseCanvasGroup != null)
            {
                pauseCanvasGroup.alpha = 1f;
                pauseCanvasGroup.interactable = true;
                pauseCanvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            // From revive: just ensure gameplay canvas is visible
            OtherThingsCanvas.SetActive(true);
        }

        isResuming = false;
        resumeCoroutine = null;
    }

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

    public bool IsInSlowMotion() => isResuming && Time.timeScale == slowMotionTimescale;

    // Cancel any active slow‑motion and reset to normal
    private void StopAllSlowMotion()
    {
        if (resumeCoroutine != null)
            StopCoroutine(resumeCoroutine);
        resumeCoroutine = null;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        isResuming = false;

        RestoreAllAnimationsToNormal();
        RestoreAllParticleSystemsToNormal();
        RestoreAllAudioToNormal();

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    // -------------------- SCENE MANAGEMENT (Home / Restart) --------------------
    public void Home()
    {
        // Force‑hide ALL UI panels that might be open
        ForceHideAllPanels();

        StopAllSlowMotion();
        SaveCoins();
        Time.timeScale = 1f;
        SceneManager.LoadScene("HomeScreen");
    }

    public void Restart()
    {
        // Force‑hide ALL UI panels that might be open
        ForceHideAllPanels();

        StopAllSlowMotion();
        SaveCoins();
        DeductEnergyFromHome();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Ensures every possible UI panel is closed before scene change
    private void ForceHideAllPanels()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (revivePanel != null) revivePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (OtherThingsCanvas != null) OtherThingsCanvas.SetActive(true); // gameplay UI back on
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
            Debug.LogWarning("⚠️ PlayerFunctions not found!");
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
            Debug.Log("⚠️ No energy left!");
    }

    // -------------------- REVIVE METHODS --------------------
    public void ShowRevivePanel()
    {
        if (revivePanel == null)
        {
            Debug.LogError("Revive panel not assigned in PauseMenu!");
            return;
        }

        if (playerFunctions == null)
            playerFunctions = FindObjectOfType<PlayerFunctions>();

        if (pauseMenu.activeSelf)
            pauseMenu.SetActive(false);

        revivePanel.SetActive(true);
        Time.timeScale = 0;

        foreach (GameObject ui in uiToDisable)
            if (ui != null)
                ui.SetActive(false);

        UpdateRevivePriceUI();
    }

    public void RevivePlayer()
    {
        if (playerFunctions == null)
        {
            Debug.LogWarning("Cannot revive: PlayerFunctions reference missing!");
            return;
        }

        if (!playerFunctions.SpendCoins(revivePrice))
        {
            Debug.Log("❌ Not enough coins to revive!");
            return;
        }

        // Hide revive panel
        if (revivePanel != null)
            revivePanel.SetActive(false);

        // Re-enable normal UI
        foreach (GameObject ui in uiToDisable)
            if (ui != null)
                ui.SetActive(true);

        // Revive the player (this should restore health and enable movement)
        playerFunctions.ReviveFromDeath();

        // Increase revive price for next time
        revivePrice = Mathf.RoundToInt(revivePrice * 1.5f);
        UpdateRevivePriceUI();

        // --- SLOW‑MOTION RESUME AFTER REVIVE (exactly like pause menu) ---
        StopAllSlowMotion(); // prevent any leftover coroutine
        resumeCoroutine = StartCoroutine(SlowMotionResumeCoroutine(true)); // true = from revive

        Debug.Log("🔄 Revived! New price: " + revivePrice);
    }

    public void CancelRevive()
    {
        if (revivePanel != null)
            revivePanel.SetActive(false);

        foreach (GameObject ui in uiToDisable)
            if (ui != null)
                ui.SetActive(true);

        // Resume normally (no slow‑motion)
        Time.timeScale = 1;

        Debug.Log("❌ Revive canceled, game resumed.");
    }

    private void UpdateRevivePriceUI()
    {
        if (revivePriceText != null)
            revivePriceText.text = revivePrice.ToString();
    }

    // -------------------- ANIMATION / AUDIO / PARTICLE HELPERS --------------------
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
}