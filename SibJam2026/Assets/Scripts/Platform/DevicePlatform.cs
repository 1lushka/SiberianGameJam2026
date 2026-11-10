// Stateless native/editor detection; Device.Application follows the active Device Simulator.
public static class DevicePlatform
{
    public enum Kind { Desktop, Mobile }
    public static Kind Current => IsMobile ? Kind.Mobile : Kind.Desktop;
    public static bool IsDesktop => !IsMobile;
    public static bool IsMobile
    {
        get
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!mobileBrowser.HasValue) mobileBrowser = DevicePlatform_IsMobileBrowser() != 0;
            return mobileBrowser.Value;
#else
            return UnityEngine.Device.Application.isMobilePlatform;
#endif
        }
    }
#if UNITY_WEBGL && !UNITY_EDITOR
    static bool? mobileBrowser;
    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern int DevicePlatform_IsMobileBrowser();
#endif
}
