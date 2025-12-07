using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterPreviewRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public bool autoRotate = false; // Changed to false by default
    public float autoRotateSpeed = 20f;
    
    [Header("Manual Rotation (Touch/Mouse)")]
    public bool enableManualRotation = true;
    public float manualRotationSpeed = 0.3f;
    
    [Header("Rotation Area (Optional)")]
    public RectTransform rotationArea; // Drag a UI panel here to limit rotation area
    
    private bool isDragging = false;
    private float lastMouseX;
    private float currentRotation = 0f;
    
    void Update()
    {
        // Auto rotation when not being dragged
        if (autoRotate && !isDragging)
        {
            currentRotation += autoRotateSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
        
        // Manual rotation with mouse/touch
        if (enableManualRotation)
        {
            HandleManualRotation();
        }
    }
    
    void HandleManualRotation()
    {
        // Check if we should handle input
        bool canRotate = true;
        
        // If rotation area is set, check if pointer is inside it
        if (rotationArea != null)
        {
            canRotate = RectTransformUtility.RectangleContainsScreenPoint(
                rotationArea, 
                Input.mousePosition, 
                null
            );
        }
        
        if (!canRotate)
            return;
        
        // Mouse/Touch down
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMouseX = Input.mousePosition.x;
            autoRotate = false; // Stop auto rotation when user starts dragging
        }
        
        // Mouse/Touch drag
        if (Input.GetMouseButton(0) && isDragging)
        {
            float currentMouseX = Input.mousePosition.x;
            float deltaX = currentMouseX - lastMouseX;
            
            // Rotate based on horizontal drag
            currentRotation += deltaX * manualRotationSpeed;
            transform.rotation = Quaternion.Euler(0, currentRotation, 0);
            
            lastMouseX = currentMouseX;
        }
        
        // Mouse/Touch up
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            // Auto rotation removed - character stays at user's chosen angle
        }
    }
    
    void ResumeAutoRotation()
    {
        // This function is no longer called, but kept for manual use if needed
        autoRotate = true;
    }
    
    // Reset rotation to default
    public void ResetRotation()
    {
        currentRotation = 0f;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    
    // Stop auto rotation
    public void StopAutoRotation()
    {
        autoRotate = false;
    }
    
    // Start auto rotation
    public void StartAutoRotation()
    {
        autoRotate = true;
    }
    
    // Set rotation to specific angle
    public void SetRotation(float angle)
    {
        currentRotation = angle;
        transform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }
}