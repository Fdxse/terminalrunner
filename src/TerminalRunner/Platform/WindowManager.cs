namespace TerminalRunner.Platform;

/// <summary>
/// Switches the console between windowed and fullscreen.
///
/// Both hosts that run console apps on Windows use Alt+Enter for this:
///  - Windows Terminal (default on Windows 11): Alt+Enter and F11 toggle fullscreen.
///  - The classic console window (conhost): Alt+Enter toggles fullscreen.
/// So the simplest reliable approach is to send Alt+Enter to the focused window.
/// </summary>
internal sealed class WindowManager
{
    /// <summary>
    /// Toggles fullscreen. Returns false if the platform is not supported.
    /// The console window must have focus, which it does when the user pressed a key in it.
    /// </summary>
    public bool ToggleFullscreen()
    {
        if (!OperatingSystem.IsWindows())
        {
            // On Linux/macOS fullscreen is handled by the terminal emulator itself (usually F11).
            return false;
        }

        NativeMethods.PressKeyCombination(NativeMethods.VirtualKeyAlt, NativeMethods.VirtualKeyEnter);
        return true;
    }
}
