using UnityEngine;

public class CostumeManager : MonoBehaviour
{
    [System.Serializable]
    public class ModelOption
    {
        public GameObject modelPrefab;
        public bool useTrigger;
    }

    [Header("Settings")]
    public bool replaceOnStart = true;
    public bool useSavedIndex = true;

    [Header("Model Options")]
    public ModelOption[] models;

    [Header("Model Parent")]
    public Transform modelParent;

    private GameObject currentModel;
    private int currentIndex = 0;
    private int lastIndex = -1; // tracks the previous index

    void Start()
    {
        if (replaceOnStart && models.Length > 0)
        {
            currentIndex = useSavedIndex ? PlayerPrefs.GetInt("SelectedCostume", 0) : 0;
            currentIndex = Mathf.Clamp(currentIndex, 0, models.Length - 1);
            ReplaceModel(models[currentIndex].modelPrefab);
            lastIndex = currentIndex; // initialize lastIndex
        }
    }

    void Update()
    {
        // If currentIndex has changed, update the costume
        if (currentIndex != lastIndex)
        {
            currentIndex = Mathf.Clamp(currentIndex, 0, models.Length - 1);
            ReplaceModel(models[currentIndex].modelPrefab);
            lastIndex = currentIndex;
        }
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

        if (newAnimator && parentAnimator)
        {
            parentAnimator.runtimeAnimatorController = newAnimator.runtimeAnimatorController;
            parentAnimator.applyRootMotion = newAnimator.applyRootMotion;
        }
    }

    public void SetCostumeByIndex(int index)
    {
        currentIndex = index; // simply set the index; Update() will handle replacement
    }

    public void TriggerModel(int index)
    {
        if (index < 0 || index >= models.Length) return;
        if (!models[index].useTrigger) return;

        SetCostumeByIndex(index); // just set index, Update handles swapping
    }
}
//testing