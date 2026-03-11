using UnityEngine;

public class GameplayCostumeManager : MonoBehaviour
{
    [System.Serializable]
    public class ModelOption
    {
        public GameObject modelPrefab;
        public bool useTrigger;
    }

    [Header("Settings")]
    public bool replaceOnStart = true;        // Instantiate on Start? (should be true)

    [Header("Character Type")]
    [SerializeField] private bool _useGirlCostumes = true;
    public bool useGirlCostumes
    {
        get => _useGirlCostumes;
        set
        {
            if (_useGirlCostumes == value) return;
            _useGirlCostumes = value;
            // Update activeModels immediately when gender changes
            activeModels = _useGirlCostumes ? girlModels : boyModels;
            // If we already have a valid index, refresh the model
            if (activeModels != null && activeModels.Length > 0 && currentIndex >= 0 && currentIndex < activeModels.Length)
            {
                currentIndex = Mathf.Clamp(currentIndex, 0, activeModels.Length - 1);
                ReplaceModel(activeModels[currentIndex].modelPrefab);
            }
        }
    }

    [Header("Girl Costumes")]
    public ModelOption[] girlModels;

    [Header("Boy Costumes")]
    public ModelOption[] boyModels;

    [Header("Model Parent")]
    public Transform modelParent;

    private GameObject currentModel;
    private int currentIndex = 0;
    private int lastIndex = -1;
    private ModelOption[] activeModels;

    void Start()
    {
        if (!replaceOnStart) return;

        // ----- READ FROM PLAYER PREFS -----
        int selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 1); // default 1 = boy
        int costumeIndex = PlayerPrefs.GetInt("EquippedCostume", 0);

        // Set gender based on saved character
        _useGirlCostumes = (selectedCharacter == 2);
        // ---------------------------------

        // Select the correct array
        activeModels = _useGirlCostumes ? girlModels : boyModels;

        if (activeModels == null || activeModels.Length == 0)
        {
            Debug.LogWarning("No models assigned for the selected gender.");
            return;
        }

        // Clamp and use the saved costume index
        currentIndex = Mathf.Clamp(costumeIndex, 0, activeModels.Length - 1);
        ReplaceModel(activeModels[currentIndex].modelPrefab);
        lastIndex = currentIndex;
    }

    void Update()
    {
        if (activeModels == null || activeModels.Length == 0) return;
        if (currentIndex == lastIndex) return;

        currentIndex = Mathf.Clamp(currentIndex, 0, activeModels.Length - 1);
        ReplaceModel(activeModels[currentIndex].modelPrefab);
        lastIndex = currentIndex;
    }

    public void ReplaceModel(GameObject prefab)
    {
        if (prefab == null || modelParent == null) return;

        foreach (Transform child in modelParent)
            Destroy(child.gameObject);

        currentModel = Instantiate(prefab, modelParent);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;

        Animator newAnimator = currentModel.GetComponent<Animator>();
        Animator parentAnimator = modelParent.GetComponent<Animator>();
        if (newAnimator != null && parentAnimator != null)
        {
            parentAnimator.runtimeAnimatorController = newAnimator.runtimeAnimatorController;
            parentAnimator.applyRootMotion = newAnimator.applyRootMotion;
        }
    }

    public void SetCostumeByIndex(int index)
    {
        currentIndex = index;
    }

    /// <summary>
    /// Call this from CharacterDisplay if both scripts are in the same scene.
    /// </summary>
    public void SetCharacter(bool isGirl, int index)
    {
        useGirlCostumes = isGirl;  // updates activeModels and refreshes
        currentIndex = index;       // in case the setter already refreshed
    }

    public void TriggerModel(int index)
    {
        if (activeModels == null) return;
        if (index < 0 || index >= activeModels.Length) return;
        if (!activeModels[index].useTrigger) return;
        SetCostumeByIndex(index);
    }
}