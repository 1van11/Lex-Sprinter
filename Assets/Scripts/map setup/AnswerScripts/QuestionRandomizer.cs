using UnityEngine;
using TMPro;

public class QuestionRandomizer : MonoBehaviour
{
    [Header("spell test")]
    public TMP_Text clueText;
    public TMP_Text jumpText;
    public TMP_Text slideText;
    [Header("Clue Text")]
     // New Text for clue

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
        if (jumpText == null || slideText == null)
        {
            Debug.LogError("Text components are null!");
            return;
        }

        if (index >= 0 && index < spellingPairs.GetLength(0))
        {
           // clueText.text = spellingPairs[index, 0];
            jumpText.text = spellingPairs[index, 1];
            slideText.text = spellingPairs[index, 2];
            Debug.Log($"Spelling question: {jumpText.text} / {slideText.text}");
        }
        else
        {
            Debug.LogError($"Invalid spelling index: {index}");
        }
    }

    // -------------------- Complete the Sentence --------------------
  private string[,] sentencePairs = new string[,]
{
    { "The sun is", "bright", "dark" },
    { "I love to", "read", "sleep" },
    { "The cat is", "sleeping", "running" },
    { "He likes to", "run", "jump" },
    { "We play in the", "park", "school" },
    { "She has a", "book", "bag" },
    { "They are eating", "rice", "bread" },
    { "My father is", "strong", "weak" },
    { "The dog is", "barking", "sleeping" },
    { "It is raining", "today", "yesterday" },
    { "I want to", "learn", "play" },
    { "She can", "dance", "sing" },
    { "He eats an", "apple", "banana" },
    { "We go to", "school", "park" },
    { "My bag is", "blue", "red" },
    { "Birds can", "fly", "swim" },
    { "The baby is", "crying", "laughing" },
    { "I am very", "happy", "sad" },
    { "The car is", "fast", "slow" },
    { "The fish can", "swim", "fly" },
    // Additional sentences
    { "I like to", "draw", "read" },
    { "She always", "smiles", "cries" },
    { "They went to the", "park", "mall" },
    { "He is", "singing", "sleeping" },
    { "We like to", "jump", "sit" },
    { "The banana is", "yellow", "green" },
    { "My neighbor has a", "dog", "cat" },
    { "She loves", "painting", "dancing" },
    { "The athletes will", "run", "walk" },
    { "Children often", "play", "study" },
    { "Tomorrow it might", "rain", "snow" },
    { "I need to", "study", "rest" }
};

    public void SetSentenceQuestion(int index)
    {
        if (jumpText == null || slideText == null)
        {
            Debug.LogError("Text components are null!");
            return;
        }

        if (index >= 0 && index < sentencePairs.GetLength(0))
        {
            clueText.text = sentencePairs[index, 0];
            jumpText.text = sentencePairs[index, 1];
            slideText.text = sentencePairs[index, 2];
            Debug.Log($"Sentence question: {jumpText.text} / {slideText.text}");
        }
        else
        {
            Debug.LogError($"Invalid sentence index: {index}");
        }
    }
}
