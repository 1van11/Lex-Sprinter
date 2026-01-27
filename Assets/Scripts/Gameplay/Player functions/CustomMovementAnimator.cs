using UnityEngine;

public class CustomMovementAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator childAnimator;        // Child costume
    public PlayerControls playerControls; // Reference to parent movement

    void Awake()
    {
        if (!childAnimator)
            childAnimator = GetComponentInChildren<Animator>();

        if (!playerControls)
            playerControls = GetComponent<PlayerControls>();
    }

    void Update()
    {
        if (!childAnimator || !playerControls) return;

        // Animate child based on movement state
        bool isRunning = Mathf.Abs(playerControls.HorizontalInput) > 0.1f;
        bool isJumping = !playerControls.IsGrounded && !playerControls.IsFastDescending;
        bool isFastDesc = playerControls.IsFastDescending;
        bool isIdle = playerControls.IsGrounded && !isRunning;

        childAnimator.SetBool("isRunning", isRunning);
        childAnimator.SetBool("isJumping", isJumping);
        childAnimator.SetBool("isFastDescending", isFastDesc);
        childAnimator.SetBool("isIdle", isIdle);
    }
}
