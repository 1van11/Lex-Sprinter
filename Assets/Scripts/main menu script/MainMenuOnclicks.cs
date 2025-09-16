using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuOnclicks : MonoBehaviour
{
    public void PlayButton()
    {
        // Load the loading splash first
        SceneManager.LoadScene("Splashscreen");
    }
}
