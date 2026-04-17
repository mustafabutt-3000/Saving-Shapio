using UnityEngine;

public class Core : MonoBehaviour
{
    [HideInInspector] public CoreSpawner spawner;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DashScript dash = other.GetComponent<DashScript>();
            if (dash != null && dash.IsDashing)
            {
                spawner.NotifyCoreDestroyed();
                Destroy(gameObject);
            }
        }
    }
}