using UnityEngine;

public class KillEffect : MonoBehaviour
{
    [Header("Split Cube Prefab")]
    public GameObject cutCubePrefab; // assign your split version in Inspector
    public GameObject CrushedCubePrefab;
    [Header("Separation Force Settings")]
    public float separationForce = 2f; // tweak in inspector
    public float separationRadius = 0.5f; // how wide the push spreads

    private JumpScript jumpScript;

    void Start()
    {
        jumpScript = GetComponent<JumpScript>();
    }

    public void TriggerKill1Vertical()
    {
        SpawnAndPush(Quaternion.Euler(transform.rotation.x, 90f, transform.rotation.z));
    }

    public void TriggerKill1Horizontal()
    {
        SpawnAndPush(Quaternion.Euler(transform.rotation.x, transform.rotation.y, 90f));
    }
    public void TriggerCrush()
    {
        // Spawn crushed cube
        GameObject crushedCube = Instantiate(CrushedCubePrefab, transform.position, transform.rotation);
        crushedCube.transform.localScale = transform.localScale;
        // Destroy the original cube
        Destroy(gameObject);
    }
    private void SpawnAndPush(Quaternion rot)
    {
        // Spawn cut cube
        GameObject cutCube = Instantiate(cutCubePrefab, transform.position, rot);
        cutCube.transform.localScale = transform.localScale;

        // Only apply force if grounded
        if (jumpScript != null && jumpScript.IsGrounded)
        {
            Rigidbody[] rbs = cutCube.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbs)
            {
                rb.AddExplosionForce(separationForce, transform.position, separationRadius, 0f, ForceMode.Impulse);
            }
        }

        // Destroy the original cube
        Destroy(gameObject);
    }
}
