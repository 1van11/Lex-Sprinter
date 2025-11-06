using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MediumMode : MonoBehaviour
{
    public PowerManager powerManager;

    public void PlayButton()
    {
        if (powerManager.UsePower())
        {
            SceneManager.LoadScene("MediumMode");
        }
        else
        {
            Debug.Log("Not enough stamina to play!");
        }
    }
}
