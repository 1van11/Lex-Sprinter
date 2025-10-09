using UnityEngine;

public class TileTrigger : MonoBehaviour
{
  private InfiniteRunner runner;

    void Start()
    {
        // find the InfiniteRunner in the scene automatically
        runner = FindObjectOfType<InfiniteRunner>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ask the InfiniteRunner to spawn a new tile
            runner.OnPlayerTrigger();
        }
    }
}
