using System.Collections;
using UnityEngine;

public class ShowQuestionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bgTopQuestions;
    public GameObject question1;

    [Header("Player Reference")]
    public Transform player;
    public PlayerMovement playerMovement;

    void Start()
    {
        StartCoroutine(ShowAfterDelay());
    }

    IEnumerator ShowAfterDelay()
    {
        // 1. Wait 2 seconds → show BGTopQuestions
        yield return new WaitForSeconds(2f);
        bgTopQuestions.SetActive(true);

        // 2. Place Question1 ahead of player
        float distanceAhead = playerMovement.forwardSpeed * 5f;

        Vector3 newPos = new Vector3(
            question1.transform.position.x,
            question1.transform.position.y,
            player.position.z + distanceAhead
        );

        question1.transform.position = newPos;

        // 3. Activate Question1
        question1.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the player collides with Question1 → hide bgTopQuestions
        if (other.gameObject == question1)
        {
            bgTopQuestions.SetActive(false);
        }
    }
}
