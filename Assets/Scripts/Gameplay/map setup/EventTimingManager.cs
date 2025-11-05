using UnityEngine;
using System.Collections;

public class EventTimingManager : MonoBehaviour
{
    public GameObject gameA;   // normal gameplay
    public GameObject bossB;   // boss mode

    public float gameADuration = 60f; // time A stays active
    public float bossDuration = 20f;  // NEW: time boss stays active

    [Header("Boss Guide UI")]
    public GameObject bossGuideUI; // assign your Canvas UI here
    public float guideDelay = 1f;  // delay before showing guide

    [Header("Post Boss Delay")]
    public float postBossDelay = 2f; // delay before switching back to gameA

    private bool bossActive = false;

    void Start()
    {
        if (bossGuideUI != null)
            bossGuideUI.SetActive(false); // hide at start

        StartCoroutine(CycleRoutine());
    }

    IEnumerator CycleRoutine()
    {
        while (true)
        {
            // --- PLAY NORMAL GAME ---
            gameA.SetActive(true);
            bossB.SetActive(false);
            if (bossGuideUI != null)
                bossGuideUI.SetActive(false); // hide guide during normal game

            yield return new WaitForSeconds(gameADuration);

            // --- START BOSS ---
            gameA.SetActive(false);
            bossB.SetActive(true);
            bossActive = true;

            // Delay before showing guide
            if (bossGuideUI != null)
                StartCoroutine(ShowGuideWithDelay());

            // Wait until boss is done or time limit reached
            float bossTimer = 0f;
            while (bossActive && bossTimer < bossDuration)
            {
                bossTimer += Time.deltaTime;
                yield return null;
            }

            // End boss automatically if time runs out
            bossActive = false;
            gameBToNormal(); // optional helper

            // Boss finished, hide guide
            if (bossGuideUI != null)
                bossGuideUI.SetActive(false);

            // **WAIT BEFORE RESTARTING NORMAL GAME**
            yield return new WaitForSeconds(postBossDelay);
        }
    }

    IEnumerator ShowGuideWithDelay()
    {
        yield return new WaitForSeconds(guideDelay);
        if (bossActive && bossGuideUI != null)
            bossGuideUI.SetActive(true);
    }

    // Call this from boss logic when boss is finished manually
    public void FinishBoss()
    {
        bossActive = false;
    }

    private void gameBToNormal()
    {
        gameBToNormal(false);
    }

    private void gameBToNormal(bool manualEnd)
    {
        if (bossB != null)
            bossB.SetActive(false);
        if (gameA != null)
            gameA.SetActive(true);

        bossActive = false;
    }
}
