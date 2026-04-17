using UnityEngine;

public class OriginSender : MonoBehaviour
{
    [Tooltip("The tag of the object that should be sent to 0,0,0.")]
    public string targetTag = "Player";

    // Use OnCollisionEnter if the collider is NOT a trigger
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(targetTag))
        {
            ResetToOrigin(collision.gameObject);
        }
    }

    private void ResetToOrigin(GameObject target)
    {
        target.transform.position = Vector3.zero;

        // Optional: Reset velocity if it's a Physics object to prevent it from flying away
        if (target.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
