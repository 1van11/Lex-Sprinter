using UnityEngine;
using TMPro;

public class MediumLetterRandomizer : MonoBehaviour
{
    public TMP_Text displayText;
    public TMP_Text collectedText;
    public TMP_Text targetWordText;
    public float correctLetterChance = 0.4f;

    private char letter;

    void OnEnable()
    {
        letter = GetSpawnedLetter();

        if (displayText != null)
            displayText.text = letter.ToString();
    }

    char GetSpawnedLetter()
    {
        correctLetterChance = Mathf.Clamp01(correctLetterChance);

        if (targetWordText != null && collectedText != null && targetWordText.text.Contains(":"))
        {
            string fullTarget = targetWordText.text.Split(':')[1].Trim().ToLower();
            string collected = collectedText.text.Trim().ToLower();

            if (collected.Length < fullTarget.Length)
            {
                char nextNeededLetter = fullTarget[collected.Length];
                float boostedChance = correctLetterChance + 0.4f;
                boostedChance = Mathf.Clamp01(boostedChance);

                if (Random.value <= boostedChance)
                    return char.ToUpper(nextNeededLetter);
            }
        }

        return (char)Random.Range('A', 'Z' + 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectedText != null)
                collectedText.text += char.ToLower(letter);

            gameObject.SetActive(false);
        }
    }
}
