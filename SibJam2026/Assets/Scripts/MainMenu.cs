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
    [SerializeField] private Button backButton;      // кнопка "Назад" внутри панели настроек

    [Header("Загружаемая сцена")]
    [SerializeField] private string gameSceneName = "Map";

    private void Start()
    {
        // Подписываемся на кнопки главного меню
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Кнопка "Назад" в панели настроек
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

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
            settingsPanel.SetActive(true);   // показываем панель настроек
    }

    private void OnBackClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);  // скрываем панель, возвращаемся в главное меню
    }

    private void OnQuitClicked()
    {
        Application.Quit();
        // В редакторе Unity это не закроет окно, но в билде сработает.
    }
}