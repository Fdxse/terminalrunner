using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TerminalRunner.Platform;

/// <summary>
/// Win32 functions used for sleep prevention and fullscreen toggling.
/// Only call these after checking <see cref="OperatingSystem.IsWindows"/>.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class NativeMethods
{
    // Flags for SetThreadExecutionState.
    // https://learn.microsoft.com/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate

    /// <summary>The state stays in effect until the next call that uses ES_CONTINUOUS.</summary>
    internal const uint ExecutionStateContinuous = 0x80000000;

    /// <summary>Keeps the system from going to sleep.</summary>
    internal const uint ExecutionStateSystemRequired = 0x00000001;

    /// <summary>Keeps the display from turning off.</summary>
    internal const uint ExecutionStateDisplayRequired = 0x00000002;

    // Virtual key codes used with keybd_event.
    internal const byte VirtualKeyAlt = 0x12;     // VK_MENU
    internal const byte VirtualKeyEnter = 0x0D;   // VK_RETURN
    internal const byte VirtualKeyF15 = 0x7E;     // VK_F15, exists in Windows but not on normal keyboards

    /// <summary>keybd_event flag meaning "the key is being released".</summary>
    private const uint KeyEventKeyUp = 0x0002;

    /// <summary>
    /// Tells Windows that this thread needs the display / system to stay on.
    /// Returns the previous state, or 0 on failure.
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern uint SetThreadExecutionState(uint executionStateFlags);

    /// <summary>Synthesizes a key stroke (old but simple API, still fully supported).</summary>
    [DllImport("user32.dll", EntryPoint = "keybd_event")]
    private static extern void KeyboardEvent(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

    /// <summary>
    /// Presses the given keys in order, then releases them in reverse order.
    /// Example: PressKeyCombination(Alt, Enter) sends Alt+Enter.
    /// </summary>
    internal static void PressKeyCombination(params byte[] virtualKeys)
    {
        // Press every key down in the order given.
        foreach (byte virtualKey in virtualKeys)
        {
            KeyboardEvent(virtualKey, 0, 0, UIntPtr.Zero);
        }

        // Release them in reverse so modifiers (like Alt) are released last.
        for (int keyIndex = virtualKeys.Length - 1; keyIndex >= 0; keyIndex--)
        {
            KeyboardEvent(virtualKeys[keyIndex], 0, KeyEventKeyUp, UIntPtr.Zero);
        }
    }
}
