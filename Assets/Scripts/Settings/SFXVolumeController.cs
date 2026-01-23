using UnityEngine;
using UnityEngine.UI;

public class SFXVolumeController : MonoBehaviour
{
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private Slider sfxSlider;

    private const string SFXPrefKey = "SFXVolume";

    private void Start()
    {
        // Load saved SFX volume
        if (PlayerPrefs.HasKey(SFXPrefKey))
        {
            float savedVolume = PlayerPrefs.GetFloat(SFXPrefKey);
            sfxSlider.value = savedVolume;
        }
        else
        {
            sfxSlider.value = sfxSlider.maxValue;
        }

        // Set the initial volume
        SetSFXVolume();

        // Add listener for slider changes
        sfxSlider.onValueChanged.AddListener(delegate { SetSFXVolume(); });
    }

    public void SetSFXVolume()
    {
        // Convert slider value (0-100) to volume (0-1)
        sfxAudioSource.volume = sfxSlider.value / 100f;

        // Save the value
        PlayerPrefs.SetFloat(SFXPrefKey, sfxSlider.value);
        PlayerPrefs.Save();
    }
}