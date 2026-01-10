using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControls : MonoBehaviour
{
    [Header("Horizontal Movement")]
    public float laneDistance = 3f;
    public float horizontalMoveSpeed = 10f;
    private float horizontalInput = 0f;

    [Header("Jump")]
    public float jumpForce = 9f;
    public float extraFallForce = 10f;
    public float jumpCooldown = 0.25f;
    private float lastJumpTime;

    [Header("Jump Assist")]
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    private float lastGroundedTime;
    private float lastJumpPressedTime;

    [Header("Jump Smash")]
    public float smashDownForce = 20f;
    private bool isSmashing = false;

    [Header("Rotation")]
    public float tiltAngle = 20f;
    public float tiltSpeed = 10f;
    public float lookAngle = 25f;

    [Header("Ground Detection")]
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("Scene Animation Settings")]
    public string homeSceneName = "HomeScreen";

    private Animator anim;
    private Rigidbody rb;
    private CapsuleCollider col;

    private bool isJumping = false;
    private bool jumpHeld = false;

    private float inputCooldown = 0.2f;
    private float lastInputTime;

    private float targetTilt = 0f;
    private float targetYaw = 0f;

    // Swipe Controls
    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private bool swipeDetected = false;
    private float swipeThreshold = 50f;

    [HideInInspector] public bool canMove = true;

    // Static reference to ensure only one PlayerControls is active
    private static PlayerControls activeInstance;
    private bool isActiveInstance = false;

    // Reference to PlayerFunctions for forward speed
    private PlayerFunctions playerFunctions;

    // Track current scene
    private string currentSceneName;

    void Start()
    {
        // Check if there's already an active instance
        if (activeInstance == null)
        {
            // This is the first/active instance
            activeInstance = this;
            isActiveInstance = true;
            InitializeComponents();
        }
        else
        {
            // There's already an active instance, disable this one
            isActiveInstance = false;
            Debug.Log($"⚠️ Disabling duplicate PlayerControls on {gameObject.name}");
            
            // Disable this script but keep the GameObject active
            this.enabled = false;
            return;
        }

        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Set initial scene and animation state
        currentSceneName = SceneManager.GetActiveScene().name;
        UpdateAnimationBasedOnScene();
    }

    void InitializeComponents()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        
        // Try to get PlayerFunctions from this GameObject or parent
        playerFunctions = GetComponent<PlayerFunctions>();
        if (playerFunctions == null)
        {
            playerFunctions = GetComponentInParent<PlayerFunctions>();
        }
        if (playerFunctions == null)
        {
            playerFunctions = FindObjectOfType<PlayerFunctions>();
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        }
        else
        {
            Debug.LogWarning("❌ Rigidbody not found on PlayerControls GameObject");
        }

        Debug.Log($"✅ Active PlayerControls initialized on {gameObject.name}");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        UpdateAnimationBasedOnScene();
    }

    void UpdateAnimationBasedOnScene()
    {
        if (anim == null) return;

        bool isHomeScreen = currentSceneName == homeSceneName;

        if (isHomeScreen)
        {
            // In HomeScreen: trigger isIdle, disable isRunning
            anim.SetBool("isIdle", true);
            anim.SetBool("isRunning", false);
            Debug.Log("🏠 HomeScreen detected - Setting isIdle to true");
        }
        else
        {
            // Not in HomeScreen: trigger isRunning, disable isIdle
            anim.SetBool("isIdle", false);
            anim.SetBool("isRunning", true);
            Debug.Log($"🏃 Scene '{currentSceneName}' detected - Setting isRunning to true");
        }
    }

    void Update()
    {
        // Only the active instance should process input
        if (!isActiveInstance || !canMove) return;

        // Update grounded time FIRST
        bool grounded = IsGrounded();
        if (grounded)
            lastGroundedTime = Time.time;

        HandleLaneInput();
        MoveHorizontal();
        HandleJump();
        HandleTiltAndLook();
        ApplyExtraGravity();
        DetectSwipe();

        // Update animation based on current state
        UpdateJumpAnimation();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            lastJumpPressedTime = Time.time;
    }

    void OnDestroy()
    {
        // Unsubscribe from scene events
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // If this was the active instance, clear the reference
        if (isActiveInstance)
        {
            activeInstance = null;
        }
    }

    // -----------------------
    // Horizontal Free Movement
    // -----------------------
    void HandleLaneInput()
    {
        if (Time.time - lastInputTime < inputCooldown) return;

        horizontalInput = 0f;

        // Keyboard
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput = -1f;
            targetTilt = tiltAngle;
            targetYaw = -lookAngle;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontalInput = 1f;
            targetTilt = -tiltAngle;
            targetYaw = lookAngle;
        }
    }

    void MoveHorizontal()
    {
        if (transform == null) return;

        Vector3 pos = transform.position;

        // Move left/right
        pos.x += horizontalInput * horizontalMoveSpeed * Time.deltaTime;

        // Clamp to old 3-lane range
        pos.x = Mathf.Clamp(pos.x, -laneDistance, laneDistance);

        transform.position = pos;

        bool atLeftEdge = pos.x <= -laneDistance + 0.01f;
        bool atRightEdge = pos.x >= laneDistance - 0.01f;

        // If at edge AND input pushes further → cancel tilt
        if ((atLeftEdge && horizontalInput < 0) || (atRightEdge && horizontalInput > 0))
        {
            horizontalInput = 0;
            targetTilt = 0f;
            targetYaw = 0f;
        }

        // Reset rotation when no input
        if (horizontalInput == 0)
        {
            targetTilt = 0f;
            targetYaw = 0f;
        }
    }

    // -----------------------
    // Jump System (FIXED)
    // -----------------------
    void HandleJump()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            lastJumpPressedTime = Time.time;
            jumpHeld = true;
        }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow))
            jumpHeld = false;

        bool canJump =
            Time.time - lastGroundedTime <= coyoteTime &&
            Time.time - lastJumpPressedTime <= jumpBufferTime &&
            Time.time - lastJumpTime >= jumpCooldown &&
            !isJumping &&
            jumpHeld;

        if (canJump)
        {
            if (rb != null)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            lastJumpTime = Time.time;
            isJumping = true;
            jumpHeld = false;
            lastJumpPressedTime = -999f;
            
            // Update animation immediately when jumping
            if (anim != null)
                anim.SetBool("isJumping", true);
        }

        // Smash
        if (!IsGrounded() && !isSmashing && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            isSmashing = true;
            if (rb != null)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.down * smashDownForce, ForceMode.Impulse);
            }
        }

        // Reset when grounded AND velocity is low
        if (IsGrounded() && (rb == null || rb.velocity.y <= 0.1f))
        {
            if (isJumping || isSmashing)
            {
                isJumping = false;
                isSmashing = false;
                
                // Update animation immediately when landing
                if (anim != null)
                    anim.SetBool("isJumping", false);
            }
        }
    }

    void UpdateJumpAnimation()
    {
        if (anim == null) return;
        
        // Set based on actual grounded state, not just the flag
        bool shouldBeJumping = !IsGrounded() || (rb != null && rb.velocity.y > 0.1f);
        anim.SetBool("isJumping", shouldBeJumping);
    }

    void ApplyExtraGravity()
    {
        if (!IsGrounded() && rb != null && rb.velocity.y < 0)
        {
            float gravityMultiplier = isSmashing ? 2f : 1f;
            rb.AddForce(Vector3.down * extraFallForce * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    // -----------------------
    // Tilt & Rotation
    // -----------------------
    void HandleTiltAndLook()
    {
        Quaternion targetRotation = Quaternion.Euler(0, targetYaw, targetTilt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * tiltSpeed);

        if (horizontalInput == 0)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, Time.deltaTime * tiltSpeed);
    }

    // -----------------------
    // Swipe
    // -----------------------
    void DetectSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPos = touch.position;
                    swipeDetected = true;
                    break;

                case TouchPhase.Ended:
                    if (!swipeDetected) return;

                    endTouchPos = touch.position;
                    Vector2 swipeDelta = endTouchPos - startTouchPos;

                    if (swipeDelta.magnitude < swipeThreshold) return;

                    float x = swipeDelta.x;
                    float y = swipeDelta.y;

                    if (Mathf.Abs(x) > Mathf.Abs(y))
                    {
                        if (x > 0)
                        {
                            horizontalInput = 1;
                            targetTilt = -tiltAngle;
                            targetYaw = lookAngle;
                        }
                        else
                        {
                            horizontalInput = -1;
                            targetTilt = tiltAngle;
                            targetYaw = -lookAngle;
                        }
                    }
                    else
                    {
                        if (y > 0)
                        {
                            lastJumpPressedTime = Time.time;
                            jumpHeld = true;
                        }
                        else
                        {
                            if (!IsGrounded() && !isSmashing)
                            {
                                isSmashing = true;
                                if (rb != null)
                                {
                                    rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                                    rb.AddForce(Vector3.down * smashDownForce, ForceMode.Impulse);
                                }
                            }
                        }
                    }

                    swipeDetected = false;
                    break;
            }
        }
    }

    // -----------------------
    // Helpers (IMPROVED)
    // -----------------------
    public bool IsGrounded()
    {
        if (col == null)
        {
            // Try to get the collider if it's null
            col = GetComponent<CapsuleCollider>();
            if (col == null) return false;
        }

        // Use a slightly longer raycast for better detection
        float checkDistance = col.bounds.extents.y + groundCheckDistance;
        
        // Optional: Use layer mask if you set one
        if (groundLayer.value != 0)
            return Physics.Raycast(transform.position, Vector3.down, checkDistance, groundLayer);
        
        return Physics.Raycast(transform.position, Vector3.down, checkDistance);
    }

    // Public methods for PlayerFunctions to control movement
    public void SetForwardSpeed(float speed)
    {
        // This is now just a passthrough for PlayerFunctions to use
        // The actual forward movement is handled in PlayerFunctions
        // Can be used for any speed-related notifications if needed
    }

    public void StopMovement()
    {
        canMove = false;
        if (rb != null)
            rb.velocity = Vector3.zero;
    }

    public void ResumeMovement()
    {
        canMove = true;
    }

    // Public property to check if this is the active instance
    public bool IsActiveInstance => isActiveInstance;

    // Static method to get the active instance
    public static PlayerControls GetActiveInstance()
    {
        return activeInstance;
    }

    void OnDrawGizmos()
    {
        if (!isActiveInstance) return;

        Gizmos.color = Color.yellow;
        float left = -laneDistance;
        float right = laneDistance;

        Vector3 startL = new Vector3(left, 1, transform.position.z - 10);
        Vector3 endL = new Vector3(left, 1, transform.position.z + 10);

        Vector3 startR = new Vector3(right, 1, transform.position.z - 10);
        Vector3 endR = new Vector3(right, 1, transform.position.z + 10);

        Gizmos.DrawLine(startL, endL);
        Gizmos.DrawLine(startR, endR);

        // Visualize ground check
        if (col != null)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Vector3 rayStart = transform.position;
            Vector3 rayEnd = rayStart + Vector3.down * (col.bounds.extents.y + groundCheckDistance);
            Gizmos.DrawLine(rayStart, rayEnd);
        }
    }
}