namespace TerminalRunner.Terminal;

/// <summary>
/// Thrown from inside any pause when the user asks to quit. It unwinds whatever scene
/// is running straight back to <see cref="Program"/>, which then restores the console.
/// </summary>
internal sealed class QuitRequestedException : Exception
{
    public QuitRequestedException()
        : base("The user requested to quit.")
    {
    }
}
