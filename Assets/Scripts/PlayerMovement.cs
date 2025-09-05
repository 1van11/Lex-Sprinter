using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Forward Movement")]
    public float forwardSpeed = 8f;

    [Header("Lane System")]
    public float laneDistance = 3f; 
    public float laneChangeSpeed = 10f; 

    [Header("Jump")]
    public float jumpForce = 20f;
    public float jumpCooldown = 0.5f; 
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

    private Rigidbody rb;
    private CapsuleCollider col;
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

        // Save original collider size
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
            && !isSliding 
            && Time.time - lastJumpTime >= jumpCooldown) // Check cooldown
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time; // Update last jump time
        }
    }

    void HandleSlide()
    {
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !isSliding && IsGrounded())
        {
            StartCoroutine(Slide());
        }
    }

    IEnumerator Slide()
    {
        isSliding = true;

        // Shrink collider
        col.height = slideColliderHeight;
        col.center = new Vector3(col.center.x, slideColliderHeight / 2f, col.center.z);

        // Lean back
        Quaternion slideRotation = Quaternion.Euler(-90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z); 
        float t = 0f;
        while (t < 0.5f) // lean in
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, slideRotation, t / 0.2f);
            t += Time.deltaTime;
            yield return null;
        }

        // Stay in slide pose
        yield return new WaitForSeconds(slideDuration - 1f);

        // Return to upright
        t = 0f;
        Quaternion startRot = transform.rotation;
        while (t < 0.3f) // smooth return
        {
            transform.rotation = Quaternion.Slerp(startRot, Quaternion.identity, t / 0.3f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        // Reset collider
        col.height = originalColliderHeight;
        col.center = originalColliderCenter;    

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
