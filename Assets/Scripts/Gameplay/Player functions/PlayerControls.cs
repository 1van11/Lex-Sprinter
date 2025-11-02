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

    [Header("Slide")]
    public float slideDuration = 0.5f;
    public float slideColliderHeight = 0.5f;
    private bool isSliding = false;
    private float originalColliderHeight;
    private Vector3 originalColliderCenter;

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

    // Reference to PlayerFunctions for checking death state
    private PlayerFunctions playerFunctions;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        playerFunctions = GetComponent<PlayerFunctions>();

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        originalColliderHeight = col.height;
        originalColliderCenter = col.center;

        targetPosition = new Vector3(0, transform.position.y, transform.position.z);
        transform.position = targetPosition;
    }

    void Update()
    {
        if (playerFunctions != null && playerFunctions.isDead) return; // Stop all input if dead

        // Constant forward movement
        Vector3 forwardMove = new Vector3(0, 0, forwardSpeed * Time.deltaTime);
        transform.position += forwardMove;

        HandleLaneInput();
        MoveBetweenLanes();
        HandleJump();
        HandleSlide();
        HandleTiltAndLook();
        ApplyExtraGravity();
        DetectSwipe();

        // Track grounded time
        if (IsGrounded())
            lastGroundedTime = Time.time;

        // Track jump input
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            lastJumpPressedTime = Time.time;
    }

    void HandleLaneInput()
    {
        if (Time.time - lastInputTime < inputCooldown)
            return;

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
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            lastJumpPressedTime = Time.time;
            jumpHeld = true;
        }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            jumpHeld = false;
        }

        bool canJump = Time.time - lastGroundedTime <= coyoteTime &&
                       Time.time - lastJumpPressedTime <= jumpBufferTime &&
                       Time.time - lastJumpTime >= jumpCooldown &&
                       !isJumping &&
                       jumpHeld;

        if (canJump)
        {
            if (isSliding)
            {
                StopAllCoroutines();
                col.height = originalColliderHeight;
                col.center = originalColliderCenter;
                transform.rotation = Quaternion.identity;
                isSliding = false;
                
                if (anim != null)
                    anim.SetBool("isSliding", false);
            }

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;

            isJumping = true;
            jumpHeld = false;
            lastJumpPressedTime = -999f;

            // Trigger jump animation
            if (anim != null)
                StartCoroutine(JumpForSeconds(1f));
        }

        if (IsGrounded() && rb.velocity.y <= 0.1f)
        {
            isJumping = false;
        }
    }

    void ApplyExtraGravity()
    {
        if (!IsGrounded() && rb.velocity.y < 0)
        {
            rb.AddForce(Vector3.down * extraFallForce, ForceMode.Acceleration);
        }
    }

    void HandleSlide()
    {
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !isSliding)
        {
            if (IsGrounded()) 
                StartCoroutine(Slide());
            else 
                StartCoroutine(AirSlide());
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

                    if (swipeDelta.magnitude < swipeThreshold)
                        return;

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
                        else if (y < 0)
                        {
                            if (IsGrounded()) 
                                StartCoroutine(Slide());
                            else 
                                StartCoroutine(AirSlide());
                        }
                    }

                    swipeDetected = false;
                    break;
            }
        }
    }

    IEnumerator JumpForSeconds(float duration)
    {
        if (anim != null)
            anim.SetBool("isJumping", true);
        
        yield return new WaitForSeconds(duration);
        
        if (anim != null)
            anim.SetBool("isJumping", false);
    }

    IEnumerator Slide()
    {
        isSliding = true;

        // Trigger slide animation
        if (anim != null)
            StartCoroutine(SlideForSeconds(slideDuration));

        col.height = slideColliderHeight;
        col.center = new Vector3(col.center.x, slideColliderHeight / 2f, col.center.z);

        Quaternion slideRotation = Quaternion.Euler(-90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        float t = 0f;
        while (t < 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, slideRotation, t / 0.2f);
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(slideDuration - 1f);

        t = 0f;
        Quaternion startRot = transform.rotation;
        while (t < 0.3f)
        {
            transform.rotation = Quaternion.Slerp(startRot, Quaternion.identity, t / 0.3f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        col.height = originalColliderHeight;
        col.center = originalColliderCenter;

        isSliding = false;
    }

    IEnumerator SlideForSeconds(float duration)
    {
        if (anim != null)
            anim.SetBool("isSliding", true);
        
        yield return new WaitForSeconds(duration);
        
        if (anim != null)
            anim.SetBool("isSliding", false);
    }

    IEnumerator AirSlide()
    {
        isSliding = true;

        // Trigger slide animation
        if (anim != null)
            StartCoroutine(SlideForSeconds(0.4f));

        Vector3 currentVel = rb.velocity;
        rb.velocity = new Vector3(currentVel.x, -jumpForce * 2f, currentVel.z);

        Quaternion airRotation = Quaternion.Euler(30f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        float t = 0f;
        while (t < 0.2f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, airRotation, t / 0.2f);
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        transform.rotation = Quaternion.identity;
        isSliding = false;
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, col.bounds.extents.y + 0.1f);
    }

    public float GetForwardSpeed()
    {
        return forwardSpeed;
    }

    public void SetForwardSpeed(float speed)
    {
        forwardSpeed = speed;
    }

    public void StopMovement()
    {
        rb.velocity = Vector3.zero;
        forwardSpeed = 0f;
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