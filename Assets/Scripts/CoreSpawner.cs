using System.Collections;
using UnityEngine;

public class CoreSpawner : MonoBehaviour
{
    public GameObject corePrefab;

    private GameObject currentCore;
    private BossManager bossManager;

    void Start()
    {
        bossManager = FindFirstObjectByType<BossManager>();
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 7f));

            if (currentCore == null && !bossManager.IsDefeated)
            {
                currentCore = Instantiate(corePrefab, transform.position, Quaternion.identity);
                Core core = currentCore.GetComponent<Core>();
                if (core != null)
                    core.spawner = this;

                StartCoroutine(DespawnAfterTime(currentCore));
            }
        }
    }

    IEnumerator DespawnAfterTime(GameObject core)
    {
        yield return new WaitForSeconds(8f);

        if (core != null)
        {
            Destroy(core);
            currentCore = null;
        }
    }

    public void NotifyCoreDestroyed()
    {
        currentCore = null;
        bossManager.RegisterCoreDestroyed();
    }
}