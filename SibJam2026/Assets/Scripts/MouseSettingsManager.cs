using UnityEngine;

public class MouseSettingsManager : MonoBehaviour
{
    public static MouseSettingsManager Instance { get; private set; }

    private const string INVERT_MOUSE_X_KEY = "InvertMouseX";
    private const string INVERT_MOUSE_Y_KEY = "InvertMouseY";

    private bool invertX;
    private bool invertY;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        invertX = PlayerPrefs.GetInt(INVERT_MOUSE_X_KEY, 0) == 1;
        invertY = PlayerPrefs.GetInt(INVERT_MOUSE_Y_KEY, 0) == 1;
    }

    public void SetInvertX(bool value)
    {
        invertX = value;
        PlayerPrefs.SetInt(INVERT_MOUSE_X_KEY, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetInvertY(bool value)
    {
        invertY = value;
        PlayerPrefs.SetInt(INVERT_MOUSE_Y_KEY, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsInvertX() => invertX;
    public bool IsInvertY() => invertY;
}