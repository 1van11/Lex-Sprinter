using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 4f;
    public float moveSpeed = 7f;
    public float jumpForce = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Auto move forward (Z axis)
        Vector3 forwardMove = Vector3.forward * playerSpeed;

        // Left / Right movement (X axis)
        float moveHorizontal = Input.GetAxis("Horizontal");
        Vector3 horizontalMove = new Vector3(moveHorizontal * moveSpeed, 0f, 0f);

        // Apply movement with physics
        rb.velocity = new Vector3(horizontalMove.x, rb.velocity.y, forwardMove.z);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
