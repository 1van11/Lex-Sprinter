using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionnaireSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerFunctions PlayerFunctions;

    [Tooltip("Prefab for 'Correct the Spelling' questions")]
    public GameObject questionPrefab;

    [Tooltip("Prefab for 'Complete the Sentence' questions")]
    public GameObject sentencePrefab;

    [Header("Parent Settings")]
    [Tooltip("Parent object for spawned questions (optional)")]
    public Transform questionParent;
    [Tooltip("Parent object for spawned sentence questions (optional)")]
    public Transform sentenceParent;

    [Header("Spawn Settings")]
    public float firstQuestionDelay = 5f;
    public float spawnDistance = 30f;
    public int rowsPerSpawn = 1;
    public float rowSpacing = 10f;
    public float questionRowSpacing = 5f;
    public float spawnHeight = 0.5f;
    public float despawnDistance = 10f;
    public float maxLifetime = 50f;

    [Header("Question Pattern")]
    public int spellingBeforeSentence = 3;

    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color spawnGizmoColor = Color.green;
    public float gizmoSphereSize = 1f;

    private List<GameObject> activeQuestions = new List<GameObject>();
    private float lastSpawnZ = 0f;
    private int spellingCounter = 0;
    private System.Random rng;

    void Start()
    {
        rng = new System.Random();
        lastSpawnZ = PlayerFunctions.transform.position.z + firstQuestionDelay - spawnDistance;
    }

    void Update()
    {
        if (PlayerFunctions == null) return;
        CheckQuestionSpawn();
        DespawnOldQuestions();
    }

    void CheckQuestionSpawn()
    {
        float playerZ = PlayerFunctions.transform.position.z;
        float nextSpawnZ = lastSpawnZ + rowSpacing;

        if (playerZ >= nextSpawnZ)
        {
            SpawnQuestionRows();
            lastSpawnZ = nextSpawnZ;
        }
    }

    void SpawnQuestionRows()
    {
        for (int row = 0; row < rowsPerSpawn; row++)
        {
            float zOffset = (row * questionRowSpacing);
            SpawnQuestion(zOffset);
        }
    }

    void SpawnQuestion(float zOffset = 0f)
    {
        bool spawnSentence = false;

        if (spellingCounter >= spellingBeforeSentence)
        {
            spawnSentence = true;
            spellingCounter = 0;
        }

        Vector3 spawnPos = new Vector3(
            0f,
            spawnHeight,
            PlayerFunctions.transform.position.z + spawnDistance + zOffset
        );

        GameObject prefabToSpawn = spawnSentence ? sentencePrefab : questionPrefab;
        Transform parentToUse = spawnSentence ? sentenceParent : questionParent;

        // ✅ Instantiates under the chosen parent (if assigned)
        GameObject question = Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation, parentToUse);

        QuestionRandomizer randomizer = question.GetComponent<QuestionRandomizer>();
        if (randomizer != null)
        {
            if (spawnSentence)
            {
                int randomIndex = rng.Next(0, 20);
                randomizer.SetSentenceQuestion(randomIndex);
                Debug.Log($"Spawned sentence question index: {randomIndex} at Z: {spawnPos.z}");
            }
            else
            {
                int randomIndex = rng.Next(0, 75);
                randomizer.SetSpellingQuestion(randomIndex);
                spellingCounter++;
                Debug.Log($"Spawned spelling question index: {randomIndex} at Z: {spawnPos.z}");
            }
        }

        activeQuestions.Add(question);
        StartCoroutine(AutoDespawnQuestion(question, maxLifetime));
    }

    IEnumerator AutoDespawnQuestion(GameObject question, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (question != null)
        {
            activeQuestions.Remove(question);
            Destroy(question);
        }
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

            float distanceFromPlayer = PlayerFunctions.transform.position.z - q.transform.position.z;
            if (distanceFromPlayer > despawnDistance)
            {
                Destroy(q);
                activeQuestions.RemoveAt(i);
                Debug.Log("Question destroyed (behind player)");
            }
        }
    }

    public void TriggerSpawnNow()
    {
        SpawnQuestionRows();
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || PlayerFunctions == null) return;

        float nextSpawnZ = lastSpawnZ + rowSpacing;
        Vector3 spawnPos = new Vector3(0f, spawnHeight, nextSpawnZ);

        Gizmos.color = spawnGizmoColor;
        Gizmos.DrawWireSphere(spawnPos, gizmoSphereSize);

        Vector3 playerPos = PlayerFunctions.transform.position;
        Gizmos.DrawLine(playerPos, spawnPos);
    }
}
