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
        if (obstacleSpawner != null)
        {
            obstacleSpawner.TriggerSpawnNow();
        }
    }
}
