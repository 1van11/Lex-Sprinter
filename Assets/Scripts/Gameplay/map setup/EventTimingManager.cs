using UnityEngine;

public class EventTimingManager : MonoBehaviour
{
    public GameObject gameA;   // normal gameplay
    public GameObject bossB;   // boss mode

    public float gameADuration = 60f; // time A stays active

    private bool bossActive = false;

    void Start()
    {
        StartCoroutine(CycleRoutine());
    }

    System.Collections.IEnumerator CycleRoutine()
    {
        while (true)
        {
            // --- PLAY NORMAL GAME ---
            gameA.SetActive(true);
            bossB.SetActive(false);
            yield return new WaitForSeconds(gameADuration);

            // --- START BOSS ---
            gameA.SetActive(false);
            bossB.SetActive(true);
            bossActive = true;

            // Wait until boss is done
            while (bossActive)
                yield return null;
        }
    }

    // Call this from boss logic when boss is finished
    public void FinishBoss()
    {
        bossActive = false;
    }
}
