using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameTime : MonoBehaviour
{
    public float levelDuration = 300f; // 5 minutes
    private float timer;

    public TMP_Text timerText; // assign your TMP Text in Inspector

    void Start()
    {
        timer = levelDuration; // start at 5 minutes
        UpdateTimerText();
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer < 0) timer = 0;

            UpdateTimerText();
        }
        else
        {
            // Timer reached 0, trigger event
            TimerEnded();
        }
    }

    void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    void TimerEnded()
    {
        Debug.Log("⏰ Time's up!");
        IncreaseDifficulty();

        // Load next scene
        SceneManager.LoadScene("SecondScene");

        // Optionally reset timer if you want looping
        // timer = levelDuration;
    }

    void IncreaseDifficulty()
    {
        Debug.Log("Difficulty increased!");
        // Example: spawn more obstacles, increase speed, etc.
    }
}
