using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;             // Base speed of horizontal movement
    public float maxLaneDistance = 3f;         // Max distance from center
    public float forwardSpeed = 10f;           // Forward speed (used externally)

    [Header("Jump")]
    public float jumpForce = 9f;
    public float gravity = 20f;
    public float groundCheckDistance = 0.1f;
    public bool enableJump = true;

    [Header("Fast Descent (Swipe Down)")]
    public float fastDescentForce = 15f;
    public float fastDescentGravityMultiplier = 2.5f;

    [Header("Swipe Settings")]
    public float minSwipeDistance = 50f;        // Minimum swipe length (pixels)
    public float swipeSensitivity = 0.01f;       // How much swipe distance affects movement
    public float maxSwipeSpeed = 20f;            // Maximum speed from swiping
    public float returnToCenterSpeed = 5f;       // How fast input returns to zero when not swiping
    public float swipeDirectionThreshold = 0.5f;  // Ratio to determine if horizontal or vertical

    [Header("Visual Tilt")]
    public float tiltAngle = 20f;
    public float tiltSpeed = 10f;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator anim;

    private float horizontalInput;               // -1 to 1 for movement
    private float targetTilt;
    private bool isGrounded;
    private bool isFastDescending;
    private bool isMovementStopped = false;

    // Touch tracking for continuous swipe
    private int activeTouchId = -1;
    private Vector2 touchStartPos;
    private Vector2 lastTouchPos;
    private bool isSwiping = false;
    private bool gestureLocked = false;          // Locks whether this is horizontal or vertical swipe
    private bool isHorizontalSwipe = false;      // Which direction is locked
    private float lastSwipeTime;
    private float swipeCooldown = 0.1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotation
                       | RigidbodyConstraints.FreezePositionZ;
    }

    void Update()
    {
        CheckGrounded();
        HandleKeyboardInput();
        HandleTouchInput();
        MoveHorizontally();
        ApplyTilt();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        ApplyGravity();
    }

    #region Input Handling

    void HandleKeyboardInput()
    {
        if (isMovementStopped) return;

        // Continuous movement with arrow keys or A/D
        float h = 0f;
        float tilt = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            h = -1f;
            tilt = tiltAngle;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            h = 1f;
            tilt = -tiltAngle;
        }

        horizontalInput = h;
        targetTilt = tilt;

        // Jump
        if (enableJump && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
            Jump();

        // Fast descent
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            TryFastDescent();
    }

    void HandleTouchInput()
    {
        if (isMovementStopped) return;

        // Check all touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                // Start tracking this touch
                activeTouchId = touch.fingerId;
                touchStartPos = touch.position;
                lastTouchPos = touch.position;
                isSwiping = true;
                gestureLocked = false; // Not locked until we determine direction
            }
            else if (touch.fingerId == activeTouchId)
            {
                // This is our active touch
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    // Calculate total swipe delta from start
                    Vector2 totalDelta = touch.position - touchStartPos;
                    float absTotalX = Mathf.Abs(totalDelta.x);
                    float absTotalY = Mathf.Abs(totalDelta.y);

                    // Lock gesture direction if not already locked and we've moved enough
                    if (!gestureLocked && (absTotalX > minSwipeDistance * 0.5f || absTotalY > minSwipeDistance * 0.5f))
                    {
                        // Determine if this is primarily horizontal or vertical
                        isHorizontalSwipe = absTotalX > absTotalY * swipeDirectionThreshold;
                        gestureLocked = true;
                        
                        // If vertical, check if it's a quick gesture for jump/descent
                        if (!isHorizontalSwipe)
                        {
                            if (absTotalY > minSwipeDistance)
                            {
                                if (totalDelta.y > 0 && enableJump && Time.time - lastSwipeTime > swipeCooldown)
                                {
                                    Jump();
                                    lastSwipeTime = Time.time;
                                }
                                else if (totalDelta.y < 0 && Time.time - lastSwipeTime > swipeCooldown)
                                {
                                    TryFastDescent();
                                    lastSwipeTime = Time.time;
                                }
                            }
                        }
                    }

                    // Handle horizontal movement only if this is a horizontal swipe
                    if (gestureLocked && isHorizontalSwipe)
                    {
                        // Calculate input based on current position relative to start
                        // INCREASED SENSITIVITY: Apply 2x multiplier internally
                        float currentOffset = touch.position.x - touchStartPos.x;
                        float targetInput = Mathf.Clamp(currentOffset * swipeSensitivity * 25f, -1f, 1f);
                        
                        // Smoothly move toward target input - INCREASED from 10f to 20f for faster response
                        horizontalInput = Mathf.Lerp(horizontalInput, targetInput, Time.deltaTime * 50f);
                        
                        // Set tilt based on current input
                        targetTilt = -horizontalInput * tiltAngle;
                    }
                    
                    // Update last position
                    lastTouchPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    // Touch ended, return input to zero
                    isSwiping = false;
                    activeTouchId = -1;
                    gestureLocked = false;
                }
            }
        }

        // If no active touch, gradually return input to zero
        if (!isSwiping && activeTouchId == -1)
        {
            horizontalInput = Mathf.Lerp(horizontalInput, 0f, Time.deltaTime * returnToCenterSpeed);
            targetTilt = -horizontalInput * tiltAngle;
            
            // Snap to zero if very small
            if (Mathf.Abs(horizontalInput) < 0.01f)
                horizontalInput = 0f;
        }
    }

    void TryFastDescent()
    {
        if (isGrounded) return;

        isFastDescending = true;
        rb.AddForce(Vector3.down * fastDescentForce, ForceMode.VelocityChange);
    }

    #endregion

    #region Movement Logic

    void CheckGrounded()
    {
        float dist = col.bounds.extents.y + groundCheckDistance;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, dist);

        if (isGrounded)
            isFastDescending = false;
    }

    void MoveHorizontally()
    {
        if (isMovementStopped) return;

        Vector3 pos = transform.position;
        pos.x += horizontalInput * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -maxLaneDistance, maxLaneDistance);
        transform.position = pos;
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            float g = isFastDescending
                ? gravity * fastDescentGravityMultiplier
                : gravity;

            rb.AddForce(Vector3.down * g, ForceMode.Acceleration);
        }
    }

    void ApplyTilt()
    {
        Quaternion rot = Quaternion.Euler(0, 0, targetTilt);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rot,
            Time.deltaTime * tiltSpeed
        );
    }

    void Jump()
    {
        if (!isGrounded || !enableJump) return;

        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    #endregion

    #region Animation

    void UpdateAnimations()
    {
        if (!anim) return;

        anim.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0.1f);
        anim.SetBool("isJumping", !isGrounded);
        anim.SetBool("isFastDescending", isFastDescending);
        anim.SetBool("isIdle", isGrounded && Mathf.Abs(horizontalInput) < 0.1f);
    }

    #endregion

    #region Public Methods (for external control)

    public void SetForwardSpeed(float speed) => forwardSpeed = speed;
    public float GetForwardSpeed() => forwardSpeed;

    public void StopMovement()
    {
        isMovementStopped = true;
        forwardSpeed = 0f;
        horizontalInput = 0f;
        targetTilt = 0f;
        rb.velocity = Vector3.zero;
    }

    public void ResumeMovement() => isMovementStopped = false;

    public void EnableJump(bool enable) => enableJump = enable;
    public bool IsJumpEnabled() => enableJump;

    // For animator
    public float HorizontalInput => horizontalInput;
    public bool IsGrounded => isGrounded;
    public bool IsFastDescending => isFastDescending;

    #endregion
}
//working