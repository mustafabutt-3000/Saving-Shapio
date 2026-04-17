using UnityEngine;

public class DestroyShape : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            DashScript dash = collision.collider.GetComponent<DashScript>();
            if (dash != null && dash.IsDashing)
            {
                Destroy(gameObject);
            }
        }
    }
}
