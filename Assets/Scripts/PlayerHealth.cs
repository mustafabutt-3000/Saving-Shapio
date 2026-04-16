using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Reference Materials (6 → 1 intensity)")]
    [Tooltip("Assign 6 materials from highest (index 0) to lowest intensity (index 5)")]
    public List<Material> healthMaterials = new List<Material>();

    [Header("Target Material (the one used in scene)")]
    [Tooltip("Material that actually gets modified at runtime")]
    public Material targetMaterial;

    [Header("Game Over")]
    public GameObject gameOverCanvasPrefab;
    public string firstLevelSceneName = "Level-1";

    private int currentIndex = 0;
    private GameObject gameOverInstance;
    private bool isGameOver = false;

    private List<Color> emissionLevels = new List<Color>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CacheEmissionLevels();
        ApplyEmission();
    }

    void Update()
    {
        if (isGameOver && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RestartFromFirstLevel();
        }
    }

    public void OnPlayerKilled()
    {
        if (isGameOver) return;

        currentIndex++;

        if (currentIndex >= emissionLevels.Count)
        {
            TriggerGameOver();
            return;
        }

        ApplyEmission();
    }

    void CacheEmissionLevels()
    {
        emissionLevels.Clear();

        foreach (var mat in healthMaterials)
        {
            if (mat != null && mat.HasProperty("_EmissionColor"))
            {
                emissionLevels.Add(mat.GetColor("_EmissionColor"));
            }
        }
    }

    void ApplyEmission()
    {
        if (targetMaterial == null || emissionLevels.Count == 0) return;

        targetMaterial.EnableKeyword("_EMISSION");
        targetMaterial.SetColor("_EmissionColor", emissionLevels[Mathf.Clamp(currentIndex, 0, emissionLevels.Count - 1)]);
    }

    private void TriggerGameOver()
    {
        isGameOver = true;

        if (gameOverCanvasPrefab != null)
        {
            gameOverInstance = Instantiate(gameOverCanvasPrefab);
            DontDestroyOnLoad(gameOverInstance);
        }
    }

    private void RestartFromFirstLevel()
    {
        isGameOver = false;
        currentIndex = 0;

        ApplyEmission();

        if (gameOverInstance != null)
        {
            Destroy(gameOverInstance);
            gameOverInstance = null;
        }

        SceneManager.LoadScene(firstLevelSceneName);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}