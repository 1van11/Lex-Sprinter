using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float maxLaneDistance = 3f;
    public float forwardSpeed = 10f;

    [Header("Jump")]
    public float jumpHeight = 5f;
    public float jumpTimeToPeak = 0.5f;
    public bool enableJump = true;
    public float groundCheckDistance = 0.1f;

    [Header("Jump Apex Gravity")]
    public bool enableApexGravityReduction = true;
    public float apexGravityMultiplier = 0.5f;
    public float apexThreshold = 0.1f;

    private float jumpForce;
    private float gravity;
    private bool isAtApex = false;

    [Header("Fast Descent (Swipe Down)")]
    public float fastDescentForce = 15f;
    public float fastDescentGravityMultiplier = 2.5f;

    [Header("Jump Suspension")]
    public bool enableJumpSuspension = true;
    public float suspensionDuration = 2f;
    public float suspensionGravityMultiplier = 0.1f;

    private bool isSuspended = false;
    private float suspensionTimer = 0f;

    [Header("Visual Tilt & Yaw")]
    public float tiltAngle = 20f;
    public float maxYawAngle = 30f;
    public float rotationSpeed = 10f;

    [Header("Lane Snapping (Subway Surfers style)")]
    public bool useLaneSnapping = false;
    public int laneCount = 3;
    public float laneSwitchSpeed = 10f;
    public float laneChangeTimeout = 1.0f;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator anim;

    private float horizontalInput;
    private float targetTilt;
    private float targetYaw;
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

    // Touch tracking
    private int activeTouchId = -1;
    private Vector2 touchStartPos;
    private Vector2 swipeTotalDelta;
    private bool isSwiping = false;
    private float lastSwipeTime;

    // Hardcoded swipe constants
    private const float MinSwipeDistance = 50f;
    private const float SwipeSensitivity = 0.1f;
    private const float ReturnToCenterSpeed = 5f;
    private const float SwipeDirectionThreshold = 0.5f;
    private const float SwipeCooldown = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotation
                       | RigidbodyConstraints.FreezePositionZ;

        RecalculateJumpPhysics();
    }

    void Start()
    {
        if (useLaneSnapping)
            InitializeLanes();
    }

    public void RecalculateJumpPhysics()
    {
        if (jumpTimeToPeak <= 0f) jumpTimeToPeak = 0.1f;
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
                float t = i / (float)(laneCount - 1);
                lanePositions[i] = Mathf.Lerp(-maxLaneDistance, maxLaneDistance, t);
            }
        }

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
        ApplyRotation();
        UpdateAnimations();

        if (isSuspended)
        {
            suspensionTimer -= Time.deltaTime;
            if (suspensionTimer <= 0f || isGrounded)
                isSuspended = false;
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
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                ChangeLane(-1);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                ChangeLane(1);
        }
        else
        {
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

        if (enableJump && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
            Jump();

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            TryFastDescent();
    }

    void HandleTouchInput()
    {
        if (isMovementStopped) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                activeTouchId = touch.fingerId;
                touchStartPos = touch.position;
                swipeTotalDelta = Vector2.zero;
                isSwiping = true;
            }
            else if (touch.fingerId == activeTouchId)
            {
                swipeTotalDelta = touch.position - touchStartPos;

                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    if (!useLaneSnapping)
                    {
                        // Continuous horizontal movement based on X offset
                        float targetInput = Mathf.Clamp(swipeTotalDelta.x * SwipeSensitivity * 25f, -1f, 1f);
                        horizontalInput = Mathf.Lerp(horizontalInput, targetInput, Time.deltaTime * 100f);
                        targetTilt = -horizontalInput * tiltAngle;
                    }
                    // In lane snapping mode, we do nothing during the swipe – we'll decide at the end
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    // End of touch: evaluate the swipe
                    float absX = Mathf.Abs(swipeTotalDelta.x);
                    float absY = Mathf.Abs(swipeTotalDelta.y);
                    float totalMagnitude = swipeTotalDelta.magnitude;

                    if (totalMagnitude > MinSwipeDistance)
                    {
                        if (useLaneSnapping)
                        {
                            // Lane snapping: decide between horizontal lane change or vertical action
                            if (absX > absY * SwipeDirectionThreshold)
                            {
                                // Horizontal swipe
                                int direction = swipeTotalDelta.x > 0 ? 1 : -1;
                                ChangeLane(direction);
                            }
                            else if (absY > absX * SwipeDirectionThreshold)
                            {
                                // Vertical swipe
                                if (swipeTotalDelta.y > 0 && enableJump && Time.time - lastSwipeTime > SwipeCooldown)
                                {
                                    Jump();
                                    lastSwipeTime = Time.time;
                                }
                                else if (swipeTotalDelta.y < 0 && Time.time - lastSwipeTime > SwipeCooldown)
                                {
                                    TryFastDescent();
                                    lastSwipeTime = Time.time;
                                }
                            }
                        }
                        else
                        {
                            // Free movement: vertical swipe is only considered if vertical movement dominates
                            if (absY > absX * SwipeDirectionThreshold)
                            {
                                if (swipeTotalDelta.y > 0 && enableJump && Time.time - lastSwipeTime > SwipeCooldown)
                                {
                                    Jump();
                                    lastSwipeTime = Time.time;
                                }
                                else if (swipeTotalDelta.y < 0 && Time.time - lastSwipeTime > SwipeCooldown)
                                {
                                    TryFastDescent();
                                    lastSwipeTime = Time.time;
                                }
                            }
                            // If horizontal dominated, we've already updated horizontalInput during the swipe, so nothing else to do
                        }
                    }

                    // Reset touch state
                    isSwiping = false;
                    activeTouchId = -1;
                }
            }
        }

        // For free movement: when no touch, gradually return horizontal input to zero
        if (!useLaneSnapping && !isSwiping && activeTouchId == -1)
        {
            horizontalInput = Mathf.Lerp(horizontalInput, 0f, Time.deltaTime * ReturnToCenterSpeed);
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
            if (isChangingLane)
            {
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

        if (Mathf.Abs(transform.position.x - targetLaneX) < 0.01f)
        {
            CompleteLaneChange();
            return;
        }

        float timeElapsed = Time.time - laneChangeStartTime;
        if (timeElapsed > laneChangeTimeout)
        {
            float startToTarget = Mathf.Abs(targetLaneX - laneChangeStartPosition.x);
            float currentDistanceToTarget = Mathf.Abs(targetLaneX - transform.position.x);
            float distanceMoved = startToTarget - currentDistanceToTarget;

            if (startToTarget > 0.01f && distanceMoved < startToTarget * 0.2f)
            {
                Debug.Log("Lane change cancelled - blocked by obstacle");
                CancelLaneChange();
            }
            else if (timeElapsed > laneChangeTimeout * 2f)
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
            isAtApex = false;
            if (isSuspended)
                isSuspended = false;
        }
    }

    void MoveHorizontally()
    {
        if (isMovementStopped) return;

        if (useLaneSnapping)
        {
            Vector3 previousPosition = transform.position;

            Vector3 pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, targetLaneX, laneSwitchSpeed * Time.deltaTime);
            transform.position = pos;

            bool isMovingHorizontally = Mathf.Abs(transform.position.x - previousPosition.x) > 0.001f;

            if (isChangingLane && isMovingHorizontally)
            {
                float direction = Mathf.Sign(targetLaneX - transform.position.x);
                targetYaw = direction * maxYawAngle;
            }
            else
            {
                targetYaw = 0f;
            }

            float diff = targetLaneX - pos.x;
            if (Mathf.Abs(diff) > 0.001f && isMovingHorizontally)
                targetTilt = (diff < 0) ? tiltAngle : -tiltAngle;
            else
                targetTilt = 0f;

            if (isChangingLane)
                CheckLaneChangeProgress();
        }
        else
        {
            Vector3 pos = transform.position;
            pos.x += horizontalInput * moveSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -maxLaneDistance, maxLaneDistance);
            transform.position = pos;

            targetYaw = horizontalInput * maxYawAngle;
        }
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            if (enableApexGravityReduction)
            {
                if (Mathf.Abs(rb.velocity.y) < apexThreshold && !isFastDescending && !isSuspended)
                {
                    if (!isAtApex) isAtApex = true;
                }
                else
                {
                    isAtApex = false;
                }
            }

            float gravityMultiplier = 1f;
            if (isFastDescending)
                gravityMultiplier = fastDescentGravityMultiplier;
            else if (isSuspended)
                gravityMultiplier = suspensionGravityMultiplier;
            else if (isAtApex)
                gravityMultiplier = apexGravityMultiplier;

            rb.AddForce(Vector3.down * gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    void ApplyRotation()
    {
        Vector3 forwardDir = Quaternion.Euler(0f, targetYaw, 0f) * Vector3.forward;
        if (forwardDir == Vector3.zero) forwardDir = Vector3.forward;

        Quaternion targetRotation = Quaternion.LookRotation(forwardDir) * Quaternion.Euler(0f, 0f, targetTilt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    void Jump()
    {
        if (!isGrounded || !enableJump) return;

        isAtApex = false;
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

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
            anim.SetBool("isRunning", forwardSpeed > 0);
        else
            anim.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0.1f);

        anim.SetBool("isJumping", !isGrounded);
        anim.SetBool("isFastDescending", isFastDescending);
        anim.SetBool("isIdle", isGrounded && Mathf.Abs(horizontalInput) < 0.1f);
        anim.SetBool("isChangingLane", isChangingLane);
        anim.SetBool("isAtApex", isAtApex);
        anim.SetBool("isSuspended", isSuspended);
    }

    #endregion

    #region Public Methods

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

    public float HorizontalInput => horizontalInput;
    public bool IsGrounded => isGrounded;
    public bool IsFastDescending => isFastDescending;
    public bool IsChangingLane => isChangingLane;
    public int CurrentLane => currentLaneIndex;
    public int TargetLane => targetLaneIndex;
    public bool IsAtApex => isAtApex;
    public bool IsSuspended => isSuspended;

    #endregion
}