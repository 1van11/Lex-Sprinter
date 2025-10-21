using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator galaw;
    [Header("Forward Movement")]
    public float forwardSpeed = 10f;

    [Header("Lane System")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 10f;
    [Header("Score")]
public int score = 0;                  // tracks current score
public TMPro.TMP_Text scoreText;       // assign your TMP Text in Inspector


    [Header("Jump")]
    public float jumpForce = 9f;
    public float extraFallForce = 10f;
    public float jumpCooldown = 0.25f;
    private float lastJumpTime;

    [Header("Jump Assist")]
    public float coyoteTime = 0.2f;       // grace period after leaving ground
    public float jumpBufferTime = 0.2f;   // grace period after pressing jump
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

    [Header("IFrames")]
    public float iFrameDuration = 1f;
    public float flashInterval = 0.1f;
    [HideInInspector] public bool isInvincible = false;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Renderer[] renderers;

    private int currentLane = 1;
    private Vector3 targetPosition;
    private bool isChangingLanes = false;
    private bool isJumping = false;
    private bool jumpHeld = false;

    private float inputCooldown = 0.2f;
    private float lastInputTime;

    private float targetTilt = 0f;
    private float targetYaw = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        // Get all renderers in player (including children)
        renderers = GetComponentsInChildren<Renderer>();

        // Freeze X and Z velocity so Rigidbody doesn't interfere with transform movement
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        originalColliderHeight = col.height;
        originalColliderCenter = col.center;

        targetPosition = new Vector3(0, transform.position.y, transform.position.z);
        transform.position = targetPosition;
    }

    void Update()
    {
        // Constant forward movement
        Vector3 forwardMove = new Vector3(0, 0, forwardSpeed * Time.deltaTime);
        transform.position += forwardMove;

        HandleLaneInput();
        MoveBetweenLanes();
        HandleJump();
        HandleSlide();
        HandleTiltAndLook();
        ApplyExtraGravity();

        // Track grounded time
        if (IsGrounded())
            lastGroundedTime = Time.time;

        // Track jump input
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            lastJumpPressedTime = Time.time;
    }

  void OnTriggerEnter(Collider other)
{
    // Case 1: Trap
    if (other.CompareTag("Trap") && !isInvincible)
    {
        StartCoroutine(TriggerIFrames(iFrameDuration, flashInterval));
    }

    // Case 2: Answer option
    if (other.CompareTag("AnswerOptions") && !isInvincible)
    {
        // Hardcoded correct answers
        string[] correctAnswers = new string[]
        {
            "CAT", "CUP", "NOSE", "BOY", "FAST", "DOG", "HAT", "LIP", "GIRL", "SLOW",
            "FISH", "BED", "LEG", "KID", "GOOD", "BIRD", "BOX", "ARM", "BABY", "BAD",
            "COW", "PEN", "HAND", "AUNT", "HAPPY", "PIG", "BOOK", "FOOT", "UNCLE",
            "SAD", "HEN", "CHAIR", "HAIR", "FRIEND", "TOY", "DUCK", "DESK", "TOOTH",
            "RUN", "MAP", "GOAT", "BAG", "RICE", "HOP", "CAR", "MOUSE", "SUN", "EGG",
            "SIT", "BUS", "RED", "MOON", "MILK", "STAND", "DOOR", "BLUE", "STAR",
            "MEAT", "WALK", "BELL", "GREEN", "RAIN", "CORN", "JUMP", "ME", "PINK",
            "SNOW", "PEAR", "SWIM", "YOU", "YELLOW", "TREE", "CAKE", "READ", "HIM",
            "BLACK", "LEAF", "BREAD", "WRITE", "HER", "WHITE", "WIND", "JAM", "SING",
            "BROWN", "CLOUD", "SOUP", "HOT", "GRAY", "ROCK", "MOM", "COLD", "ORANGE",
            "EYE", "DAD", "BIG", "BALL", "EAR", "MAN", "SMALL"
        };

        // Get the parent QuestionRandomizer to find the correct answer
        QuestionRandomizer questionRandomizer = other.GetComponentInParent<QuestionRandomizer>();
        
        if (questionRandomizer != null)
        {
            // Determine which answer this is based on the GameObject name
            bool isJumpOption = other.gameObject.name.Contains("Jump");
            string selectedAnswer = isJumpOption ? questionRandomizer.jumpText.text : questionRandomizer.slideText.text;
            
            // Check if the selected answer is correct (jumpText is always correct in your pairs)
            bool isCorrect = (selectedAnswer == questionRandomizer.jumpText.text);

            if (isCorrect)
            {
                    Debug.Log($"✅ Correct Answer! (+10 points) [{selectedAnswer}]");
                    score += 10;                        // add points
                    UpdateScoreUI();
            }
            else
            {
                    Debug.Log($"❌ Wrong Answer! (-5 points) [{selectedAnswer}] - Correct was: {questionRandomizer.jumpText.text}");
                    score -= 5;                         // deduct points
                    if (score < 0) score = 0;           // optional: prevent negative score
                    UpdateScoreUI();                
                    StartCoroutine(TriggerIFrames(iFrameDuration, flashInterval));
                // TODO: Deduct score
            }

            // Destroy the whole question object
            if (other.transform.parent != null)
                Destroy(other.transform.parent.gameObject);
        }
    }
}
void UpdateScoreUI()
{
    if (scoreText != null)
        scoreText.text = $"Score: {score}";
}


    public IEnumerator TriggerIFrames(float duration, float flashInterval)
    {
        isInvincible = true;
        float timer = 0f;

        while (timer < duration)
        {
            foreach (Renderer r in renderers)
            {
                r.enabled = !r.enabled; // toggle visibility
            }

            timer += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        // Ensure all renderers are visible at the end
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        isInvincible = false;
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
        // Detect initial jump press (not hold)
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            lastJumpPressedTime = Time.time;
            jumpHeld = true;
        }

        // Reset hold when released
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            jumpHeld = false;
        }

        bool canJump = Time.time - lastGroundedTime <= coyoteTime &&
                       Time.time - lastJumpPressedTime <= jumpBufferTime &&
                       Time.time - lastJumpTime >= jumpCooldown &&
                       !isJumping &&
                       jumpHeld; // Only true on the first press

        if (canJump)
        {
            if (isSliding)
            {
                StopAllCoroutines();
                col.height = originalColliderHeight;
                col.center = originalColliderCenter;
                transform.rotation = Quaternion.identity;
                isSliding = false;
            }

            // Stop all vertical velocity before new jump
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;

            // Prevent holding from stacking
            isJumping = true;
            jumpHeld = false; // block continuous pressing
            lastJumpPressedTime = -999f;
        }

        // Reset when grounded
        if (IsGrounded() && rb.velocity.y <= 0.1f)
            isJumping = false;
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
            if (IsGrounded()) StartCoroutine(Slide());
            else StartCoroutine(AirSlide());
        }
    }

   IEnumerator Slide()
{
    isSliding = true;

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


    IEnumerator AirSlide()
    {
        isSliding = true;

        // Store current velocities and only modify Y
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

    bool IsGrounded()
    {
        // Raycast from center of collider downwards
        return Physics.Raycast(transform.position, Vector3.down, col.bounds.extents.y + 0.1f);
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