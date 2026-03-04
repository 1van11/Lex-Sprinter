using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
    [Header("Monster Animation")]
    public RectTransform monster;
    public Image letterImage;

    [Header("Animation Settings")]
    public float moveDistance = 600f;
    public float animationDuration = 2f;
    public float eatSpeed = 15f;
    public float eatAmount = 0.1f;
    
    [Header("Scene Names")]
    public string nameCharacterScene = "Name&Character";
    public string homeScreenScene = "HomeScreen";

    private Vector2 startPos;
    private Vector2 endPos;
    
    // Setup keys
    private const string SETUP_COMPLETE_KEY = "HasCompletedSetup";
    private const string PLAYER_NAME_KEY = "PlayerName";

    void Start()
    {
        startPos = monster.anchoredPosition;
        endPos = startPos + new Vector2(moveDistance, 0);

        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
{
    float time = 0f;

    while (time < animationDuration)
    {
        time += Time.deltaTime;
        float t = time / animationDuration;

        // Move monster
        monster.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

        // Hide text
        letterImage.fillAmount = 1f - t;

        // Fake eating bounce
        float scale = 1f + Mathf.Sin(Time.time * eatSpeed) * eatAmount;
        monster.localScale = new Vector3(scale, scale, 1f);

        yield return null;
    }

    // Finish animation
    monster.anchoredPosition = endPos;
    letterImage.fillAmount = 0f;
    monster.localScale = Vector3.one;

    // Immediately load correct scene (no delay)
    LoadCorrectScene();
}

    void LoadCorrectScene()
    {
        // Check if player has completed setup
        bool setupComplete = PlayerPrefs.GetInt(SETUP_COMPLETE_KEY, 0) == 1;
        string playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, "");

        if (setupComplete && !string.IsNullOrEmpty(playerName))
        {
            // Returning player - skip to HomeScreen
            Debug.Log("✅ Returning player! Loading HomeScreen...");
            Debug.Log($"   Welcome back, {playerName}!");
            SceneManager.LoadScene(homeScreenScene);
        }
        else
        {
            // New player - show Name&Character setup
            Debug.Log("🆕 New player! Loading Name&Character setup...");
            SceneManager.LoadScene(nameCharacterScene);
        }
    }
}