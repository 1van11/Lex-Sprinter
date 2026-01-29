using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

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

    // Difficulty word/sentence banks


    #region Easy
    public static string[,] easySpellingPairs = new string[,]
    {
        { "a common pet that barks", "dog", "dag" },
        { "something you wear on your head", "hat", "hoat" },
        { "a color between red and white", "pink", "ponk" },
        { "the star in the sky during daytime", "sun", "san" },
        { "body part used for walking", "leg", "log" },
        { "food from animals, often eaten cooked", "meat", "met" },
        { "a small container for drinking", "cup", "cawp" },
        { "you listen with this", "pair", "pear" },
        { "a plant with leaves and branches", "tree", "tri" },
        { "opposite of white", "black", "bolck" },
        { "moving quickly", "fast", "fest" },
        { "activity in water", "swim", "swom" },
        { "refers to the person being addressed", "you", "yoo" },
        { "where you sleep at night", "bed", "ved" },
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
        { "corn", "corn", "curn" },
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

    public static string[,] easySentencePairs = new string[,]
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
#endregion
#region Medium
    public static string[,] mediumSpellingPairs = new string[,]
{
    { "a burrowing African mammal with a long nose", "aardvark", "aardvarko" },
    { "an open-air venue for performances", "amphitheater", "amfiteatro" },
    { "a small armored mammal that rolls into a ball", "armadillo", "armadilo" },
    { "an ancient astronomical instrument for measuring stars", "astrolabe", "astrolabbo" },
    { "a rare aquatic salamander with external gills", "axolotl", "axoloto" },
    { "an ancient missile weapon that launches projectiles", "ballista", "balista" },
    { "a defensive wall on top of a castle", "battlement", "batlemanto" },
    { "a rotating amusement ride with seats", "carousel", "karuselo" },
    { "a medieval device for hurling heavy stones", "catapult", "katapulto" },
    { "a mythical creature that is half human, half horse", "centaur", "sentaoro" },
    { "a lizard that can change its color", "chameleon", "kamaleono" },
    { "a hanging decorative light fixture", "chandelier", "shandeler" },
    { "the pupal stage of a butterfly", "chrysalis", "chrysaliso" },
    { "a colorful parrot with a crest", "cockatoo", "kokatu" },
    { "a large ancient Roman theater", "colosseum", "coloseo" },
    { "a bridge that can be raised or lowered", "drawbridge", "drawbriggo" },
    { "a carved figure often on buildings", "gargoyle", "gargoyo" },
    { "a professional fighter in ancient Rome", "gladiator", "gladiato" },
    { "a device used for executions by decapitation", "guillotine", "guilotino" },
    { "a spear-like weapon for fishing or combat", "harpoon", "harpono" },
    { "ancient writing system of Egypt using symbols", "hieroglyph", "hyerogliffo" },
    { "an optical toy showing colorful patterns", "kaleidoscope", "kaleidoskopo" },
    { "a complex network of paths", "labyrinth", "labirinto" },
    { "a large tent for events or shows", "marquee", "markweo" },
    { "a collection of exotic animals", "menagerie", "menajero" },
    { "a mythical creature with the body of a man and head of a bull", "minotaur", "minotauro" },
    { "a single massive upright stone", "monolith", "monolito" },
    { "a whale with a long tusk", "narwhal", "narwalo" },
    { "a tall stone pillar or monument", "obelisk", "obelisko" },
    { "a dark volcanic glass", "obsidian", "obsidiano" },
    { "a dungeon with a secret trapdoor", "oubliette", "oblietto" },
    { "a famous temple in Athens", "parthenon", "parthenono" },
    { "a tube for viewing distant objects", "periscope", "periskopo" },
    { "a ruler of ancient Egypt", "pharaoh", "faraono" },
    { "a duck-billed egg-laying mammal", "platypus", "platipo" },
    { "a heavy gate that slides vertically", "portcullis", "portkulo" },
    { "a massive triangular structure", "pyramid", "piramido" },
    { "a small marsupial from Australia", "quokka", "quokko" },
    { "a Japanese warrior", "samurai", "samuraio" },
    { "a stone coffin, usually for royalty", "sarcophagus", "sarkofago" },
    { "an arachnid with a sting", "scorpion", "skorpiono" },
    { "an ancient navigation instrument", "sextant", "sekstanto" },
    { "a mythical creature with a lion's body and human head", "sphinx", "sfinkso" },
    { "a handheld telescope", "spyglass", "spyglasso" },
    { "a large spider with long legs", "tarantula", "tarantulo" },
    { "a medieval siege engine that throws stones", "trebuchet", "trebuchato" },
    { "a three-pronged spear", "trident", "tridanto" },
    { "a Scandinavian warrior or raider", "viking", "vikingo" },
    { "a musical instrument with keys", "xylophone", "zylophono" },
    { "a stepped pyramid from ancient Mesopotamia", "ziggurat", "zigurato" }
};

    public static string[,] mediumSentencePairs = new string[,]
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
#endregion
#region Hard
    public static string[,] hardSpellingPairs = new string[,]
    {
        // Animals & nature
        { "large Australian animal that jumps", "kangaroo", "kangarooo" },
        { "large reptile with powerful jaws", "alligator", "aligater" },
        { "large black big cat", "panther", "panthar" },
        { "sea creature with eight arms", "octopus", "octupus" },
        { "flightless bird from cold regions", "penguin", "penguine" },
        { "mountain that erupts with lava", "volcano", "volcanoe" },
        { "powerful tropical cyclone", "hurricane", "hurricaine" },
        { "huge slow-moving river of ice", "glacier", "glaceir" },
        { "severe snow storm with strong winds", "blizzard", "blizzurd" },

        // Environment & tools
        { "wild natural area with little human presence", "wilderness", "wilderniss" },
        { "tool that shows north, south, east, west", "compass", "compas" },
        { "optical tool to see distant objects", "telescope", "telescop" },
        { "tool to see very small things magnified", "microscope", "microscop" },
        { "decorative items worn on the body", "jewelry", "jewelery" },
        { "tool used to create music", "instrument", "insturment" },

        // Body & food
        { "hair above the eye", "eyebrow", "eyebrou" },
        { "flat bone in the upper back", "shoulderblade", "sholderblade" },
        { "joint in the finger", "knuckle", "knuckel" },
        { "backbone", "spine", "spain" },
        { "muscle in the mouth used for tasting", "tongue", "toung" },
        { "white vegetable that looks like a brain", "cauliflower", "coliflower" },
        { "green fruit with a large pit inside", "avocado", "avacado" },
        { "long green vegetable eaten in salads", "cucumber", "cuccumber" },
        { "sweet brown food made from cocoa", "chocolate", "choclate" },
        { "long Italian noodle dish", "spaghetti", "spagetti" },

        // People & jobs
        { "person who studies or does experiments", "scientist", "sciencist" },
        { "person who travels to space", "astronaut", "astroanut" },
        { "leader of a country", "president", "presedent" },
        { "person who comes to see you", "visitor", "visiter" },
        { "people who live near you", "neighbors", "neighbours" },

        // Actions & traits
        { "to find something new", "discover", "discovar" },
        { "to go to new places to learn", "explore", "explor" },
        { "to build something", "construct", "construck" },
        { "to look at similarities and differences", "compare", "compair" },
        { "to make a choice", "decide", "deside" },
        { "willing to give and share", "generous", "genorous" },
        { "wanting to know more", "curious", "curous" },
        { "feeling worried or nervous", "anxious", "angshus" },
        { "feeling thankful", "grateful", "greatful" },
        { "believing in your own abilities", "confident", "confidant" },

        // Advanced / abstract
        { "advanced human society with cities and government", "civilization", "civilisation" },
        { "something newly created or invented", "invention", "inventon" },
        { "mathematical statement with = sign", "equation", "equasion" },
        { "system of communication (English, Spanish…)", "language", "langwage" },
        { "feeling ashamed or shy", "embarrassed", "embarassed" },
        { "very shy or easily embarrassed", "bashful", "bashfull" },
        { "aware of something", "conscious", "concious" },
        { "very fancy and expensive", "extravagant", "extravagent" },
        { "small orange-like fruit", "apricot", "apricott" },
        { "to confuse or make someone very puzzled", "discombobulate", "discombobulated" },
    };

    public static string[,] hardSentencePairs = new string[,]
    {
        { "The explorers decided to ____ the unknown cave system.", "explore", "explain" },
        { "The mathematician solved a very difficult ____.", "equation", "question" },
        { "She felt extremely ____ after making a mistake in public.", "embarrassed", "impressed" },
        { "The inventor received a patent for his latest ____.", "invention", "convention" },
        { "The ancient ____ developed complex writing systems.", "civilization", "university" },
        { "He remained ____ of his surroundings even while sleeping.", "conscious", "confident" },
        { "The wealthy family lived in a very ____ mansion.", "extravagant", "elegant" },
        { "The shy child felt quite ____ around strangers.", "bashful", "playful" },
        { "The team worked together to ____ a new bridge.", "construct", "conduct" },
        { "She always tries to ____ different points of view.", "compare", "prepare" },
        { "The ____ student asked many thoughtful questions.", "curious", "furious" },
        { "He felt very ____ about the upcoming exam results.", "anxious", "serious" },
        { "The ____ donation helped build the new library.", "generous", "famous" },
        { "Astronauts must be extremely ____ to survive in space.", "confident", "different" },
        { "We are very ____ for all your help during the project.", "grateful", "careful" },
        { "The chef carefully ____ the exotic ingredients.", "prepared", "compared" },
        { "The ____ erupted violently after many years of silence.", "volcano", "tornado" },
        { "The ____ moved slowly across the landscape over centuries.", "glacier", "river" },
        { "The pilot navigated through the dangerous ____.", "hurricane", "mountain" },
        { "She used a ____ to examine the tiny crystals.", "microscope", "telescope" },
    };
#endregion
#region Codes
    void Awake()
    {
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
        else if (sceneName == "HardMode")
        {
            activeSpellingPairs = hardSpellingPairs;
            activeSentencePairs = hardSentencePairs;
            Debug.Log("Difficulty: HARD MODE activated");
        }
        else
        {
            activeSpellingPairs = easySpellingPairs;
            activeSentencePairs = easySentencePairs;
            Debug.LogWarning("Unknown scene name. Defaulting to EASY MODE.");
        }
    }

    public bool TryLoadDailyTaskQuestion()
    {
        if (!PlayerPrefs.HasKey("CurrentTaskID"))
            return false;

        int taskID = PlayerPrefs.GetInt("CurrentTaskID", -1);
        int questionIndex = PlayerPrefs.GetInt("CurrentTaskQuestionIndex", -1);
        bool isSpelling = PlayerPrefs.GetInt("CurrentTaskIsSpelling", 1) == 1;

        if (taskID == -1 || questionIndex == -1)
            return false;

        Debug.Log($"📋 Loading Daily Task: Question #{questionIndex} ({(isSpelling ? "Spelling" : "Sentence")})");

        if (isSpelling)
            SetSpellingQuestion(questionIndex);
        else
            SetSentenceQuestion(questionIndex);

        return true;
    }

    void Start()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.isTrigger = true;
        else Debug.LogWarning("QuestionRandomizer: No collider found on question object. Add a Collider component.");

        if (clueTextObject != null) clueTextObject.SetActive(false);

        if (!TryLoadDailyTaskQuestion())
        {
            SetRandomQuestion();
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

        if (clueTextObject != null) clueTextObject.SetActive(false);

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

        if (clueTextObject != null) clueTextObject.SetActive(false);

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

    public void SetRandomQuestion()
    {
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

    public void PlayQuestionAudio()
    {
        if (audioSource == null || audioPlayed) return;

        if (isSentenceQuestion)
        {
            if (sentencePronunciations != null && currentQuestionIndex < sentencePronunciations.Length && sentencePronunciations[currentQuestionIndex] != null)
            {
                audioSource.PlayOneShot(sentencePronunciations[currentQuestionIndex]);
                audioPlayed = true;
            }
        }
        else
        {
            if (pronunciationSounds != null && currentQuestionIndex < pronunciationSounds.Length && pronunciationSounds[currentQuestionIndex] != null)
            {
                audioSource.PlayOneShot(pronunciationSounds[currentQuestionIndex]);
                audioPlayed = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered collider. isSentenceQuestion: {isSentenceQuestion}");

            if (isSentenceQuestion && clueTextObject != null)
                clueTextObject.SetActive(true);

            if (playAudioOnTrigger)
                PlayQuestionAudio();
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("EXIT detected from: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player triggered EXIT, hiding clue...");

            if (isSentenceQuestion && clueTextObject != null)
            {
                Debug.Log("Clue object found: " + clueTextObject.name);
                clueTextObject.SetActive(false);
            }
        }
    }

    public void TriggerQuestionAudio() => PlayQuestionAudio();

    public void ShowClueText()
    {
        if (clueTextObject != null && isSentenceQuestion) clueTextObject.SetActive(true);
    }

    public void HideClueText()
    {
        if (clueTextObject != null) clueTextObject.SetActive(false);
    }

    public string GetCurrentDifficulty() => SceneManager.GetActiveScene().name;

    public int GetSpellingQuestionCount() => activeSpellingPairs?.GetLength(0) ?? 0;

    public int GetSentenceQuestionCount() => activeSentencePairs?.GetLength(0) ?? 0;
}
    #endregion
    //testing