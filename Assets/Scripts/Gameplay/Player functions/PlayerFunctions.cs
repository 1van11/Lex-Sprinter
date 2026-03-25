using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerFunctions : MonoBehaviour
{
    [Header("Forward Movement")]
    public float forwardSpeed = 10f;
    public float speedIncreaseMultiplier = 1.125f;
    public float maxSpeedMultiplier = 1.8f;
    public float speedIncreaseDistance = 300f;

    [Header("Debug / Cheat Options")]
    public bool alwaysInvincible = false;

    [Header("Player Model Reference")]
    [Tooltip("Auto-detects GameObject with 'Player' tag. Can be manually assigned if needed.")]
    public Transform playerModel;

    [Header("Audio Sounds")]
    public AudioSource audioSource;
    public AudioClip coinSound;
    public AudioClip hurtSound;
    public AudioClip correctAnswerSound;
    public AudioClip wrongAnswerSound;

    [Header("Score")]
    public int score = 0;
    public TMP_Text scoreText;
    public TMP_Text wordCountText;
    private int wordsCollected = 0;

    [Header("Total Coins (Persistent)")]
    public int totalCoins = 0;
    public string coinSaveKey = "PlayerTotalCoins";
    public TMP_Text totalCoinsText;

    [Header("Distance")]
    public float distanceTraveled = 0f;
    private Vector3 lastPosition;
    public TMP_Text distanceText;

    [Header("Health System")]
    public int maxHealth = 5;
    public int currentHealth = 5;
    public TMP_Text healthText;
    public UnityEngine.UI.Image[] healthImages;
    public Sprite fullHealthSprite;
    public Sprite emptyHealthSprite;

    [Header("IFrames")]
    public float iFrameDuration = 1f;
    public float flashInterval = 0.1f;
    [HideInInspector] public bool isInvincible = false;

    #region Buffs variables
    [Header("Buffs")]
    public bool hasShield = false;
    public bool hasMagnet = false;
    public bool isSlowTime = false;
    #endregion

    [Header("Buff Durations")]
    public float shieldDuration = 8f;
    public float magnetDuration = 6f;
    public int shieldMaxHits = 3;
    [HideInInspector] public int shieldHitsRemaining;
    public float slowTimeDuration = 4f;

    [Header("Magnet Settings")]
    public float magnetRadius = 7f;
    public float magnetPullSpeed = 10f;
    public LayerMask coinLayer;

    [Header("Buff Visual Feedback (Optional)")]
    public GameObject shieldVisual;
    public GameObject magnetVisual;

    [Header("Magnet Pet Settings")]
    public GameObject magnetPetPrefab;
    public float petFollowSpeed = 5f;
    public Vector3 petOffset = new Vector3(0, 0, -2f);
    public Vector3 petScale = Vector3.one;
    public Vector3 petRotation = Vector3.zero;
    // Enhanced jumping animation
    public float petJumpHeight = 1.5f;      // How high the pet hops (increased for visibility)
    public float petJumpSpeed = 10f;        // How fast it hops (faster for snappier motion)
    public float petWobbleAngle = 15f;      // Optional: slight rotation while jumping (set to 0 to disable)

    [Header("Magnet End Warning")]
    public float magnetEndIFrameDuration = 1.5f;   // Duration of warning flash before magnet expires
    public float magnetEndFlashInterval = 0.1f;    // Flash speed during warning

    [Header("Game Over")]
    public GameObject gameOverPanel;

    [Header("Revive Panel")]
    public GameObject revivePanel;

    [Header("Answer Feedback")]
    public GameObject correctAnswerPrefab;
    public GameObject wrongAnswerPrefab;
    public float feedbackDisplayTime = 1.5f;

    private Queue<GameObject> correctPool = new Queue<GameObject>();
    private Queue<GameObject> wrongPool = new Queue<GameObject>();

    private Renderer[] renderers;
    private Renderer[] modelRenderers;

    [HideInInspector] public bool isDead = false;

    private PlayerControls playerControls;

    // For magnet end warning
    private Coroutine petWarningCoroutine;   // only track pet flash now
    private GameObject activePet;

    void Start()
    {
        UpdateWordCountUI();
        AutoDetectPlayerModel();

        renderers = GetComponentsInChildren<Renderer>();
        if (playerModel != null)
        {
            modelRenderers = playerModel.GetComponentsInChildren<Renderer>();
            Debug.Log($"✅ Found {modelRenderers.Length} renderers on player model: {playerModel.name}");
        }
        else
        {
            modelRenderers = GetComponentsInChildren<Renderer>();
            Debug.LogWarning("⚠️ Player model not found. Using all child renderers.");
        }

        playerControls = GetComponent<PlayerControls>();

        if (playerModel != null)
            lastPosition = playerModel.position;
        else
            lastPosition = transform.position;

        currentHealth = maxHealth;
        UpdateHealthUI();

        LoadTotalCoins();
        UpdateTotalCoinsUI();

        if (shieldVisual != null) shieldVisual.SetActive(false);
        if (magnetVisual != null) magnetVisual.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (revivePanel != null) revivePanel.SetActive(false);

        if (playerControls != null)
            playerControls.SetForwardSpeed(forwardSpeed);
    }

    private void AutoDetectPlayerModel()
    {
        if (playerModel != null)
        {
            Debug.Log($"✅ Player model already assigned: {playerModel.name}");
            return;
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null && taggedPlayer != this.gameObject)
        {
            playerModel = taggedPlayer.transform;
            Debug.Log($"✅ Found player model via tag 'Player': {playerModel.name}");
            return;
        }

        foreach (Transform child in transform)
        {
            if (child.CompareTag("Player"))
            {
                playerModel = child;
                Debug.Log($"✅ Found player model (child with 'Player' tag): {child.name}");
                return;
            }
        }

        Renderer[] childRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in childRenderers)
        {
            if (rend.transform != transform)
            {
                playerModel = rend.transform;
                Debug.Log($"✅ Found player model (first child with renderer): {playerModel.name}");
                return;
            }
        }

        Renderer myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
        {
            playerModel = transform;
            Debug.Log($"✅ Using self as player model (has renderer)");
            return;
        }

        if (gameObject.CompareTag("Player") || GetComponent<PlayerControls>() != null)
        {
            playerModel = transform;
            Debug.Log($"✅ Using self as player model (Player controller)");
            return;
        }

        Debug.LogWarning("❌ Could not auto-detect player model! Please assign manually in Inspector.");
    }

    void Update()
    {
        if (isDead) return;

        Vector3 forwardMove = new Vector3(0, 0, forwardSpeed * Time.deltaTime);
        transform.position += forwardMove;

        Vector3 currentPosition;
        if (playerModel != null)
            currentPosition = playerModel.position;
        else
            currentPosition = transform.position;

        distanceTraveled += Vector3.Distance(currentPosition, lastPosition);
        lastPosition = currentPosition;

        if (distanceText != null)
            distanceText.text = $"Distance: {Mathf.FloorToInt(distanceTraveled)} m";

        UpdateSpeedBasedOnDistance();

        if (hasMagnet && Time.timeScale > 0)
        {
            GameObject[] allCoins = GameObject.FindGameObjectsWithTag("Coin");
            foreach (GameObject coinObj in allCoins)
            {
                if (coinObj == null || !coinObj.activeInHierarchy) continue;
                float distance = Vector3.Distance(transform.position, coinObj.transform.position);
                if (distance <= magnetRadius)
                    coinObj.transform.position = Vector3.MoveTowards(
                        coinObj.transform.position,
                        transform.position,
                        magnetPullSpeed * Time.deltaTime
                    );
            }
        }
    }

    private void UpdateSpeedBasedOnDistance()
    {
        float bonus = 1f;
        int intervals = Mathf.FloorToInt(distanceTraveled / speedIncreaseDistance);

        for (int i = 1; i <= intervals; i++)
        {
            bonus *= speedIncreaseMultiplier;
            if (bonus >= maxSpeedMultiplier)
            {
                bonus = maxSpeedMultiplier;
                break;
            }
        }

        forwardSpeed = 10f * bonus;

        if (playerControls != null)
            playerControls.SetForwardSpeed(forwardSpeed);
    }

    #region OnTriggerEnter
    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // =========================
        // COIN COLLECTION
        // =========================
        if (other.CompareTag("Coin"))
        {
            score += 1;
            totalCoins += 1;
            UpdateScoreUI();
            UpdateTotalCoinsUI();

            if (audioSource != null && coinSound != null)
                audioSource.PlayOneShot(coinSound);

            other.gameObject.SetActive(false);
            Debug.Log("💰 Coin collected!");
            return;
        }

        // =========================
        // TRAP COLLISION
        // =========================
        if (other.CompareTag("Trap") && !isInvincible && !alwaysInvincible)
        {
            TakeDamage(1);
            if (audioSource != null && hurtSound != null)
                audioSource.PlayOneShot(hurtSound);
        }

        // =========================
        // LETTER HURDLE
        // =========================
        if (other.CompareTag("LetterHurdle"))
        {
            Debug.Log("🔤 Hit a Letter Hurdle!");

            // 🔹 I‑FRAMES: ignore completely (no damage, no deactivation)
            if (!alwaysInvincible && isInvincible)
            {
                Debug.Log("🛡️ I‑frames active – ignoring letter hurdle");
                return;
            }

            if (alwaysInvincible || isInvincible)
            {
                Debug.Log("🛡️ No damage taken");
            }
            else
            {
                TakeDamage(1);
                if (audioSource != null && hurtSound != null)
                    audioSource.PlayOneShot(hurtSound);
            }

            other.gameObject.SetActive(false);
        }

        // =========================
        // ANSWER OPTIONS
        // =========================
        if (other.CompareTag("AnswerOptions"))
        {
            // 🔹 I‑FRAMES: show feedback but skip everything else (damage, word unlock, score)
            bool skipEffects = !alwaysInvincible && isInvincible;

            QuestionRandomizer questionRandomizer =
                other.GetComponentInParent<QuestionRandomizer>();

            if (questionRandomizer != null)
            {
                // Determine the correct answer
                string correctAnswer = questionRandomizer.correctAnswer;

                // Collect all answer option GameObjects under the same parent
                List<GameObject> answerOptions = new List<GameObject>();
                foreach (Transform child in other.transform.parent)
                {
                    if (child.CompareTag("AnswerOptions"))
                        answerOptions.Add(child.gameObject);
                }

                // For each answer option, spawn feedback based on whether it's correct
                foreach (GameObject opt in answerOptions)
                {
                    TMP_Text txt = opt.GetComponentInChildren<TMP_Text>();
                    if (txt == null) continue;

                    bool isCorrect = txt.text == correctAnswer;
                    GameObject prefab = isCorrect ? correctAnswerPrefab : wrongAnswerPrefab;
                    ReplaceWithFeedbackModel(opt, prefab);
                }

                // If we are NOT in i‑frames, process normal gameplay effects
                if (!skipEffects)
                {
                    // Find which option the player actually hit
                    TMP_Text answerText = other.GetComponentInChildren<TMP_Text>();
                    string selectedAnswer = answerText != null ? answerText.text : "";

                    if (string.IsNullOrEmpty(selectedAnswer))
                    {
                        // Fallback for old naming
                        if (other.gameObject.name.Contains("Jump"))
                            selectedAnswer = questionRandomizer.jumpText.text;
                        else if (other.gameObject.name.Contains("Slide"))
                            selectedAnswer = questionRandomizer.slideText.text;
                        else if (other.gameObject.name.Contains("Option3"))
                            selectedAnswer = questionRandomizer.option3Text.text;
                    }

                    bool isCorrectSelected = selectedAnswer == correctAnswer;

                    if (isCorrectSelected)
                    {
                        // ✅ CORRECT ANSWER – give rewards
                        Debug.Log($"✅ Correct Answer! [{selectedAnswer}]");

                        score += 5;
                        totalCoins += 5;
                        UpdateScoreUI();
                        UpdateTotalCoinsUI();

                        if (audioSource != null && correctAnswerSound != null)
                            audioSource.PlayOneShot(correctAnswerSound);

                        string correctWord = correctAnswer.ToLower();
                        AddUnlockedWord(correctWord);

                        wordsCollected++;
                        UpdateWordCountUI();

                        int totalWords = PlayerPrefs.GetInt("TotalWordCount", 0);
                        totalWords++;
                        PlayerPrefs.SetInt("TotalWordCount", totalWords);
                        PlayerPrefs.Save();

                        if (DailyTaskManager.Instance != null)
                            DailyTaskManager.Instance.CheckAndCompleteTask(correctWord);
                    }
                    else
                    {
                        // ❌ WRONG ANSWER – play sound and take damage
                        Debug.Log($"❌ Wrong Answer! [{selectedAnswer}] | Correct: {correctAnswer}");

                        if (audioSource != null && wrongAnswerSound != null)
                            audioSource.PlayOneShot(wrongAnswerSound);

                        // Damage only if not invincible (i‑frames already caught above, but double‑check)
                        if (!alwaysInvincible && !isInvincible)
                            TakeDamage(1);
                    }
                }

                // Hide clue UI (always do this, even during i‑frames)
                if (questionRandomizer.clueTextObject != null)
                    questionRandomizer.clueTextObject.SetActive(false);

                if (questionRandomizer.clueImageObject != null)
                    questionRandomizer.clueImageObject.SetActive(false);

                // Clean up the question object after a delay
                if (other.transform.parent != null)
                {
                    Collider[] colliders = other.transform.parent.GetComponentsInChildren<Collider>();
                    foreach (Collider col in colliders)
                        Destroy(col);

                    StartCoroutine(DestroyAfterDelay(other.transform.parent.gameObject, feedbackDisplayTime));
                }
            }
        }

        // =========================
        // SHIELD PICKUP (now health)
        // =========================
        if (other.CompareTag("Shield"))
        {
            currentHealth = Mathf.Min(currentHealth + 1, maxHealth);
            UpdateHealthUI();

            if (audioSource != null && coinSound != null)
                audioSource.PlayOneShot(coinSound);

            Destroy(other.gameObject);
            Debug.Log($"❤️ Health pickup! Current health: {currentHealth}/{maxHealth}");
        }

        // =========================
        // MAGNET PICKUP
        // =========================
        if (other.CompareTag("Magnet"))
        {
            StartCoroutine(MagnetBuff());
            Destroy(other.gameObject);
            Debug.Log("🧲 Magnet activated! Pet summoned to follow player!");
        }

        // =========================
        // SLOW TIME PICKUP
        // =========================
        if (other.CompareTag("SlowTime"))
        {
            StartCoroutine(SlowTimeBuff());
            Destroy(other.gameObject);
            Debug.Log("⏰ Slow Time activated!");
        }
    }
    #endregion

public void IncrementWordCount()
{
    wordsCollected++;
    UpdateWordCountUI();
    Debug.Log($"📝 Word collected! Total: {wordsCollected}");
}

    void UpdateWordCountUI()
    {
        if (wordCountText != null)
            wordCountText.text = $"Words: {wordsCollected}";
    }

    private void AddUnlockedWord(string word)
    {
        string existingWords = PlayerPrefs.GetString("NewlyUnlockedWords", "");
        if (!string.IsNullOrEmpty(existingWords))
        {
            string[] words = existingWords.Split(',');
            foreach (string w in words)
            {
                if (w.Trim().ToLower() == word.ToLower())
                {
                    Debug.Log($"⚠️ Word '{word}' already unlocked, skipping.");
                    return;
                }
            }
            existingWords += "," + word;
        }
        else
        {
            existingWords = word;
        }
        PlayerPrefs.SetString("NewlyUnlockedWords", existingWords);
        PlayerPrefs.Save();
        Debug.Log($"💾 Saved to NewlyUnlockedWords: {existingWords}");
    }

    public void SaveAllUnlockedWordsForDictionary()
    {
        PlayerPrefs.Save();
        Debug.Log("📚 All unlocked words saved for Dictionary");
    }

public void TakeDamage(int damage)
{
    if (isDead || alwaysInvincible) return;

    DebugIFrameStatus();

    if (hasShield && shieldHitsRemaining > 0)
    {
        shieldHitsRemaining -= damage;
        Debug.Log($"🛡️ Shield absorbed damage! Remaining hits: {shieldHitsRemaining}/{shieldMaxHits}");

        if (shieldHitsRemaining <= 0)
        {
            hasShield = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            Debug.Log("🛡️ Shield fully depleted and deactivated");
        }
        return;
    }

    currentHealth -= damage;
    currentHealth = Mathf.Max(0, currentHealth);
    Debug.Log($"💔 Health: {currentHealth}/{maxHealth}");
    UpdateHealthUI();

    if (currentHealth <= 0)
    {
        Die();
    }
    else
    {
        StartCoroutine(TriggerIFrames(iFrameDuration, flashInterval));
    }
}

    public void TakeDamageFromWrongLetter()
    {
        if (isDead || alwaysInvincible) return;

        // 🔹 I‑FRAMES: ignore wrong letter damage completely
        if (isInvincible)
        {
            Debug.Log("🛡️ I‑frames active – ignoring wrong letter damage");
            return;
        }

        if (hasShield && shieldHitsRemaining > 0)
        {
            shieldHitsRemaining--;
            Debug.Log($"🛡️ Shield blocked wrong letter! Remaining: {shieldHitsRemaining}/{shieldMaxHits}");

            if (shieldHitsRemaining <= 0)
            {
                hasShield = false;
                if (shieldVisual != null) shieldVisual.SetActive(false);
                Debug.Log("🛡️ Shield depleted from wrong letter");
            }
            return;
        }

        TakeDamage(1);
        if (audioSource != null && hurtSound != null)
            audioSource.PlayOneShot(hurtSound);
        Debug.Log("❌ Wrong letter! Took damage.");
    }

    public void EnableInvincibility()
    {
        alwaysInvincible = true;
        Debug.Log("🛡️ Player is now always invincible!");
    }

    public void DisableInvincibility()
    {
        alwaysInvincible = false;
        Debug.Log("❌ Player invincibility disabled.");
    }

    public void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = $"❤️ {currentHealth}";

        if (healthImages != null && healthImages.Length > 0)
        {
            for (int i = 0; i < healthImages.Length; i++)
            {
                if (healthImages[i] != null)
                {
                    if (i < currentHealth)
                    {
                        healthImages[i].sprite = fullHealthSprite;
                        healthImages[i].enabled = true;
                    }
                    else
                    {
                        if (emptyHealthSprite != null)
                        {
                            healthImages[i].sprite = emptyHealthSprite;
                            healthImages[i].enabled = true;
                        }
                        else
                        {
                            healthImages[i].enabled = false;
                        }
                    }
                }
            }
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("💀 Player died!");

        Time.timeScale = 0f;

        PlayerPrefs.SetInt("LatestWordCount", wordsCollected);
        PlayerPrefs.Save();
        SaveTotalCoins();

        if (playerControls != null)
            playerControls.StopMovement();

        if (revivePanel != null)
        {
            revivePanel.SetActive(true);
            Debug.Log("🔄 Revive panel activated");
        }
        else
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
        }
    }

    public void ReviveFromDeath()
    {
        isDead = false;

        currentHealth = maxHealth;
        UpdateHealthUI();

        isInvincible = false;
        hasShield = false;
        shieldHitsRemaining = 0;
        hasMagnet = false;
        isSlowTime = false;

        if (shieldVisual != null)
            shieldVisual.SetActive(false);
        if (magnetVisual != null)
            magnetVisual.SetActive(false);

        if (playerControls != null)
            playerControls.ResumeMovement();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Debug.Log("💖 Player revived successfully!");
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = FormatNumber(score);
    }

    void UpdateTotalCoinsUI()
    {
        if (totalCoinsText != null)
            totalCoinsText.text = $" {FormatCoins(totalCoins)}";
    }

    public string FormatNumber(int number)
    {
        if (number < 1000) return number.ToString();
        else if (number < 1000000)
        {
            float thousands = number / 1000f;
            return (number % 1000 == 0) ? $"{thousands:F0}k" : $"{thousands:F1}k".Replace(".0", "");
        }
        else if (number < 1000000000)
        {
            float millions = number / 1000000f;
            return (number % 1000000 == 0) ? $"{millions:F0}M" : $"{millions:F1}M".Replace(".0", "");
        }
        else
        {
            float billions = number / 1000000000f;
            return (number % 1000000000 == 0) ? $"{billions:F0}B" : $"{billions:F1}B".Replace(".0", "");
        }
    }

    public string FormatCoins(int coins) => FormatNumber(coins);

    public void LoadTotalCoins()
    {
        totalCoins = PlayerPrefs.GetInt(coinSaveKey, 0);
        Debug.Log($"💰 Loaded total coins: {totalCoins}");
    }

    public void SaveTotalCoins()
    {
#if UNITY_ANDROID || UNITY_IOS
        PlayerPrefs.SetInt(coinSaveKey, totalCoins);
        PlayerPrefs.Save();
        Debug.Log($"📱💾 Saved total coins (mobile build): {totalCoins}");
#else
        Debug.Log("🧩 Running in Unity Editor — skipping save to PlayerPrefs.");
#endif
    }

    public void AddCoins(int amount)
    {
        score += amount;
        totalCoins += amount;
        UpdateScoreUI();
        UpdateTotalCoinsUI();
        Debug.Log($"💰 Added {amount} coins. Total: {totalCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            UpdateTotalCoinsUI();
            SaveTotalCoins();
            Debug.Log($"💰 Spent {amount} coins. Remaining: {totalCoins}");
            return true;
        }
        else
        {
            Debug.LogWarning($"💰 Not enough coins! Need {amount}, but only have {totalCoins}");
            return false;
        }
    }

    public int GetTotalCoins() => totalCoins;

    public void ResetTotalCoins()
    {
        totalCoins = 0;
        PlayerPrefs.SetInt(coinSaveKey, 0);
        PlayerPrefs.Save();
        UpdateTotalCoinsUI();
        Debug.Log("💰 Total coins reset to 0");
    }

    public void SetTotalCoins(int amount)
    {
        totalCoins = Mathf.Max(0, amount);
        PlayerPrefs.SetInt(coinSaveKey, totalCoins);
        PlayerPrefs.Save();
        UpdateTotalCoinsUI();
        Debug.Log($"💰 Total coins set to: {totalCoins}");
    }

    #region iFrames
    public IEnumerator TriggerIFrames(float duration, float flashInterval)
    {
        isInvincible = true;
        float timer = 0f;
        bool flashState = true;

        Dictionary<Renderer, bool> initialStates = new Dictionary<Renderer, bool>();

        List<Renderer> allRenderers = new List<Renderer>();

        if (modelRenderers != null)
        {
            foreach (Renderer r in modelRenderers)
            {
                if (r != null)
                {
                    allRenderers.Add(r);
                    initialStates[r] = r.enabled;
                }
            }
        }

        foreach (Renderer r in renderers)
        {
            if (r != null && !allRenderers.Contains(r))
            {
                allRenderers.Add(r);
                initialStates[r] = r.enabled;
            }
        }

        while (timer < duration)
        {
            foreach (Renderer r in allRenderers)
            {
                if (r != null)
                    r.enabled = flashState;
            }

            yield return new WaitForSeconds(flashInterval);

            flashState = !flashState;
            timer += flashInterval;
        }

        foreach (Renderer r in allRenderers)
        {
            if (r != null && initialStates.ContainsKey(r))
                r.enabled = initialStates[r];
        }

        isInvincible = false;
        Debug.Log("🛡️ iFrames ended");
    }

    // Visual-only flash (no damage immunity) – used for magnet end warning
    public IEnumerator VisualFlash(float duration, float flashInterval)
    {
        float timer = 0f;
        bool flashState = true;

        List<Renderer> allRenderers = new List<Renderer>();

        if (modelRenderers != null)
        {
            foreach (Renderer r in modelRenderers)
                if (r != null) allRenderers.Add(r);
        }

        foreach (Renderer r in renderers)
            if (r != null && !allRenderers.Contains(r)) allRenderers.Add(r);

        // Store initial states, but we'll restore at the end
        Dictionary<Renderer, bool> initialStates = new Dictionary<Renderer, bool>();
        foreach (Renderer r in allRenderers)
            if (r != null) initialStates[r] = r.enabled;

        while (timer < duration)
        {
            foreach (Renderer r in allRenderers)
                if (r != null) r.enabled = flashState;

            yield return new WaitForSeconds(flashInterval);
            flashState = !flashState;
            timer += flashInterval;
        }

        // Restore initial states
        foreach (Renderer r in allRenderers)
            if (r != null && initialStates.ContainsKey(r))
                r.enabled = initialStates[r];
    }

    // Visual flash for a specific GameObject (e.g., the pet)
    public IEnumerator VisualFlashForObject(GameObject obj, float duration, float flashInterval)
    {
        if (obj == null) yield break;
        Renderer[] objRenderers = obj.GetComponentsInChildren<Renderer>();
        if (objRenderers.Length == 0) yield break;

        float timer = 0f;
        bool flashState = true;

        // Store initial states
        Dictionary<Renderer, bool> initialStates = new Dictionary<Renderer, bool>();
        foreach (Renderer r in objRenderers)
            if (r != null) initialStates[r] = r.enabled;

        while (timer < duration)
        {
            foreach (Renderer r in objRenderers)
                if (r != null) r.enabled = flashState;
            yield return new WaitForSeconds(flashInterval);
            flashState = !flashState;
            timer += flashInterval;
        }

        // Restore initial states
        foreach (Renderer r in objRenderers)
            if (r != null && initialStates.ContainsKey(r))
                r.enabled = initialStates[r];
    }
    #endregion

    #region power ups
    IEnumerator ShieldBuff()
    {
        hasShield = true;
        shieldHitsRemaining = shieldMaxHits;
        if (shieldVisual != null)
            shieldVisual.SetActive(true);

        Debug.Log($"🛡️ Shield activated! Absorbs {shieldMaxHits} hits for up to {shieldDuration} seconds");

        float timer = shieldDuration;
        while (timer > 0 && hasShield)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        if (hasShield)
        {
            hasShield = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            Debug.Log("🛡️ Shield expired (time out)");
        }
    }

    IEnumerator MagnetBuff()
    {
        hasMagnet = true;

        // Summon pet
        if (magnetPetPrefab != null && activePet == null)
        {
            Vector3 spawnPosition = transform.position + petOffset;
            activePet = Instantiate(magnetPetPrefab, spawnPosition, Quaternion.identity);

            activePet.transform.localScale = petScale;
            activePet.transform.rotation = Quaternion.Euler(petRotation);

            Debug.Log($"🐕 Pet summoned: {activePet.name} | Scale: {petScale} | Rotation: {petRotation}");
        }

        if (magnetVisual != null)
            magnetVisual.SetActive(true);

        Debug.Log($"🧲 Magnet active for {magnetDuration} seconds - Player attracts coins! Pet follows & jumps!");

        float timer = magnetDuration;
        bool warningStarted = false;

        while (timer > 0 && hasMagnet)
        {
            // ----- Pet jumping & following -----
            if (activePet != null)
            {
                // Base follow position
                Vector3 targetPosition = transform.position + petOffset;

                // Add hopping using sine wave (faster + higher)
                float yOffset = Mathf.Sin(Time.time * petJumpSpeed) * petJumpHeight;
                targetPosition.y += yOffset;

                // Optional rotation wobble for extra flair
                if (petWobbleAngle > 0)
                {
                    float zRot = Mathf.Sin(Time.time * petJumpSpeed * 2f) * petWobbleAngle;
                    activePet.transform.rotation = Quaternion.Euler(petRotation.x, petRotation.y, petRotation.z + zRot);
                }
                else
                {
                    activePet.transform.rotation = Quaternion.Euler(petRotation);
                }

                activePet.transform.position = Vector3.Lerp(
                    activePet.transform.position,
                    targetPosition,
                    petFollowSpeed * Time.deltaTime
                );
            }

            // ----- Magnet end warning (flash ONLY the pet) -----
            if (!warningStarted && timer <= magnetEndIFrameDuration)
            {
                warningStarted = true;
                // Flash pet (player does NOT flash)
                if (activePet != null)
                {
                    petWarningCoroutine = StartCoroutine(VisualFlashForObject(activePet, magnetEndIFrameDuration, magnetEndFlashInterval));
                }
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        // Clean up pet and warning
        if (activePet != null)
        {
            Destroy(activePet);
            activePet = null;
            Debug.Log("🐕 Pet despawned");
        }

        if (magnetVisual != null)
            magnetVisual.SetActive(false);

        hasMagnet = false;
        Debug.Log("🧲 Magnet expired");

        // Stop pet flash and restore its renderers
        if (petWarningCoroutine != null)
        {
            StopCoroutine(petWarningCoroutine);
            petWarningCoroutine = null;
            if (activePet != null) // pet might still exist if we stopped early (unlikely, but safe)
            {
                Renderer[] petRenderers = activePet.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in petRenderers) if (r != null) r.enabled = true;
            }
        }
    }

    IEnumerator SlowTimeBuff()
    {
        isSlowTime = true;
        Time.timeScale = 0.5f;
        Debug.Log($"⏰ Slow Time active for {slowTimeDuration} seconds (real time)");

        yield return new WaitForSecondsRealtime(slowTimeDuration);

        Time.timeScale = 1f;
        isSlowTime = false;
        Debug.Log("⏰ Slow Time expired");
    }
    #endregion

    private GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefab);
    }

    private void ReturnToPool(GameObject obj, Queue<GameObject> pool)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    void ReplaceWithFeedbackModel(GameObject answerOption, GameObject feedbackPrefab)
    {
        if (feedbackPrefab == null) return;

        Queue<GameObject> pool = feedbackPrefab == correctAnswerPrefab ? correctPool : wrongPool;

        GameObject feedback = GetFromPool(pool, feedbackPrefab);
        feedback.transform.position = answerOption.transform.position;
        feedback.transform.rotation = feedbackPrefab.transform.rotation;

        StartCoroutine(ReturnFeedbackToPool(feedback, pool, feedbackDisplayTime));
    }

    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }

    IEnumerator ReturnFeedbackToPool(GameObject obj, Queue<GameObject> pool, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(obj, pool);
    }

    void OnApplicationQuit()
    {
#if UNITY_ANDROID || UNITY_IOS
        SaveTotalCoins();
#else
        totalCoins = 0;
        PlayerPrefs.DeleteKey(coinSaveKey);
        PlayerPrefs.Save();
        UpdateTotalCoinsUI();
        Debug.Log("🧹 Unity Editor stopped — coins reset to 0.");
#endif
    }

    void OnApplicationPause(bool pauseStatus)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (pauseStatus) SaveTotalCoins();
#endif
    }

    void OnDrawGizmos()
    {
        if (hasMagnet)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, magnetRadius);
        }
    }

    public void SetPlayerModel(Transform newModel)
    {
        playerModel = newModel;
        if (playerModel != null)
        {
            modelRenderers = playerModel.GetComponentsInChildren<Renderer>();
            Debug.Log($"✅ Player model manually set to: {playerModel.name}");
        }
    }

    public float GetForwardSpeed() => forwardSpeed;

    public void SetForwardSpeed(float speed)
    {
        forwardSpeed = speed;
        Debug.Log($"⚡ Speed set to: {forwardSpeed}");
    }

    public void StopMovement()
    {
        forwardSpeed = 0f;
        if (playerControls != null)
            playerControls.SetForwardSpeed(0f);
        Debug.Log("🛑 Movement stopped");
    }

    public void ResumeMovement(float baseSpeed = 10f)
    {
        forwardSpeed = baseSpeed;
        if (playerControls != null)
            playerControls.SetForwardSpeed(forwardSpeed);
        Debug.Log($"▶️ Movement resumed at speed: {forwardSpeed}");
    }

    public void DebugIFrameStatus()
    {
        Debug.Log($"iFrame Status:");
        Debug.Log($"- isInvincible: {isInvincible}");
        Debug.Log($"- alwaysInvincible: {alwaysInvincible}");
        Debug.Log($"- hasShield: {hasShield}");
        Debug.Log($"- shieldHitsRemaining: {shieldHitsRemaining}");
        Debug.Log($"- modelRenderers count: {(modelRenderers != null ? modelRenderers.Length : 0)}");

        if (modelRenderers != null)
        {
            for (int i = 0; i < Mathf.Min(3, modelRenderers.Length); i++)
            {
                if (modelRenderers[i] != null)
                    Debug.Log($"  Renderer {i}: {modelRenderers[i].name} enabled={modelRenderers[i].enabled}");
            }
        }
    }
}

#if UNITY_EDITOR
public static class PlayerFunctionsEditor
{
    [UnityEditor.InitializeOnLoadMethod]
    static void ClearCoinsOnEditorPlayStop()
    {
        UnityEditor.EditorApplication.playModeStateChanged += (state) =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                PlayerPrefs.DeleteKey("PlayerTotalCoins");
                PlayerPrefs.Save();
                Debug.Log("🧹 Editor exiting Play Mode — coins cleared to 0.");
            }
        };
    }
}
#endif