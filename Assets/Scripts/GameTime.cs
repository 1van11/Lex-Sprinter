using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTime : MonoBehaviour
{
    public float levelDuration = 300f; // 5 minutes in seconds
    private float timer;

    void Start()
    {
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= levelDuration)
        {
            // Increase difficulty here
            IncreaseDifficulty();

            // Or load the next scene
            SceneManager.LoadScene("SecondScene");
            timer = 0f; // Reset timer if you want to keep doing this repeatedly
        }
    }

    void IncreaseDifficulty()
    {
        // Implement your difficulty increase logic here
        Debug.Log("Difficulty increased!");
        // For example, spawn more obstacles, increase speed, etc.
    }
}
