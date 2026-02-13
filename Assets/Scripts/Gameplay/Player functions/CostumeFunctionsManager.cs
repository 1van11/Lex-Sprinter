using UnityEngine;

public class CostumeFunctionsManager : MonoBehaviour
{
    [Header("References")]
    public Animator childAnimator;           // The costume's animator (attached here)
    public Animator sourceAnimator;          // 👈 NEW: The original player animator to copy from
    public PlayerControls playerControls;
    public PlayerFunctions playerFunctions;

    // Internal state
    private bool hasVibratedThisIFrame = false;

    void Awake()
    {
        if (!childAnimator)
            childAnimator = GetComponentInChildren<Animator>();
        
        if (!playerControls)
            playerControls = FindObjectOfType<PlayerControls>();
        
        if (!playerFunctions)
            playerFunctions = FindObjectOfType<PlayerFunctions>();

        // 👇 Find the source animator from the main player object
        if (!sourceAnimator && playerControls != null)
            sourceAnimator = playerControls.GetComponent<Animator>();
    }

    void Update()
    {
        if (!childAnimator) return;

        // ───────── ANIMATION LOGIC - Copy from source animator ───────────
        if (sourceAnimator != null)
        {
            // Copy animation states directly from the source animator
            childAnimator.SetBool("isRunning", sourceAnimator.GetBool("isRunning"));
            childAnimator.SetBool("isJumping", sourceAnimator.GetBool("isJumping"));
            childAnimator.SetBool("isFastDescending", sourceAnimator.GetBool("isFastDescending"));
            childAnimator.SetBool("isIdle", sourceAnimator.GetBool("isIdle"));
        }
        else if (playerControls != null)
        {
            // Fallback: Calculate from PlayerControls if no source animator
            bool isRunning = Mathf.Abs(playerControls.HorizontalInput) > 0.1f;
            bool isJumping = !playerControls.IsGrounded && !playerControls.IsFastDescending;
            bool isFastDesc = playerControls.IsFastDescending;
            bool isIdle = playerControls.IsGrounded && !isRunning;

            childAnimator.SetBool("isRunning", isRunning);
            childAnimator.SetBool("isJumping", isJumping);
            childAnimator.SetBool("isFastDescending", isFastDesc);
            childAnimator.SetBool("isIdle", isIdle);
        }

        // ───────── I-FRAME VIBRATION LOGIC ───────────
        if (playerFunctions == null) return;

        // Vibrate ONCE when i-frames start
        if (playerFunctions.isInvincible && !hasVibratedThisIFrame)
        {
            Vibrate();
            hasVibratedThisIFrame = true;
        }

        // Reset when i-frames end
        if (!playerFunctions.isInvincible)
        {
            hasVibratedThisIFrame = false;
        }
    }

    void Vibrate()
    {
        #if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        #endif
    }
}