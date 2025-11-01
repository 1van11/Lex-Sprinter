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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] pronunciationSounds; // same length as spellingPairs rows
    public AudioClip[] sentencePronunciations; // optional for sentence questions

    [Header("Trigger Settings")]
    public bool playAudioOnTrigger = true;
    
    // Store the current question index for audio lookup
    private int currentQuestionIndex = -1;
    private bool isSentenceQuestion = false;
    private bool audioPlayed = false;

    // -------------------- Correct the Spelling --------------------
    public string[,] spellingPairs = new string[,]
    {
        { "a common pet that barks", "dog", "dag" },
{ "something you wear on your head", "hat", "hot" },
{ "a color between red and white", "pink", "ponk" },
{ "the star in the sky during daytime", "sun", "san" },
{ "body part used for walking", "leg", "log" },
{ "food from animals, often eaten cooked", "meat", "met" },
{ "a small container for drinking", "cup", "cap" },
{ "you listen with this", "ear", "air" },
{ "a plant with leaves and branches", "tree", "tri" },
{ "opposite of white", "black", "block" },
{ "moving quickly", "fast", "fest" },
{ "activity in water", "swim", "swom" },
{ "refers to the person being addressed", "you", "yoo" },
{ "where you sleep at night", "bed", "bad" },
{ "part of your body used to hold things", "hand", "henk" },
{ "a flying animal with feathers", "bird", "burd" },
{ "white drink from cows", "milk", "milx" },
{ "to leap into the air", "jump", "jomp" },
{ "baked food made from flour", "bread", "bredd" },
{ "sweet dessert", "cake", "cak" },
{ "used for walking or running", "foot", "fut" },
{ "female child", "girl", "gurl" },
{ "part of your face used to smell", "nose", "nosh" },
{ "color of the sky on a clear day", "blue", "blu" },
{ "vehicle with wheels", "car", "cor" },
{ "corn", "corn", "cok" },
{ "mother's sister", "aunt", "ant" },
{ "farm animal that gives milk", "cow", "coe" },
{ "feeling of joy", "happy", "hoppy" },
{ "color of an apple", "red", "rad" },
{ "arm", "arm", "arum" },
{ "a companion", "friend", "frend" },
{ "box", "box", "bocs" },
{ "used to carry things", "bag", "bog" },
{ "furniture to sit on", "chair", "chare" },
{ "hop hop", "hop", "hut" },
{ "a male parent", "dad", "did" },
{ "a young child", "kid", "kod" },
{ "female bird", "hen", "han" },
{ "staple food, often eaten with dishes", "rice", "rais" }, 
{ "slow", "slow", "slew" },
{ "reading material", "book", "buck" },
{ "farm animal that oinks", "pig", "peg" },
{ "baby", "baby", "bebe" },
{ "refers to a male person", "him", "hem" },
{ "bell", "bell", "vel" },
{ "toy you play with", "toy", "toi" },
{ "mother", "mom", "mum" },
{ "feeling of sadness", "sad", "sed" },
{ "leaves from a tree", "leaf", "loaf" },
{ "to form letters on paper", "write", "rite" },
{ "color opposite of black", "white", "whyte" },
{ "cat", "cat", "gat" },
{ "farm animal with horns", "goat", "got" },
{ "desk", "desk", "deks" },  
{ "a celestial object at night", "moon", "mun" },
{ "falling water from clouds", "rain", "rein" },
{ "frozen precipitation", "snow", "snou" },
{ "part of the face used for speaking", "lip", "lap" },
{ "sweet spread for bread", "jam", "jem" },
{ "young male child", "boy", "boi" },
{ "relative of parents", "uncle", "unkl" },
{ "good or desirable", "good", "gud" },
{ "opposite of good", "bad", "badd" },
{ "to move on feet at a moderate pace", "walk", "wok" },
{ "to stand upright", "stand", "stend" },
{ "activity of singing", "sing", "sung" },
{ "transport by road", "bus", "bos" },
{ "passage to enter or exit", "door", "dor" },
{ "something that shows location", "map", "mop" },
{ "farm product from birds", "egg", "egh" },
{ "edible fruit", "pear", "per" },
{ "small flying waterbird", "duck", "duc" },
{ "small rodent", "mouse", "mawz" },
{ "air in motion", "wind", "windz" },
//{ "visible rock fragment", "rock", "rok" },x
//{ "cloudy water vapor in sky", "cloud", "clou" },x
//{ "opposite of up", "down", "dun" }x
//{ "part of the body used for seeing", "eye", "eyeh" }, x
//{ "hot meal in liquid form", "soup", "soux" }, x
//{ "to sit down", "sit", "sitz" },x
//{ "a round object used in games", "ball", "bal" },x
//{ "opposite of hot", "cold", "kold" }, x


    };

    private string[,] sentencePairs = new string[,]
    {
        { "The chef ____ a delicious meal.", "cooked", "taught" },
        { "The teacher ____ the students about history.", "taught", "slept" },
        { "The cat ____ on the sunny windowsill.", "slept", "flew" },
        { "The pianist ____ a beautiful melody.", "played", "ran" },
        { "The athlete ____ across the finish line.", "ran", "cried" },
        { "The baby ____ when it's hungry.", "cries", "tumbles" },
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

    void Start()
    {
        // Ensure there's a collider set as trigger
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("QuestionRandomizer: No collider found on question object. Add a Collider component.");
        }
    }

    public void SetSpellingQuestion(int index)
    {
        // REMOVED audio playback from here - it will now play on trigger
        string correct = spellingPairs[index, 1];
        string wrong = spellingPairs[index, 2];
        correctAnswer = correct;
        currentQuestionIndex = index;
        isSentenceQuestion = false;
        audioPlayed = false;

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

    public void SetSentenceQuestion(int index)
    {
        // REMOVED audio playback from here - it will now play on trigger
        clueText.text = sentencePairs[index, 0];
        string correct = sentencePairs[index, 1];
        string wrong = sentencePairs[index, 2];
        correctAnswer = correct;
        currentQuestionIndex = index;
        isSentenceQuestion = true;
        audioPlayed = false;

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

    // Play pronunciation audio
    public void PlayQuestionAudio()
    {
        if (audioSource == null || audioPlayed) return;

        if (isSentenceQuestion)
        {
            // Play sentence pronunciation
            if (sentencePronunciations != null && 
                currentQuestionIndex < sentencePronunciations.Length && 
                sentencePronunciations[currentQuestionIndex] != null)
            {
                audioSource.PlayOneShot(sentencePronunciations[currentQuestionIndex]);
                audioPlayed = true;
            }
        }
        else
        {
            // Play spelling pronunciation
            if (pronunciationSounds != null && 
                currentQuestionIndex < pronunciationSounds.Length && 
                pronunciationSounds[currentQuestionIndex] != null)
            {
                audioSource.PlayOneShot(pronunciationSounds[currentQuestionIndex]);
                audioPlayed = true;
            }
        }
    }

    // Trigger when player enters collider
    void OnTriggerEnter(Collider other)
    {
        if (playAudioOnTrigger && other.CompareTag("Player"))
        {
            PlayQuestionAudio();
        }
    }

    // Optional: Manual trigger method if you want to trigger audio from other scripts
    public void TriggerQuestionAudio()
    {
        PlayQuestionAudio();
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
}