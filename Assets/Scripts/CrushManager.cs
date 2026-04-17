using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CrushData
{
    public GameObject originalObject;
    public GameObject crushedPrefab;
}

public class CrushManager : MonoBehaviour
{
    // Singleton instance to prevent duplicates
    public static CrushManager Instance;

    public List<CrushData> crushList;

    private Dictionary<GameObject, Pose> lastKnownPoses = new Dictionary<GameObject, Pose>();
    private int frameCounter = 0;

    void Awake()
    {
        // Singleton pattern: if an instance already exists, destroy this one
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This makes it persist
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        frameCounter++;

        if (frameCounter >= 30)
        {
            UpdateTrackedTransforms();
            frameCounter = 0;
        }

        CheckForDestructions();
    }

    private void UpdateTrackedTransforms()
    {
        foreach (var data in crushList)
        {
            if (data.originalObject != null)
            {
                lastKnownPoses[data.originalObject] = new Pose(
                    data.originalObject.transform.position,
                    data.originalObject.transform.rotation
                );
            }
        }
    }

    private void CheckForDestructions()
    {
        for (int i = crushList.Count - 1; i >= 0; i--)
        {
            var data = crushList[i];

            if (data.originalObject == null)
            {
                // We use the originalObject reference as the key here. 
                // Note: Even if the object is null, C# dictionary lookups 
                // for null keys can be tricky. We use a secondary check.
                if (lastKnownPoses.ContainsKey(data.originalObject))
                {
                    Pose lastPose = lastKnownPoses[data.originalObject];
                    Instantiate(data.crushedPrefab, lastPose.position, lastPose.rotation);

                    lastKnownPoses.Remove(data.originalObject);
                }

                crushList.RemoveAt(i);
            }
        }
    }
}