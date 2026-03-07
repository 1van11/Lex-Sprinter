using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// DictionaryWordViewer
/// ─────────────────────────────────────────────────────────────────────────────
/// Attach this to the DICTIONARY PANEL root object.
///
/// Features:
///   • Click any word button → shows the Dictionary Panel with image + definition.
///   • BtnPlay_Pause toggles Play/Pause sprite and plays/stops the word audio.
///   • Sprite for the word illustration changes per word.
///   • BACK BTN hides the panel.
///   • Definition text uses a typewriter effect when the panel opens.
///   • Supports Easy Words (100), Medium Words (50), and Hard Words (50).
///
/// HOW TO SET UP IN INSPECTOR
/// ─────────────────────────────────────────────────────────────────────────────
///   1. Add this script to the DICTIONARY PANEL.
///   2. Assign all [Header] fields.
///   3. Easy Words[]   — 100 easy words  (word + illustration + audio).
///   4. Medium Words[] — 50 medium words (word + illustration + audio).
///   5. Hard Words[]   — 50 hard words   (word + illustration + audio).
///      Order does NOT matter for any list.
/// </summary>
public class DictionaryWordViewer : MonoBehaviour
{
    // ─── Panel & core UI ──────────────────────────────────────────────────────
    [Header("Panel References")]
    [Tooltip("The root panel that slides in/out (this object or a child).")]
    public GameObject dictionaryPanel;

    [Tooltip("Image component that shows the word illustration.")]
    public Image wordIllustrationImage;

    [Tooltip("TMP text that shows the word.")]
    public TMP_Text wordTitleText;

    [Tooltip("TMP text that shows the definition.")]
    public TMP_Text definitionText;

    [Tooltip("The BACK / close button.")]
    public Button backButton;

    // ─── Play / Pause button ──────────────────────────────────────────────────
    [Header("Play / Pause Button")]
    public Button btnPlayPause;
    public Image btnPlayPauseImage;
    public Sprite playSprite;
    public Sprite pauseSprite;

    // ─── Audio ────────────────────────────────────────────────────────────────
    [Header("Audio")]
    public AudioSource audioSource;

    // ─── Word Entry class ─────────────────────────────────────────────────────
    [System.Serializable]
    public class WordEntry
    {
        public string word;
        public Sprite illustration;
        public AudioClip audioClip;
    }

    // ─── Easy Words (100 words) ───────────────────────────────────────────────
    [Header("Easy Words (100 words — word + illustration + audio)")]
    public WordEntry[] easyWords;

    // ─── Medium Words (50 words) ──────────────────────────────────────────────
    [Header("Medium Words (50 words — word + illustration + audio)")]
    public WordEntry[] mediumWords;

    // ─── Hard Words (50 words) ────────────────────────────────────────────────
    [Header("Hard Words (50 words — word + illustration + audio)")]
    public WordEntry[] hardWords;

    // ─── Panel Slide Animation ────────────────────────────────────────────────
    [Header("Panel Slide Animation")]
    public bool animatePanel = true;
    public float slideDuration = 0.25f;
    public Vector2 hiddenAnchoredPos = new Vector2(0, -600f);
    public Vector2 shownAnchoredPos = new Vector2(0, 0f);

    // ─── Typewriter Effect ────────────────────────────────────────────────────
    [Header("Typewriter Effect")]
    [Tooltip("Seconds per character. Lower = faster typing.")]
    public float typewriterSpeed = 0.04f;

    // ─── Internal state ───────────────────────────────────────────────────────
    private RectTransform panelRect;
    private Coroutine slideCoroutine;
    private Coroutine typewriterCoroutine;
    private bool isPlaying = false;
    private string currentWord = "";
    private string pendingDefinition = "";

    // Combined runtime lookup: word → sprite / audio (all difficulties merged)
    private Dictionary<string, Sprite> illustrationMap = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, AudioClip> audioMap = new Dictionary<string, AudioClip>(System.StringComparer.OrdinalIgnoreCase);

    // ─────────────────────────────────────────────────────────────────────────
    // DEFINITIONS  (Easy 100 + Medium 50 + Hard 50 = 200 words)
    // ─────────────────────────────────────────────────────────────────────────
    private static readonly Dictionary<string, string> definitions =
        new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
    {
        // ══ EASY (100) ════════════════════════════════════════════════════════
        { "arm",     "A human upper limb, especially the part between the shoulder and the wrist." },
        { "aunt",    "The sister of one's father or mother (or the wife of one's uncle)." },
        { "baby",    "An extremely young child or infant." },
        { "bad",     "Failing to reach an acceptable standard." },
        { "bag",     "A flexible container, often made of paper, plastic, cloth, or leather, used to hold, carry, or store things." },
        { "ball",    "A round object used for throwing, hitting, or kicking in games and sports." },
        { "bed",     "A piece of furniture on which one lies or sleeps." },
        { "bell",    "A hollow metal object that makes a ringing sound when struck." },
        { "big",     "Large or great in size, bulk, or extent." },
        { "bird",    "Any warm-blooded vertebrate animal covered with feathers and having forelimbs modified as wings." },
        { "black",   "The color that is the darkest, like the absence of light." },
        { "blue",    "Being the color of the clear sky or ocean." },
        { "book",    "A written text that can be published in printed or electronic form." },
        { "box",     "A rigid typically rectangular container with or without a cover." },
        { "boy",     "A male child from birth to adulthood." },
        { "bread",   "A baked food made from flour and water, and usually yeast." },
        { "brown",   "A dark color between red and yellow, often like chocolate or earth." },
        { "bus",     "A large motor vehicle designed to carry many passengers along a route." },
        { "cake",    "A sweet baked food made from a mixture of flour, eggs, fat, and sugar." },
        { "car",     "A road vehicle with an engine, four wheels, and seating for passengers." },
        { "cat",     "A carnivorous mammal (Felis catus) long kept as a pet and for catching rats and mice." },
        { "chair",   "A seat for one person that has a back, usually four legs." },
        { "cloud",   "A visible mass of tiny droplets of water or ice high in the sky." },
        { "cold",    "A low or relatively low temperature." },
        { "corn",    "A cereal plant with yellow seeds that people and animals eat." },
        { "cow",     "The adult female of cattle that is kept on a farm to produce milk or meat." },
        { "cup",     "An open usually bowl-shaped drinking vessel." },
        { "dad",     "A male parent." },
        { "desk",    "A type of table, often with drawers and a flat surface, used for writing or working." },
        { "dog",     "A carnivorous mammal (Canis familiaris) domesticated as a pet and sometimes trained for work." },
        { "door",    "A usually flat object that closes and opens the entrance to a room or building." },
        { "duck",    "A bird that lives by water and has webbed feet, a short neck, and a large beak." },
        { "ear",     "Either of the organs on the sides of the head that you hear with." },
        { "egg",     "The round or oval reproductive body produced by female birds and other animals, often eaten as food." },
        { "eye",     "One of the two organs in your face that are used for seeing." },
        { "fast",    "Moving or capable of moving at high speed." },
        { "fish",    "An aquatic animal, usually a cold-blooded vertebrate that lives in water and has fins and gills." },
        { "foot",    "The part of the body at the bottom of the leg on which a person stands." },
        { "friend",  "Someone you know well and like and usually trust." },
        { "girl",    "A female person who has not yet reached adulthood." },
        { "goat",    "An animal related to sheep, usually with horns, kept on farms for its milk, meat, or wool." },
        { "good",    "A favorable character or quality." },
        { "gray",    "A neutral color between black and white." },
        { "green",   "Having the colour of grass or the leaves of most plants and trees." },
        { "hair",    "The mass of thin thread-like structures on the head of a person." },
        { "hand",    "The part of the body at the end of the arm that is used for holding, moving, touching, and feeling things." },
        { "happy",   "Enjoying or showing well-being, contentment, or pleasure." },
        { "hat",     "A covering for the head usually having a shaped crown and brim." },
        { "hen",     "An adult female chicken, often kept for its eggs." },
        { "her",     "A third-person singular feminine pronoun used as the object of a verb or preposition." },
        { "him",     "A third-person singular masculine pronoun used as the object of a verb or preposition." },
        { "hop",     "To jump on one foot or with both feet together." },
        { "hot",     "Having a high temperature." },
        { "jam",     "A sweet spread made by cooking fruit with sugar." },
        { "jump",    "To push yourself suddenly off the ground and into the air using your legs." },
        { "kid",     "A young person (especially a child)." },
        { "leaf",    "A flattened green part of a plant that grows from a stem and makes food for the plant through photosynthesis." },
        { "leg",     "A limb of an animal used especially for supporting the body and for walking." },
        { "lip",     "One of the two soft parts that form the upper and lower edges of the mouth." },
        { "man",     "An individual human, especially an adult male human." },
        { "map",     "A drawing that represents a region or place, showing features such as roads, rivers, and boundaries." },
        { "me",      "Used, usually as the object of a verb or preposition, to refer to the person speaking or writing." },
        { "meat",    "The soft part of an animal or a bird that can be eaten as food." },
        { "milk",    "A fluid produced by the mammary glands of female mammals, used to nourish their young and consumed as food by people." },
        { "mom",     "A female parent." },
        { "moon",    "The large natural object that moves around Earth and shines by reflecting light from the sun; often visible at night." },
        { "mouse",   "A small rodent that typically has a pointed snout, small rounded ears, and a long tail." },
        { "nose",    "The part of the face that contains the nostrils and organs of smell." },
        { "orange",  "A color between red and yellow (like the fruit)." },
        { "pear",    "The sweet, edible fruit of a tree with a rounded bottom and a narrower top." },
        { "pen",     "An instrument for writing with ink." },
        { "pig",     "A large pink, brown, or black farm animal with short legs and a curved tail, kept for its meat." },
        { "pink",    "A pale red color." },
        { "rain",    "Water that falls in drops from clouds in the sky." },
        { "read",    "To look at written words and understand their meaning." },
        { "red",     "Being the same color as fire." },
        { "rice",    "The starchy seeds of a plant (Oryza sativa) cooked and eaten as food." },
        { "rock",    "A hard solid substance made naturally from minerals." },
        { "run",     "The act of moving quickly on foot." },
        { "sad",     "Affected with or expressive of grief or unhappiness." },
        { "sing",    "To make musical sounds with your voice." },
        { "sit",     "To rest your weight on your bottom with your back straight." },
        { "slow",    "Moving at less than usual speed." },
        { "small",   "Not large in size, number, degree, amount, etc." },
        { "snow",    "Small, soft white pieces of ice that fall from the sky when the weather is very cold." },
        { "soup",    "A liquid food made by cooking ingredients (like vegetables or meat) in water or stock." },
        { "stand",   "To be in an upright position on the feet." },
        { "star",    "A very large ball of burning gas in space that shines with its own light." },
        { "sun",     "The star at the center of our solar system that gives light and warmth to the Earth." },
        { "swim",    "To move through water by moving your arms and legs." },
        { "tooth",   "One of the hard, bony structures in the mouth used especially for biting and chewing." },
        { "toy",     "Something for a child to play with." },
        { "tree",    "A woody plant that lives for many years, usually with a single tall main stem and branches." },
        { "uncle",   "The brother of one's father or mother (or the husband of one's auntie)." },
        { "walk",    "To move on foot at a normal pace." },
        { "water",   "A clear, liquid substance that falls as rain and that people and animals drink." },
        { "white",   "A very light color like snow or milk." },
        { "wind",    "The natural movement of air outside, especially when it's moving noticeably." },
        { "write",   "To make marks that represent letters or words on a surface (like paper) to record information or ideas." },
        { "yellow",  "A color like that of ripe lemons or sunflowers." },
        { "you",     "Used to refer to the person or people being spoken or written to." },

        // ══ MEDIUM (50) ═══════════════════════════════════════════════════════
        { "alligator",   "A large reptile with powerful jaws, found in wetlands of the Americas and China." },
        { "anchor",      "A heavy object dropped from a ship to keep it in place." },
        { "armor",       "Metal protective clothing worn in battle." },
        { "banana",      "A long curved yellow fruit." },
        { "basket",      "A container for carrying things, often woven from straw or wire." },
        { "blanket",     "A warm covering for a bed." },
        { "blizzard",    "A severe snow storm with strong winds." },
        { "bottle",      "A container for liquids, usually made of glass or plastic." },
        { "bridge",      "A structure built to cross water or another obstacle." },
        { "cactus",      "A desert plant with spines instead of leaves." },
        { "castle",      "A large fortified building, typically from the Middle Ages." },
        { "cheese",      "A dairy food made from milk, available in many varieties." },
        { "chocolate",   "Sweet brown food made from cocoa beans." },
        { "compass",     "A tool that shows north, south, east, and west." },
        { "cookie",      "A small sweet baked treat." },
        { "crown",       "A royal head decoration, often made of gold and jewels." },
        { "desert",      "A dry barren area with little rain." },
        { "eagle",       "A large bird of prey with a hooked beak and sharp talons." },
        { "forest",      "A large area covered with trees." },
        { "garden",      "An area for growing plants, flowers, or vegetables." },
        { "giraffe",     "A tall African animal with a very long neck." },
        { "glacier",     "A huge slow-moving river of ice." },
        { "helmet",      "Protective headgear worn for safety." },
        { "hurricane",   "A powerful tropical cyclone with very strong winds." },
        { "instrument",  "A tool used to create music." },
        { "jewelry",     "Decorative items worn on the body, such as rings or necklaces." },
        { "ladder",      "A set of steps or rungs used for climbing up or down." },
        { "lantern",     "A portable light source with a protective case." },
        { "lighthouse",  "A tower with a bright light to guide ships safely." },
        { "magnet",      "A metal object that attracts iron and other magnetic materials." },
        { "market",      "A public place where goods are sold." },
        { "melon",       "A large sweet juicy fruit with a hard rind." },
        { "microscope",  "A tool used to see very small things magnified." },
        { "monkey",      "A tree-climbing primate with a long tail." },
        { "octopus",     "A sea creature with eight arms and a soft body." },
        { "panther",     "A large black big cat, often a melanistic leopard or jaguar." },
        { "penguin",     "A flightless bird from cold regions that swims expertly." },
        { "pillow",      "A soft support for the head during sleep." },
        { "rabbit",      "A small burrowing mammal with long ears." },
        { "school",      "A place for learning and education." },
        { "scissors",    "A cutting tool with two sharp blades joined together." },
        { "spaghetti",   "A long Italian noodle dish served with sauce." },
        { "statue",      "A carved or cast figure of a person or animal." },
        { "street",      "A public road in a town or city." },
        { "telescope",   "An optical tool used to see distant objects more clearly." },
        { "tiger",       "A large striped big cat native to Asia." },
        { "tomato",      "A red juicy fruit often used in sauces and salads." },
        { "volcano",     "A mountain that erupts with lava and ash." },
        { "windmill",    "A building with large blades that turn in the wind to generate power or grind grain." },
        { "zebra",       "An African striped animal related to the horse." },

        // ══ HARD (50) ═════════════════════════════════════════════════════════
        { "aardvark",      "A nocturnal African mammal with a long snout that feeds on ants and termites." },
        { "amphitheater",  "An open-air venue with a central stage surrounded by rising tiers of seats for spectators." },
        { "armadillo",     "A small mammal with leathery armor-like plates covering its body that can roll into a ball for protection." },
        { "astrolabe",     "An ancient astronomical instrument used by mariners to tell time and determine latitude by measuring the altitude of stars." },
        { "axolotl",       "A rare aquatic salamander native to Mexico that keeps its larval form throughout its adult life and can regenerate lost limbs." },
        { "ballista",      "An ancient missile weapon shaped like a giant crossbow that launches heavy bolts or stones with great power." },
        { "battlement",    "A defensive parapet at the top of a castle wall with alternating high sections and gaps for soldiers to shoot through." },
        { "carousel",      "A rotating circular platform with seats, often shaped like horses, that people ride for amusement." },
        { "catapult",      "A medieval device used to hurl heavy stones or projectiles over long distances during battles." },
        { "centaur",       "A mythical creature from Greek folklore with the upper body of a human and the lower body of a horse." },
        { "chameleon",     "A lizard famous for its ability to change color and move its eyes independently." },
        { "chandelier",    "A decorative hanging light fixture with multiple arms and ornate crystals." },
        { "chrysalis",     "The hard-shelled protective stage of a caterpillar's life cycle during which it transforms into a butterfly." },
        { "cockatoo",      "A colorful parrot with a prominent feathered crest on its head and a highly social personality." },
        { "colosseum",     "A massive oval amphitheater in ancient Rome famous for hosting gladiator battles and public spectacles." },
        { "drawbridge",    "A movable bridge over a castle's moat that can be raised to prevent entry or lowered to allow crossing." },
        { "gargoyle",      "A carved stone figure shaped like a grotesque creature, designed to act as a decorative rain spout on buildings." },
        { "gladiator",     "A professional fighter in ancient Rome who battled other warriors or wild animals in public arenas." },
        { "guillotine",    "A device designed for executions by a heavy falling blade that quickly severs the head." },
        { "harpoon",       "A long spear-like weapon with a barbed head, traditionally used for hunting whales or large fish." },
        { "hieroglyph",    "A stylized picture or symbol used as a character in ancient Egyptian writing." },
        { "kaleidoscope",  "An optical toy with mirrors and colorful pieces that creates beautiful symmetrical patterns when rotated." },
        { "labyrinth",     "A complex network of intricate paths and passages designed as a confusing maze." },
        { "marquee",       "A large tent or a brightly lit sign over a building entrance used to display its name or current features." },
        { "menagerie",     "A collection of diverse or exotic wild animals kept in captivity for exhibition." },
        { "minotaur",      "A powerful mythical creature from Greek mythology with the head of a bull and the body of a man." },
        { "monolith",      "A large single upright block of stone, often shaped into a pillar or monument by ancient people." },
        { "narwhal",       "A medium-sized whale found in Arctic waters with a long spiral tusk, often called the unicorn of the sea." },
        { "obelisk",       "A tall four-sided stone pillar that tapers to a pyramid-like point at the top." },
        { "obsidian",      "A naturally occurring volcanic glass formed when lava cools rapidly." },
        { "oubliette",     "A secret dungeon in a castle where prisoners were thrown and forgotten, accessible only through a ceiling opening." },
        { "parthenon",     "An ancient Greek temple on the Acropolis in Athens dedicated to the goddess Athena." },
        { "periscope",     "An optical instrument using mirrors or prisms to allow viewing of objects outside the direct line of sight." },
        { "pharaoh",       "A powerful ruler of ancient Egypt who was viewed as both a political leader and a living god." },
        { "platypus",      "A unique egg-laying mammal from Australia with a duck-like bill, beaver-like tail, and otter-like feet." },
        { "portcullis",    "A heavy vertically-sliding gate of wood or iron used to seal the entrance of a medieval castle during attack." },
        { "pyramid",       "A massive stone structure with a square base and four triangular sides meeting at a point, built as royal tombs in ancient Egypt." },
        { "quokka",        "A small furry marsupial from Australia famous for its round face and friendly smiling expression." },
        { "samurai",       "A highly skilled warrior of pre-modern Japan who followed a strict code of honor known as Bushido." },
        { "sarcophagus",   "An ornate stone coffin decorated with carvings, used for burying royalty in ancient civilizations." },
        { "scorpion",      "A predatory arachnid with eight legs, grasping pincers, and a venomous stinger at the end of its curved tail." },
        { "sextant",       "A precision navigational instrument used to measure the angle between a celestial object and the horizon." },
        { "sphinx",        "A mythical creature with the body of a lion and the head of a human, famous in both Egyptian and Greek traditions." },
        { "spyglass",      "A small portable telescope used to see distant objects more clearly." },
        { "tarantula",     "A large hairy spider found in warm regions, known for its impressive size and generally docile nature." },
        { "trebuchet",     "A powerful medieval siege engine that uses a heavy counterweight to hurl large projectiles over great distances." },
        { "trident",       "A three-pronged spear traditionally used for fishing and as the weapon of the sea god Poseidon." },
        { "viking",        "A seafaring warrior and explorer from Scandinavia who traveled across Europe between the 8th and 11th centuries." },
        { "xylophone",     "A percussion instrument made of wooden bars that produce musical notes when struck with mallets." },
        { "ziggurat",      "A massive terraced step pyramid built in ancient Mesopotamia as a towering temple platform to honor the gods." },
    };

    // ─────────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        // Build combined word → sprite/audio lookup from all three entry arrays
        RegisterWordEntries(easyWords);
        RegisterWordEntries(mediumWords);
        RegisterWordEntries(hardWords);

        if (dictionaryPanel != null)
            panelRect = dictionaryPanel.GetComponent<RectTransform>();

        if (dictionaryPanel != null)
        {
            if (animatePanel && panelRect != null)
                panelRect.anchoredPosition = hiddenAnchoredPos;
            else
                dictionaryPanel.SetActive(false);
        }
    }

    /// <summary>Registers all entries from a WordEntry array into the runtime lookup maps.</summary>
    private void RegisterWordEntries(WordEntry[] entries)
    {
        if (entries == null) return;
        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.word)) continue;
            string key = entry.word.ToLower().Trim();
            if (entry.illustration != null && !illustrationMap.ContainsKey(key))
                illustrationMap[key] = entry.illustration;
            if (entry.audioClip != null && !audioMap.ContainsKey(key))
                audioMap[key] = entry.audioClip;
        }
    }

    void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(HidePanel);
        if (btnPlayPause != null) btnPlayPause.onClick.AddListener(TogglePlayPause);
        SetPlayIcon();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────────────────────────

    public void ShowWord(string word)
    {
        word = word.ToLower().Trim();
        currentWord = word;

        // ── Word title ──
        if (wordTitleText != null)
            wordTitleText.text = word.ToUpper();

        // ── Store definition — typewriter starts AFTER panel is active ──
        pendingDefinition = definitions.TryGetValue(word, out string def)
            ? def
            : "No definition available.";

        // Clear text immediately so old text doesn't flash
        if (definitionText != null)
            definitionText.text = "";

        // ── Illustration (by word key) ──
        if (wordIllustrationImage != null)
        {
            if (illustrationMap.TryGetValue(word, out Sprite sprite))
            {
                wordIllustrationImage.sprite = sprite;
                wordIllustrationImage.enabled = true;
            }
            else
            {
                wordIllustrationImage.enabled = false;
            }
        }

        // ── Reset audio ──
        StopAudio();
        SetPlayIcon();

        // ── Show panel, then start typewriter once panel is fully active ──
        ShowPanel();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PANEL SHOW / HIDE
    // ─────────────────────────────────────────────────────────────────────────

    public void ShowPanel()
    {
        if (dictionaryPanel == null) return;

        dictionaryPanel.SetActive(true);

        if (animatePanel && panelRect != null)
        {
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            slideCoroutine = StartCoroutine(SlidePanel(panelRect.anchoredPosition, shownAnchoredPos));
        }

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterAfterFrame());
    }

    public void HidePanel()
    {
        StopAudio();
        SetPlayIcon();

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (dictionaryPanel == null) return;

        if (animatePanel && panelRect != null)
        {
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            slideCoroutine = StartCoroutine(SlidePanel(panelRect.anchoredPosition, hiddenAnchoredPos,
                onComplete: () => dictionaryPanel.SetActive(false)));
        }
        else
        {
            dictionaryPanel.SetActive(false);
        }
    }

    private IEnumerator SlidePanel(Vector2 from, Vector2 to, System.Action onComplete = null)
    {
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            panelRect.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }
        panelRect.anchoredPosition = to;
        onComplete?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TYPEWRITER EFFECT
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Waits one frame to ensure the panel GameObject is fully active on device,
    /// then starts the typewriter. Fixes silent coroutine failure on real Android devices.
    /// </summary>
    private IEnumerator TypewriterAfterFrame()
    {
        yield return null;
        if (definitionText == null) yield break;
        yield return StartCoroutine(TypewriterEffect(pendingDefinition));
    }

    private IEnumerator TypewriterEffect(string fullText)
    {
        definitionText.text = "";
        foreach (char c in fullText)
        {
            definitionText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        typewriterCoroutine = null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PLAY / PAUSE
    // ─────────────────────────────────────────────────────────────────────────

    private void TogglePlayPause()
    {
        if (isPlaying) StopAudio();
        else PlayAudio();
    }

    private void PlayAudio()
    {
        if (audioSource == null) return;

        if (!audioMap.TryGetValue(currentWord, out AudioClip clip) || clip == null)
        {
            Debug.LogWarning($"[DictionaryWordViewer] No audio clip for word: '{currentWord}'");
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
        isPlaying = true;
        SetPauseIcon();
        StartCoroutine(WaitForAudioEnd(clip.length));
    }

    private void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
        isPlaying = false;
    }

    private IEnumerator WaitForAudioEnd(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (isPlaying)
        {
            isPlaying = false;
            SetPlayIcon();
        }
    }

    private void SetPlayIcon()
    {
        if (btnPlayPauseImage != null && playSprite != null)
            btnPlayPauseImage.sprite = playSprite;
        isPlaying = false;
    }

    private void SetPauseIcon()
    {
        if (btnPlayPauseImage != null && pauseSprite != null)
            btnPlayPauseImage.sprite = pauseSprite;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Returns all words (Easy + Medium + Hard) sorted alphabetically.</summary>
    public static string[] GetWordList()
    {
        var keys = new List<string>(definitions.Keys);
        keys.Sort();
        return keys.ToArray();
    }
}