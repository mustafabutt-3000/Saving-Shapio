using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private AudioSource audioSource;

    public AudioClip mainTrack;
    public AudioClip finalRoundTrack;


    private string[] finalRoundScenes = { "Level-5", "Level-5 1", "Level-6" };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        bool isFinalRound = System.Array.Exists(finalRoundScenes, s => s == sceneName);

        if (isFinalRound)
        {
            if (audioSource.clip != finalRoundTrack)
                SwitchToFinalRound();
        }
        else
        {
            if (audioSource.clip != mainTrack)
            {
                audioSource.clip = mainTrack;
                audioSource.Play();
            }
        }
    }

    public void SwitchToFinalRound()
    {
        StartCoroutine(CrossfadeToFinalRound());
    }

    private IEnumerator CrossfadeToFinalRound(float duration = 1.5f)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0f)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.clip = finalRoundTrack;
        audioSource.Play();

        while (audioSource.volume < startVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.volume = startVolume;
    }
}