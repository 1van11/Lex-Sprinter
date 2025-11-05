using UnityEngine;
using System.Collections;

public class EventTimingManager : MonoBehaviour
{
    public static bool isBossActive = false;

    public GameObject gameA;
    public GameObject bossB;

    public float gameADuration = 60f; // time A stays active

    [Header("Boss Duration")]
    public float bossDuration = 30f; // ✅ Boss stays active for 30 seconds

    [Header("Delay Before UI Disable (Boss Start)")]
    public float uiDisableDelay = 2f;

    [Header("Boss Guide UI")]
    public GameObject bossGuideUI;
    public float guideDelay = 1f;

    [Header("Post Boss Delay")]
    public float postBossDelay = 2f;

    [Header("UI to Disable During Boss")]
    public GameObject[] uiToDisableDuringBoss;

    void Start()
    {
        if (bossGuideUI != null)
            bossGuideUI.SetActive(false);

        StartCoroutine(CycleRoutine());
    }

    IEnumerator CycleRoutine()
    {
        while (true)
        {
            // --- NORMAL PLAY ---
            gameA.SetActive(true);
            bossB.SetActive(false);
            isBossActive = false;

            foreach (GameObject ui in uiToDisableDuringBoss)
                if (ui != null) ui.SetActive(true);

            if (bossGuideUI != null)
                bossGuideUI.SetActive(false);

            yield return new WaitForSeconds(gameADuration);

            // --- START BOSS ---
            isBossActive = true;
            bossB.SetActive(true);
            gameA.SetActive(false);

            StartCoroutine(DisableUIAfterDelay());
            if (bossGuideUI != null)
                StartCoroutine(ShowGuideWithDelay());

            // ✅ Boss automatically ends after 30 seconds
            yield return new WaitForSeconds(bossDuration);
            FinishBoss();

            // ✅ Delay before going back to normal
            if (bossGuideUI != null)
                bossGuideUI.SetActive(false);

            yield return new WaitForSeconds(postBossDelay);
        }
    }

    IEnumerator DisableUIAfterDelay()
    {
        yield return new WaitForSeconds(uiDisableDelay);

        foreach (GameObject ui in uiToDisableDuringBoss)
            if (ui != null) ui.SetActive(false);
    }

    IEnumerator ShowGuideWithDelay()
    {
        yield return new WaitForSeconds(guideDelay);
        if (isBossActive && bossGuideUI != null)
            bossGuideUI.SetActive(true);
    }

    public void FinishBoss()
    {
        isBossActive = false;
    }
}
