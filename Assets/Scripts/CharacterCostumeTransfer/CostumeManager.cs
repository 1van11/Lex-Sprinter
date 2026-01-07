using UnityEngine;

public class CostumeManager : MonoBehaviour
{
    [Header("Settings")]
    public bool replaceOnStart = true;
    public GameObject newModelPrefab;   // The prefab of the new character model
    public Transform modelParent;       // The parent where the model should go

    private GameObject currentModel;

    void Start()
    {
        if (replaceOnStart && newModelPrefab != null)
        {
            ReplaceModel(newModelPrefab);
        }
    }

    public void ReplaceModel(GameObject prefab)
    {
        if (prefab == null || modelParent == null)
        {
            Debug.LogWarning("Prefab or modelParent not assigned!");
            return;
        }

        // Destroy all existing children of the modelParent
        foreach (Transform child in modelParent)
        {
            Destroy(child.gameObject);
        }

        // Instantiate the new model as a child of modelParent
        currentModel = Instantiate(prefab, modelParent);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;

        // Assign Animator (if your scripts use it)
        Animator newAnimator = currentModel.GetComponent<Animator>();
        Animator parentAnimator = modelParent.GetComponent<Animator>();
        if (newAnimator != null && parentAnimator != null)
        {
            parentAnimator.runtimeAnimatorController = newAnimator.runtimeAnimatorController;
            parentAnimator.applyRootMotion = newAnimator.applyRootMotion;
        }
    }
}
