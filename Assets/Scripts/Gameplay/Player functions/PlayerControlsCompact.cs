using UnityEngine;

public class PlayerControlsCompact : MonoBehaviour
{
    [Header("Animation Parameters")]
    public string runParam    = "isRunning";
    public string jumpParam   = "isJumping";
    public string idleParam   = "isIdle";
    public string fastDescendParam = "isFastDescending";

    [Header("Mobile Controls")]
    public float swipeThreshold = 70f;

    private Animator animator;

    // Current frame input state
    private float horizontal = 0f;
    private bool wantsToJumpThisFrame   = false;
    private bool fastFallPressed        = false;

    // Touch tracking
    private Vector2 touchStartPosition;
    private bool touchBeganThisFrame = false;

    // Reference to main PlayerControls script
    public PlayerControls mainControls;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning("PlayerControlsCompact → No Animator found on " + gameObject.name);
            enabled = false;
        }

        if (mainControls == null)
            Debug.LogWarning("PlayerControlsCompact → Assign the main PlayerControls script in inspector!");
    }

    void Update()
    {
        ResetFrameInput();

        ReadPCInput();
        ReadMobileInput();

        UpdateAnimationState();
    }

    private void ResetFrameInput()
    {
        horizontal = 0f;
        wantsToJumpThisFrame = false;
        fastFallPressed = false;
        touchBeganThisFrame = false;
    }

    private void ReadPCInput()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal = 1f;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            wantsToJumpThisFrame = true;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            fastFallPressed = true;
    }

    private void ReadMobileInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            touchStartPosition = touch.position;
            touchBeganThisFrame = true;
        }

        if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
            Vector2 delta = touch.position - touchStartPosition;

            if (Mathf.Abs(delta.x) > swipeThreshold)
                horizontal = Mathf.Sign(delta.x);

            if (delta.y < -swipeThreshold)
                fastFallPressed = true;
        }

        if (touch.phase == TouchPhase.Ended && touchBeganThisFrame)
        {
            Vector2 delta = touch.position - touchStartPosition;
            if (delta.magnitude < swipeThreshold * 0.8f)
                wantsToJumpThisFrame = true;
        }
    }

    private void UpdateAnimationState()
    {
        if (animator == null) return;

        bool isMovingHorizontally = Mathf.Abs(horizontal) > 0.01f;

        // Mirror jump state from main PlayerControls script
        bool isJumping = mainControls != null ? mainControls.IsGrounded() == false : false;

        animator.SetBool(jumpParam, isJumping);
        animator.SetBool(runParam, isMovingHorizontally);
        animator.SetBool(idleParam, !isJumping && !isMovingHorizontally);
        animator.SetBool(fastDescendParam, fastFallPressed && isJumping);
    }

    // ────────── Public Interface ──────────

    public float GetHorizontalInput() => horizontal;
    public bool GetJumpInputThisFrame() => wantsToJumpThisFrame;
    public bool GetFastFallInput() => fastFallPressed;
}
//working