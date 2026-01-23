using UnityEngine;
using UnityEngine.UI;

public class VibratorManager : MonoBehaviour
{
    public Toggle vibrationToggle;

    void Start()
    {
        // Load saved setting (default ON)
        int vib = PlayerPrefs.GetInt("Vibration", 1);
        vibrationToggle.isOn = vib == 1;
    }

    public void OnVibrationToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("Vibration", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}