using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class QuestionRandomizer : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text clueText;
    public TMP_Text jumpText;
    public TMP_Text slideText;
    public GameObject clueTextObject;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] pronunciationSounds;
    public AudioClip[] sentencePronunciations;

    [Header("Trigger Settings")]
    public bool playAudioOnTrigger = true;

    // Current answer and state
    public string correctAnswer;
    private int currentQuestionIndex = -1;
    private bool isSentenceQuestion = false;
    private bool audioPlayed = false;

    // Active sets based on scene
    private string[,] activeSpellingPairs;
    private string[,] activeSentencePairs;

    // -------------------- EASY MODE (Your existing arrays) --------------------
    private string[,] easySpellingPairs = new string[,]
    {
        { "a common pet that barks", "dog", "dag" },
        { "something you wear on your head", "hat", "hot" },
        { "a color between red and white", "pink", "ponk" },
        { "the star in the sky during daytime", "sun", "san" },
        { "body part used for walking", "leg", "log" },
        { "food from animals, often eaten cooked", "meat", "met" },
        { "a small container for drinking", "cup", "cap" },
        { "you listen with this", "pair", "pear" },
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
    };

    private string[,] easySentencePairs = new string[,]
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

    // -------------------- MEDIUM MODE (More challenging words) --------------------
 private string[,] mediumSpellingPairs = new string[,]
{
    { "a small hopping animal with long ears", "rabbit", "rabblit" },
    { "a primate that can climb trees", "monkey", "monkoo" },
    { "a large striped wild cat", "tiger", "tygerru" },
    { "a black and white striped animal", "zebra", "zeebara" },
    { "a large bird with sharp eyesight", "eagle", "eeglun" },
    { "a large wild cat that moves silently", "panther", "panthoro" },
    { "a tall animal with a long neck", "giraffe", "jyrraffo" },
    { "a large reptile with sharp teeth", "alligator", "alygattor" },
    { "a sea creature with eight arms", "octopus", "oktaplis" },
    { "a bird that cannot fly and likes cold places", "penguin", "pengwinu" },
    { "a large area full of trees", "forest", "foryest" },
    { "a hot, sandy area with few plants", "desert", "dezarto" },
    { "a container for carrying items", "basket", "basklert" },
    { "a tool used for climbing up or down", "ladder", "laddiru" },
    { "a container for liquids", "bottle", "botelri" },
    { "a soft object used for sleeping comfort", "pillow", "pilowu" },
    { "a warm cover used on a bed", "blanket", "blanniko" },
    { "a portable light source", "lantern", "lantroon" },
    { "an object that attracts metal", "magnet", "magnetu" },
    { "a plant found in dry places", "cactus", "caktizo" },
    { "a long yellow fruit", "banana", "banoola" },
    { "a sweet baked snack", "cookie", "kookria" },
    { "a dairy product often eaten with bread", "cheese", "cheeso" },
    { "a red fruit used in salads and sauces", "tomato", "tomaitoox" },
    { "a juicy round fruit", "melon", "melanu" },
    { "a place where people buy things", "market", "markato" },
    { "a place where plants are grown", "garden", "gardonu" },
    { "a place where students learn", "school", "skolehra" },
    { "a place with roads and buildings", "street", "streelto" },
    { "a large strong building for royalty", "castle", "castilo" },
    { "a carved figure made of stone or metal", "statue", "stachuno" },
    { "a symbol worn by a king or queen", "crown", "crowlix" },
    { "a structure that crosses over water", "bridge", "brigdo" },
    { "a heavy object used to keep a ship in place", "anchor", "ankuro" },
    { "a mountain that can erupt", "volcano", "volkaino" },
    { "a strong spinning storm", "hurricane", "hurikano" },
    { "a large body of moving ice", "glacier", "glayshiru" },
    { "a severe snowstorm", "blizzard", "blizardo" },
    { "a tool that shows direction", "compass", "kompazzor" },
    { "a tool used for cutting paper", "scissors", "sizzuro" },
    { "a tool used to see far away", "telescope", "teleskopo" },
    { "a tool used to see tiny objects", "microscope", "microskopo" },
    { "a protective head covering", "helmet", "helmuto" },
    { "decorative personal ornaments", "jewelry", "juwelriq" },
    { "a tool used to make music", "instrument", "instruminko" },
    { "protective metal covering worn in battle", "armor", "armuro" },
    { "a sweet brown treat", "chocolate", "chokoliva" },
    { "a long noodle dish often eaten with sauce", "spaghetti", "spagotto" },
    { "a tall building that guides ships", "lighthouse", "lighthoovo" },
    { "a machine that uses wind to turn blades", "windmill", "windmallo" }
};
private string[,] mediumSentencePairs = new string[,]
{
    { "The students will ____ for their final exams tomorrow.", "study", "relax" },
    { "The construction workers ____ the new building quickly.", "built", "repaired" },
    { "The author ____ a fascinating novel last year.", "wrote", "reviewed" },
    { "The chef ____ the ingredients carefully for the recipe.", "measured", "washed" },
    { "The athlete ____ every day to improve his skills.", "trains", "rests" },
    { "The musician ____ a beautiful song for the audience.", "performed", "listened" },
    { "The gardener ____ the plants every morning.", "waters", "trims" },
    { "The programmer ____ a new software application.", "developed", "tested" },
    { "The detective ____ the mystery carefully.", "investigated", "observed" },
    { "The artist ____ the landscape with vibrant colors.", "painted", "sketched" },
    { "The scientist ____ the results to confirm the hypothesis.", "analyzed", "ignored" },
    { "The students ____ quietly while the teacher explained.", "listened", "whispered" },
    { "The captain ____ the ship safely to shore.", "guided", "followed" },
    { "The nurse ____ the patient throughout the night.", "cared for", "watched" },
    { "The engineer ____ a new solution to the problem.", "designed", "reviewed" },
    { "The actor ____ his lines before the performance.", "practiced", "forgot" },
    { "The librarian ____ the books back on the shelves.", "organized", "stacked" },
    { "The explorer ____ new regions of the jungle.", "discovered", "visited" },
    { "The reporter ____ the event for the evening news.", "covered", "announced" },
    { "The professor ____ the topic in great detail.", "explained", "mentioned" }
};


    void Awake()
    {
        // Set active question sets based on current scene
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "EasyMode")
        {
            activeSpellingPairs = easySpellingPairs;
            activeSentencePairs = easySentencePairs;
            Debug.Log("Difficulty: EASY MODE activated");
        }
        else if (sceneName == "MediumMode")
        {
            activeSpellingPairs = mediumSpellingPairs;
            activeSentencePairs = mediumSentencePairs;
            Debug.Log("Difficulty: MEDIUM MODE activated");
        }
        else
        {
            // Default to easy mode if scene name doesn't match
            activeSpellingPairs = easySpellingPairs;
            activeSentencePairs = easySentencePairs;
            Debug.LogWarning("Unknown scene name. Defaulting to EASY MODE.");
        }
    }

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
        
        // Hide clue text at start
        if (clueTextObject != null)
        {
            clueTextObject.SetActive(false);
        }
    }

    public void SetSpellingQuestion(int index)
    {
        if (index < 0 || index >= activeSpellingPairs.GetLength(0)) 
        {
            Debug.LogError($"Invalid spelling question index: {index}");
            return;
        }

        string clue = activeSpellingPairs[index, 0];
        string correct = activeSpellingPairs[index, 1];
        string wrong = activeSpellingPairs[index, 2];
        
        correctAnswer = correct;
        currentQuestionIndex = index;
        isSentenceQuestion = false;
        audioPlayed = false;

        // Hide clue text for spelling questions
        if (clueTextObject != null)
        {
            clueTextObject.SetActive(false);
        }

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

        Debug.Log($"Spelling Question: {clue} | Correct: {correct} | Wrong: {wrong}");
    }

    public void SetSentenceQuestion(int index)
    {
        if (index < 0 || index >= activeSentencePairs.GetLength(0)) 
        {
            Debug.LogError($"Invalid sentence question index: {index}");
            return;
        }

        string sentence = activeSentencePairs[index, 0];
        string correct = activeSentencePairs[index, 1];
        string wrong = activeSentencePairs[index, 2];
        
        clueText.text = sentence;
        correctAnswer = correct;
        currentQuestionIndex = index;
        isSentenceQuestion = true;
        audioPlayed = false;

        // Hide clue text initially for sentence questions (will show on trigger)
        if (clueTextObject != null)
        {
            clueTextObject.SetActive(false);
        }

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

        Debug.Log($"Sentence Question: {sentence} | Correct: {correct} | Wrong: {wrong}");
    }

    // Get a random question based on current difficulty
    public void SetRandomQuestion()
    {
        // 50% chance for spelling vs sentence questions
        if (Random.value > 0.5f)
        {
            int randomIndex = Random.Range(0, activeSpellingPairs.GetLength(0));
            SetSpellingQuestion(randomIndex);
        }
        else
        {
            int randomIndex = Random.Range(0, activeSentencePairs.GetLength(0));
            SetSentenceQuestion(randomIndex);
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
        if (other.CompareTag("Player"))
        {
            // Only show clue text for sentence questions
            if (isSentenceQuestion && clueTextObject != null)
            {
                clueTextObject.SetActive(true);
            }
            
            if (playAudioOnTrigger)
            {
                PlayQuestionAudio();
            }
        }
    }

    // Trigger when player exits collider
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Only hide clue text for sentence questions
            if (isSentenceQuestion && clueTextObject != null)
            {
                clueTextObject.SetActive(false);
            }
        }
    }

    // Optional: Manual trigger method if you want to trigger audio from other scripts
    public void TriggerQuestionAudio()
    {
        PlayQuestionAudio();
    }

    // Manual methods to show/hide clue text from other scripts
    public void ShowClueText()
    {
        if (clueTextObject != null && isSentenceQuestion)
        {
            clueTextObject.SetActive(true);
        }
    }

    public void HideClueText()
    {
        if (clueTextObject != null)
        {
            clueTextObject.SetActive(false);
        }
    }

    // Get current difficulty mode
    public string GetCurrentDifficulty()
    {
        return SceneManager.GetActiveScene().name;
    }

    // Get number of available questions for current difficulty
    public int GetSpellingQuestionCount()
    {
        return activeSpellingPairs?.GetLength(0) ?? 0;
    }

    public int GetSentenceQuestionCount()
    {
        return activeSentencePairs?.GetLength(0) ?? 0;
    }
}   