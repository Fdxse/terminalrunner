namespace TerminalRunner.Platform;

/// <summary>
/// Keeps the screen (and the computer) awake while the app is running.
///
/// It uses SetThreadExecutionState, which is what video players use. Because that call
/// is per-thread, <see cref="Tick"/> must be called from the main thread, which the
/// <see cref="Terminal.TerminalWriter"/> does on every pause.
///
/// Optionally it also taps the F15 key once a minute. That resets the Windows idle timer,
/// so screen savers and "lock after X minutes" policies do not kick in either.
/// </summary>
internal sealed class PowerManager : IDisposable
{
    /// <summary>How often the execution state is re-applied (cheap, done just to be safe).</summary>
    private static readonly TimeSpan ExecutionStateRefreshInterval = TimeSpan.FromSeconds(30);

    /// <summary>How often the keep-alive F15 key press is sent.</summary>
    private static readonly TimeSpan KeepAliveKeystrokeInterval = TimeSpan.FromSeconds(59);

    private readonly bool sendKeepAliveKeystroke;
    private DateTime lastExecutionStateRefreshUtc = DateTime.MinValue;
    private DateTime lastKeepAliveKeystrokeUtc = DateTime.UtcNow;
    private bool isActive;

    public PowerManager(bool sendKeepAliveKeystroke)
    {
        this.sendKeepAliveKeystroke = sendKeepAliveKeystroke;
    }

    /// <summary>Starts blocking sleep. Does nothing on non-Windows systems.</summary>
    public void Start()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        isActive = true;
        ApplyExecutionState();
    }

    /// <summary>
    /// Called frequently from the main thread. Re-applies the execution state and
    /// sends the keep-alive key press when their intervals have passed.
    /// </summary>
    public void Tick()
    {
        if (!isActive || !OperatingSystem.IsWindows())
        {
            return;
        }

        DateTime nowUtc = DateTime.UtcNow;

        if (nowUtc - lastExecutionStateRefreshUtc >= ExecutionStateRefreshInterval)
        {
            ApplyExecutionState();
        }

        if (sendKeepAliveKeystroke && nowUtc - lastKeepAliveKeystrokeUtc >= KeepAliveKeystrokeInterval)
        {
            // F15 does nothing in virtually every application, but still counts as user input.
            NativeMethods.PressKeyCombination(NativeMethods.VirtualKeyF15);
            lastKeepAliveKeystrokeUtc = nowUtc;
        }
    }

    /// <summary>Tells Windows the display and system must stay on until further notice.</summary>
    private void ApplyExecutionState()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        NativeMethods.SetThreadExecutionState(
            NativeMethods.ExecutionStateContinuous
            | NativeMethods.ExecutionStateDisplayRequired
            | NativeMethods.ExecutionStateSystemRequired);

        lastExecutionStateRefreshUtc = DateTime.UtcNow;
    }

    /// <summary>Restores normal power behaviour.</summary>
    public void Dispose()
    {
        if (isActive && OperatingSystem.IsWindows())
        {
            // ES_CONTINUOUS alone clears the display/system requirements set earlier.
            NativeMethods.SetThreadExecutionState(NativeMethods.ExecutionStateContinuous);
        }

        isActive = false;
    }
}
