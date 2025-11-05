using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class Revive : MonoBehaviour
{
    [Header("Reference to Player")]
    public GameObject playerObject;
    private PlayerFunctions player;

    [Header("UI Panel")]
    public GameObject revivePanel; // assign your revive panel here

    [Header("UI to Disable When Revive Panel is Active")]
    public GameObject[] uiToDisable; // ✅ assign normal UI here (score, buttons, etc.)

    [Header("Revive Price UI")]
    public TMP_Text revivePriceText; // ✅ Drag your TMP text here
    private int revivePrice = 250;   // ✅ Starting price

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

        UpdateRevivePriceUI(); // ✅ Show initial price
    }

    void Update()
    {
        if (revivePanel != null)
        {
            // Pause when panel is open
            if (revivePanel.activeSelf && Time.timeScale != 0f)
                Time.timeScale = 0f;
            else if (!revivePanel.activeSelf && Time.timeScale == 0f)
                Time.timeScale = 1f;

            // Disable/Enable UI depending on revive panel state
            foreach (GameObject ui in uiToDisable)
            {
                if (ui != null)
                    ui.SetActive(!revivePanel.activeSelf);
            }
        }
    }

    public void ShowRevivePanel()
    {
        if (revivePanel != null)
            revivePanel.SetActive(true);

        UpdateRevivePriceUI(); // ✅ Ensure price shows whenever opened
    }

   public void RevivePlayer()
{
    Time.timeScale = 1f;

    if (player == null)
    {
        Debug.LogWarning("Cannot revive: PlayerFunctions reference missing!");
        return;
    }

    // ✅ Try to pay for revive
    if (!player.SpendCoins(revivePrice))
    {
        Debug.Log("❌ Not enough coins to revive!");
        return; // Do not revive if can't pay
    }

    // Hide revive UI
    if (revivePanel != null)
        revivePanel.SetActive(false);

    // Actually revive player
    player.ReviveFromDeath();

    // ✅ Increase revive price for next attempt
    revivePrice = Mathf.RoundToInt(revivePrice * 1.5f);
    UpdateRevivePriceUI();

    Debug.Log("🔄 Revived! New price: " + revivePrice);
}

    private void UpdateRevivePriceUI()
    {
        if (revivePriceText != null)
            revivePriceText.text = revivePrice.ToString();
    }

    public void Home()
    {
        if (player != null)
            player.SaveTotalCoins();

        Time.timeScale = 1;
        SceneManager.LoadScene("HomeScreen");
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
