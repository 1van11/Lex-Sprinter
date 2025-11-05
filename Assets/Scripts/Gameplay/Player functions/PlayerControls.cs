using System.Collections;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Forward Movement")]
    public float forwardSpeed = 10f;

    [Header("Lane System")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 10f;
    
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

    private int currentLane = 1;
    private Vector3 targetPosition;
    private bool isChangingLanes = false;
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

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        targetPosition = new Vector3(0, transform.position.y, transform.position.z);
        transform.position = targetPosition;
    }

    void Update()
    {
        if (!canMove) return;

        Vector3 forwardMove = new Vector3(0, 0, forwardSpeed * Time.deltaTime);
        transform.position += forwardMove;

        HandleLaneInput();
        MoveBetweenLanes();
        HandleJump();
        HandleTiltAndLook();
        ApplyExtraGravity();
        DetectSwipe();
        UpdateJumpAnimation(); // NEW: Update animation based on grounded state

        if (IsGrounded())
            lastGroundedTime = Time.time;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            lastJumpPressedTime = Time.time;
    }

    void HandleLaneInput()
    {
        if (Time.time - lastInputTime < inputCooldown) return;

        if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && currentLane > 0)
        {
            currentLane--;
            SetTargetPosition();
            lastInputTime = Time.time;
            targetTilt = tiltAngle;
            targetYaw = -lookAngle;
        }
        else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && currentLane < 2)
        {
            currentLane++;
            SetTargetPosition();
            lastInputTime = Time.time;
            targetTilt = -tiltAngle;
            targetYaw = lookAngle;
        }
    }

    void SetTargetPosition()
    {
        float targetX = (currentLane - 1) * laneDistance;
        targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);
        isChangingLanes = true;
    }

    void MoveBetweenLanes()
    {
        if (isChangingLanes)
        {
            Vector3 currentPos = transform.position;
            float newX = Mathf.MoveTowards(currentPos.x, targetPosition.x, laneChangeSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, currentPos.y, currentPos.z);

            if (Mathf.Abs(transform.position.x - targetPosition.x) < 0.1f)
            {
                transform.position = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
                isChangingLanes = false;
                targetTilt = 0f;
                targetYaw = 0f;
            }
        }
    }

    void HandleTiltAndLook()
    {
        Quaternion targetRotation = Quaternion.Euler(0, targetYaw, targetTilt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * tiltSpeed);

        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
        if (angleDifference < 1f)
            transform.rotation = targetRotation;

        if (!isChangingLanes && Mathf.Abs(targetTilt) < 0.01f && Mathf.Abs(targetYaw) < 0.01f)
            transform.rotation = Quaternion.identity;
    }

    void HandleJump()
    {
        // Regular jump input
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            lastJumpPressedTime = Time.time;
            jumpHeld = true;
        }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow))
            jumpHeld = false;

        // Jump execution
        bool canJump = Time.time - lastGroundedTime <= coyoteTime &&
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

        // Jump smash input
        if (!IsGrounded() && !isSmashing && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            isSmashing = true;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.down * smashDownForce, ForceMode.Impulse);
        }

        // Reset jump states when grounded
        if (IsGrounded() && rb.velocity.y <= 0.1f)
        {
            isJumping = false;
            isSmashing = false;
        }
    }

    // NEW: Update jump animation based on mid-air state
    void UpdateJumpAnimation()
    {
        if (anim == null) return;

        // Only set isJumping to true when character is in the air
        bool shouldPlayJumpAnim = !IsGrounded();
        anim.SetBool("isJumping", shouldPlayJumpAnim);
    }

    void ApplyExtraGravity()
    {
        if (!IsGrounded() && rb.velocity.y < 0)
        {
            float gravityMultiplier = isSmashing ? 2f : 1f;
            rb.AddForce(Vector3.down * extraFallForce * gravityMultiplier, ForceMode.Acceleration);
        }
    }

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
                        if (x > 0 && currentLane < 2)
                        {
                            currentLane++;
                            SetTargetPosition();
                            targetTilt = -tiltAngle;
                            targetYaw = lookAngle;
                        }
                        else if (x < 0 && currentLane > 0)
                        {
                            currentLane--;
                            SetTargetPosition();
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
                    }

                    swipeDetected = false;
                    break;
            }
        }
    }

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
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * laneDistance;
            Vector3 start = new Vector3(x, 1, transform.position.z - 10);
            Vector3 end = new Vector3(x, 1, transform.position.z + 10);
            Gizmos.DrawLine(start, end);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
    }
}