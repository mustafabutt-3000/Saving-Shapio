using UnityEngine;
using System.Collections.Generic;

public class DestroyGate : MonoBehaviour
{
    // The specific object we are monitoring
    public GameObject GameObjectToDestroy;

    // A list of extra objects that should be destroyed when the main one is gone
    public List<GameObject> extraObjectsToDestroy;

    private bool triggered = false;

    void Update()
    {
        // If the main object is destroyed and we haven't triggered this yet
        if (GameObjectToDestroy == null && !triggered)
        {
            triggered = true;

            // Destroy the extra objects in the list
            foreach (GameObject obj in extraObjectsToDestroy)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            // Finally, destroy this gate object itself
            Destroy(gameObject);
        }
    }
}