using UnityEngine;
using Screen = UnityEngine.Device.Screen;

[ExecuteAlways]
public class MobileControls : MonoBehaviour
{
    public RectTransform safeArea;
    public MobileStick stick;
    public MobileJumpButton jump;
    public MobileLookField look;
    public Vector2 cameraSensitivity = new Vector2(.75f, .75f);
    public bool invertY;
    [Range(0, .9f)] public float deadZone = .15f;
    public static MobileControls Instance { get; private set; }
    public static bool Visible => Instance && Instance.Ready;
    public static bool SuppressMouse => Visible;
    bool focused = true, paused;
    Canvas canvas;
    Rect lastArea;
    Vector2 lastSize;
    bool ShowControls => DevicePlatform.IsMobile;
    bool Ready => ShowControls && isActiveAndEnabled && canvas && canvas.isActiveAndEnabled && safeArea.gameObject.activeInHierarchy && focused && !paused && Time.timeScale > 0;
    public static Vector2 Move => Visible && Instance.stick.isActiveAndEnabled ? Instance.stick.Value(Instance.deadZone) : Vector2.zero;
    public static bool Jump => Visible && Instance.jump.isActiveAndEnabled && Instance.jump.Held;
    public static Vector2 LookRate
    {
        get
        {
            if (!Visible || !Instance.look.isActiveAndEnabled || Time.deltaTime <= 0) return Vector2.zero;
            var v = Vector2.Scale(Instance.look.Delta, Instance.cameraSensitivity);
            v.y *= Instance.invertY ? 1 : -1;
            return v / Mathf.Max(1, Mathf.Min(Screen.width, Screen.height)) / Time.deltaTime;
        }
    }
    void OnEnable() { canvas = GetComponent<Canvas>(); if (Application.IsPlaying(gameObject)) Instance = this; }
    void Update()
    {
        if (!safeArea || !stick || !jump || !look) return;
        bool show = ShowControls;
#if UNITY_EDITOR
        // Keep the UI editable in Prefab Mode, while scene previews follow the device.
        show |= !Application.IsPlaying(gameObject) && UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null;
#endif
        if (Application.IsPlaying(gameObject)) show &= canvas && canvas.isActiveAndEnabled && focused && !paused && Time.timeScale > 0;
        if (safeArea.gameObject.activeSelf != show) safeArea.gameObject.SetActive(show);
        if (!show) return;
        if (Application.IsPlaying(gameObject) && ShowControls) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        ApplySafeArea();
    }
    public void ApplySafeArea() => ApplySafeArea(Screen.safeArea, new Vector2(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height)));
    public void ApplySafeArea(Rect area, Vector2 size)
    {
        if (!safeArea) return;
        if (Application.IsPlaying(gameObject) && (area != lastArea || size != lastSize)) Release();
        lastArea = area; lastSize = size;
        safeArea.anchorMin = area.min / size; safeArea.anchorMax = area.max / size;
        safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
    }
    void Release() { if (stick) stick.Release(); if (jump) jump.Release(); if (look) look.Release(); }
    void OnDisable() { Release(); if (Application.IsPlaying(gameObject) && safeArea) safeArea.gameObject.SetActive(false); if (Instance == this) Instance = null; }
    void OnApplicationFocus(bool value) { focused = value; if (!value) Release(); }
    void OnApplicationPause(bool value) { paused = value; if (value) Release(); }
}
