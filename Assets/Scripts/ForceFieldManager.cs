using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to any empty GameObject.
///
/// Manages two lists of force field objects:
///   • Repellers  — push the player away from their origin
///   • Attractors — pull the player toward their origin
///
/// Forces are NOT absolute — the player's own movement can work against them.
/// All ranges and forces are global (shared across all objects in each list).
/// Gizmos are visible in both Scene view and Game view.
///
/// Requirements:
///   • Player must have a Rigidbody and be tagged with the tag set in playerTag.
/// </summary>
public class ForceFieldManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  Lists
    // ─────────────────────────────────────────────────────────────

    [Header("Force Field Objects")]
    [Tooltip("Objects that push the player away from their position.")]
    public List<GameObject> repellers = new List<GameObject>();

    [Tooltip("Objects that pull the player toward their position.")]
    public List<GameObject> attractors = new List<GameObject>();

    // ─────────────────────────────────────────────────────────────
    //  Global Force Settings
    // ─────────────────────────────────────────────────────────────

    [Header("Global Force Settings")]

    [Tooltip("Radius within which repellers affect the player.")]
    public float repellerRange = 5f;

    [Tooltip("Strength of the repeller push force.\n" +
             "The player's own movement can overcome this.")]
    public float repellerForce = 15f;

    [Tooltip("Radius within which attractors affect the player.")]
    public float attractorRange = 6f;

    [Tooltip("Strength of the attractor pull force.\n" +
             "The player's own movement can overcome this.")]
    public float attractorForce = 12f;

    // ─────────────────────────────────────────────────────────────
    //  Toggles
    // ─────────────────────────────────────────────────────────────

    [Header("Toggles")]

    [Tooltip("Master toggle for ALL repeller forces.\n" +
             "Uncheck to disable all repellers at once.")]
    public bool repellersEnabled = true;

    [Tooltip("Master toggle for ALL attractor forces.\n" +
             "Uncheck to disable all attractors at once.")]
    public bool attractorsEnabled = true;

    [Tooltip("Show or hide the repeller range spheres in Scene and Game view.")]
    public bool showRepellerRange = true;

    [Tooltip("Show or hide the attractor range spheres in Scene and Game view.")]
    public bool showAttractorRange = true;

    // ─────────────────────────────────────────────────────────────
    //  Player Tag
    // ─────────────────────────────────────────────────────────────

    [Header("Player Settings")]
    [Tooltip("Tag on the player object. Default is 'Player'.")]
    public string playerTag = "Player";

    // ─────────────────────────────────────────────────────────────
    //  Gizmo Colours
    // ─────────────────────────────────────────────────────────────

    [Header("Gizmo Colours")]
    public Color repellerGizmoColour = new Color(1f, 0.2f, 0.2f, 0.25f); // red, semi-transparent
    public Color attractorGizmoColour = new Color(0.2f, 0.5f, 1f, 0.25f); // blue, semi-transparent

    // ─────────────────────────────────────────────────────────────
    //  Private state
    // ─────────────────────────────────────────────────────────────

    private Rigidbody _playerRb;

    // GL material used to draw gizmos in Game view
    private Material _gizmoMaterial;

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        // Find player by tag
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player == null)
        {
            Debug.LogError($"[ForceFieldManager] No GameObject with tag '{playerTag}' found.");
            enabled = false;
            return;
        }

        _playerRb = player.GetComponent<Rigidbody>();
        if (_playerRb == null)
        {
            Debug.LogError("[ForceFieldManager] Player has no Rigidbody.");
            enabled = false;
            return;
        }

        // Build a simple unlit material for GL drawing in Game view
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader != null)
        {
            _gizmoMaterial = new Material(shader);
            _gizmoMaterial.hideFlags = HideFlags.HideAndDontSave;
            _gizmoMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _gizmoMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _gizmoMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _gizmoMaterial.SetInt("_ZWrite", 0);
        }
    }

    void FixedUpdate()
    {
        if (_playerRb == null) return;

        if (repellersEnabled) ApplyRepellers();
        if (attractorsEnabled) ApplyAttractors();
    }

    void OnDestroy()
    {
        if (_gizmoMaterial != null)
            Destroy(_gizmoMaterial);
    }

    // ─────────────────────────────────────────────────────────────
    //  Force Application
    // ─────────────────────────────────────────────────────────────

    void ApplyRepellers()
    {
        Vector3 playerPos = _playerRb.position;

        foreach (GameObject obj in repellers)
        {
            if (obj == null) continue;

            float dist = Vector3.Distance(playerPos, obj.transform.position);
            if (dist >= repellerRange || dist <= 0.01f) continue;

            // Direction away from repeller origin toward player
            Vector3 awayDir = (playerPos - obj.transform.position).normalized;

            // Quadratic ramp — force grows stronger closer to the origin
            // but never becomes absolute (player movement works against it)
            float t = 1f - (dist / repellerRange);
            float smoothT = t * t;

            _playerRb.AddForce(awayDir * repellerForce * smoothT, ForceMode.Force);
        }
    }

    void ApplyAttractors()
    {
        Vector3 playerPos = _playerRb.position;

        foreach (GameObject obj in attractors)
        {
            if (obj == null) continue;

            float dist = Vector3.Distance(playerPos, obj.transform.position);
            if (dist >= attractorRange || dist <= 0.01f) continue;

            // Direction from player toward attractor origin
            Vector3 pullDir = (obj.transform.position - playerPos).normalized;

            // Linear ramp — pull strengthens as player gets closer
            float t = 1f - (dist / attractorRange);

            _playerRb.AddForce(pullDir * attractorForce * t, ForceMode.Force);
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Scene View Gizmos
    //  OnDrawGizmos runs in the Scene view even outside Play mode,
    //  letting you see ranges while placing objects in the editor.
    // ─────────────────────────────────────────────────────────────

    void OnDrawGizmos()
    {
        // Repeller ranges — red wire spheres
        if (showRepellerRange && repellers != null)
        {
            foreach (GameObject obj in repellers)
            {
                if (obj == null) continue;

                // Filled transparent sphere
                Gizmos.color = repellerGizmoColour;
                Gizmos.DrawSphere(obj.transform.position, repellerRange);

                // Solid wire outline
                Gizmos.color = new Color(
                    repellerGizmoColour.r,
                    repellerGizmoColour.g,
                    repellerGizmoColour.b,
                    1f);
                Gizmos.DrawWireSphere(obj.transform.position, repellerRange);
            }
        }

        // Attractor ranges — blue wire spheres
        if (showAttractorRange && attractors != null)
        {
            foreach (GameObject obj in attractors)
            {
                if (obj == null) continue;

                Gizmos.color = attractorGizmoColour;
                Gizmos.DrawSphere(obj.transform.position, attractorRange);

                Gizmos.color = new Color(
                    attractorGizmoColour.r,
                    attractorGizmoColour.g,
                    attractorGizmoColour.b,
                    1f);
                Gizmos.DrawWireSphere(obj.transform.position, attractorRange);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Game View Gizmos
    //  OnRenderObject draws into the Game view camera at runtime
    //  using GL (immediate mode graphics) — Unity's gizmos don't
    //  appear in Game view, so we draw manually here.
    // ─────────────────────────────────────────────────────────────

    void OnRenderObject()
    {
        if (_gizmoMaterial == null) return;

        _gizmoMaterial.SetPass(0);

        if (showRepellerRange && repellers != null)
        {
            foreach (GameObject obj in repellers)
            {
                if (obj == null) continue;
                DrawGLCircles(obj.transform.position, repellerRange, repellerGizmoColour);
            }
        }

        if (showAttractorRange && attractors != null)
        {
            foreach (GameObject obj in attractors)
            {
                if (obj == null) continue;
                DrawGLCircles(obj.transform.position, attractorRange, attractorGizmoColour);
            }
        }
    }

    /// <summary>
    /// Draws three orthogonal wireframe circles (XY, XZ, YZ planes) at the
    /// given position and radius using GL lines — visible in Game view.
    /// </summary>
    void DrawGLCircles(Vector3 centre, float radius, Color colour)
    {
        int segments = 48;
        float step = 360f / segments;

        GL.Begin(GL.LINES);
        GL.Color(colour);

        for (int i = 0; i < segments; i++)
        {
            float a0 = Mathf.Deg2Rad * (i * step);
            float a1 = Mathf.Deg2Rad * ((i + 1) * step);

            // XZ plane (horizontal ring — most visible from above)
            GL.Vertex(centre + new Vector3(Mathf.Cos(a0) * radius, 0f, Mathf.Sin(a0) * radius));
            GL.Vertex(centre + new Vector3(Mathf.Cos(a1) * radius, 0f, Mathf.Sin(a1) * radius));

            // XY plane (vertical ring facing camera)
            GL.Vertex(centre + new Vector3(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius, 0f));
            GL.Vertex(centre + new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, 0f));

            // YZ plane (side ring)
            GL.Vertex(centre + new Vector3(0f, Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius));
            GL.Vertex(centre + new Vector3(0f, Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius));
        }

        GL.End();
    }

    // ─────────────────────────────────────────────────────────────
    //  Public API — call from other scripts if needed
    // ─────────────────────────────────────────────────────────────

    /// <summary>Toggle all repeller forces on or off at runtime.</summary>
    public void SetRepellersEnabled(bool state) => repellersEnabled = state;

    /// <summary>Toggle all attractor forces on or off at runtime.</summary>
    public void SetAttractorsEnabled(bool state) => attractorsEnabled = state;

    /// <summary>Toggle repeller range visualisation on or off at runtime.</summary>
    public void SetRepellerRangeVisible(bool state) => showRepellerRange = state;

    /// <summary>Toggle attractor range visualisation on or off at runtime.</summary>
    public void SetAttractorRangeVisible(bool state) => showAttractorRange = state;

    /// <summary>Add a repeller at runtime (e.g. spawned enemy).</summary>
    public void RegisterRepeller(GameObject obj)
    {
        if (obj != null && !repellers.Contains(obj))
            repellers.Add(obj);
    }

    /// <summary>Add an attractor at runtime.</summary>
    public void RegisterAttractor(GameObject obj)
    {
        if (obj != null && !attractors.Contains(obj))
            attractors.Add(obj);
    }
}