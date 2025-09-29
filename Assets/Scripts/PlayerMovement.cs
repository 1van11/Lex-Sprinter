using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Forward Movement")]
    public float forwardSpeed = 10f;

    [Header("Lane System")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 10f;          
    public float extraFallForce = 10f;    
    public float jumpCooldown = 0.2f;
    private float lastJumpTime;

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

        originalColliderHeight = col.height;
        originalColliderCenter = col.center;

        targetPosition = new Vector3(0, transform.position.y, transform.position.z);
        transform.position = targetPosition;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * forwardSpeed, Space.World);

        HandleLaneInput();
        MoveBetweenLanes();
        HandleJump();
        HandleSlide();
        HandleTiltAndLook();
        ApplyExtraGravity();
    }

    void OnTriggerEnter(Collider other)
    {
        // Trigger i-frames if player touches a trap
        if (other.CompareTag("Trap") && !isInvincible)
        {
            StartCoroutine(TriggerIFrames(iFrameDuration, flashInterval));
        }
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
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            && IsGrounded()
            && Time.time - lastJumpTime >= jumpCooldown)
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
        }
    }

    void ApplyExtraGravity()
    {
        if (!IsGrounded())
        {
            if (rb.velocity.y < 0)
                rb.AddForce(Vector3.down * extraFallForce, ForceMode.Acceleration);
            else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
                rb.AddForce(Vector3.down * (extraFallForce * 0.5f), ForceMode.Acceleration);
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

        rb.AddForce(Vector3.down * (jumpForce * 2f), ForceMode.Impulse);

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
        return Mathf.Abs(rb.velocity.y) < 0.01f;
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
