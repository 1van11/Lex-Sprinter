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

    [Header("Model Options")]
    public ModelOption[] models;

    [Header("Model Parent")]
    public Transform modelParent;

    private GameObject currentModel;
    private int currentIndex = 0;
    private int lastIndex = -1;

    void Start()
    {
        if (!replaceOnStart || models.Length == 0)
            return;

        // CHANGED: Read from "EquippedCostume" instead of "SelectedCostume"
        currentIndex = useSavedIndex
            ? PlayerPrefs.GetInt("EquippedCostume", 0)
            : 0;

        currentIndex = Mathf.Clamp(currentIndex, 0, models.Length - 1);

        ReplaceModel(models[currentIndex].modelPrefab);
        lastIndex = currentIndex;
    }

    void Update()
    {
        if (currentIndex == lastIndex)
            return;

        currentIndex = Mathf.Clamp(currentIndex, 0, models.Length - 1);
        ReplaceModel(models[currentIndex].modelPrefab);
        lastIndex = currentIndex;
    }

    public void ReplaceModel(GameObject prefab)
    {
        if (prefab == null || modelParent == null)
            return;

        // Remove existing model
        foreach (Transform child in modelParent)
        {
            Destroy(child.gameObject);
        }

        // Spawn new model
        currentModel = Instantiate(prefab, modelParent);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;

        // Sync animator if present
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
        if (index < 0 || index >= models.Length)
            return;

        if (!models[index].useTrigger)
            return;

        SetCostumeByIndex(index);
    }
}