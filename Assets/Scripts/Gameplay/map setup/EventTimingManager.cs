using UnityEngine;
using System.Collections;

public class EventTimingManager : MonoBehaviour
{
    public GameObject gameA;   // normal gameplay
    public GameObject bossB;   // boss mode

    public float gameADuration = 60f; // time A stays active

    [Header("Boss Guide UI")]
    public GameObject bossGuideUI; // assign your Canvas UI here
    public float guideDelay = 1f;  // delay before showing guide

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

            // Wait until boss is done
            while (bossActive)
                yield return null;

            // Boss finished, hide guide
            if (bossGuideUI != null)
                bossGuideUI.SetActive(false);
        }
    }

    IEnumerator ShowGuideWithDelay()
    {
        yield return new WaitForSeconds(guideDelay);
        if (bossActive && bossGuideUI != null)
            bossGuideUI.SetActive(true);
    }

    // Call this from boss logic when boss is finished
    public void FinishBoss()
    {
        bossActive = false;
    }
}
