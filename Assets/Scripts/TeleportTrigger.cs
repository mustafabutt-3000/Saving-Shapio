using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    public Transform teleportPoint;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        other.transform.position = teleportPoint.position;

        // Reset rigidbody velocity so player doesn't carry momentum through
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = Vector3.zero;
    }
}