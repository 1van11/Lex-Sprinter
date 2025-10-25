using UnityEngine;

public class BossFunction : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private float spawnInterval = 300f; // e.g. 10 for testing
    [SerializeField] private float spawnDistanceAhead = 50f;
    [SerializeField] private float spawnHeightAbove = 20f;
    [SerializeField] private float slideDownSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("GameObjects to Disable During Boss")]
    [SerializeField] private GameObject questionaireSpawnerObject;
    [SerializeField] private GameObject obstacleSpawnerObject;

    private float timer = 0f;
    private GameObject currentBoss;
    private bool bossSliding = false;
    private bool bossActive = false;
    private float targetYPosition;

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
        // Only count timer when boss is NOT active and NOT sliding
        if (!bossActive && currentBoss == null)
        {
            timer += Time.deltaTime;

            // Spawn only when interval reached and no active boss
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
        if (bossActive || currentBoss != null)
        {
            Debug.LogWarning("Boss already exists, skipping spawn.");
            return;
        }

        if (bossPrefab == null || player == null)
        {
            Debug.LogError("BossFunction: Missing references!");
            return;
        }

        Vector3 spawnPosition = player.position + player.forward * spawnDistanceAhead;
        spawnPosition.y = player.position.y + spawnHeightAbove;
        targetYPosition = player.position.y;

        currentBoss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        currentBoss.transform.LookAt(new Vector3(player.position.x, currentBoss.transform.position.y, player.position.z));

        bossSliding = true;
        bossActive = true;

        DisableSpawners();
        Debug.Log("Boss spawned!");
    }

    void UpdateBossPosition()
    {
        if (currentBoss == null) return;

        Vector3 targetPosition = player.position + player.forward * spawnDistanceAhead;

        if (bossSliding)
        {
            float newY = Mathf.MoveTowards(currentBoss.transform.position.y, targetYPosition, slideDownSpeed * Time.deltaTime);
            currentBoss.transform.position = new Vector3(targetPosition.x, newY, targetPosition.z);

            if (Mathf.Abs(newY - targetYPosition) < 0.1f)
                bossSliding = false;
        }
        else
        {
            currentBoss.transform.position = new Vector3(targetPosition.x, targetYPosition, targetPosition.z);
        }

        currentBoss.transform.LookAt(new Vector3(player.position.x, currentBoss.transform.position.y, player.position.z));
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
        timer = 0f; // Reset timer only after boss is gone
        EnableSpawners();
    }

    public bool IsBossActive() => bossActive;
}
