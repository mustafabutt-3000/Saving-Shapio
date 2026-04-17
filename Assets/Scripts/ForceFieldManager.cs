using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ForceFieldObject
{
    public GameObject target;
    public float range = 5f;
}

public class ForceFieldManager : MonoBehaviour
{
    [Header("Force Field Objects")]
    public List<ForceFieldObject> repellers = new List<ForceFieldObject>();
    public List<ForceFieldObject> attractors = new List<ForceFieldObject>();

    [Header("Global Force Settings")]
    public float repellerForce = 15f;
    public float attractorForce = 12f;

    [Header("Toggles")]
    public bool repellersEnabled = true;
    public bool attractorsEnabled = true;
    public bool showRepellerRange = true;
    public bool showAttractorRange = true;

    [Header("Player Settings")]
    public string playerTag = "Player";

    [Header("Gizmo Colours")]
    public Color repellerGizmoColour = new Color(1f, 0.2f, 0.2f, 0.25f);
    public Color attractorGizmoColour = new Color(0.2f, 0.5f, 1f, 0.25f);

    private Rigidbody _playerRb;
    private Material _gizmoMaterial;

    void Start()
    {
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player == null) { enabled = false; return; }
        _playerRb = player.GetComponent<Rigidbody>();

        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader != null)
        {
            _gizmoMaterial = new Material(shader);
            _gizmoMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void FixedUpdate()
    {
        if (_playerRb == null) return;
        if (repellersEnabled) ApplyRepellers();
        if (attractorsEnabled) ApplyAttractors();
    }

    void ApplyRepellers()
    {
        Vector3 pPos = _playerRb.position;
        foreach (var item in repellers)
        {
            if (item.target == null) continue;
            float dist = Vector3.Distance(pPos, item.target.transform.position);
            if (dist >= item.range || dist <= 0.01f) continue;

            Vector3 awayDir = (pPos - item.target.transform.position).normalized;
            float t = 1f - (dist / item.range);
            _playerRb.AddForce(awayDir * repellerForce * (t * t), ForceMode.Force);
        }
    }

    void ApplyAttractors()
    {
        Vector3 pPos = _playerRb.position;
        foreach (var item in attractors)
        {
            if (item.target == null) continue;
            float dist = Vector3.Distance(pPos, item.target.transform.position);
            if (dist >= item.range || dist <= 0.01f) continue;

            Vector3 pullDir = (item.target.transform.position - pPos).normalized;
            float t = 1f - (dist / item.range);
            _playerRb.AddForce(pullDir * attractorForce * t, ForceMode.Force);
        }
    }

    void OnDrawGizmos()
    {
        if (showRepellerRange)
        {
            Gizmos.color = repellerGizmoColour;
            foreach (var item in repellers) if (item.target) Gizmos.DrawSphere(item.target.transform.position, item.range);
        }
        if (showAttractorRange)
        {
            Gizmos.color = attractorGizmoColour;
            foreach (var item in attractors) if (item.target) Gizmos.DrawSphere(item.target.transform.position, item.range);
        }
    }

    void OnRenderObject()
    {
        if (_gizmoMaterial == null) return;
        _gizmoMaterial.SetPass(0);
        if (showRepellerRange) foreach (var item in repellers) if (item.target) DrawGLCircles(item.target.transform.position, item.range, repellerGizmoColour);
        if (showAttractorRange) foreach (var item in attractors) if (item.target) DrawGLCircles(item.target.transform.position, item.range, attractorGizmoColour);
    }

    void DrawGLCircles(Vector3 centre, float radius, Color colour)
    {
        int segments = 24;
        float step = 360f / segments;
        GL.Begin(GL.LINES);
        GL.Color(colour);
        for (int i = 0; i < segments; i++)
        {
            float a0 = Mathf.Deg2Rad * (i * step);
            float a1 = Mathf.Deg2Rad * ((i + 1) * step);
            GL.Vertex(centre + new Vector3(Mathf.Cos(a0) * radius, 0, Mathf.Sin(a0) * radius));
            GL.Vertex(centre + new Vector3(Mathf.Cos(a1) * radius, 0, Mathf.Sin(a1) * radius));
            GL.Vertex(centre + new Vector3(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius, 0));
            GL.Vertex(centre + new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, 0));
        }
        GL.End();
    }
}