using System.Collections;
using UnityEngine;

public class GlitchMaterialCycler : MonoBehaviour
{
    [System.Serializable]
    public class GlitchTarget
    {
        public Renderer targetRenderer;
        public Material[] materials;
    }

    [Header("Glitch Targets")]
    public GlitchTarget[] targets;

    [Header("Timing")]
    [Tooltip("Minimum time (seconds) between material switches")]
    public float minInterval = 0.03f;
    [Tooltip("Maximum time (seconds) between material switches")]
    public float maxInterval = 0.08f;

    [Header("Control")]
    public bool playOnStart = true;
    private bool isRunning = false;

    private int[] currentIndexes;
    private Coroutine glitchCoroutine;

    void Start()
    {
        currentIndexes = new int[targets.Length];

        if (playOnStart)
            StartGlitch();
    }

    public void StartGlitch()
    {
        if (isRunning) return;
        isRunning = true;
        glitchCoroutine = StartCoroutine(GlitchLoop());
    }

    public void StopGlitch()
    {
        if (!isRunning) return;
        isRunning = false;
        if (glitchCoroutine != null)
            StopCoroutine(glitchCoroutine);
    }

    public void ToggleGlitch()
    {
        if (isRunning) StopGlitch();
        else StartGlitch();
    }

    private IEnumerator GlitchLoop()
    {
        while (isRunning)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                GlitchTarget target = targets[i];

                if (target.targetRenderer == null || target.materials == null || target.materials.Length == 0)
                    continue;

                currentIndexes[i] = (currentIndexes[i] + 1) % target.materials.Length;
                target.targetRenderer.material = target.materials[currentIndexes[i]];
            }

            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
}