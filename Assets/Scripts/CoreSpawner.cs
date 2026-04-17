using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject corePrefab;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints;

    [Header("Timing")]
    public float minSpawnDelay = 3f;
    public float maxSpawnDelay = 7f;
    public float coreLiveTime = 8f;

    [Header("Destruction Target")]
    [Tooltip("This object is destroyed/replaced once the required cores are destroyed")]
    public GameObject objectToDestroy;
    [Tooltip("The 'crushed' version to spawn when the object is destroyed")]
    public GameObject crushedObjectToDestroyPrefab;
    [Tooltip("How many cores need to be destroyed to destroy the object above")]
    public int coresRequiredToDestroy = 5;

    private static int coresDestroyed = 0;
    private GameObject currentCore;
    private BossManager bossManager;

    void Start()
    {
        bossManager = FindFirstObjectByType<BossManager>();

        if (coresDestroyed >= coresRequiredToDestroy)
            DestroyObjectToDestroy();

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));

            if (currentCore == null && (bossManager == null || !bossManager.IsDefeated))
            {
                Transform spawnPoint = GetRandomSpawnPoint();
                if (spawnPoint == null) continue;

                currentCore = Instantiate(corePrefab, spawnPoint.position, Quaternion.identity);

                Core core = currentCore.GetComponent<Core>();
                if (core != null)
                    core.spawner = this;

                StartCoroutine(DespawnAfterTime(currentCore));
            }
        }
    }

    IEnumerator DespawnAfterTime(GameObject core)
    {
        yield return new WaitForSeconds(coreLiveTime);
        if (core != null)
        {
            Destroy(core);
            currentCore = null;
        }
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    public void NotifyCoreDestroyed()
    {
        currentCore = null;
        coresDestroyed++;

        if (coresDestroyed >= coresRequiredToDestroy)
            DestroyObjectToDestroy();

        if (bossManager != null)
            bossManager.RegisterCoreDestroyed();
    }

    private void DestroyObjectToDestroy()
    {
        if (objectToDestroy != null)
        {
            // Spawn the crushed version at the object's last position/rotation
            if (crushedObjectToDestroyPrefab != null)
            {
                Instantiate(crushedObjectToDestroyPrefab, objectToDestroy.transform.position, objectToDestroy.transform.rotation);
            }

            Destroy(objectToDestroy);
        }
    }

    public static void ResetCount()
    {
        coresDestroyed = 0;
    }
}