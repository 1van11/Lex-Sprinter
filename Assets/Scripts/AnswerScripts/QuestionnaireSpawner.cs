using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionnaireSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;

    [Tooltip("Prefab for 'Correct the Spelling' questions")]
    public GameObject questionPrefab;

    [Tooltip("Prefab for 'Complete the Sentence' questions")]
    public GameObject sentencePrefab;

    [Header("Spawn Settings")]
    public float spawnDistance = 30f;
    public float spawnInterval = 2f;
    public float spawnHeight = 0.5f;
    public float despawnDistance = 10f;
    public float maxLifetime = 50f;  // Auto-destroy after this time

    private List<GameObject> activeQuestions = new List<GameObject>();
    private float timer = 0f;
    private int spellingCounter = 0; // Track how many spelling questions were spawned
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
        bool spawnSentence = false;

        // After every 3 spelling questions, spawn 1 sentence question
        if (spellingCounter >= 3)
        {
            spawnSentence = true;
            spellingCounter = 0;
        }

        Vector3 spawnPos = new Vector3(
            0f,
            spawnHeight,
            playerMovement.transform.position.z + spawnDistance
        );

        GameObject prefabToSpawn = spawnSentence ? sentencePrefab : questionPrefab;
        GameObject question = Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation);

        QuestionRandomizer randomizer = question.GetComponent<QuestionRandomizer>();
        if (randomizer != null)
        {
            if (spawnSentence)
            {
                int randomIndex = rng.Next(0, 20); // 20 sentence pairs
                randomizer.SetSentenceQuestion(randomIndex);
                Debug.Log($"Spawned sentence question index: {randomIndex}");
            }
            else
            {
                int randomIndex = rng.Next(0, 55); // 62 spelling pairs
                randomizer.SetSpellingQuestion(randomIndex);
                spellingCounter++;
                Debug.Log($"Spawned spelling question index: {randomIndex}");
            }
        }

        activeQuestions.Add(question);
        Destroy(question, maxLifetime); // Auto cleanup after some time
    }

    void DespawnOldQuestions()
    {
        for (int i = activeQuestions.Count - 1; i >= 0; i--)
        {
            GameObject q = activeQuestions[i];
            if (q == null)
            {
                activeQuestions.RemoveAt(i);
                continue;
            }

            float distanceFromPlayer = playerMovement.transform.position.z - q.transform.position.z;
            if (distanceFromPlayer > despawnDistance)
            {
                Destroy(q);
                activeQuestions.RemoveAt(i);
                Debug.Log("Question destroyed (behind player)");
            }
        }
    }
}
