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
    // ─────────────────────────────────────────────────────────────────────────────
    // INSPECTOR HEADERS
    // ─────────────────────────────────────────────────────────────────────────────

    [Header("UI")]
    public TMP_Text jumpText;
    public TMP_Text slideText;
    public TMP_Text option3Text;
    public GameObject clueTextObject;
    public TMP_Text clueText;
    public GameObject clueImageObject;
    public Image cluePic;

    [Header("Letter Hurdle UI")]
    public TMP_Text collectedText;       // Shows: "d _ _" style progress display
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
    public int easySpellingBeforeSentence = 3;
    public int mediumSpellingBeforeSentence = 4;
    public int hardSpellingBeforeSentence = 5;

    // Static so other scripts (like ObstacleSpawner) can read the current difficulty's value
    public static int CurrentSpellingBeforeSentence = 3;

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

    [Header("Letter Hurdle – Timing")]
    [Tooltip("Delay in seconds before the word is pronounced when a new letter hurdle word appears (entrance).")]
    public float letterHurdlePronunciationDelay = 2f;

    [Tooltip("Delay in seconds after correct spelling before the completion (outro) pronunciation plays. " +
             "Must be less than or equal to Letter Hurdle Next Word Delay.")]
    public float letterHurdleCompletionPronunciationDelay = 0.5f;

    [Tooltip("How long (seconds) the completed word's image stays visible before the next word appears. " +
             "The image is actively kept on screen for this entire duration. " +
             "Set this to at least 2–4 seconds so the player can read it comfortably.")]
    public float letterHurdleNextWordDelay = 4f;

    // ─────────────────────────────────────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────────────────────────────────────

    public string correctAnswer;
    private int currentQuestionIndex = -1;
    private bool isSentenceQuestion = false;
    private bool audioPlayed = false;
    private bool playerInTrigger = false;

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

    private string previousRawCollected = "";

    // Keep the old field name alive as an alias so any leftover references compile
    private string previousCollectedText
    {
        get => previousRawCollected;
        set => previousRawCollected = value;
    }

    private List<GameObject> spawnedLetters = new List<GameObject>();
    private Dictionary<string, Sprite> wordToImageMap = new Dictionary<string, Sprite>();

    // pronunciationCoroutine  — delayed ENTRANCE sound for a newly shown word
    // wordTransitionCoroutine — completion flow: keeps image alive + outro + next word
    private Coroutine pronunciationCoroutine;
    private Coroutine wordTransitionCoroutine;

    // ─────────────────────────────────────────────────────────────────────────────
    // DATA BANKS
    // ─────────────────────────────────────────────────────────────────────────────

    #region Easy Data Banks

    public static string[,] easySpellingPairs = new string[,]
    {
/*-- 0 --*/  { "a common pet that barks",                       "dog",        "dag",        "dug"        },
/*-- 1 --*/  { "something you wear on your head",               "hat",        "hoat",       "hut"        },
/*-- 2 --*/  { "a color between red and white",                 "pink",       "ponk",       "pank"       },
/*-- 3 --*/  { "the star in the sky during daytime",            "sun",        "san",        "son"        },
/*-- 4 --*/  { "body part used for walking",                    "leg",        "log",        "lig"        },
/*-- 5 --*/  { "food from animals, often eaten cooked",         "meat",       "met",        "mait"       },
/*-- 6 --*/  { "a small container for drinking",                "cup",        "cawp",       "cop"        },
/*-- 7 --*/  { "you listen with this",                          "ear",        "eer",        "eur"        },
/*-- 8 --*/  { "a plant with leaves and branches",              "tree",       "tri",        "trea"       },
/*-- 9 --*/  { "opposite of white",                             "black",      "bolck",      "blak"       },
/*--10 --*/  { "moving quickly",                                "fast",       "fest",       "fust"       },
/*--11 --*/  { "activity in water",                             "swim",       "swom",       "swam"       },
/*--12 --*/  { "refers to the person being addressed",          "you",        "yoo",        "yu"         },
/*--13 --*/  { "where you sleep at night",                      "bed",        "ved",        "bad"        },
/*--14 --*/  { "part of your body used to hold things",         "hand",       "henk",       "hend"       },
/*--15 --*/  { "a flying animal with feathers",                 "bird",       "burd",       "berd"       },
/*--16 --*/  { "white drink from cows",                         "milk",       "milx",       "melk"       },
/*--17 --*/  { "to leap into the air",                          "jump",       "jomp",       "jamp"       },
/*--18 --*/  { "baked food made from flour",                    "bread",      "bredd",      "bred"       },
/*--19 --*/  { "sweet dessert",                                 "cake",       "cak",        "caek"       },
/*--20 --*/  { "used for walking or running",                   "foot",       "fut",        "fot"        },
/*--21 --*/  { "female child",                                  "girl",       "gurl",       "gerl"       },
/*--22 --*/  { "part of your face used to smell",               "nose",       "nosh",       "noze"       },
/*--23 --*/  { "color of the sky on a clear day",               "blue",       "blu",        "blew"       },
/*--24 --*/  { "vehicle with wheels",                           "car",        "cor",        "kar"        },
/*--25 --*/  { "yellow vegetable",                              "corn",       "curn",       "korn"       },
/*--26 --*/  { "mother's sister",                               "aunt",       "ant",        "awnt"       },
/*--27 --*/  { "farm animal that gives milk",                   "cow",        "coe",        "kow"        },
/*--28 --*/  { "feeling of joy",                                "happy",      "hoppy",      "hapi"       },
/*--29 --*/  { "color of an apple",                             "red",        "rad",        "reed"       },
/*--30 --*/  { "limb attached to shoulder",                     "arm",        "arum",       "erm"        },
/*--31 --*/  { "a companion",                                   "friend",     "frend",      "freind"     },
/*--32 --*/  { "container with sides",                          "box",        "bocs",       "boks"       },
/*--33 --*/  { "used to carry things",                          "bag",        "bog",        "beg"        },
/*--34 --*/  { "furniture to sit on",                           "chair",      "chare",      "chear"      },
/*--35 --*/  { "jump on one foot",                              "hop",        "hut",        "hap"        },
/*--36 --*/  { "a male parent",                                 "dad",        "did",        "dod"        },
/*--37 --*/  { "a young child",                                 "kid",        "kod",        "ked"        },
/*--38 --*/  { "female chicken",                                "hen",        "han",        "hin"        },
/*--39 --*/  { "staple food, often eaten with dishes",          "rice",       "rais",       "ryce"       },
/*--40 --*/  { "opposite of fast",                              "slow",       "slew",       "sloe"       },
/*--41 --*/  { "reading material",                              "book",       "buck",       "boke"       },
/*--42 --*/  { "farm animal that oinks",                        "pig",        "peg",        "pog"        },
/*--43 --*/  { "very young human",                              "baby",       "bebe",       "beby"       },
/*--44 --*/  { "refers to a male person",                       "him",        "hem",        "hym"        },
/*--45 --*/  { "makes ringing sound",                           "bell",       "vel",        "bel"        },
/*--46 --*/  { "plaything for children",                        "toy",        "toi",        "tay"        },
/*--47 --*/  { "female parent",                                 "mom",        "mum",        "mam"        },
/*--48 --*/  { "feeling of sadness",                            "sad",        "sed",        "sod"        },
/*--49 --*/  { "part of a tree or plant",                       "leaf",       "loaf",       "leef"       },
/*--50 --*/  { "to form letters on paper",                      "write",      "rite",       "writ"       },
/*--51 --*/  { "color opposite of black",                       "white",      "whyte",      "wite"       },
/*--52 --*/  { "small furry pet that meows",                    "cat",        "gat",        "kat"        },
/*--53 --*/  { "farm animal with horns",                        "goat",       "got",        "gote"       },
/*--54 --*/  { "furniture for working",                         "desk",       "deks",       "dask"       },
/*--55 --*/  { "a celestial object at night",                   "moon",       "mun",        "mune"       },
/*--56 --*/  { "falling water from clouds",                     "rain",       "rein",       "rayn"       },
/*--57 --*/  { "frozen precipitation",                          "snow",       "snou",       "snoe"       },
/*--58 --*/  { "part of the face used for speaking",            "lip",        "lap",        "lep"        },
/*--59 --*/  { "sweet spread for bread",                        "jam",        "jem",        "jom"        },
/*--60 --*/  { "young male child",                              "boy",        "boi",        "bouy"       },
/*--61 --*/  { "brother of your parent",                        "uncle",      "unkl",       "uncal"      },
/*--62 --*/  { "good or desirable",                             "good",       "gud",        "goud"       },
/*--63 --*/  { "opposite of good",                              "bad",        "badd",       "bod"        },
/*--64 --*/  { "to move on feet at a moderate pace",            "walk",       "wok",        "wolk"       },
/*--65 --*/  { "to be upright on feet",                         "stand",      "stend",      "stond"      },
/*--66 --*/  { "to make music with voice",                      "sing",       "sung",       "seng"       },
/*--67 --*/  { "transport by road",                             "bus",        "bos",        "bas"        },
/*--68 --*/  { "passage to enter or exit",                      "door",       "dor",        "doer"       },
/*--69 --*/  { "something that shows location",                 "map",        "mop",        "mep"        },
/*--70 --*/  { "oval farm product from birds",                  "egg",        "egh",        "eg"         },
/*--71 --*/  { "edible fruit shaped like a bulb",               "pear",       "per",        "pare"       },
/*--72 --*/  { "small swimming waterbird",                      "duck",       "duc",        "duk"        },
/*--73 --*/  { "small rodent",                                  "mouse",      "mawz",       "mous"       },
/*--74 --*/  { "air in motion",                                 "wind",       "windz",      "wynd"       },
/*--75 --*/  { "round object used in games",                    "ball",       "bol",        "bawl"       },
/*--76 --*/  { "large in size",                                 "big",        "beg",        "bug"        },
/*--77 --*/  { "color like chocolate",                          "brown",      "brawn",      "bron"       },
/*--78 --*/  { "having low temperature",                        "cold",       "kold",       "cald"       },
/*--79 --*/  { "seen in the sky, made of vapor",                "cloud",      "clod",       "clowd"      },
/*--80 --*/  { "an animal that lives in water",                 "fish",       "fesh",       "fosh"       },
/*--81 --*/  { "grows on your head",                            "hair",       "hare",       "heir"       },
/*--82 --*/  { "refers to a female person",                     "her",        "hur",        "hir"        },
/*--83 --*/  { "color of grass",                                "green",      "grean",      "gren"       },
/*--84 --*/  { "organ used for seeing",                         "eye",        "aye",        "eie"        },
/*--85 --*/  { "adult male human",                              "man",        "men",        "mun"        },
/*--86 --*/  { "refers to the speaker",                         "me",         "mi",         "meh"        },
/*--87 --*/  { "color between red and yellow",                  "orange",     "oranj",      "ornge"      },
/*--88 --*/  { "tool used for writing with ink",                "pen",        "pin",        "pan"        },
/*--89 --*/  { "look at words and understand",                  "read",       "reed",       "red"        },
/*--90 --*/  { "move fast on foot",                             "run",        "ran",        "ron"        },
/*--91 --*/  { "rest on your bottom",                           "sit",        "set",        "sat"        },
/*--92 --*/  { "not large in size",                             "small",      "smol",       "smel"       },
/*--93 --*/  { "bright object in the night sky",                "star",       "stor",       "stir"       },
/*--94 --*/  { "liquid food eaten hot",                         "soup",       "soop",       "sup"        },
/*--95 --*/  { "color between black and white",                 "gray",       "grey",       "grai"       },
/*--96 --*/  { "hard natural stone",                            "rock",       "rok",        "ruck"       },
/*--97 --*/  { "used to bite and chew",                         "tooth",      "toot",       "toth"       },
/*--98 --*/  { "clear liquid you drink",                        "water",      "watar",      "woter"      },
/*--99 --*/  { "color of the sun",                              "yellow",     "yelow",      "yello"      },
    };

    public static string[,] easySentencePairs = new string[,]
    {
        { "The chef ____ a delicious meal.",                        "cooked",       "taught",       "slept"      },
        { "The teacher ____ the students about history.",           "taught",       "slept",        "flew"       },
        { "The cat ____ on the sunny windowsill.",                  "slept",        "flew",         "cooked"     },
        { "The pianist ____ a beautiful melody.",                   "played",       "ran",          "cried"      },
        { "The athlete ____ across the finish line.",               "ran",          "cried",        "tumbled"    },
        { "The baby ____ when it's hungry.",                        "cries",        "tumbles",      "runs"       },
        { "The photographer ____ pictures of the sunset.",          "took",         "repaired",     "studied"    },
        { "The mechanic ____ the broken engine.",                   "repaired",     "studied",      "painted"    },
        { "The student ____ for the upcoming exam.",                "studied",      "painted",      "took"       },
        { "The artist ____ a portrait on the canvas.",              "painted",      "wagged",       "sang"       },
        { "The dog ____ its tail happily.",                         "wagged",       "sang",         "watered"    },
        { "The singer ____ in front of the audience.",              "sang",         "watered",      "wrote"      },
        { "The gardener ____ the flowers every morning.",           "watered",      "wrote",        "examined"   },
        { "The writer ____ a new short story.",                     "wrote",        "examined",     "drove"      },
        { "The doctor ____ the patient carefully.",                 "examined",     "drove",        "built"      },
        { "The driver ____ the car down the highway.",              "drove",        "built",        "experimented"},
        { "The carpenter ____ a sturdy table.",                     "built",        "experimented", "served"     },
        { "The scientist ____ an experiment in the lab.",           "experimented", "served",       "swam"       },
        { "The waiter ____ food to the customers.",                 "served",       "swam",         "cooked"     },
        { "The swimmer ____ laps in the pool.",                     "swam",         "cooked",       "taught"     }
    };

    #endregion

    #region Medium Data Banks

    public static string[,] mediumSpellingPairs = new string[,]
    {
/*-- 0 --*/  { "large reptile with powerful jaws",                          "alligator",     "aligater",      "alligater"     },
/*-- 1 --*/  { "a heavy object dropped from a ship to keep it in place",    "anchor",        "anker",         "anchore"       },
/*-- 2 --*/  { "metal protective clothing worn in battle",                  "armor",         "armour",        "armar"         },
/*-- 3 --*/  { "a long curved yellow fruit",                                "banana",        "bannana",       "bananna"       },
/*-- 4 --*/  { "a container for carrying things",                           "basket",        "baskit",        "baskett"       },
/*-- 5 --*/  { "a warm covering for a bed",                                 "blanket",       "blancket",      "blankit"       },
/*-- 6 --*/  { "severe snow storm with strong winds",                       "blizzard",      "blizzurd",      "blizard"       },
/*-- 7 --*/  { "a container for liquids",                                   "bottle",        "bottel",        "botal"         },
/*-- 8 --*/  { "a structure built to cross water",                          "bridge",        "brige",         "bridg"         },
/*-- 9 --*/  { "a desert plant with spines",                                "cactus",        "cactuss",       "kactus"        },
/*--10 --*/  { "a large fortified building",                                "castle",        "castel",        "cassle"        },
/*--11 --*/  { "a dairy food made from milk",                               "cheese",        "cheeze",        "chese"         },
/*--12 --*/  { "sweet brown food made from cocoa",                          "chocolate",     "choclate",      "chocolet"      },
/*--13 --*/  { "tool that shows north, south, east, west",                  "compass",       "compas",        "compess"       },
/*--14 --*/  { "a small sweet baked treat",                                 "cookie",        "cookies",       "cooky"         },
/*--15 --*/  { "a royal head decoration",                                   "crown",         "croun",         "crowne"        },
/*--16 --*/  { "a dry barren area with little rain",                        "desert",        "dessert",       "desart"        },
/*--17 --*/  { "a large bird of prey",                                      "eagle",         "eagel",         "egle"          },
/*--18 --*/  { "a large area covered with trees",                           "forest",        "forrest",       "forist"        },
/*--19 --*/  { "an area for growing plants",                                "garden",        "gardin",        "gardon"        },
/*--20 --*/  { "a tall African animal with a long neck",                    "giraffe",       "girafe",        "girrafe"       },
/*--21 --*/  { "huge slow-moving river of ice",                             "glacier",       "glaceir",       "glaciar"       },
/*--22 --*/  { "protective headgear",                                       "helmet",        "helmit",        "helmett"       },
/*--23 --*/  { "powerful tropical cyclone",                                 "hurricane",     "hurricaine",    "huricane"      },
/*--24 --*/  { "tool used to create music",                                 "instrument",    "insturment",    "instrament"    },
/*--25 --*/  { "decorative items worn on the body",                         "jewelry",       "jewelery",      "jewlery"       },
/*--26 --*/  { "a set of steps or rungs for climbing",                      "ladder",        "lader",         "ladar"         },
/*--27 --*/  { "a portable light source with a protective case",            "lantern",       "lanturn",       "lantren"       },
/*--28 --*/  { "a tower with a bright light to guide ships",                "lighthouse",    "lighthous",     "lighthuse"     },
/*--29 --*/  { "a metal object that attracts iron",                         "magnet",        "magnit",        "magnett"       },
/*--30 --*/  { "a public place where goods are sold",                       "market",        "markit",        "markett"       },
/*--31 --*/  { "a large sweet juicy fruit",                                 "melon",         "mellon",        "melonn"        },
/*--32 --*/  { "tool to see very small things magnified",                   "microscope",    "microscop",     "micrascope"    },
/*--33 --*/  { "a tree-climbing primate",                                   "monkey",        "monkie",        "monky"         },
/*--34 --*/  { "sea creature with eight arms",                              "octopus",       "octupus",       "octapus"       },
/*--35 --*/  { "large black big cat",                                       "panther",       "panthar",       "pantor"        },
/*--36 --*/  { "flightless bird from cold regions",                         "penguin",       "penguine",      "pengwin"       },
/*--37 --*/  { "a soft support for the head during sleep",                  "pillow",        "pilloe",        "pillo"         },
/*--38 --*/  { "a small burrowing mammal with long ears",                   "rabbit",        "rabitt",        "rabbitt"       },
/*--39 --*/  { "a place for learning",                                      "school",        "skool",         "schol"         },
/*--40 --*/  { "a cutting tool with two blades",                            "scissors",      "sissors",       "scisors"       },
/*--41 --*/  { "long Italian noodle dish",                                  "spaghetti",     "spagetti",      "spagheti"      },
/*--42 --*/  { "a carved or cast figure of a person or animal",             "statue",        "statoo",        "stachu"        },
/*--43 --*/  { "a public road in a town or city",                           "street",        "streat",        "stret"         },
/*--44 --*/  { "optical tool to see distant objects",                       "telescope",     "telescop",      "telascope"     },
/*--45 --*/  { "a large striped big cat",                                   "tiger",         "tigger",        "tyger"         },
/*--46 --*/  { "a red juicy fruit often used in sauces",                    "tomato",        "tamato",        "tomatto"       },
/*--47 --*/  { "mountain that erupts with lava",                            "volcano",       "volcanoe",      "volcane"       },
/*--48 --*/  { "a building with blades that turn in the wind",              "windmill",      "windmil",       "windmell"      },
/*--49 --*/  { "an African striped animal",                                 "zebra",         "zeebra",        "zebrah"        },
    };

    public static string[,] mediumSentencePairs = new string[,]
    {
        { "The students will ____ for their final exams tomorrow.",     "study",        "relax",        "ignore"     },
        { "The construction workers ____ the new building quickly.",    "built",        "repaired",     "destroyed"  },
        { "The author ____ a fascinating novel last year.",             "wrote",        "reviewed",     "read"       },
        { "The chef ____ the ingredients carefully for the recipe.",    "measured",     "washed",       "mixed"      },
        { "The athlete ____ every day to improve his skills.",          "trains",       "rests",        "sleeps"     },
        { "The musician ____ a beautiful song for the audience.",       "performed",    "listened",     "recorded"   },
        { "The gardener ____ the plants every morning.",                "waters",       "trims",        "plants"     },
        { "The programmer ____ a new software application.",            "developed",    "tested",       "installed"  },
        { "The detective ____ the mystery carefully.",                  "investigated", "observed",     "solved"     },
        { "The artist ____ the landscape with vibrant colors.",         "painted",      "sketched",     "drew"       },
        { "The scientist ____ the results to confirm the hypothesis.",  "analyzed",     "ignored",      "recorded"   },
        { "The students ____ quietly while the teacher explained.",     "listened",     "whispered",    "slept"      },
        { "The captain ____ the ship safely to shore.",                 "guided",       "followed",     "sailed"     },
        { "The nurse ____ the patient throughout the night.",           "cared for",    "watched",      "examined"   },
        { "The engineer ____ a new solution to the problem.",           "designed",     "reviewed",     "tested"     },
        { "The actor ____ his lines before the performance.",           "practiced",    "forgot",       "memorized"  },
        { "The librarian ____ the books back on the shelves.",          "organized",    "stacked",      "sorted"     },
        { "The explorer ____ new regions of the jungle.",               "discovered",   "visited",      "mapped"     },
        { "The reporter ____ the event for the evening news.",          "covered",      "announced",    "filmed"     },
        { "The professor ____ the topic in great detail.",              "explained",    "mentioned",    "discussed"  }
    };

    #endregion

    #region Hard Data Banks

    public static string[,] hardSpellingPairs = new string[,]
    {
/*-- 0 --*/  { "a burrowing African mammal with a long nose",                  "aardvark",      "aardvarko",     "ardvark"       },
/*-- 1 --*/  { "an open-air venue for performances",                           "amphitheater",  "amfiteatro",    "amphitheatre"  },
/*-- 2 --*/  { "a small armored mammal that rolls into a ball",                "armadillo",     "armadilo",      "armadelo"      },
/*-- 3 --*/  { "an ancient astronomical instrument for measuring stars",       "astrolabe",     "astrolabbo",    "astrolab"      },
/*-- 4 --*/  { "a rare aquatic salamander with external gills",                "axolotl",       "axoloto",       "axolotal"      },
/*-- 5 --*/  { "an ancient missile weapon that launches projectiles",          "ballista",      "balista",       "ballisto"      },
/*-- 6 --*/  { "a defensive wall on top of a castle",                          "battlement",    "batlemanto",    "battlemont"    },
/*-- 7 --*/  { "a rotating amusement ride with seats",                         "carousel",      "karuselo",      "carosel"       },
/*-- 8 --*/  { "a medieval device for hurling heavy stones",                   "catapult",      "katapulto",     "catopult"      },
/*-- 9 --*/  { "a mythical creature that is half human, half horse",           "centaur",       "sentaoro",      "centuar"       },
/*--10 --*/  { "a lizard that can change its color",                           "chameleon",     "kamaleono",     "chamelion"     },
/*--11 --*/  { "a hanging decorative light fixture",                           "chandelier",    "shandeler",     "chandalier"    },
/*--12 --*/  { "the pupal stage of a butterfly",                               "chrysalis",     "chrysaliso",    "chrysalys"     },
/*--13 --*/  { "a colorful parrot with a crest",                               "cockatoo",      "kokatu",        "cockatou"      },
/*--14 --*/  { "a large ancient Roman theater",                                "colosseum",     "coloseo",       "coliseum"      },
/*--15 --*/  { "a bridge that can be raised or lowered",                       "drawbridge",    "drawbriggo",    "drawbrige"     },
/*--16 --*/  { "a carved figure often on buildings",                           "gargoyle",      "gargoyo",       "gargoil"       },
/*--17 --*/  { "a professional fighter in ancient Rome",                       "gladiator",     "gladiato",      "gladiater"     },
/*--18 --*/  { "a device used for executions by decapitation",                 "guillotine",    "guilotino",     "guillotene"    },
/*--19 --*/  { "a spear-like weapon for fishing or combat",                    "harpoon",       "harpono",       "harpune"       },
/*--20 --*/  { "ancient writing system of Egypt using symbols",                "hieroglyph",    "hyerogliffo",   "hieroglif"     },
/*--21 --*/  { "an optical toy showing colorful patterns",                     "kaleidoscope",  "kaleidoskopo",  "kaleidoskope"  },
/*--22 --*/  { "a complex network of paths",                                   "labyrinth",     "labirinto",     "laberinth"     },
/*--23 --*/  { "a large tent for events or shows",                             "marquee",       "markweo",       "marquea"       },
/*--24 --*/  { "a collection of exotic animals",                               "menagerie",     "menajero",      "managerie"     },
/*--25 --*/  { "a mythical creature with the body of a man and head of a bull","minotaur",      "minotauro",     "minotar"       },
/*--26 --*/  { "a single massive upright stone",                               "monolith",      "monolito",      "monoleth"      },
/*--27 --*/  { "a whale with a long tusk",                                     "narwhal",       "narwalo",       "narwal"        },
/*--28 --*/  { "a tall stone pillar or monument",                              "obelisk",       "obelisko",      "obelics"       },
/*--29 --*/  { "a dark volcanic glass",                                        "obsidian",      "obsidiano",     "obsidien"      },
/*--30 --*/  { "a dungeon with a secret trapdoor",                             "oubliette",     "oblietto",      "oubliete"      },
/*--31 --*/  { "a famous temple in Athens",                                    "parthenon",     "parthenono",    "parthanon"     },
/*--32 --*/  { "a tube for viewing distant objects",                           "periscope",     "periskopo",     "perascope"     },
/*--33 --*/  { "a ruler of ancient Egypt",                                     "pharaoh",       "faraono",       "pharoh"        },
/*--34 --*/  { "a duck-billed egg-laying mammal",                              "platypus",      "platipo",       "platypos"      },
/*--35 --*/  { "a heavy gate that slides vertically",                          "portcullis",    "portkulo",      "portculis"     },
/*--36 --*/  { "a massive triangular structure",                               "pyramid",       "piramido",      "pyramyd"       },
/*--37 --*/  { "a small marsupial from Australia",                             "quokka",        "quokko",        "quoka"         },
/*--38 --*/  { "a Japanese warrior",                                           "samurai",       "samuraio",      "samuray"       },
/*--39 --*/  { "a stone coffin, usually for royalty",                          "sarcophagus",   "sarkofago",     "sarcofagus"    },
/*--40 --*/  { "an arachnid with a sting",                                     "scorpion",      "skorpiono",     "scorpeon"      },
/*--41 --*/  { "an ancient navigation instrument",                             "sextant",       "sekstanto",     "sextent"       },
/*--42 --*/  { "a mythical creature with a lion's body and human head",        "sphinx",        "sfinkso",       "sfinx"         },
/*--43 --*/  { "a handheld telescope",                                         "spyglass",      "spyglasso",     "spyglas"       },
/*--44 --*/  { "a large spider with long legs",                                "tarantula",     "tarantulo",     "tarantala"     },
/*--45 --*/  { "a medieval siege engine that throws stones",                   "trebuchet",     "trebuchato",    "trebuchat"     },
/*--46 --*/  { "a three-pronged spear",                                        "trident",       "tridanto",      "tridant"       },
/*--47 --*/  { "a Scandinavian warrior or raider",                             "viking",        "vikingo",       "vyking"        },
/*--48 --*/  { "a musical instrument with keys",                               "xylophone",     "zylophono",     "xilophone"     },
/*--49 --*/  { "a stepped pyramid from ancient Mesopotamia",                   "ziggurat",      "zigurato",      "zigurat"       },
    };

    public static string[,] hardSentencePairs = new string[,]
    {
        { "The explorers decided to ____ the unknown cave system.",         "explore",       "explain",       "examine"       },
        { "The mathematician solved a very difficult ____.",                "equation",      "question",      "problem"       },
        { "She felt extremely ____ after making a mistake in public.",      "embarrassed",   "impressed",     "ashamed"       },
        { "The inventor received a patent for his latest ____.",            "invention",     "convention",    "creation"      },
        { "The ancient ____ developed complex writing systems.",            "civilization",  "university",    "society"       },
        { "He remained ____ of his surroundings even while sleeping.",      "conscious",     "confident",     "aware"         },
        { "The wealthy family lived in a very ____ mansion.",               "extravagant",   "elegant",       "expensive"     },
        { "The shy child felt quite ____ around strangers.",                "bashful",       "playful",       "timid"         },
        { "The team worked together to ____ a new bridge.",                 "construct",     "conduct",       "create"        },
        { "She always tries to ____ different points of view.",             "compare",       "prepare",       "consider"      },
        { "The ____ student asked many thoughtful questions.",              "curious",       "furious",       "intelligent"   },
        { "He felt very ____ about the upcoming exam results.",             "anxious",       "serious",       "nervous"       },
        { "The ____ donation helped build the new library.",                "generous",      "famous",        "large"         },
        { "Astronauts must be extremely ____ to survive in space.",         "confident",     "different",     "brave"         },
        { "We are very ____ for all your help during the project.",         "grateful",      "careful",       "thankful"      },
        { "The chef carefully ____ the exotic ingredients.",                "prepared",      "compared",      "selected"      },
        { "The ____ erupted violently after many years of silence.",        "volcano",       "tornado",       "mountain"      },
        { "The ____ moved slowly across the landscape over centuries.",     "glacier",       "river",         "desert"        },
        { "The pilot navigated through the dangerous ____.",                "hurricane",     "mountain",      "storm"         },
        { "She used a ____ to examine the tiny crystals.",                  "microscope",    "telescope",     "magnifier"     },
    };

    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // CORE
    // ─────────────────────────────────────────────────────────────────────────────

    #region Core

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
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        else Debug.LogWarning("QuestionRandomizer: No collider found. Add a Collider component.");

        if (clueTextObject != null)   clueTextObject.SetActive(false);
        if (clueImageObject != null)  clueImageObject.SetActive(false);
        if (letterHurdleClueImage != null) letterHurdleClueImage.gameObject.SetActive(false);

        if (clueImageObject != null)
        {
            imageCanvasGroup = clueImageObject.GetComponent<CanvasGroup>() ?? clueImageObject.AddComponent<CanvasGroup>();
            imageRect = clueImageObject.GetComponent<RectTransform>();
        }

        if (clueTextObject != null)
        {
            textCanvasGroup = clueTextObject.GetComponent<CanvasGroup>() ?? clueTextObject.AddComponent<CanvasGroup>();
            textRect = clueTextObject.GetComponent<RectTransform>();
        }

        StartCoroutine(CaptureOriginalPositions());

        if (playerFunctions == null)  playerFunctions  = FindObjectOfType<PlayerFunctions>();
        if (obstacleSpawner == null)  obstacleSpawner  = FindObjectOfType<ObstacleSpawner>();

        InitializeLetterHurdle();

        if (!TryLoadDailyTaskQuestion())
            SetRandomQuestion();
    }

    private IEnumerator CaptureOriginalPositions()
    {
        yield return null;
        if (imageRect != null) originalImagePos = imageRect.anchoredPosition;
        if (textRect  != null) originalTextPos  = textRect.anchoredPosition;
    }

    void Update()
    {
        if (collectedText == null) return;
        string rawNow = ExtractRawLetters(collectedText.text);
        if (rawNow != previousRawCollected)
            CheckSpellingFast(rawNow);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // WORD HURDLE  –  Spelling Questions  (jump / slide / option3 UI)
    // ─────────────────────────────────────────────────────────────────────────────

    #region Word Hurdle – Spelling Questions

    public void SetSpellingQuestion(int index)
    {
        if (index < 0 || index >= activeSpellingPairs.GetLength(0))
        {
            Debug.LogError($"Invalid spelling question index: {index}");
            return;
        }

        string clue    = activeSpellingPairs[index, 0];
        string correct = activeSpellingPairs[index, 1];
        string wrong1  = activeSpellingPairs[index, 2];
        string wrong2  = activeSpellingPairs[index, 3];

        clueText.text        = clue;
        correctAnswer        = correct;
        currentQuestionIndex = index;
        isSentenceQuestion   = false;
        audioPlayed          = false;

        int correctPosition = Random.Range(0, 3);
        AssignOptions(correct, wrong1, wrong2, correctPosition);
        UpdateClueVisibility();

        Debug.Log($"Spelling Q: {clue} | Correct: {correct} @ pos {correctPosition}");
    }

    public int GetSpellingQuestionCount() => activeSpellingPairs?.GetLength(0) ?? 0;

    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // SENTENCE HURDLE  –  Sentence Completion Questions  (jump / slide / option3 UI)
    // ─────────────────────────────────────────────────────────────────────────────

    #region Sentence Hurdle – Sentence Questions

    public void SetSentenceQuestion(int index)
    {
        if (index < 0 || index >= activeSentencePairs.GetLength(0))
        {
            Debug.LogError($"Invalid sentence question index: {index}");
            return;
        }

        string sentence = activeSentencePairs[index, 0];
        string correct  = activeSentencePairs[index, 1];
        string wrong1   = activeSentencePairs[index, 2];
        string wrong2   = activeSentencePairs[index, 3];

        clueText.text        = sentence;
        correctAnswer        = correct;
        currentQuestionIndex = index;
        isSentenceQuestion   = true;
        audioPlayed          = false;

        int correctPosition = Random.Range(0, 3);
        AssignOptions(correct, wrong1, wrong2, correctPosition);
        UpdateClueVisibility();

        Debug.Log($"Sentence Q: {sentence} | Correct: {correct} @ pos {correctPosition}");
    }

    public int GetSentenceQuestionCount() => activeSentencePairs?.GetLength(0) ?? 0;

    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // SHARED QUESTION LOGIC  –  used by both Word Hurdle and Sentence Hurdle
    // ─────────────────────────────────────────────────────────────────────────────

    #region Shared Question Logic

    private void AssignOptions(string correct, string wrong1, string wrong2, int correctPosition)
    {
        TMP_Text[] options = new TMP_Text[] { jumpText, slideText, option3Text };
        string[]   wrongs  = new string[]   { wrong1, wrong2 };

        int[] wrongIndices = new int[2];
        int idx = 0;
        for (int i = 0; i < 3; i++)
            if (i != correctPosition) wrongIndices[idx++] = i;

        if (Random.value > 0.5f) { string t = wrongs[0]; wrongs[0] = wrongs[1]; wrongs[1] = t; }

        options[correctPosition].text  = correct;
        options[wrongIndices[0]].text  = wrongs[0];
        options[wrongIndices[1]].text  = wrongs[1];
    }

    public void SetRandomQuestion()
    {
        if (Random.value > 0.5f)
            SetSpellingQuestion(Random.Range(0, activeSpellingPairs.GetLength(0)));
        else
            SetSentenceQuestion(Random.Range(0, activeSentencePairs.GetLength(0)));
    }

    public bool TryLoadDailyTaskQuestion()
    {
        if (!PlayerPrefs.HasKey("CurrentTaskID")) return false;

        int taskID        = PlayerPrefs.GetInt("CurrentTaskID", -1);
        int questionIndex = PlayerPrefs.GetInt("CurrentTaskQuestionIndex", -1);
        bool isSpelling   = PlayerPrefs.GetInt("CurrentTaskIsSpelling", 1) == 1;

        if (taskID == -1 || questionIndex == -1) return false;

        Debug.Log($"📋 Loading Daily Task: Question #{questionIndex} ({(isSpelling ? "Spelling" : "Sentence")})");
        if (isSpelling) SetSpellingQuestion(questionIndex);
        else            SetSentenceQuestion(questionIndex);
        return true;
    }

    public bool IsSentenceQuestion() => isSentenceQuestion;

    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // LETTER HURDLE  –  Letter-by-letter Spelling Game
    // ─────────────────────────────────────────────────────────────────────────────

    #region Letter Hurdle – Letter Collection Game

    // ── Init ──────────────────────────────────────────────────────────────────────

    void InitializeLetterHurdle()
    {
        List<string> words = new List<string>();
        for (int i = 0; i < activeSpellingPairs.GetLength(0); i++)
            words.Add(activeSpellingPairs[i, 1].ToLower());

        wordList = words.ToArray();
        BuildWordToImageMap();

        shuffledWords = wordList.OrderBy(x => Random.value).ToList();

        if (letterHurdleFeedbackText != null) letterHurdleFeedbackText.text = "";
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
                wordToImageMap.Add(word, currentClueImages[i]);
        }
        Debug.Log($"Word-to-image map built: {wordToImageMap.Count} words mapped");
    }

    void SetNewTargetWord()
    {
        // Cancel any stale entrance pronunciation
        if (pronunciationCoroutine != null)
        {
            StopCoroutine(pronunciationCoroutine);
            pronunciationCoroutine = null;
        }

        foreach (var letter in spawnedLetters) Destroy(letter);
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

        previousRawCollected = "";
        UpdateCollectedDisplay("");

        if (letterHurdleFeedbackText != null) letterHurdleFeedbackText.text = "";

        UpdateLetterHurdleClueImage();

        if (obstacleSpawner == null || obstacleSpawner.IsLetterEventActive)
        {
            pronunciationCoroutine = StartCoroutine(PlayLetterHurdlePronunciationDelayed(letterHurdlePronunciationDelay));
            Debug.Log($"🔊 Entrance pronunciation scheduled in {letterHurdlePronunciationDelay}s for: {currentTargetWord}");
        }
        else
        {
            Debug.Log($"⏭️ Skipping entrance pronunciation for '{currentTargetWord}' — letter event no longer active.");
        }
    }

    void UpdateLetterHurdleClueImage()
    {
        if (letterHurdleClueImage == null) return;

        string targetWord = currentTargetWord.ToLower();
        Sprite clueSprite = null;

        if (!wordToImageMap.TryGetValue(targetWord, out clueSprite))
        {
            var match = wordToImageMap.FirstOrDefault(x =>
                string.Equals(x.Key, targetWord, System.StringComparison.OrdinalIgnoreCase));
            clueSprite = match.Value;
        }

        if (clueSprite != null)
        {
            letterHurdleClueImage.sprite = clueSprite;
            letterHurdleClueImage.gameObject.SetActive(true);
            Debug.Log($"Showing clue image for word: {currentTargetWord}");
        }
        else
        {
            letterHurdleClueImage.gameObject.SetActive(false);
            Debug.LogWarning($"No clue image found for word: {currentTargetWord}");
        }
    }

    void SpawnLetters(string word)
    {
        if (letterPrefab == null || letterSpawnParent == null) return;

        for (int i = 0; i < word.Length; i++)
        {
            GameObject letterObj = Instantiate(letterPrefab, letterSpawnParent);
            letterObj.transform.localPosition = new Vector3(i * letterSpacing, 0, 0);
            TMP_Text lt = letterObj.GetComponent<TMP_Text>();
            if (lt != null) lt.text = word[i].ToString().ToUpper();
            spawnedLetters.Add(letterObj);
        }
    }

    // ── Display helpers ────────────────────────────────────────────────────────────

    private string ExtractRawLetters(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in text)
        {
            if (c != '_' && c != ' ')
                sb.Append(char.ToLower(c));
        }
        return sb.ToString();
    }

    private void UpdateCollectedDisplay(string rawCollected)
    {
        if (collectedText == null || string.IsNullOrEmpty(currentTargetWord)) return;

        string target = currentTargetWord.ToLower();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < target.Length; i++)
        {
            if (i > 0) sb.Append(' ');
            if (i < rawCollected.Length)
                sb.Append(rawCollected[i]);
            else
                sb.Append('_');
        }

        collectedText.text = sb.ToString();
    }

    // ── Spell check ───────────────────────────────────────────────────────────────

    void CheckSpellingFast(string collected)
    {
        if (string.IsNullOrEmpty(collected))
        {
            previousRawCollected = "";
            UpdateCollectedDisplay("");
            return;
        }

        string target = currentTargetWord.ToLower();

        if (collected == target)
        {
            UpdateCollectedDisplay(collected);
            previousRawCollected = collected;

            if (letterHurdleFeedbackText != null)
            {
                letterHurdleFeedbackText.text  = "Correct!";
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

            // Notify systems — do NOT hide letterHurdleClueImage here.
            // WordCompletionFlow actively keeps it alive for the full delay.
            if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
            {
                obstacleSpawner.OnLetterHurdleSuccess();
                Debug.Log("✅ Word completed! Notified ObstacleSpawner.");
            }

            if (bossManager != null) bossManager.FinishBoss();

            // Cancel entrance pronunciation — completion flow owns audio now
            if (pronunciationCoroutine != null)
            {
                StopCoroutine(pronunciationCoroutine);
                pronunciationCoroutine = null;
            }

            if (wordTransitionCoroutine != null)
            {
                StopCoroutine(wordTransitionCoroutine);
                wordTransitionCoroutine = null;
            }

            currentWordIndex++;
            wordTransitionCoroutine = StartCoroutine(WordCompletionFlow());
            return;
        }

        int minLen = Mathf.Min(collected.Length, target.Length);
        for (int i = 0; i < minLen; i++)
        {
            if (collected[i] != target[i])
            {
                if (letterHurdleFeedbackText != null)
                {
                    letterHurdleFeedbackText.text  = "Wrong Letter!";
                    letterHurdleFeedbackText.color = Color.red;
                }

                if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
                    obstacleSpawner.OnLetterHurdleFailed();

                if (playerFunctions != null)
                    playerFunctions.TakeDamageFromWrongLetter();

                string trimmed = collected.Substring(0, i);
                previousRawCollected = trimmed;
                UpdateCollectedDisplay(trimmed);

                if (letterHurdleFeedbackText != null)
                    Invoke("ClearLetterHurdleFeedback", 1f);

                return;
            }
        }

        previousRawCollected = collected;
        UpdateCollectedDisplay(collected);

        if (letterHurdleFeedbackText != null) letterHurdleFeedbackText.text = "";
    }

    // ── Completion flow ───────────────────────────────────────────────────────────

    /// <summary>
    /// Runs every frame for the full letterHurdleNextWordDelay duration.
    ///
    /// WHY per-frame instead of a single yield:
    /// ObstacleSpawner.EndLetterEvent() hides letterEventClueUI, which may be the
    /// same panel or a parent of letterHurdleClueImage. By re-asserting the sprite
    /// and SetActive(true) each frame we guarantee the image stays visible no matter
    /// what else in the scene tries to hide it during the delay window.
    ///
    /// Timeline:
    ///   0s                            → image kept on screen
    ///   letterHurdleCompletionPronunciationDelay → outro sound plays
    ///   letterHurdleNextWordDelay     → SetNewTargetWord() swaps to next word
    /// </summary>
    private IEnumerator WordCompletionFlow()
    {
        string completedWord = shuffledWords[Mathf.Clamp(currentWordIndex - 1, 0, shuffledWords.Count - 1)];

        // Snapshot the completed word's sprite immediately so we can restore it
        // even if something externally clears the Image component's sprite.
        Sprite completedSprite = null;
        if (letterHurdleClueImage != null)
            completedSprite = letterHurdleClueImage.sprite;

        bool outroPlayed = false;
        float elapsed    = 0f;

        while (elapsed < letterHurdleNextWordDelay)
        {
            // ── Force image visible every frame ───────────────────────────────
            // This is the key fix: whatever external code hides the image panel
            // (ObstacleSpawner.EndLetterEvent, trigger exits, etc.), we override
            // it on the very next frame so the player always sees the completed word.
            if (letterHurdleClueImage != null && completedSprite != null)
            {
                if (!letterHurdleClueImage.gameObject.activeSelf)
                    letterHurdleClueImage.gameObject.SetActive(true);

                // Restore sprite in case it was swapped out
                if (letterHurdleClueImage.sprite != completedSprite)
                    letterHurdleClueImage.sprite = completedSprite;
            }
            // ─────────────────────────────────────────────────────────────────

            // Play outro sound at the configured offset
            if (!outroPlayed && elapsed >= letterHurdleCompletionPronunciationDelay)
            {
                PlayLetterHurdlePronunciationForWord(completedWord);
                Debug.Log($"🔊 Outro pronunciation played for: {completedWord}");
                outroPlayed = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Delay fully elapsed — advance to the next word.
        // SetNewTargetWord() will call UpdateLetterHurdleClueImage() which swaps
        // the sprite to the next word (or hides the image if no sprite exists).
        wordTransitionCoroutine = null;
        SetNewTargetWord();
    }

    void ClearLetterHurdleFeedback()
    {
        if (letterHurdleFeedbackText != null) letterHurdleFeedbackText.text = "";
    }

    void UpdateScoreText()
    {
        if (letterHurdleScoreText != null && playerFunctions != null)
            letterHurdleScoreText.text = "Score: " + playerFunctions.score;
    }

    // ── Pronunciation ─────────────────────────────────────────────────────────────

    /// <summary>Plays the pronunciation for the current target word immediately.</summary>
    public void PlayLetterHurdlePronunciation()
    {
        PlayLetterHurdlePronunciationForWord(currentTargetWord);
    }

    /// <summary>
    /// Plays pronunciation by word string. Safe to call with the completed word
    /// after currentTargetWord has already advanced.
    /// </summary>
    private void PlayLetterHurdlePronunciationForWord(string word)
    {
        if (audioSource == null || wordList == null || string.IsNullOrEmpty(word)) return;

        int wordIndex = System.Array.FindIndex(wordList, w =>
            string.Equals(w, word, System.StringComparison.OrdinalIgnoreCase));

        if (wordIndex >= 0
            && currentPronunciationSounds != null
            && wordIndex < currentPronunciationSounds.Length
            && currentPronunciationSounds[wordIndex] != null)
        {
            audioSource.PlayOneShot(currentPronunciationSounds[wordIndex]);
            Debug.Log($"🔊 Playing pronunciation for: {word}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No pronunciation clip found for: {word} (index {wordIndex})");
        }
    }

    private IEnumerator PlayLetterHurdlePronunciationDelayed(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        PlayLetterHurdlePronunciation();
        pronunciationCoroutine = null;
    }

    // ── Public API ────────────────────────────────────────────────────────────────

    public void ClearCollectedLetters()
    {
        previousRawCollected = "";
        UpdateCollectedDisplay("");
        if (letterHurdleFeedbackText != null) letterHurdleFeedbackText.text = "";
    }

    public void SkipWord()
    {
        if (wordTransitionCoroutine != null)
        {
            StopCoroutine(wordTransitionCoroutine);
            wordTransitionCoroutine = null;
        }
        if (pronunciationCoroutine != null)
        {
            StopCoroutine(pronunciationCoroutine);
            pronunciationCoroutine = null;
        }
        currentWordIndex++;
        SetNewTargetWord();
    }

    public string GetCurrentWord() => currentTargetWord;

    public void CheckBossSpell()
    {
        if (collectedText == null) return;

        string typed  = ExtractRawLetters(collectedText.text);
        string target = currentTargetWord.ToLower().Trim();

        if (typed == target)
        {
            if (obstacleSpawner != null && obstacleSpawner.IsLetterEventActive)
            {
                obstacleSpawner.OnLetterHurdleSuccess();
                Debug.Log("✅ CheckBossSpell: Word completed! Ending letter event.");
            }
        }
    }

    public void ShowLetterHurdleClueImage(bool show)
    {
        if (letterHurdleClueImage != null)
            letterHurdleClueImage.gameObject.SetActive(show);
    }

    public void RefreshLetterHurdleClueImage() => UpdateLetterHurdleClueImage();

    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // AUDIO  –  Word Hurdle / Sentence Hurdle question audio
    // ─────────────────────────────────────────────────────────────────────────────

    #region Audio

    public void PlayQuestionAudio()
    {
        if (audioSource == null || audioPlayed) return;

        if (isSentenceQuestion)
        {
            if (sentencePronunciations != null && currentQuestionIndex < sentencePronunciations.Length
                && sentencePronunciations[currentQuestionIndex] != null)
            {
                audioSource.PlayOneShot(sentencePronunciations[currentQuestionIndex]);
                audioPlayed = true;
            }
        }
        else
        {
            if (currentPronunciationSounds != null && currentQuestionIndex < currentPronunciationSounds.Length
                && currentPronunciationSounds[currentQuestionIndex] != null)
            {
                audioSource.PlayOneShot(currentPronunciationSounds[currentQuestionIndex]);
                audioPlayed = true;
            }
        }
    }

    public void TriggerQuestionAudio() => PlayQuestionAudio();

    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // TRIGGER & UI
    // ─────────────────────────────────────────────────────────────────────────────

    #region Trigger and UI

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInTrigger = true;
        UpdateClueVisibility();
        if (playAudioOnTrigger) PlayQuestionAudio();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInTrigger = false;
        if (clueTextObject  != null) clueTextObject.SetActive(false);
        if (clueImageObject != null) clueImageObject.SetActive(false);
    }

    private void UpdateClueVisibility()
    {
        if (!playerInTrigger) return;

        if (isSentenceQuestion)
        {
            if (clueTextObject != null)
            {
                if (animateClueText && textRect != null && textCanvasGroup != null)
                    AnimateClueTextIn();
                else
                    clueTextObject.SetActive(true);
            }
            if (clueImageObject != null) clueImageObject.SetActive(false);
        }
        else
        {
            if (HasClueImage())
            {
                if (cluePic != null && currentQuestionIndex >= 0 && currentQuestionIndex < currentClueImages.Length)
                    cluePic.sprite = currentClueImages[currentQuestionIndex];

                if (animateClueImage && clueImageObject != null && imageRect != null && imageCanvasGroup != null)
                    AnimateClueImageIn();
                else if (clueImageObject != null)
                    clueImageObject.SetActive(true);

                if (clueTextObject != null) clueTextObject.SetActive(false);
            }
            else
            {
                if (clueTextObject != null)
                {
                    if (animateClueText && textRect != null && textCanvasGroup != null)
                        AnimateClueTextIn();
                    else
                        clueTextObject.SetActive(true);
                }
                if (clueImageObject != null) clueImageObject.SetActive(false);
            }
        }
    }

    public void ShowClueText()
    {
        if (playerInTrigger) UpdateClueVisibility();
    }

    public void HideClueText()
    {
        if (clueTextObject  != null) clueTextObject.SetActive(false);
        if (clueImageObject != null) clueImageObject.SetActive(false);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // ANIMATION
    // ─────────────────────────────────────────────────────────────────────────────

    #region Animation

    private void AnimateClueImageIn()
    {
        if (clueImageObject == null || imageRect == null || imageCanvasGroup == null) return;
        if (imageAnimationCoroutine != null) StopCoroutine(imageAnimationCoroutine);
        imageAnimationCoroutine = StartCoroutine(AnimateImageCoroutine());
    }

    private IEnumerator AnimateImageCoroutine()
    {
        clueImageObject.SetActive(true);
        imageCanvasGroup.alpha = 0f;

        Vector2 startPos = originalImagePos;
        switch (imageSlideDirection)
        {
            case SlideDirection.Left:  startPos.x -= imageSlideDistance; break;
            case SlideDirection.Right: startPos.x += imageSlideDistance; break;
            case SlideDirection.Up:    startPos.y += imageSlideDistance; break;
            case SlideDirection.Down:  startPos.y -= imageSlideDistance; break;
        }
        imageRect.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < imageAnimationDuration)
        {
            float t  = elapsed / imageAnimationDuration;
            float cv = imageAnimationCurve.Evaluate(t);
            imageCanvasGroup.alpha     = cv;
            imageRect.anchoredPosition = Vector2.Lerp(startPos, originalImagePos, cv);
            elapsed += Time.deltaTime;
            yield return null;
        }
        imageCanvasGroup.alpha     = 1f;
        imageRect.anchoredPosition = originalImagePos;
        imageAnimationCoroutine    = null;
    }

    private void AnimateClueTextIn()
    {
        if (clueTextObject == null || textRect == null || textCanvasGroup == null) return;
        if (textAnimationCoroutine != null) StopCoroutine(textAnimationCoroutine);
        textAnimationCoroutine = StartCoroutine(AnimateTextCoroutine());
    }

    private IEnumerator AnimateTextCoroutine()
    {
        clueTextObject.SetActive(true);
        textCanvasGroup.alpha = 0f;

        Vector2 startPos = originalTextPos;
        switch (textSlideDirection)
        {
            case SlideDirection.Left:  startPos.x -= textSlideDistance; break;
            case SlideDirection.Right: startPos.x += textSlideDistance; break;
            case SlideDirection.Up:    startPos.y += textSlideDistance; break;
            case SlideDirection.Down:  startPos.y -= textSlideDistance; break;
        }
        textRect.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < textAnimationDuration)
        {
            float t  = elapsed / textAnimationDuration;
            float cv = textAnimationCurve.Evaluate(t);
            textCanvasGroup.alpha     = cv;
            textRect.anchoredPosition = Vector2.Lerp(startPos, originalTextPos, cv);
            elapsed += Time.deltaTime;
            yield return null;
        }
        textCanvasGroup.alpha     = 1f;
        textRect.anchoredPosition = originalTextPos;
        textAnimationCoroutine    = null;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    // PUBLIC UTILITIES
    // ─────────────────────────────────────────────────────────────────────────────

    #region Public Utilities

    public void AddWordCount()
    {
        int currentWords = PlayerPrefs.GetInt("TotalWordCount", 0) + 1;
        PlayerPrefs.SetInt("TotalWordCount", currentWords);
        PlayerPrefs.Save();
        Debug.Log("Total Words: " + currentWords);
    }

    public string GetCurrentDifficulty() => SceneManager.GetActiveScene().name;

    public Sprite GetCurrentClueImage()
    {
        if (currentClueImages != null && currentQuestionIndex >= 0 && currentQuestionIndex < currentClueImages.Length)
            return currentClueImages[currentQuestionIndex];
        return null;
    }

    public bool HasClueImage()
    {
        return currentClueImages != null
            && currentQuestionIndex >= 0
            && currentQuestionIndex < currentClueImages.Length
            && currentClueImages[currentQuestionIndex] != null;
    }

    #endregion
}
// ─── CHANGE LOG ──────────────────────────────────────────────────────────────
// WordCompletionFlow now runs a per-frame loop instead of yield WaitForSeconds.
// Each frame it re-asserts letterHurdleClueImage.SetActive(true) and restores
// the completed word's sprite. This overrides any external hide (ObstacleSpawner
// hiding letterEventClueUI, trigger exits, etc.) every single frame until the
// full letterHurdleNextWordDelay has elapsed, guaranteeing the image stays on
// screen. Default delay raised to 4 seconds.