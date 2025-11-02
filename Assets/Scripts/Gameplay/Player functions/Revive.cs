using UnityEngine;
using UnityEngine.SceneManagement;

public class Revive : MonoBehaviour
{
    private static Vector3 deathPosition;
    private static bool useDeathPosition = false;

    public static void SetDeathPosition(Vector3 position)
    {
        deathPosition = position;
        useDeathPosition = true;
    }

    public void RevivePlayer()
    {
        Time.timeScale = 1;
        
        // Store the death position before loading the scene
        if (useDeathPosition)
        {
            // This will be used after the scene loads
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the player in the new scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null && useDeathPosition)
        {
            // Move player to death position
            player.transform.position = deathPosition;
            
            // Reset the flag
            useDeathPosition = false;
            
            // Remove the event listener
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Optional: You can also call this from the same place where you'd call Restart()
    public void RestartWithRevive()
    {
        RevivePlayer();
    }
}