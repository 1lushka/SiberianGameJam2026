using UnityEngine;

public class OneTimeCursorLock : MonoBehaviour
{
    [SerializeField] private bool lockAndHideCursor = true;
    [SerializeField] private bool lockOnStart = true;
    [SerializeField] private bool relockOnFocus = true;
    [SerializeField] private bool requireClickForWebGL = true;
    [SerializeField] private KeyCode relockKey = KeyCode.Mouse0;

    private bool hasRequestedInitialLock;

    private void Start()
    {
        if (lockOnStart)
            ApplyCursorState();
    }

    private void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (requireClickForWebGL && !hasRequestedInitialLock && Input.GetKeyDown(relockKey))
        {
            ApplyCursorState();
            return;
        }
#endif

        if (Cursor.lockState != CursorLockMode.Locked && Input.GetKeyDown(relockKey))
            ApplyCursorState();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus || !relockOnFocus)
            return;

        ApplyCursorState();
    }

    public void ApplyCursorState()
    {
        if (lockAndHideCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        hasRequestedInitialLock = true;
    }
}
