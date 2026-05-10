using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip openChestSound;
    [SerializeField] private AudioClip coinCollectedSound;
    [SerializeField] private AudioClip keyCollectedSound;
    [SerializeField] private AudioClip interactSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------------- MUSIC ----------------
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlayMainMenuMusic() => PlayMusic(mainMenuMusic, true);
    public void PlayGameplayMusic() => PlayMusic(gameplayMusic, true);

    // ---------------- SFX ----------------
    public void PlayDeath() => PlaySFX(deathSound);
    public void PlayOpenChest() => PlaySFX(openChestSound);
    public void PlayCoin() => PlaySFX(coinCollectedSound);
    public void PlayKey() => PlaySFX(keyCollectedSound);
    public void PlayInteract() => PlaySFX(interactSound);

    public void PlayWalk() => PlaySFX(walkSound);

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}