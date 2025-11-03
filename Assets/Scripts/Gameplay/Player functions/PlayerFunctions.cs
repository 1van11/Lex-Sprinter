using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerFunctions : MonoBehaviour
{
    [Header("Audio Sounds")]    
    public AudioSource audioSource;
    public AudioClip coinSound;
    public AudioClip hurtSound;
    public AudioClip correctAnswerSound;
    public AudioClip wrongAnswerSound;
    
    [Header("Score")]
    public int score = 0;
    public TMP_Text scoreText;
    
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

    [Header("Buffs")]
    public bool hasShield = false;
    public bool hasMagnet = false;
    public bool isSlowTime = false;
    
    [Header("Buff Durations")]
    public float shieldDuration = 8f;
    public float magnetDuration = 6f;
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

    [Header("Answer Feedback")]
    public GameObject correctAnswerPrefab;
    public GameObject wrongAnswerPrefab;
    public float feedbackDisplayTime = 1.5f;

    private Renderer[] renderers;
    [HideInInspector] public bool isDead = false;

    // Reference to PlayerControls
    private PlayerControls playerControls;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        playerControls = GetComponent<PlayerControls>();

        lastPosition = transform.position;

        // Initialize health
        currentHealth = maxHealth;
        UpdateHealthUI();

        // Load total coins from PlayerPrefs
        LoadTotalCoins();

        // Update total coins UI on start
        UpdateTotalCoinsUI();

        // Hide buff visuals and game over panel at start
        if (shieldVisual != null) shieldVisual.SetActive(false);
        if (magnetVisual != null) magnetVisual.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;

        // Track distance
        distanceTraveled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        if (distanceText != null)
            distanceText.text = $"Distance: {Mathf.FloorToInt(distanceTraveled)} m";

        // Speed increase logic
        if (playerControls != null)
        {
            float bonus = 1f;
            for (int i = 300; i <= distanceTraveled; i += 300)
            {
                bonus *= 1.125f;
                if (bonus >= 1.8f)
                    break;
            }
            playerControls.SetForwardSpeed(10f * bonus);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // Coin collection
        if (other.CompareTag("Coin"))
        {
            score += 1;
            totalCoins += 1; // Also add to total coins
            UpdateScoreUI();
            UpdateTotalCoinsUI(); // Update total coins display

            if (audioSource != null && coinSound != null)
                audioSource.PlayOneShot(coinSound);

            other.gameObject.SetActive(false);
            Debug.Log("💰 Coin collected!");
            Debug.Log($"💰 Total Coins: {totalCoins}");
            return;
        }

        if (other.CompareTag("Trap") && !isInvincible && !hasShield)
        {
            TakeDamage(1);

            if (audioSource != null && hurtSound != null)
                audioSource.PlayOneShot(hurtSound);
        }

        // LETTER HURDLE
        if (other.CompareTag("LetterHurdle"))
        {
            Debug.Log("🔤 Hit a Letter Hurdle!");

            if (!isInvincible && !hasShield)
            {
                TakeDamage(1);

                if (audioSource != null && hurtSound != null)
                    audioSource.PlayOneShot(hurtSound);
            }
            else if (hasShield)
            {
                Debug.Log("🛡️ Shield protected you from the hurdle!");
            }

            // Disable or destroy the hurdle if you want:
            other.gameObject.SetActive(false);
        }

        // Answer options - REMOVED the isInvincible check so shield doesn't block answers
        if (other.CompareTag("AnswerOptions"))
        {
            QuestionRandomizer questionRandomizer = other.GetComponentInParent<QuestionRandomizer>();

            if (questionRandomizer != null)
            {
                bool isJumpOption = other.gameObject.name.Contains("Jump");
                string selectedAnswer = isJumpOption ? questionRandomizer.jumpText.text : questionRandomizer.slideText.text;

                bool isCorrect = (selectedAnswer == questionRandomizer.correctAnswer);

                if (isCorrect)
                {
                    Debug.Log($"✅ Correct Answer! (+5 points) [{selectedAnswer}]");
                    score += 5;
                    totalCoins += 5; // Also add to total coins
                    UpdateScoreUI();
                    UpdateTotalCoinsUI(); // Update total coins display

                    if (audioSource != null && correctAnswerSound != null)
                        audioSource.PlayOneShot(correctAnswerSound);

                    ReplaceWithFeedbackModel(other.gameObject, correctAnswerPrefab);
                }
                else
                {
                    Debug.Log($"❌ Wrong Answer! [{selectedAnswer}] - Correct was: {questionRandomizer.correctAnswer}");

                    if (audioSource != null && wrongAnswerSound != null)
                        audioSource.PlayOneShot(wrongAnswerSound);

                    ReplaceWithFeedbackModel(other.gameObject, wrongAnswerPrefab);

                    if (!hasShield)
                        TakeDamage(1);
                    else
                        Debug.Log("🛡️ Shield protected you from wrong answer!");
                }

                // Show correct answer if player chose wrong
                if (!isCorrect)
                {
                    GameObject otherOption = null;
                    Transform parent = other.transform.parent;

                    if (parent != null)
                    {
                        foreach (Transform child in parent)
                        {
                            if (child.CompareTag("AnswerOptions") && child.gameObject != other.gameObject)
                            {
                                otherOption = child.gameObject;
                                break;
                            }
                        }
                    }

                    if (otherOption != null)
                        ReplaceWithFeedbackModel(otherOption, correctAnswerPrefab);
                }

                // Remove colliders and destroy question after delay
                if (other.transform.parent != null)
                {
                    Collider[] colliders = other.transform.parent.GetComponentsInChildren<Collider>();
                    foreach (Collider col in colliders)
                        Destroy(col);

                    StartCoroutine(DestroyAfterDelay(other.transform.parent.gameObject, feedbackDisplayTime));
                }
            }
        }

        // SHIELD
        if (other.CompareTag("Shield"))
        {
            StartCoroutine(ShieldBuff());
            Destroy(other.gameObject);
            Debug.Log("🛡️ Shield activated!");
        }

        // MAGNET
        if (other.CompareTag("Magnet"))
        {
            StartCoroutine(MagnetBuff());
            Destroy(other.gameObject);
            Debug.Log("🧲 Magnet activated!");
        }

        // SLOW TIME
        if (other.CompareTag("SlowTime"))
        {
            StartCoroutine(SlowTimeBuff());
            Destroy(other.gameObject);
            Debug.Log("⏰ Slow Time activated!");
        }
    }

    void TakeDamage(int damage)
    {
        if (isDead || hasShield) return; // Shield prevents damage

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

    void UpdateHealthUI()
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
        Debug.Log("💀 Game Over!");

        // Save total coins when game ends
        SaveTotalCoins();

        // Stop movement
        if (playerControls != null)
            playerControls.StopMovement();

        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
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
        if (number < 1000)
        {
            return number.ToString();
        }
        else if (number < 1000000) 
        {
            float thousands = number / 1000f;
            if (number % 1000 == 0)
                return $"{thousands:F0}k";
            else
                return $"{thousands:F1}k".Replace(".0", "");
        }
        else if (number < 1000000000)
        {
            float millions = number / 1000000f;
            if (number % 1000000 == 0)
                return $"{millions:F0}M";
            else
                return $"{millions:F1}M".Replace(".0", "");
        }
        else
        {
            float billions = number / 1000000000f;
            if (number % 1000000000 == 0)
                return $"{billions:F0}B";
            else
                return $"{billions:F1}B".Replace(".0", "");
        }
    }

    public string FormatCoins(int coins)
    {
        return FormatNumber(coins);
    }

    public string GetFormattedCoins()
    {
        return FormatCoins(totalCoins);
    }

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
        UpdateTotalCoinsUI(); // Update total coins display
        Debug.Log($"💰 Added {amount} coins. Total: {totalCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            UpdateTotalCoinsUI(); // Update total coins display
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

    public int GetTotalCoins()
    {
        return totalCoins;
    }

    public void ResetTotalCoins()
    {
        totalCoins = 0;
        PlayerPrefs.SetInt(coinSaveKey, 0);
        PlayerPrefs.Save();
        UpdateTotalCoinsUI(); // Update total coins display
        Debug.Log("💰 Total coins reset to 0");
    }

    public void SetTotalCoins(int amount)
    {
        totalCoins = Mathf.Max(0, amount);
        PlayerPrefs.SetInt(coinSaveKey, totalCoins);
        PlayerPrefs.Save();
        UpdateTotalCoinsUI(); // Update total coins display
        Debug.Log($"💰 Total coins set to: {totalCoins}");
    }

    public IEnumerator TriggerIFrames(float duration, float flashInterval)
    {
        isInvincible = true;
        float timer = 0f;

        while (timer < duration)
        {
            foreach (Renderer r in renderers)
            {
                r.enabled = !r.enabled;
            }

            timer += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        isInvincible = false;
    }

    IEnumerator ShieldBuff()
    {
        hasShield = true;
        // REMOVED: isInvincible = true; - Shield should not block answer interactions
        
        if (shieldVisual != null)
            shieldVisual.SetActive(true);

        Debug.Log($"🛡️ Shield active for {shieldDuration} seconds");
        yield return new WaitForSeconds(shieldDuration);

        if (shieldVisual != null)
            shieldVisual.SetActive(false);
        
        hasShield = false;
        // REMOVED: isInvincible = false;
        Debug.Log("🛡️ Shield expired");
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
            GameObject[] allCoins = GameObject.FindGameObjectsWithTag("Coin");
            
            foreach (GameObject coinObj in allCoins)
            {
                if (coinObj == null || !coinObj.activeInHierarchy)
                    continue;
                
                float distance = Vector3.Distance(transform.position, coinObj.transform.position);
                if (distance <= magnetRadius)
                {
                    coinObj.transform.position = Vector3.MoveTowards(
                        coinObj.transform.position,
                        transform.position,
                        magnetPullSpeed * Time.deltaTime
                    );
                }
            }

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

    void ReplaceWithFeedbackModel(GameObject answerOption, GameObject feedbackPrefab)
    {
        if (feedbackPrefab == null) return;

        GameObject feedback = Instantiate(feedbackPrefab, answerOption.transform.position, feedbackPrefab.transform.rotation);
    }

    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }

    void OnApplicationQuit()
    {
#if UNITY_ANDROID || UNITY_IOS
        SaveTotalCoins();
#else
        totalCoins = 0;
        PlayerPrefs.DeleteKey(coinSaveKey);
        PlayerPrefs.Save();
        UpdateTotalCoinsUI(); // Update total coins display
        Debug.Log("🧹 Unity Editor stopped — coins reset to 0.");
#endif
    }

    void OnApplicationPause(bool pauseStatus)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (pauseStatus)
        {
            SaveTotalCoins();
        }
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