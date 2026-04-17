using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to an empty GameObject.
///
/// BLUE surfaces  →  attract the player + increase damping (harder to move through)
/// RED  surfaces  →  repel/reflect the player + slippery friction
///
/// Requirements:
///   • Player tagged "Player" with a Rigidbody.
///   • Surface GameObjects have a MeshRenderer — colours are set automatically.
/// Unity 6 compatible (linearDamping, linearVelocity, PhysicsMaterial).
/// </summary>
public class SurfaceForceManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  Inspector Lists
    // ─────────────────────────────────────────────────────────────

    [Header("Blue Surfaces  –  Attract")]
    public List<Collider> blueSurfaces = new List<Collider>();

    [Header("Red Surfaces  –  Repel")]
    public List<Collider> redSurfaces = new List<Collider>();

    // ─────────────────────────────────────────────────────────────
    //  Blue Settings
    // ─────────────────────────────────────────────────────────────

    [Header("Blue – Attraction Settings")]
    public float attractionRange  = 20f;
    public float attractionForce  = 35f;
    public float blueExtraDamping = 40f;

    // ─────────────────────────────────────────────────────────────
    //  Red Settings
    // ─────────────────────────────────────────────────────────────

    [Header("Red – Repulsion Settings")]
    public float repulsionRange               = 4f;
    public float repulsionForce               = 15f;
    public float reflectionImpulseMultiplier  = 0.1f;

    [Range(0f, 1f)]
    public float redSurfaceFriction = 0.02f;

    // ─────────────────────────────────────────────────────────────
    //  Colours
    // ─────────────────────────────────────────────────────────────

    [Header("Colours  (applied automatically on Start)")]
    public Color blueColour = new Color(0.18f, 0.52f, 1.00f);
    public Color redColour  = new Color(1.00f, 0.18f, 0.18f);

    // ─────────────────────────────────────────────────────────────
    //  Private state
    // ─────────────────────────────────────────────────────────────

    private Rigidbody _playerRb;
    private float     _originalDamping;

    // How many blue surfaces are currently attracting the player.
    // We only restore damping when this hits zero — fixes the
    // "slow between two platforms" bug.
    private int _activeBlueCount = 0;

    private Dictionary<Collider, PhysicsMaterial> _originalMaterials
        = new Dictionary<Collider, PhysicsMaterial>();

    private PhysicsMaterial _slipperyMat;

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[SurfaceForceManager] No GameObject tagged 'Player' found.");
            enabled = false;
            return;
        }

        _playerRb = player.GetComponent<Rigidbody>();
        if (_playerRb == null)
        {
            Debug.LogError("[SurfaceForceManager] Player has no Rigidbody.");
            enabled = false;
            return;
        }

        _originalDamping = _playerRb.linearDamping;

        _slipperyMat = new PhysicsMaterial("Red_Slippery_Runtime")
        {
            staticFriction  = redSurfaceFriction,
            dynamicFriction = redSurfaceFriction,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounciness      = 0f,
            bounceCombine   = PhysicsMaterialCombine.Minimum
        };

        ApplyColourToSurfaces(blueSurfaces, blueColour);
        ApplyColourAndFrictionToRed();
    }

    void FixedUpdate()
    {
        if (_playerRb == null) return;

        HandleBlueAttraction();
        HandleRedRepulsion();
    }

    void OnDestroy()
    {
        foreach (var pair in _originalMaterials)
            if (pair.Key != null)
                pair.Key.material = pair.Value;

        if (_slipperyMat != null) Destroy(_slipperyMat);
        if (_playerRb    != null) _playerRb.linearDamping = _originalDamping;
    }

    // ─────────────────────────────────────────────────────────────
    //  BLUE – Attraction
    // ─────────────────────────────────────────────────────────────

    void HandleBlueAttraction()
    {
        Vector3 playerPos      = _playerRb.position;
        int     blueThisFrame  = 0;   // count active blue surfaces this frame

        foreach (Collider col in blueSurfaces)
        {
            if (col == null) continue;

            Vector3 pt   = col.ClosestPoint(playerPos);
            float   dist = Vector3.Distance(playerPos, pt);

            if (dist >= attractionRange) continue;

            // ── Edge check ────────────────────────────────────────
            bool stillOn = false;

            // Method A: downward RaycastAll (ignores overhead platforms)
            Vector3      origin  = playerPos + Vector3.up * 0.15f;
            RaycastHit[] hits    = Physics.RaycastAll(origin, Vector3.down, attractionRange + 1f);
            foreach (RaycastHit h in hits)
                if (h.collider == col) { stillOn = true; break; }

            // Method B: toward-surface ray (catches walls & slopes)
            if (!stillOn)
            {
                Vector3 toSurface = (pt - playerPos).normalized;
                if (Physics.Raycast(playerPos, toSurface, out RaycastHit hB, attractionRange + 0.5f))
                    if (hB.collider == col) stillOn = true;
            }

            if (!stillOn) continue;

            // Apply pull
            Vector3 pullDir = (pt - playerPos).normalized;
            float   t       = 1f - (dist / attractionRange);
            _playerRb.AddForce(pullDir * attractionForce * t, ForceMode.Force);

            blueThisFrame++;
        }

        // ── Damping: only add extra when at least one blue is active,
        //    restore immediately the frame none are active.
        //    This fixes the slowdown that occurred between two platforms.
        _activeBlueCount = blueThisFrame;

        _playerRb.linearDamping = _activeBlueCount > 0
            ? _originalDamping + blueExtraDamping
            : _originalDamping;
    }

    // ─────────────────────────────────────────────────────────────
    //  RED – Repulsion + Reflection
    // ─────────────────────────────────────────────────────────────

    void HandleRedRepulsion()
    {
        Vector3 playerPos = _playerRb.position;

        foreach (Collider col in redSurfaces)
        {
            if (col == null) continue;

            Vector3 closest = col.ClosestPoint(playerPos);
            float   dist    = Vector3.Distance(playerPos, closest);

            if (dist >= repulsionRange) continue;

            Vector3 awayDir   = (playerPos - closest).normalized;
            float   proximity = 1f - (dist / repulsionRange);
            _playerRb.AddForce(awayDir * repulsionForce * proximity, ForceMode.Force);

            // Reflection: only fires when moving INTO the surface
            float approachDot = Vector3.Dot(_playerRb.linearVelocity.normalized, -awayDir);
            if (approachDot > 0.1f)
            {
                Vector3 reflected         = Vector3.Reflect(_playerRb.linearVelocity, awayDir);
                _playerRb.linearVelocity  = Vector3.zero;
                _playerRb.AddForce(reflected * reflectionImpulseMultiplier, ForceMode.Impulse);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Setup Helpers
    // ─────────────────────────────────────────────────────────────

    void ApplyColourToSurfaces(List<Collider> surfaces, Color colour)
    {
        foreach (Collider col in surfaces)
        {
            if (col == null) continue;
            Renderer rend = col.GetComponent<Renderer>()
                         ?? col.GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = colour;
        }
    }

    void ApplyColourAndFrictionToRed()
    {
        foreach (Collider col in redSurfaces)
        {
            if (col == null) continue;
            if (!_originalMaterials.ContainsKey(col))
                _originalMaterials[col] = col.sharedMaterial;
            col.material = _slipperyMat;
            Renderer rend = col.GetComponent<Renderer>()
                         ?? col.GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = redColour;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────────

    public void RegisterBlueSurface(Collider col)
    {
        if (col == null || blueSurfaces.Contains(col)) return;
        blueSurfaces.Add(col);
        Renderer rend = col.GetComponent<Renderer>()
                     ?? col.GetComponentInChildren<Renderer>();
        if (rend != null) rend.material.color = blueColour;
    }

    public void RegisterRedSurface(Collider col)
    {
        if (col == null || redSurfaces.Contains(col)) return;
        if (!_originalMaterials.ContainsKey(col))
            _originalMaterials[col] = col.sharedMaterial;
        col.material = _slipperyMat;
        redSurfaces.Add(col);
        Renderer rend = col.GetComponent<Renderer>()
                     ?? col.GetComponentInChildren<Renderer>();
        if (rend != null) rend.material.color = redColour;
    }
}