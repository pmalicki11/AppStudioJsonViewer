using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AppStudioJsonViewer.Services;

/// <summary>
/// Win32/DWM helpers to make a window's native title bar follow the dark theme.
/// WPF only themes the client area; the title bar is OS chrome and stays light
/// unless we opt in via DWM.
/// </summary>
public static class WindowTheming
{
    // Attribute id for immersive dark mode. It was 19 on Windows 10 1809 and
    // became 20 from 10 build 18985 onward (and on Windows 11).
    private const int DwmwaUseImmersiveDarkModePre20H1 = 19;
    private const int DwmwaUseImmersiveDarkMode = 20;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);

    /// <summary>
    /// Switches the native title bar to dark. Call once the HWND exists — e.g.
    /// from <see cref="Window.OnSourceInitialized"/>. No-op on OS versions that
    /// don't support the attribute.
    /// </summary>
    public static void UseDarkTitleBar(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == nint.Zero)
            return;

        int enabled = 1;
        // Try the current attribute id first; fall back to the older one so this
        // also works on early Windows 10 builds.
        if (DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref enabled, sizeof(int)) != 0)
            DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkModePre20H1, ref enabled, sizeof(int));
    }
}
