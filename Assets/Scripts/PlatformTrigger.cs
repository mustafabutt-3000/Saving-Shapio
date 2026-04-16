using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public PlatformController controller;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            controller.TriggerPlatform(gameObject);
        }
    }
}