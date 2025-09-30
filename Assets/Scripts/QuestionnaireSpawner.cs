using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionnaireSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;      // Reference to PlayerMovement
    public GameObject questionPrefab;          // Prefab with Image + 2 options

    [Header("Spawn Settings")]
    public float spawnDistance = 20f;          // Distance ahead of player to spawn
    public float spawnInterval = 3f;           // How often questions appear
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
    // Always center lane (lane 1)
    float laneX = 0f; 
    Vector3 spawnPos = new Vector3(laneX, spawnHeight, playerMovement.transform.position.z + spawnDistance);

    // Apply rotation offset
    Quaternion rotation = Quaternion.Euler(0F, 90f, 0f); // tweak these values
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
            else if (spawnedQuestions[i].transform.position.z < playerMovement.transform.position.z - despawnDistance)
            {
                Destroy(spawnedQuestions[i]);
                spawnedQuestions.RemoveAt(i);
            }
        }
    }
}
