using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SceneChange : MonoBehaviour
{
    [Header("Fade / Canvas")]
    [Tooltip("Optional: assign the Canvas GameObject that contains the fullscreen Image (preferred).")]
    public GameObject canvasToPersist;

    [Tooltip("Optional: assign the fullscreen UI Image used for fading. If you pass only the Image, the script will use its GameObject as the persistent canvas.")]
    public Image fadeImage;

    [Header("Event System")]
    [Tooltip("Optional: assign the EventSystem GameObject you want to carry across scenes (to avoid duplicates).")]
    public GameObject eventSystemToPersist;

    [Header("Scene Naming")]
    [Tooltip("Prefix used for level scenes, e.g. 'Lvl-' so levels are Lvl-1, Lvl-2, ...")]
    public string levelPrefix = "Lvl-";

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;

    // internal state
    private bool isTransitioning = false;
    private GameObject persistentCanvasObj;
    private GameObject persistentEventSystemObj;

    private void Start()
    {
        // Determine what we will preserve across scenes:
        if (canvasToPersist != null)
            persistentCanvasObj = canvasToPersist;
        else if (fadeImage != null)
            persistentCanvasObj = fadeImage.gameObject;

        persistentEventSystemObj = eventSystemToPersist;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTransitioning) return;
        if (!other.CompareTag("Player")) return;

        if (fadeImage == null)
        {
            Debug.LogWarning("[SceneChange] No fadeImage assigned. Assign a fullscreen UI Image for fading.");
            return;
        }

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;

        // Ensure persistent objects are not destroyed on load
        DontDestroyOnLoad(gameObject);
        if (persistentCanvasObj != null)
        {
            DontDestroyOnLoad(persistentCanvasObj);
        }
        if (persistentEventSystemObj != null)
        {
            DontDestroyOnLoad(persistentEventSystemObj);
        }

        // Ensure fadeImage starts at alpha 0
        SetImageAlpha(fadeImage, 0f);

        // Fade out (0 -> 1)
        yield return StartCoroutine(FadeImage(0f, 1f, fadeDuration));

        // Determine next scene name
        string currentScene = SceneManager.GetActiveScene().name;
        string nextScene = ComputeNextSceneName(currentScene);

        // If the computed next scene doesn't exist in build, fallback to reloading current
        if (!SceneExistsInBuild(nextScene))
        {
            Debug.Log($"[SceneChange] Next scene '{nextScene}' not found in build settings. Reloading current scene '{currentScene}'.");
            nextScene = currentScene;
        }
        else
        {
            Debug.Log($"[SceneChange] Loading next scene: {nextScene}");
        }


        // Register to handle after-load maintenance (like event system cleanup)
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Load scene synchronously (safe because screen is black). Use async if you prefer.
        SceneManager.LoadScene(nextScene);

        // Wait one frame to ensure sceneLoaded runs, then proceed to fade-in in OnSceneLoaded
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Remove listener immediately
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // If we have a persistent EventSystem, remove any other EventSystem instances in the scene
        if (persistentEventSystemObj != null)
        {
            EventSystem[] systems = FindObjectsOfType<EventSystem>();
            foreach (EventSystem es in systems)
            {
                if (es.gameObject != persistentEventSystemObj)
                {
                    // destroy duplicates
                    Destroy(es.gameObject);
                }
            }
        }

        // If we didn't persist a canvas but there is one in the loaded scene, make sure our fadeImage reference is still valid.
        // (If persistentCanvasObj was used, fadeImage will still be valid because the whole canvas is not destroyed.)
        if (fadeImage == null && persistentCanvasObj != null)
        {
            fadeImage = persistentCanvasObj.GetComponentInChildren<Image>();
            if (fadeImage == null)
                Debug.LogWarning("[SceneChange] Could not find Image component on persistent canvas after scene load.");
        }

        // Now fade in (1 -> 0). Run a coroutine on a MonoBehaviour that is not destroyed (this one persists only for the duration of the call).
        // If the persistent canvas was set, the fadeImage exists across scenes so we can fade it.
        if (fadeImage != null)
        {
            StartCoroutine(FadeInAndFinish());
        }
        else
        {
            // nothing to fade in; end transition state
            isTransitioning = false;
        }
    }

    private IEnumerator FadeInAndFinish()
    {
        // make sure image alpha is exactly 1 before fading in
        SetImageAlpha(fadeImage, 1f);
        yield return StartCoroutine(FadeImage(1f, 0f, fadeDuration));

        // done
        isTransitioning = false;
    }

    private IEnumerator FadeImage(float from, float to, float duration)
    {
        float t = 0f;
        Color c = fadeImage.color;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // use unscaled so fade isn't affected by Time.timeScale
            float alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }
        c.a = to;
        fadeImage.color = c;
    }

    private void SetImageAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    private string ComputeNextSceneName(string currentScene)
    {
        // Expecting "Lvl-<N>" pattern. If it matches - increment number.
        // Otherwise fallback to current scene (will cause reload).
        if (string.IsNullOrEmpty(currentScene)) return currentScene;

        if (currentScene.StartsWith(levelPrefix))
        {
            string numPart = currentScene.Substring(levelPrefix.Length);
            if (int.TryParse(numPart, out int n))
            {
                int next = n + 1;
                return $"{levelPrefix}{next}";
            }
        }

        // fallback: return current scene (reload)
        return currentScene;
    }

    private bool SceneExistsInBuild(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}