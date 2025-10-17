using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionnaireSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;      // Reference to PlayerMovement
    public GameObject questionPrefab;          // Prefab with Image + 2 options

    [Header("Spawn Settings")]
    public float spawnDistance = 10f;          // Distance ahead of player
    public float spawnInterval = 2f;           // How often questions appear
    public float spawnHeight = 0.5f;           // Adjust Y to match ground
    public float despawnDistance = 10f;        // Remove when behind player
    public float maxLifetime = 15f;            // Auto-remove just in case

    private float timer = 0f;
    private List<GameObject> spawnedQuestions = new List<GameObject>();

    void Update()
    {
        if (playerMovement == null) return;

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
        // Always spawn in the center (X = 0)
        Vector3 playerForward = playerMovement.transform.forward;
        Vector3 spawnPos = new Vector3(
            0f,                                // X fixed to 0 (center)
            spawnHeight,                       // Y position
            playerMovement.transform.position.z + spawnDistance  // Z ahead of player
        );

        // Make it face the player’s direction
        Quaternion rotation = Quaternion.LookRotation(-playerForward) * Quaternion.Euler(0f, 90f, 0f);

        // Spawn the question
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
