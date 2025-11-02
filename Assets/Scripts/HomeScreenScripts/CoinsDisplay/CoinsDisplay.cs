using UnityEngine;
using TMPro;
using System.Collections;

public class CoinsDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text coinText; // Assign your TextMeshProUGUI here

    [Header("Coin Settings")]
    public string coinSaveKey = "PlayerTotalCoins"; // Must match PlayerFunctions
    public float animationDuration = 1.2f; // How long the number animates
    public bool animateOnStart = true;

    private int totalCoins;

    void Start()
    {
        LoadCoins();
        if (animateOnStart)
            StartCoroutine(AnimateCoinCount(0, totalCoins));
        else
            UpdateCoinUI(totalCoins);
    }

    /// <summary>
    /// Loads saved total coins from PlayerPrefs.
    /// </summary>
    void LoadCoins()
    {
        totalCoins = PlayerPrefs.GetInt(coinSaveKey, 0);
        Debug.Log($"💰 Main Menu Loaded Coins: {totalCoins}");
    }

    /// <summary>
    /// Instantly updates the UI to show the given amount.
    /// </summary>
    void UpdateCoinUI(int value)
    {
        if (coinText != null)
            coinText.text = FormatNumber(value);
    }

    /// <summary>
    /// Refreshes the coin count and animates from the old to the new value.
    /// </summary>
    public void RefreshCoinDisplay()
    {
        int oldCoins = totalCoins;
        LoadCoins();
        StopAllCoroutines();
        StartCoroutine(AnimateCoinCount(oldCoins, totalCoins));
    }

    /// <summary>
    /// Smoothly animates the coin counter from start to end.
    /// </summary>
    IEnumerator AnimateCoinCount(int startValue, int endValue)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            UpdateCoinUI(currentValue);
            yield return null;
        }

        UpdateCoinUI(endValue);
    }

    /// <summary>
    /// Formats numbers like 1000 → 1k, 1500 → 1.5k, 1000000 → 1M, etc.
    /// </summary>
    string FormatNumber(int number)
    {
        if (number < 1000)
            return number.ToString();
        else if (number < 1000000)
        {
            float thousands = number / 1000f;
            return $"{thousands:F1}k".Replace(".0", "");
        }
        else if (number < 1000000000)
        {
            float millions = number / 1000000f;
            return $"{millions:F1}M".Replace(".0", "");
        }
        else
        {
            float billions = number / 1000000000f;
            return $"{billions:F1}B".Replace(".0", "");
        }
    }
}
