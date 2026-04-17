using System.Collections;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public int coresRequiredToDefeat = 5;
    public bool IsDefeated { get; private set; }

    private int coresDestroyed = 0;

    public void RegisterCoreDestroyed()
    {
        if (IsDefeated) return;

        coresDestroyed++;
        Debug.Log("Cores destroyed: " + coresDestroyed + "/" + coresRequiredToDefeat);

        if (coresDestroyed >= coresRequiredToDefeat)
        {
            DefeatBoss();
        }
    }

    void DefeatBoss()
    {
        IsDefeated = true;
        Debug.Log("Boss defeated!");

        // Shatter effect
        for (int i = 0; i < 8; i++)
        {
            GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fragment.transform.position = transform.position;
            fragment.transform.localScale = Vector3.one * 0.3f;
            Rigidbody fragRb = fragment.AddComponent<Rigidbody>();
            fragRb.AddForce(Random.insideUnitSphere * 6f, ForceMode.Impulse);
            Destroy(fragment, 2f);
        }

        Destroy(gameObject);
    }
}