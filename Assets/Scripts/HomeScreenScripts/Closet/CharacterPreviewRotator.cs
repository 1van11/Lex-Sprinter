using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class CharacterPreviewRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public bool autoRotate = false;
    public float autoRotateSpeed = 20f;

    [Header("Manual Rotation (Touch/Mouse)")]
    public bool enableManualRotation = true;
    public float manualRotationSpeed = 0.3f;

    [Header("Default Position")]
    public float defaultRotationY = 0f; // Set this in Inspector to any angle you want!

    [Header("Rotation Area (Optional)")]
    public RectTransform rotationArea;

    private bool isDragging = false;
    private float lastMouseX;
    private float currentRotation = 0f;

    void Awake()
    {
        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Always unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Fires every time a new scene loads
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetToDefault();
    }

    // Fires every time this GameObject/Panel is enabled
    void OnEnable()
    {
        ResetToDefault();
    }

    void ResetToDefault()
    {
        currentRotation = defaultRotationY;
        transform.rotation = Quaternion.Euler(0, defaultRotationY, 0);
    }

    void Update()
    {
        if (autoRotate && !isDragging)
        {
            currentRotation += autoRotateSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, currentRotation, 0);
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
            transform.rotation = Quaternion.Euler(0, currentRotation, 0);
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
        transform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }
}