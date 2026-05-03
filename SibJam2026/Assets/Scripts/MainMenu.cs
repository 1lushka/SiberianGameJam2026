using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Кнопки главного меню")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Панель настроек")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button backButton;

    [Header("Элементы настроек")]
    [SerializeField] private Slider soundSlider;      // ползунок громкости звуков
    [SerializeField] private Slider musicSlider;      // ползунок громкости музыки
    [SerializeField] private Toggle invertMouseXToggle; // инверсия мыши по X
    [SerializeField] private Toggle invertMouseYToggle; // инверсия мыши по Y

    [Header("Загружаемая сцена")]
    [SerializeField] private string gameSceneName = "Map";

    // Ключи для PlayerPrefs
    private const string SOUND_VOLUME_KEY = "SoundVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string INVERT_MOUSE_X_KEY = "InvertMouseX";
    private const string INVERT_MOUSE_Y_KEY = "InvertMouseY";

    private void Start()
    {
        // Подписываемся на кнопки главного меню
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        // Подписываемся на изменения UI-элементов настроек
        if (soundSlider != null)
            soundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (invertMouseXToggle != null)
            invertMouseXToggle.onValueChanged.AddListener(OnInvertMouseXChanged);
        if (invertMouseYToggle != null)
            invertMouseYToggle.onValueChanged.AddListener(OnInvertMouseYChanged);

        // Загружаем сохранённые настройки (или значения по умолчанию)
        LoadSettings();

        // Панель настроек изначально скрыта
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnStartClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void OnBackClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnQuitClicked()
    {
        Application.Quit();
        // В редакторе Unity это не закроет окно, но в билде сработает.
    }

    // Обработчики изменения настроек
    private void OnSoundVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, value);
        PlayerPrefs.Save();
        ApplySoundVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        PlayerPrefs.Save();
        ApplyMusicVolume(value);
    }

    private void OnInvertMouseXChanged(bool value)
    {
        PlayerPrefs.SetInt(INVERT_MOUSE_X_KEY, value ? 1 : 0);
        PlayerPrefs.Save();
        ApplyInvertMouseX(value);
    }

    private void OnInvertMouseYChanged(bool value)
    {
        PlayerPrefs.SetInt(INVERT_MOUSE_Y_KEY, value ? 1 : 0);
        PlayerPrefs.Save();
        ApplyInvertMouseY(value);
    }

    // Загрузка сохранённых настроек и применение к UI и игровой системе
    private void LoadSettings()
    {
        float soundVolume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        bool invertX = PlayerPrefs.GetInt(INVERT_MOUSE_X_KEY, 0) == 1;
        bool invertY = PlayerPrefs.GetInt(INVERT_MOUSE_Y_KEY, 0) == 1;

        if (soundSlider != null) soundSlider.value = soundVolume;
        if (musicSlider != null) musicSlider.value = musicVolume;
        if (invertMouseXToggle != null) invertMouseXToggle.isOn = invertX;
        if (invertMouseYToggle != null) invertMouseYToggle.isOn = invertY;

        ApplySoundVolume(soundVolume);
        ApplyMusicVolume(musicVolume);
        ApplyInvertMouseX(invertX);
        ApplyInvertMouseY(invertY);
    }

    // Методы применения настроек (вызываются при изменении и загрузке)
    private void ApplySoundVolume(float volume)
    {
        // Здесь нужно реализовать логику установки громкости звуков.
        // Обычно используется AudioMixer, но для простоты можно положить настройку в статическую переменную.
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetSoundVolume(volume);
        else
        {
            // fallback: поищем AudioListener и применим ко всем AudioSource?
            // Для примера просто сохраняем громкость в PlayerPrefs и никак не применяем (так как нет своего AudioManager).
            // В вашем проекте должен быть общий контроллер звука.
        }
    }

    private void ApplyMusicVolume(float volume)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetMusicVolume(volume);
    }

    private void ApplyInvertMouseX(bool invert)
    {
        // Здесь нужно передать настройку в ваш скрипт управления камерой/взглядом.
        // Например, статическая переменная или вызов менеджера настроек.
        if (MouseSettingsManager.Instance != null)
            MouseSettingsManager.Instance.SetInvertX(invert);
        else
        {
            // простой fallback: сохраняем в статике, потом скрипт камеры может считать.
            PlayerPrefs.SetInt(INVERT_MOUSE_X_KEY, invert ? 1 : 0);
        }
    }

    private void ApplyInvertMouseY(bool invert)
    {
        if (MouseSettingsManager.Instance != null)
            MouseSettingsManager.Instance.SetInvertY(invert);
    }
}