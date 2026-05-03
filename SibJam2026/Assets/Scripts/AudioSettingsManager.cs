using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    [Header("Audio Mixer (опционально)")]
    [SerializeField] private AudioMixer audioMixer;   // если используешь AudioMixer, перетащи сюда
    [SerializeField] private string soundVolumeParameter = "SoundVolume";
    [SerializeField] private string musicVolumeParameter = "MusicVolume";

    private const string SOUND_VOLUME_KEY = "SoundVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    private float soundVolume;
    private float musicVolume;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Загружаем сохранённые значения или ставим 1 (максимум)
        soundVolume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY, 1f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);

        ApplyVolumes();
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, soundVolume);
        PlayerPrefs.Save();
        ApplySoundVolume();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.Save();
        ApplyMusicVolume();
    }

    public float GetSoundVolume() => soundVolume;
    public float GetMusicVolume() => musicVolume;

    private void ApplyVolumes()
    {
        ApplySoundVolume();
        ApplyMusicVolume();
    }

    private void ApplySoundVolume()
    {
        if (audioMixer != null)
            audioMixer.SetFloat(soundVolumeParameter, VolumeToDecibels(soundVolume));
        else
            AudioListener.volume = soundVolume;
    }

    private void ApplyMusicVolume()
    {
        if (audioMixer != null)
            audioMixer.SetFloat(musicVolumeParameter, VolumeToDecibels(musicVolume));
        // Если нет микшера, громкость музыки обычно управляется через отдельный AudioSource
    }

    // Переводит линейную громкость (0..1) в децибелы для AudioMixer
    private float VolumeToDecibels(float volume)
    {
        if (volume <= 0.0001f) return -80f;
        return Mathf.Log10(volume) * 20f;
    }
}