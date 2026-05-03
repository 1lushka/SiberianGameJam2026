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
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle invertMouseXToggle;
    [SerializeField] private Toggle invertMouseYToggle;

    [Header("Загружаемая сцена")]
    [SerializeField] private string gameSceneName = "Map";

    private void Start()
    {
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

        if (soundSlider != null) soundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (invertMouseXToggle != null) invertMouseXToggle.onValueChanged.AddListener(OnInvertMouseXChanged);
        if (invertMouseYToggle != null) invertMouseYToggle.onValueChanged.AddListener(OnInvertMouseYChanged);

        LoadSettings();
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnStartClicked() => SceneManager.LoadScene(gameSceneName);
    private void OnSettingsClicked() { if (settingsPanel != null) settingsPanel.SetActive(true); }
    private void OnBackClicked() { if (settingsPanel != null) settingsPanel.SetActive(false); }
    private void OnQuitClicked() => Application.Quit();

    private void OnSoundVolumeChanged(float value)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetSoundVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetMusicVolume(value);
    }

    private void OnInvertMouseXChanged(bool value)
    {
        if (MouseSettingsManager.Instance != null)
            MouseSettingsManager.Instance.SetInvertX(value);
    }

    private void OnInvertMouseYChanged(bool value)
    {
        if (MouseSettingsManager.Instance != null)
            MouseSettingsManager.Instance.SetInvertY(value);
    }

    private void LoadSettings()
    {
        if (AudioSettingsManager.Instance != null)
        {
            float soundVol = AudioSettingsManager.Instance.GetSoundVolume();
            float musicVol = AudioSettingsManager.Instance.GetMusicVolume();
            if (soundSlider != null) soundSlider.value = soundVol;
            if (musicSlider != null) musicSlider.value = musicVol;
        }
        if (MouseSettingsManager.Instance != null)
        {
            if (invertMouseXToggle != null) invertMouseXToggle.isOn = MouseSettingsManager.Instance.IsInvertX();
            if (invertMouseYToggle != null) invertMouseYToggle.isOn = MouseSettingsManager.Instance.IsInvertY();
        }
    }
}