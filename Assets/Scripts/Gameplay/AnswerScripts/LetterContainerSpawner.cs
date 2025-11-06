using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterContainerSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform spawnParent;
    public GameObject letterContainerPrefab;
    public GameObject spawnerIndicatorPrefab; // New: Prefab for the 3D spawner indicator

    [Header("Lane Settings")]
    public float laneDistance = 3f;

    [Header("Spawn Placement")]
    public float spawnDistanceAhead = 20f;
    public float spawnInterval = 10f;
    public float spawnHeight = 0.5f;
    public Vector3 spawnPositionOffset = Vector3.zero;

    [Header("Spawner Offset (Editable Position)")]
    public Vector3 spawnerOffset = Vector3.zero; 

    [Header("Initial Delay")]
    public float initialSpawnDelaySeconds = 1f;

    [Header("Event Spawn")]
    public float eventFrequency = 60f;
    public float eventInitialDelay = 10f;
    public float eventSpawnDistanceAhead = 25f;

    [Header("Indicator Settings")] // New: Settings for the spawner indicator
    public float indicatorSpawnDistanceAhead = 30f; // Spawn distance for the indicator ahead of the player
    public float indicatorAnimationTime = 2f; // Time for the indicator to animate down and up
    public float indicatorStayTime = 5f; // Time the indicator stays down before animating up
    public float indicatorStartHeightOffset = 10f; // How high above spawnHeight the indicator starts

    [Header("Prefab Transform")]
    public bool usePrefabTransform = true;
    public Vector3 spawnRotationEuler = Vector3.zero;
    public Vector3 spawnScale = Vector3.one;

    [Header("Pooling Settings")]
    public int poolSize = 50;
    public float despawnTime = 5f;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<SpawnedInfo> activeObjects = new List<SpawnedInfo>();
    private bool allowRegularSpawning = false;
    private float nextSpawnZ;

    class SpawnedInfo
    {
        public GameObject obj;
        public float timer;
    }

    void Start()
    {
        CreatePool();
        nextSpawnZ = player.position.z + spawnDistanceAhead + 0.01f;
        StartCoroutine(EnableRegularSpawningAfterDelay(initialSpawnDelaySeconds));
        StartCoroutine(EventSpawner());
    }

    void Update()
    {
        if (allowRegularSpawning && player.position.z + spawnDistanceAhead >= nextSpawnZ)
        {
            SpawnRandomLaneAtZ(nextSpawnZ);
            nextSpawnZ += spawnInterval;
        }

        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            activeObjects[i].timer += Time.deltaTime;
            if (activeObjects[i].timer >= despawnTime)
            {
                ReturnToPool(activeObjects[i].obj);
                activeObjects.RemoveAt(i);
            }
        }
    }

    void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject o = Instantiate(letterContainerPrefab, Vector3.zero, Quaternion.identity, spawnParent);
            o.SetActive(false);
            pool.Enqueue(o);
        }
    }

    GameObject GetFromPool()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        GameObject o = Instantiate(letterContainerPrefab, Vector3.zero, Quaternion.identity, spawnParent);
        return o;
    }

    void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    IEnumerator EnableRegularSpawningAfterDelay(float s)
    {
        yield return new WaitForSeconds(s);
        nextSpawnZ = player.position.z + spawnDistanceAhead;
        allowRegularSpawning = true;
    }

    void SpawnRandomLaneAtZ(float z)
    {
        int lane = Random.Range(-1, 2);
        float x = lane * laneDistance;
        SpawnFromPool(new Vector3(x, spawnHeight, z) + spawnPositionOffset + spawnerOffset);
    }

    void SpawnFromPool(Vector3 pos)
    {
        GameObject go = GetFromPool();
        go.SetActive(true);

        Quaternion rot = usePrefabTransform ?
            letterContainerPrefab.transform.rotation :
            Quaternion.Euler(spawnRotationEuler);

        Vector3 scale = usePrefabTransform ?
            letterContainerPrefab.transform.localScale :
            spawnScale;

        go.transform.SetPositionAndRotation(pos, rot);
        go.transform.localScale = scale;

        activeObjects.Add(new SpawnedInfo() { obj = go, timer = 0f });
    }

    // New: Method to spawn event hurdles at a specific Z
    void SpawnEventHurdlesAtZ(float z)
    {
        int count = Random.Range(1, 3);
        List<int> lanes = new List<int>() { -1, 0, 1 };

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, lanes.Count);
            int lane = lanes[idx];
            lanes.RemoveAt(idx);

            float x = lane * laneDistance;
            SpawnFromPool(new Vector3(x, spawnHeight, z) + spawnPositionOffset + spawnerOffset);
        }
    }

    // New: Method to spawn the indicator at a specific Z
    void SpawnIndicatorAtZ(float z)
    {
        float x = 0f; // Center lane for indicator
        float startY = spawnHeight + indicatorStartHeightOffset;
        Vector3 pos = new Vector3(x, startY, z) + spawnPositionOffset + spawnerOffset;
        GameObject indicator = Instantiate(spawnerIndicatorPrefab, pos, Quaternion.identity, spawnParent);
        StartCoroutine(AnimateIndicator(indicator, z));
    }

    // New: Coroutine to animate the indicator: down to spawn, stay, then up
    IEnumerator AnimateIndicator(GameObject indicator, float z)
    {
        Vector3 startPos = indicator.transform.position;
        Vector3 downPos = new Vector3(startPos.x, spawnHeight, startPos.z);

        // Animate down
        float time = 0f;
        while (time < indicatorAnimationTime)
        {
            indicator.transform.position = Vector3.Lerp(startPos, downPos, time / indicatorAnimationTime);
            time += Time.deltaTime;
            yield return null;
        }
        indicator.transform.position = downPos;

        // Spawn the hurdles while indicator is down
        SpawnEventHurdlesAtZ(z);

        // Stay down for a period (simulating "when defeated or finish" - you can adjust logic here)
        yield return new WaitForSeconds(indicatorStayTime);

        // Animate up
        Vector3 upPos = startPos;
        time = 0f;
        while (time < indicatorAnimationTime)
        {
            indicator.transform.position = Vector3.Lerp(downPos, upPos, time / indicatorAnimationTime);
            time += Time.deltaTime;
            yield return null;
        }

        // Destroy the indicator after animation
        Destroy(indicator);
    }

    IEnumerator EventSpawner()
    {
        yield return new WaitForSeconds(eventInitialDelay);

        while (true)
        {
            yield return new WaitForSeconds(eventFrequency);

            // Use the indicator's spawn distance
            float z = player.position.z + indicatorSpawnDistanceAhead;
            SpawnIndicatorAtZ(z);
        }
    }
}