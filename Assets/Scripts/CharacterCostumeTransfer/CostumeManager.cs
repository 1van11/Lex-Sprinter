using UnityEngine;

public class CostumeManager : MonoBehaviour
{
    [System.Serializable]
    public class ModelOption
    {
        public GameObject modelPrefab;
        public bool useTrigger; // checkbox per element
    }

    [Header("Settings")]
    public bool replaceOnStart = true;
    public bool useSavedIndex = true; // New option to use saved index from menu

    [Header("Model Options")]
    public ModelOption[] models;

    [Header("Model Parent")]
    public Transform modelParent;

    private GameObject currentModel;

    void Start()
    {
        if (replaceOnStart && models.Length > 0)
        {
            int startIndex = 0;
            
            if (useSavedIndex)
            {
                // Get the saved index from PlayerPrefs
                startIndex = PlayerPrefs.GetInt("SelectedCostume", 0);
            }
            else
            {
                // Use a fixed start index (for testing)
                startIndex = 0;
            }
            
            startIndex = Mathf.Clamp(startIndex, 0, models.Length - 1);
            ReplaceModel(models[startIndex].modelPrefab);
        }
    }

    public void ReplaceModel(GameObject prefab)
    {
        if (prefab == null || modelParent == null) return;

        foreach (Transform child in modelParent)
        {
            Destroy(child.gameObject);
        }

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

    // Called by triggers
    public void TriggerModel(int index)
    {
        if (index < 0 || index >= models.Length) return;
        if (!models[index].useTrigger) return;

        ReplaceModel(models[index].modelPrefab);
    }
    
    // Optional: Method to change costume directly from other scripts
    public void SetCostumeByIndex(int index)
    {
        if (index < 0 || index >= models.Length) return;
        ReplaceModel(models[index].modelPrefab);
    }
}