using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;             // Base speed of horizontal movement (continuous mode)
    public float maxLaneDistance = 3f;         // Max distance from center
    public float forwardSpeed = 10f;           // Forward speed (used externally)

    [Header("Jump")]
    public float jumpHeight = 5f;               // Desired max height of the jump
    public float jumpTimeToPeak = 0.5f;          // Time to reach the peak (smaller = faster jump)
    public bool enableJump = true;                // Can the player jump?
    public float groundCheckDistance = 0.1f;      // How far to check for ground

    [Header("Jump Apex Gravity")]
    public bool enableApexGravityReduction = true;   // Enable/disable the effect
    public float apexGravityMultiplier = 0.5f;       // Gravity multiplier at apex (0.5 = half gravity)
    public float apexThreshold = 0.1f;                // Vertical speed threshold to consider "at apex"

    private float jumpForce;                     // Calculated from height & time
    private float gravity;                        // Calculated from height & time
    private bool isAtApex = false;                 // Track if we're at jump apex

    [Header("Fast Descent (Swipe Down)")]
    public float fastDescentForce = 15f;
    public float fastDescentGravityMultiplier = 2.5f;

    // --- NEW: Jump Suspension (Floating) ---
    [Header("Jump Suspension")]
    public bool enableJumpSuspension = true;          // Enable/disable suspension
    public float suspensionDuration = 2f;              // How long suspension lasts (seconds)
    public float suspensionGravityMultiplier = 0.1f;   // Very low gravity while suspended

    private bool isSuspended = false;                  // Currently suspended?
    private float suspensionTimer = 0f;                 // Time left in suspension

    [Header("Swipe Settings")]
    public float minSwipeDistance = 50f;        // Minimum swipe length (pixels)
    public float swipeSensitivity = 0.1f;       // How much swipe distance affects movement (continuous mode)
    public float maxSwipeSpeed = 20f;            // Maximum speed from swiping
    public float returnToCenterSpeed = 5f;       // How fast input returns to zero when not swiping
    public float swipeDirectionThreshold = 0.5f;  // Ratio to determine if horizontal or vertical
    public float swipeCooldown = 0f;              // <<< CHANGE THIS VALUE TO ADJUST COOLDOWN (0 = no cooldown)

    [Header("Visual Tilt & Yaw")]
    public float tiltAngle = 20f;                // Max roll angle when moving sideways
    public float maxYawAngle = 30f;               // Max yaw angle when moving sideways
    public float rotationSpeed = 10f;             // Smoothing speed for both yaw and tilt

    [Header("Lane Snapping (Subway Surfers style)")]
    public bool useLaneSnapping = false;         // Enable discrete lane switching
    public int laneCount = 3;                     // Number of lanes (e.g., 3 for left, center, right)
    public float laneSwitchSpeed = 10f;            // Speed of moving to target lane
    public float laneChangeTimeout = 1.0f;         // Max time allowed to complete a lane change

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator anim;

    private float horizontalInput;               // -1 to 1 for movement (continuous mode)
    private float targetTilt;
    private float targetYaw;                      // New: desired yaw angle for facing direction
    private bool isGrounded;
    private bool isFastDescending;
    private bool isMovementStopped = false;

    // Lane snapping variables
    private float[] lanePositions;
    private int currentLaneIndex;
    private int targetLaneIndex;
    private float targetLaneX;
    private bool isChangingLane = false;
    private float laneChangeStartTime;
    private Vector3 laneChangeStartPosition;

    // Touch tracking for continuous swipe
    private int activeTouchId = -1;
    private Vector2 touchStartPos;
    private Vector2 lastTouchPos;
    private Vector2 swipeTotalDelta;              // Total delta from start to current touch
    private bool isSwiping = false;
    private bool gestureLocked = false;          // Locks whether this is horizontal or vertical swipe
    private bool isHorizontalSwipe = false;      // Which direction is locked
    private float lastSwipeTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotation
                       | RigidbodyConstraints.FreezePositionZ;

        // Calculate jump parameters based on desired height and time
        RecalculateJumpPhysics();
    }

    void Start()
    {
        if (useLaneSnapping)
            InitializeLanes();
    }

    // Call this if you change jumpHeight or jumpTimeToPeak at runtime
    public void RecalculateJumpPhysics()
    {
        if (jumpTimeToPeak <= 0f) jumpTimeToPeak = 0.1f; // avoid division by zero
        gravity = 2f * jumpHeight / (jumpTimeToPeak * jumpTimeToPeak);
        jumpForce = gravity * jumpTimeToPeak;
    }

    void InitializeLanes()
    {
        if (laneCount < 1) laneCount = 1;
        lanePositions = new float[laneCount];
        if (laneCount == 1)
        {
            lanePositions[0] = 0f;
        }
        else
        {
            for (int i = 0; i < laneCount; i++)
            {
                // Map i from 0 to laneCount-1 to -maxLaneDistance to +maxLaneDistance
                float t = i / (float)(laneCount - 1);
                lanePositions[i] = Mathf.Lerp(-maxLaneDistance, maxLaneDistance, t);
            }
        }
        // Find nearest lane to current position
        float currentX = transform.position.x;
        int nearest = 0;
        float minDist = Mathf.Abs(currentX - lanePositions[0]);
        for (int i = 1; i < laneCount; i++)
        {
            float dist = Mathf.Abs(currentX - lanePositions[i]);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = i;
            }
        }
        currentLaneIndex = nearest;
        targetLaneIndex = currentLaneIndex;
        targetLaneX = lanePositions[currentLaneIndex];
        isChangingLane = false;
    }

    void Update()
    {
        CheckGrounded();
        HandleKeyboardInput();
        HandleTouchInput();
        MoveHorizontally();
        ApplyRotation();          // Now handles both yaw and tilt
        UpdateAnimations();

        // --- NEW: Update suspension timer ---
        if (isSuspended)
        {
            suspensionTimer -= Time.deltaTime;
            if (suspensionTimer <= 0f || isGrounded) // End when timer runs out or we land
            {
                isSuspended = false;
            }
        }
    }

    void FixedUpdate()
    {
        ApplyGravity();
    }

    #region Input Handling

    void HandleKeyboardInput()
    {
        if (isMovementStopped) return;

        if (useLaneSnapping)
        {
            // Discrete lane changes
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                ChangeLane(-1);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                ChangeLane(1);
        }
        else
        {
            // Continuous movement
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
        }

        // Jump (same for both modes)
        if (enableJump && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
            Jump();

        // Fast descent (same for both modes)
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
                swipeTotalDelta = Vector2.zero;
                isSwiping = true;
                gestureLocked = false;
            }
            else if (touch.fingerId == activeTouchId)
            {
                // This is our active touch
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    // Update total swipe delta
                    swipeTotalDelta = touch.position - touchStartPos;
                    float absTotalX = Mathf.Abs(swipeTotalDelta.x);
                    float absTotalY = Mathf.Abs(swipeTotalDelta.y);

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
                                if (swipeTotalDelta.y > 0 && enableJump && Time.time - lastSwipeTime > swipeCooldown)
                                {
                                    Jump();
                                    lastSwipeTime = Time.time;
                                }
                                else if (swipeTotalDelta.y < 0 && Time.time - lastSwipeTime > swipeCooldown)
                                {
                                    TryFastDescent();
                                    lastSwipeTime = Time.time;
                                }
                            }
                        }
                    }

                    // Handle horizontal movement (continuous mode only)
                    if (!useLaneSnapping && gestureLocked && isHorizontalSwipe)
                    {
                        // Calculate input based on current position relative to start
                        float currentOffset = touch.position.x - touchStartPos.x;
                        float targetInput = Mathf.Clamp(currentOffset * swipeSensitivity * 25f, -1f, 1f);
                        horizontalInput = Mathf.Lerp(horizontalInput, targetInput, Time.deltaTime * 100f);
                        targetTilt = -horizontalInput * tiltAngle;
                    }

                    lastTouchPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    // Touch ended
                    if (useLaneSnapping && gestureLocked && isHorizontalSwipe)
                    {
                        // Lane change based on total horizontal swipe
                        if (Mathf.Abs(swipeTotalDelta.x) > minSwipeDistance)
                        {
                            int direction = swipeTotalDelta.x > 0 ? 1 : -1;
                            ChangeLane(direction);
                        }
                    }

                    // Reset touch state
                    isSwiping = false;
                    activeTouchId = -1;
                    gestureLocked = false;
                }
            }
        }

        // If no active touch, gradually return input to zero (continuous mode only)
        if (!useLaneSnapping && !isSwiping && activeTouchId == -1)
        {
            horizontalInput = Mathf.Lerp(horizontalInput, 0f, Time.deltaTime * returnToCenterSpeed);
            targetTilt = -horizontalInput * tiltAngle;

            if (Mathf.Abs(horizontalInput) < 0.01f)
                horizontalInput = 0f;
        }
    }

    void ChangeLane(int direction)
    {
        int newIndex = currentLaneIndex + direction;
        if (newIndex >= 0 && newIndex < laneCount)
        {
            // If we're already changing lanes, check if this is a new direction
            if (isChangingLane)
            {
                // If trying to change to a different lane than current target, cancel current and start new
                if (newIndex != targetLaneIndex)
                {
                    CancelLaneChange();
                    StartLaneChange(newIndex);
                }
            }
            else
            {
                StartLaneChange(newIndex);
            }
        }
    }

    void StartLaneChange(int newLaneIndex)
    {
        targetLaneIndex = newLaneIndex;
        targetLaneX = lanePositions[targetLaneIndex];
        isChangingLane = true;
        laneChangeStartTime = Time.time;
        laneChangeStartPosition = transform.position;
    }

    void CheckLaneChangeProgress()
    {
        if (!isChangingLane) return;

        // Check if we've reached the target lane
        if (Mathf.Abs(transform.position.x - targetLaneX) < 0.01f)
        {
            CompleteLaneChange();
            return;
        }

        // Check for timeout
        float timeElapsed = Time.time - laneChangeStartTime;
        if (timeElapsed > laneChangeTimeout)
        {
            // Calculate how far we've moved toward the target lane
            float startToTarget = Mathf.Abs(targetLaneX - laneChangeStartPosition.x);
            float currentDistanceToTarget = Mathf.Abs(targetLaneX - transform.position.x);
            float distanceMoved = startToTarget - currentDistanceToTarget;
            
            // If we haven't made significant progress (less than 20% of the way), cancel the lane change
            if (startToTarget > 0.01f && distanceMoved < startToTarget * 0.2f)
            {
                Debug.Log("Lane change cancelled - blocked by obstacle");
                CancelLaneChange();
            }
            else if (timeElapsed > laneChangeTimeout * 2f) // Force cancel if taking too long
            {
                Debug.Log("Lane change cancelled - timeout");
                CancelLaneChange();
            }
        }
    }

    void CancelLaneChange()
    {
        if (isChangingLane)
        {
            // Revert to current lane
            targetLaneIndex = currentLaneIndex;
            targetLaneX = lanePositions[currentLaneIndex];
            isChangingLane = false;
        }
    }

    void CompleteLaneChange()
    {
        if (isChangingLane)
        {
            currentLaneIndex = targetLaneIndex;
            isChangingLane = false;
        }
    }

    void TryFastDescent()
    {
        if (isGrounded) return;

        isFastDescending = true;
        rb.AddForce(Vector3.down * fastDescentForce, ForceMode.VelocityChange);

        // --- NEW: Fast descent cancels suspension ---
        if (isSuspended)
            isSuspended = false;
    }

    #endregion

    #region Movement Logic

    void CheckGrounded()
    {
        float dist = col.bounds.extents.y + groundCheckDistance;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, dist);

        if (isGrounded)
        {
            isFastDescending = false;
            isAtApex = false; // Reset apex when grounded
            // --- NEW: Grounding cancels suspension ---
            if (isSuspended)
                isSuspended = false;
        }
    }

    void MoveHorizontally()
    {
        if (isMovementStopped) return;

        if (useLaneSnapping)
        {
            // Store previous position to check if we're actually moving
            Vector3 previousPosition = transform.position;
            
            // Move towards target lane
            Vector3 pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, targetLaneX, laneSwitchSpeed * Time.deltaTime);
            transform.position = pos;

            // Check if we're actually moving horizontally
            bool isMovingHorizontally = Mathf.Abs(transform.position.x - previousPosition.x) > 0.001f;

            // Update yaw based on lane change direction
            if (isChangingLane && isMovingHorizontally)
            {
                // Point towards the target lane
                float direction = Mathf.Sign(targetLaneX - transform.position.x);
                targetYaw = direction * maxYawAngle;
            }
            else
            {
                targetYaw = 0f;
            }

            // Update tilt based on movement direction
            float diff = targetLaneX - pos.x;
            if (Mathf.Abs(diff) > 0.001f && isMovingHorizontally)
            {
                // Moving left (diff < 0) -> tilt right (positive angle)
                targetTilt = (diff < 0) ? tiltAngle : -tiltAngle;
            }
            else
            {
                targetTilt = 0f;
            }

            // Check lane change progress (for timeout/blocking)
            if (isChangingLane)
            {
                if (!isMovingHorizontally && Mathf.Abs(transform.position.x - targetLaneX) > 0.1f)
                {
                    CheckLaneChangeProgress();
                }
                else
                {
                    CheckLaneChangeProgress();
                }
            }
        }
        else
        {
            // Continuous mode: move horizontally
            Vector3 pos = transform.position;
            pos.x += horizontalInput * moveSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -maxLaneDistance, maxLaneDistance);
            transform.position = pos;

            // Set yaw based on horizontal input
            targetYaw = horizontalInput * maxYawAngle;
            // Tilt is already set in input handling
        }
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            // Check if we're at the apex of a jump
            if (enableApexGravityReduction)
            {
                // Apex is when vertical velocity is near zero and we're not fast descending
                if (Mathf.Abs(rb.velocity.y) < apexThreshold && !isFastDescending && !isSuspended) // Suspended overrides apex
                {
                    if (!isAtApex)
                    {
                        isAtApex = true;   // Enter apex state
                    }
                }
                else
                {
                    isAtApex = false;      // Exit apex state
                }
            }

            // Determine gravity multiplier (order: fast descent > suspension > apex > normal)
            float gravityMultiplier = 1f;
            if (isFastDescending)
                gravityMultiplier = fastDescentGravityMultiplier;
            else if (isSuspended)
                gravityMultiplier = suspensionGravityMultiplier;   // Very low gravity
            else if (isAtApex)
                gravityMultiplier = apexGravityMultiplier;

            float g = gravity * gravityMultiplier;

            rb.AddForce(Vector3.down * g, ForceMode.Acceleration);
        }
    }

    void ApplyRotation()
    {
        // Determine the forward direction based on target yaw
        // We assume forward is along world Z, and yaw rotates around world Y
        Vector3 forwardDir = Quaternion.Euler(0f, targetYaw, 0f) * Vector3.forward;

        // If forwardDir is zero (shouldn't happen), default to world forward
        if (forwardDir == Vector3.zero)
            forwardDir = Vector3.forward;

        // Build the target rotation: first face forwardDir, then apply local tilt
        Quaternion targetRotation = Quaternion.LookRotation(forwardDir) * Quaternion.Euler(0f, 0f, targetTilt);

        // Smoothly rotate towards the target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    void Jump()
    {
        if (!isGrounded || !enableJump) return;

        isAtApex = false; // Reset apex state for new jump
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

        // --- NEW: Activate suspension on jump ---
        if (enableJumpSuspension)
        {
            isSuspended = true;
            suspensionTimer = suspensionDuration;
        }
    }

    #endregion

    #region Animation

    void UpdateAnimations()
    {
        if (!anim) return;

        if (useLaneSnapping)
        {
            anim.SetBool("isRunning", forwardSpeed > 0);
        }
        else
        {
            anim.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0.1f);
        }

        anim.SetBool("isJumping", !isGrounded);
        anim.SetBool("isFastDescending", isFastDescending);
        anim.SetBool("isIdle", isGrounded && Mathf.Abs(horizontalInput) < 0.1f);
        anim.SetBool("isChangingLane", isChangingLane);
        anim.SetBool("isAtApex", isAtApex); // Optional: add to animator for special apex animations
        // --- NEW: Suspension animation parameter ---
        anim.SetBool("isSuspended", isSuspended);
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
        targetYaw = 0f;
        rb.velocity = Vector3.zero;
    }

    public void ResumeMovement() => isMovementStopped = false;

    public void EnableJump(bool enable) => enableJump = enable;
    public bool IsJumpEnabled() => enableJump;

    // For animator
    public float HorizontalInput => horizontalInput;
    public bool IsGrounded => isGrounded;
    public bool IsFastDescending => isFastDescending;
    public bool IsChangingLane => isChangingLane;
    public int CurrentLane => currentLaneIndex;
    public int TargetLane => targetLaneIndex;
    public bool IsAtApex => isAtApex;
    // --- NEW: Public getter for suspension ---
    public bool IsSuspended => isSuspended;

    #endregion
}