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
    [Tooltip("This object is destroyed once the required number of cores are destroyed")]
    public GameObject objectToDestroy;
    [Tooltip("How many cores need to be destroyed to destroy the object above")]
    public int coresRequiredToDestroy = 5;

    // Static — survives scene reloads within the same play session, resets when game is closed
    private static int coresDestroyed = 0;

    private GameObject currentCore;
    private BossManager bossManager;

    void Start()
    {
        bossManager = FindFirstObjectByType<BossManager>();

        if (coresDestroyed >= coresRequiredToDestroy && objectToDestroy != null)
            Destroy(objectToDestroy);

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
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("[CoreSpawner] No spawn points assigned.");
            return null;
        }
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    public void NotifyCoreDestroyed()
    {
        currentCore = null;
        coresDestroyed++;

        if (coresDestroyed >= coresRequiredToDestroy && objectToDestroy != null)
            Destroy(objectToDestroy);

        if (bossManager != null)
            bossManager.RegisterCoreDestroyed();
    }

    // Call this from PlayerHealth when a full game restart happens (game over → Level-1)
    public static void ResetCount()
    {
        coresDestroyed = 0;
    }
}