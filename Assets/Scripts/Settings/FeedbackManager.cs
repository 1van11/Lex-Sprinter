using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // keeps it across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Vibrate()
{
    if (PlayerPrefs.GetInt("Vibration", 1) == 1)
    {
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
        Debug.Log("📳 VIBRATION TRIGGERED");
    }
}
}
