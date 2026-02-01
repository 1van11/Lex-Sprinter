using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] int startCount = 3;
    [SerializeField] float delayBetweenNumbers = 1f;
    [SerializeField] float goTextDuration = 0.5f;
    [SerializeField] float slowMotionSpeed = 0.01f; // Ultra slow (almost paused)

    private void OnEnable()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        // Immediately slow down the game to ultra slow
        Time.timeScale = slowMotionSpeed;
        Debug.Log($"🐌 Game slowed to {slowMotionSpeed}x speed");

        // Initial delay
        yield return new WaitForSecondsRealtime(0.3f);

        // Countdown from startCount to 1
        for (int i = startCount; i > 0; i--)
        {
            countdownText.text = i.ToString();
            Debug.Log($"⏱️ Countdown: {i}");
            
            yield return new WaitForSecondsRealtime(delayBetweenNumbers);
        }

        // Show "GO!" text
        countdownText.text = "GO!";
        Debug.Log("🚀 GO!");
        yield return new WaitForSecondsRealtime(goTextDuration);

        // Clear text and return to normal speed
        countdownText.text = "";
        Time.timeScale = 1f;
        Debug.Log("✅ Game resumed to normal speed!");
        
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Safety: ensure game speed is restored if this is disabled
        if (Time.timeScale < 0.5f)
        {
            Time.timeScale = 1f;
            Debug.Log("⚠️ Countdown disabled - restoring normal speed");
        }
    }
}