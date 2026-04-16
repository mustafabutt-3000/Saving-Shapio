using UnityEngine;

public class BladeCollision : MonoBehaviour
{
    private void Start()
    {
        // Find the player cube (assuming it's tagged "Player")
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Collider bladeCol = GetComponent<Collider>();
            Collider[] allCols = FindObjectsOfType<Collider>();

            // Ignore everything EXCEPT the player
            foreach (Collider col in allCols)
            {
                if (col != player.GetComponent<Collider>())
                {
                    Physics.IgnoreCollision(bladeCol, col);
                }
            }
        }
    }
}
