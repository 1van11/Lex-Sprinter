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
///   • Click any word button (locked, unlocked, or clicked sprite) → shows the
///     Dictionary Panel at the bottom with that word's image + definition.
///   • BtnPlay_Pause toggles Play/Pause sprite and plays/stops the word audio.
///   • Sprite for the word illustration changes per word.
///   • BACK BTN hides the panel.
///   • Definition text uses a typewriter effect when the panel opens.
///
/// FIX (device compatibility):
///   • Typewriter coroutine now waits one frame after the panel is activated
///     before starting — prevents silent coroutine failure on real Android devices.
///   • Illustrations and AudioClips are keyed by word string via WordEntry[],
///     so there is no index mismatch with WordUnlockManager.
/// ─────────────────────────────────────────────────────────────────────────────
/// HOW TO SET UP IN INSPECTOR
/// ─────────────────────────────────────────────────────────────────────────────
///   1. Add this script to the DICTIONARY PANEL.
///   2. Assign all [Header] fields.
///   3. Word Entries[] — set the size to however many words you have.
///      For each element, type the word (e.g. "arm") and drag its sprite + clip.
///      Order does NOT matter.
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

    // ─── Word Entries ─────────────────────────────────────────────────────────
    [Header("Word Entries (word + illustration + audio — order doesn't matter)")]
    public WordEntry[] wordEntries;

    [System.Serializable]
    public class WordEntry
    {
        public string word;
        public Sprite illustration;
        public AudioClip audioClip;
    }

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
    private string pendingDefinition = "";   // stored until panel is active

    // Runtime lookups built from wordEntries[]
    private Dictionary<string, Sprite> illustrationMap = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, AudioClip> audioMap = new Dictionary<string, AudioClip>(System.StringComparer.OrdinalIgnoreCase);

    // ─────────────────────────────────────────────────────────────────────────
    // DEFINITIONS
    // ─────────────────────────────────────────────────────────────────────────
    private static readonly Dictionary<string, string> definitions =
        new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
    {
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
        { "dad",     "A male parent." },
        { "desk",    "A type of table, often with drawers and a flat surface, used for writing or working." },
        { "dog",     "A carnivorous mammal (Canis familiaris) domesticated as a pet and sometimes trained for work." },
        { "door",    "A usually flat object that closes and opens the entrance to a room or building." },
        { "duck",    "A bird that lives by water and has webbed feet, a short neck, and a large beak." },
        { "ear",     "Either of the organs on the sides of the head that you hear with." },
        { "egg",     "The round or oval reproductive body produced by female birds and other animals, often eaten as food." },
        { "eye",     "One of the two organs in your face that are used for seeing." },
        { "fast",    "Moving or capable of moving at high speed." },
        { "foot",    "The part of the body at the bottom of the leg on which a person stands." },
        { "friend",  "Someone you know well and like and usually trust." },
        { "girl",    "A female person who has not yet reached adulthood." },
        { "goat",    "An animal related to sheep, usually with horns, kept on farms for its milk, meat, or wool." },
        { "good",    "A favorable character or quality." },
        { "green",   "Having the colour of grass or the leaves of most plants and trees." },
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
    };

    // ─────────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        // Build word → sprite/audio lookup from wordEntries[]
        if (wordEntries != null)
        {
            foreach (var entry in wordEntries)
            {
                if (string.IsNullOrEmpty(entry.word)) continue;
                string key = entry.word.ToLower().Trim();
                if (entry.illustration != null) illustrationMap[key] = entry.illustration;
                if (entry.audioClip != null) audioMap[key] = entry.audioClip;
            }
        }

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
        if (definitions.TryGetValue(word, out string def))
            pendingDefinition = def;
        else
            pendingDefinition = "No definition available.";

        // Clear text immediately so old text doesn't flash
        if (definitionText != null)
            definitionText.text = "";

        // ── Illustration (by word key, NOT index) ──
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

        // Always activate the GameObject first
        dictionaryPanel.SetActive(true);

        if (animatePanel && panelRect != null)
        {
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            slideCoroutine = StartCoroutine(SlidePanel(panelRect.anchoredPosition, shownAnchoredPos));
        }

        // Start typewriter AFTER one frame so the panel is guaranteed active
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
    /// then starts the typewriter. This fixes the silent coroutine failure on
    /// real Android devices (e.g. Infinix Note 50).
    /// </summary>
    private IEnumerator TypewriterAfterFrame()
    {
        // Wait one frame — guarantees panel is active and layout is ready
        yield return null;

        // Extra safety: if definition text component is missing, bail out
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

    public static string[] GetWordList()
    {
        var keys = new List<string>(definitions.Keys);
        keys.Sort();
        return keys.ToArray();
    }
}