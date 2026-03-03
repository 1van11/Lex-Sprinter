using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

// Enum for slide direction of the clue elements
public enum SlideDirection
{
    Left,
    Right,
    Up,
    Down
}

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

    [Header("Letter Hurdle UI")]
    public TMP_Text collectedText;
    public TMP_Text targetWordText;
    public Image letterHurdleClueImage;  // This will use the same clue images

    [Header("Clue Images (Difficulty Based)")]
    public Sprite[] easyClueImages;
    public Sprite[] mediumClueImages;
    public Sprite[] hardClueImages;
    private Sprite[] currentClueImages;          // active set based on difficulty

    [Header("Pronunciation Sounds (Difficulty Based)")]
    public AudioClip[] easyPronunciationSounds;
    public AudioClip[] mediumPronunciationSounds;
    public AudioClip[] hardPronunciationSounds;
    private AudioClip[] currentPronunciationSounds; // active set

    [Header("Sentence Pronunciations (optional, could also be split)")]
    public AudioClip[] sentencePronunciations;   // may be extended later

    [Header("Spelling vs Sentence Frequency")]
    public int easySpellingBeforeSentence = 3;   // after 3 spelling, spawn a sentence
    public int mediumSpellingBeforeSentence = 4;
    public int hardSpellingBeforeSentence = 5;

    // Static so other scripts (like ObstacleSpawner) can read the current difficulty's value
    public static int CurrentSpellingBeforeSentence = 3; // default

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Trigger Settings")]
    public bool playAudioOnTrigger = true;

    [Header("Clue Image Animation")]
    public bool animateClueImage = true;
    public float imageAnimationDuration = 0.3f;
    public AnimationCurve imageAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public SlideDirection imageSlideDirection = SlideDirection.Up;
    public float imageSlideDistance = 100f;

    [Header("Clue Text Animation")]
    public bool animateClueText = true;
    public float textAnimationDuration = 0.3f;
    public AnimationCurve textAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public SlideDirection textSlideDirection = SlideDirection.Up;
    public float textSlideDistance = 100f;

    [Header("Letter Hurdle Settings")]
    public GameObject letterPrefab;
    public Transform letterSpawnParent;
    public float letterSpacing = 1f;
    public TMP_Text letterHurdleFeedbackText;
    public TMP_Text letterHurdleScoreText;
    public EventTimingManager bossManager;
    public PlayerFunctions playerFunctions;
    public ObstacleSpawner obstacleSpawner;

    // Current question state
    public string correctAnswer;
    private int currentQuestionIndex = -1;
    private bool isSentenceQuestion = false;
    private bool audioPlayed = false;
    private bool playerInTrigger = false;

    // Active sets based on scene
    private string[,] activeSpellingPairs;
    private string[,] activeSentencePairs;

    // Animation components for image
    private CanvasGroup imageCanvasGroup;
    private RectTransform imageRect;
    private Vector2 originalImagePos;
    private Coroutine imageAnimationCoroutine;

    // Animation components for text
    private CanvasGroup textCanvasGroup;
    private RectTransform textRect;
    private Vector2 originalTextPos;
    private Coroutine textAnimationCoroutine;

    // Letter Hurdle variables
    private string[] wordList;
    private string currentTargetWord;
    private List<string> shuffledWords;
    private int currentWordIndex = 0;
    private string previousCollectedText = "";
    private List<GameObject> spawnedLetters = new List<GameObject>();
    private Dictionary<string, Sprite> wordToImageMap = new Dictionary<string, Sprite>();

    // ─────────────────────────────────────────────────────────────────────────────
    // DIFFICULTY BANKS – each row has 4 columns: clue + correct + wrong1 + wrong2
    // ─────────────────────────────────────────────────────────────────────────────

    #region Easy
    public static string[,] easySpellingPairs = new string[,]
    {
/*-- 0 --*/  { "a common pet that barks", /*-- 0 --*/ "dog"/*-- 0 --*/, "dag", "dug" }, /*-- 0 --*/ 
/*-- 1 --*/  { "something you wear on your head", /*-- 1 --*/ "hat"/*-- 1 --*/, "hoat", "hut" }, /*-- 1 --*/ 
/*-- 2 --*/  { "a color between red and white", /*-- 2 --*/ "pink"/*-- 2 --*/, "ponk", "pank" }, /*-- 2 --*/ 
/*-- 3 --*/  { "the star in the sky during daytime", /*-- 3 --*/ "sun"/*-- 3 --*/, "san", "son" }, /*-- 3 --*/ 
/*-- 4 --*/  { "body part used for walking", /*-- 4 --*/ "leg"/*-- 4 --*/, "log", "lig" }, /*-- 4 --*/ 
/*-- 5 --*/  { "food from animals, often eaten cooked", /*-- 5 --*/ "meat"/*-- 5 --*/, "met", "mait" }, /*-- 5 --*/ 
/*-- 6 --*/  { "a small container for drinking", /*-- 6 --*/ "cup"/*-- 6 --*/, "cawp", "cop" }, /*-- 6 --*/ 
/*-- 7 --*/  { "you listen with this", /*-- 7 --*/ "ear"/*-- 7 --*/, "eer", "eur" }, /*-- 7 --*/ 
/*-- 8 --*/  { "a plant with leaves and branches", /*-- 8 --*/ "tree"/*-- 8 --*/, "tri", "trea" }, /*-- 8 --*/ 
/*-- 9 --*/  { "opposite of white", /*-- 9 --*/ "black"/*-- 9 --*/, "bolck", "blak" }, /*-- 9 --*/ 
/*-- 10 --*/ { "moving quickly", /*-- 10 --*/ "fast"/*-- 10 --*/, "fest", "fust" }, /*-- 10 --*/ 
/*-- 11 --*/ { "activity in water", /*-- 11 --*/ "swim"/*-- 11 --*/, "swom", "swam" }, /*-- 11 --*/ 
/*-- 12 --*/ { "refers to the person being addressed", /*-- 12 --*/ "you"/*-- 12 --*/, "yoo", "yu" }, /*-- 12 --*/ 
/*-- 13 --*/ { "where you sleep at night", /*-- 13 --*/ "bed"/*-- 13 --*/, "ved", "bad" }, /*-- 13 --*/ 
/*-- 14 --*/ { "part of your body used to hold things", /*-- 14 --*/ "hand"/*-- 14 --*/, "henk", "hend" }, /*-- 14 --*/ 
/*-- 15 --*/ { "a flying animal with feathers", /*-- 15 --*/ "bird"/*-- 15 --*/, "burd", "berd" }, /*-- 15 --*/ 
/*-- 16 --*/ { "white drink from cows", /*-- 16 --*/ "milk"/*-- 16 --*/, "milx", "melk" }, /*-- 16 --*/ 
/*-- 17 --*/ { "to leap into the air", /*-- 17 --*/ "jump"/*-- 17 --*/, "jomp", "jamp" }, /*-- 17 --*/ 
/*-- 18 --*/ { "baked food made from flour", /*-- 18 --*/ "bread"/*-- 18 --*/, "bredd", "bred" }, /*-- 18 --*/ 
/*-- 19 --*/ { "sweet dessert", /*-- 19 --*/ "cake"/*-- 19 --*/, "cak", "caek" }, /*-- 19 --*/ 
/*-- 20 --*/ { "used for walking or running", /*-- 20 --*/ "foot"/*-- 20 --*/, "fut", "fot" }, /*-- 20 --*/ 
/*-- 21 --*/ { "female child", /*-- 21 --*/ "girl"/*-- 21 --*/, "gurl", "gerl" }, /*-- 21 --*/ 
/*-- 22 --*/ { "part of your face used to smell", /*-- 22 --*/ "nose"/*-- 22 --*/, "nosh", "noze" }, /*-- 22 --*/ 
/*-- 23 --*/ { "color of the sky on a clear day", /*-- 23 --*/ "blue"/*-- 23 --*/, "blu", "blew" }, /*-- 23 --*/ 
/*-- 24 --*/ { "vehicle with wheels", /*-- 24 --*/ "car"/*-- 24 --*/, "cor", "kar" }, /*-- 24 --*/ 
/*-- 25 --*/ { "yellow vegetable", /*-- 25 --*/ "corn"/*-- 25 --*/, "curn", "korn" }, /*-- 25 --*/ 
/*-- 26 --*/ { "mother's sister", /*-- 26 --*/ "aunt"/*-- 26 --*/, "ant", "awnt" }, /*-- 26 --*/ 
/*-- 27 --*/ { "farm animal that gives milk", /*-- 27 --*/ "cow"/*-- 27 --*/, "coe", "kow" }, /*-- 27 --*/ 
/*-- 28 --*/ { "feeling of joy", /*-- 28 --*/ "happy"/*-- 28 --*/, "hoppy", "hapi" }, /*-- 28 --*/ 
/*-- 29 --*/ { "color of an apple", /*-- 29 --*/ "red"/*-- 29 --*/, "rad", "reed" }, /*-- 29 --*/ 
/*-- 30 --*/ { "limb attached to shoulder", /*-- 30 --*/ "arm"/*-- 30 --*/, "arum", "erm" }, /*-- 30 --*/ 
/*-- 31 --*/ { "a companion", /*-- 31 --*/ "friend"/*-- 31 --*/, "frend", "freind" }, /*-- 31 --*/ 
/*-- 32 --*/ { "container with sides", /*-- 32 --*/ "box"/*-- 32 --*/, "bocs", "boks" }, /*-- 32 --*/ 
/*-- 33 --*/ { "used to carry things", /*-- 33 --*/ "bag"/*-- 33 --*/, "bog", "beg" }, /*-- 33 --*/ 
/*-- 34 --*/ { "furniture to sit on", /*-- 34 --*/ "chair"/*-- 34 --*/, "chare", "chear" }, /*-- 34 --*/ 
/*-- 35 --*/ { "jump on one foot", /*-- 35 --*/ "hop"/*-- 35 --*/, "hut", "hap" }, /*-- 35 --*/ 
/*-- 36 --*/ { "a male parent", /*-- 36 --*/ "dad"/*-- 36 --*/, "did", "dod" }, /*-- 36 --*/ 
/*-- 37 --*/ { "a young child", /*-- 37 --*/ "kid"/*-- 37 --*/, "kod", "ked" }, /*-- 37 --*/ 
/*-- 38 --*/ { "female chicken", /*-- 38 --*/ "hen"/*-- 38 --*/, "han", "hin" }, /*-- 38 --*/ 
/*-- 39 --*/ { "staple food, often eaten with dishes", /*-- 39 --*/ "rice"/*-- 39 --*/, "rais", "ryce" }, /*-- 39 --*/ 
/*-- 40 --*/ { "opposite of fast", /*-- 40 --*/ "slow"/*-- 40 --*/, "slew", "sloe" }, /*-- 40 --*/ 
/*-- 41 --*/ { "reading material", /*-- 41 --*/ "book"/*-- 41 --*/, "buck", "boke" }, /*-- 41 --*/ 
/*-- 42 --*/ { "farm animal that oinks", /*-- 42 --*/ "pig"/*-- 42 --*/, "peg", "pog" }, /*-- 42 --*/ 
/*-- 43 --*/ { "very young human", /*-- 43 --*/ "baby"/*-- 43 --*/, "bebe", "beby" }, /*-- 43 --*/ 
/*-- 44 --*/ { "refers to a male person", /*-- 44 --*/ "him"/*-- 44 --*/, "hem", "hym" }, /*-- 44 --*/ 
/*-- 45 --*/ { "makes ringing sound", /*-- 45 --*/ "bell"/*-- 45 --*/, "vel", "bel" }, /*-- 45 --*/ 
/*-- 46 --*/ { "plaything for children", /*-- 46 --*/ "toy"/*-- 46 --*/, "toi", "tay" }, /*-- 46 --*/ 
/*-- 47 --*/ { "female parent", /*-- 47 --*/ "mom"/*-- 47 --*/, "mum", "mam" }, /*-- 47 --*/ 
/*-- 48 --*/ { "feeling of sadness", /*-- 48 --*/ "sad"/*-- 48 --*/, "sed", "sod" }, /*-- 48 --*/ 
/*-- 49 --*/ { "part of a tree or plant", /*-- 49 --*/ "leaf"/*-- 49 --*/, "loaf", "leef" }, /*-- 49 --*/ 
/*-- 50 --*/ { "to form letters on paper", /*-- 50 --*/ "write"/*-- 50 --*/, "rite", "writ" }, /*-- 50 --*/ 
/*-- 51 --*/ { "color opposite of black", /*-- 51 --*/ "white"/*-- 51 --*/, "whyte", "wite" }, /*-- 51 --*/ 
/*-- 52 --*/ { "small furry pet that meows", /*-- 52 --*/ "cat"/*-- 52 --*/, "gat", "kat" }, /*-- 52 --*/ 
/*-- 53 --*/ { "farm animal with horns", /*-- 53 --*/ "goat"/*-- 53 --*/, "got", "gote" }, /*-- 53 --*/ 
/*-- 54 --*/ { "furniture for working", /*-- 54 --*/ "desk"/*-- 54 --*/, "deks", "dask" }, /*-- 54 --*/ 
/*-- 55 --*/ { "a celestial object at night", /*-- 55 --*/ "moon"/*-- 55 --*/, "mun", "mune" }, /*-- 55 --*/ 
/*-- 56 --*/ { "falling water from clouds", /*-- 56 --*/ "rain"/*-- 56 --*/, "rein", "rayn" }, /*-- 56 --*/ 
/*-- 57 --*/ { "frozen precipitation", /*-- 57 --*/ "snow"/*-- 57 --*/, "snou", "snoe" }, /*-- 57 --*/ 
/*-- 58 --*/ { "part of the face used for speaking", /*-- 58 --*/ "lip"/*-- 58 --*/, "lap", "lep" }, /*-- 58 --*/ 
/*-- 59 --*/ { "sweet spread for bread", /*-- 59 --*/ "jam"/*-- 59 --*/, "jem", "jom" }, /*-- 59 --*/ 
/*-- 60 --*/ { "young male child", /*-- 60 --*/ "boy"/*-- 60 --*/, "boi", "bouy" }, /*-- 60 --*/ 
/*-- 61 --*/ { "brother of your parent", /*-- 61 --*/ "uncle"/*-- 61 --*/, "unkl", "uncal" }, /*-- 61 --*/ 
/*-- 62 --*/ { "good or desirable", /*-- 62 --*/ "good"/*-- 62 --*/, "gud", "goud" }, /*-- 62 --*/ 
/*-- 63 --*/ { "opposite of good", /*-- 63 --*/ "bad"/*-- 63 --*/, "badd", "bod" }, /*-- 63 --*/ 
/*-- 64 --*/ { "to move on feet at a moderate pace", /*-- 64 --*/ "walk"/*-- 64 --*/, "wok", "wolk" }, /*-- 64 --*/ 
/*-- 65 --*/ { "to be upright on feet", /*-- 65 --*/ "stand"/*-- 65 --*/, "stend", "stond" }, /*-- 65 --*/ 
/*-- 66 --*/ { "to make music with voice", /*-- 66 --*/ "sing"/*-- 66 --*/, "sung", "seng" }, /*-- 66 --*/ 
/*-- 67 --*/ { "transport by road", /*-- 67 --*/ "bus"/*-- 67 --*/, "bos", "bas" }, /*-- 67 --*/ 
/*-- 68 --*/ { "passage to enter or exit", /*-- 68 --*/ "door"/*-- 68 --*/, "dor", "doer" }, /*-- 68 --*/ 
/*-- 69 --*/ { "something that shows location", /*-- 69 --*/ "map"/*-- 69 --*/, "mop", "mep" }, /*-- 69 --*/ 
/*-- 70 --*/ { "oval farm product from birds", /*-- 70 --*/ "egg"/*-- 70 --*/, "egh", "eg" }, /*-- 70 --*/ 
/*-- 71 --*/ { "edible fruit shaped like a bulb", /*-- 71 --*/ "pear"/*-- 71 --*/, "per", "pare" }, /*-- 71 --*/ 
/*-- 72 --*/ { "small swimming waterbird", /*-- 72 --*/ "duck"/*-- 72 --*/, "duc", "duk" }, /*-- 72 --*/ 
/*-- 73 --*/ { "small rodent", /*-- 73 --*/ "mouse"/*-- 73 --*/, "mawz", "mous" }, /*-- 73 --*/ 
/*-- 74 --*/ { "air in motion", /*-- 74 --*/ "wind"/*-- 74 --*/, "windz", "wynd" }, /*-- 74 --*/ 

//forgotten words
/*-- 75 --*/ { "round object used in games", /*-- 75 --*/ "ball"/*-- 75 --*/, "bol", "bawl" },  
/*-- 76 --*/ { "large in size", /*-- 76 --*/ "big"/*-- 76 --*/, "beg", "bug" },  
/*-- 77 --*/ { "color like chocolate", /*-- 77 --*/ "brown"/*-- 77 --*/, "brawn", "bron" },  
/*-- 78 --*/ { "having low temperature", /*-- 78 --*/ "cold"/*-- 78 --*/, "kold", "cald" },  
/*-- 79 --*/ { "seen in the sky, made of vapor", /*-- 79 --*/ "cloud"/*-- 79 --*/, "clod", "clowd" },  
/*-- 80 --*/ { "an animal that lives in water", /*-- 80 --*/ "fish"/*-- 80 --*/, "fesh", "fosh" },  
/*-- 81 --*/ { "grows on your head", /*-- 81 --*/ "hair"/*-- 81 --*/, "hare", "heir" },  
/*-- 82 --*/ { "refers to a female person", /*-- 82 --*/ "her"/*-- 82 --*/, "hur", "hir" },  
/*-- 83 --*/ { "color of grass", /*-- 83 --*/ "green"/*-- 83 --*/, "grean", "gren" },  
/*-- 84 --*/ { "organ used for seeing", /*-- 84 --*/ "eye"/*-- 84 --*/, "aye", "eie" },  
/*-- 85 --*/ { "adult male human", /*-- 85 --*/ "man"/*-- 85 --*/, "men", "mun" },  
/*-- 86 --*/ { "refers to the speaker", /*-- 86 --*/ "me"/*-- 86 --*/, "mi", "meh" },  
/*-- 87 --*/ { "color between red and yellow", /*-- 87 --*/ "orange"/*-- 87 --*/, "oranj", "ornge" },  
/*-- 88 --*/ { "tool used for writing with ink", /*-- 88 --*/ "pen"/*-- 88 --*/, "pin", "pan" },  
/*-- 89 --*/ { "look at words and understand", /*-- 89 --*/ "read"/*-- 89 --*/, "reed", "red" },  
/*-- 90 --*/ { "move fast on foot", /*-- 90 --*/ "run"/*-- 90 --*/, "ran", "ron" },  
/*-- 91 --*/ { "rest on your bottom", /*-- 91 --*/ "sit"/*-- 91 --*/, "set", "sat" },  
/*-- 92 --*/ { "not large in size", /*-- 92 --*/ "small"/*-- 92 --*/, "smol", "smel" },  
/*-- 93 --*/ { "bright object in the night sky", /*-- 93 --*/ "star"/*-- 93 --*/, "stor", "stir" },  
/*-- 94 --*/ { "liquid food eaten hot", /*-- 94 --*/ "soup"/*-- 94 --*/, "soop", "sup" },  
/*-- 95 --*/ { "color between black and white", /*-- 95 --*/ "gray"/*-- 95 --*/, "grey", "grai" },  
/*-- 96 --*/ { "hard natural stone", /*-- 96 --*/ "rock"/*-- 96 --*/, "rok", "ruck" },  
/*-- 97 --*/ { "used to bite and chew", /*-- 97 --*/ "tooth"/*-- 97 --*/, "toot", "toth" },  
/*-- 98 --*/ { "clear liquid you drink", /*-- 98 --*/ "water"/*-- 98 --*/, "watar", "woter" },  
/*-- 99 --*/ { "color of the sun", /*-- 99 --*/ "yellow"/*-- 99 --*/, "yelow", "yello" },  
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
/*-- 0 --*/  { "large reptile with powerful jaws", /*-- 0 --*/ "alligator"/*-- 0 --*/, "aligater", "alligater" }, /*-- 0 --*/ 
/*-- 1 --*/  { "a heavy object dropped from a ship to keep it in place", /*-- 1 --*/ "anchor"/*-- 1 --*/, "anker", "anchore" }, /*-- 1 --*/ 
/*-- 2 --*/  { "metal protective clothing worn in battle", /*-- 2 --*/ "armor"/*-- 2 --*/, "armour", "armar" }, /*-- 2 --*/ 
/*-- 3 --*/  { "a long curved yellow fruit", /*-- 3 --*/ "banana"/*-- 3 --*/, "bannana", "bananna" }, /*-- 3 --*/ 
/*-- 4 --*/  { "a container for carrying things", /*-- 4 --*/ "basket"/*-- 4 --*/, "baskit", "baskett" }, /*-- 4 --*/ 
/*-- 5 --*/  { "a warm covering for a bed", /*-- 5 --*/ "blanket"/*-- 5 --*/, "blancket", "blankit" }, /*-- 5 --*/ 
/*-- 6 --*/  { "severe snow storm with strong winds", /*-- 6 --*/ "blizzard"/*-- 6 --*/, "blizzurd", "blizard" }, /*-- 6 --*/ 
/*-- 7 --*/  { "a container for liquids", /*-- 7 --*/ "bottle"/*-- 7 --*/, "bottel", "botal" }, /*-- 7 --*/ 
/*-- 8 --*/  { "a structure built to cross water", /*-- 8 --*/ "bridge"/*-- 8 --*/, "brige", "bridg" }, /*-- 8 --*/ 
/*-- 9 --*/  { "a desert plant with spines", /*-- 9 --*/ "cactus"/*-- 9 --*/, "cactuss", "kactus" }, /*-- 9 --*/ 
/*-- 10 --*/ { "a large fortified building", /*-- 10 --*/ "castle"/*-- 10 --*/, "castel", "cassle" }, /*-- 10 --*/ 
/*-- 11 --*/ { "a dairy food made from milk", /*-- 11 --*/ "cheese"/*-- 11 --*/, "cheeze", "chese" }, /*-- 11 --*/ 
/*-- 12 --*/ { "sweet brown food made from cocoa", /*-- 12 --*/ "chocolate"/*-- 12 --*/, "choclate", "chocolet" }, /*-- 12 --*/ 
/*-- 13 --*/ { "tool that shows north, south, east, west", /*-- 13 --*/ "compass"/*-- 13 --*/, "compas", "compess" }, /*-- 13 --*/ 
/*-- 14 --*/ { "a small sweet baked treat", /*-- 14 --*/ "cookie"/*-- 14 --*/, "cookies", "cooky" }, /*-- 14 --*/ 
/*-- 15 --*/ { "a royal head decoration", /*-- 15 --*/ "crown"/*-- 15 --*/, "croun", "crowne" }, /*-- 15 --*/ 
/*-- 16 --*/ { "a dry barren area with little rain", /*-- 16 --*/ "desert"/*-- 16 --*/, "dessert", "desart" }, /*-- 16 --*/ 
/*-- 17 --*/ { "a large bird of prey", /*-- 17 --*/ "eagle"/*-- 17 --*/, "eagel", "egle" }, /*-- 17 --*/ 
/*-- 18 --*/ { "a large area covered with trees", /*-- 18 --*/ "forest"/*-- 18 --*/, "forrest", "forist" }, /*-- 18 --*/ 
/*-- 19 --*/ { "an area for growing plants", /*-- 19 --*/ "garden"/*-- 19 --*/, "gardin", "gardon" }, /*-- 19 --*/ 
/*-- 20 --*/ { "a tall African animal with a long neck", /*-- 20 --*/ "giraffe"/*-- 20 --*/, "girafe", "girrafe" }, /*-- 20 --*/ 
/*-- 21 --*/ { "huge slow-moving river of ice", /*-- 21 --*/ "glacier"/*-- 21 --*/, "glaceir", "glaciar" }, /*-- 21 --*/ 
/*-- 22 --*/ { "protective headgear", /*-- 22 --*/ "helmet"/*-- 22 --*/, "helmit", "helmett" }, /*-- 22 --*/ 
/*-- 23 --*/ { "powerful tropical cyclone", /*-- 23 --*/ "hurricane"/*-- 23 --*/, "hurricaine", "huricane" }, /*-- 23 --*/ 
/*-- 24 --*/ { "tool used to create music", /*-- 24 --*/ "instrument"/*-- 24 --*/, "insturment", "instrament" }, /*-- 24 --*/ 
/*-- 25 --*/ { "decorative items worn on the body", /*-- 25 --*/ "jewelry"/*-- 25 --*/, "jewelery", "jewlery" }, /*-- 25 --*/ 
/*-- 26 --*/ { "a set of steps or rungs for climbing", /*-- 26 --*/ "ladder"/*-- 26 --*/, "lader", "ladar" }, /*-- 26 --*/ 
/*-- 27 --*/ { "a portable light source with a protective case", /*-- 27 --*/ "lantern"/*-- 27 --*/, "lanturn", "lantren" }, /*-- 27 --*/ 
/*-- 28 --*/ { "a tower with a bright light to guide ships", /*-- 28 --*/ "lighthouse"/*-- 28 --*/, "lighthous", "lighthuse" }, /*-- 28 --*/ 
/*-- 29 --*/ { "a metal object that attracts iron", /*-- 29 --*/ "magnet"/*-- 29 --*/, "magnit", "magnett" }, /*-- 29 --*/ 
/*-- 30 --*/ { "a public place where goods are sold", /*-- 30 --*/ "market"/*-- 30 --*/, "markit", "markett" }, /*-- 30 --*/ 
/*-- 31 --*/ { "a large sweet juicy fruit", /*-- 31 --*/ "melon"/*-- 31 --*/, "mellon", "melonn" }, /*-- 31 --*/ 
/*-- 32 --*/ { "tool to see very small things magnified", /*-- 32 --*/ "microscope"/*-- 32 --*/, "microscop", "micrascope" }, /*-- 32 --*/ 
/*-- 33 --*/ { "a tree-climbing primate", /*-- 33 --*/ "monkey"/*-- 33 --*/, "monkie", "monky" }, /*-- 33 --*/ 
/*-- 34 --*/ { "sea creature with eight arms", /*-- 34 --*/ "octopus"/*-- 34 --*/, "octupus", "octapus" }, /*-- 34 --*/ 
/*-- 35 --*/ { "large black big cat", /*-- 35 --*/ "panther"/*-- 35 --*/, "panthar", "pantor" }, /*-- 35 --*/ 
/*-- 36 --*/ { "flightless bird from cold regions", /*-- 36 --*/ "penguin"/*-- 36 --*/, "penguine", "pengwin" }, /*-- 36 --*/ 
/*-- 37 --*/ { "a soft support for the head during sleep", /*-- 37 --*/ "pillow"/*-- 37 --*/, "pilloe", "pillo" }, /*-- 37 --*/ 
/*-- 38 --*/ { "a small burrowing mammal with long ears", /*-- 38 --*/ "rabbit"/*-- 38 --*/, "rabitt", "rabbitt" }, /*-- 38 --*/ 
/*-- 39 --*/ { "a place for learning", /*-- 39 --*/ "school"/*-- 39 --*/, "skool", "schol" }, /*-- 39 --*/ 
/*-- 40 --*/ { "a cutting tool with two blades", /*-- 40 --*/ "scissors"/*-- 40 --*/, "sissors", "scisors" }, /*-- 40 --*/ 
/*-- 41 --*/ { "long Italian noodle dish", /*-- 41 --*/ "spaghetti"/*-- 41 --*/, "spagetti", "spagheti" }, /*-- 41 --*/ 
/*-- 42 --*/ { "a carved or cast figure of a person or animal", /*-- 42 --*/ "statue"/*-- 42 --*/, "statue", "statue" }, /*-- 42 --*/ 
/*-- 43 --*/ { "a public road in a town or city", /*-- 43 --*/ "street"/*-- 43 --*/, "streat", "stret" }, /*-- 43 --*/ 
/*-- 44 --*/ { "optical tool to see distant objects", /*-- 44 --*/ "telescope"/*-- 44 --*/, "telescop", "telascope" }, /*-- 44 --*/ 
/*-- 45 --*/ { "a large striped big cat", /*-- 45 --*/ "tiger"/*-- 45 --*/, "tigger", "tyger" }, /*-- 45 --*/ 
/*-- 46 --*/ { "a red juicy fruit often used in sauces", /*-- 46 --*/ "tomato"/*-- 46 --*/, "tamato", "tomatto" }, /*-- 46 --*/ 
/*-- 47 --*/ { "mountain that erupts with lava", /*-- 47 --*/ "volcano"/*-- 47 --*/, "volcanoe", "volcane" }, /*-- 47 --*/ 
/*-- 48 --*/ { "a building with blades that turn in the wind", /*-- 48 --*/ "windmill"/*-- 48 --*/, "windmil", "windmill" }, /*-- 48 --*/ 
/*-- 49 --*/ { "an African striped animal", /*-- 49 --*/ "zebra"/*-- 49 --*/, "zebra", "zebra" }, /*-- 49 --*/ 
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
/*-- 0 --*/  { "a burrowing African mammal with a long nose", /*-- 0 --*/ "aardvark"/*-- 0 --*/, "aardvarko", "ardvark" }, /*-- 0 --*/ 
/*-- 1 --*/  { "an open-air venue for performances", /*-- 1 --*/ "amphitheater"/*-- 1 --*/, "amfiteatro", "amphitheatre" }, /*-- 1 --*/ 
/*-- 2 --*/  { "a small armored mammal that rolls into a ball", /*-- 2 --*/ "armadillo"/*-- 2 --*/, "armadilo", "armadelo" }, /*-- 2 --*/ 
/*-- 3 --*/  { "an ancient astronomical instrument for measuring stars", /*-- 3 --*/ "astrolabe"/*-- 3 --*/, "astrolabbo", "astrolab" }, /*-- 3 --*/ 
/*-- 4 --*/  { "a rare aquatic salamander with external gills", /*-- 4 --*/ "axolotl"/*-- 4 --*/, "axoloto", "axolotal" }, /*-- 4 --*/ 
/*-- 5 --*/  { "an ancient missile weapon that launches projectiles", /*-- 5 --*/ "ballista"/*-- 5 --*/, "balista", "ballisto" }, /*-- 5 --*/ 
/*-- 6 --*/  { "a defensive wall on top of a castle", /*-- 6 --*/ "battlement"/*-- 6 --*/, "batlemanto", "battlemont" }, /*-- 6 --*/ 
/*-- 7 --*/  { "a rotating amusement ride with seats", /*-- 7 --*/ "carousel"/*-- 7 --*/, "karuselo", "carosel" }, /*-- 7 --*/ 
/*-- 8 --*/  { "a medieval device for hurling heavy stones", /*-- 8 --*/ "catapult"/*-- 8 --*/, "katapulto", "catopult" }, /*-- 8 --*/ 
/*-- 9 --*/  { "a mythical creature that is half human, half horse", /*-- 9 --*/ "centaur"/*-- 9 --*/, "sentaoro", "centuar" }, /*-- 9 --*/ 
/*-- 10 --*/ { "a lizard that can change its color", /*-- 10 --*/ "chameleon"/*-- 10 --*/, "kamaleono", "chamelion" }, /*-- 10 --*/ 
/*-- 11 --*/ { "a hanging decorative light fixture", /*-- 11 --*/ "chandelier"/*-- 11 --*/, "shandeler", "chandalier" }, /*-- 11 --*/ 
/*-- 12 --*/ { "the pupal stage of a butterfly", /*-- 12 --*/ "chrysalis"/*-- 12 --*/, "chrysaliso", "chrysalys" }, /*-- 12 --*/ 
/*-- 13 --*/ { "a colorful parrot with a crest", /*-- 13 --*/ "cockatoo"/*-- 13 --*/, "kokatu", "cockatou" }, /*-- 13 --*/ 
/*-- 14 --*/ { "a large ancient Roman theater", /*-- 14 --*/ "colosseum"/*-- 14 --*/, "coloseo", "coliseum" }, /*-- 14 --*/ 
/*-- 15 --*/ { "a bridge that can be raised or lowered", /*-- 15 --*/ "drawbridge"/*-- 15 --*/, "drawbriggo", "drawbrige" }, /*-- 15 --*/ 
/*-- 16 --*/ { "a carved figure often on buildings", /*-- 16 --*/ "gargoyle"/*-- 16 --*/, "gargoyo", "gargoil" }, /*-- 16 --*/ 
/*-- 17 --*/ { "a professional fighter in ancient Rome", /*-- 17 --*/ "gladiator"/*-- 17 --*/, "gladiato", "gladiater" }, /*-- 17 --*/ 
/*-- 18 --*/ { "a device used for executions by decapitation", /*-- 18 --*/ "guillotine"/*-- 18 --*/, "guilotino", "guillotene" }, /*-- 18 --*/ 
/*-- 19 --*/ { "a spear-like weapon for fishing or combat", /*-- 19 --*/ "harpoon"/*-- 19 --*/, "harpono", "harpune" }, /*-- 19 --*/ 
/*-- 20 --*/ { "ancient writing system of Egypt using symbols", /*-- 20 --*/ "hieroglyph"/*-- 20 --*/, "hyerogliffo", "hieroglif" }, /*-- 20 --*/ 
/*-- 21 --*/ { "an optical toy showing colorful patterns", /*-- 21 --*/ "kaleidoscope"/*-- 21 --*/, "kaleidoskopo", "kaleidoskope" }, /*-- 21 --*/ 
/*-- 22 --*/ { "a complex network of paths", /*-- 22 --*/ "labyrinth"/*-- 22 --*/, "labirinto", "laberinth" }, /*-- 22 --*/ 
/*-- 23 --*/ { "a large tent for events or shows", /*-- 23 --*/ "marquee"/*-- 23 --*/, "markweo", "marquea" }, /*-- 23 --*/ 
/*-- 24 --*/ { "a collection of exotic animals", /*-- 24 --*/ "menagerie"/*-- 24 --*/, "menajero", "managerie" }, /*-- 24 --*/ 
/*-- 25 --*/ { "a mythical creature with the body of a man and head of a bull", /*-- 25 --*/ "minotaur"/*-- 25 --*/, "minotauro", "minotar" }, /*-- 25 --*/ 
/*-- 26 --*/ { "a single massive upright stone", /*-- 26 --*/ "monolith"/*-- 26 --*/, "monolito", "monoleth" }, /*-- 26 --*/ 
/*-- 27 --*/ { "a whale with a long tusk", /*-- 27 --*/ "narwhal"/*-- 27 --*/, "narwalo", "narwal" }, /*-- 27 --*/ 
/*-- 28 --*/ { "a tall stone pillar or monument", /*-- 28 --*/ "obelisk"/*-- 28 --*/, "obelisko", "obelics" }, /*-- 28 --*/ 
/*-- 29 --*/ { "a dark volcanic glass", /*-- 29 --*/ "obsidian"/*-- 29 --*/, "obsidiano", "obsidien" }, /*-- 29 --*/ 
/*-- 30 --*/ { "a dungeon with a secret trapdoor", /*-- 30 --*/ "oubliette"/*-- 30 --*/, "oblietto", "oubliete" }, /*-- 30 --*/ 
/*-- 31 --*/ { "a famous temple in Athens", /*-- 31 --*/ "parthenon"/*-- 31 --*/, "parthenono", "parthanon" }, /*-- 31 --*/ 
/*-- 32 --*/ { "a tube for viewing distant objects", /*-- 32 --*/ "periscope"/*-- 32 --*/, "periskopo", "perascope" }, /*-- 32 --*/ 
/*-- 33 --*/ { "a ruler of ancient Egypt", /*-- 33 --*/ "pharaoh"/*-- 33 --*/, "faraono", "pharoh" }, /*-- 33 --*/ 
/*-- 34 --*/ { "a duck-billed egg-laying mammal", /*-- 34 --*/ "platypus"/*-- 34 --*/, "platipo", "platypos" }, /*-- 34 --*/ 
/*-- 35 --*/ { "a heavy gate that slides vertically", /*-- 35 --*/ "portcullis"/*-- 35 --*/, "portkulo", "portculis" }, /*-- 35 --*/ 
/*-- 36 --*/ { "a massive triangular structure", /*-- 36 --*/ "pyramid"/*-- 36 --*/, "piramido", "pyramyd" }, /*-- 36 --*/ 
/*-- 37 --*/ { "a small marsupial from Australia", /*-- 37 --*/ "quokka"/*-- 37 --*/, "quokko", "quoka" }, /*-- 37 --*/ 
/*-- 38 --*/ { "a Japanese warrior", /*-- 38 --*/ "samurai"/*-- 38 --*/, "samuraio", "samuray" }, /*-- 38 --*/ 
/*-- 39 --*/ { "a stone coffin, usually for royalty", /*-- 39 --*/ "sarcophagus"/*-- 39 --*/, "sarkofago", "sarcofagus" }, /*-- 39 --*/ 
/*-- 40 --*/ { "an arachnid with a sting", /*-- 40 --*/ "scorpion"/*-- 40 --*/, "skorpiono", "scorpeon" }, /*-- 40 --*/ 
/*-- 41 --*/ { "an ancient navigation instrument", /*-- 41 --*/ "sextant"/*-- 41 --*/, "sekstanto", "sextent" }, /*-- 41 --*/ 
/*-- 42 --*/ { "a mythical creature with a lion's body and human head", /*-- 42 --*/ "sphinx"/*-- 42 --*/, "sfinkso", "sfinx" }, /*-- 42 --*/ 
/*-- 43 --*/ { "a handheld telescope", /*-- 43 --*/ "spyglass"/*-- 43 --*/, "spyglasso", "spyglas" }, /*-- 43 --*/ 
/*-- 44 --*/ { "a large spider with long legs", /*-- 44 --*/ "tarantula"/*-- 44 --*/, "tarantulo", "tarantala" }, /*-- 44 --*/ 
/*-- 45 --*/ { "a medieval siege engine that throws stones", /*-- 45 --*/ "trebuchet"/*-- 45 --*/, "trebuchato", "trebuchat" }, /*-- 45 --*/ 
/*-- 46 --*/ { "a three-pronged spear", /*-- 46 --*/ "trident"/*-- 46 --*/, "tridanto", "tridant" }, /*-- 46 --*/ 
/*-- 47 --*/ { "a Scandinavian warrior or raider", /*-- 47 --*/ "viking"/*-- 47 --*/, "vikingo", "vyking" }, /*-- 47 --*/ 
/*-- 48 --*/ { "a musical instrument with keys", /*-- 48 --*/ "xylophone"/*-- 48 --*/, "zylophono", "xilophone" }, /*-- 48 --*/ 
/*-- 49 --*/ { "a stepped pyramid from ancient Mesopotamia", /*-- 49 --*/ "ziggurat"/*-- 49 --*/, "zigurato", "zigurat" }, /*-- 49 --*/ 
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

    #region Question logic
    // ─────────────────────────────────────────────────────────────────────────────
    // CORE LOGIC
    // ─────────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GAMEMODE")
        {
            activeSpellingPairs = easySpellingPairs;
            activeSentencePairs = easySentencePairs;
            currentClueImages = easyClueImages;
            currentPronunciationSounds = easyPronunciationSounds;
            CurrentSpellingBeforeSentence = easySpellingBeforeSentence;
            Debug.Log("Difficulty: EASY MODE activated");
        }
        else if (sceneName == "GAMEMODE 1")
        {
            activeSpellingPairs = mediumSpellingPairs;
            activeSentencePairs = mediumSentencePairs;
            currentClueImages = mediumClueImages;
            currentPronunciationSounds = mediumPronunciationSounds;
            CurrentSpellingBeforeSentence = mediumSpellingBeforeSentence;
            Debug.Log("Difficulty: MEDIUM MODE activated");
        }
        else if (sceneName == "GAMEMODE 2")
        {
            activeSpellingPairs = hardSpellingPairs;
            activeSentencePairs = hardSentencePairs;
            currentClueImages = hardClueImages;
            currentPronunciationSounds = hardPronunciationSounds;
            CurrentSpellingBeforeSentence = hardSpellingBeforeSentence;
            Debug.Log("Difficulty: HARD MODE activated");
        }
        else
        {
            activeSpellingPairs = easySpellingPairs;
            activeSentencePairs = easySentencePairs;
            currentClueImages = easyClueImages;
            currentPronunciationSounds = easyPronunciationSounds;
            CurrentSpellingBeforeSentence = easySpellingBeforeSentence;
            Debug.LogWarning("Unknown scene name. Defaulting to EASY MODE.");
        }
    }

    void Start()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.isTrigger = true;
        else Debug.LogWarning("QuestionRandomizer: No collider found on question object. Add a Collider component.");

        if (clueTextObject != null) clueTextObject.SetActive(false);
        if (clueImageObject != null) clueImageObject.SetActive(false);
        if (letterHurdleClueImage != null) letterHurdleClueImage.gameObject.SetActive(false);

        // Prepare for clue image animation
        if (clueImageObject != null)
        {
            imageCanvasGroup = clueImageObject.GetComponent<CanvasGroup>();
            if (imageCanvasGroup == null)
                imageCanvasGroup = clueImageObject.AddComponent<CanvasGroup>();

            imageRect = clueImageObject.GetComponent<RectTransform>();
            if (imageRect != null)
                originalImagePos = imageRect.anchoredPosition;
        }

        // Prepare for clue text animation
        if (clueTextObject != null)
        {
            textCanvasGroup = clueTextObject.GetComponent<CanvasGroup>();
            if (textCanvasGroup == null)
                textCanvasGroup = clueTextObject.AddComponent<CanvasGroup>();

            textRect = clueTextObject.GetComponent<RectTransform>();
            if (textRect != null)
                originalTextPos = textRect.anchoredPosition;
        }

        // Auto-find references for Letter Hurdle
        if (playerFunctions == null)
            playerFunctions = FindObjectOfType<PlayerFunctions>();

        if (obstacleSpawner == null)
            obstacleSpawner = FindObjectOfType<ObstacleSpawner>();

        InitializeLetterHurdle();

        if (!TryLoadDailyTaskQuestion())
        {
            SetRandomQuestion();
        }
    }

    void Update()
    {
        // Letter Hurdle update logic
        if (collectedText != null)
        {
            string currentCollected = collectedText.text.ToLower().Trim();
            if (currentCollected != previousCollectedText)
            {
                CheckSpellingFast(currentCollected);
                previousCollectedText = currentCollected;
            }
        }
    }

    void InitializeLetterHurdle()
    {
        // Build word list from spelling pairs
        List<string> words = new List<string>();
        for (int i = 0; i < activeSpellingPairs.GetLength(0); i++)
        {
            words.Add(activeSpellingPairs[i, 1].ToLower());
        }
        wordList = words.ToArray();
        
        // Build word to image map using the clue images
        BuildWordToImageMap();

        shuffledWords = wordList.OrderBy(x => Random.value).ToList();

        if (letterHurdleFeedbackText != null)
            letterHurdleFeedbackText.text = "";

        UpdateScoreText();

        SetNewTargetWord();
    }

    void BuildWordToImageMap()
    {
        wordToImageMap.Clear();
        
        if (currentClueImages == null || currentClueImages.Length == 0)
        {
            Debug.LogWarning("No clue images assigned for current difficulty");
            return;
        }

        int count = Mathf.Min(wordList.Length, currentClueImages.Length);
        for (int i = 0; i < count; i++)
        {
            string word = wordList[i].ToLower();
            if (!wordToImageMap.ContainsKey(word) && currentClueImages[i] != null)
            {
                wordToImageMap.Add(word, currentClueImages[i]);
            }
        }
        
        Debug.Log($"Word to image map built: {wordToImageMap.Count} words mapped");
    }

    void SetNewTargetWord()
    {
        foreach (var letter in spawnedLetters)
            Destroy(letter);
        spawnedLetters.Clear();

        if (currentWordIndex >= shuffledWords.Count)
        {
            shuffledWords = wordList.OrderBy(x => Random.value).ToList();
            currentWordIndex = 0;
        }

        currentTargetWord = shuffledWords[currentWordIndex];

        if (targetWordText != null)
            targetWordText.text = "Spell: " + currentTargetWord.ToLower();

        SpawnLetters(currentTargetWord);

        if (collectedText != null)
            collectedText.text = "";

        previousCollectedText = "";

        if (letterHurdleFeedbackText != null)
            letterHurdleFeedbackText.text = "";

        // Update clue image for the new word
        UpdateLetterHurdleClueImage();
    }

    void UpdateLetterHurdleClueImage()
    {
        if (letterHurdleClueImage == null) return;

        string targetWord = currentTargetWord.ToLower();
        
        if (wordToImageMap.TryGetValue(targetWord, out Sprite clueSprite))
        {
            letterHurdleClueImage.sprite = clueSprite;
            letterHurdleClueImage.gameObject.SetActive(true);
            Debug.Log($"Showing clue image for word: {currentTargetWord}");
        }
        else
        {
            // Try case-insensitive fallback
            var match = wordToImageMap.FirstOrDefault(x => 
                string.Equals(x.Key, targetWord, System.StringComparison.OrdinalIgnoreCase));
            
            if (match.Value != null)
            {
                letterHurdleClueImage.sprite = match.Value;
                letterHurdleClueImage.gameObject.SetActive(true);
            }
            else
            {
                letterHurdleClueImage.gameObject.SetActive(false);
                Debug.LogWarning($"No clue image found for word: {currentTargetWord}");
            }
        }
    }

    void SpawnLetters(string word)
    {
        if (letterPrefab == null || letterSpawnParent == null) return;

        for (int i = 0; i < word.Length; i++)
        {
            GameObject letterObj = Instantiate(letterPrefab, letterSpawnParent);
            letterObj.transform.localPosition = new Vector3(i * letterSpacing, 0, 0);
            TMP_Text letterText = letterObj.GetComponent<TMP_Text>();
            if (letterText != null)
                letterText.text = word[i].ToString().ToUpper();

            spawnedLetters.Add(letterObj);
        }
    }

    void CheckSpellingFast(string collected)
    {
        if (string.IsNullOrEmpty(collected)) return;

        string target = currentTargetWord.ToLower();

        if (collected == target)
        {
            if (letterHurdleFeedbackText != null)
            {
                letterHurdleFeedbackText.text = "Correct!";
                letterHurdleFeedbackText.color = Color.green;
            }

            if (playerFunctions != null)
            {
                string scene = SceneManager.GetActiveScene().name;
                int coinReward = scene switch
                {
                    "GAMEMODE 2" => 200,
                    "GAMEMODE 1" => 100,
                    _            => 25
                };
                playerFunctions.AddCoins(coinReward);
            }

            // Notify ObstacleSpawner that word was completed
            if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
            {
                obstacleSpawner.OnLetterHurdleSuccess();
                Debug.Log("✅ Word completed! Notified ObstacleSpawner.");
                
                // Hide clue image when event ends
                if (letterHurdleClueImage != null)
                    letterHurdleClueImage.gameObject.SetActive(false);
            }

            // Call boss manager if it exists
            if (bossManager != null)
                bossManager.FinishBoss();

            currentWordIndex++;
            SetNewTargetWord();
            return;
        }

        int minLength = Mathf.Min(collected.Length, target.Length);
        for (int i = 0; i < minLength; i++)
        {
            if (collected[i] != target[i])
            {
                if (letterHurdleFeedbackText != null)
                {
                    letterHurdleFeedbackText.text = "Wrong Letter!";
                    letterHurdleFeedbackText.color = Color.red;
                }

                // Notify ObstacleSpawner of failure
                if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
                {
                    obstacleSpawner.OnLetterHurdleFailed();
                }

                if (playerFunctions != null)
                    playerFunctions.TakeDamageFromWrongLetter();

                collectedText.text = collected.Substring(0, i);
                previousCollectedText = collectedText.text;

                if (letterHurdleFeedbackText != null)
                    Invoke("ClearLetterHurdleFeedback", 1f);

                return;
            }
        }

        if (letterHurdleFeedbackText != null)
            letterHurdleFeedbackText.text = "";
    }

    void ClearLetterHurdleFeedback()
    {
        if (letterHurdleFeedbackText != null)
            letterHurdleFeedbackText.text = "";
    }

    void UpdateScoreText()
    {
        if (letterHurdleScoreText != null && playerFunctions != null)
            letterHurdleScoreText.text = "Score: " + playerFunctions.score;
    }

    public void ClearCollectedLetters()
    {
        if (collectedText != null)
            collectedText.text = "";

        previousCollectedText = "";

        if (letterHurdleFeedbackText != null)
            letterHurdleFeedbackText.text = "";
    }

    public void SkipWord()
    {
        currentWordIndex++;
        SetNewTargetWord();
    }

    public string GetCurrentWord()
    {
        return currentTargetWord;
    }

    public void CheckBossSpell()
    {
        if (collectedText == null) return;

        string typed = collectedText.text.ToLower().Trim();
        string target = currentTargetWord.ToLower().Trim();

        if (typed == target)
        {
            // Notify ObstacleSpawner first
            if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
            {
                obstacleSpawner.OnLetterHurdleSuccess();
                Debug.Log("✅ CheckBossSpell: Word completed! Ending letter event.");
                
                // Hide clue image when event ends
                if (letterHurdleClueImage != null)
                    letterHurdleClueImage.gameObject.SetActive(false);
            }
        }
    }

    public void ShowLetterHurdleClueImage(bool show)
    {
        if (letterHurdleClueImage != null)
            letterHurdleClueImage.gameObject.SetActive(show);
    }

    public void RefreshLetterHurdleClueImage()
    {
        UpdateLetterHurdleClueImage();
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

    // Fixed AssignOptions: always uses two distinct wrong answers
    private void AssignOptions(string correct, string wrong1, string wrong2, int correctPosition)
    {
        TMP_Text[] optionTexts = new TMP_Text[] { jumpText, slideText, option3Text };

        // Collect the two wrong answers
        string[] wrongs = new string[] { wrong1, wrong2 };

        // Find the indices of the two wrong slots
        int[] wrongIndices = new int[2];
        int idx = 0;
        for (int i = 0; i < 3; i++)
        {
            if (i != correctPosition)
                wrongIndices[idx++] = i;
        }

        // Optional: shuffle the wrong answers so the order varies
        if (Random.value > 0.5f)
        {
            string temp = wrongs[0];
            wrongs[0] = wrongs[1];
            wrongs[1] = temp;
        }

        // Assign
        optionTexts[correctPosition].text = correct;
        optionTexts[wrongIndices[0]].text = wrongs[0];
        optionTexts[wrongIndices[1]].text = wrongs[1];
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
            // Use the difficulty‑specific pronunciation array
            if (currentPronunciationSounds != null && currentQuestionIndex < currentPronunciationSounds.Length && currentPronunciationSounds[currentQuestionIndex] != null)
            {
                audioSource.PlayOneShot(currentPronunciationSounds[currentQuestionIndex]);
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

        // Instantly hide both clues (you could add a fade‑out animation here if needed)
        if (clueTextObject != null) clueTextObject.SetActive(false);
        if (clueImageObject != null) clueImageObject.SetActive(false);
    }

    private void UpdateClueVisibility()
    {
        if (!playerInTrigger) return;

        // Sentence questions always show text clue, never image
        if (isSentenceQuestion)
        {
            if (clueTextObject != null)
            {
                if (animateClueText)
                    AnimateClueTextIn();
                else
                    clueTextObject.SetActive(true);
            }
            if (clueImageObject != null) clueImageObject.SetActive(false);
        }
        // Spelling questions: show image if available, else text
        else
        {
            if (HasClueImage())
            {
                if (cluePic != null && currentQuestionIndex >= 0 && currentQuestionIndex < currentClueImages.Length)
                    cluePic.sprite = currentClueImages[currentQuestionIndex];

                if (animateClueImage && clueImageObject != null)
                    AnimateClueImageIn();
                else if (clueImageObject != null)
                    clueImageObject.SetActive(true);

                if (clueTextObject != null) clueTextObject.SetActive(false);
            }
            else
            {
                if (clueTextObject != null)
                {
                    if (animateClueText)
                        AnimateClueTextIn();
                    else
                        clueTextObject.SetActive(true);
                }
                if (clueImageObject != null) clueImageObject.SetActive(false);
            }
        }
    }

    // Animates the clue image with fade + slide from the chosen direction
    private void AnimateClueImageIn()
    {
        if (clueImageObject == null || imageRect == null || imageCanvasGroup == null) return;

        // Stop any ongoing animation
        if (imageAnimationCoroutine != null)
            StopCoroutine(imageAnimationCoroutine);

        imageAnimationCoroutine = StartCoroutine(AnimateImageCoroutine());
    }

    private IEnumerator AnimateImageCoroutine()
    {
        // Ensure object is active and set initial alpha to 0
        clueImageObject.SetActive(true);
        imageCanvasGroup.alpha = 0f;

        // Calculate start position based on direction
        Vector2 startPos = originalImagePos;
        switch (imageSlideDirection)
        {
            case SlideDirection.Left:
                startPos.x -= imageSlideDistance;
                break;
            case SlideDirection.Right:
                startPos.x += imageSlideDistance;
                break;
            case SlideDirection.Up:
                startPos.y += imageSlideDistance;
                break;
            case SlideDirection.Down:
                startPos.y -= imageSlideDistance;
                break;
        }
        imageRect.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < imageAnimationDuration)
        {
            float t = elapsed / imageAnimationDuration;
            float curveValue = imageAnimationCurve.Evaluate(t);

            // Fade
            imageCanvasGroup.alpha = curveValue;

            // Slide
            imageRect.anchoredPosition = Vector2.Lerp(startPos, originalImagePos, curveValue);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final values are exact
        imageCanvasGroup.alpha = 1f;
        imageRect.anchoredPosition = originalImagePos;

        imageAnimationCoroutine = null;
    }

    // Animates the clue text with fade + slide from the chosen direction
    private void AnimateClueTextIn()
    {
        if (clueTextObject == null || textRect == null || textCanvasGroup == null) return;

        // Stop any ongoing animation
        if (textAnimationCoroutine != null)
            StopCoroutine(textAnimationCoroutine);

        textAnimationCoroutine = StartCoroutine(AnimateTextCoroutine());
    }

    private IEnumerator AnimateTextCoroutine()
    {
        // Ensure object is active and set initial alpha to 0
        clueTextObject.SetActive(true);
        textCanvasGroup.alpha = 0f;

        // Calculate start position based on direction
        Vector2 startPos = originalTextPos;
        switch (textSlideDirection)
        {
            case SlideDirection.Left:
                startPos.x -= textSlideDistance;
                break;
            case SlideDirection.Right:
                startPos.x += textSlideDistance;
                break;
            case SlideDirection.Up:
                startPos.y += textSlideDistance;
                break;
            case SlideDirection.Down:
                startPos.y -= textSlideDistance;
                break;
        }
        textRect.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < textAnimationDuration)
        {
            float t = elapsed / textAnimationDuration;
            float curveValue = textAnimationCurve.Evaluate(t);

            // Fade
            textCanvasGroup.alpha = curveValue;

            // Slide
            textRect.anchoredPosition = Vector2.Lerp(startPos, originalTextPos, curveValue);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final values are exact
        textCanvasGroup.alpha = 1f;
        textRect.anchoredPosition = originalTextPos;

        textAnimationCoroutine = null;
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
        if (currentClueImages != null && currentQuestionIndex >= 0 && currentQuestionIndex < currentClueImages.Length)
            return currentClueImages[currentQuestionIndex];
        return null;
    }

    public bool HasClueImage()
    {
        return currentClueImages != null && currentQuestionIndex >= 0 && currentQuestionIndex < currentClueImages.Length && currentClueImages[currentQuestionIndex] != null;
    }
    #endregion
}
//merged letter hurdle and word hurdle