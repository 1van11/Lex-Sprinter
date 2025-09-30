using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowQuestionsUi : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public GameObject questionPrefab;

    [Header("Spawn Settings")]
    public float spawnDistance = 20f;      // Distance ahead of player
    public float spawnInterval = 3f;       // Time between spawns
    public float spawnHeight = 0f;         // Adjust Y to match ground
    public float despawnDistance = 10f;    // Remove when behind player
    public float maxLifetime = 15f;        // Auto-remove just in case

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
        Vector3 spawnPos = new Vector3(0f, spawnHeight, playerMovement.transform.position.z + spawnDistance);
        GameObject q = Instantiate(questionPrefab, spawnPos, Quaternion.identity);
        spawnedQuestions.Add(q);
        Destroy(q, maxLifetime);
    }

    void DespawnOldQuestions()
    {
        for (int i = spawnedQuestions.Count - 1; i >= 0; i--)
        {
            if (spawnedQuestions[i] == null) spawnedQuestions.RemoveAt(i);
            else if (spawnedQuestions[i].transform.position.z < playerMovement.transform.position.z - despawnDistance)
            {
                Destroy(spawnedQuestions[i]);
                spawnedQuestions.RemoveAt(i);
            }
        }
    }
}