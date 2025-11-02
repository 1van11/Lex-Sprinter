using System.Collections;
using UnityEngine;

public class Revive : MonoBehaviour
{
    [Header("UI References")]
    public GameObject reviveUI;     // The UI shown when the player can revive
    public GameObject gameOverUI;   // The UI shown when the game ends

    [Header("Player Reference")]
    public GameObject player;       // The player GameObject to revive

    private bool hasRevived = false; // Prevent multiple revives

    // Called when the player dies
    public void OnPlayerDeath()
    {
        if (hasRevived)
        {
            ShowGameOver();
        }
        else
        {
            // Show the revive screen
            if (reviveUI != null)
                reviveUI.SetActive(true);
        }
    }

    // Called when the player chooses to revive (button click, ad, or coin)
    public void ReviveChoice()
    {
        if (reviveUI != null)
            reviveUI.SetActive(false);

        RevivePlayer();
    }

    // Actually revives the player
   private void RevivePlayer()
{
    hasRevived = true;

    // Reset player position to a safe zone (adjust Y as needed)
    player.transform.position = new Vector3(player.transform.position.x, 0, player.transform.position.z);

    // Reactivate player if disabled
    player.SetActive(true);

    // Reset movement
    var moveScript = player.GetComponent<PlayerControls>();
    if (moveScript != null)
        moveScript.ResumeMovement();
}

    // Shows the Game Over UI
    private void ShowGameOver()
    {
        if (reviveUI != null)
            reviveUI.SetActive(false);

        if (gameOverUI != null)
            gameOverUI.SetActive(true);
    }
}
