using UnityEngine;

public class DestroyShape : MonoBehaviour
{
    // Drag your crushed prefab here in the Inspector
    public GameObject crushedPrefab;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            DashScript dash = collision.collider.GetComponent<DashScript>();

            // Check if dashing
            if (dash != null && dash.IsDashing)
            {
                CrushAndDestroy();
            }
        }
    }

    private void CrushAndDestroy()
    {
        // 1. Spawn the crushed version at this object's exact location/rotation
        if (crushedPrefab != null)
        {
            Instantiate(crushedPrefab, transform.position, transform.rotation);
        }

        // 2. Destroy the original object
        Destroy(gameObject);
    }
}