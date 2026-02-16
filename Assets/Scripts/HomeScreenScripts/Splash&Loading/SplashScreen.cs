using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
    public RectTransform monster;
    public Image letterImage;

    public float moveDistance = 600f;
    public float animationDuration = 2f;

    public float eatSpeed = 15f;
    public float eatAmount = 0.1f;

    public string nextSceneName; // Name of scene to load

    private Vector2 startPos;
    private Vector2 endPos;

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

        monster.anchoredPosition = endPos;
        letterImage.fillAmount = 0f;
        monster.localScale = Vector3.one;

        // Small delay (optional)
        yield return new WaitForSeconds(0.5f);

        // Load next scene
        SceneManager.LoadScene(nextSceneName);
    }
}
