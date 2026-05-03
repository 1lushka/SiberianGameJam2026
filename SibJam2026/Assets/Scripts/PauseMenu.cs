using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI панель паузы")]
    [SerializeField] private GameObject pausePanel;

    [Header("Кнопки")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button returnButton;

    [Header("Сцена, куда вернуться")]
    [SerializeField] private string returnSceneName = "Map";

    [Header("Отключаемые объекты")]
    [SerializeField] private GameObject playerObject;   // GameObject игрока
    [SerializeField] private GameObject cameraObject;   // GameObject камеры (если отдельный)

    private bool isPaused;

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
        if (returnButton != null)
            returnButton.onClick.AddListener(ReturnToMap);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    private void Pause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        // Отключаем объекты целиком
        if (playerObject != null)
            playerObject.SetActive(false);
        if (cameraObject != null)
            cameraObject.SetActive(false);

        // Показываем курсор
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Resume()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        // Включаем объекты обратно
        if (playerObject != null)
            playerObject.SetActive(true);
        if (cameraObject != null)
            cameraObject.SetActive(true);

        // Скрываем курсор
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ReturnToMap()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(returnSceneName);
    }
}