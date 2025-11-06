using UnityEngine;
using TMPro;

public class Revive : MonoBehaviour
{
    [Header("Reference to Player")]
    public GameObject playerObject;
    private PlayerFunctions player;

    [Header("UI Panel")]
    public GameObject revivePanel; // assign your revive panel here

    [Header("UI to Disable When Revive Panel is Active")]
    public GameObject[] uiToDisable; // assign normal UI here (score, buttons, etc.)

    [Header("Revive Price UI")]
    public TMP_Text revivePriceText; // Drag your TMP text here
    private int revivePrice = 250;   // Starting price

    void Awake()
    {
        if (playerObject != null)
            player = playerObject.GetComponent<PlayerFunctions>();
        else
            player = FindObjectOfType<PlayerFunctions>();

        if (player == null)
            Debug.LogWarning("PlayerFunctions not found in scene!");

        UpdateRevivePriceUI(); // Show initial price
    }

    public void ShowRevivePanel()
    {
        if (revivePanel != null)
        {
            revivePanel.SetActive(true);

            // Pause the game
            Time.timeScale = 0;

            // Disable other UI
            foreach (GameObject ui in uiToDisable)
                if (ui != null)
                    ui.SetActive(false);
        }

        UpdateRevivePriceUI();
    }

    public void RevivePlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("Cannot revive: PlayerFunctions reference missing!");
            return;
        }

        // Try to spend coins
        if (!player.SpendCoins(revivePrice))
        {
            Debug.Log("❌ Not enough coins to revive!");
            return;
        }

        // Hide revive panel
        if (revivePanel != null)
            revivePanel.SetActive(false);

        // Re-enable UI
        foreach (GameObject ui in uiToDisable)
            if (ui != null)
                ui.SetActive(true);

        // Resume the game
        Time.timeScale = 1;

        // Actually revive player
        player.ReviveFromDeath();

        // Increase revive price for next time
        revivePrice = Mathf.RoundToInt(revivePrice * 1.5f);
        UpdateRevivePriceUI();

        Debug.Log("🔄 Revived! New price: " + revivePrice);
    }

    public void CancelRevive()
    {
        // Called if player closes the revive panel without reviving
        if (revivePanel != null)
            revivePanel.SetActive(false);

        foreach (GameObject ui in uiToDisable)
            if (ui != null)
                ui.SetActive(true);

        // Resume the game
        Time.timeScale = 1;

        Debug.Log("❌ Revive canceled, game resumed.");
    }

    private void UpdateRevivePriceUI()
    {
        if (revivePriceText != null)
            revivePriceText.text = revivePrice.ToString();
    }
}
