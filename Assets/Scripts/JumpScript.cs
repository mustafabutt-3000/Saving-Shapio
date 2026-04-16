using UnityEngine;
using UnityEngine.InputSystem;

public class JumpScript : MonoBehaviour
{
    private Rigidbody rb;
    private bool isGrounded;
    private float originalGravity;
    private float groundedBuffer = 0f;
    private const float groundedGraceTime = 0.08f; // seconds of grace after leaving surface

    public float jumpForce = 8f;
    public float fallMultiplier = 2.5f;
    public bool IsGrounded => isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalGravity = Physics.gravity.y;
    }

    void Update()
    {
        // Count down the grace timer
        if (groundedBuffer > 0f)
            groundedBuffer -= Time.deltaTime;

        bool canJump = isGrounded || groundedBuffer > 0f;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && canJump)
        {
            groundedBuffer = 0f;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            rb.AddTorque(Vector3.forward * -0.44f, ForceMode.Impulse);
        }

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * originalGravity * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
        groundedBuffer = groundedGraceTime;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
        groundedBuffer = groundedGraceTime;
    }
}