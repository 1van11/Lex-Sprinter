using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSplash : MonoBehaviour
{
    public string gameSceneName = "SampleScene"; // Your actual game scene

    void Start()
    {
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        // Wait for 2 seconds to show splash (change as needed)
        yield return new WaitForSeconds(2f);

        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }
}