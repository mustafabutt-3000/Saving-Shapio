using System.Collections;
using UnityEngine;

public enum ProjectileType
{
    Curveball,      
    StopAndDrop,   
    ZigZag          
}

public class ProjectileController : MonoBehaviour
{
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public ProjectileType projectileType;

    public float speed = 6f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void Start()
    {
        switch (projectileType)
        {
            case ProjectileType.Curveball:
                StartCoroutine(DoCurveball());
                break;
            case ProjectileType.StopAndDrop:
                StartCoroutine(DoStopAndDrop());
                break;
            case ProjectileType.ZigZag:
                StartCoroutine(DoZigZag());
                break;
        }
    }


    IEnumerator DoCurveball()
    {
        float elapsed = 0f;
        float duration = 2f;
        Vector3 startPos = transform.position;

        Vector3 mid;
        int curveType = Random.Range(0, 3);

        if (curveType == 0)
        {
            mid = (startPos + targetPosition) / 2f + Vector3.up * 4f;
        }
        else if (curveType == 1)
        {
            mid = (startPos + targetPosition) / 2f + Vector3.left * 4f;
        }
        else
        {
            mid = (startPos + targetPosition) / 2f + Vector3.right * 4f;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Mathf.Pow(1 - t, 2) * startPos
                                + 2 * (1 - t) * t * mid
                                + Mathf.Pow(t, 2) * targetPosition;
            yield return null;
        }
        Destroy(gameObject);
    }

    IEnumerator DoStopAndDrop()
    {
        Vector3 abovePlayer = new Vector3(targetPosition.x, targetPosition.y + 5f, targetPosition.z);
        float flyDuration = 0.8f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;
            transform.position = Vector3.Lerp(startPos, abovePlayer, t);
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(0.6f);

        float dropDuration = 0.5f;
        elapsed = 0f;
        Vector3 dropStart = transform.position;
        Vector3 dropTarget = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            transform.position = Vector3.Lerp(dropStart, dropTarget, t);
            yield return null;
        }
        Destroy(gameObject);
    }

    IEnumerator DoZigZag()
    {
        float elapsed = 0f;
        float duration = 2.5f;
        float zigInterval = 0.3f;
        float zigTimer = 0f;
        int zigDir = 1;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            zigTimer += Time.deltaTime;

            if (zigTimer >= zigInterval)
            {
                zigDir *= -1;
                zigTimer = 0f;
            }

            Vector3 toPlayer = (targetPosition - transform.position).normalized;
            Vector3 zigOffset = Vector3.up * zigDir * 3f;
            rb.linearVelocity = (toPlayer * speed) + zigOffset;

            yield return null;
        }
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            Destroy(gameObject);
        }
        else if (!collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}