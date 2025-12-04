using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetData : MonoBehaviour
{
    [Header("Keybind Settings")]
    [Tooltip("The key to press to reset game data")]
    public KeyCode resetKey = KeyCode.R;
    
    [Tooltip("Require holding Ctrl/Cmd while pressing the reset key")]
    public bool requireModifier = true;
    
    [Header("Reset Options")]
    [Tooltip("Clear all PlayerPrefs data")]
    public bool clearPlayerPrefs = true;
    
    [Tooltip("Reload the current scene after reset")]
    public bool reloadScene = true;
    
    [Tooltip("Show confirmation message in console")]
    public bool showDebugMessage = true;

    void Update()
    {
        // Check if modifier key is required and pressed
        bool modifierPressed = Input.GetKey(KeyCode.LeftControl) || 
                               Input.GetKey(KeyCode.RightControl) ||
                               Input.GetKey(KeyCode.LeftCommand) || 
                               Input.GetKey(KeyCode.RightCommand);
        
        // Only trigger if:
        // - Modifier is NOT required, OR
        // - Modifier IS required AND is being pressed
        bool canReset = (!requireModifier) || (requireModifier && modifierPressed);
        
        if (canReset && Input.GetKeyDown(resetKey))
        {
            ResetGameData();
        }
    }

    void ResetGameData()
    {
        if (showDebugMessage)
        {
            Debug.Log("=== GAME DATA RESET ===");
        }

        // Clear PlayerPrefs (saved game data)
        if (clearPlayerPrefs)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            if (showDebugMessage)
            {
                Debug.Log("PlayerPrefs cleared!");
            }
        }

        // Add any additional custom reset logic here
        // For example:
        // GameManager.Instance.ResetScore();
        // InventoryManager.Instance.ClearInventory();

        // Reload the current scene
        if (reloadScene)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            
            if (showDebugMessage)
            {
                Debug.Log($"Reloading scene: {currentScene}");
            }
            
            SceneManager.LoadScene(currentScene);
        }
    }

    // Optional: Public method to reset data programmatically
    public void ResetDataButton()
    {
        ResetGameData();
    }
}