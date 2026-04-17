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
        // Automatically add PlatformStick to all listed platforms
        // and pass the playerTag so each platform knows what to look for
        SetupPlatforms(xPlatforms);
        SetupPlatforms(yPlatforms);

        // Start movement coroutines
        foreach (var data in xPlatforms)
            StartCoroutine(MovePlatform(data, true));
        foreach (var data in yPlatforms)
            StartCoroutine(MovePlatform(data, false));
    }

    private void SetupPlatforms(List<MovingPlatformData> list)
    {
        foreach (var data in list)
        {
            if (data.platform == null) continue;

            // Add PlatformStick if not already present
            PlatformStick stick = data.platform.GetComponent<PlatformStick>();
            if (stick == null)
                stick = data.platform.AddComponent<PlatformStick>();

            // Pass the player tag down to the stick component
            stick.playerTag = playerTag;
        }
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

//  PlatformStick — auto-added to platforms by PlatformManager.
//  Do NOT attach this manually.
//
//  How it works:
//    1 OnCollisionEnter: when the Player lands on a platform, we parent
//      the player's transform to the platform. The player then inherits
//      the platform's movement for free — no manual position tracking needed.
//
//    2 OnCollisionExit: when the Player leaves, we un-parent so they move
//      freely again.
//
//    3 LateUpdate: parenting a non-uniform-scale object causes Unity to
//      distort the child's scale. We correct the player's world scale back
//      to (1,1,1) every frame while they are parented so the cube never
//      stretches or shrinks.
public class PlatformStick : MonoBehaviour
{
    // Set automatically by PlatformManager — do not change manually
    [HideInInspector] public string playerTag = "Player";

    // Cached reference to the player transform while they are on this platform
    private Transform _playerOnPlatform = null;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(playerTag)) return;

        Transform playerT = collision.transform;

        // Parent the player to this platform so they ride it automatically
        playerT.SetParent(transform);

        // Cache reference so LateUpdate can fix scale every frame
        _playerOnPlatform = playerT;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag(playerTag)) return;

        Transform playerT = collision.transform;

        // Un-parent so the player moves freely again
        playerT.SetParent(null);

        // Clear cached reference
        if (_playerOnPlatform == playerT)
            _playerOnPlatform = null;
    }

    private void LateUpdate()
    {
        // Correct the player's world scale back to (1,1,1) every frame
        // while they are parented to this platform.
        //
        // Why: when a parent has non-unit scale, Unity multiplies that
        // into the child's localScale, stretching the cube. Setting
        // localScale to the inverse of the parent's lossyScale
        // counteracts this exactly.
        if (_playerOnPlatform == null) return;

        Vector3 parentScale = transform.lossyScale;

        // Avoid division by zero on any axis
        if (parentScale.x == 0f || parentScale.y == 0f || parentScale.z == 0f) return;

        _playerOnPlatform.localScale = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f / parentScale.z
        );
    }
}