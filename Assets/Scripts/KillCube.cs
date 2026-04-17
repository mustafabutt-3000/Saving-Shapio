using UnityEngine;

public class KillCube : MonoBehaviour
{
    [Tooltip("Tag of the player object (must match the player's tag).")]
    public string playerTag = "Player";

    [Tooltip("If true, this blade will only trigger a kill once.")]
    public bool onlyOnce = true;

    bool hasTriggered = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (onlyOnce && hasTriggered) return;
        TryTriggerKill(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && hasTriggered) return;
        TryTriggerKill(other.gameObject);
    }

    private void TryTriggerKill(GameObject target)
    {
        if (!target.CompareTag(playerTag)) return;

        // Try to get the KillEffect component and call TriggerKill()
        if (target.TryGetComponent<KillEffect>(out var killEffect))
        {
            if (gameObject.name.StartsWith("trap 1"))
            {
                Debug.Log($"[KillCube] Blade hit '{target.name}' — calling TriggerKill().");
                killEffect.TriggerKill1Vertical();
            }
            if (gameObject.name.StartsWith("Projectile"))
            {
                Debug.Log($"[KillCube] Projectile hit '{target.name}' — calling TriggerCrush().");
                killEffect.TriggerCrush();
            }
            hasTriggered = true;
        }
        else
        {
            Debug.LogWarning($"[KillCube] Blade hit '{target.name}' (tagged '{playerTag}') but no KillEffect component found.");
        }
    }
}
