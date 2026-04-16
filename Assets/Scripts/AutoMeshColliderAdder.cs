using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AutoMeshColliderAdder : MonoBehaviour
{
    [Tooltip("Prefabs or GameObjects to exclude from getting a MeshCollider added.")]
    public List<GameObject> exceptions = new List<GameObject>();

    void Start()
    {
        StartCoroutine(AddCollidersNextFrame());
    }

    private IEnumerator AddCollidersNextFrame()
    {
        // Wait one frame for all objects to fully initialize
        yield return null;

        // Build a set of exception names for quick lookup
        HashSet<string> exceptionNames = new HashSet<string>();
        foreach (var exception in exceptions)
        {
            if (exception != null)
                exceptionNames.Add(exception.name);
        }

        // Find every object in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Skip inactive objects
            if (!obj.activeInHierarchy) continue;

            // Skip lights and cameras
            if (obj.GetComponent<Light>() != null || obj.GetComponent<Camera>() != null)
                continue;

            // Skip if it already has ANY collider
            if (obj.GetComponent<Collider>() != null)
                continue;

            // Skip exceptions — check by name (handles both prefab instances and scene objects)
            string objBaseName = obj.name.Replace("(Clone)", "").Trim();
            if (exceptionNames.Contains(objBaseName))
            {
                Debug.Log($"[AutoMeshColliderAdder] Skipping exception: {obj.name}");
                continue;
            }

            // If it has a MeshFilter (so it has a mesh), give it a MeshCollider
            if (obj.GetComponent<MeshFilter>() != null)
            {
                obj.AddComponent<MeshCollider>();
                Debug.Log("Added MeshCollider to: " + obj.name);
            }
        }
    }
}