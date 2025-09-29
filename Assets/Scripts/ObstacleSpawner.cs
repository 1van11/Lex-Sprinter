using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;   // Reference to your PlayerMovement script
    public GameObject obstaclePrefab;       // Obstacle prefab

    [Header("Spawn Settings")]
    public float spawnDistance = 20f;       // Distance ahead of player to spawn obstacles
    public float spawnInterval = 2f;        // Time between spawns
    public int maxObstaclesPerRow = 2;      // Max obstacles per row (max 3 for 3 lanes)
    public float spawnHeight = -1f;          // Y position (vertical location) - set to 0 for ground level
    public float laneDistance = 3f;         // HARDCODED: Distance between lanes (set to match your setup)

    [Header("Despawn Settings")]
    public float despawnDistance = 10f;     // Distance behind player before obstacle despawns
    public float maxObstacleLifetime = 15f; // Max time before obstacle auto-despawns

    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color spawnGizmoColor = Color.red;
    public Color despawnGizmoColor = Color.blue;
    public Color distanceGizmoColor = Color.yellow;
    public float gizmoSphereSize = 0.5f;
    public float laneWidth = 2.5f;          // Width of each lane box
    public float laneLength = 5f;           // Length of each lane box
    public float gizmoHeight = 1f;          // Height where gizmo is drawn (match PlayerMovement)

    private float timer = 0f;
    private List<GameObject> spawnedObstacles = new List<GameObject>();

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnObstacles();
            timer = 0f;
        }

        // Check and despawn obstacles that are behind the player
        DespawnOldObstacles();
    }

void SpawnObstacles()
{
    List<int> availableLanes = new List<int> { 0, 1, 2 }; // lanes 0 = left, 1 = center, 2 = right
    int obstaclesToSpawn = Mathf.Min(maxObstaclesPerRow, availableLanes.Count);

    for (int i = 0; i < obstaclesToSpawn; i++)
    {
        int index = Random.Range(0, availableLanes.Count);
        int lane = availableLanes[index];
        availableLanes.RemoveAt(index);

        // Snap obstacle exactly to lane center
        float laneX = (lane - 1.3f) * laneDistance; // -1 = left, 0 = center, 1 = right
        Vector3 spawnPos = new Vector3(laneX, spawnHeight, playerMovement.transform.position.z + spawnDistance);

        GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, obstaclePrefab.transform.rotation);
        spawnedObstacles.Add(obstacle);

        Destroy(obstacle, maxObstacleLifetime);
    }
}


    void DespawnOldObstacles()
    {
        // Remove null references and obstacles behind player
        for (int i = spawnedObstacles.Count - 1; i >= 0; i--)
        {
            if (spawnedObstacles[i] == null)
            {
                spawnedObstacles.RemoveAt(i);
            }
            else if (spawnedObstacles[i].transform.position.z < playerMovement.transform.position.z - despawnDistance)
            {
                Destroy(spawnedObstacles[i]);
                spawnedObstacles.RemoveAt(i);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || playerMovement == null) return;

        // Calculate spawn and despawn Z positions
        float spawnZ = playerMovement.transform.position.z + spawnDistance;
        float despawnZ = playerMovement.transform.position.z - despawnDistance;

        // Draw SPAWN LANE BOXES (red) - at gizmoHeight to match PlayerMovement gizmos
        Gizmos.color = spawnGizmoColor;
        
        for (int lane = 0; lane < 3; lane++)
        {
            float laneX = (lane - 1) * laneDistance; // Now uses hardcoded laneDistance
            Vector3 lanePos = new Vector3(laneX, gizmoHeight, spawnZ);
            
            // Draw wire cube for each lane (bigger visual area)
            Gizmos.DrawWireCube(lanePos, new Vector3(laneWidth, 0.5f, laneLength));
            
            // Draw sphere at center
            Gizmos.DrawWireSphere(lanePos, gizmoSphereSize);
        }

        // Draw DESPAWN LANE BOXES (blue)
        Gizmos.color = despawnGizmoColor;
        
        for (int lane = 0; lane < 3; lane++)
        {
            float laneX = (lane - 1) * laneDistance; // Now uses hardcoded laneDistance
            Vector3 lanePos = new Vector3(laneX, gizmoHeight, despawnZ);
            
            // Draw wire cube for each lane
            Gizmos.DrawWireCube(lanePos, new Vector3(laneWidth, 0.5f, laneLength));
        }

        // Draw distance indicator from player to spawn line (yellow)
        Gizmos.color = distanceGizmoColor;
        Vector3 playerPos = playerMovement.transform.position;
        Vector3 spawnLineCenter = new Vector3(0, gizmoHeight, spawnZ);
        Gizmos.DrawLine(playerPos, spawnLineCenter);
    }
}