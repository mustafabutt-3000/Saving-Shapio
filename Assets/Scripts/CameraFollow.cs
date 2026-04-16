using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    public float smoothSpeed = 0.125f;

    private void LateUpdate()
    {
        if (target == null)
        {
            // Try to find a new target (the split cube parts)
            GameObject newTarget = GameObject.FindWithTag("Player");
            if (newTarget != null)
                target = newTarget.transform;
            else
                return;
        }

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    // Call this when you know the new split cube instance
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
