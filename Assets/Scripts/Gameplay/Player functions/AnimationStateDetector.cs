using UnityEngine;

[DisallowMultipleComponent]
public class AnimationStateDetector : MonoBehaviour
{
    [Header("Auto-detected")]
    private Animator anim;
    private Rigidbody rb;
    private PlayerControls player;

    [Header("Tuning")]
    public float jumpVelocityThreshold = 0.1f;
    public float fallVelocityThreshold = -0.1f;

    void Awake()
    {
        // Animator: prefer THIS object, fallback to children/parent
        anim = GetComponent<Animator>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
        if (anim == null)
            anim = GetComponentInParent<Animator>();

        // Rigidbody: usually on parent
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = GetComponentInParent<Rigidbody>();

        // PlayerControls: always on player root
        player = GetComponent<PlayerControls>();
        if (player == null)
            player = GetComponentInParent<PlayerControls>();

        if (anim == null)
            Debug.LogError("AnimationStateDetector: Animator not found.");
        if (rb == null)
            Debug.LogError("AnimationStateDetector: Rigidbody not found.");
        if (player == null)
            Debug.LogError("AnimationStateDetector: PlayerControls not found.");
    }

    void Update()
    {
        if (anim == null || rb == null || player == null) return;

        bool grounded = player.IsGrounded();
        float yVel = rb.velocity.y;

        bool isJumping = !grounded && yVel > jumpVelocityThreshold;
        bool isFalling = !grounded && yVel < fallVelocityThreshold;

        anim.SetBool("isGrounded", grounded);
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("isFalling", isFalling);
    }
}
