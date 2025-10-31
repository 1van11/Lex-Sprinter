using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerFunctions PlayerFunctions;

    [Header("Obstacle Prefabs")]
    public GameObject obstaclePrefab;
    public GameObject coinPrefab;

    [Header("Question Prefabs")]
    [Tooltip("Prefab for 'Correct the Spelling' questions")]
    public GameObject questionPrefab;
    [Tooltip("Prefab for 'Complete the Sentence' questions")]
    public GameObject sentencePrefab;

    [Header("Parent Transforms")]
    [Tooltip("Parent object for spawned obstacles, coins, and power-ups")]
    public Transform obstacleParent;
    [Tooltip("Parent object for spawned spelling questions")]
    public Transform questionParent;
    [Tooltip("Parent object for spawned sentence questions")]
    public Transform sentenceParent;

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

    [Header("Question Height Settings")]
    [Tooltip("Height for spelling questions")]
    public float spellingQuestionHeight = 0.5f;
    [Tooltip("Height for sentence completion questions")]
    public float sentenceQuestionHeight = 0.5f;

    [Header("Spawn Pattern")]
    [Tooltip("Pattern: 0 = obstacle, 1 = question")]
    public int[] spawnPattern = new int[] { 0, 1, 0, 1 }; // Spawns entire pattern in one go
    [Tooltip("Spacing between pattern elements")]
    public float patternSpacing = 15f;

    [Header("Question Pattern")]
    [Tooltip("Number of spelling questions before a sentence question")]
    public int spellingBeforeSentence = 3;

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

    [Tooltip("Set spawn chances (must total 100)")]
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
    public float maxQuestionLifetime = 50f;

    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color trapGizmoColor = Color.red;
    public Color questionGizmoColor = Color.green;
    public Color despawnGizmoColor = Color.blue;
    public float gizmoSphereSize = 0.5f;
    public float laneWidth = 2.5f;
    public float laneLength = 5f;
    public float gizmoHeight = 1f;

    private List<GameObject> activeObstacles = new List<GameObject>();
    private Queue<GameObject> obstaclePool = new Queue<GameObject>();
    private List<GameObject> activeCoins = new List<GameObject>();
    private Queue<GameObject> coinPool = new Queue<GameObject>();
    private List<GameObject> activePowerUps = new List<GameObject>();
    private List<GameObject> activeQuestions = new List<GameObject>();
    private Queue<GameObject> shieldPool = new Queue<GameObject>();
    private Queue<GameObject> magnetPool = new Queue<GameObject>();
    private Queue<GameObject> slowTimePool = new Queue<GameObject>();

    private float nextPowerUpDistance;
    private bool hasSpawnedFirstPowerUp = false;
    private int patternIndex = 0;
    private int spellingCounter = 0;
    private System.Random rng;

    public enum PowerUpType { Shield, Magnet, SlowTime }

    [System.Serializable]
    public class PowerUpSpawnChance
    {
        public PowerUpType type;
        [Range(0, 100)] public int chance;
    }

    private Transform ObstacleParentTransform => obstacleParent != null ? obstacleParent : transform;

    void Start()
    {
        InitializePool();
        nextPowerUpDistance = firstPowerUpDistance;
        rng = new System.Random();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab);
            obj.transform.SetParent(ObstacleParentTransform);
            obj.SetActive(false);
            obstaclePool.Enqueue(obj);
        }

        if (coinPrefab != null)
        {
            for (int i = 0; i < coinPoolSize; i++)
            {
                GameObject coin = Instantiate(coinPrefab);
                coin.transform.SetParent(ObstacleParentTransform);
                coin.SetActive(false);
                coinPool.Enqueue(coin);
            }
        }

        if (shieldPowerUpPrefab != null)
        {
            for (int i = 0; i < powerUpPoolSize; i++)
            {
                GameObject shield = Instantiate(shieldPowerUpPrefab);
                shield.transform.SetParent(ObstacleParentTransform);
                shield.SetActive(false);
                shieldPool.Enqueue(shield);
            }
        }

        if (magnetPowerUpPrefab != null)
        {
            for (int i = 0; i < powerUpPoolSize; i++)
            {
                GameObject magnet = Instantiate(magnetPowerUpPrefab);
                magnet.transform.SetParent(ObstacleParentTransform);
                magnet.SetActive(false);
                magnetPool.Enqueue(magnet);
            }
        }

        if (slowTimePowerUpPrefab != null)
        {
            for (int i = 0; i < powerUpPoolSize; i++)
            {
                GameObject slowTime = Instantiate(slowTimePowerUpPrefab);
                slowTime.transform.SetParent(ObstacleParentTransform);
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
            obj.transform.SetParent(ObstacleParentTransform);
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
            coin.transform.SetParent(ObstacleParentTransform);
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
            powerUp.transform.SetParent(ObstacleParentTransform);
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
        DespawnOldQuestions();
        CheckPowerUpSpawn();
    }

    void CheckPowerUpSpawn()
    {
        if (PlayerFunctions == null) return;
        float playerDistance = PlayerFunctions.transform.position.z;

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
        float spawnZ = PlayerFunctions.transform.position.z + powerUpSpawnAhead;

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
            powerUp.transform.localScale = prefab.transform.localScale;

            activePowerUps.Add(powerUp);
            StartCoroutine(AutoDespawnPowerUp(powerUp, maxObstacleLifetime));

            Debug.Log($"Spawned {powerUpType} power-up at {PlayerFunctions.transform.position.z}m in lane {randomLane}");
        }
    }

    List<int> GetEmptyLanesAtDistance(float distance, float checkRange)
    {
        List<int> emptyLanes = new List<int> { 0, 1, 2 };
        float checkZ = PlayerFunctions.transform.position.z + distance;

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

    public void TriggerSpawnNow()
    {
        SpawnPatternSequence();
    }

    void SpawnPatternSequence()
    {
        if (spawnPattern.Length == 0)
        {
            SpawnObstacleRows();
            return;
        }

        // Spawn the entire pattern sequence with spacing
        for (int i = 0; i < spawnPattern.Length; i++)
        {
            float zOffset = spawnDistance + (i * patternSpacing);
            
            if (spawnPattern[i] == 1) // 1 = question
            {
                SpawnSingleQuestion(zOffset);
            }
            else // 0 = obstacles
            {
                SpawnSingleObstacleRow(zOffset);
            }
        }

        Debug.Log($"Spawned complete pattern sequence with {spawnPattern.Length} elements");
    }

  void SpawnSingleObstacleRow(float zOffset)
{
    // Instead of using the prefab, reference your original obstacle in the scene
    // Assign this in the inspector
    GameObject originalObstacle = obstaclePrefab; // <-- replace with your resized object in the scene

    List<int> availableLanes = new List<int> { 0, 1, 2 };
    int obstaclesToSpawn = Mathf.Min(maxObstaclesPerRow, 3);
    obstaclesToSpawn = Mathf.Clamp(obstaclesToSpawn, 1, 2);

    for (int i = 0; i < obstaclesToSpawn; i++)
    {
        int index = Random.Range(0, availableLanes.Count);
        int lane = availableLanes[index];
        availableLanes.RemoveAt(index);

        float laneX = (lane - 1.3f) * laneDistance;
        Vector3 spawnPos = new Vector3(laneX, originalObstacle.transform.position.y, PlayerFunctions.transform.position.z + zOffset);

        // This clones the exact object, including scale and rotation
        GameObject obstacle = Instantiate(originalObstacle, spawnPos, originalObstacle.transform.rotation, ObstacleParentTransform);

        activeObstacles.Add(obstacle);
        StartCoroutine(AutoDespawnObstacle(obstacle, maxObstacleLifetime));
    }

    // Spawn coins in empty lanes
    if (coinPrefab != null)
    {
        foreach (int emptyLane in availableLanes)
        {
            float baseLaneX = (emptyLane - 1.3f) * laneDistance;
            for (int c = 0; c < coinsPerLane; c++)
            {
                float baseZPosition = PlayerFunctions.transform.position.z + zOffset;

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
                coin.transform.localScale = coinPrefab.transform.localScale;

                activeCoins.Add(coin);
                StartCoroutine(AutoDespawnCoin(coin, maxObstacleLifetime));
            }
        }
    }
}


    void SpawnSingleQuestion(float zOffset)
    {
        bool spawnSentence = false;

        if (spellingCounter >= spellingBeforeSentence)
        {
            spawnSentence = true;
            spellingCounter = 0;
        }

        // Use different heights based on question type
        float questionHeight = spawnSentence ? sentenceQuestionHeight : spellingQuestionHeight;

        // Always spawn in the middle (lane 1, which is at x=0 with the -1.3f offset calculation)
        Vector3 spawnPos = new Vector3(
            0f, // Middle lane
            questionHeight,
            PlayerFunctions.transform.position.z + zOffset
        );

        GameObject prefabToSpawn = spawnSentence ? sentencePrefab : questionPrefab;
        Transform parentToUse = spawnSentence ? sentenceParent : questionParent;

        GameObject question = Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation, parentToUse);

        QuestionRandomizer randomizer = question.GetComponent<QuestionRandomizer>();
        if (randomizer != null)
        {
            if (spawnSentence)
            {
                int randomIndex = rng.Next(0, 20);
                randomizer.SetSentenceQuestion(randomIndex);
                Debug.Log($"Spawned sentence question at Z: {spawnPos.z}, index: {randomIndex}");
            }
            else
            {
                int randomIndex = rng.Next(0, 55);
                randomizer.SetSpellingQuestion(randomIndex);
                spellingCounter++;
                Debug.Log($"Spawned spelling question at Z: {spawnPos.z}, index: {randomIndex}");
            }
        }

        activeQuestions.Add(question);
        StartCoroutine(AutoDespawnQuestion(question, maxQuestionLifetime));
    }

    // Legacy method for backward compatibility
    public void SpawnObstacleRows()
    {
        for (int row = 0; row < rowsPerSpawn; row++)
        {
            float zOffset = spawnDistance + (row * rowSpacing);
            SpawnSingleObstacleRow(zOffset);
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

    IEnumerator AutoDespawnQuestion(GameObject question, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (question != null)
        {
            activeQuestions.Remove(question);
            Destroy(question);
        }
    }

    void DespawnOldObstacles()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null || !activeObstacles[i].activeSelf)
                activeObstacles.RemoveAt(i);
            else if (activeObstacles[i].transform.position.z < PlayerFunctions.transform.position.z - despawnDistance)
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
            else if (activeCoins[i].transform.position.z < PlayerFunctions.transform.position.z - despawnDistance)
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
            else if (activePowerUps[i].transform.position.z < PlayerFunctions.transform.position.z - despawnDistance)
            {
                GameObject powerUp = activePowerUps[i];
                activePowerUps.RemoveAt(i);
                ReturnPowerUpToPool(powerUp);
            }
        }
    }

    void DespawnOldQuestions()
    {
        for (int i = activeQuestions.Count - 1; i >= 0; i--)
        {
            GameObject q = activeQuestions[i];
            if (q == null)
            {
                activeQuestions.RemoveAt(i);
                continue;
            }

            float distanceFromPlayer = PlayerFunctions.transform.position.z - q.transform.position.z;
            if (distanceFromPlayer > despawnDistance)
            {
                Destroy(q);
                activeQuestions.RemoveAt(i);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || PlayerFunctions == null) return;

        float despawnZ = PlayerFunctions.transform.position.z - despawnDistance;

        // Draw pattern spawn zones
        if (spawnPattern != null && spawnPattern.Length > 0)
        {
            for (int i = 0; i < spawnPattern.Length; i++)
            {
                float spawnZ = PlayerFunctions.transform.position.z + spawnDistance + (i * patternSpacing);
                float alpha = Mathf.Clamp01(1f - (i * 0.2f));

                if (spawnPattern[i] == 1) // Question
                {
                    Gizmos.color = new Color(questionGizmoColor.r, questionGizmoColor.g, questionGizmoColor.b, alpha);
                    
                    // Show both spelling and sentence question heights
                    Vector3 spellingPos = new Vector3(0f, spellingQuestionHeight, spawnZ);
                    Gizmos.DrawWireSphere(spellingPos, gizmoSphereSize * 1.5f);
                    
                    Vector3 sentencePos = new Vector3(0f, sentenceQuestionHeight, spawnZ);
                    Gizmos.DrawWireSphere(sentencePos, gizmoSphereSize * 1.5f);
                }
                else // Obstacle
                {
                    Gizmos.color = new Color(trapGizmoColor.r, trapGizmoColor.g, trapGizmoColor.b, alpha);
                    DrawLaneBoxes(spawnZ);
                }
            }
        }

        // Draw despawn zone
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