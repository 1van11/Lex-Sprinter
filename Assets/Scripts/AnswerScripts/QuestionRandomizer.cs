using UnityEngine;
using TMPro;

public class QuestionRandomizer : MonoBehaviour
{
    [Header("Answer Texts")]
    public TMP_Text jumpText;
    public TMP_Text slideText;

    private string[,] questionPairs = new string[,]
    {
 { "DOG", "DAG" },
{ "HAT", "HOT" },
{ "PINK", "PONK" },
{ "SUN", "SAN" },
{ "LEG", "LOG" },
{ "MEAT", "MET" },
{ "CUP", "CAP" },
{ "EAR", "AIR" },
{ "TREE", "TRI" },
{ "BLACK", "BLOCK" },
{ "FAST", "FEST" },
{ "SWIM", "SWOM" },
{ "YOU", "YOO" },
{ "BED", "BAD" },
{ "HAND", "HOND" },
{ "BIRD", "BURD" },
{ "MILK", "MALK" },
{ "JUMP", "JOMP" },
{ "BREAD", "BREDD" },
{ "CAKE", "COKE" },
{ "FOOT", "FOT" },
{ "GIRL", "GURL" },
{ "NOSE", "NOSS" },
{ "BLUE", "BLU" },
{ "CAR", "COR" },
{ "COOK", "COK" }, // intentional fake
{ "AUNT", "ANT" },
{ "COW", "COE" },
{ "HAPPY", "HOPPY" },
{ "RED", "RAD" },
{ "BALL", "BULL" },
{ "FRIEND", "FREND" },
{ "BIG", "BUG" },
{ "BAG", "BOG" },
{ "CHAIR", "CHARE" },
{ "HOT", "HUT" },
{ "DOG", "DOGE" }, // intentional fake
{ "ME", "MI" },
{ "DAD", "DID" },
{ "BLACK", "BLAK" }, // intentional fake
{ "KID", "KOD" },
{ "HEN", "HAN" },
{ "RICE", "RAIS" },
{ "CLOUD", "CLOOD" },
{ "BLUE", "BLOO" }, // intentional fake
{ "LEG", "LUG" }, // intentional fake
{ "BOOK", "BUCK" },
{ "PIG", "PEG" },
{ "ORANGE", "ORENGE" },
{ "EAR", "EER" }, // intentional fake
{ "HIM", "HEM" },
{ "BROWN", "BRAWN" },
{ "TOY", "TOI" },
{ "MOM", "MUM" },
{ "HAND", "HANE" }, // intentional fake
{ "SAD", "SED" },
{ "FRIEND", "FREIND" }, // intentional fake
{ "LEAF", "LOAF" },
{ "WRITE", "RITE" },
{ "WHITE", "WHYTE" },
{ "ROCK", "RACK" },
{ "GOAT", "GOT" },
{ "SOUP", "SOOP" }

    };

    public void SetQuestion(int index)
    {
        if (jumpText == null || slideText == null)
        {
            Debug.LogError("Text components are null!");
            return;
        }

        if (index >= 0 && index < questionPairs.GetLength(0))
        {
            jumpText.text = questionPairs[index, 0];
            slideText.text = questionPairs[index, 1];
            
            Debug.Log($"Question text set to: {jumpText.text} / {slideText.text}");
        }
        else
        {
            Debug.LogError($"Invalid index: {index}");
        }
    }
}