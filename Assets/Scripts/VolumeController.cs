using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;

    private const string MusicPrefKey = "MusicVolume";

    private void Start()
    {
        // Load saved volume if exists
        if (PlayerPrefs.HasKey(MusicPrefKey))
        {
            float savedVolume = PlayerPrefs.GetFloat(MusicPrefKey);
            musicSlider.value = savedVolume;
            SetMusicVolume();
        }
        else
        {
            // Default volume (max)
            musicSlider.value = musicSlider.maxValue;
            SetMusicVolume();
        }

        // Add listener so every time slider changes, volume updates & saves
        musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(); });
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value / 100f; // normalize 0–1

        if (volume <= 0f)
            myMixer.SetFloat("music", -80f);
        else
            myMixer.SetFloat("music", Mathf.Log10(volume) * 20);

        // Save slider value
        PlayerPrefs.SetFloat(MusicPrefKey, musicSlider.value);
        PlayerPrefs.Save();
    }
}
