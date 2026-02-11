using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class QuestionRandomizer : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text jumpText;
    public TMP_Text slideText;
    public TMP_Text option3Text; // NEW
    public TMP_Text option4Text; // NEW
    public GameObject clueTextObject;
    public TMP_Text clueText;
    public GameObject clueImageObject; // NEW: Image object for spelling hurdles
    public Image cluePic;

    [Header("Clue Images")]
    public Sprite[] clueImages; // Array for clue images in word/spelling hurdles

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
    private bool playerInTrigger = false; // Track if player is inside trigger

    // Active sets based on scene
    private string[,] activeSpellingPairs;
    private string[,] activeSentencePairs;

    // Difficulty word/sentence banks
    #region Easy
    public static string[,] easySpellingPairs = new string[,]
    {
        { "a common pet that barks", "dog", "dag", "dug", "dig" },
        { "something you wear on your head", "hat", "hoat", "hut", "het" },
        { "a color between red and white", "pink", "ponk", "pank", "pynk" },
        { "the star in the sky during daytime", "sun", "san", "son", "syn" },
        { "body part used for walking", "leg", "log", "lig", "lug" },
        { "food from animals, often eaten cooked", "meat", "met", "mait", "mete" },
        { "a small container for drinking", "cup", "cawp", "cop", "cap" },
        { "you listen with this", "ear", "eer", "eur", "air" },
        { "a plant with leaves and branches", "tree", "tri", "trea", "trie" },
        { "opposite of white", "black", "bolck", "blak", "bleck" },
        { "moving quickly", "fast", "fest", "fust", "fist" },
        { "activity in water", "swim", "swom", "swam", "swem" },
        { "refers to the person being addressed", "you", "yoo", "yu", "yew" },
        { "where you sleep at night", "bed", "ved", "bad", "bid" },
        { "part of your body used to hold things", "hand", "henk", "hend", "hond" },
        { "a flying animal with feathers", "bird", "burd", "berd", "bird" },
        { "white drink from cows", "milk", "milx", "melk", "malk" },
        { "to leap into the air", "jump", "jomp", "jamp", "jemp" },
        { "baked food made from flour", "bread", "bredd", "bred", "braed" },
        { "sweet dessert", "cake", "cak", "caek", "keik" },
        { "used for walking or running", "foot", "fut", "fot", "faat" },
        { "female child", "girl", "gurl", "gerl", "gril" },
        { "part of your face used to smell", "nose", "nosh", "noze", "nos" },
        { "color of the sky on a clear day", "blue", "blu", "blew", "bloo" },
        { "vehicle with wheels", "car", "cor", "kar", "cir" },
        { "yellow vegetable", "corn", "curn", "korn", "cron" },
        { "mother's sister", "aunt", "ant", "awnt", "aont" },
        { "farm animal that gives milk", "cow", "coe", "kow", "caw" },
        { "feeling of joy", "happy", "hoppy", "hapi", "hapey" },
        { "color of an apple", "red", "rad", "reed", "rid" },
        { "limb attached to shoulder", "arm", "arum", "erm", "orm" },
        { "a companion", "friend", "frend", "freind", "frind" },
        { "container with sides", "box", "bocs", "boks", "bax" },
        { "used to carry things", "bag", "bog", "beg", "buge" },
        { "furniture to sit on", "chair", "chare", "chear", "cheir" },
        { "jump on one foot", "hop", "hut", "hap", "hep" },
        { "a male parent", "dad", "did", "dod", "ded" },
        { "a young child", "kid", "kod", "ked", "kyd" },
        { "female chicken", "hen", "han", "hin", "heon" },
        { "staple food, often eaten with dishes", "rice", "rais", "ryce", "riece" },
        { "opposite of fast", "slow", "slew", "sloe", "slo" },
        { "reading material", "book", "buck", "boke", "bouk" },
        { "farm animal that oinks", "pig", "peg", "pog", "pyg" },
        { "very young human", "baby", "bebe", "beby", "babie" },
        { "refers to a male person", "him", "hem", "hym", "hum" },
        { "makes ringing sound", "bell", "vel", "bel", "behl" },
        { "plaything for children", "toy", "toi", "tay", "toey" },
        { "female parent", "mom", "mum", "mam", "mem" },
        { "feeling of sadness", "sad", "sed", "sod", "saad" },
        { "part of a tree or plant", "leaf", "loaf", "leef", "leif" },
        { "to form letters on paper", "write", "rite", "writ", "wryte" },
        { "color opposite of black", "white", "whyte", "wite", "whit" },
        { "small furry pet that meows", "cat", "gat", "kat", "cet" },
        { "farm animal with horns", "goat", "got", "gote", "goet" },
        { "furniture for working", "desk", "deks", "dask", "deske" },
        { "a celestial object at night", "moon", "mun", "mune", "moun" },
        { "falling water from clouds", "rain", "rein", "rayn", "rane" },
        { "frozen precipitation", "snow", "snou", "snoe", "sno" },
        { "part of the face used for speaking", "lip", "lap", "lep", "lyp" },
        { "sweet spread for bread", "jam", "jem", "jom", "jame" },
        { "young male child", "boy", "boi", "bouy", "boe" },
        { "brother of your parent", "uncle", "unkl", "uncal", "onkle" },
        { "good or desirable", "good", "gud", "goud", "goode" },
        { "opposite of good", "bad", "badd", "bod", "bede" },
        { "to move on feet at a moderate pace", "walk", "wok", "wolk", "wauk" },
        { "to be upright on feet", "stand", "stend", "stond", "stande" },
        { "to make music with voice", "sing", "sung", "seng", "syng" },
        { "transport by road", "bus", "bos", "bas", "buss" },
        { "passage to enter or exit", "door", "dor", "doer", "dour" },
        { "something that shows location", "map", "mop", "mep", "maap" },
        { "oval farm product from birds", "egg", "egh", "eg", "agg" },
        { "edible fruit shaped like a bulb", "pear", "per", "pare", "peir" },
        { "small swimming waterbird", "duck", "duc", "duk", "dack" },
        { "small rodent", "mouse", "mawz", "mous", "mouce" },
        { "air in motion", "wind", "windz", "wynd", "wend" },
    };

    public static string[,] easySentencePairs = new string[,]
    {
        { "The chef ____ a delicious meal.", "cooked", "taught", "slept", "flew" },
        { "The teacher ____ the students about history.", "taught", "slept", "flew", "cooked" },
        { "The cat ____ on the sunny windowsill.", "slept", "flew", "cooked", "taught" },
        { "The pianist ____ a beautiful melody.", "played", "ran", "cried", "tumbled" },
        { "The athlete ____ across the finish line.", "ran", "cried", "tumbled", "played" },
        { "The baby ____ when it's hungry.", "cries", "tumbles", "runs", "plays" },
        { "The photographer ____ pictures of the sunset.", "took", "repaired", "studied", "painted" },
        { "The mechanic ____ the broken engine.", "repaired", "studied", "painted", "took" },
        { "The student ____ for the upcoming exam.", "studied", "painted", "took", "repaired" },
        { "The artist ____ a portrait on the canvas.", "painted", "wagged", "sang", "watered" },
        { "The dog ____ its tail happily.", "wagged", "sang", "watered", "painted" },
        { "The singer ____ in front of the audience.", "sang", "watered", "wrote", "wagged" },
        { "The gardener ____ the flowers every morning.", "watered", "wrote", "examined", "sang" },
        { "The writer ____ a new short story.", "wrote", "examined", "drove", "watered" },
        { "The doctor ____ the patient carefully.", "examined", "drove", "built", "wrote" },
        { "The driver ____ the car down the highway.", "drove", "built", "experimented", "examined" },
        { "The carpenter ____ a sturdy table.", "built", "experimented", "served", "drove" },
        { "The scientist ____ an experiment in the lab.", "experimented", "served", "swam", "built" },
        { "The waiter ____ food to the customers.", "served", "swam", "cooked", "experimented" },
        { "The swimmer ____ laps in the pool.", "swam", "cooked", "taught", "served" }
    };
    #endregion

    #region Medium
    public static string[,] mediumSpellingPairs = new string[,]
    {
        { "a burrowing African mammal with a long nose", "aardvark", "aardvarko", "ardvark", "aardwerk" },
        { "an open-air venue for performances", "amphitheater", "amfiteatro", "amphitheatre", "ampitheater" },
        { "a small armored mammal that rolls into a ball", "armadillo", "armadilo", "armadelo", "armodillo" },
        { "an ancient astronomical instrument for measuring stars", "astrolabe", "astrolabbo", "astrolab", "astralabe" },
        { "a rare aquatic salamander with external gills", "axolotl", "axoloto", "axolotal", "axalotl" },
        { "an ancient missile weapon that launches projectiles", "ballista", "balista", "ballisto", "ballistia" },
        { "a defensive wall on top of a castle", "battlement", "batlemanto", "battlemont", "battelment" },
        { "a rotating amusement ride with seats", "carousel", "karuselo", "carosel", "carouzel" },
        { "a medieval device for hurling heavy stones", "catapult", "katapulto", "catopult", "catapalt" },
        { "a mythical creature that is half human, half horse", "centaur", "sentaoro", "centuar", "sentar" },
        { "a lizard that can change its color", "chameleon", "kamaleono", "chamelion", "chameleun" },
        { "a hanging decorative light fixture", "chandelier", "shandeler", "chandalier", "chandeleer" },
        { "the pupal stage of a butterfly", "chrysalis", "chrysaliso", "chrysalys", "crysalis" },
        { "a colorful parrot with a crest", "cockatoo", "kokatu", "cockatou", "cockatu" },
        { "a large ancient Roman theater", "colosseum", "coloseo", "coliseum", "coloseum" },
        { "a bridge that can be raised or lowered", "drawbridge", "drawbriggo", "drawbrige", "drawbridg" },
        { "a carved figure often on buildings", "gargoyle", "gargoyo", "gargoil", "gargoyal" },
        { "a professional fighter in ancient Rome", "gladiator", "gladiato", "gladiater", "gladietor" },
        { "a device used for executions by decapitation", "guillotine", "guilotino", "guillotene", "gilotine" },
        { "a spear-like weapon for fishing or combat", "harpoon", "harpono", "harpune", "harpon" },
        { "ancient writing system of Egypt using symbols", "hieroglyph", "hyerogliffo", "hieroglif", "hieroglyf" },
        { "an optical toy showing colorful patterns", "kaleidoscope", "kaleidoskopo", "kaleidoskope", "kaliedoscope" },
        { "a complex network of paths", "labyrinth", "labirinto", "laberinth", "labrynth" },
        { "a large tent for events or shows", "marquee", "markweo", "marquea", "markee" },
        { "a collection of exotic animals", "menagerie", "menajero", "managerie", "menagery" },
        { "a mythical creature with the body of a man and head of a bull", "minotaur", "minotauro", "minotar", "minetaur" },
        { "a single massive upright stone", "monolith", "monolito", "monoleth", "monolyth" },
        { "a whale with a long tusk", "narwhal", "narwalo", "narwal", "narwhale" },
        { "a tall stone pillar or monument", "obelisk", "obelisko", "obelics", "obelysk" },
        { "a dark volcanic glass", "obsidian", "obsidiano", "obsidien", "absidian" },
        { "a dungeon with a secret trapdoor", "oubliette", "oblietto", "oubliete", "ubliette" },
        { "a famous temple in Athens", "parthenon", "parthenono", "parthanon", "parthenun" },
        { "a tube for viewing distant objects", "periscope", "periskopo", "perascope", "perescope" },
        { "a ruler of ancient Egypt", "pharaoh", "faraono", "pharoh", "pharoah" },
        { "a duck-billed egg-laying mammal", "platypus", "platipo", "platypos", "platapus" },
        { "a heavy gate that slides vertically", "portcullis", "portkulo", "portculis", "portcullus" },
        { "a massive triangular structure", "pyramid", "piramido", "pyramyd", "piramid" },
        { "a small marsupial from Australia", "quokka", "quokko", "quoka", "quokah" },
        { "a Japanese warrior", "samurai", "samuraio", "samuray", "samourai" },
        { "a stone coffin, usually for royalty", "sarcophagus", "sarkofago", "sarcofagus", "sarcophagos" },
        { "an arachnid with a sting", "scorpion", "skorpiono", "scorpeon", "scorpian" },
        { "an ancient navigation instrument", "sextant", "sekstanto", "sextent", "sextan" },
        { "a mythical creature with a lion's body and human head", "sphinx", "sfinkso", "sfinx", "sphinks" },
        { "a handheld telescope", "spyglass", "spyglasso", "spyglas", "spieglass" },
        { "a large spider with long legs", "tarantula", "tarantulo", "tarantala", "tarentula" },
        { "a medieval siege engine that throws stones", "trebuchet", "trebuchato", "trebuchat", "trebushet" },
        { "a three-pronged spear", "trident", "tridanto", "tridant", "trydent" },
        { "a Scandinavian warrior or raider", "viking", "vikingo", "vyking", "vikin" },
        { "a musical instrument with keys", "xylophone", "zylophono", "xilophone", "xylofone" },
        { "a stepped pyramid from ancient Mesopotamia", "ziggurat", "zigurato", "zigurat", "ziggaret" }
    };

    public static string[,] mediumSentencePairs = new string[,]
    {
        { "The students will ____ for their final exams tomorrow.", "study", "relax", "ignore", "forget" },
        { "The construction workers ____ the new building quickly.", "built", "repaired", "destroyed", "painted" },
        { "The author ____ a fascinating novel last year.", "wrote", "reviewed", "read", "published" },
        { "The chef ____ the ingredients carefully for the recipe.", "measured", "washed", "mixed", "tasted" },
        { "The athlete ____ every day to improve his skills.", "trains", "rests", "sleeps", "eats" },
        { "The musician ____ a beautiful song for the audience.", "performed", "listened", "recorded", "practiced" },
        { "The gardener ____ the plants every morning.", "waters", "trims", "plants", "picks" },
        { "The programmer ____ a new software application.", "developed", "tested", "installed", "deleted" },
        { "The detective ____ the mystery carefully.", "investigated", "observed", "solved", "ignored" },
        { "The artist ____ the landscape with vibrant colors.", "painted", "sketched", "drew", "photographed" },
        { "The scientist ____ the results to confirm the hypothesis.", "analyzed", "ignored", "recorded", "published" },
        { "The students ____ quietly while the teacher explained.", "listened", "whispered", "slept", "talked" },
        { "The captain ____ the ship safely to shore.", "guided", "followed", "sailed", "steered" },
        { "The nurse ____ the patient throughout the night.", "cared for", "watched", "examined", "monitored" },
        { "The engineer ____ a new solution to the problem.", "designed", "reviewed", "tested", "implemented" },
        { "The actor ____ his lines before the performance.", "practiced", "forgot", "memorized", "read" },
        { "The librarian ____ the books back on the shelves.", "organized", "stacked", "sorted", "arranged" },
        { "The explorer ____ new regions of the jungle.", "discovered", "visited", "mapped", "photographed" },
        { "The reporter ____ the event for the evening news.", "covered", "announced", "filmed", "reported" },
        { "The professor ____ the topic in great detail.", "explained", "mentioned", "discussed", "summarized" }
    };
    #endregion

    #region Hard
    public static string[,] hardSpellingPairs = new string[,]
    {
        // Animals & nature
        { "large Australian animal that jumps", "kangaroo", "kangarooo", "kangeroo", "kangarou" },
        { "large reptile with powerful jaws", "alligator", "aligater", "alligater", "aligator" },
        { "large black big cat", "panther", "panthar", "pantor", "panthur" },
        { "sea creature with eight arms", "octopus", "octupus", "octapus", "octopas" },
        { "flightless bird from cold regions", "penguin", "penguine", "pengwin", "penquin" },
        { "mountain that erupts with lava", "volcano", "volcanoe", "volcane", "vulcano" },
        { "powerful tropical cyclone", "hurricane", "hurricaine", "huricane", "hurrican" },
        { "huge slow-moving river of ice", "glacier", "glaceir", "glaciar", "glasier" },
        { "severe snow storm with strong winds", "blizzard", "blizzurd", "blizard", "blizzerd" },
        // Environment & tools
        { "wild natural area with little human presence", "wilderness", "wilderniss", "wildernes", "wildernis" },
        { "tool that shows north, south, east, west", "compass", "compas", "compess", "compos" },
        { "optical tool to see distant objects", "telescope", "telescop", "telascope", "telescupe" },
        { "tool to see very small things magnified", "microscope", "microscop", "micrascope", "micrescope" },
        { "decorative items worn on the body", "jewelry", "jewelery", "jewlery", "jewellery" },
        { "tool used to create music", "instrument", "insturment", "instrament", "instrumant" },
        // Body & food
        { "hair above the eye", "eyebrow", "eyebrou", "eyebrau", "eyebrow" },
        { "flat bone in the upper back", "shoulderblade", "sholderblade", "shoulderblayd", "sholderblayd" },
        { "joint in the finger", "knuckle", "knuckel", "nuckle", "knuckal" },
        { "backbone", "spine", "spain", "spyne", "spien" },
        { "muscle in the mouth used for tasting", "tongue", "toung", "tung", "tonge" },
        { "white vegetable that looks like a brain", "cauliflower", "coliflower", "cauliflouwer", "coliflouwer" },
        { "green fruit with a large pit inside", "avocado", "avacado", "avocodo", "avacodo" },
        { "long green vegetable eaten in salads", "cucumber", "cuccumber", "cucamber", "cukumber" },
        { "sweet brown food made from cocoa", "chocolate", "choclate", "chocolet", "chocholate" },
        { "long Italian noodle dish", "spaghetti", "spagetti", "spagheti", "spaghetty" },
        // People & jobs
        { "person who studies or does experiments", "scientist", "sciencist", "scientest", "scienctist" },
        { "person who travels to space", "astronaut", "astroanut", "astronot", "astronaut" },
        { "leader of a country", "president", "presedent", "presidant", "presedant" },
        { "person who comes to see you", "visitor", "visiter", "vizitor", "visitur" },
        { "people who live near you", "neighbors", "neighbours", "naybors", "neighburs" },
        // Actions & traits
        { "to find something new", "discover", "discovar", "disover", "discovor" },
        { "to go to new places to learn", "explore", "explor", "eksplore", "exploar" },
        { "to build something", "construct", "construck", "construkt", "constrict" },
        { "to look at similarities and differences", "compare", "compair", "compar", "compere" },
        { "to make a choice", "decide", "deside", "decyde", "desyde" },
        { "willing to give and share", "generous", "genorous", "generus", "genereus" },
        { "wanting to know more", "curious", "curous", "curius", "cureous" },
        { "feeling worried or nervous", "anxious", "angshus", "anxius", "ancsious" },
        { "feeling thankful", "grateful", "greatful", "gratefull", "gratful" },
        { "believing in your own abilities", "confident", "confidant", "confadent", "confidunt" },
        // Advanced / abstract
        { "advanced human society with cities and government", "civilization", "civilisation", "sivilization", "civilazation" },
        { "something newly created or invented", "invention", "inventon", "invension", "inventian" },
        { "mathematical statement with = sign", "equation", "equasion", "equatian", "equashen" },
        { "system of communication (English, Spanish…)", "language", "langwage", "languege", "langauge" },
        { "feeling ashamed or shy", "embarrassed", "embarassed", "embarrased", "embarased" },
        { "very shy or easily embarrassed", "bashful", "bashfull", "bashfal", "baschful" },
        { "aware of something", "conscious", "concious", "consious", "conshous" },
        { "very fancy and expensive", "extravagant", "extravagent", "extravagint", "extravegant" },
        { "small orange-like fruit", "apricot", "apricott", "apracot", "aprikot" },
        { "to confuse or make someone very puzzled", "discombobulate", "discombobulated", "discombobulat", "discombobolate" },
    };

    public static string[,] hardSentencePairs = new string[,]
    {
        { "The explorers decided to ____ the unknown cave system.", "explore", "explain", "examine", "exploit" },
        { "The mathematician solved a very difficult ____.", "equation", "question", "problem", "calculation" },
        { "She felt extremely ____ after making a mistake in public.", "embarrassed", "impressed", "ashamed", "upset" },
        { "The inventor received a patent for his latest ____.", "invention", "convention", "creation", "discovery" },
        { "The ancient ____ developed complex writing systems.", "civilization", "university", "society", "culture" },
        { "He remained ____ of his surroundings even while sleeping.", "conscious", "confident", "aware", "cautious" },
        { "The wealthy family lived in a very ____ mansion.", "extravagant", "elegant", "expensive", "luxurious" },
        { "The shy child felt quite ____ around strangers.", "bashful", "playful", "timid", "fearful" },
        { "The team worked together to ____ a new bridge.", "construct", "conduct", "create", "design" },
        { "She always tries to ____ different points of view.", "compare", "prepare", "consider", "analyze" },
        { "The ____ student asked many thoughtful questions.", "curious", "furious", "intelligent", "eager" },
        { "He felt very ____ about the upcoming exam results.", "anxious", "serious", "nervous", "worried" },
        { "The ____ donation helped build the new library.", "generous", "famous", "large", "charitable" },
        { "Astronauts must be extremely ____ to survive in space.", "confident", "different", "brave", "skilled" },
        { "We are very ____ for all your help during the project.", "grateful", "careful", "thankful", "appreciative" },
        { "The chef carefully ____ the exotic ingredients.", "prepared", "compared", "selected", "measured" },
        { "The ____ erupted violently after many years of silence.", "volcano", "tornado", "mountain", "geyser" },
        { "The ____ moved slowly across the landscape over centuries.", "glacier", "river", "desert", "ocean" },
        { "The pilot navigated through the dangerous ____.", "hurricane", "mountain", "storm", "clouds" },
        { "She used a ____ to examine the tiny crystals.", "microscope", "telescope", "magnifier", "lens" },
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
        if (collider != null)
            collider.isTrigger = true;
        else
            Debug.LogWarning("QuestionRandomizer: No collider found on question object. Add a Collider component.");

        if (clueTextObject != null)
            clueTextObject.SetActive(false);

        if (clueImageObject != null)
            clueImageObject.SetActive(false);

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
        string wrong1 = activeSpellingPairs[index, 2];
        string wrong2 = activeSpellingPairs[index, 3];
        string wrong3 = activeSpellingPairs[index, 4];

        clueText.text = clue;
        correctAnswer = correct;
        currentQuestionIndex = index;
        isSentenceQuestion = false;
        audioPlayed = false;

        // For spelling questions, show image if available, otherwise show text
        if (playerInTrigger)
        {
            if (HasClueImage())
            {
                if (cluePic != null)
                {
                    cluePic.sprite = clueImages[currentQuestionIndex];
                }
                if (clueImageObject != null)
                    clueImageObject.SetActive(true);
                if (clueTextObject != null)
                    clueTextObject.SetActive(false);
            }
            else
            {
                if (clueTextObject != null)
                    clueTextObject.SetActive(true);
                if (clueImageObject != null)
                    clueImageObject.SetActive(false);
            }
        }
        else
        {
            if (clueTextObject != null)
                clueTextObject.SetActive(false);
            if (clueImageObject != null)
                clueImageObject.SetActive(false);
        }

        // Randomize all 4 options
        string[] allOptions = new string[] { correct, wrong1, wrong2, wrong3 };
        System.Collections.Generic.List<string> shuffled = new System.Collections.Generic.List<string>(allOptions);
        
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        jumpText.text = shuffled[0];
        slideText.text = shuffled[1];
        option3Text.text = shuffled[2];
        option4Text.text = shuffled[3];

        Debug.Log($"Spelling Question: {clue} | Correct: {correct} | Wrong: {wrong1}, {wrong2}, {wrong3}");
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
        string wrong3 = activeSentencePairs[index, 4];

        clueText.text = sentence;
        correctAnswer = correct;
        currentQuestionIndex = index;
        isSentenceQuestion = true;
        audioPlayed = false;

        // For sentence questions, always show text clue, hide image
        if (clueTextObject != null)
            clueTextObject.SetActive(playerInTrigger);
            
        if (clueImageObject != null)
            clueImageObject.SetActive(false);

        // Randomize all 4 options
        string[] allOptions = new string[] { correct, wrong1, wrong2, wrong3 };
        System.Collections.Generic.List<string> shuffled = new System.Collections.Generic.List<string>(allOptions);
        
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        jumpText.text = shuffled[0];
        slideText.text = shuffled[1];
        option3Text.text = shuffled[2];
        option4Text.text = shuffled[3];

        Debug.Log($"Sentence Question: {sentence} | Correct: {correct} | Wrong: {wrong1}, {wrong2}, {wrong3}");
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
        if (audioSource == null || audioPlayed)
            return;

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
    if (!other.CompareTag("Player"))
        return;

    playerInTrigger = true;

    Debug.Log("Player ENTER trigger | isSentenceQuestion = " + isSentenceQuestion);

    // SENTENCE QUESTION → TEXT ONLY
    if (isSentenceQuestion)
    {
        if (clueTextObject != null)
            clueTextObject.SetActive(true);

        if (clueImageObject != null)
            clueImageObject.SetActive(false);
    }
    // SPELLING QUESTION → IMAGE if available, else TEXT
    else
    {
        if (HasClueImage())
        {
            if (cluePic != null && currentQuestionIndex >= 0)
                cluePic.sprite = clueImages[currentQuestionIndex];

            if (clueImageObject != null)
                clueImageObject.SetActive(true);

            if (clueTextObject != null)
                clueTextObject.SetActive(false);
        }
        else
        {
            if (clueTextObject != null)
                clueTextObject.SetActive(true);

            if (clueImageObject != null)
                clueImageObject.SetActive(false);
        }
    }

    if (playAudioOnTrigger)
        PlayQuestionAudio();
}

void OnTriggerExit(Collider other)
{
    if (!other.CompareTag("Player"))
        return;

    playerInTrigger = false;

    Debug.Log("Player EXIT trigger → hiding clues");

    if (clueTextObject != null)
        clueTextObject.SetActive(false);

    if (clueImageObject != null)
        clueImageObject.SetActive(false);
}


    public void TriggerQuestionAudio() => PlayQuestionAudio();

    public void ShowClueText()
    {
        if (isSentenceQuestion)
        {
            if (clueTextObject != null)
                clueTextObject.SetActive(true);
        }
        else
        {
            // Spelling question - show image if available, otherwise text
            if (HasClueImage())
            {
                if (cluePic != null)
                {
                    cluePic.sprite = clueImages[currentQuestionIndex];
                }
                if (clueImageObject != null)
                    clueImageObject.SetActive(true);
            }
            else
            {
                if (clueTextObject != null)
                    clueTextObject.SetActive(true);
            }
        }
    }

    public void HideClueText()
    {
        if (clueTextObject != null)
            clueTextObject.SetActive(false);
        if (clueImageObject != null)
            clueImageObject.SetActive(false);
    }

    public string GetCurrentDifficulty() => SceneManager.GetActiveScene().name;

    public int GetSpellingQuestionCount() => activeSpellingPairs?.GetLength(0) ?? 0;

    public int GetSentenceQuestionCount() => activeSentencePairs?.GetLength(0) ?? 0;

    public bool IsSentenceQuestion() => isSentenceQuestion;

    // New method to get clue image for current question
    public Sprite GetCurrentClueImage()
    {
        if (clueImages != null && currentQuestionIndex >= 0 && currentQuestionIndex < clueImages.Length)
        {
            return clueImages[currentQuestionIndex];
        }
        return null;
    }

    // New method to check if clue image is available for current question
    public bool HasClueImage()
    {
        return clueImages != null && currentQuestionIndex >= 0 && currentQuestionIndex < clueImages.Length && clueImages[currentQuestionIndex] != null;
    }
    #endregion

}
//working clue images
//working clue images
//v2 from claude