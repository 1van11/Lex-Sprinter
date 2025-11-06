using System.Collections;
using UnityEngine;

public class Lexivore : MonoBehaviour
{
    [Header("Movement Settings")]
    public float idleHeight = 6f;         // Height when idle
    public float descendHeight = 1.5f;    // Height to descend for spawning
    public float descendSpeed = 4f;       // Speed of moving down
    public float ascendSpeed = 3f;        // Speed of moving up
    public float stayTime = 2f;           // How long it stays down

    private bool isAnimating = false;

    // Call this to trigger animation at a specific XZ spawn position
    public void AnimateAtPosition(Vector3 spawnPos)
    {
        if (isAnimating) StopAllCoroutines();
        StartCoroutine(AnimateDownUp(spawnPos));
    }

    IEnumerator AnimateDownUp(Vector3 spawnPos)
    {
        isAnimating = true;

        Vector3 startPos = new Vector3(spawnPos.x, idleHeight, spawnPos.z);
        Vector3 downPos = new Vector3(spawnPos.x, descendHeight, spawnPos.z);

        transform.position = startPos;

        // Move down
        while (Vector3.Distance(transform.position, downPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, downPos, descendSpeed * Time.deltaTime);
            yield return null;
        }

        // Stay down
        yield return new WaitForSeconds(stayTime);

        // Move back up
        while (Vector3.Distance(transform.position, startPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, ascendSpeed * Time.deltaTime);
            yield return null;
        }

        isAnimating = false;
    }
}
