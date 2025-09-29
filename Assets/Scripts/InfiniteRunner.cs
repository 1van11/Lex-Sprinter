using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteRunner : MonoBehaviour
{
    [Header("Prefab & timing")]
    public GameObject mapPrefab;           // your map/tile prefab
    public float spawnInterval = 6f;      // seconds between spawns

    [Header("Placement")]
    public Transform spawnPoint;          // optional: where the first tile spawns (uses this.transform if null)
    public Vector3 spawnOffset = new Vector3(0, 0, 50); // how much to move for each new tile

    [Header("Cleanup")]
    public int maxActiveTiles = 6;        // keep scene tidy

    public Vector3 spawnRotationEuler = new Vector3(-90f, -90f, 0f);

    private Queue<GameObject> activeTiles = new Queue<GameObject>();
    private Vector3 nextSpawnPosition;

    void Start()
    {
        nextSpawnPosition = spawnPoint ? spawnPoint.position : transform.position;

        // optional: pre-spawn a few tiles so the world looks continuous at start
        for (int i = 0; i < 1; i++)
            SpawnTile();

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnTile();
        }
    }

    void SpawnTile()
    {
        if (mapPrefab == null) return;

        GameObject go = Instantiate(mapPrefab, nextSpawnPosition, mapPrefab.transform.rotation);
        activeTiles.Enqueue(go);

        // measure the spawned prefab's bounds
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        Bounds totalBounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            totalBounds.Encapsulate(r.bounds);

        // move the spawn position forward based on prefab size
        float length = totalBounds.size.z;  // if obby runs along Z axis
        nextSpawnPosition += new Vector3(0f, 0f, length);

        // cleanup
        if (activeTiles.Count > maxActiveTiles)
        {
            GameObject old = activeTiles.Dequeue();
            Destroy(old);
        }
    }
}
