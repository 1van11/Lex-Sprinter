using System.Collections;
using System.Collections.Generic;
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
    public bool replaceOnStart = true;
    public bool useSavedIndex = true;

    [Header("Character Type")]
    public bool useGirlCostumes = true; // true = girl, false = boy

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
        if (!replaceOnStart)
            return;

        // Select which costume array to use
        activeModels = useGirlCostumes ? girlModels : boyModels;

        if (activeModels.Length == 0)
            return;

        currentIndex = useSavedIndex
            ? PlayerPrefs.GetInt("EquippedCostume", 0)
            : 0;

        currentIndex = Mathf.Clamp(currentIndex, 0, activeModels.Length - 1);

        ReplaceModel(activeModels[currentIndex].modelPrefab);
        lastIndex = currentIndex;
    }

    void Update()
    {
        if (activeModels == null || activeModels.Length == 0)
            return;

        if (currentIndex == lastIndex)
            return;

        currentIndex = Mathf.Clamp(currentIndex, 0, activeModels.Length - 1);
        ReplaceModel(activeModels[currentIndex].modelPrefab);
        lastIndex = currentIndex;
    }

    public void ReplaceModel(GameObject prefab)
    {
        if (prefab == null || modelParent == null)
            return;

        foreach (Transform child in modelParent)
        {
            Destroy(child.gameObject);
        }

        currentModel = Instantiate(prefab, modelParent);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;

        Animator newAnimator = currentModel.GetComponent<Animator>();
        Animator parentAnimator = modelParent.GetComponent<Animator>();

        if (newAnimator != null && parentAnimator != null)
        {
            parentAnimator.runtimeAnimatorController =
                newAnimator.runtimeAnimatorController;

            parentAnimator.applyRootMotion =
                newAnimator.applyRootMotion;
        }
    }

    public void SetCostumeByIndex(int index)
    {
        currentIndex = index;
    }

    public void TriggerModel(int index)
    {
        if (activeModels == null)
            return;

        if (index < 0 || index >= activeModels.Length)
            return;

        if (!activeModels[index].useTrigger)
            return;

        SetCostumeByIndex(index);
    }
}