using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionnaireSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;      // Reference to PlayerMovement
    public GameObject questionPrefab;          // Prefab with Image + 2 options

    [Header("Spawn Settings")]
    public float spawnDistance = 10f;          // Distance ahead of player to spawn
    public float spawnInterval = 2f;           // How often questions appear
    public float spawnHeight = 2f;             // Y offset (adjust to match UI/canvas worldspace)

    [Header("Despawn Settings")]
    public float despawnDistance = 10f;
    public float maxLifetime = 15f;

    private float timer = 0f;
    private List<GameObject> spawnedQuestions = new List<GameObject>();

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnQuestion();
            timer = 0f;
        }

        DespawnOldQuestions();
    }

    void SpawnQuestion()
    {
        // Get player's forward direction
        Vector3 playerForward = playerMovement.transform.forward;
        
        // Calculate spawn position ahead of player along their forward direction
        Vector3 spawnPos = playerMovement.transform.position + (playerForward * spawnDistance);
        spawnPos.y = spawnHeight; // Set the height
        
        // Make question face the player
        Quaternion rotation = Quaternion.LookRotation(-playerForward) * Quaternion.Euler(0f, 90f, 0f);
        
        GameObject question = Instantiate(questionPrefab, spawnPos, rotation);
        spawnedQuestions.Add(question);
        Destroy(question, maxLifetime);
    }

    void DespawnOldQuestions()
    {
        for (int i = spawnedQuestions.Count - 1; i >= 0; i--)
        {
            if (spawnedQuestions[i] == null)
            {
                spawnedQuestions.RemoveAt(i);
            }
            else
            {
                // Check distance from player
                float distance = Vector3.Distance(spawnedQuestions[i].transform.position, playerMovement.transform.position);
                if (distance > despawnDistance + spawnDistance)
                {
                    Destroy(spawnedQuestions[i]);
                    spawnedQuestions.RemoveAt(i);
                }
            }
        }
    }
}