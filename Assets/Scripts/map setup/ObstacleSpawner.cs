using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement; // Reference to your PlayerMovement script
    public GameObject obstaclePrefab; // Obstacle prefab
    public GameObject coinPrefab; // Coin prefab

    [Header("Spawn Settings")]
    public float spawnDistance = 20f; // Distance ahead of player to spawn obstacles
    public int rowsPerSpawn = 2; // Number of rows to spawn at once
    public float rowSpacing = 10f; // Distance between rows
    public int maxObstaclesPerRow = 2; // Max obstacles per row (max 3 for 3 lanes)
    public float spawnHeight = -1f; // Y position (vertical location)
    public float laneDistance = 3f; // Distance between lanes

    [Header("Coin Settings")]
    public Vector3 coinPositionOffset = Vector3.zero; // XYZ offset from lane center (X=left/right, Y=up/down, Z=forward/back)
    public Vector3 coinScale = Vector3.one; // Coin size (1,1,1 = normal)
    public int coinsPerLane = 1; // Number of coins to spawn per empty lane
    public float coinSpacing = 2f; // Distance between coins (if multiple per lane)

    [Header("Object Pooling")]
    public int poolSize = 20; // Initial pool size
    public int coinPoolSize = 30; // Initial coin pool size

    [Header("Despawn Settings")]
    public float despawnDistance = 10f; // Distance behind player before obstacle despawns
    public float maxObstacleLifetime = 15f; // Max time before obstacle auto-despawns

    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color spawnGizmoColor = Color.red;
    public Color despawnGizmoColor = Color.blue;
    public Color distanceGizmoColor = Color.yellow;
    public float gizmoSphereSize = 0.5f;
    public float laneWidth = 2.5f;
    public float laneLength = 5f;
    public float gizmoHeight = 1f;

    private List<GameObject> activeObstacles = new List<GameObject>();
    private Queue<GameObject> obstaclePool = new Queue<GameObject>();
    private List<GameObject> activeCoins = new List<GameObject>();
    private Queue<GameObject> coinPool = new Queue<GameObject>();

    public float iFrameDuration = 2f;
    public float flashInterval = 0.2f;
    private bool triggered = false;

    void Start()
    {
        InitializePool();
    }

    void InitializePool()
    {
        // Initialize obstacle pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab);
            obj.SetActive(false);
            obstaclePool.Enqueue(obj);
        }

        // Initialize coin pool
        if (coinPrefab != null)
        {
            for (int i = 0; i < coinPoolSize; i++)
            {
                GameObject coin = Instantiate(coinPrefab);
                coin.SetActive(false);
                coinPool.Enqueue(coin);
            }
        }
    }

    GameObject GetPooledObstacle()
    {
        if (obstaclePool.Count > 0)
        {
            GameObject obj = obstaclePool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(obstaclePrefab);
            return obj;
        }
    }

    GameObject GetPooledCoin()
    {
        if (coinPool.Count > 0)
        {
            GameObject coin = coinPool.Dequeue();
            coin.SetActive(true);
            return coin;
        }
        else
        {
            GameObject coin = Instantiate(coinPrefab);
            return coin;
        }
    }

    void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obstaclePool.Enqueue(obj);
    }

    void ReturnCoinToPool(GameObject coin)
    {
        coin.SetActive(false);
        coinPool.Enqueue(coin);
    }

    void Update()
    {
        DespawnOldObstacles();
        DespawnOldCoins();
    }

    public void TriggerSpawnNow()
    {
        SpawnObstacleRows();
    }

    void SpawnObstacleRows()
    {
        for (int row = 0; row < rowsPerSpawn; row++)
        {
            float zOffset = spawnDistance + (row * rowSpacing);
            SpawnObstacleRow(zOffset);
        }
    }

    void SpawnObstacleRow(float zOffset)
    {
        List<int> availableLanes = new List<int> { 0, 1, 2 };
        int obstaclesToSpawn = Mathf.Min(maxObstaclesPerRow, 3);
        obstaclesToSpawn = Mathf.Clamp(obstaclesToSpawn, 1, 2);

        List<int> occupiedLanes = new List<int>();

        // Spawn obstacles
        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            int index = Random.Range(0, availableLanes.Count);
            int lane = availableLanes[index];
            availableLanes.RemoveAt(index);
            occupiedLanes.Add(lane);

            float laneX = (lane - 1.3f) * laneDistance;
            Vector3 spawnPos = new Vector3(laneX, spawnHeight, playerMovement.transform.position.z + zOffset);

            GameObject obstacle = GetPooledObstacle();
            obstacle.transform.position = spawnPos;
            obstacle.transform.rotation = obstaclePrefab.transform.rotation;

            activeObstacles.Add(obstacle);
            StartCoroutine(AutoDespawnObstacle(obstacle, maxObstacleLifetime));
        }

        // Spawn coins in empty lanes
        if (coinPrefab != null)
        {
            foreach (int emptyLane in availableLanes)
            {
                float baseLaneX = (emptyLane - 1.3f) * laneDistance;
                
                // Spawn multiple coins per lane if specified
                for (int c = 0; c < coinsPerLane; c++)
                {
                    float baseZPosition = playerMovement.transform.position.z + zOffset;
                    
                    // If multiple coins, space them out along the Z axis
                    if (coinsPerLane > 1)
                    {
                        float totalSpacing = (coinsPerLane - 1) * coinSpacing;
                        float startOffset = -totalSpacing / 2f;
                        baseZPosition += startOffset + (c * coinSpacing);
                    }
                    
                    // Apply the position offset
                    Vector3 coinPos = new Vector3(
                        baseLaneX + coinPositionOffset.x, 
                        spawnHeight + coinPositionOffset.y, 
                        baseZPosition + coinPositionOffset.z
                    );

                    GameObject coin = GetPooledCoin();
                    coin.transform.position = coinPos;
                    coin.transform.rotation = coinPrefab.transform.rotation;
                    coin.transform.localScale = coinScale;

                    activeCoins.Add(coin);
                    StartCoroutine(AutoDespawnCoin(coin, maxObstacleLifetime));
                }
            }
        }
    }

    IEnumerator AutoDespawnObstacle(GameObject obstacle, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (obstacle != null && obstacle.activeSelf)
        {
            activeObstacles.Remove(obstacle);
            ReturnToPool(obstacle);
        }
    }

    IEnumerator AutoDespawnCoin(GameObject coin, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (coin != null && coin.activeSelf)
        {
            activeCoins.Remove(coin);
            ReturnCoinToPool(coin);
        }
    }

    void DespawnOldObstacles()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null || !activeObstacles[i].activeSelf)
            {
                activeObstacles.RemoveAt(i);
            }
            else if (activeObstacles[i].transform.position.z < playerMovement.transform.position.z - despawnDistance)
            {
                GameObject obstacle = activeObstacles[i];
                activeObstacles.RemoveAt(i);
                ReturnToPool(obstacle);
            }
        }
    }

    void DespawnOldCoins()
    {
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (activeCoins[i] == null || !activeCoins[i].activeSelf)
            {
                activeCoins.RemoveAt(i);
            }
            else if (activeCoins[i].transform.position.z < playerMovement.transform.position.z - despawnDistance)
            {
                GameObject coin = activeCoins[i];
                activeCoins.RemoveAt(i);
                ReturnCoinToPool(coin);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || playerMovement == null) return;

        float despawnZ = playerMovement.transform.position.z - despawnDistance;

        for (int row = 0; row < rowsPerSpawn; row++)
        {
            float spawnZ = playerMovement.transform.position.z + spawnDistance + (row * rowSpacing);
            float alpha = 1f - (row * 0.3f);
            alpha = Mathf.Clamp01(alpha);

            Gizmos.color = new Color(spawnGizmoColor.r, spawnGizmoColor.g, spawnGizmoColor.b, alpha);
            DrawLaneBoxes(spawnZ);

            Gizmos.color = new Color(distanceGizmoColor.r, distanceGizmoColor.g, distanceGizmoColor.b, alpha);
            Vector3 playerPos = playerMovement.transform.position;
            Vector3 spawnLineCenter = new Vector3(0, gizmoHeight, spawnZ);
            Gizmos.DrawLine(playerPos, spawnLineCenter);
        }

        Gizmos.color = despawnGizmoColor;
        DrawLaneBoxes(despawnZ);
    }

    void DrawLaneBoxes(float zPos)
    {
        for (int lane = 0; lane < 3; lane++)
        {
            float laneX = (lane - 1) * laneDistance;
            Vector3 lanePos = new Vector3(laneX, gizmoHeight, zPos);
            Gizmos.DrawWireCube(lanePos, new Vector3(laneWidth, 0.5f, laneLength));
            Gizmos.DrawWireSphere(lanePos, gizmoSphereSize);
        }
    }
}