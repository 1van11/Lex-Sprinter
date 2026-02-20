using UnityEngine;

public class CharacterBreathing : MonoBehaviour
{
    [Header("Breathing Settings")]
    [Tooltip("How much to scale (0.02 = 2% bigger)")]
    public float breathAmount = 0.02f;
    
    [Tooltip("How fast to breathe (0.5 = slow, 2 = fast)")]
    public float breathSpeed = 0.8f;
    
    private Vector3 originalScale;
    
    void Start()
    {
        // Save original size
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        // Calculate breathing using sine wave (smooth in/out)
        float breathe = Mathf.Sin(Time.time * breathSpeed) * breathAmount;
        
        // Apply to scale (very subtle growth/shrink)
        transform.localScale = originalScale + new Vector3(breathe, breathe, breathe);
    }
}