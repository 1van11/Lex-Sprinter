using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControls : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Movement")]
    public float laneDistance = 3f;
    public float horizontalMoveSpeed = 10f;
    public float forwardSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 9f;
    public float gravity = 20f;
    public float groundCheckDistance = 0.1f;
    public bool enableJump = true; // NEW: Toggle jump on/off in Inspector

    [Header("Fast Descent")]
    public float fastDescentForce = 15f;
    public float fastDescentGravityMultiplier = 2.5f;
    public float minSwipeDistance = 50f;
    public float swipeCooldown = 0.3f;

    [Header("Rotation")]
    public float tiltAngle = 20f;
    public float tiltSpeed = 10f;

    [Header("Mobile Input")]
    public bool useTouchControls = true;
    public float mobileSensitivity = 0.5f;

    // Components
    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator anim;

    // State
    private float horizontalInput;
    private float targetTilt;
    private bool isGrounded;
    private bool isFastDescending;
    private float lastSwipeTime;
    private bool isMovementStopped = false;

    // Touch / Drag
    private Vector2 touchStart;
    private float touchTime;
    private bool isDragging;

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

        if (useTouchControls)
            HandleTouchInput();

        MoveHorizontal();
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

        if (enableJump && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
            Jump();

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            TryFastDescent();
    }

    void HandleTouchInput()
    {
        if (isMovementStopped) return;
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);
        float screenX = t.position.x / Screen.width;

        horizontalInput = 0;
        targetTilt = 0;

        if (screenX < 0.4f)
        {
            horizontalInput = -1;
            targetTilt = tiltAngle;
        }
        else if (screenX > 0.6f)
        {
            horizontalInput = 1;
            targetTilt = -tiltAngle;
        }

        if (t.phase == TouchPhase.Began)
        {
            touchStart = t.position;
            touchTime = Time.time;

            if (enableJump && screenX >= 0.4f && screenX <= 0.6f)
                Jump();
        }

        if (t.phase == TouchPhase.Ended)
            DetectSwipe(t.position);
    }

    void DetectSwipe(Vector2 endPos)
    {
        if (Time.time - lastSwipeTime < swipeCooldown) return;

        Vector2 delta = endPos - touchStart;

        if (delta.y < -minSwipeDistance &&
            Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
        {
            TryFastDescent();
            lastSwipeTime = Time.time;
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

    void MoveHorizontal()
    {
        if (isMovementStopped) return;

        Vector3 pos = transform.position;
        pos.x += horizontalInput * horizontalMoveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -laneDistance, laneDistance);
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

    #region Drag / Swipe Interface

    public void OnBeginDrag(PointerEventData e)
    {
        if (isMovementStopped) return;
        touchStart = e.position;
        isDragging = true;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!isDragging || isMovementStopped) return;

        float x = (e.position.x - touchStart.x) * mobileSensitivity / Screen.width;
        horizontalInput = Mathf.Clamp(x, -1f, 1f);
        targetTilt = horizontalInput > 0 ? -tiltAngle : tiltAngle;
    }

    public void OnEndDrag(PointerEventData e)
    {
        isDragging = false;
        DetectSwipe(e.position);
        horizontalInput = 0;
        targetTilt = 0;
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

    #region Public Control Methods

    public void SetForwardSpeed(float speed)
    {
        forwardSpeed = speed;
    }

    public float GetForwardSpeed()
    {
        return forwardSpeed;
    }

    public void StopMovement()
    {
        isMovementStopped = true;
        forwardSpeed = 0f;
        horizontalInput = 0f;
        targetTilt = 0f;
        if (rb != null)
            rb.velocity = Vector3.zero;
        Debug.Log("🛑 PlayerControls movement stopped");
    }

    public void ResumeMovement()
    {
        isMovementStopped = false;
        Debug.Log("▶️ PlayerControls movement resumed");
    }

    // NEW: Methods to enable/disable jump at runtime
    public void EnableJump(bool enable)
    {
        enableJump = enable;
        Debug.Log(enable ? "✅ Jump enabled" : "❌ Jump disabled");
    }

    public bool IsJumpEnabled()
    {
        return enableJump;
    }

    #endregion

    #region Public Getters for Animator

    // Make these accessible to the CustomMovementAnimator
    public float HorizontalInput => horizontalInput;
    public bool IsGrounded => isGrounded;
    public bool IsFastDescending => isFastDescending;

    #endregion
}