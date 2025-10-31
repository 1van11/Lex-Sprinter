using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;

    [Header("Heart Settings")]
    [SerializeField] private int maxHearts = 5;
    [SerializeField] private int currentHearts;

    [Header("UI")]
    [SerializeField] private Image[] heartIcons;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    [Header("IFrames")]
    public float iFrameDuration = 1f;
    public float flashInterval = 0.1f;
    private bool isInvincible = false;

    private SpriteRenderer[] sprites;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentHearts = maxHearts;
        UpdateHeartsUI();

        // ✅ FIX: Use SpriteRenderer, not Renderer
        sprites = GetComponentsInChildren<SpriteRenderer>();
    }

    public void TakeDamage(int amount = 1)
    {
        if (isInvincible) return;

        currentHearts -= amount;
        UpdateHeartsUI();

        if (currentHearts <= 0)
        {
            SceneManager.LoadScene("MainMenuSplash");
            return;
        }

        StartCoroutine(IFrames());
    }

    IEnumerator IFrames()
    {
        isInvincible = true;
        float timer = 0f;

        while (timer < iFrameDuration)
        {
            foreach (SpriteRenderer s in sprites)
                s.enabled = !s.enabled; // ✅ THIS NOW BLINKS

            timer += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        foreach (SpriteRenderer s in sprites)
            s.enabled = true;

        isInvincible = false;
    }

    public void Heal(int amount)
    {
        currentHearts = Mathf.Min(currentHearts + amount, maxHearts);
        UpdateHeartsUI();
    }

    private void UpdateHeartsUI()
    {
        for (int i = 0; i < heartIcons.Length; i++)
            heartIcons[i].sprite = (i < currentHearts) ? fullHeart : emptyHeart;
    }
}
