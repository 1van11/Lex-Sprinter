using UnityEngine;

public class TileTrigger : MonoBehaviour
{
    private InfiniteRunner runner;
    private ObstacleSpawner obstacleSpawner;

    void Start()
    {
        // Automatically find both systems in the scene
        runner = FindObjectOfType<InfiniteRunner>();
        obstacleSpawner = FindObjectOfType<ObstacleSpawner>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Ask InfiniteRunner to spawn the next tile
        if (runner != null)
            runner.OnPlayerTrigger();

        // Ask ObstacleSpawner to spawn traps for the new tile
        // BUT only if letter event is NOT active
        if (obstacleSpawner != null && !obstacleSpawner.IsLetterEventActive)
        {
            obstacleSpawner.TriggerSpawnNow();
        }
        else if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
        {
            Debug.Log("⏸️ TileTrigger: Skipping obstacle spawn - Letter event active");
        }
    }
}