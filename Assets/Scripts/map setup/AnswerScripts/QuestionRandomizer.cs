using UnityEngine;
using TMPro;

public class QuestionRandomizer : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text clueText;
    public TMP_Text jumpText;
    public TMP_Text slideText;

    // Store the correct answer for the current question
    public string correctAnswer;

    // -------------------- Correct the Spelling --------------------
    private string[,] spellingPairs = new string[,]
    {
        { "A common pet that barks", "DOG", "DAG" },
        { "Something you wear on your head", "HAT", "HOT" },
        { "A color between red and white", "PINK", "PONK" },
        { "The star in the sky during daytime", "SUN", "SAN" },
        { "Body part used for walking", "LEG", "LOG" },
        { "Food from animals, often eaten cooked", "MEAT", "MET" },
        { "A small container for drinking", "CUP", "CAP" },
        { "You listen with this", "EAR", "AIR" },
        { "A plant with leaves and branches", "TREE", "TRI" },
        { "Opposite of white", "BLACK", "BLOCK" },
        { "Moving quickly", "FAST", "FEST" },
        { "Activity in water", "SWIM", "SWOM" },
        { "Refers to the person being addressed", "YOU", "YOO" },
        { "Where you sleep at night", "BED", "BAD" },
        { "Part of your body used to hold things", "HAND", "HOND" },
        { "A flying animal with feathers", "BIRD", "BURD" },
        { "White drink from cows", "MILK", "MALK" },
        { "To leap into the air", "JUMP", "JOMP" },
        { "Baked food made from flour", "BREAD", "BREDD" },
        { "Sweet dessert", "CAKE", "COKE" },
        { "Used for walking or running", "FOOT", "FOT" },
        { "Female child", "GIRL", "GURL" },
        { "Part of your face used to smell", "NOSE", "NOSS" },
        { "Color of the sky on a clear day", "BLUE", "BLU" },
        { "Vehicle with wheels", "CAR", "COR" },
        { "Someone who cooks", "COOK", "COK" },
        { "Mother's sister", "AUNT", "ANT" },
        { "Farm animal that gives milk", "COW", "COE" },
        { "Feeling of joy", "HAPPY", "HOPPY" },
        { "Color of an apple", "RED", "RAD" },
        { "Round object used in games", "BALL", "BULL" },
        { "A companion", "FRIEND", "FREND" },
        { "Opposite of small", "BIG", "BUG" },
        { "Used to carry things", "BAG", "BOG" },
        { "Furniture to sit on", "CHAIR", "CHARE" },
        { "High temperature", "HOT", "HUT" },
        { "A male parent", "DAD", "DID" },
        { "A young child", "KID", "KOD" },
        { "Female bird", "HEN", "HAN" },
        { "Staple food, often eaten with dishes", "RICE", "RAIS" },
        { "Visible vapor in the sky", "CLOUD", "CLOOD" },
        { "Reading material", "BOOK", "BUCK" },
        { "Farm animal that oinks", "PIG", "PEG" },
        { "Citrus fruit", "ORANGE", "ORENGE" },
        { "Refers to a male person", "HIM", "HEM" },
        { "Color of chocolate or soil", "BROWN", "BRAWN" },
        { "Toy you play with", "TOY", "TOI" },
        { "Mother", "MOM", "MUM" },
        { "Feeling of sadness", "SAD", "SED" },
        { "Leaves from a tree", "LEAF", "LOAF" },
        { "To form letters on paper", "WRITE", "RITE" },
        { "Color opposite of black", "WHITE", "WHYTE" },
        { "Solid mineral in nature", "ROCK", "RACK" },
        { "Farm animal with horns", "GOAT", "GOT" },
        { "Liquid food eaten with a spoon", "SOUP", "SOOP" }
    };


   public void SetSpellingQuestion(int index)
    {
        string correct = spellingPairs[index, 1];
        string wrong = spellingPairs[index, 2];
        correctAnswer = correct;

        // Randomize placement
        if (Random.value > 0.5f)
        {
            jumpText.text = correct;
            slideText.text = wrong;
        }
        else
        {
            jumpText.text = wrong;
            slideText.text = correct;
        }
    }


    // Randomize only a single pair (used when randomizePerQuestion is true)
    void RandomizeSpellingPairAt(int index)
    {
        if (index < 0 || index >= spellingPairs.GetLength(0)) return;
        if (Random.value < 0.5f)
        {
            string temp = spellingPairs[index, 1];
            spellingPairs[index, 1] = spellingPairs[index, 2];
            spellingPairs[index, 2] = temp;
            Debug.Log($"RandomizeSpellingPairAt swapped index {index}");
        }
    }


    // -------------------- Complete the Sentence --------------------
    private string[,] sentencePairs = new string[,]
    {
           { "The chef ____ a delicious meal.", "cooked", "taught" },
    { "The teacher ____ the students about history.", "taught", "slept" },
    { "The cat ____ on the sunny windowsill.", "slept", "flew" },
    { "The pianist ____ a beautiful melody.", "played", "ran" },
    { "The athlete ____ across the finish line.", "ran", "cried" },

    { "The baby ____ when it’s hungry.", "cries", "tumbles" },
    { "The photographer ____ pictures of the sunset.", "took", "repaired" },
    { "The mechanic ____ the broken engine.", "repaired", "studied" },
    { "The student ____ for the upcoming exam.", "studied", "painted" },
    { "The artist ____ a portrait on the canvas.", "painted", "wagged" },

    { "The dog ____ its tail happily.", "wagged", "sang" },
    { "The singer ____ in front of the audience.", "sang", "watered" },
    { "The gardener ____ the flowers every morning.", "watered", "wrote" },
    { "The writer ____ a new short story.", "wrote", "examined" },
    { "The doctor ____ the patient carefully.", "examined", "drove" },

    { "The driver ____ the car down the highway.", "drove", "built" },
    { "The carpenter ____ a sturdy table.", "built", "experimented" },
    { "The scientist ____ an experiment in the lab.", "experimented", "served" },
    { "The waiter ____ food to the customers.", "served", "swam" },
    { "The swimmer ____ laps in the pool.", "swam", "cooked" }
    };

   public void SetSentenceQuestion(int index)
    {
        clueText.text = sentencePairs[index, 0];
        string correct = sentencePairs[index, 1];
        string wrong = sentencePairs[index, 2];
        correctAnswer = correct;

        // Randomize placement
        if (Random.value > 0.5f)
        {
            jumpText.text = correct;
            slideText.text = wrong;
        }
        else
        {
            jumpText.text = wrong;
            slideText.text = correct;
        }
    }
}