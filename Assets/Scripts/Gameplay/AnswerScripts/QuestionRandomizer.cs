using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class QuestionRandomizer : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text jumpText;
    public TMP_Text slideText;
    public TMP_Text option3Text;
    public GameObject clueTextObject;
    public TMP_Text clueText;
    public GameObject clueImageObject;
    public Image cluePic;

    [Header("Clue Images")]
    public Sprite[] clueImages;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] pronunciationSounds;
    public AudioClip[] sentencePronunciations;

    [Header("Trigger Settings")]
    public bool playAudioOnTrigger = true;

    // Current question state
    public string correctAnswer;
    private int currentQuestionIndex = -1;
    private bool isSentenceQuestion = false;
    private bool audioPlayed = false;
    private bool playerInTrigger = false;

    // Active sets based on scene
    private string[,] activeSpellingPairs;
    private string[,] activeSentencePairs;

    // ─────────────────────────────────────────────────────────────────────────────
    // DIFFICULTY BANKS – each row has 4 columns: clue + correct + wrong1 + wrong2
    // ─────────────────────────────────────────────────────────────────────────────

    #region Easy
    public static string[,] easySpellingPairs = new string[,]
    {
        { "a common pet that barks", "dog", "dag", "dug" },
        { "something you wear on your head", "hat", "hoat", "hut" },
        { "a color between red and white", "pink", "ponk", "pank" },
        { "the star in the sky during daytime", "sun", "san", "son" },
        { "body part used for walking", "leg", "log", "lig" },
        { "food from animals, often eaten cooked", "meat", "met", "mait" },
        { "a small container for drinking", "cup", "cawp", "cop" },
        { "you listen with this", "ear", "eer", "eur" },
        { "a plant with leaves and branches", "tree", "tri", "trea" },
        { "opposite of white", "black", "bolck", "blak" },
        { "moving quickly", "fast", "fest", "fust" },
        { "activity in water", "swim", "swom", "swam" },
        { "refers to the person being addressed", "you", "yoo", "yu" },
        { "where you sleep at night", "bed", "ved", "bad" },
        { "part of your body used to hold things", "hand", "henk", "hend" },
        { "a flying animal with feathers", "bird", "burd", "berd" },
        { "white drink from cows", "milk", "milx", "melk" },
        { "to leap into the air", "jump", "jomp", "jamp" },
        { "baked food made from flour", "bread", "bredd", "bred" },
        { "sweet dessert", "cake", "cak", "caek" },
        { "used for walking or running", "foot", "fut", "fot" },
        { "female child", "girl", "gurl", "gerl" },
        { "part of your face used to smell", "nose", "nosh", "noze" },
        { "color of the sky on a clear day", "blue", "blu", "blew" },
        { "vehicle with wheels", "car", "cor", "kar" },
        { "yellow vegetable", "corn", "curn", "korn" },
        { "mother's sister", "aunt", "ant", "awnt" },
        { "farm animal that gives milk", "cow", "coe", "kow" },
        { "feeling of joy", "happy", "hoppy", "hapi" },
        { "color of an apple", "red", "rad", "reed" },
        { "limb attached to shoulder", "arm", "arum", "erm" },
        { "a companion", "friend", "frend", "freind" },
        { "container with sides", "box", "bocs", "boks" },
        { "used to carry things", "bag", "bog", "beg" },
        { "furniture to sit on", "chair", "chare", "chear" },
        { "jump on one foot", "hop", "hut", "hap" },
        { "a male parent", "dad", "did", "dod" },
        { "a young child", "kid", "kod", "ked" },
        { "female chicken", "hen", "han", "hin" },
        { "staple food, often eaten with dishes", "rice", "rais", "ryce" },
        { "opposite of fast", "slow", "slew", "sloe" },
        { "reading material", "book", "buck", "boke" },
        { "farm animal that oinks", "pig", "peg", "pog" },
        { "very young human", "baby", "bebe", "beby" },
        { "refers to a male person", "him", "hem", "hym" },
        { "makes ringing sound", "bell", "vel", "bel" },
        { "plaything for children", "toy", "toi", "tay" },
        { "female parent", "mom", "mum", "mam" },
        { "feeling of sadness", "sad", "sed", "sod" },
        { "part of a tree or plant", "leaf", "loaf", "leef" },
        { "to form letters on paper", "write", "rite", "writ" },
        { "color opposite of black", "white", "whyte", "wite" },
        { "small furry pet that meows", "cat", "gat", "kat" },
        { "farm animal with horns", "goat", "got", "gote" },
        { "furniture for working", "desk", "deks", "dask" },
        { "a celestial object at night", "moon", "mun", "mune" },
        { "falling water from clouds", "rain", "rein", "rayn" },
        { "frozen precipitation", "snow", "snou", "snoe" },
        { "part of the face used for speaking", "lip", "lap", "lep" },
        { "sweet spread for bread", "jam", "jem", "jom" },
        { "young male child", "boy", "boi", "bouy" },
        { "brother of your parent", "uncle", "unkl", "uncal" },
        { "good or desirable", "good", "gud", "goud" },
        { "opposite of good", "bad", "badd", "bod" },
        { "to move on feet at a moderate pace", "walk", "wok", "wolk" },
        { "to be upright on feet", "stand", "stend", "stond" },
        { "to make music with voice", "sing", "sung", "seng" },
        { "transport by road", "bus", "bos", "bas" },
        { "passage to enter or exit", "door", "dor", "doer" },
        { "something that shows location", "map", "mop", "mep" },
        { "oval farm product from birds", "egg", "egh", "eg" },
        { "edible fruit shaped like a bulb", "pear", "per", "pare" },
        { "small swimming waterbird", "duck", "duc", "duk" },
        { "small rodent", "mouse", "mawz", "mous" },
        { "air in motion", "wind", "windz", "wynd" },
    };

    public static string[,] easySentencePairs = new string[,]
    {
        { "The chef ____ a delicious meal.", "cooked", "taught", "slept" },
        { "The teacher ____ the students about history.", "taught", "slept", "flew" },
        { "The cat ____ on the sunny windowsill.", "slept", "flew", "cooked" },
        { "The pianist ____ a beautiful melody.", "played", "ran", "cried" },
        { "The athlete ____ across the finish line.", "ran", "cried", "tumbled" },
        { "The baby ____ when it's hungry.", "cries", "tumbles", "runs" },
        { "The photographer ____ pictures of the sunset.", "took", "repaired", "studied" },
        { "The mechanic ____ the broken engine.", "repaired", "studied", "painted" },
        { "The student ____ for the upcoming exam.", "studied", "painted", "took" },
        { "The artist ____ a portrait on the canvas.", "painted", "wagged", "sang" },
        { "The dog ____ its tail happily.", "wagged", "sang", "watered" },
        { "The singer ____ in front of the audience.", "sang", "watered", "wrote" },
        { "The gardener ____ the flowers every morning.", "watered", "wrote", "examined" },
        { "The writer ____ a new short story.", "wrote", "examined", "drove" },
        { "The doctor ____ the patient carefully.", "examined", "drove", "built" },
        { "The driver ____ the car down the highway.", "drove", "built", "experimented" },
        { "The carpenter ____ a sturdy table.", "built", "experimented", "served" },
        { "The scientist ____ an experiment in the lab.", "experimented", "served", "swam" },
        { "The waiter ____ food to the customers.", "served", "swam", "cooked" },
        { "The swimmer ____ laps in the pool.", "swam", "cooked", "taught" }
    };
    #endregion

    #region Medium
    public static string[,] mediumSpellingPairs = new string[,]
    {
        { "a burrowing African mammal with a long nose", "aardvark", "aardvarko", "ardvark" },
        { "an open-air venue for performances", "amphitheater", "amfiteatro", "amphitheatre" },
        { "a small armored mammal that rolls into a ball", "armadillo", "armadilo", "armadelo" },
        { "an ancient astronomical instrument for measuring stars", "astrolabe", "astrolabbo", "astrolab" },
        { "a rare aquatic salamander with external gills", "axolotl", "axoloto", "axolotal" },
        { "an ancient missile weapon that launches projectiles", "ballista", "balista", "ballisto" },
        { "a defensive wall on top of a castle", "battlement", "batlemanto", "battlemont" },
        { "a rotating amusement ride with seats", "carousel", "karuselo", "carosel" },
        { "a medieval device for hurling heavy stones", "catapult", "katapulto", "catopult" },
        { "a mythical creature that is half human, half horse", "centaur", "sentaoro", "centuar" },
        { "a lizard that can change its color", "chameleon", "kamaleono", "chamelion" },
        { "a hanging decorative light fixture", "chandelier", "shandeler", "chandalier" },
        { "the pupal stage of a butterfly", "chrysalis", "chrysaliso", "chrysalys" },
        { "a colorful parrot with a crest", "cockatoo", "kokatu", "cockatou" },
        { "a large ancient Roman theater", "colosseum", "coloseo", "coliseum" },
        { "a bridge that can be raised or lowered", "drawbridge", "drawbriggo", "drawbrige" },
        { "a carved figure often on buildings", "gargoyle", "gargoyo", "gargoil" },
        { "a professional fighter in ancient Rome", "gladiator", "gladiato", "gladiater" },
        { "a device used for executions by decapitation", "guillotine", "guilotino", "guillotene" },
        { "a spear-like weapon for fishing or combat", "harpoon", "harpono", "harpune" },
        { "ancient writing system of Egypt using symbols", "hieroglyph", "hyerogliffo", "hieroglif" },
        { "an optical toy showing colorful patterns", "kaleidoscope", "kaleidoskopo", "kaleidoskope" },
        { "a complex network of paths", "labyrinth", "labirinto", "laberinth" },
        { "a large tent for events or shows", "marquee", "markweo", "marquea" },
        { "a collection of exotic animals", "menagerie", "menajero", "managerie" },
        { "a mythical creature with the body of a man and head of a bull", "minotaur", "minotauro", "minotar" },
        { "a single massive upright stone", "monolith", "monolito", "monoleth" },
        { "a whale with a long tusk", "narwhal", "narwalo", "narwal" },
        { "a tall stone pillar or monument", "obelisk", "obelisko", "obelics" },
        { "a dark volcanic glass", "obsidian", "obsidiano", "obsidien" },
        { "a dungeon with a secret trapdoor", "oubliette", "oblietto", "oubliete" },
        { "a famous temple in Athens", "parthenon", "parthenono", "parthanon" },
        { "a tube for viewing distant objects", "periscope", "periskopo", "perascope" },
        { "a ruler of ancient Egypt", "pharaoh", "faraono", "pharoh" },
        { "a duck-billed egg-laying mammal", "platypus", "platipo", "platypos" },
        { "a heavy gate that slides vertically", "portcullis", "portkulo", "portculis" },
        { "a massive triangular structure", "pyramid", "piramido", "pyramyd" },
        { "a small marsupial from Australia", "quokka", "quokko", "quoka" },
        { "a Japanese warrior", "samurai", "samuraio", "samuray" },
        { "a stone coffin, usually for royalty", "sarcophagus", "sarkofago", "sarcofagus" },
        { "an arachnid with a sting", "scorpion", "skorpiono", "scorpeon" },
        { "an ancient navigation instrument", "sextant", "sekstanto", "sextent" },
        { "a mythical creature with a lion's body and human head", "sphinx", "sfinkso", "sfinx" },
        { "a handheld telescope", "spyglass", "spyglasso", "spyglas" },
        { "a large spider with long legs", "tarantula", "tarantulo", "tarantala" },
        { "a medieval siege engine that throws stones", "trebuchet", "trebuchato", "trebuchat" },
        { "a three-pronged spear", "trident", "tridanto", "tridant" },
        { "a Scandinavian warrior or raider", "viking", "vikingo", "vyking" },
        { "a musical instrument with keys", "xylophone", "zylophono", "xilophone" },
        { "a stepped pyramid from ancient Mesopotamia", "ziggurat", "zigurato", "zigurat" }
    };

    public static string[,] mediumSentencePairs = new string[,]
    {
        { "The students will ____ for their final exams tomorrow.", "study", "relax", "ignore" },
        { "The construction workers ____ the new building quickly.", "built", "repaired", "destroyed" },
        { "The author ____ a fascinating novel last year.", "wrote", "reviewed", "read" },
        { "The chef ____ the ingredients carefully for the recipe.", "measured", "washed", "mixed" },
        { "The athlete ____ every day to improve his skills.", "trains", "rests", "sleeps" },
        { "The musician ____ a beautiful song for the audience.", "performed", "listened", "recorded" },
        { "The gardener ____ the plants every morning.", "waters", "trims", "plants" },
        { "The programmer ____ a new software application.", "developed", "tested", "installed" },
        { "The detective ____ the mystery carefully.", "investigated", "observed", "solved" },
        { "The artist ____ the landscape with vibrant colors.", "painted", "sketched", "drew" },
        { "The scientist ____ the results to confirm the hypothesis.", "analyzed", "ignored", "recorded" },
        { "The students ____ quietly while the teacher explained.", "listened", "whispered", "slept" },
        { "The captain ____ the ship safely to shore.", "guided", "followed", "sailed" },
        { "The nurse ____ the patient throughout the night.", "cared for", "watched", "examined" },
        { "The engineer ____ a new solution to the problem.", "designed", "reviewed", "tested" },
        { "The actor ____ his lines before the performance.", "practiced", "forgot", "memorized" },
        { "The librarian ____ the books back on the shelves.", "organized", "stacked", "sorted" },
        { "The explorer ____ new regions of the jungle.", "discovered", "visited", "mapped" },
        { "The reporter ____ the event for the evening news.", "covered", "announced", "filmed" },
        { "The professor ____ the topic in great detail.", "explained", "mentioned", "discussed" }
    };
    #endregion

    #region Hard
    public static string[,] hardSpellingPairs = new string[,]
    {
        // Animals & nature
        { "large Australian animal that jumps", "kangaroo", "kangarooo", "kangeroo" },
        { "large reptile with powerful jaws", "alligator", "aligater", "alligater" },
        { "large black big cat", "panther", "panthar", "pantor" },
        { "sea creature with eight arms", "octopus", "octupus", "octapus" },
        { "flightless bird from cold regions", "penguin", "penguine", "pengwin" },
        { "mountain that erupts with lava", "volcano", "volcanoe", "volcane" },
        { "powerful tropical cyclone", "hurricane", "hurricaine", "huricane" },
        { "huge slow-moving river of ice", "glacier", "glaceir", "glaciar" },
        { "severe snow storm with strong winds", "blizzard", "blizzurd", "blizard" },
        // Environment & tools
        { "wild natural area with little human presence", "wilderness", "wilderniss", "wildernes" },
        { "tool that shows north, south, east, west", "compass", "compas", "compess" },
        { "optical tool to see distant objects", "telescope", "telescop", "telascope" },
        { "tool to see very small things magnified", "microscope", "microscop", "micrascope" },
        { "decorative items worn on the body", "jewelry", "jewelery", "jewlery" },
        { "tool used to create music", "instrument", "insturment", "instrament" },
        // Body & food
        { "hair above the eye", "eyebrow", "eyebrou", "eyebrau" },
        { "flat bone in the upper back", "shoulderblade", "sholderblade", "shoulderblayd" },
        { "joint in the finger", "knuckle", "knuckel", "nuckle" },
        { "backbone", "spine", "spain", "spyne" },
        { "muscle in the mouth used for tasting", "tongue", "toung", "tung" },
        { "white vegetable that looks like a brain", "cauliflower", "coliflower", "cauliflouwer" },
        { "green fruit with a large pit inside", "avocado", "avacado", "avocodo" },
        { "long green vegetable eaten in salads", "cucumber", "cuccumber", "cucamber" },
        { "sweet brown food made from cocoa", "chocolate", "choclate", "chocolet" },
        { "long Italian noodle dish", "spaghetti", "spagetti", "spagheti" },
        // People & jobs
        { "person who studies or does experiments", "scientist", "sciencist", "scientest" },
        { "person who travels to space", "astronaut", "astroanut", "astronot" },
        { "leader of a country", "president", "presedent", "presidant" },
        { "person who comes to see you", "visitor", "visiter", "vizitor" },
        { "people who live near you", "neighbors", "neighbours", "naybors" },
        // Actions & traits
        { "to find something new", "discover", "discovar", "disover" },
        { "to go to new places to learn", "explore", "explor", "eksplore" },
        { "to build something", "construct", "construck", "construkt" },
        { "to look at similarities and differences", "compare", "compair", "compar" },
        { "to make a choice", "decide", "deside", "decyde" },
        { "willing to give and share", "generous", "genorous", "generus" },
        { "wanting to know more", "curious", "curous", "curius" },
        { "feeling worried or nervous", "anxious", "angshus", "anxius" },
        { "feeling thankful", "grateful", "greatful", "gratefull" },
        { "believing in your own abilities", "confident", "confidant", "confadent" },
        // Advanced / abstract
        { "advanced human society with cities and government", "civilization", "civilisation", "sivilization" },
        { "something newly created or invented", "invention", "inventon", "invension" },
        { "mathematical statement with = sign", "equation", "equasion", "equatian" },
        { "system of communication (English, Spanish…)", "language", "langwage", "languege" },
        { "feeling ashamed or shy", "embarrassed", "embarassed", "embarrased" },
        { "very shy or easily embarrassed", "bashful", "bashfull", "bashfal" },
        { "aware of something", "conscious", "concious", "consious" },
        { "very fancy and expensive", "extravagant", "extravagent", "extravagint" },
        { "small orange-like fruit", "apricot", "apricott", "apracot" },
        { "to confuse or make someone very puzzled", "discombobulate", "discombobulated", "discombobulat" },
    };

    public static string[,] hardSentencePairs = new string[,]
    {
        { "The explorers decided to ____ the unknown cave system.", "explore", "explain", "examine" },
        { "The mathematician solved a very difficult ____.", "equation", "question", "problem" },
        { "She felt extremely ____ after making a mistake in public.", "embarrassed", "impressed", "ashamed" },
        { "The inventor received a patent for his latest ____.", "invention", "convention", "creation" },
        { "The ancient ____ developed complex writing systems.", "civilization", "university", "society" },
        { "He remained ____ of his surroundings even while sleeping.", "conscious", "confident", "aware" },
        { "The wealthy family lived in a very ____ mansion.", "extravagant", "elegant", "expensive" },
        { "The shy child felt quite ____ around strangers.", "bashful", "playful", "timid" },
        { "The team worked together to ____ a new bridge.", "construct", "conduct", "create" },
        { "She always tries to ____ different points of view.", "compare", "prepare", "consider" },
        { "The ____ student asked many thoughtful questions.", "curious", "furious", "intelligent" },
        { "He felt very ____ about the upcoming exam results.", "anxious", "serious", "nervous" },
        { "The ____ donation helped build the new library.", "generous", "famous", "large" },
        { "Astronauts must be extremely ____ to survive in space.", "confident", "different", "brave" },
        { "We are very ____ for all your help during the project.", "grateful", "careful", "thankful" },
        { "The chef carefully ____ the exotic ingredients.", "prepared", "compared", "selected" },
        { "The ____ erupted violently after many years of silence.", "volcano", "tornado", "mountain" },
        { "The ____ moved slowly across the landscape over centuries.", "glacier", "river", "desert" },
        { "The pilot navigated through the dangerous ____.", "hurricane", "mountain", "storm" },
        { "She used a ____ to examine the tiny crystals.", "microscope", "telescope", "magnifier" },
    };
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // CORE LOGIC
    // ─────────────────────────────────────────────────────────────────────────────

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
        if (!PlayerPrefs.HasKey("CurrentTaskID")) return false;

        int taskID = PlayerPrefs.GetInt("CurrentTaskID", -1);
        int questionIndex = PlayerPrefs.GetInt("CurrentTaskQuestionIndex", -1);
        bool isSpelling = PlayerPrefs.GetInt("CurrentTaskIsSpelling", 1) == 1;

        if (taskID == -1 || questionIndex == -1) return false;

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
        if (clueImageObject != null) clueImageObject.SetActive(false);

        if (!TryLoadDailyTaskQuestion())
        {
            SetRandomQuestion();
        }
    }

    // ------------------------------------------------------------
    // Simple position rotation - correct answer rotates among the 3 positions
    // ------------------------------------------------------------
    public void SetSpellingQuestion(int index)
    {
        if (index < 0 || index >= activeSpellingPairs.GetLength(0))
        {
            Debug.LogError($"Invalid spelling question index: {index}");
            return;
        }

        string clue = activeSpellingPairs[index, 0];
        string correct = activeSpellingPairs[index, 1];
        string wrong1 = activeSpellingPairs[index, 2];
        string wrong2 = activeSpellingPairs[index, 3];

        clueText.text = clue;
        correctAnswer = correct;
        currentQuestionIndex = index;
        isSentenceQuestion = false;
        audioPlayed = false;

        // Randomly choose which position gets the correct answer (0-2)
        int correctPosition = Random.Range(0, 3);
        
        // Assign all 3 options based on the correct position
        AssignOptions(correct, wrong1, wrong2, correctPosition);
        
        UpdateClueVisibility();

        Debug.Log($"Spelling Question: {clue} | Correct: {correct} at position {correctPosition} | Wrong: {wrong1}, {wrong2}");
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
        string wrong1 = activeSentencePairs[index, 2];
        string wrong2 = activeSentencePairs[index, 3];

        clueText.text = sentence;
        correctAnswer = correct;
        currentQuestionIndex = index;
        isSentenceQuestion = true;
        audioPlayed = false;

        // Randomly choose which position gets the correct answer (0-2)
        int correctPosition = Random.Range(0, 3);
        
        // Assign all 3 options based on the correct position
        AssignOptions(correct, wrong1, wrong2, correctPosition);
        
        UpdateClueVisibility();

        Debug.Log($"Sentence Question: {sentence} | Correct: {correct} at position {correctPosition} | Wrong: {wrong1}, {wrong2}");
    }

    // Assign options to the 3 text fields with the correct answer at the specified position
    private void AssignOptions(string correct, string wrong1, string wrong2, int correctPosition)
    {
        // Create an array of the 3 TMP texts
        TMP_Text[] optionTexts = new TMP_Text[] { jumpText, slideText, option3Text };
        
        // Fill with wrong answers first
        for (int i = 0; i < 3; i++)
        {
            if (i == correctPosition)
            {
                optionTexts[i].text = correct;
            }
            else
            {
                // Distribute the two wrong answers among the remaining 2 positions
                // We have 2 wrong answers for 2 remaining slots
                if (i < correctPosition)
                {
                    optionTexts[i].text = wrong1;
                }
                else
                {
                    optionTexts[i].text = wrong2;
                }
            }
        }
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

    // ─────────────────────────────────────────────────────────────────────────────
    // AUDIO
    // ─────────────────────────────────────────────────────────────────────────────
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

    // ─────────────────────────────────────────────────────────────────────────────
    // TRIGGER & UI
    // ─────────────────────────────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInTrigger = true;

        UpdateClueVisibility();

        if (playAudioOnTrigger)
            PlayQuestionAudio();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInTrigger = false;

        if (clueTextObject != null) clueTextObject.SetActive(false);
        if (clueImageObject != null) clueImageObject.SetActive(false);
    }

    private void UpdateClueVisibility()
    {
        if (!playerInTrigger) return;

        // Sentence questions always show text clue, never image
        if (isSentenceQuestion)
        {
            if (clueTextObject != null) clueTextObject.SetActive(true);
            if (clueImageObject != null) clueImageObject.SetActive(false);
        }
        // Spelling questions: show image if available, else text
        else
        {
            if (HasClueImage())
            {
                if (cluePic != null && currentQuestionIndex >= 0 && currentQuestionIndex < clueImages.Length)
                    cluePic.sprite = clueImages[currentQuestionIndex];
                if (clueImageObject != null) clueImageObject.SetActive(true);
                if (clueTextObject != null) clueTextObject.SetActive(false);
            }
            else
            {
                if (clueTextObject != null) clueTextObject.SetActive(true);
                if (clueImageObject != null) clueImageObject.SetActive(false);
            }
        }
    }

    public void TriggerQuestionAudio() => PlayQuestionAudio();

    public void ShowClueText()
    {
        if (playerInTrigger)
            UpdateClueVisibility();
    }
    // Call this when player answers correctly
    public void AddWordCount()
    {
        int currentWords = PlayerPrefs.GetInt("TotalWordCount", 0);
        currentWords++;
        PlayerPrefs.SetInt("TotalWordCount", currentWords);
        PlayerPrefs.Save();

        Debug.Log("Total Words: " + currentWords);
    }

    public void HideClueText()
    {
        if (clueTextObject != null) clueTextObject.SetActive(false);
        if (clueImageObject != null) clueImageObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // UTILITIES
    // ─────────────────────────────────────────────────────────────────────────────
    public string GetCurrentDifficulty() => SceneManager.GetActiveScene().name;
    public int GetSpellingQuestionCount() => activeSpellingPairs?.GetLength(0) ?? 0;
    public int GetSentenceQuestionCount() => activeSentencePairs?.GetLength(0) ?? 0;
    public bool IsSentenceQuestion() => isSentenceQuestion;

    public Sprite GetCurrentClueImage()
    {
        if (clueImages != null && currentQuestionIndex >= 0 && currentQuestionIndex < clueImages.Length)
            return clueImages[currentQuestionIndex];
        return null;
    }

    public bool HasClueImage()
    {
        return clueImages != null && currentQuestionIndex >= 0 && currentQuestionIndex < clueImages.Length && clueImages[currentQuestionIndex] != null;
    }
}