using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionnaireSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public GameObject questionPrefab;

    [Header("Spawn Settings")]
    public float spawnDistance = 10f;
    public float spawnInterval = 2f;
    public float spawnHeight = 0.5f;
    public float despawnDistance = 10f;
    public float maxLifetime = 20f;            // Auto-destroy after this time

    private List<GameObject> activeQuestions = new List<GameObject>();
    private float timer = 0f;
    private System.Random rng;

    void Start()
    {
        rng = new System.Random();
    }

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
        Vector3 spawnPos = new Vector3(
            0f,
            spawnHeight,
            playerMovement.transform.position.z + spawnDistance
        );

        // ✅ Just instantiate a new one
        GameObject question = Instantiate(questionPrefab, spawnPos, questionPrefab.transform.rotation);
        
        // ✅ Set random question
        int randomIndex = rng.Next(0, 62);
        QuestionRandomizer randomizer = question.GetComponent<QuestionRandomizer>();
        if (randomizer != null)
        {
            randomizer.SetQuestion(randomIndex);
            Debug.Log($"Spawned question with index: {randomIndex}");
        }

        activeQuestions.Add(question);
        
        // ✅ Auto-destroy as backup
        Destroy(question, maxLifetime);
    }

    void DespawnOldQuestions()
    {
        for (int i = activeQuestions.Count - 1; i >= 0; i--)
        {
            if (activeQuestions[i] == null)
            {
                activeQuestions.RemoveAt(i);
                continue;
            }

            float distanceFromPlayer = playerMovement.transform.position.z - activeQuestions[i].transform.position.z;

            // ✅ Destroy when question is behind player
            if (distanceFromPlayer > despawnDistance)
            {
                Destroy(activeQuestions[i]);
                activeQuestions.RemoveAt(i);
                Debug.Log("Question destroyed (behind player)");
            }
        }
    }
}