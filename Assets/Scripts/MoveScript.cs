using UnityEngine;
using UnityEngine.InputSystem;

public class MoveScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    [HideInInspector] public int lastFacing = 1;

    private Rigidbody rb;
    private DashScript dash;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        dash = GetComponent<DashScript>();
    }

    void Update()
    {
        if (dash != null && dash.IsDashing) return;

        float move = 0f;
        if (Keyboard.current.dKey.isPressed) move = 1f;
        if (Keyboard.current.aKey.isPressed) move = -1f;

        if (move != 0f) lastFacing = move > 0f ? 1 : -1;

        Vector3 vel = rb.linearVelocity;
        vel.x = move * moveSpeed;
        rb.linearVelocity = vel;
    }
}