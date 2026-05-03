using UnityEngine;

public class ProgressDebugActions : MonoBehaviour
{
    [Header("Hotkeys")]
    [SerializeField] private KeyCode logProgressKey = KeyCode.P;
    [SerializeField] private string logProgressCharacter = "\u043F";
    [SerializeField] private KeyCode clearProgressKey = KeyCode.X;
    [SerializeField] private string clearProgressCharacter = "\u0445";

    private void Update()
    {
        if (WasHotkeyPressed(logProgressKey, logProgressCharacter))
            LogCompletedLevels();

        if (WasHotkeyPressed(clearProgressKey, clearProgressCharacter))
            ClearProgressSave();
    }

    public void LogCompletedLevels()
    {
        if (ProgressManager.Instance == null)
        {
            Debug.LogWarning("[ProgressDebugActions] ProgressManager.Instance is null.");
            return;
        }

        ProgressManager.Instance.LogDebugSummary();
    }

    public void ClearProgressSave()
    {
        if (ProgressManager.Instance == null)
        {
            Debug.LogWarning("[ProgressDebugActions] ProgressManager.Instance is null.");
            return;
        }

        ProgressManager.Instance.ClearAllProgress();
    }

    private bool WasHotkeyPressed(KeyCode keyCode, string character)
    {
        if (Input.GetKeyDown(keyCode))
            return true;

        if (string.IsNullOrWhiteSpace(character) || string.IsNullOrEmpty(Input.inputString))
            return false;

        string pressed = Input.inputString.ToLowerInvariant();
        string expected = character.ToLowerInvariant();
        return pressed == expected;
    }
}
