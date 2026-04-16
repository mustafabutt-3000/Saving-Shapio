using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    // Define the possible axes for rotation
    public enum RotationAxis { X, Y, Z }

    [Header("Rotation Settings")]
    [Tooltip("Select which axis the object should rotate around.")]
    public RotationAxis axis = RotationAxis.Y;

    [Tooltip("The speed of rotation. Use negative values for reverse direction.")]
    public float rotationSpeed = 30.0f;

    void Update()
    {
        // Calculate the rotation amount for this frame
        float step = rotationSpeed * Time.deltaTime;

        // Apply rotation based on the selected axis
        switch (axis)
        {
            case RotationAxis.X:
                transform.Rotate(step, 0, 0);
                break;
            case RotationAxis.Y:
                transform.Rotate(0, step, 0);
                break;
            case RotationAxis.Z:
                transform.Rotate(0, 0, step);
                break;
        }
    }
}