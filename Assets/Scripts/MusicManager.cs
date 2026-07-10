using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    public void Play(AudioClip clip)
    {
        if (audioSource.clip == clip && audioSource.isPlaying) return;
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }

    public bool IsPlaying() => audioSource.isPlaying;
    public static MusicManager GetInstance() => instance;
}
