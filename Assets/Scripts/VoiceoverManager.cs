using UnityEngine;

public class VoiceoverManager : MonoBehaviour
{
    public AudioClip voiceoverClip;
    private AudioSource source;
    private const string VOICEOVER_KEY = "VoiceoverPlayed";

    void Start()
    {
        source = GetComponent<AudioSource>();
        source.loop = false;

        if (PlayerPrefs.GetInt(VOICEOVER_KEY, 0) == 0)
        {
            source.clip = voiceoverClip;
            source.PlayDelayed(0.5f);
            PlayerPrefs.SetInt(VOICEOVER_KEY, 1);
            PlayerPrefs.Save();
        }
    }
}
