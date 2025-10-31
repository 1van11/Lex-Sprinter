using UnityEngine;

public class BossFunction : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private float spawnInterval = 300f;
    [SerializeField] private float spawnDistanceAhead = 50f;
    [SerializeField] private float spawnHeightAbove = 20f;
    [SerializeField] private float slideDownSpeed = 5f;

    [Header("Boss Size (Scale)")]
    [SerializeField] private Vector3 bossScale = new Vector3(1f, 1f, 1f);

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("GameObjects to Disable During Boss")]
    [SerializeField] private GameObject questionaireSpawnerObject;
    [SerializeField] private GameObject obstacleSpawnerObject;

    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 3f;
    [SerializeField] private int laneCount = 3;

    [Header("Spawned Object After Landing")]
    [SerializeField] private GameObject objectToSpawnAfterLanding;

    private float timer = 0f;
    private GameObject currentBoss;
    private bool bossSliding = false;
    private bool bossActive = false;
    private float targetYPosition;
    private float landedZ; // remember z for spawned object
    private float landedX; // remember x (lane) for spawned object

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            else Debug.LogError("BossFunction: Player not found!");
        }
    }

    void Update()
    {
        if (!bossActive && currentBoss == null)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnBoss();
                timer = 0f;
            }
        }

        if (currentBoss != null)
            UpdateBossPosition();
    }

    void SpawnBoss()
    {
        if (bossActive || currentBoss != null) return;
        if (bossPrefab == null || player == null) return;

        // Choose random lane
        int laneIndex = Random.Range(0, laneCount);
        float laneX = (laneIndex - 1) * laneDistance;

        Vector3 spawnPosition = player.position + player.forward * spawnDistanceAhead;
        spawnPosition.x = laneX;
        spawnPosition.y = player.position.y + spawnHeightAbove;

        targetYPosition = player.position.y;
        landedX = laneX;
        landedZ = spawnPosition.z;

        currentBoss = Instantiate(bossPrefab, spawnPosition, bossPrefab.transform.rotation);
        //currentBoss.transform.localScale = bossScale;

        bossSliding = true;
        bossActive = true;

        DisableSpawners();
    }

    void UpdateBossPosition()
    {
        if (currentBoss == null) return;

        Vector3 targetPosition = player.position + player.forward * spawnDistanceAhead;

        if (bossSliding)
        {
            float newY = Mathf.MoveTowards(currentBoss.transform.position.y, targetYPosition, slideDownSpeed * Time.deltaTime);
            currentBoss.transform.position = new Vector3(currentBoss.transform.position.x, newY, targetPosition.z);

            if (Mathf.Abs(newY - targetYPosition) < 0.1f)
            {
                bossSliding = false;
                OnBossLanded();
            }
        }
        else
        {
            currentBoss.transform.position = new Vector3(currentBoss.transform.position.x, targetYPosition, targetPosition.z);
        }
    }

    void OnBossLanded()
    {
        if (objectToSpawnAfterLanding != null)
        {
            Vector3 spawnPos = new Vector3(landedX, targetYPosition, landedZ);
            GameObject spawnedObj = Instantiate(objectToSpawnAfterLanding, spawnPos, objectToSpawnAfterLanding.transform.rotation);
            spawnedObj.transform.localScale = objectToSpawnAfterLanding.transform.localScale;

            Debug.Log("Boss landed! Spawned object at lane.");
        }
    }

    void DisableSpawners()
    {
        if (questionaireSpawnerObject != null) questionaireSpawnerObject.SetActive(false);
        if (obstacleSpawnerObject != null) obstacleSpawnerObject.SetActive(false);
    }

    void EnableSpawners()
    {
        if (questionaireSpawnerObject != null) questionaireSpawnerObject.SetActive(true);
        if (obstacleSpawnerObject != null) obstacleSpawnerObject.SetActive(true);
    }

    public void RemoveBoss()
    {
        if (currentBoss != null)
        {
            Destroy(currentBoss);
            currentBoss = null;
        }

        bossSliding = false;
        bossActive = false;
        timer = 0f;
        EnableSpawners();
    }

    public bool IsBossActive() => bossActive;
}
