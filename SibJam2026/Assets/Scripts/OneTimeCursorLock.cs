using UnityEngine;

public class OneTimeCursorLock : MonoBehaviour
{
    [SerializeField] private bool lockOnStart = true;
    [SerializeField] private bool relockOnFocus = true;
    [SerializeField] private bool requireClickForWebGL = true;
    [SerializeField] private KeyCode relockKey = KeyCode.Mouse0;

    private bool hasRequestedInitialLock;

    private void Start()
    {
        if (lockOnStart)
            TryLockCursor();
    }

    private void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (requireClickForWebGL && !hasRequestedInitialLock && Input.GetKeyDown(relockKey))
        {
            TryLockCursor();
            return;
        }
#endif

        if (Cursor.lockState != CursorLockMode.Locked && Input.GetKeyDown(relockKey))
            TryLockCursor();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus || !relockOnFocus)
            return;

        TryLockCursor();
    }

    public void TryLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        hasRequestedInitialLock = true;
    }
}
