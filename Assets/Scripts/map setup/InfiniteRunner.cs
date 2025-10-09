using System.Collections.Generic;
using UnityEngine;

public class InfiniteRunner : MonoBehaviour
{
    [Header("Prefabs & Pooling")]
    public GameObject[] mapPrefabs;      // should have 2 prefabs
    public int poolSize = 12;            // total pooled tiles
    public int tilesPerMap = 6;          // switch after this many tiles

    [Header("Placement")]
    public Transform spawnPoint;

    private Queue<GameObject> tilePool = new Queue<GameObject>();
    private Vector3 nextSpawnPosition;
    private int totalSpawned = 0;
    private int currentMapIndex = 0;

    // store prefab rotations (copied from the prefab reference)
    private Quaternion[] prefabRotations;

    void Start()
    {
        nextSpawnPosition = spawnPoint ? spawnPoint.position : transform.position;

        // Save each prefab's rotation as you placed them in the scene or prefab asset
        prefabRotations = new Quaternion[mapPrefabs.Length];
        for (int i = 0; i < mapPrefabs.Length; i++)
            prefabRotations[i] = mapPrefabs[i].transform.rotation;

        // Fill pool initially with first map only
        for (int i = 0; i < poolSize; i++)
        {
            GameObject tile = Instantiate(mapPrefabs[0], transform);
            tile.SetActive(false);
            tilePool.Enqueue(tile);
        }

        SpawnTile();
    }

    public void OnPlayerTrigger()
    {
        SpawnTile();
    }

    void SpawnTile()
    {
        // Switch map every X tiles
        if (totalSpawned > 0 && totalSpawned % tilesPerMap == 0)
            currentMapIndex = (currentMapIndex + 1) % mapPrefabs.Length;

        // Get next tile from pool
        GameObject tile = tilePool.Dequeue();

        // If tile type doesn’t match current map, recreate it properly
        if (!tile || !tile.name.Contains(mapPrefabs[currentMapIndex].name))
        {
            if (tile) Destroy(tile);
            tile = Instantiate(mapPrefabs[currentMapIndex], transform);
        }

        // Position + rotation from prefab
        tile.transform.position = nextSpawnPosition;
        tile.transform.rotation = prefabRotations[currentMapIndex];
        tile.SetActive(true);

        // Measure map length using renderers
        Renderer[] renderers = tile.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);
        float length = bounds.size.z;

        nextSpawnPosition += new Vector3(0f, 0f, length);
        totalSpawned++;

        // Re-enqueue the tile for reuse
        tilePool.Enqueue(tile);
    }
}
