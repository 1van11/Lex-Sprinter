using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterPreviewRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public bool autoRotate = false;
    public float autoRotateSpeed = 20f;

    [Header("Manual Rotation (Touch/Mouse)")]
    public bool enableManualRotation = true;
    public float manualRotationSpeed = 0.3f;

    [Header("Default Position")]
    public float defaultRotationY = 180f;

    [Header("Rotation Area (Optional)")]
    public RectTransform rotationArea;

    private bool isDragging = false;
    private float lastMouseX;
    private float currentRotation = 0f;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetToDefault();
    }

    void OnEnable()
    {
        ResetToDefault();
    }

    void ResetToDefault()
    {
        currentRotation = defaultRotationY;
        // ✅ FIXED: localRotation keeps character in place
        transform.localRotation = Quaternion.Euler(0, defaultRotationY, 0);
    }

    void Update()
    {
        if (autoRotate && !isDragging)
        {
            currentRotation += autoRotateSpeed * Time.deltaTime;
            // ✅ FIXED: localRotation
            transform.localRotation = Quaternion.Euler(0, currentRotation, 0);
        }

        if (enableManualRotation)
        {
            HandleManualRotation();
        }
    }

    void HandleManualRotation()
    {
        bool canRotate = true;

        if (rotationArea != null)
        {
            canRotate = RectTransformUtility.RectangleContainsScreenPoint(
                rotationArea,
                Input.mousePosition,
                null
            );
        }

        if (!canRotate) return;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMouseX = Input.mousePosition.x;
            autoRotate = false;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            float currentMouseX = Input.mousePosition.x;
            float deltaX = currentMouseX - lastMouseX;
            currentRotation += deltaX * manualRotationSpeed;
            // ✅ FIXED: localRotation
            transform.localRotation = Quaternion.Euler(0, currentRotation, 0);
            lastMouseX = currentMouseX;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    public void ResetRotation() => ResetToDefault();
    public void StopAutoRotation() => autoRotate = false;
    public void StartAutoRotation() => autoRotate = true;

    public void SetRotation(float angle)
    {
        currentRotation = angle;
        // ✅ FIXED: localRotation
        transform.localRotation = Quaternion.Euler(0, currentRotation, 0);
    }
}