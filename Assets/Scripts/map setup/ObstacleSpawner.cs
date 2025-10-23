using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public GameObject obstaclePrefab;
    public GameObject coinPrefab;
    
    [Header("Power-Up Prefabs")]
    public GameObject shieldPowerUpPrefab; // Shield power-up
    public GameObject magnetPowerUpPrefab; // Magnet power-up
    public GameObject slowTimePowerUpPrefab; // Slow time power-up

    [Header("Spawn Settings")]
    public float spawnDistance = 20f;
    public int rowsPerSpawn = 2;
    public float rowSpacing = 10f;
    public int maxObstaclesPerRow = 2;
    public float spawnHeight = -1f;
    public float laneDistance = 3f;

    [Header("Coin Settings")]
    public Vector3 coinPositionOffset = Vector3.zero;
    public Vector3 coinScale = Vector3.one;
    public int coinsPerLane = 1;
    public float coinSpacing = 2f;

    [Header("Power-Up Settings")]
    public float firstPowerUpDistance = 500f; // First power-up at 500m
    public float powerUpInterval = 200f; // Every 200m after that
    public Vector3 powerUpPositionOffset = Vector3.zero;
    public Vector3 powerUpScale = Vector3.one;
    public float powerUpSpawnAhead = 20f; // Spawn ahead of player
    [Tooltip("Set spawn chances (must total 100). If empty, spawns randomly.")]
    public PowerUpSpawnChance[] spawnChances = new PowerUpSpawnChance[]
    {
        new PowerUpSpawnChance { type = PowerUpType.Shield, chance = 33 },
        new PowerUpSpawnChance { type = PowerUpType.Magnet, chance = 33 },
        new PowerUpSpawnChance { type = PowerUpType.SlowTime, chance = 34 }
    };

    [Header("Object Pooling")]
    public int poolSize = 20;
    public int coinPoolSize = 30;
    public int powerUpPoolSize = 5; // Pool size per power-up type

    [Header("Despawn Settings")]
    public float despawnDistance = 10f;
    public float maxObstacleLifetime = 15f;

    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color spawnGizmoColor = Color.red;
    public Color despawnGizmoColor = Color.blue;
    public Color distanceGizmoColor = Color.yellow;
    public Color shieldGizmoColor = Color.cyan;
    public Color magnetGizmoColor = Color.yellow;
    public Color slowTimeGizmoColor = Color.magenta;
    public float gizmoSphereSize = 0.5f;
    public float laneWidth = 2.5f;
    public float laneLength = 5f;
    public float gizmoHeight = 1f;

    private List<GameObject> activeObstacles = new List<GameObject>();
    private Queue<GameObject> obstaclePool = new Queue<GameObject>();
    private List<GameObject> activeCoins = new List<GameObject>();
    private Queue<GameObject> coinPool = new Queue<GameObject>();
    
    // Separate pools for each power-up type
    private List<GameObject> activePowerUps = new List<GameObject>();
    private Queue<GameObject> shieldPool = new Queue<GameObject>();
    private Queue<GameObject> magnetPool = new Queue<GameObject>();
    private Queue<GameObject> slowTimePool = new Queue<GameObject>();

    private float nextPowerUpDistance;
    private bool hasSpawnedFirstPowerUp = false;

    public float iFrameDuration = 2f;
    public float flashInterval = 0.2f;
    private bool triggered = false;

    public enum PowerUpType
    {
        Shield,
        Magnet,
        SlowTime
    }

    [System.Serializable]
    public class PowerUpSpawnChance
    {
        public PowerUpType type;
        [Range(0, 100)]
        public int chance;
    }

    void Start()
    {
        InitializePool();
        nextPowerUpDistance = firstPowerUpDistance;
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab);
            obj.SetActive(false);
            obstaclePool.Enqueue(obj);
        }

        if (coinPrefab != null)
        {
            for (int i = 0; i < coinPoolSize; i++)
            {
                GameObject coin = Instantiate(coinPrefab);
                coin.SetActive(false);
                coinPool.Enqueue(coin);
            }
        }

        // Initialize power-up pools
        if (shieldPowerUpPrefab != null)
        {
            for (int i = 0; i < powerUpPoolSize; i++)
            {
                GameObject shield = Instantiate(shieldPowerUpPrefab);
                shield.SetActive(false);
                shieldPool.Enqueue(shield);
            }
        }

        if (magnetPowerUpPrefab != null)
        {
            for (int i = 0; i < powerUpPoolSize; i++)
            {
                GameObject magnet = Instantiate(magnetPowerUpPrefab);
                magnet.SetActive(false);
                magnetPool.Enqueue(magnet);
            }
        }

        if (slowTimePowerUpPrefab != null)
        {
            for (int i = 0; i < powerUpPoolSize; i++)
            {
                GameObject slowTime = Instantiate(slowTimePowerUpPrefab);
                slowTime.SetActive(false);
                slowTimePool.Enqueue(slowTime);
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

    GameObject GetPooledPowerUp(PowerUpType type)
    {
        Queue<GameObject> pool = null;
        GameObject prefab = null;

        switch (type)
        {
            case PowerUpType.Shield:
                pool = shieldPool;
                prefab = shieldPowerUpPrefab;
                break;
            case PowerUpType.Magnet:
                pool = magnetPool;
                prefab = magnetPowerUpPrefab;
                break;
            case PowerUpType.SlowTime:
                pool = slowTimePool;
                prefab = slowTimePowerUpPrefab;
                break;
        }

        if (pool != null && pool.Count > 0)
        {
            GameObject powerUp = pool.Dequeue();
            powerUp.SetActive(true);
            return powerUp;
        }
        else if (prefab != null)
        {
            GameObject powerUp = Instantiate(prefab);
            return powerUp;
        }

        return null;
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

    void ReturnPowerUpToPool(GameObject powerUp)
    {
        powerUp.SetActive(false);

        // Determine which pool to return to based on prefab
        if (shieldPowerUpPrefab != null && powerUp.name.Contains(shieldPowerUpPrefab.name))
        {
            shieldPool.Enqueue(powerUp);
        }
        else if (magnetPowerUpPrefab != null && powerUp.name.Contains(magnetPowerUpPrefab.name))
        {
            magnetPool.Enqueue(powerUp);
        }
        else if (slowTimePowerUpPrefab != null && powerUp.name.Contains(slowTimePowerUpPrefab.name))
        {
            slowTimePool.Enqueue(powerUp);
        }
    }

    void Update()
    {
        DespawnOldObstacles();
        DespawnOldCoins();
        DespawnOldPowerUps();
        CheckPowerUpSpawn();
    }

    void CheckPowerUpSpawn()
    {
        if (playerMovement == null) return;

        float playerDistance = playerMovement.transform.position.z;

        if (playerDistance >= nextPowerUpDistance)
        {
            SpawnPowerUp();

            if (!hasSpawnedFirstPowerUp)
            {
                hasSpawnedFirstPowerUp = true;
                nextPowerUpDistance = firstPowerUpDistance + powerUpInterval;
            }
            else
            {
                nextPowerUpDistance += powerUpInterval;
            }
        }
    }

    PowerUpType GetRandomPowerUpType()
    {
        // If no spawn chances defined, return random type
        if (spawnChances == null || spawnChances.Length == 0)
        {
            return (PowerUpType)Random.Range(0, 3);
        }

        // Calculate total chance
        int totalChance = 0;
        foreach (var sc in spawnChances)
        {
            totalChance += sc.chance;
        }

        // Pick random value
        int random = Random.Range(0, totalChance);
        int cumulative = 0;

        // Find which type was selected
        foreach (var sc in spawnChances)
        {
            cumulative += sc.chance;
            if (random < cumulative)
            {
                return sc.type;
            }
        }

        // Fallback
        return PowerUpType.Shield;
    }

    void SpawnPowerUp()
    {
        // Get random power-up type
        PowerUpType powerUpType = GetRandomPowerUpType();

        // Check if prefab exists
        GameObject prefab = null;
        switch (powerUpType)
        {
            case PowerUpType.Shield:
                prefab = shieldPowerUpPrefab;
                break;
            case PowerUpType.Magnet:
                prefab = magnetPowerUpPrefab;
                break;
            case PowerUpType.SlowTime:
                prefab = slowTimePowerUpPrefab;
                break;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"Power-up prefab for {powerUpType} not assigned!");
            return;
        }

        List<int> emptyLanes = GetEmptyLanesAtDistance(powerUpSpawnAhead, 5f);

        if (emptyLanes.Count == 0)
        {
            Debug.LogWarning("No empty lanes for power-up!");
            return;
        }

        int randomLane = emptyLanes[Random.Range(0, emptyLanes.Count)];
        float laneX = (randomLane - 1.3f) * laneDistance;
        float spawnZ = playerMovement.transform.position.z + powerUpSpawnAhead;

        Vector3 spawnPos = new Vector3(
            laneX + powerUpPositionOffset.x,
            spawnHeight + powerUpPositionOffset.y,
            spawnZ + powerUpPositionOffset.z
        );

        GameObject powerUp = GetPooledPowerUp(powerUpType);
        if (powerUp != null)
        {
            powerUp.transform.position = spawnPos;
            powerUp.transform.rotation = prefab.transform.rotation;
            powerUp.transform.localScale = powerUpScale;

            activePowerUps.Add(powerUp);
            StartCoroutine(AutoDespawnPowerUp(powerUp, maxObstacleLifetime));

            Debug.Log($"Spawned {powerUpType} power-up at {playerMovement.transform.position.z}m in lane {randomLane}");
        }
    }

    List<int> GetEmptyLanesAtDistance(float distance, float checkRange)
    {
        List<int> emptyLanes = new List<int> { 0, 1, 2 };
        float checkZ = playerMovement.transform.position.z + distance;

        // Check obstacles
        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle == null || !obstacle.activeSelf) continue;
            if (Mathf.Abs(obstacle.transform.position.z - checkZ) < checkRange)
            {
                int lane = GetLaneFromPosition(obstacle.transform.position.x);
                emptyLanes.Remove(lane);
            }
        }

        // Check coins
        foreach (GameObject coin in activeCoins)
        {
            if (coin == null || !coin.activeSelf) continue;
            if (Mathf.Abs(coin.transform.position.z - checkZ) < checkRange)
            {
                int lane = GetLaneFromPosition(coin.transform.position.x);
                emptyLanes.Remove(lane);
            }
        }

        return emptyLanes;
    }

    int GetLaneFromPosition(float xPosition)
    {
        float normalizedX = (xPosition / laneDistance) + 1.3f;
        int lane = Mathf.RoundToInt(normalizedX);
        return Mathf.Clamp(lane, 0, 2);
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

        if (coinPrefab != null)
        {
            foreach (int emptyLane in availableLanes)
            {
                float baseLaneX = (emptyLane - 1.3f) * laneDistance;
                
                for (int c = 0; c < coinsPerLane; c++)
                {
                    float baseZPosition = playerMovement.transform.position.z + zOffset;
                    
                    if (coinsPerLane > 1)
                    {
                        float totalSpacing = (coinsPerLane - 1) * coinSpacing;
                        float startOffset = -totalSpacing / 2f;
                        baseZPosition += startOffset + (c * coinSpacing);
                    }
                    
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

    IEnumerator AutoDespawnPowerUp(GameObject powerUp, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (powerUp != null && powerUp.activeSelf)
        {
            activePowerUps.Remove(powerUp);
            ReturnPowerUpToPool(powerUp);
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

    void DespawnOldPowerUps()
    {
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            if (activePowerUps[i] == null || !activePowerUps[i].activeSelf)
            {
                activePowerUps.RemoveAt(i);
            }
            else if (activePowerUps[i].transform.position.z < playerMovement.transform.position.z - despawnDistance)
            {
                GameObject powerUp = activePowerUps[i];
                activePowerUps.RemoveAt(i);
                ReturnPowerUpToPool(powerUp);
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

        // Draw power-up spawn points with different colors
        float powerUpZ = nextPowerUpDistance + powerUpSpawnAhead - playerMovement.transform.position.z + playerMovement.transform.position.z;
        
        for (int lane = 0; lane < 3; lane++)
        {
            float laneX = (lane - 1.3f) * laneDistance;
            Vector3 basePos = new Vector3(laneX, spawnHeight + 1f, powerUpZ);
            
            // Draw three colored spheres for each power-up type
            Gizmos.color = shieldGizmoColor;
            Gizmos.DrawWireSphere(basePos + Vector3.left * 0.3f, gizmoSphereSize);
            
            Gizmos.color = magnetGizmoColor;
            Gizmos.DrawWireSphere(basePos, gizmoSphereSize);
            
            Gizmos.color = slowTimeGizmoColor;
            Gizmos.DrawWireSphere(basePos + Vector3.right * 0.3f, gizmoSphereSize);
        }
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