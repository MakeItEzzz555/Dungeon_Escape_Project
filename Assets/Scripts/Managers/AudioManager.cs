using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string MainMenuSceneName = "Main Menu";
    private const string Level2SceneName = "Level 2";
    private const string Level3SceneName = "Level 3";

    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Header("Background Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip level2Music;
    [SerializeField] private AudioClip level3Music;

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
        LoadVolumeSettings();
        ApplyVolumeSettings();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void ApplyMissingClipReferencesFrom(AudioManager source)
    {
        if (source == null) return;

        if (mainMenuMusic == null) mainMenuMusic = source.mainMenuMusic;
        if (gameplayMusic == null) gameplayMusic = source.gameplayMusic;
        if (level2Music == null) level2Music = source.level2Music;
        if (level3Music == null) level3Music = source.level3Music;
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

    private void LoadVolumeSettings()
    {
        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SFXVolumeKey, 1f));
    }

    private void ApplyVolumeSettings()
    {
        ApplyMusicVolume();
        ApplySFXVolume();
    }

    private void ApplyMusicVolume()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    private void ApplySFXVolume()
    {
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    private void Start()
    {
        ApplyMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMusicForScene(scene.name);
    }

    private void ApplyMusicForScene(string sceneName)
    {
        if (sceneName == MainMenuSceneName)
        {
            PlayMainMenuMusic();
            return;
        }

        PlayGameplayMusicForScene(sceneName);
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
    public void PlayGameplayMusic() => PlayGameplayMusicForScene(SceneManager.GetActiveScene().name);

    private void PlayGameplayMusicForScene(string sceneName)
    {
        PlayMusic(GetGameplayMusicForScene(sceneName), true);
    }

    private AudioClip GetGameplayMusicForScene(string sceneName)
    {
        if (sceneName == Level2SceneName && level2Music != null) return level2Music;
        if (sceneName == Level3SceneName && level3Music != null) return level3Music;

        return gameplayMusic;
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyMusicVolume();
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplySFXVolume();
        PlayerPrefs.SetFloat(SFXVolumeKey, sfxVolume);
        PlayerPrefs.Save();
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

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
