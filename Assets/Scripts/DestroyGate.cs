using UnityEngine;

public class DestroyGate : MonoBehaviour
{
    public GameObject GameObjectToDestroy;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GameObjectToDestroy == null)
        {
            Destroy(gameObject);
        }
    }
}
