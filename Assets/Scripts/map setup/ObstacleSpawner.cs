using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement; // Reference to your PlayerMovement script
    public GameObject obstaclePrefab; // Obstacle prefab

    [Header("Spawn Settings")]
    public float spawnDistance = 20f; // Distance ahead of player to spawn obstacles
    public int rowsPerSpawn = 2; // Number of rows to spawn at once
    public float rowSpacing = 10f; // Distance between rows
    public int maxObstaclesPerRow = 2; // Max obstacles per row (max 3 for 3 lanes)
    public float spawnHeight = -1f; // Y position (vertical location)
    public float laneDistance = 3f; // Distance between lanes

    [Header("Object Pooling")]
    public int poolSize = 20; // Initial pool size

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

    public float iFrameDuration = 2f;
    public float flashInterval = 0.2f;
    private bool triggered = false;

    void Start()
    {
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab);
            obj.SetActive(false);
            obstaclePool.Enqueue(obj);
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

    void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obstaclePool.Enqueue(obj);
    }

    void Update()
    {
        DespawnOldObstacles();
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

        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            int index = Random.Range(0, availableLanes.Count);
            int lane = availableLanes[index];
            availableLanes.RemoveAt(index);

            float laneX = (lane - 1.3f) * laneDistance;
            Vector3 spawnPos = new Vector3(laneX, spawnHeight, playerMovement.transform.position.z + zOffset);

            GameObject obstacle = GetPooledObstacle();
            obstacle.transform.position = spawnPos;
            obstacle.transform.rotation = obstaclePrefab.transform.rotation;

            activeObstacles.Add(obstacle);
            StartCoroutine(AutoDespawnObstacle(obstacle, maxObstacleLifetime));
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