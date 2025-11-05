using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Revive : MonoBehaviour
{
    [Header("Reference to Player")]
    public GameObject playerObject;

    private PlayerFunctions player;

    [Header("UI Panel")]
    public GameObject revivePanel; // assign your revive panel here

    void Awake()
    {
        if (playerObject != null)
        {
            player = playerObject.GetComponent<PlayerFunctions>();
            if (player == null)
                Debug.LogWarning("No PlayerFunctions component found on assigned playerObject!");
        }
        else
        {
            player = FindObjectOfType<PlayerFunctions>();
            if (player == null)
                Debug.LogWarning("No PlayerFunctions found in scene!");
        }
    }

    void Update()
    {
        // Automatically pause when revive panel is active
        if (revivePanel != null)
        {
            if (revivePanel.activeSelf && Time.timeScale != 0f)
            {
                Time.timeScale = 0f;
                Debug.Log("⏸️ Game paused because revive panel is active.");
            }
            else if (!revivePanel.activeSelf && Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
                Debug.Log("⏯️ Game resumed because revive panel is inactive.");
            }
        }
    }

    public void ShowRevivePanel()
    {
        if (revivePanel != null)
            revivePanel.SetActive(true);
    }

    public void RevivePlayer()
    {
        Time.timeScale = 1f;
        if (player == null)
        {
            Debug.LogWarning("Cannot revive: PlayerFunctions reference missing!");
            return;
        }

        if (revivePanel != null)
            revivePanel.SetActive(false);

        // Call the PlayerFunctions revive method instead of using coroutine
        player.ReviveFromDeath();
        
        Debug.Log("🔄 Revive process completed!");
    }

    public void Home()
    {
        if (player != null)
        {
            player.SaveTotalCoins();
            Debug.Log("💾 Coins saved before returning to HomeScreen.");
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerFunctions not found! Coins might not be saved.");
        }

        Time.timeScale = 1;
        SceneManager.LoadScene("HomeScreen");
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}