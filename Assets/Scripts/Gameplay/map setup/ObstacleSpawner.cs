using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerFunctions PlayerFunctions;

    [Header("Obstacle Prefabs")]
    public GameObject[] obstaclePrefabs;
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
    public int[] spawnPattern = new int[] { 0, 1, 0, 1 };
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
    [Tooltip("Maximum number of power-ups to spawn (0 = unlimited)")]
    public int maxPowerUpCount = 1;
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

    [Header("Letter Container Settings")]
    public GameObject letterContainerPrefab;
    public GameObject spawnerIndicatorPrefab;
    public Transform letterSpawnParent;
    
    [Header("Letter Container Spawn Placement")]
    public float letterSpawnDistanceAhead = 20f;
    public float letterSpawnInterval = 10f;
    public float letterSpawnHeight = 0.5f;
    public Vector3 letterSpawnPositionOffset = Vector3.zero;
    public Vector3 letterSpawnerOffset = Vector3.zero;
    
    [Header("Letter Container Initial Delay")]
    public float letterInitialSpawnDelaySeconds = 1f;
    
    [Header("Letter Event Spawn")]
    public float letterEventFrequency = 60f;
    public float letterEventDuration = 40f;
    public float letterEventInitialDelay = 10f;
    public float letterEventSpawnDistanceAhead = 150f;
    [Tooltip("Delay before spawning indicator after event starts (gives player time to reach spawn point)")]
    public float letterEventIndicatorDelay = 3f;
    [Tooltip("Delay after indicator spawns before letter hurdles appear")]
    public float letterHurdleSpawnDelay = 2f;
    
    [Header("Letter Indicator Settings")]
    public float letterIndicatorSpawnDistanceAhead = 200f;
    [Tooltip("Minimum guaranteed distance ahead (overrides calculation if needed)")]
    public float letterIndicatorMinimumSpawnAhead = 150f;
    public float letterIndicatorAnimationTime = 2f;
    public float letterIndicatorStayTime = 5f;
    public float letterIndicatorStartHeightOffset = 10f;
    
    [Header("Letter Prefab Transform")]
    public bool letterUsePrefabTransform = true;
    public Vector3 letterSpawnRotationEuler = Vector3.zero;
    public Vector3 letterSpawnScale = Vector3.one;
    
    [Header("Letter Pooling Settings")]
    public int letterPoolSize = 50;
    public float letterDespawnTime = 5f;
    
    [Header("Letter Event Damage")]
    public int letterHurdleDamage = 1;
    
    [Header("Letter Event UI")]
    public GameObject letterEventClueUI;
    public float clueTextDelay = 2f;

    [Header("Letter Event Word Limit")]
    [Tooltip("Number of words to solve before ending the event (set to 1 for single word)")]
    public int wordsToSolvePerEvent = 1;

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
    private List<GameObject> activeCoins = new List<GameObject>();
    private List<GameObject> activePowerUps = new List<GameObject>();
    private List<GameObject> activeQuestions = new List<GameObject>();

    private float nextPowerUpDistance;
    private bool hasSpawnedFirstPowerUp = false;
    private int patternIndex = 0;
    private int spellingCounter = 0;
    private int totalPowerUpsSpawned = 0;
    private System.Random rng;

    private Queue<GameObject> letterPool = new Queue<GameObject>();
    private List<LetterSpawnedInfo> activeLetterObjects = new List<LetterSpawnedInfo>();
    private bool allowRegularLetterSpawning = false;
    private float nextLetterSpawnZ;
    private bool isLetterEventActive = false;
    private GameObject currentEventIndicator = null;
    private Coroutine letterEventCoroutine;
    private int wordsCompletedInCurrentEvent = 0;

    public enum PowerUpType { Shield, Magnet, SlowTime }

    [System.Serializable]
    public class PowerUpSpawnChance
    {
        public PowerUpType type;
        [Range(0, 100)] public int chance;
    }

    class LetterSpawnedInfo
    {
        public GameObject obj;
        public float timer;
    }

    private Transform ObstacleParentTransform => obstacleParent != null ? obstacleParent : transform;
    private Transform LetterSpawnParentTransform => letterSpawnParent != null ? letterSpawnParent : transform;

    public bool IsLetterEventActive => isLetterEventActive;

    void Start()
    {
        nextPowerUpDistance = firstPowerUpDistance;
        rng = new System.Random();
        totalPowerUpsSpawned = 0;
        
        CreateLetterPool();
        nextLetterSpawnZ = PlayerFunctions.transform.position.z + letterSpawnDistanceAhead + 0.01f;
        
        allowRegularLetterSpawning = false;
        
        if (letterEventClueUI != null)
            letterEventClueUI.SetActive(false);
        
        letterEventCoroutine = StartCoroutine(LetterEventSpawner());
    }

    void Update()
    {
        if (!isLetterEventActive)
        {
            DespawnOldObstacles();
            DespawnOldCoins();
            DespawnOldPowerUps();
            DespawnOldQuestions();
            CheckPowerUpSpawn();
        }
        
        if (isLetterEventActive && allowRegularLetterSpawning && PlayerFunctions.transform.position.z + letterSpawnDistanceAhead >= nextLetterSpawnZ)
        {
            SpawnRandomLetterLaneAtZ(nextLetterSpawnZ);
            nextLetterSpawnZ += letterSpawnInterval;
        }

        for (int i = activeLetterObjects.Count - 1; i >= 0; i--)
        {
            activeLetterObjects[i].timer += Time.deltaTime;
            if (activeLetterObjects[i].timer >= letterDespawnTime)
            {
                ReturnLetterToPool(activeLetterObjects[i].obj);
                activeLetterObjects.RemoveAt(i);
            }
        }
    }

    #region Letter Container System

    void CreateLetterPool()
    {
        if (letterContainerPrefab == null) return;
        
        for (int i = 0; i < letterPoolSize; i++)
        {
            GameObject o = Instantiate(letterContainerPrefab, Vector3.zero, Quaternion.identity, LetterSpawnParentTransform);
            o.SetActive(false);
            letterPool.Enqueue(o);
        }
    }

    GameObject GetLetterFromPool()
    {
        if (letterPool.Count > 0)
            return letterPool.Dequeue();

        GameObject o = Instantiate(letterContainerPrefab, Vector3.zero, Quaternion.identity, LetterSpawnParentTransform);
        return o;
    }

    void ReturnLetterToPool(GameObject obj)
    {
        obj.SetActive(false);
        letterPool.Enqueue(obj);
    }

    void SpawnRandomLetterLaneAtZ(float z)
    {
        int lane = Random.Range(-1, 2);
        float x = lane * laneDistance;
        SpawnLetterFromPool(new Vector3(x, letterSpawnHeight, z) + letterSpawnPositionOffset + letterSpawnerOffset);
    }

    void SpawnLetterFromPool(Vector3 pos)
    {
        GameObject go = GetLetterFromPool();
        go.SetActive(true);

        Quaternion rot = letterUsePrefabTransform ?
            letterContainerPrefab.transform.rotation :
            Quaternion.Euler(letterSpawnRotationEuler);

        Vector3 scale = letterUsePrefabTransform ?
            letterContainerPrefab.transform.localScale :
            letterSpawnScale;

        go.transform.SetPositionAndRotation(pos, rot);
        go.transform.localScale = scale;

        activeLetterObjects.Add(new LetterSpawnedInfo() { obj = go, timer = 0f });
    }

    void SpawnEventLetterHurdlesAtZ(float z)
    {
        int count = Random.Range(1, 3);
        List<int> lanes = new List<int>() { -1, 0, 1 };

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, lanes.Count);
            int lane = lanes[idx];
            lanes.RemoveAt(idx);

            float x = lane * laneDistance;
            SpawnLetterFromPool(new Vector3(x, letterSpawnHeight, z) + letterSpawnPositionOffset + letterSpawnerOffset);
        }
        
        Debug.Log($"📦 Spawned {count} letter hurdles at Z: {z}");
    }

    void SpawnLetterIndicatorAtZ(float z)
    {
        if (spawnerIndicatorPrefab == null) 
        {
            Debug.LogWarning("⚠️ Spawner Indicator Prefab is missing!");
            StartCoroutine(DelayedLetterHurdleSpawn(z));
            return;
        }
        
        float x = 0f;
        float startY = letterSpawnHeight + letterIndicatorStartHeightOffset;
        Vector3 pos = new Vector3(x, startY, z) + letterSpawnPositionOffset + letterSpawnerOffset;
        currentEventIndicator = Instantiate(spawnerIndicatorPrefab, pos, Quaternion.identity, LetterSpawnParentTransform);
        
        Debug.Log($"🎯 Spawned indicator at Z: {z} (Player Z: {PlayerFunctions.transform.position.z})");
        
        StartCoroutine(AnimateLetterIndicator(currentEventIndicator, z));
    }

    IEnumerator DelayedLetterHurdleSpawn(float z)
    {
        yield return new WaitForSeconds(letterHurdleSpawnDelay);
        SpawnEventLetterHurdlesAtZ(z);
    }

    IEnumerator AnimateLetterIndicator(GameObject indicator, float z)
    {
        Vector3 startPos = indicator.transform.position;
        Vector3 downPos = new Vector3(startPos.x, letterSpawnHeight, startPos.z);

        float time = 0f;
        while (time < letterIndicatorAnimationTime)
        {
            if (indicator == null) yield break;
            indicator.transform.position = Vector3.Lerp(startPos, downPos, time / letterIndicatorAnimationTime);
            time += Time.deltaTime;
            yield return null;
        }
        
        if (indicator != null)
            indicator.transform.position = downPos;

        yield return new WaitForSeconds(letterHurdleSpawnDelay);
        
        SpawnEventLetterHurdlesAtZ(z);

        yield return new WaitForSeconds(letterIndicatorStayTime);

        if (indicator == null) yield break;
        
        Vector3 upPos = startPos;
        time = 0f;
        while (time < letterIndicatorAnimationTime)
        {
            if (indicator == null) yield break;
            indicator.transform.position = Vector3.Lerp(downPos, upPos, time / letterIndicatorAnimationTime);
            time += Time.deltaTime;
            yield return null;
        }

        if (indicator != null)
        {
            Destroy(indicator);
            currentEventIndicator = null;
        }
    }

    IEnumerator LetterEventSpawner()
    {
        yield return new WaitForSeconds(letterEventInitialDelay);

        while (true)
        {
            yield return new WaitForSeconds(letterEventFrequency);

            isLetterEventActive = true;
            wordsCompletedInCurrentEvent = 0;
            allowRegularLetterSpawning = false;
            
            Debug.Log($"🔤 Letter Event Started - Player at Z: {PlayerFunctions.transform.position.z}");
            
            float z = CalculateFarAheadSpawnPosition();
            Debug.Log($"📍 Indicator will spawn at Z: {z} (Distance ahead: {z - PlayerFunctions.transform.position.z})");
            
            yield return new WaitForSeconds(letterEventIndicatorDelay);
            
            SpawnLetterIndicatorAtZ(z);
            
            if (letterEventClueUI != null)
            {
                yield return new WaitForSeconds(clueTextDelay);
                if (isLetterEventActive)
                    letterEventClueUI.SetActive(true);
            }

            yield return new WaitForSeconds(letterInitialSpawnDelaySeconds);
            
            if (isLetterEventActive)
            {
                allowRegularLetterSpawning = true;
                nextLetterSpawnZ = PlayerFunctions.transform.position.z + letterSpawnDistanceAhead;
                Debug.Log($"✅ Regular letter spawning enabled at Z: {nextLetterSpawnZ}");
            }

            yield return new WaitForSeconds(letterEventDuration - letterEventIndicatorDelay - letterInitialSpawnDelaySeconds);

            if (isLetterEventActive)
            {
                EndLetterEvent();
            }
        }
    }

    float CalculateFarAheadSpawnPosition()
    {
        if (PlayerFunctions == null) 
            return letterIndicatorMinimumSpawnAhead;

        float currentPlayerZ = PlayerFunctions.transform.position.z;
        
        float calculatedDistance = Mathf.Max(
            letterEventSpawnDistanceAhead, 
            letterIndicatorSpawnDistanceAhead, 
            letterIndicatorMinimumSpawnAhead
        );
        
        float finalSpawnZ = currentPlayerZ + calculatedDistance;
        
        Debug.Log($"🎯 Spawn calculation: PlayerZ={currentPlayerZ:F1}, Distance={calculatedDistance:F1}, FinalZ={finalSpawnZ:F1}");
        
        return finalSpawnZ;
    }

    public void EndLetterEvent()
    {
        isLetterEventActive = false;
        allowRegularLetterSpawning = false;
        wordsCompletedInCurrentEvent = 0;
        
        Debug.Log("✅ Letter Event Ended - Normal spawning resumes");
        
        if (letterEventClueUI != null)
            letterEventClueUI.SetActive(false);
        
        if (letterEventCoroutine != null)
            StopCoroutine(letterEventCoroutine);
        letterEventCoroutine = StartCoroutine(LetterEventSpawner());
    }

    public void OnLetterHurdleFailed()
    {
        if (PlayerFunctions != null)
        {
            PlayerFunctions.TakeDamageFromWrongLetter();
            Debug.Log($"❌ Letter hurdle failed! Player took damage");
        }
    }

    public void OnLetterHurdleSuccess()
    {
        wordsCompletedInCurrentEvent++;
        Debug.Log($"✅ Letter hurdle success! Words completed: {wordsCompletedInCurrentEvent}/{wordsToSolvePerEvent}");
        
        if (wordsCompletedInCurrentEvent >= wordsToSolvePerEvent)
        {
            Debug.Log($"🎯 Target reached! Ending letter event after {wordsCompletedInCurrentEvent} word(s)");
            EndLetterEvent();
            
            if (currentEventIndicator != null)
            {
                StopAllCoroutines();
                StartCoroutine(AnimateIndicatorUp(currentEventIndicator));
            }
        }
        else
        {
            Debug.Log($"⏳ Continue event - {wordsToSolvePerEvent - wordsCompletedInCurrentEvent} word(s) remaining");
        }
    }

    IEnumerator AnimateIndicatorUp(GameObject indicator)
    {
        if (indicator == null) yield break;
        
        Vector3 startPos = indicator.transform.position;
        Vector3 upPos = new Vector3(startPos.x, letterSpawnHeight + letterIndicatorStartHeightOffset, startPos.z);

        float time = 0f;
        while (time < letterIndicatorAnimationTime)
        {
            if (indicator == null) yield break;
            indicator.transform.position = Vector3.Lerp(startPos, upPos, time / letterIndicatorAnimationTime);
            time += Time.deltaTime;
            yield return null;
        }

        if (indicator != null)
        {
            Destroy(indicator);
            currentEventIndicator = null;
        }
    }

    #endregion

    #region Original Obstacle/Power-Up/Question System

    void CheckPowerUpSpawn()
    {
        if (PlayerFunctions == null) return;
        
        if (maxPowerUpCount > 0 && totalPowerUpsSpawned >= maxPowerUpCount)
        {
            return;
        }
        
        float playerDistance = PlayerFunctions.transform.position.z;

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
            
            Debug.Log($"Power-ups spawned: {totalPowerUpsSpawned}/{maxPowerUpCount} | Next at: {nextPowerUpDistance}m");
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

        GameObject powerUp = Instantiate(prefab, spawnPos, prefab.transform.rotation, ObstacleParentTransform);
        powerUp.transform.localScale = prefab.transform.localScale;

        activePowerUps.Add(powerUp);
        totalPowerUpsSpawned++;
        StartCoroutine(AutoDespawnPowerUp(powerUp, maxObstacleLifetime));

        Debug.Log($"Spawned {powerUpType} power-up #{totalPowerUpsSpawned} at {PlayerFunctions.transform.position.z}m in lane {randomLane}");
    }

    public void ResetPowerUpSpawning()
    {
        hasSpawnedFirstPowerUp = false;
        nextPowerUpDistance = firstPowerUpDistance;
        totalPowerUpsSpawned = 0;
        Debug.Log("Power-up spawning reset");
    }

    List<int> GetEmptyLanesAtDistance(float distance, float checkRange)
    {
        List<int> emptyLanes = new List<int> { 0, 1, 2 };
        float checkZ = PlayerFunctions.transform.position.z + distance;

        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle == null) continue;
            if (Mathf.Abs(obstacle.transform.position.z - checkZ) < checkRange)
                emptyLanes.Remove(GetLaneFromPosition(obstacle.transform.position.x));
        }

        foreach (GameObject coin in activeCoins)
        {
            if (coin == null) continue;
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

        for (int i = 0; i < spawnPattern.Length; i++)
        {
            float zOffset = spawnDistance + (i * patternSpacing);
            
            if (spawnPattern[i] == 1)
            {
                SpawnSingleQuestion(zOffset);
            }
            else
            {
                SpawnSingleObstacleRow(zOffset);
            }
        }

        Debug.Log($"Spawned complete pattern sequence with {spawnPattern.Length} elements");
    }

    void SpawnSingleObstacleRow(float zOffset)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("No obstacle prefabs assigned!");
            return;
        }

        List<int> availableLanes = new List<int> { 0, 1, 2 };
        int obstaclesToSpawn = Mathf.Min(maxObstaclesPerRow, 3);
        obstaclesToSpawn = Mathf.Clamp(obstaclesToSpawn, 1, 2);

        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            int index = Random.Range(0, availableLanes.Count);
            int lane = availableLanes[index];
            availableLanes.RemoveAt(index);

            int randomPrefabIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject selectedPrefab = obstaclePrefabs[randomPrefabIndex];

            float laneX = (lane - 1.3f) * laneDistance;
            Vector3 spawnPos = new Vector3(laneX, selectedPrefab.transform.position.y, PlayerFunctions.transform.position.z + zOffset);

            GameObject obstacle = Instantiate(selectedPrefab, spawnPos, selectedPrefab.transform.rotation, ObstacleParentTransform);

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

                    GameObject coin = Instantiate(coinPrefab, coinPos, coinPrefab.transform.rotation, ObstacleParentTransform);
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

        float questionHeight = spawnSentence ? sentenceQuestionHeight : spellingQuestionHeight;

        Vector3 spawnPos = new Vector3(
            0f,
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
        if (obstacle != null)
        {
            activeObstacles.Remove(obstacle);
            Destroy(obstacle);
        }
    }

    IEnumerator AutoDespawnCoin(GameObject coin, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (coin != null)
        {
            activeCoins.Remove(coin);
            Destroy(coin);
        }
    }

    IEnumerator AutoDespawnPowerUp(GameObject powerUp, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (powerUp != null)
        {
            activePowerUps.Remove(powerUp);
            Destroy(powerUp);
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
            if (activeObstacles[i] == null)
            {
                activeObstacles.RemoveAt(i);
            }
            else if (activeObstacles[i].transform.position.z < PlayerFunctions.transform.position.z - despawnDistance)
            {
                GameObject obstacle = activeObstacles[i];
                activeObstacles.RemoveAt(i);
                Destroy(obstacle);
            }
        }
    }

    void DespawnOldCoins()
    {
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (activeCoins[i] == null)
            {
                activeCoins.RemoveAt(i);
            }
            else if (activeCoins[i].transform.position.z < PlayerFunctions.transform.position.z - despawnDistance)
            {
                GameObject coin = activeCoins[i];
                activeCoins.RemoveAt(i);
                Destroy(coin);
            }
        }
    }

    void DespawnOldPowerUps()
    {
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            if (activePowerUps[i] == null)
            {
                activePowerUps.RemoveAt(i);
            }
            else if (activePowerUps[i].transform.position.z < PlayerFunctions.transform.position.z - despawnDistance)
            {
                GameObject powerUp = activePowerUps[i];
                activePowerUps.RemoveAt(i);
                Destroy(powerUp);
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

    #endregion

    void OnDrawGizmos()
    {
        if (!showGizmos || PlayerFunctions == null) return;

        float despawnZ = PlayerFunctions.transform.position.z - despawnDistance;

        if (spawnPattern != null && spawnPattern.Length > 0)
        {
            for (int i = 0; i < spawnPattern.Length; i++)
            {
                float spawnZ = PlayerFunctions.transform.position.z + spawnDistance + (i * patternSpacing);
                float alpha = Mathf.Clamp01(1f - (i * 0.2f));

                if (spawnPattern[i] == 1)
                {
                    Gizmos.color = new Color(questionGizmoColor.r, questionGizmoColor.g, questionGizmoColor.b, alpha);
                    
                    Vector3 spellingPos = new Vector3(0f, spellingQuestionHeight, spawnZ);
                    Gizmos.DrawWireSphere(spellingPos, gizmoSphereSize * 1.5f);
                    
                    Vector3 sentencePos = new Vector3(0f, sentenceQuestionHeight, spawnZ);
                    Gizmos.DrawWireSphere(sentencePos, gizmoSphereSize * 1.5f);
                }
                else
                {
                    Gizmos.color = new Color(trapGizmoColor.r, trapGizmoColor.g, trapGizmoColor.b, alpha);
                    DrawLaneBoxes(spawnZ);
                }
            }
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