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
    public int score = 0;
    public TMPro.TMP_Text scoreText;
    
    [Header("Distance")]
    public float distanceTraveled = 0f;
    private Vector3 lastPosition;
    public TMPro.TMP_Text distanceText;
    
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

    [Header("IFrames")]
    public float iFrameDuration = 1f;
    public float flashInterval = 0.1f;
    [HideInInspector] public bool isInvincible = false;

    [Header("Buffs")]
    public bool hasShield = false;
    public bool hasMagnet = false;
    public bool isSlowTime = false;
    
    [Header("Buff Durations")]
    public float shieldDuration = 8f;      // Shield lasts longer for protection
    public float magnetDuration = 6f;      // Magnet medium duration
    public float slowTimeDuration = 4f;    // Slow time shorter (very powerful)

    [Header("Magnet Settings")]
    public float magnetRadius = 7f;
    public float magnetPullSpeed = 10f;
    public LayerMask coinLayer;

    [Header("Buff Visual Feedback (Optional)")]
    public GameObject shieldVisual;
    public GameObject magnetVisual;

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

    // Swipe Controls
    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private bool swipeDetected = false;
    private float swipeThreshold = 50f;

    private PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        renderers = GetComponentsInChildren<Renderer>();

        playerHealth = GetComponent<PlayerHealth>(); // ✅ LINK HEALTH

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        originalColliderHeight = col.height;
        originalColliderCenter = col.center;

        targetPosition = new Vector3(0, transform.position.y, transform.position.z);
        transform.position = targetPosition;
        lastPosition = transform.position;
    }

    void Update()
    {
        Vector3 forwardMove = new Vector3(0, 0, forwardSpeed * Time.deltaTime);
        transform.position += forwardMove;

        distanceTraveled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        if (distanceText != null)
            distanceText.text = $"Distance: {Mathf.FloorToInt(distanceTraveled)} m";

        float bonus = 1f;
        for (int i = 300; i <= distanceTraveled; i += 300)
        {
            bonus *= 1.125f;
            if (bonus >= 1.8f)
                break;
        }
        forwardSpeed = 10f * bonus;

        HandleLaneInput();
        MoveBetweenLanes();
        HandleJump();
        HandleSlide();
        HandleTiltAndLook();
        ApplyExtraGravity();
        DetectSwipe();

        if (IsGrounded())
            lastGroundedTime = Time.time;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            lastJumpPressedTime = Time.time;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            score += 1;
            UpdateScoreUI();
            other.gameObject.SetActive(false);
            return;
        }

        // ✅ Trap Damage Now Uses Hearts
        if (other.CompareTag("Trap"))
        {
            if (!hasShield)
                playerHealth.TakeDamage();
            else
                Debug.Log("🛡️ Shield blocked trap!");

            return;
        }

        // ✅ Answer Check Uses Hearts
        if (other.CompareTag("AnswerOptions"))
        {
            QuestionRandomizer questionRandomizer = other.GetComponentInParent<QuestionRandomizer>();

            if (questionRandomizer != null)
            {
                bool isJumpOption = other.gameObject.name.Contains("Jump");
                string selectedAnswer = isJumpOption ? questionRandomizer.jumpText.text : questionRandomizer.slideText.text;

                bool isCorrect = (selectedAnswer == questionRandomizer.correctAnswer);

                if (isCorrect)
                {
                    score += 5;
                    UpdateScoreUI();
                }
                else
                {
                    if (!hasShield)
                        playerHealth.TakeDamage();
                    else
                        Debug.Log("🛡️ Shield protected wrong answer!");
                }

                Destroy(other.transform.parent.gameObject);
            }
        }

        if (other.CompareTag("Shield"))
        {
            hasShield = true;
            Destroy(other.gameObject);
            Debug.Log("🛡️ Shield on!");
        }
    


        // 🟢 SHIELD BUFF
        if (other.CompareTag("Shield"))
        {
            StartCoroutine(ShieldBuff());
            Destroy(other.gameObject);
            Debug.Log("🛡️ Shield activated!");
        }

        // 🟢 MAGNET BUFF
        if (other.CompareTag("Magnet"))
        {
            StartCoroutine(MagnetBuff());
            Destroy(other.gameObject);
            Debug.Log("🧲 Magnet activated!");
        }

        // 🟢 SLOW TIME BUFF
        if (other.CompareTag("SlowTime"))
        {
            StartCoroutine(SlowTimeBuff());
            Destroy(other.gameObject);
            Debug.Log("⏰ Slow Time activated!");
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"{score}";
    }
    public IEnumerator TriggerIFrames(float duration, float flashInterval)
    {
        isInvincible = true;
        float timer = 0f;

        while (timer < duration)
        {
            foreach (Renderer r in renderers)
            {
                r.enabled = !r.enabled;
            }

            timer += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        isInvincible = false;
    }

    IEnumerator ShieldBuff()
    {
        hasShield = true;
        isInvincible = true;
        
        if (shieldVisual != null)
            shieldVisual.SetActive(true);

        Debug.Log($"🛡️ Shield active for {shieldDuration} seconds");
        yield return new WaitForSeconds(shieldDuration);

        if (shieldVisual != null)
            shieldVisual.SetActive(false);
        
        hasShield = false;
        isInvincible = false;
        Debug.Log("🛡️ Shield expired");
    }

    IEnumerator MagnetBuff()
    {
        hasMagnet = true;

        if (magnetVisual != null)
            magnetVisual.SetActive(true);

        Debug.Log($"🧲 Magnet active for {magnetDuration} seconds");

        float timer = magnetDuration;
        while (timer > 0)
        {
            // Find all coins within radius (using tag instead of layer for simplicity)
            GameObject[] allCoins = GameObject.FindGameObjectsWithTag("Coin");
            
            int pulledCount = 0;
            
            foreach (GameObject coinObj in allCoins)
            {
                // Skip disabled/pooled coins
                if (coinObj == null || !coinObj.activeInHierarchy)
                    continue;
                
                // Check if coin is within magnet radius
                float distance = Vector3.Distance(transform.position, coinObj.transform.position);
                if (distance <= magnetRadius)
                {
                    pulledCount++;
                    
                    // Pull coin toward player (it will trigger OnTriggerEnter when it touches)
                    coinObj.transform.position = Vector3.MoveTowards(
                        coinObj.transform.position,
                        transform.position,
                        magnetPullSpeed * Time.deltaTime
                    );
                }
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        if (magnetVisual != null)
            magnetVisual.SetActive(false);

        hasMagnet = false;
        Debug.Log("🧲 Magnet expired");
    }

    IEnumerator SlowTimeBuff()
    {
        isSlowTime = true;
        Time.timeScale = 0.5f; // Slow to 50% speed

        Debug.Log($"⏰ Slow Time active for {slowTimeDuration} seconds (real time)");
        
        // Wait for real-time duration (not affected by timeScale)
        yield return new WaitForSecondsRealtime(slowTimeDuration);

        Time.timeScale = 1f; // Return to normal speed
        isSlowTime = false;
        Debug.Log("⏰ Slow Time expired");
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
            }

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;

            isJumping = true;
            jumpHeld = false;
            lastJumpPressedTime = -999f;
        }

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
                        // Horizontal Swipe
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
                        // Vertical Swipe
                        if (y > 0)
                        {
                            lastJumpPressedTime = Time.time;
                            jumpHeld = true;
                        }
                        else if (y < 0)
                        {
                            if (IsGrounded()) StartCoroutine(Slide());
                            else StartCoroutine(AirSlide());
                        }
                    }

                    swipeDetected = false;
                    break;
            }
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

        // Draw magnet radius when active
        if (hasMagnet)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, magnetRadius);
        }
    }
}