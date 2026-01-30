using UnityEngine;

public class CostumeFunctionsManager : MonoBehaviour
{
    [Header("References")]
    public Animator childAnimator;        
    public PlayerControls playerControls;
    public PlayerFunctions playerFunctions; // 👈 i-frame source

    // Internal state
    private bool hasVibratedThisIFrame = false;

    void Awake()
    {
        if (!childAnimator)
            childAnimator = GetComponentInChildren<Animator>();

        if (!playerControls)
            playerControls = GetComponent<PlayerControls>();

        if (!playerFunctions)
            playerFunctions = GetComponent<PlayerFunctions>();
    }

    void Update()
    {
        if (!childAnimator || !playerControls) return;

        // ───────── ANIMATION LOGIC ─────────
        bool isRunning = Mathf.Abs(playerControls.HorizontalInput) > 0.1f;
        bool isJumping = !playerControls.IsGrounded && !playerControls.IsFastDescending;
        bool isFastDesc = playerControls.IsFastDescending;
        bool isIdle = playerControls.IsGrounded && !isRunning;

        childAnimator.SetBool("isRunning", isRunning);
        childAnimator.SetBool("isJumping", isJumping);
        childAnimator.SetBool("isFastDescending", isFastDesc);
        childAnimator.SetBool("isIdle", isIdle);

        // ───────── I-FRAME VIBRATION LOGIC ─────────
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
//testiing