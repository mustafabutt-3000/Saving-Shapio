using System.Collections;
using UnityEngine;

public class RectangulusEnemy : MonoBehaviour
{
    [Header("Firing")]
    public GameObject projectilePrefab;
    public float fireRate = 3f;
    // Add this offset field
    [Tooltip("The offset from the enemy's transform position to spawn projectiles")]
    public Vector3 spawnOffset = new Vector3(0, 0, 0);

    private Transform player;
    private float fireTimer;
    private int shotIndex = 0;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            FireNextProjectile();
            fireTimer = 0f;
        }
    }

    void FireNextProjectile()
    {
        // Apply the offset to the spawn position
        // We transform the local offset to world space so it rotates with the enemy
        Vector3 spawnPos = transform.position + transform.TransformDirection(spawnOffset);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        ProjectileController pc = proj.GetComponent<ProjectileController>();
        if (pc == null) return;

        pc.targetPosition = player.position;
        pc.projectileType = (ProjectileType)(shotIndex % 3);
        shotIndex++;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DashScript dash = other.GetComponent<DashScript>();
            if (dash != null && dash.IsDashing)
                Destroy(gameObject);
        }
    }
}