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

    [Header("Saved Index (Editable)")]
    public int savedCostumeIndex = 0;

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

        currentIndex = useSavedIndex
            ? PlayerPrefs.GetInt("SelectedCostume", savedCostumeIndex)
            : savedCostumeIndex;

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

        // Keep Inspector + PlayerPrefs in sync
        savedCostumeIndex = currentIndex;
        PlayerPrefs.SetInt("SelectedCostume", currentIndex);

        lastIndex = currentIndex;
    }

    public void ReplaceModel(GameObject prefab)
    {
        if (prefab == null || modelParent == null)
            return;

        foreach (Transform child in modelParent)
            Destroy(child.gameObject);

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
        if (index < 0 || index >= models.Length)
            return;

        if (!models[index].useTrigger)
            return;

        SetCostumeByIndex(index);
    }
}
