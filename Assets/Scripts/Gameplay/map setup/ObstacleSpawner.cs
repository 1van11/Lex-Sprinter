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
    [Tooltip("Chance to spawn a power-up instead of an obstacle (0-100)")]
    [Range(0, 100)] public int powerUpSpawnChance = 10;
    [Tooltip("Distance before first power-up can spawn")]
    public float firstPowerUpDistance = 100f;
    public Vector3 powerUpPositionOffset = Vector3.zero;
    public Vector3 powerUpScale = Vector3.one;
    [Tooltip("Rotation speed for power-ups (degrees per second)")]
    public float powerUpRotationSpeed = 100f;

    [Tooltip("Set spawn chances for different power-up types (must total 100)")]
    public PowerUpSpawnChance[] spawnChances = new PowerUpSpawnChance[]
    {
        new PowerUpSpawnChance { type = PowerUpType.Shield, chance = 33 },
        new PowerUpSpawnChance { type = PowerUpType.Magnet, chance = 33 },
        new PowerUpSpawnChance { type = PowerUpType.SlowTime, chance = 34 }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // PLATFORM SPAWNER
    // ─────────────────────────────────────────────────────────────────────────
    [Header("─── PLATFORM SPAWNER ───────────────────────────────────")]
    [Tooltip("Platform prefabs to randomly pick from (size & rotation taken from prefab)")]
    public GameObject[] platformPrefabs;

    [Tooltip("Parent object for spawned platforms")]
    public Transform platformParent;

    [Header("Platform Spawn Chances")]
    [Range(0, 100)]
    [Tooltip("Chance (0-100) that a platform replaces an obstacle row")]
    public int platformRowSpawnChance = 25;

    [Range(0, 100)]
    [Tooltip("Chance (0-100) that a platform spans 2 lanes instead of 1")]
    public int platformDoubleLaneChance = 30;

    [Header("Platform Position and Height")]
    [Tooltip("If true, uses each prefab\'s own Y position for height (same way obstacles work). If false, uses Platform Spawn Height below.")]
    public bool platformUsePrefabHeight = true;
    [Tooltip("Manual height used when Platform Use Prefab Height is OFF")]
    public float platformSpawnHeight = 0f;
    [Tooltip("Fine-tune offset applied on top of the calculated lane position. Adjust X to fix left/right misalignment, Y to fix ground overlap.")]
    public Vector3 platformPositionOffset = Vector3.zero;

    [Header("Platform – Coin / Power-Up On Top")]
    [Tooltip("Chance (0-100) that coins appear on top of a platform")]
    [Range(0, 100)] public int platformCoinChance = 60;

    [Tooltip("Number of coins to place on a single-lane platform")]
    public int platformCoinsCount = 3;

    [Tooltip("Spacing between coins placed on a platform")]
    public float platformCoinSpacing = 1.5f;

    [Tooltip("Height offset above the platform surface for coins")]
    public float platformCoinHeightOffset = 1.0f;

    [Tooltip("Chance (0-100) that a power-up appears on top of a platform")]
    [Range(0, 100)] public int platformPowerUpChance = 30;

    [Tooltip("Height offset above the platform surface for power-ups")]
    public float platformPowerUpHeightOffset = 1.2f;

    [Header("Platform Obstacle Override")]
    [Tooltip("When a platform row spawns, spawn this many obstacles in the remaining lane(s) instead of 0")]
    public int obstaclesAlongsidePlatform = 1;

    // ─────────────────────────────────────────────────────────────────────────
    // LETTER HURDLE
    // ─────────────────────────────────────────────────────────────────────────
    [Header("─── LETTER HURDLE ──────────────────────────────────────")]
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
    [Tooltip("Distance ahead to spawn the indicator (reduced for less wait time)")]
    public float letterEventSpawnDistanceAhead = 50f;
    [Tooltip("Delay before spawning indicator after event starts (gives player time to reach spawn point)")]
    public float letterEventIndicatorDelay = 1f;
    [Tooltip("Delay after indicator spawns before letter hurdles appear")]
    public float letterHurdleSpawnDelay = 2f;

    [Header("Letter Indicator Settings")]
    [Tooltip("Distance ahead for indicator (reduced for less wait time)")]
    public float letterIndicatorSpawnDistanceAhead = 60f;
    [Tooltip("Minimum guaranteed distance ahead (reduced for less wait time)")]
    public float letterIndicatorMinimumSpawnAhead = 50f;
    public float letterIndicatorAnimationTime = 2f;
    public float letterIndicatorStayTime = 5f;
    public float letterIndicatorStartHeightOffset = 10f;

    [Header("Letter Hurdle Animation Settings")]
    [Tooltip("Starting height offset for letter hurdles when spawning")]
    public float letterHurdleSpawnHeightOffset = 8f;
    [Tooltip("Animation time for letter hurdle to drop down")]
    public float letterHurdleDropAnimationTime = 1f;
    [Tooltip("Animation time for letter hurdle to fall to ground after defeat")]
    public float letterHurdleFallAnimationTime = 0.5f;
    [Tooltip("Ground Y position for defeated letters")]
    public float letterHurdleGroundY = -2f;

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

    [Header("Letter Event Start/End UI")]
    [Tooltip("UI to display when letter event is about to start")]
    public GameObject letterEventStartWarningUI;
    [Tooltip("How long before event starts to show warning (seconds)")]
    public float warningDisplayTime = 5f;
    [Tooltip("UI to display when letter event ends successfully")]
    public GameObject letterEventCompleteUI;
    [Tooltip("How long to display completion UI (seconds)")]
    public float completionUIDisplayTime = 3f;

    [Header("Letter Event Word Limit")]
    [Tooltip("Number of words to solve before ending the event (set to 1 for single word)")]
    public int wordsToSolvePerEvent = 1;

    [Header("Despawn Settings")]
    public float despawnDistance = 10f;
    public float maxObstacleLifetime = 15f;

    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color trapGizmoColor = Color.red;
    public Color questionGizmoColor = Color.green;
    public Color platformGizmoColor = Color.cyan;
    public Color despawnGizmoColor = Color.blue;
    public float gizmoSphereSize = 0.5f;
    public float laneWidth = 2.5f;
    public float laneLength = 5f;
    public float gizmoHeight = 1f;

    // ─────────────────────────────────────────────────────────────────────────
    // Private State
    // ─────────────────────────────────────────────────────────────────────────
    private List<GameObject> activeObstacles = new List<GameObject>();
    private List<GameObject> activeCoins = new List<GameObject>();
    private List<GameObject> activePowerUps = new List<GameObject>();
    private List<GameObject> activeQuestions = new List<GameObject>();
    private List<GameObject> activePlatforms = new List<GameObject>();

    private bool hasPassedFirstPowerUpDistance = false;
    private int patternIndex = 0;
    private int spellingCounter = 0;
    private System.Random rng;

    private Queue<GameObject> letterPool = new Queue<GameObject>();
    private List<LetterSpawnedInfo> activeLetterObjects = new List<LetterSpawnedInfo>();
    private Dictionary<GameObject, Coroutine> letterAnimations = new Dictionary<GameObject, Coroutine>();
    private Dictionary<GameObject, Coroutine> powerUpRotations = new Dictionary<GameObject, Coroutine>();
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
    private Transform PlatformParentTransform => platformParent != null ? platformParent : transform;
    private Transform LetterSpawnParentTransform => letterSpawnParent != null ? letterSpawnParent : transform;

    public bool IsLetterEventActive => isLetterEventActive;

    // ─────────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        rng = new System.Random();

        CreateLetterPool();
        nextLetterSpawnZ = PlayerFunctions.transform.position.z + letterSpawnDistanceAhead + 0.01f;

        allowRegularLetterSpawning = false;

        if (letterEventClueUI != null)          letterEventClueUI.SetActive(false);
        if (letterEventStartWarningUI != null)  letterEventStartWarningUI.SetActive(false);
        if (letterEventCompleteUI != null)      letterEventCompleteUI.SetActive(false);

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
            DespawnOldPlatforms();

            if (!hasPassedFirstPowerUpDistance && PlayerFunctions != null)
            {
                if (PlayerFunctions.transform.position.z >= firstPowerUpDistance)
                {
                    hasPassedFirstPowerUpDistance = true;
                    Debug.Log($"✅ Player passed first power-up distance at {firstPowerUpDistance}m. Power-ups can now spawn.");
                }
            }
        }

        if (isLetterEventActive && allowRegularLetterSpawning &&
            PlayerFunctions.transform.position.z + letterSpawnDistanceAhead >= nextLetterSpawnZ)
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

    // =========================================================================
    // PLATFORM SPAWNER SYSTEM
    // =========================================================================
    #region Platform Spawner System

    /// <summary>
    /// Spawns a platform row at the given Z offset.
    /// Handles single-lane and double-lane platforms, coins, and power-ups on top.
    /// Also spawns the configured number of obstacles in lanes NOT occupied by the platform.
    /// </summary>
    void SpawnPlatformRow(float zOffset)
    {
        if (platformPrefabs == null || platformPrefabs.Length == 0)
        {
            Debug.LogWarning("⚠️ No platform prefabs assigned! Falling back to obstacle row.");
            SpawnSingleObstacleRow(zOffset);
            return;
        }

        float baseZ = PlayerFunctions.transform.position.z + zOffset;

        // Decide single (1 lane) or double (2 lanes) platform
        bool isDoubleLane = Random.Range(0, 100) < platformDoubleLaneChance;

        // Pick random platform prefab
        int prefabIdx = Random.Range(0, platformPrefabs.Length);
        GameObject prefab = platformPrefabs[prefabIdx];

        // ── Determine which lanes the platform occupies ───────────────────
        List<int> allLanes = new List<int> { 0, 1, 2 }; // 0=left, 1=center, 2=right
        List<int> platformLanes = new List<int>();
        List<int> freeLanes    = new List<int>();

        if (isDoubleLane)
        {
            // Pick 2 adjacent lanes
            int startLane = Random.Range(0, 2); // 0 or 1
            platformLanes.Add(startLane);
            platformLanes.Add(startLane + 1);
        }
        else
        {
            // Pick 1 random lane
            int lane = allLanes[Random.Range(0, allLanes.Count)];
            platformLanes.Add(lane);
        }

        foreach (int l in allLanes)
            if (!platformLanes.Contains(l))
                freeLanes.Add(l);

        // ── Calculate platform world position ─────────────────────────────
        float platformCenterX;
        if (isDoubleLane)
        {
            // Average of the two lane X positions
            float x0 = (platformLanes[0] - 1) * laneDistance;
            float x1 = (platformLanes[1] - 1) * laneDistance;
            platformCenterX = (x0 + x1) * 0.5f;
        }
        else
        {
            platformCenterX = (platformLanes[0] - 1) * laneDistance;
        }

        float platformY = platformUsePrefabHeight ? prefab.transform.position.y : platformSpawnHeight;
        Vector3 spawnPos = new Vector3(
            platformCenterX + platformPositionOffset.x,
            platformY       + platformPositionOffset.y,
            baseZ           + platformPositionOffset.z);

        // Instantiate platform using prefab's own rotation and scale
        GameObject platform = Instantiate(
            prefab,
            spawnPos,
            prefab.transform.rotation,
            PlatformParentTransform);

        platform.transform.localScale = prefab.transform.localScale;
        activePlatforms.Add(platform);
        StartCoroutine(AutoDespawnPlatform(platform, maxObstacleLifetime));

        Debug.Log($"🟩 Spawned {(isDoubleLane ? "DOUBLE" : "SINGLE")}-lane platform at Z: {baseZ}, Lane(s): {string.Join(",", platformLanes)}");

        // ── Spawn coins on top of platform ────────────────────────────────
        if (coinPrefab != null && Random.Range(0, 100) < platformCoinChance)
        {
            SpawnCoinsOnPlatform(platform, platformLanes, baseZ);
        }

        // ── Spawn power-up on top of platform ────────────────────────────
        // Only if no coins were placed (or you want both — remove this check to allow both)
        bool skipPowerUp = false;
        if (!skipPowerUp && hasPassedFirstPowerUpDistance && Random.Range(0, 100) < platformPowerUpChance)
        {
            SpawnPowerUpOnPlatform(platform, baseZ);
        }

        // ── Spawn obstacle(s) in free lanes beside the platform ───────────
        int toSpawn = Mathf.Min(obstaclesAlongsidePlatform, freeLanes.Count);
        for (int i = 0; i < toSpawn; i++)
        {
            int idx = Random.Range(0, freeLanes.Count);
            int lane = freeLanes[idx];
            freeLanes.RemoveAt(idx);

            float laneX = (lane - 1) * laneDistance;
            Vector3 obsPos = new Vector3(laneX, spawnHeight, baseZ);

            // Check if a power-up should replace this obstacle
            bool canSpawnPowerUpHere = hasPassedFirstPowerUpDistance;
            bool doPowerUp = canSpawnPowerUpHere && Random.Range(0, 100) < powerUpSpawnChance;

            if (doPowerUp)
            {
                SpawnPowerUpAtPosition(obsPos);
            }
            else if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
            {
                int rIdx = Random.Range(0, obstaclePrefabs.Length);
                GameObject obsPrefab = obstaclePrefabs[rIdx];
                obsPos.y = obsPrefab.transform.position.y;
                GameObject obs = Instantiate(obsPrefab, obsPos, obsPrefab.transform.rotation, ObstacleParentTransform);
                activeObstacles.Add(obs);
                StartCoroutine(AutoDespawnObstacle(obs, maxObstacleLifetime));
            }
        }

        // ── Fill remaining free lanes with coins ──────────────────────────
        if (coinPrefab != null)
        {
            foreach (int emptyLane in freeLanes)
            {
                float baseLaneX = (emptyLane - 1.3f) * laneDistance;
                for (int c = 0; c < coinsPerLane; c++)
                {
                    float zPos = baseZ;
                    if (coinsPerLane > 1)
                    {
                        float totalSpacing = (coinsPerLane - 1) * coinSpacing;
                        zPos += -(totalSpacing / 2f) + (c * coinSpacing);
                    }

                    Vector3 coinPos = new Vector3(
                        baseLaneX + coinPositionOffset.x,
                        spawnHeight + coinPositionOffset.y,
                        zPos + coinPositionOffset.z);

                    GameObject coin = Instantiate(coinPrefab, coinPos, coinPrefab.transform.rotation, ObstacleParentTransform);
                    coin.transform.localScale = coinPrefab.transform.localScale;
                    activeCoins.Add(coin);
                    StartCoroutine(AutoDespawnCoin(coin, maxObstacleLifetime));
                }
            }
        }
    }

    /// <summary>
    /// Places a row of coins on top of the platform surface.
    /// </summary>
    void SpawnCoinsOnPlatform(GameObject platform, List<int> platformLanes, float baseZ)
    {
        if (coinPrefab == null) return;

        // Approximate surface height using platform bounds
        float surfaceY = GetPlatformSurfaceY(platform);
        float coinY = surfaceY + platformCoinHeightOffset;

        // Place coins in a line centered on the platform
        float totalLength = (platformCoinsCount - 1) * platformCoinSpacing;
        float startZ = baseZ - totalLength * 0.5f;

        float centerX = 0f;
        foreach (int l in platformLanes)
            centerX += (l - 1) * laneDistance;
        centerX /= platformLanes.Count;

        for (int i = 0; i < platformCoinsCount; i++)
        {
            Vector3 coinPos = new Vector3(centerX, coinY, startZ + i * platformCoinSpacing);
            GameObject coin = Instantiate(coinPrefab, coinPos, coinPrefab.transform.rotation, ObstacleParentTransform);
            coin.transform.localScale = coinPrefab.transform.localScale;
            activeCoins.Add(coin);
            StartCoroutine(AutoDespawnCoin(coin, maxObstacleLifetime));
        }

        Debug.Log($"💰 Spawned {platformCoinsCount} coins on platform at Z: {baseZ}");
    }

    /// <summary>
    /// Places a power-up on top of the platform surface.
    /// </summary>
    void SpawnPowerUpOnPlatform(GameObject platform, float baseZ)
    {
        float surfaceY = GetPlatformSurfaceY(platform);
        float puY = surfaceY + platformPowerUpHeightOffset;

        Vector3 puPos = new Vector3(platform.transform.position.x, puY, baseZ);
        SpawnPowerUpAtPosition(puPos);

        Debug.Log($"⭐ Spawned power-up on platform at Z: {baseZ}");
    }

    /// <summary>
    /// Returns the Y coordinate of the top surface of a platform using its Renderer bounds.
    /// Falls back to spawnHeight + 1 if no renderer is found.
    /// </summary>
    float GetPlatformSurfaceY(GameObject platform)
    {
        Renderer rend = platform.GetComponentInChildren<Renderer>();
        if (rend != null)
            return rend.bounds.max.y;

        Collider col = platform.GetComponentInChildren<Collider>();
        if (col != null)
            return col.bounds.max.y;

        return platform.transform.position.y + 1f;
    }

    void DespawnOldPlatforms()
    {
        for (int i = activePlatforms.Count - 1; i >= 0; i--)
        {
            if (activePlatforms[i] == null)
            {
                activePlatforms.RemoveAt(i);
                continue;
            }
            if (activePlatforms[i].transform.position.z < PlayerFunctions.transform.position.z - despawnDistance)
            {
                Destroy(activePlatforms[i]);
                activePlatforms.RemoveAt(i);
            }
        }
    }

    IEnumerator AutoDespawnPlatform(GameObject platform, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (platform != null)
        {
            activePlatforms.Remove(platform);
            Destroy(platform);
        }
    }

    #endregion

    // =========================================================================
    // LETTER CONTAINER SYSTEM
    // =========================================================================
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
        if (letterPool.Count > 0) return letterPool.Dequeue();
        return Instantiate(letterContainerPrefab, Vector3.zero, Quaternion.identity, LetterSpawnParentTransform);
    }

    void ReturnLetterToPool(GameObject obj)
    {
        if (letterAnimations.ContainsKey(obj))
        {
            if (letterAnimations[obj] != null) StopCoroutine(letterAnimations[obj]);
            letterAnimations.Remove(obj);
        }
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

        Quaternion rot = letterUsePrefabTransform ? letterContainerPrefab.transform.rotation : Quaternion.Euler(letterSpawnRotationEuler);
        Vector3 scale  = letterUsePrefabTransform ? letterContainerPrefab.transform.localScale : letterSpawnScale;

        Vector3 startPos = new Vector3(pos.x, pos.y + letterHurdleSpawnHeightOffset, pos.z);
        go.transform.SetPositionAndRotation(startPos, rot);
        go.transform.localScale = scale;

        activeLetterObjects.Add(new LetterSpawnedInfo() { obj = go, timer = 0f });
        Coroutine animCoroutine = StartCoroutine(AnimateLetterDrop(go, startPos, pos));
        letterAnimations[go] = animCoroutine;
    }

    IEnumerator AnimateLetterDrop(GameObject letter, Vector3 startPos, Vector3 targetPos)
    {
        float time = 0f;
        while (time < letterHurdleDropAnimationTime)
        {
            if (letter == null || !letter.activeInHierarchy) yield break;
            float t = time / letterHurdleDropAnimationTime;
            float easedT = 1 - Mathf.Pow(1 - t, 3);
            letter.transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            time += Time.deltaTime;
            yield return null;
        }
        if (letter != null && letter.activeInHierarchy) letter.transform.position = targetPos;
        if (letterAnimations.ContainsKey(letter)) letterAnimations.Remove(letter);
    }

    public void AnimateLetterDefeat(GameObject letter)
    {
        if (letter == null) return;
        if (letterAnimations.ContainsKey(letter))
        {
            if (letterAnimations[letter] != null) StopCoroutine(letterAnimations[letter]);
            letterAnimations.Remove(letter);
        }
        Coroutine fallCoroutine = StartCoroutine(AnimateLetterFallToGround(letter));
        letterAnimations[letter] = fallCoroutine;
    }

    IEnumerator AnimateLetterFallToGround(GameObject letter)
    {
        if (letter == null) yield break;
        Vector3 startPos = letter.transform.position;
        Vector3 groundPos = new Vector3(startPos.x, letterHurdleGroundY, startPos.z);
        float time = 0f;
        while (time < letterHurdleFallAnimationTime)
        {
            if (letter == null || !letter.activeInHierarchy) yield break;
            letter.transform.position = Vector3.Lerp(startPos, groundPos, time / letterHurdleFallAnimationTime);
            time += Time.deltaTime;
            yield return null;
        }
        if (letter != null && letter.activeInHierarchy) letter.transform.position = groundPos;
        if (letterAnimations.ContainsKey(letter)) letterAnimations.Remove(letter);

        yield return new WaitForSeconds(0.5f);
        if (letter != null && letter.activeInHierarchy)
        {
            for (int i = activeLetterObjects.Count - 1; i >= 0; i--)
            {
                if (activeLetterObjects[i].obj == letter)
                {
                    activeLetterObjects.RemoveAt(i);
                    break;
                }
            }
            ReturnLetterToPool(letter);
        }
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
        float startY = letterSpawnHeight + letterIndicatorStartHeightOffset;
        Vector3 pos = new Vector3(0f, startY, z) + letterSpawnPositionOffset + letterSpawnerOffset;
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
        if (indicator != null) indicator.transform.position = downPos;
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
        if (indicator != null) { Destroy(indicator); currentEventIndicator = null; }
    }

    IEnumerator LetterEventSpawner()
    {
        yield return new WaitForSeconds(letterEventInitialDelay);
        while (true)
        {
            float timeUntilEvent = letterEventFrequency - warningDisplayTime;
            if (timeUntilEvent > 0) yield return new WaitForSeconds(timeUntilEvent);

            if (letterEventStartWarningUI != null)
            {
                letterEventStartWarningUI.SetActive(true);
                Debug.Log($"⚠️ Letter Event Warning displayed! Event starts in {warningDisplayTime} seconds");
            }

            if (timeUntilEvent > 0) yield return new WaitForSeconds(warningDisplayTime);
            else                    yield return new WaitForSeconds(letterEventFrequency);

            if (letterEventStartWarningUI != null) letterEventStartWarningUI.SetActive(false);

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
                if (isLetterEventActive) letterEventClueUI.SetActive(true);
            }

            yield return new WaitForSeconds(letterInitialSpawnDelaySeconds);

            if (isLetterEventActive)
            {
                allowRegularLetterSpawning = true;
                nextLetterSpawnZ = PlayerFunctions.transform.position.z + letterSpawnDistanceAhead;
                Debug.Log($"✅ Regular letter spawning enabled at Z: {nextLetterSpawnZ}");
            }

            yield return new WaitForSeconds(letterEventDuration - letterEventIndicatorDelay - letterInitialSpawnDelaySeconds);

            if (isLetterEventActive) EndLetterEvent(false);
        }
    }

    float CalculateFarAheadSpawnPosition()
    {
        if (PlayerFunctions == null) return letterIndicatorMinimumSpawnAhead;
        float currentPlayerZ = PlayerFunctions.transform.position.z;
        float calculatedDistance = Mathf.Max(letterEventSpawnDistanceAhead, letterIndicatorSpawnDistanceAhead, letterIndicatorMinimumSpawnAhead);
        float finalSpawnZ = currentPlayerZ + calculatedDistance;
        Debug.Log($"🎯 Spawn calculation: PlayerZ={currentPlayerZ:F1}, Distance={calculatedDistance:F1}, FinalZ={finalSpawnZ:F1}");
        return finalSpawnZ;
    }

    public void EndLetterEvent(bool wasCompleted = false)
    {
        isLetterEventActive = false;
        allowRegularLetterSpawning = false;
        wordsCompletedInCurrentEvent = 0;

        if (wasCompleted)
        {
            Debug.Log("✅ Letter Event Completed Successfully!");
            if (letterEventCompleteUI != null) StartCoroutine(ShowCompletionUI());
        }
        else
        {
            Debug.Log("⏱️ Letter Event Ended (Time Expired) - Normal spawning resumes");
        }

        if (letterEventClueUI != null) letterEventClueUI.SetActive(false);
        if (letterEventCoroutine != null) StopCoroutine(letterEventCoroutine);
        letterEventCoroutine = StartCoroutine(LetterEventSpawner());
    }

    IEnumerator ShowCompletionUI()
    {
        if (letterEventCompleteUI != null)
        {
            letterEventCompleteUI.SetActive(true);
            Debug.Log($"🎉 Letter Event Complete UI displayed for {completionUIDisplayTime} seconds");
            yield return new WaitForSeconds(completionUIDisplayTime);
            letterEventCompleteUI.SetActive(false);
        }
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
            EndLetterEvent(true);
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
        if (indicator != null) { Destroy(indicator); currentEventIndicator = null; }
    }

    #endregion

    // =========================================================================
    // QUESTION SPAWNING SYSTEM
    // =========================================================================
    #region Question Spawning System

    void SpawnSingleQuestion(float zOffset)
    {
        bool spawnSentence = (spellingCounter >= spellingBeforeSentence);
        float questionHeight = spawnSentence ? sentenceQuestionHeight : spellingQuestionHeight;

        Vector3 spawnPos = new Vector3(0f, questionHeight, PlayerFunctions.transform.position.z + zOffset);
        GameObject prefabToSpawn = spawnSentence ? sentencePrefab : questionPrefab;
        Transform parentToUse    = spawnSentence ? sentenceParent : questionParent;

        GameObject question = Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation, parentToUse);

        QuestionRandomizer randomizer = question.GetComponent<QuestionRandomizer>();
        if (randomizer != null)
        {
            if (spawnSentence)
            {
                int randomIndex = rng.Next(0, 20);
                randomizer.SetSentenceQuestion(randomIndex);
                spellingCounter = 0;
                Debug.Log($"✅ Spawned SENTENCE question at Z: {spawnPos.z}, index: {randomIndex}, Counter RESET to 0");
            }
            else
            {
                int randomIndex = rng.Next(0, 55);
                randomizer.SetSpellingQuestion(randomIndex);
                Debug.Log($"✅ Spawned SPELLING question at Z: {spawnPos.z}, index: {randomIndex}, Counter: {spellingCounter}/{spellingBeforeSentence}");
                spellingCounter++;
                Debug.Log($"📊 Counter incremented to {spellingCounter}/{spellingBeforeSentence}");
            }
        }
        else
        {
            Debug.LogError($"❌ QuestionRandomizer component NOT FOUND on {question.name}!");
        }

        activeQuestions.Add(question);
    }

    void DespawnOldQuestions()
    {
        for (int i = activeQuestions.Count - 1; i >= 0; i--)
        {
            GameObject q = activeQuestions[i];
            if (q == null) { activeQuestions.RemoveAt(i); continue; }
            if (PlayerFunctions.transform.position.z - q.transform.position.z > despawnDistance)
            {
                Debug.Log($"🗑️ Despawning question at Z: {q.transform.position.z}");
                Destroy(q);
                activeQuestions.RemoveAt(i);
            }
        }
    }

    #endregion

    // =========================================================================
    // OBSTACLE SPAWNING SYSTEM
    // =========================================================================
    #region Obstacle Spawning System

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
                // Roll for platform row
                if (platformPrefabs != null && platformPrefabs.Length > 0 &&
                    Random.Range(0, 100) < platformRowSpawnChance)
                {
                    SpawnPlatformRow(zOffset);
                }
                else
                {
                    SpawnSingleObstacleRow(zOffset);
                }
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
        int obstaclesToSpawn = Mathf.Clamp(maxObstaclesPerRow, 1, 2);

        bool canSpawnPowerUp = hasPassedFirstPowerUpDistance;
        bool shouldSpawnPowerUp = canSpawnPowerUp && Random.Range(0, 100) < powerUpSpawnChance;
        bool powerUpSpawned = false;

        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            int index = Random.Range(0, availableLanes.Count);
            int lane = availableLanes[index];
            availableLanes.RemoveAt(index);

            float laneX = (lane - 1) * laneDistance;
            Vector3 spawnPos = new Vector3(laneX, spawnHeight, PlayerFunctions.transform.position.z + zOffset);

            if (shouldSpawnPowerUp && !powerUpSpawned)
            {
                SpawnPowerUpAtPosition(spawnPos);
                powerUpSpawned = true;
            }
            else
            {
                int randomPrefabIndex = Random.Range(0, obstaclePrefabs.Length);
                GameObject selectedPrefab = obstaclePrefabs[randomPrefabIndex];
                spawnPos.y = selectedPrefab.transform.position.y;
                GameObject obstacle = Instantiate(selectedPrefab, spawnPos, selectedPrefab.transform.rotation, ObstacleParentTransform);
                activeObstacles.Add(obstacle);
                StartCoroutine(AutoDespawnObstacle(obstacle, maxObstacleLifetime));
            }
        }

        // Fill empty lanes with coins
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
                        baseZPosition += -(totalSpacing / 2f) + (c * coinSpacing);
                    }

                    Vector3 coinPos = new Vector3(
                        baseLaneX + coinPositionOffset.x,
                        spawnHeight + coinPositionOffset.y,
                        baseZPosition + coinPositionOffset.z);

                    GameObject coin = Instantiate(coinPrefab, coinPos, coinPrefab.transform.rotation, ObstacleParentTransform);
                    coin.transform.localScale = coinPrefab.transform.localScale;
                    activeCoins.Add(coin);
                    StartCoroutine(AutoDespawnCoin(coin, maxObstacleLifetime));
                }
            }
        }
    }

    public void SpawnObstacleRows()
    {
        for (int row = 0; row < rowsPerSpawn; row++)
        {
            float zOffset = spawnDistance + (row * rowSpacing);
            // Roll for platform on each row
            if (platformPrefabs != null && platformPrefabs.Length > 0 &&
                Random.Range(0, 100) < platformRowSpawnChance)
            {
                SpawnPlatformRow(zOffset);
            }
            else
            {
                SpawnSingleObstacleRow(zOffset);
            }
        }
    }

    public void TriggerSpawnNow()
    {
        SpawnPatternSequence();
    }

    IEnumerator AutoDespawnObstacle(GameObject obstacle, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (obstacle != null) { activeObstacles.Remove(obstacle); Destroy(obstacle); }
    }

    IEnumerator AutoDespawnCoin(GameObject coin, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (coin != null) { activeCoins.Remove(coin); Destroy(coin); }
    }

    void DespawnOldObstacles()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null) { activeObstacles.RemoveAt(i); continue; }
            if (activeObstacles[i].transform.position.z < PlayerFunctions.transform.position.z - despawnDistance)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    void DespawnOldCoins()
    {
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (activeCoins[i] == null) { activeCoins.RemoveAt(i); continue; }
            if (activeCoins[i].transform.position.z < PlayerFunctions.transform.position.z - despawnDistance)
            {
                Destroy(activeCoins[i]);
                activeCoins.RemoveAt(i);
            }
        }
    }

    #endregion

    // =========================================================================
    // POWER-UP SYSTEM
    // =========================================================================
    #region Power-Up System

    PowerUpType GetRandomPowerUpType()
    {
        if (spawnChances == null || spawnChances.Length == 0)
            return (PowerUpType)Random.Range(0, 3);

        int totalChance = 0;
        foreach (var sc in spawnChances) totalChance += sc.chance;

        int random = Random.Range(0, totalChance);
        int cumulative = 0;
        foreach (var sc in spawnChances)
        {
            cumulative += sc.chance;
            if (random < cumulative) return sc.type;
        }
        return PowerUpType.Shield;
    }

    void SpawnPowerUpAtPosition(Vector3 position)
    {
        PowerUpType powerUpType = GetRandomPowerUpType();
        GameObject prefab = null;
        switch (powerUpType)
        {
            case PowerUpType.Shield:   prefab = shieldPowerUpPrefab;   break;
            case PowerUpType.Magnet:   prefab = magnetPowerUpPrefab;   break;
            case PowerUpType.SlowTime: prefab = slowTimePowerUpPrefab; break;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"Power-up prefab for {powerUpType} not assigned!");
            return;
        }

        Vector3 spawnPos = new Vector3(
            position.x + powerUpPositionOffset.x,
            position.y + powerUpPositionOffset.y,
            position.z + powerUpPositionOffset.z);

        GameObject powerUp = Instantiate(prefab, spawnPos, prefab.transform.rotation, ObstacleParentTransform);
        powerUp.transform.localScale = prefab.transform.localScale;

        activePowerUps.Add(powerUp);
        Coroutine rotateCoroutine = StartCoroutine(RotatePowerUp(powerUp));
        powerUpRotations[powerUp] = rotateCoroutine;
        StartCoroutine(AutoDespawnPowerUp(powerUp, maxObstacleLifetime));

        Debug.Log($"Spawned {powerUpType} power-up at position: {spawnPos}");
    }

    IEnumerator RotatePowerUp(GameObject powerUp)
    {
        while (powerUp != null)
        {
            powerUp.transform.Rotate(Vector3.up, powerUpRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void ResetPowerUpSpawning()
    {
        hasPassedFirstPowerUpDistance = false;
        Debug.Log("Power-up spawning reset - waiting for player to reach first power-up distance");
    }

    IEnumerator AutoDespawnPowerUp(GameObject powerUp, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (powerUp != null)
        {
            if (powerUpRotations.ContainsKey(powerUp))
            {
                StopCoroutine(powerUpRotations[powerUp]);
                powerUpRotations.Remove(powerUp);
            }
            activePowerUps.Remove(powerUp);
            Destroy(powerUp);
        }
    }

    void DespawnOldPowerUps()
    {
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            GameObject powerUp = activePowerUps[i];
            if (powerUp == null) { activePowerUps.RemoveAt(i); continue; }
            if (powerUp.transform.position.z < PlayerFunctions.transform.position.z - despawnDistance)
            {
                if (powerUpRotations.ContainsKey(powerUp))
                {
                    StopCoroutine(powerUpRotations[powerUp]);
                    powerUpRotations.Remove(powerUp);
                }
                activePowerUps.RemoveAt(i);
                Destroy(powerUp);
            }
        }
    }

    #endregion

    // =========================================================================
    // HELPER METHODS
    // =========================================================================
    #region Helper Methods

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
        return Mathf.Clamp(Mathf.RoundToInt(normalizedX), 0, 2);
    }

    #endregion

    // =========================================================================
    // GIZMOS
    // =========================================================================
    #region Gizmos

    void OnDrawGizmos()
    {
        if (!showGizmos || PlayerFunctions == null) return;

        float despawnZ = PlayerFunctions.transform.position.z - despawnDistance;

        if (spawnPattern != null && spawnPattern.Length > 0)
        {
            for (int i = 0; i < spawnPattern.Length; i++)
            {
                float spawnZ = PlayerFunctions.transform.position.z + spawnDistance + (i * patternSpacing);
                float alpha  = Mathf.Clamp01(1f - (i * 0.2f));

                if (spawnPattern[i] == 1)
                {
                    Gizmos.color = new Color(questionGizmoColor.r, questionGizmoColor.g, questionGizmoColor.b, alpha);
                    Gizmos.DrawWireSphere(new Vector3(0f, spellingQuestionHeight, spawnZ), gizmoSphereSize * 1.5f);
                    Gizmos.DrawWireSphere(new Vector3(0f, sentenceQuestionHeight, spawnZ), gizmoSphereSize * 1.5f);
                }
                else
                {
                    // Draw platform gizmo overlay (translucent cyan box over normal lane boxes)
                    Gizmos.color = new Color(platformGizmoColor.r, platformGizmoColor.g, platformGizmoColor.b, alpha * 0.4f);
                    Gizmos.DrawCube(new Vector3(0f, gizmoHeight, spawnZ), new Vector3(laneDistance * 2f, 0.2f, laneLength));

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

    #endregion
}
//TEST PLATFOTM