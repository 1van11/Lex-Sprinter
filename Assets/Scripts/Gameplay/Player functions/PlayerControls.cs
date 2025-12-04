using System.Collections;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Forward Movement")]
    public float forwardSpeed = 10f;

    [Header("Horizontal Movement")]
    public float laneDistance = 3f; // limit range (-laneDistance to +laneDistance)
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

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    void Update()
    {
        if (!canMove) return;

        // Forward movement
        Vector3 forwardMove = new Vector3(0, 0, forwardSpeed * Time.deltaTime);
        transform.position += forwardMove;

        HandleLaneInput();
        MoveHorizontal();
        HandleJump();
        HandleTiltAndLook();
        ApplyExtraGravity();
        DetectSwipe();
        UpdateJumpAnimation();

        if (IsGrounded())
            lastGroundedTime = Time.time;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            lastJumpPressedTime = Time.time;
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
    // Jump System
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
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;
            isJumping = true;
            jumpHeld = false;
            lastJumpPressedTime = -999f;
        }

        // Smash
        if (!IsGrounded() && !isSmashing && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            isSmashing = true;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.down * smashDownForce, ForceMode.Impulse);
        }

        // Reset when grounded
        if (IsGrounded() && rb.velocity.y <= 0.1f)
        {
            isJumping = false;
            isSmashing = false;
        }
    }

    void UpdateJumpAnimation()
    {
        if (anim == null) return;
        anim.SetBool("isJumping", !IsGrounded());
    }

    void ApplyExtraGravity()
    {
        if (!IsGrounded() && rb.velocity.y < 0)
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
                                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                                rb.AddForce(Vector3.down * smashDownForce, ForceMode.Impulse);
                            }
                        }
                    }

                    swipeDetected = false;
                    break;
            }
        }
    }

    // -----------------------
    // Helpers
    // -----------------------
    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, col.bounds.extents.y + 0.1f);
    }

    public float GetForwardSpeed() => forwardSpeed;

    public void SetForwardSpeed(float speed) => forwardSpeed = speed;

    public void StopMovement()
    {
        canMove = false;
        rb.velocity = Vector3.zero;
        forwardSpeed = 0f;
    }

    public void ResumeMovement()
    {
        canMove = true;
        forwardSpeed = 10f;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        float left = -laneDistance;
        float right = laneDistance;

        Vector3 startL = new Vector3(left, 1, transform.position.z - 10);
        Vector3 endL = new Vector3(left, 1, transform.position.z + 10);

        Vector3 startR = new Vector3(right, 1, transform.position.z - 10);
        Vector3 endR = new Vector3(right, 1, transform.position.z + 10);

        Gizmos.DrawLine(startL, endL);
        Gizmos.DrawLine(startR, endR);
    }
}
