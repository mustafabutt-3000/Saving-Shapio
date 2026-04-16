using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [System.Serializable]
    public class Platform
    {
        public GameObject platformObject;

        [HideInInspector] public MeshRenderer[] meshes;
        [HideInInspector] public Collider[] colliders;
    }

    public List<Platform> platforms = new List<Platform>();

    [Header("Timings")]
    public float disableDelay = 2f;   // Time after touch to disappear
    public float respawnDelay = 3f;   // Time to come back

    private void Start()
    {
        foreach (var p in platforms)
        {
            if (p.platformObject != null)
            {
                p.meshes = p.platformObject.GetComponentsInChildren<MeshRenderer>();
                p.colliders = p.platformObject.GetComponentsInChildren<Collider>();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (var p in platforms)
            {
                if (collision.gameObject == collision.collider.gameObject)
                {
                    // Not needed, but keeping logic clear
                }
            }
        }
    }

    // Call this from each platform (see note below)
    public void TriggerPlatform(GameObject platformObj)
    {
        StartCoroutine(HandlePlatform(platformObj));
    }

    IEnumerator HandlePlatform(GameObject platformObj)
    {
        yield return new WaitForSeconds(disableDelay);

        platformObj.SetActive(false);

        yield return new WaitForSeconds(respawnDelay);

        platformObj.SetActive(true);
    }
}