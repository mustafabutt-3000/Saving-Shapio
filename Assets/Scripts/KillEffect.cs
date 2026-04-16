using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class KillEffect : MonoBehaviour
{
    [Header("Split Cube Prefab")]
    public GameObject cutCubePrefab;
    public GameObject CrushedCubePrefab;

    [Header("Separation Force Settings")]
    public float separationForce = 2f;
    public float separationRadius = 0.5f;

    [Header("Restart Settings")]
    public float restartDelay = 3.0f;

    private JumpScript jumpScript;

    void Start()
    {
        jumpScript = GetComponent<JumpScript>();
    }

    public void TriggerKill1Vertical()
    {
        Debug.Log("Triggering vertical kill");
        SpawnAndPush(Quaternion.Euler(transform.rotation.x, 90f, transform.rotation.z));
    }

    public void TriggerCrush()
    {
        Debug.Log("Triggering crush kill");
        GameObject crushedCube = Instantiate(CrushedCubePrefab, transform.position, transform.rotation);
        crushedCube.transform.localScale = transform.localScale;
        InitiateRestart();
    }

    private void SpawnAndPush(Quaternion rot)
    {
        GameObject cutCube = Instantiate(cutCubePrefab, transform.position, rot);
        cutCube.transform.localScale = transform.localScale;

        if (jumpScript != null && jumpScript.IsGrounded)
        {
            Rigidbody[] rbs = cutCube.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbs)
            {
                rb.AddExplosionForce(separationForce, transform.position, separationRadius, 0f, ForceMode.Impulse);
            }
        }

        InitiateRestart();
    }

    private void InitiateRestart()
    {
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.OnPlayerKilled();

        if (PlayerHealth.Instance == null || !IsGameOver())
        {
            GameObject loader = new GameObject("RestartTimer");
            loader.AddComponent<RestartHelper>().BeginTimer(restartDelay);
        }

        Destroy(gameObject);
    }

    private bool IsGameOver()
    {
        //  Updated for new material system
        return PlayerHealth.Instance != null &&
               PlayerHealth.Instance.IsGameOver();
    }
}

public class RestartHelper : MonoBehaviour
{
    public void BeginTimer(float delay)
    {
        StartCoroutine(RestartRoutine(delay));
    }

    private IEnumerator RestartRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}