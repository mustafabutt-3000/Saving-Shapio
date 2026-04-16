using System.Collections;
using UnityEngine;

public class TriggerTrap1Bottom : MonoBehaviour
{
    [Header("Blade Settings")]
    public Transform blade;           // Assign the blade in Inspector
    public float rotationTime = 0.1f; // How long it takes to rotate
    public float targetX = -210f;     // Target X rotation (degrees)

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(RotateBladeOtherWay());
        }
    }

    private IEnumerator RotateBladeOtherWay()
    {
        // use localEulerAngles so rotation is relative to blade's local space
        Vector3 startEuler = blade.localEulerAngles;
        float startX = startEuler.x;
        float endX = targetX;

        // shortest signed angle from start to end (-180 .. 180)
        float shortest = Mathf.DeltaAngle(startX, endX);

        // compute the *long* delta (the opposite direction)
        float longDelta = (shortest > 0f) ? shortest - 360f : shortest + 360f;

        float elapsed = 0f;
        while (elapsed < rotationTime)
        {
            float t = elapsed / rotationTime;
            float currentX = startX + longDelta * t; // step along the long route
            blade.localEulerAngles = new Vector3(currentX, startEuler.y, startEuler.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ensure exact final rotation
        blade.localEulerAngles = new Vector3(startX + longDelta, startEuler.y, startEuler.z);
    }
}
