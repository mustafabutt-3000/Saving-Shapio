using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DashScript : MonoBehaviour
{
    private Rigidbody rb;
    public float dashSpeed = 15f;
    public float dashDuration = 0.18f;
    public bool IsDashing { get; private set; }

    private bool isGrounded;
    private bool hasAirDashed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (IsDashing) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isGrounded && hasAirDashed)
                return;

            Vector3 dashDir = Vector3.right;

            if (Keyboard.current.dKey.isPressed) dashDir = Vector3.right;
            else if (Keyboard.current.aKey.isPressed) dashDir = Vector3.left;
            else if (Keyboard.current.sKey.isPressed) dashDir = Vector3.down;
            else
            {
                var move = GetComponent<MoveScript>();
                if (move != null)
                    dashDir = (move.lastFacing == 1) ? Vector3.right : Vector3.left;
                else if (rb.linearVelocity.x > 0.1f) dashDir = Vector3.right;
                else if (rb.linearVelocity.x < -0.1f) dashDir = Vector3.left;
            }

            if (!isGrounded)
                hasAirDashed = true;

            IsDashing = true;
            StartCoroutine(DoDash(dashDir));
        }
    }

    private IEnumerator DoDash(Vector3 dashDir)
    {
        rb.linearVelocity = new Vector3(dashDir.x * dashSpeed, dashDir.y * dashSpeed, dashDir.z * dashSpeed);

        float t = 0f;
        while (t < dashDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        IsDashing = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
        hasAirDashed = false;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}