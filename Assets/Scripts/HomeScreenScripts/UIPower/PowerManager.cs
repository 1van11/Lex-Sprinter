using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerManager : MonoBehaviour
{
    [Header("Letter Images (P, O, W, E, R order)")]
    public Image[] letterImages;

    [Header("Full Sprites for each letter")]
    public Sprite[] fullPowerSprites; // full P, O, W, E, R

    [Header("Used Sprites for each letter")]
    public Sprite[] usedPowerSprites; // gray P, O, W, E, R

    [Header("Recharge Settings")]
    public float rechargeTimeMinutes = 15f;

    [Header("Play Button (auto disable when out of energy)")]
    public Button playButton;

    private DateTime[] nextRechargeTimes;
    private int maxPower;
    private int currentPower;

    void Start()
    {
        maxPower = letterImages.Length;
        nextRechargeTimes = new DateTime[maxPower];
        LoadData();
        UpdatePlayButtonState();
        StartCoroutine(RechargeRoutine());
    }

    // Called by Play button
    public bool UsePower()
    {
        if (currentPower <= 0)
        {
            Debug.Log("Not enough energy!");
            UpdatePlayButtonState();
            return false;
        }

        // Use from right to left (R → E → W → O → P)
        int index = maxPower - currentPower;

        letterImages[index].sprite = usedPowerSprites[index];
        nextRechargeTimes[index] = DateTime.Now.AddMinutes(rechargeTimeMinutes);
        currentPower--;

        SaveData();
        UpdatePlayButtonState();
        return true;
    }

    private IEnumerator RechargeRoutine()
    {
        while (true)
        {
            for (int i = 0; i < maxPower; i++)
            {
                if (letterImages[i].sprite == usedPowerSprites[i] && DateTime.Now >= nextRechargeTimes[i])
                {
                    letterImages[i].sprite = fullPowerSprites[i];
                    currentPower++;
                    SaveData();
                    UpdatePlayButtonState();
                    break;
                }
            }
            yield return new WaitForSeconds(10f);
        }
    }

    public string GetRechargeStatus()
    {
        for (int i = 0; i < maxPower; i++)
        {
            if (letterImages[i].sprite == usedPowerSprites[i])
            {
                TimeSpan remaining = nextRechargeTimes[i] - DateTime.Now;
                if (remaining.TotalSeconds > 0)
                    return $"Recharge Energy in {Mathf.CeilToInt((float)remaining.TotalMinutes)} mins.";
            }
        }
        return "All energy full!";
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("currentPower", currentPower);
        for (int i = 0; i < maxPower; i++)
        {
            PlayerPrefs.SetString($"rechargeTime_{i}", nextRechargeTimes[i].ToBinary().ToString());
        }
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        currentPower = PlayerPrefs.GetInt("currentPower", maxPower);

        for (int i = 0; i < maxPower; i++)
        {
            string saved = PlayerPrefs.GetString($"rechargeTime_{i}", "");
            if (!string.IsNullOrEmpty(saved))
            {
                nextRechargeTimes[i] = DateTime.FromBinary(Convert.ToInt64(saved));

                if (DateTime.Now < nextRechargeTimes[i])
                    letterImages[i].sprite = usedPowerSprites[i];
                else
                    letterImages[i].sprite = fullPowerSprites[i];
            }
            else
            {
                letterImages[i].sprite = fullPowerSprites[i];
            }
        }

        // Count full energy
        currentPower = 0;
        for (int i = 0; i < maxPower; i++)
        {
            if (letterImages[i].sprite == fullPowerSprites[i])
                currentPower++;
        }
    }

    private void UpdatePlayButtonState()
    {
        if (playButton != null)
        {
            playButton.interactable = currentPower > 0;
        }
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        // Reset all when exiting Play Mode
        Debug.Log("Resetting PowerManager after stopping play mode in Unity Editor...");
        PlayerPrefs.DeleteKey("currentPower");
        for (int i = 0; i < maxPower; i++)
        {
            PlayerPrefs.DeleteKey($"rechargeTime_{i}");
        }
        PlayerPrefs.Save();
#endif
    }
}
