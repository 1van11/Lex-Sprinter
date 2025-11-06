using UnityEngine;
using System.Collections;

public class EventTimingManager : MonoBehaviour
{
    [Header("Gameplay Objects")]
    public GameObject gameA;   // normal gameplay
    public GameObject bossB;   // boss mode

    [Header("Timing Settings")]
    public float gameADuration = 60f; // time A stays active
    public float bossDuration = 20f;  // time boss stays active
    public float preBossDelay = 1f;   // delay before disabling gameA
    public float postBossDelay = 2f;  // delay before switching back to gameA

    [Header("Boss Guide UI")]
    public GameObject bossGuideUI; // assign your Canvas UI here
    public float guideDelay = 1f;  // delay before showing guide
    [Header("Hurdle Settings")]
public int hurdleFailDamage = 1; // damage player takes if they fail


    [Header("Player Reference")]
    public PlayerFunctions player; // assign your player here

    private bool bossActive = false;
    private bool hurdleCompleted = false; // track if player finished hurdle

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
            hurdleCompleted = false; // reset for next boss
            if (bossGuideUI != null)
                bossGuideUI.SetActive(false);

            yield return new WaitForSeconds(gameADuration);

            // --- DELAY BEFORE STARTING BOSS ---
            if (preBossDelay > 0f)
                yield return new WaitForSeconds(preBossDelay);

            // --- START BOSS ---
            gameA.SetActive(false);
            bossB.SetActive(true);
            bossActive = true;

            if (bossGuideUI != null)
                StartCoroutine(ShowGuideWithDelay());

            // Wait until boss is done or time limit reached
            float bossTimer = 0f;
            while (bossActive && bossTimer < bossDuration)
            {
                bossTimer += Time.deltaTime;
                yield return null;
            }

            // If hurdle not completed, hurt the player
            if (!hurdleCompleted && player != null)
            {
                Debug.Log("❌ Player failed the hurdle! Taking damage...");
                player.TakeDamageFromWrongLetter(); // or TakeDamage(1)
            }

            // End boss
            bossActive = false;
            gameBToNormal();

            if (bossGuideUI != null)
                bossGuideUI.SetActive(false);

            yield return new WaitForSeconds(postBossDelay);
        }
    }

    IEnumerator ShowGuideWithDelay()
    {
        yield return new WaitForSeconds(guideDelay);
        if (bossActive && bossGuideUI != null)
            bossGuideUI.SetActive(true);
    }

    public void FinishBoss(bool completedHurdle = true)
    {
        bossActive = false;
        hurdleCompleted = completedHurdle;
    }

    private void gameBToNormal()
    {
        if (bossB != null)
            bossB.SetActive(false);
        if (gameA != null)
            gameA.SetActive(true);

        bossActive = false;
    }
}
