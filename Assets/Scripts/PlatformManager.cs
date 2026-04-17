using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MovingPlatformData
{
    public GameObject platform;
    public float distance = 6f;
    public float duration = 3f;
}

public class PlatformManager : MonoBehaviour
{
    [Header("X-Axis Moving Platforms")]
    public List<MovingPlatformData> xPlatforms;

    [Header("Y-Axis Moving Platforms")]
    public List<MovingPlatformData> yPlatforms;

    [Header("Player Settings")]
    [Tooltip("The tag assigned to the player object. Default is 'Player'.\n" +
             "Change this if your module uses a different tag.")]
    public string playerTag = "Player";

    void Start()
    {

        // Start movement coroutines
        foreach (var data in xPlatforms)
            StartCoroutine(MovePlatform(data, true));
        foreach (var data in yPlatforms)
            StartCoroutine(MovePlatform(data, false));
    }

    private System.Collections.IEnumerator MovePlatform(MovingPlatformData data, bool isXAxis)
    {
        Vector3 startPos = data.platform.transform.position;
        float halfDuration = data.duration / 2f;
        while (true)
        {
            yield return MoveTo(data.platform, startPos, isXAxis, data.distance, halfDuration);
            yield return MoveTo(data.platform, startPos, isXAxis, 0f, halfDuration);
        }
    }

    private System.Collections.IEnumerator MoveTo(GameObject obj, Vector3 start, bool isX, float offset, float time)
    {
        float elapsed = 0f;
        Vector3 targetPos = start + (isX ? new Vector3(offset, 0, 0) : new Vector3(0, offset, 0));
        Vector3 initialPos = obj.transform.position;

        while (elapsed < time)
        {
            obj.transform.position = Vector3.Lerp(initialPos, targetPos, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.transform.position = targetPos;
    }
}