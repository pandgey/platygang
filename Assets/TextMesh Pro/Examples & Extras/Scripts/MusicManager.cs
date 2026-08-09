using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip musicClip;
    [Range(0f, 1f)] public float volume = 0.5f;

    static MusicManager instance;
    AudioSource audioSource;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        audioSource.Play();
    }
}