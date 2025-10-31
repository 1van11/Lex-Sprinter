using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameTime : MonoBehaviour
{
    public float timer = 0f; // start from zero
    public TMP_Text timerText; // assign your TMP Text in Inspector

    void Start()
    {
        UpdateTimerText(); // display 0:00 at start
    }

    void Update()
    {
        timer += Time.deltaTime; // count upward every frame
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }
}
