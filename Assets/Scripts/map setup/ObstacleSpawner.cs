using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public GameObject obstaclePrefab;
    public GameObject coinPrefab;

    [Header("Parent Transform")]
    [Tooltip("Parent object for spawned obstacles, coins, and power-ups. Leave empty to use this GameObject.")]
    public Transform spawnParent;

    [Header("Power-Up Prefabs")]
    public GameObject shieldPowerUpPrefab;
    public GameObject magnetPowerUpPrefab;
    public GameObject slowTimePowerUpPrefab;

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
    public float firstPowerUpDistance = 500f;
    public float powerUpInterval = 200f;
    public Vector3 powerUpPositionOffset = Vector3.zero;
    public Vector3 powerUpScale = Vector3.one;
    public float powerUpSpawnAhead = 20f;

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
    public int powerUpPoolSize = 5;

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

    private List<GameObject> activePowerUps = new List<GameObject>();
    private Queue<GameObject> shieldPool = new Queue<GameObject>();
    private Queue<GameObject> magnetPool = new Queue<GameObject>();
    private Queue<GameObject> slowTimePool = new Queue<GameObject>();

    private float nextPowerUpDistance;
    private bool hasSpawnedFirstPowerUp = false;

    public float iFrameDuration = 2f;
    public float flashInterval = 0.2f;
    private bool triggered = false;

    public enum PowerUpType { Shield, Magnet, SlowTime }

    [System.Serializable]
    public class PowerUpSpawnChance
    {
        public PowerUpType type;
        [Range(0, 100)] public int chance;
    }

    // Helper property to get the parent transform
    private Transform ParentTransform
    {
        get { return spawnParent != null ? spawnParent : transform; }
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
            obj.transform.SetParent(ParentTransform);
            obj.SetActive(false);
            obstaclePool.Enqueue(obj);
        }

        if (coinPrefab != null)
        {
            for (int i = 0; i < coinPoolSize; i++)
            {
                GameObject coin = Instantiate(coinPrefab);
                coin.transform.SetParent(ParentTransform);
                coin.SetActive(false);
                coinPool.Enqueue(coin);
            }
        }

        if (shieldPowerUpPrefab != null)
        {
            for (int i = 0; i < powerUpPoolSize; i++)
            {
                GameObject shield = Instantiate(shieldPowerUpPrefab);
                shield.transform.SetParent(ParentTransform);
                shield.SetActive(false);
                shieldPool.Enqueue(shield);
            }
        }

        if (magnetPowerUpPrefab != null)
        {
            for (int i = 0; i < powerUpPoolSize; i++)
            {
                GameObject magnet = Instantiate(magnetPowerUpPrefab);
                magnet.transform.SetParent(ParentTransform);
                magnet.SetActive(false);
                magnetPool.Enqueue(magnet);
            }
        }

        if (slowTimePowerUpPrefab != null)
        {
            for (int i = 0; i < powerUpPoolSize; i++)
            {
                GameObject slowTime = Instantiate(slowTimePowerUpPrefab);
                slowTime.transform.SetParent(ParentTransform);
                slowTime.SetActive(false);
                slowTimePool.Enqueue(slowTime);
            }
        }
    }

    GameObject GetPooledObstacle()
    {
        GameObject obj;
        if (obstaclePool.Count > 0)
        {
            obj = obstaclePool.Dequeue();
        }
        else
        {
            obj = Instantiate(obstaclePrefab);
            obj.transform.SetParent(ParentTransform);
        }
        obj.SetActive(true);
        return obj;
    }

    GameObject GetPooledCoin()
    {
        GameObject coin;
        if (coinPool.Count > 0)
        {
            coin = coinPool.Dequeue();
        }
        else
        {
            coin = Instantiate(coinPrefab);
            coin.transform.SetParent(ParentTransform);
        }
        coin.SetActive(true);
        return coin;
    }

    GameObject GetPooledPowerUp(PowerUpType type)
    {
        Queue<GameObject> pool = null;
        GameObject prefab = null;

        switch (type)
        {
            case PowerUpType.Shield: pool = shieldPool; prefab = shieldPowerUpPrefab; break;
            case PowerUpType.Magnet: pool = magnetPool; prefab = magnetPowerUpPrefab; break;
            case PowerUpType.SlowTime: pool = slowTimePool; prefab = slowTimePowerUpPrefab; break;
        }

        GameObject powerUp;
        if (pool != null && pool.Count > 0)
        {
            powerUp = pool.Dequeue();
        }
        else if (prefab != null)
        {
            powerUp = Instantiate(prefab);
            powerUp.transform.SetParent(ParentTransform);
        }
        else
        {
            return null;
        }

        powerUp.SetActive(true);
        return powerUp;
    }

    void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false); // FIXED: Was true, should be false
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

        if (shieldPowerUpPrefab != null && powerUp.name.Contains(shieldPowerUpPrefab.name))
            shieldPool.Enqueue(powerUp);
        else if (magnetPowerUpPrefab != null && powerUp.name.Contains(magnetPowerUpPrefab.name))
            magnetPool.Enqueue(powerUp);
        else if (slowTimePowerUpPrefab != null && powerUp.name.Contains(slowTimePowerUpPrefab.name))
            slowTimePool.Enqueue(powerUp);
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
            else nextPowerUpDistance += powerUpInterval;
        }
    }

    PowerUpType GetRandomPowerUpType()
    {
        if (spawnChances == null || spawnChances.Length == 0)
            return (PowerUpType)Random.Range(0, 3);

        int totalChance = 0;
        foreach (var sc in spawnChances)
            totalChance += sc.chance;

        int random = Random.Range(0, totalChance);
        int cumulative = 0;

        foreach (var sc in spawnChances)
        {
            cumulative += sc.chance;
            if (random < cumulative) return sc.type;
        }

        return PowerUpType.Shield;
    }

    void SpawnPowerUp()
{
    PowerUpType powerUpType = GetRandomPowerUpType();
    GameObject prefab = null;

    switch (powerUpType)
    {
        case PowerUpType.Shield: prefab = shieldPowerUpPrefab; break;
        case PowerUpType.Magnet: prefab = magnetPowerUpPrefab; break;
        case PowerUpType.SlowTime: prefab = slowTimePowerUpPrefab; break;
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

        // ✅ Copy the exact look of the prefab
        powerUp.transform.rotation = prefab.transform.rotation;
        powerUp.transform.localScale = prefab.transform.localScale;

        activePowerUps.Add(powerUp);
        StartCoroutine(AutoDespawnPowerUp(powerUp, maxObstacleLifetime));

        Debug.Log($"Spawned {powerUpType} power-up at {playerMovement.transform.position.z}m in lane {randomLane}");
    }
}

    List<int> GetEmptyLanesAtDistance(float distance, float checkRange)
    {
        List<int> emptyLanes = new List<int> { 0, 1, 2 };
        float checkZ = playerMovement.transform.position.z + distance;

        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle == null || !obstacle.activeSelf) continue;
            if (Mathf.Abs(obstacle.transform.position.z - checkZ) < checkRange)
                emptyLanes.Remove(GetLaneFromPosition(obstacle.transform.position.x));
        }

        foreach (GameObject coin in activeCoins)
        {
            if (coin == null || !coin.activeSelf) continue;
            if (Mathf.Abs(coin.transform.position.z - checkZ) < checkRange)
                emptyLanes.Remove(GetLaneFromPosition(coin.transform.position.x));
        }

        return emptyLanes;
    }

    int GetLaneFromPosition(float xPosition)
    {
        float normalizedX = (xPosition / laneDistance) + 1.3f;
        int lane = Mathf.RoundToInt(normalizedX);
        return Mathf.Clamp(lane, 0, 2);
    }

    public void TriggerSpawnNow() => SpawnObstacleRows();

    public void SpawnObstacleRows()
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

    for (int i = 0; i < obstaclesToSpawn; i++)
    {
        int index = Random.Range(0, availableLanes.Count);
        int lane = availableLanes[index];
        availableLanes.RemoveAt(index);

        float laneX = (lane - 1.3f) * laneDistance;
        Vector3 spawnPos = new Vector3(laneX, spawnHeight, playerMovement.transform.position.z + zOffset);

        GameObject obstacle = GetPooledObstacle();
        obstacle.transform.position = spawnPos;

        // ✅ Copy your prefab's exact rotation and scale
        obstacle.transform.rotation = obstaclePrefab.transform.rotation;
        obstacle.transform.localScale = obstaclePrefab.transform.localScale;

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

                // ✅ Copy exact rotation + scale from prefab
                coin.transform.rotation = coinPrefab.transform.rotation;
                coin.transform.localScale = coinPrefab.transform.localScale;

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
                activeObstacles.RemoveAt(i);
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
                activeCoins.RemoveAt(i);
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
                activePowerUps.RemoveAt(i);
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
            float alpha = Mathf.Clamp01(1f - (row * 0.3f));

            Gizmos.color = new Color(spawnGizmoColor.r, spawnGizmoColor.g, spawnGizmoColor.b, alpha);
            DrawLaneBoxes(spawnZ);

            Gizmos.color = new Color(distanceGizmoColor.r, distanceGizmoColor.g, distanceGizmoColor.b, alpha);
            Vector3 playerPos = playerMovement.transform.position;
            Vector3 spawnLineCenter = new Vector3(0, gizmoHeight, spawnZ);
            Gizmos.DrawLine(playerPos, spawnLineCenter);
        }

        Gizmos.color = despawnGizmoColor;
        DrawLaneBoxes(despawnZ);

        float powerUpZ = nextPowerUpDistance + powerUpSpawnAhead - playerMovement.transform.position.z + playerMovement.transform.position.z;

        for (int lane = 0; lane < 3; lane++)
        {
            float laneX = (lane - 1.3f) * laneDistance;
            Vector3 basePos = new Vector3(laneX, spawnHeight + 1f, powerUpZ);

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