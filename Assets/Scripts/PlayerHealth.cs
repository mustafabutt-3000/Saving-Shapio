using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHits = 3;
    private int currentHits = 0;
    private bool isInvincible = false;

    public float invincibilityDuration = 1.5f;

    public void TakeDamage()
    {
        if (isInvincible) return;

        currentHits++;
        Debug.Log("Player hit! Hits taken: " + currentHits);

        if (currentHits >= maxHits)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityWindow());
        }
    }

    IEnumerator InvincibilityWindow()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Player is dead!");
        Destroy(gameObject);
    }
}