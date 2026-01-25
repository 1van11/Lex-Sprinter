using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControls : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Movement")]
    public float laneDistance = 3f;
    public float horizontalMoveSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 9f;
    public float gravity = 20f;
    public float groundCheckDistance = 0.1f;

    [Header("Fast Descent")]
    public float fastDescentForce = 15f;
    public float fastDescentGravityMultiplier = 2.5f;
    public float minSwipeDistance = 50f;
    public float swipeCooldown = 0.3f;
    
    [Header("Rotation")]
    public float tiltAngle = 20f;
    public float tiltSpeed = 10f;

    [Header("Mobile Input")]
    public float mobileSensitivity = 0.5f;
    public bool useTouchControls = true;
    
    private Animator anim;
    private Rigidbody rb;
    private CapsuleCollider col;
    
    private float horizontalInput = 0f;
    private float targetTilt = 0f;
    private bool isGrounded;
    private bool isFastDescending = false;
    private float lastSwipeTime = 0f;
    
    // Mobile input variables
    private Vector2 touchStartPos;
    private Vector2 touchCurrentPos;
    private bool isTouching = false;
    private float touchTime = 0f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        }
    }

    void Update()
    {
        CheckGrounded();
        HandleInput();
        MoveHorizontal();
        UpdateAnimations();
        ApplyTilt();
        HandleFastDescent();
    }

    void CheckGrounded()
    {
        if (col == null) return;
        
        float checkDist = col.bounds.extents.y + groundCheckDistance;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, checkDist);
        
        // Reset fast descent when grounded
        if (isGrounded && isFastDescending)
        {
            isFastDescending = false;
        }
    }

    void HandleInput()
    {
        // Reset input
        horizontalInput = 0f;
        targetTilt = 0f;
        
        // Handle desktop input
        HandleDesktopInput();
        
        // Handle mobile touch input
        if (useTouchControls)
        {
            HandleTouchInput();
        }
        
        // Handle jump for both desktop and mobile
        HandleJumpInput();
    }

    void HandleDesktopInput()
    {
        // Left/Right
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput = -1f;
            targetTilt = tiltAngle;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontalInput = 1f;
            targetTilt = -tiltAngle;
        }
        
        // Fast descent (Swipe Down on Desktop: S or Down Arrow)
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !isGrounded)
        {
            TriggerFastDescent();
        }
    }

    void HandleTouchInput()
    {
        // Mobile horizontal movement via touch position
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            // Convert touch position to screen percentage
            float screenPercent = touch.position.x / Screen.width;
            
            // Determine horizontal input based on screen position
            if (screenPercent < 0.4f) // Left 40% of screen
            {
                horizontalInput = -1f;
                targetTilt = tiltAngle;
            }
            else if (screenPercent > 0.6f) // Right 40% of screen
            {
                horizontalInput = 1f;
                targetTilt = -tiltAngle;
            }
            // Middle 20% is neutral
            
            // Handle tap for jump
            if (touch.phase == TouchPhase.Began && isGrounded)
            {
                touchStartPos = touch.position;
                touchTime = Time.time;
            }
            
            // Handle swipe detection
            if (touch.phase == TouchPhase.Ended)
            {
                Vector2 swipeDelta = touch.position - touchStartPos;
                float swipeTime = Time.time - touchTime;
                
                // Check for fast swipe down
                if (swipeDelta.y < -minSwipeDistance && Mathf.Abs(swipeDelta.x) < Mathf.Abs(swipeDelta.y) && 
                    swipeTime < 0.5f && !isGrounded)
                {
                    TriggerFastDescent();
                }
            }
        }
    }

    void HandleJumpInput()
    {
        // Desktop jump
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded && rb != null)
        {
            PerformJump();
        }
        
        // Mobile jump (tap in middle of screen)
        if (useTouchControls && Input.touchCount > 0 && isGrounded && rb != null)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                float screenPercent = touch.position.x / Screen.width;
                if (screenPercent >= 0.4f && screenPercent <= 0.6f) // Middle 20% for jump
                {
                    PerformJump();
                }
            }
        }
    }

    void PerformJump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        if (anim != null)
        {
            anim.SetTrigger("Jump");
        }
    }

    void TriggerFastDescent()
    {
        if (Time.time - lastSwipeTime < swipeCooldown) return;
        
        isFastDescending = true;
        lastSwipeTime = Time.time;
        
        if (rb != null && !isGrounded)
        {
            // Apply downward force
            rb.AddForce(Vector3.down * fastDescentForce, ForceMode.VelocityChange);
            
            // Trigger fast descent animation
            if (anim != null)
            {
                anim.SetTrigger("FastDescent");
            }
            
            Debug.Log("Fast Descent Activated!");
        }
    }

    void HandleFastDescent()
    {
        if (isFastDescending && !isGrounded && rb != null)
        {
            // Apply extra gravity during fast descent
            rb.AddForce(Vector3.down * (gravity * fastDescentGravityMultiplier), ForceMode.Acceleration);
        }
    }

    void MoveHorizontal()
    {
        Vector3 pos = transform.position;
        pos.x += horizontalInput * horizontalMoveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -laneDistance, laneDistance);
        transform.position = pos;
    }

    void UpdateAnimations()
    {
        if (anim == null) return;
        
        // Running when moving horizontally
        anim.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0.01f);
        
        // Jumping when not grounded
        anim.SetBool("isJumping", !isGrounded);
        
        // Idle when grounded and not moving
        anim.SetBool("isIdle", isGrounded && Mathf.Abs(horizontalInput) < 0.01f);
        
        // Fast descent animation state
        anim.SetBool("isFastDescending", isFastDescending && !isGrounded);
    }

    void ApplyTilt()
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * tiltSpeed);
    }

    void FixedUpdate()
    {
        // Apply normal gravity when not fast descending
        if (!isGrounded && rb != null && rb.velocity.y < 0 && !isFastDescending)
        {
            rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        }
    }

    // UI Drag handlers for mobile (optional alternative control scheme)
    public void OnBeginDrag(PointerEventData eventData)
    {
        touchStartPos = eventData.position;
        isTouching = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isTouching) return;
        
        touchCurrentPos = eventData.position;
        Vector2 dragDelta = touchCurrentPos - touchStartPos;
        
        // Horizontal movement based on drag
        if (Mathf.Abs(dragDelta.x) > 10f)
        {
            horizontalInput = Mathf.Clamp(dragDelta.x * mobileSensitivity / Screen.width, -1f, 1f);
            targetTilt = horizontalInput > 0 ? -tiltAngle : tiltAngle;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isTouching) return;
        
        Vector2 swipeDelta = eventData.position - touchStartPos;
        
        // Check for swipe down
        if (swipeDelta.y < -minSwipeDistance && Mathf.Abs(swipeDelta.x) < Mathf.Abs(swipeDelta.y) && !isGrounded)
        {
            TriggerFastDescent();
        }
        
        isTouching = false;
        horizontalInput = 0f;
    }

    public bool IsGrounded() => isGrounded;

    public void StopMovement()
    {
        if (rb != null)
            rb.velocity = Vector3.zero;
    }

    public void ResumeMovement()
    {
        // Movement resumes automatically
    }

    // Dummy method for PlayerFunctions compatibility
    public void SetForwardSpeed(float speed)
    {
        // PlayerFunctions handles forward speed, this is just for compatibility
    }

    void OnDrawGizmos()
    {
        // Lane boundaries
        Gizmos.color = Color.yellow;
        Vector3 startL = new Vector3(-laneDistance, 1, transform.position.z - 10);
        Vector3 endL = new Vector3(-laneDistance, 1, transform.position.z + 10);
        Vector3 startR = new Vector3(laneDistance, 1, transform.position.z - 10);
        Vector3 endR = new Vector3(laneDistance, 1, transform.position.z + 10);
        
        Gizmos.DrawLine(startL, endL);
        Gizmos.DrawLine(startR, endR);
        
        // Ground check
        if (col != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 rayStart = transform.position;
            Vector3 rayEnd = rayStart + Vector3.down * (col.bounds.extents.y + groundCheckDistance);
            Gizmos.DrawLine(rayStart, rayEnd);
        }
        
        // Fast descent indicator when active
        if (isFastDescending && !isGrounded)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.down * 2f, 0.5f);
        }
    }
#region vibrate
    void OnCollisionEnter(Collision other)
{
    if (other.gameObject.CompareTag("Obstacle"))
    {
        FeedbackManager.Instance.Vibrate();
    }
}

void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("WrongItem"))
    {
        FeedbackManager.Instance.Vibrate();
    }
}
#endregion
}
//working