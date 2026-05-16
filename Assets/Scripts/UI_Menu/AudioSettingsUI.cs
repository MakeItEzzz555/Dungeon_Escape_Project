using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        ConfigureSlider(musicSlider);
        ConfigureSlider(sfxSlider);
        SyncSlidersFromAudioManager();
        RegisterListeners();
    }

    private void OnDisable()
    {
        UnregisterListeners();
    }

    public void OnMusicSliderChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    public void OnSFXSliderChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
    }

    private void SyncSlidersFromAudioManager()
    {
        AudioManager audioManager = AudioManager.Instance;

        if (musicSlider != null)
        {
            float value = audioManager != null ? audioManager.GetMusicVolume() : 1f;
            musicSlider.SetValueWithoutNotify(value);
        }

        if (sfxSlider != null)
        {
            float value = audioManager != null ? audioManager.GetSFXVolume() : 1f;
            sfxSlider.SetValueWithoutNotify(value);
        }
    }

    private void RegisterListeners()
    {
        UnregisterListeners();

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    private void UnregisterListeners()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
        }
    }

    private static void ConfigureSlider(Slider slider)
    {
        if (slider == null) return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }
}
