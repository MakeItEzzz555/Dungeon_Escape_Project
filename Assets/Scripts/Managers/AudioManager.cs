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
    [SerializeField] private AudioClip playerAttack1SFX;
    [SerializeField] private AudioClip playerAttack2SFX;
    [SerializeField] private AudioClip playerAttack3SFX;
    [SerializeField] private AudioClip enemySwordSwing1SFX;
    [SerializeField] private AudioClip enemySwordSwing2SFX;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.ApplyMissingClipReferencesFrom(this);
            Debug.Log($"[DEBUG_LOG] AudioManager: Duplicate detected on {gameObject.name}, destroying.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void ApplyMissingClipReferencesFrom(AudioManager source)
    {
        if (source == null) return;

        if (mainMenuMusic == null) mainMenuMusic = source.mainMenuMusic;
        if (gameplayMusic == null) gameplayMusic = source.gameplayMusic;
        if (deathSound == null) deathSound = source.deathSound;
        if (walkSound == null) walkSound = source.walkSound;
        if (openChestSound == null) openChestSound = source.openChestSound;
        if (coinCollectedSound == null) coinCollectedSound = source.coinCollectedSound;
        if (keyCollectedSound == null) keyCollectedSound = source.keyCollectedSound;
        if (interactSound == null) interactSound = source.interactSound;
        if (playerAttack1SFX == null) playerAttack1SFX = source.playerAttack1SFX;
        if (playerAttack2SFX == null) playerAttack2SFX = source.playerAttack2SFX;
        if (playerAttack3SFX == null) playerAttack3SFX = source.playerAttack3SFX;
        if (enemySwordSwing1SFX == null) enemySwordSwing1SFX = source.enemySwordSwing1SFX;
        if (enemySwordSwing2SFX == null) enemySwordSwing2SFX = source.enemySwordSwing2SFX;
    }

    private void Start()
    {
        // Automatically play Main Menu music if we are in the main menu scene,
        // or Gameplay music otherwise.
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main Menu")
        {
            PlayMainMenuMusic();
        }
        else
        {
            PlayGameplayMusic();
        }
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
    public void PlayPlayerAttack1() => PlaySFX(playerAttack1SFX);
    public void PlayPlayerAttack2() => PlaySFX(playerAttack2SFX);
    public void PlayPlayerAttack3() => PlaySFX(playerAttack3SFX);
    public void PlayEnemySwordSwing1() => PlaySFX(enemySwordSwing1SFX);
    public void PlayEnemySwordSwing2() => PlaySFX(enemySwordSwing2SFX);

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
