
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance
    {
        get;
        private set;
    }

    private AudioSource audioSource;

    private void Awake()
    {
        // Prevent duplicate music when scenes reload.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep music alive between scene changes.
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.spatialBlend = 0f;

        if (audioSource.clip != null &&
            !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}

