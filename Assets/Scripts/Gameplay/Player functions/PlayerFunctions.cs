using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerFunctions : MonoBehaviour
{
    [Header("Forward Movement")]
    public float forwardSpeed = 10f;
    public float speedIncreaseMultiplier = 1.125f;
    public float maxSpeedMultiplier = 1.8f;
    public float speedIncreaseDistance = 300f; // Distance interval for speed increase
    
    [Header("Debug / Cheat Options")]
    public bool alwaysInvincible = false; // toggle in Inspector or via code

    [Header("Player Model Reference")]
    [Tooltip("Auto-detects GameObject with 'Player' tag. Can be manually assigned if needed.")]
    public Transform playerModel; // Reference to the actual 3D model (drag in Inspector)
    
    [Header("Audio Sounds")]
    public AudioSource audioSource;
    public AudioClip coinSound;
    public AudioClip hurtSound;
    public AudioClip correctAnswerSound;
    public AudioClip wrongAnswerSound;

    [Header("Score")]
    public int score = 0;
    public TMP_Text scoreText;
    public TMP_Text wordCountText; // Drag a TextMeshPro UI element here in Inspector
    private int wordsCollected = 0;
    [Header("Total Coins (Persistent)")]
    public int totalCoins = 0;
    public string coinSaveKey = "PlayerTotalCoins";
    public TMP_Text totalCoinsText; // Added TextMeshPro for total coins display

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
    public int shieldMaxHits = 3; // 👈 EDIT THIS - how many hits the shield can absorb
    [HideInInspector] public int shieldHitsRemaining;
    public float slowTimeDuration = 4f;

    [Header("Magnet Settings")]
    public float magnetRadius = 7f;
    public float magnetPullSpeed = 10f;
    public LayerMask coinLayer;

    [Header("Buff Visual Feedback (Optional)")]
    public GameObject shieldVisual;
    public GameObject magnetVisual;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    [Header("Revive Panel")]
    public GameObject revivePanel; // NEW: Reference to revive panel

    [Header("Answer Feedback")]
    public GameObject correctAnswerPrefab;
    public GameObject wrongAnswerPrefab;
    public float feedbackDisplayTime = 1.5f;

    private Queue<GameObject> correctPool = new Queue<GameObject>();
    private Queue<GameObject> wrongPool = new Queue<GameObject>();

    private Renderer[] renderers;
    private Renderer[] modelRenderers; // Renderers from the actual 3D model

    [HideInInspector] public bool isDead = false;

    // Reference to PlayerControls
    private PlayerControls playerControls;

    void Start()
    {

    UpdateWordCountUI();
        // AUTO-DETECT PLAYER MODEL USING TAG
        AutoDetectPlayerModel();

        // Initialize renderers arrays
        renderers = GetComponentsInChildren<Renderer>();
        
        // Get renderers from the actual 3D model
        if (playerModel != null)
        {
            modelRenderers = playerModel.GetComponentsInChildren<Renderer>();
            Debug.Log($"✅ Found {modelRenderers.Length} renderers on player model: {playerModel.name}");
        }
        else
        {
            // Fallback to getting all renderers in children
            modelRenderers = GetComponentsInChildren<Renderer>();
            Debug.LogWarning("⚠️ Player model not found. Using all child renderers.");
        }

        playerControls = GetComponent<PlayerControls>();

        // Initialize lastPosition based on player model
        if (playerModel != null)
        {
            lastPosition = playerModel.position;
        }
        else
        {
            lastPosition = transform.position;
        }

        // Initialize health
        currentHealth = maxHealth;
        UpdateHealthUI();

        // Load total coins from PlayerPrefs
        LoadTotalCoins();

        // Update total coins UI on start
        UpdateTotalCoinsUI();

        // Hide buff visuals and panels at start
        if (shieldVisual != null) shieldVisual.SetActive(false);
        if (magnetVisual != null) magnetVisual.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (revivePanel != null) revivePanel.SetActive(false);
        
        // Initialize forward speed in PlayerControls
        if (playerControls != null)
            playerControls.SetForwardSpeed(forwardSpeed);
    }

    

    /// <summary>
    /// Auto-detects the player model by looking for GameObject with "Player" tag
    /// </summary>
    private void AutoDetectPlayerModel()
    {
        // If already assigned, use that
        if (playerModel != null)
        {
            Debug.Log($"✅ Player model already assigned: {playerModel.name}");
            return;
        }

        // Method 1: Look for GameObject with "Player" tag in the scene
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            // Check if this is the same GameObject (to avoid infinite loops)
            if (taggedPlayer != this.gameObject)
            {
                playerModel = taggedPlayer.transform;
                Debug.Log($"✅ Found player model via tag 'Player': {playerModel.name}");
                return;
            }
        }

        // Method 2: Look among children for a GameObject tagged as "Player"
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Player"))
            {
                playerModel = child;
                Debug.Log($"✅ Found player model (child with 'Player' tag): {child.name}");
                return;
            }
        }

        // Method 3: Look for any child with renderer
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

        // Method 4: If this GameObject has a renderer, use it
        Renderer myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
        {
            playerModel = transform;
            Debug.Log($"✅ Using self as player model (has renderer)");
            return;
        }

        // Last resort: Check if this is the player controller
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

        // Forward movement - moved to PlayerFunctions
        Vector3 forwardMove = new Vector3(0, 0, forwardSpeed * Time.deltaTime);
        transform.position += forwardMove;

        // Track distance using player model position
        Vector3 currentPosition;
        if (playerModel != null)
        {
            currentPosition = playerModel.position;
        }
        else
        {
            currentPosition = transform.position;
        }
        
        distanceTraveled += Vector3.Distance(currentPosition, lastPosition);
        lastPosition = currentPosition;

        if (distanceText != null)
            distanceText.text = $"Distance: {Mathf.FloorToInt(distanceTraveled)} m";

        // Speed increase logic
        UpdateSpeedBasedOnDistance();

        // Magnet effect during magnet buff
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

    /// <summary>
    /// Updates speed based on distance traveled
    /// </summary>
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
        
        // Update PlayerControls with the new speed
        if (playerControls != null)
            playerControls.SetForwardSpeed(forwardSpeed);
    }

#region OntriggerEnter
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
        QuestionRandomizer questionRandomizer = other.GetComponentInParent<QuestionRandomizer>();
        if (questionRandomizer != null)
        {
            bool isJumpOption = other.gameObject.name.Contains("Jump");
            string selectedAnswer = isJumpOption
                ? questionRandomizer.jumpText.text
                : questionRandomizer.slideText.text;

            bool isCorrect = selectedAnswer == questionRandomizer.correctAnswer;

            if (isCorrect)
            {
                // ✅ CORRECT ANSWER
                Debug.Log($"✅ Correct Answer! [{selectedAnswer}]");

                score += 5;
                totalCoins += 5;
                UpdateScoreUI();
                UpdateTotalCoinsUI();

                if (audioSource != null && correctAnswerSound != null)
                    audioSource.PlayOneShot(correctAnswerSound);

                ReplaceWithFeedbackModel(other.gameObject, correctAnswerPrefab);

                string correctWord = questionRandomizer.correctAnswer.ToLower();
                AddUnlockedWord(correctWord);

                wordsCollected++;
                UpdateWordCountUI();

                if (DailyTaskManager.Instance != null)
                    DailyTaskManager.Instance.CheckAndCompleteTask(correctWord);
            }
            else
            {
                // ❌ WRONG ANSWER
                Debug.Log($"❌ Wrong Answer! [{selectedAnswer}] | Correct: {questionRandomizer.correctAnswer}");

                if (audioSource != null && wrongAnswerSound != null)
                    audioSource.PlayOneShot(wrongAnswerSound);

                // ❌ Show wrong feedback on chosen option
                ReplaceWithFeedbackModel(other.gameObject, wrongAnswerPrefab);

                // ✅ ALSO show correct feedback on the correct option
                Transform parent = other.transform.parent;
                if (parent != null)
                {
                    foreach (Transform child in parent)
                    {
                        if (child == other.transform) continue;

                        TMP_Text txt = child.GetComponentInChildren<TMP_Text>();
                        if (txt != null && txt.text == questionRandomizer.correctAnswer)
                        {
                            ReplaceWithFeedbackModel(child.gameObject, correctAnswerPrefab);
                            break;
                        }
                    }
                }

                if (!alwaysInvincible)
                    TakeDamage(1);
            }

            // Remove colliders + destroy question after delay
            if (other.transform.parent != null)
            {
                Collider[] colliders = other.transform.parent.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                    Destroy(col);

                StartCoroutine(
                    DestroyAfterDelay(
                        other.transform.parent.gameObject,
                        feedbackDisplayTime
                    )
                );
            }
        }
    }

    // =========================
    // SHIELD PICKUP
    // =========================
    if (other.CompareTag("Shield"))
    {
        StartCoroutine(ShieldBuff());
        Destroy(other.gameObject);
        Debug.Log("🛡️ Shield activated!");
    }

    // =========================
    // MAGNET PICKUP
    // =========================
    if (other.CompareTag("Magnet"))
    {
        StartCoroutine(MagnetBuff());
        Destroy(other.gameObject);
        Debug.Log("🧲 Magnet activated!");
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

void UpdateWordCountUI()
{
    if (wordCountText != null)
        wordCountText.text = $"Words: {wordsCollected}";
}

    /// <summary>
    /// Adds a word to the unlocked words list in PlayerPrefs
    /// </summary>
    private void AddUnlockedWord(string word)
    {
        // Get existing unlocked words
        string existingWords = PlayerPrefs.GetString("NewlyUnlockedWords", "");
        // Check if word is already in the list
        if (!string.IsNullOrEmpty(existingWords))
        {
            string[] words = existingWords.Split(',');
            foreach (string w in words)
            {
                if (w.Trim().ToLower() == word.ToLower())
                {
                    Debug.Log($"⚠️ Word '{word}' already unlocked, skipping.");
                    return; // Word already exists, don't add again
                }
            }
            // Add to existing list
            existingWords += "," + word;
        }
        else
        {
            // First word
            existingWords = word;
        }
        PlayerPrefs.SetString("NewlyUnlockedWords", existingWords);
        PlayerPrefs.Save();
        Debug.Log($"💾 Saved to NewlyUnlockedWords: {existingWords}");
    }

    #region Save Words Unlocked
    public void SaveAllUnlockedWordsForDictionary()
    {
        // This is called when going back to HomeScreen
        // The WordUnlockManager will read "NewlyUnlockedWords" and unlock them all
        PlayerPrefs.Save();
        Debug.Log("📚 All unlocked words saved for Dictionary");
    }
    #endregion

    void TakeDamage(int damage)
    {
        if (isDead || alwaysInvincible) return;

        // Debug current state
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
            return; // Shield took the hit → no health loss
        }

        // No shield or shield depleted → take real damage
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

    #region Letter Hurdle
    public void TakeDamageFromWrongLetter()
    {
        if (isDead || alwaysInvincible) return;

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
    #endregion

    // Toggleable invincibility helpers
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

        // Pause the game as soon as death happens
        Time.timeScale = 0f;

        PlayerPrefs.SetFloat("LatestDistance", distanceTraveled);
        PlayerPrefs.Save();
        SaveTotalCoins();

        if (playerControls != null)
            playerControls.StopMovement();

        // Show revive panel if available
        if (revivePanel != null)
        {
            revivePanel.SetActive(true);
            Debug.Log("🔄 Revive panel activated");
        }
        else
        {
            // If no revive panel, show game over panel
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
        }
    }

    // NEW METHOD: Revive the player from death
    public void ReviveFromDeath()
    {
        // Reset death state
        isDead = false;

        // Restore health
        currentHealth = maxHealth;
        UpdateHealthUI();

        // Reset all buff states
        isInvincible = false;
        hasShield = false;
        shieldHitsRemaining = 0;
        hasMagnet = false;
        isSlowTime = false;

        // Hide buff visuals
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
        if (magnetVisual != null)
            magnetVisual.SetActive(false);

        // Re-enable player movement
        if (playerControls != null)
            playerControls.ResumeMovement();

        // Hide game over panel
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
        
        // Store initial renderer states
        Dictionary<Renderer, bool> initialStates = new Dictionary<Renderer, bool>();
        
        // Collect all renderers to flash
        List<Renderer> allRenderers = new List<Renderer>();
        
        // Add model renderers
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
        
        // Add other renderers
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
            // Toggle visibility of all renderers
            foreach (Renderer r in allRenderers)
            {
                if (r != null)
                    r.enabled = flashState;
            }
            
            // Wait for the flash interval
            yield return new WaitForSeconds(flashInterval);
            
            // Toggle flash state
            flashState = !flashState;
            timer += flashInterval;
        }
        
        // Restore all renderers to their original state
        foreach (Renderer r in allRenderers)
        {
            if (r != null && initialStates.ContainsKey(r))
                r.enabled = initialStates[r];
        }
        
        isInvincible = false;
        Debug.Log("🛡️ iFrames ended");
    }
    #endregion

    #region power ups
    IEnumerator ShieldBuff()
    {
        hasShield = true;
        shieldHitsRemaining = shieldMaxHits; // Reset hits when picking up shield
        if (shieldVisual != null)
            shieldVisual.SetActive(true);

        Debug.Log($"🛡️ Shield activated! Absorbs {shieldMaxHits} hits for up to {shieldDuration} seconds");

        float timer = shieldDuration;
        while (timer > 0 && hasShield)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // Time ran out → deactivate shield
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
        if (magnetVisual != null)
            magnetVisual.SetActive(true);
        Debug.Log($"🧲 Magnet active for {magnetDuration} seconds");

        float timer = magnetDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        if (magnetVisual != null)
            magnetVisual.SetActive(false);
        hasMagnet = false;
        Debug.Log("🧲 Magnet expired");
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

        Queue<GameObject> pool =
            feedbackPrefab == correctAnswerPrefab ? correctPool : wrongPool;

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

    // Public method to manually set player model (optional)
    public void SetPlayerModel(Transform newModel)
    {
        playerModel = newModel;
        if (playerModel != null)
        {
            modelRenderers = playerModel.GetComponentsInChildren<Renderer>();
            Debug.Log($"✅ Player model manually set to: {playerModel.name}");
        }
    }
    
    // Public methods for forward speed control
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

    // Debug Helper for iFrames
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

#if UNITY_EDITOR
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
#endif
}
//testing