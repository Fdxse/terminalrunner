namespace TerminalRunner.Terminal;

/// <summary>
/// Simulates the interactive bash shell: draws the prompt, "types" commands,
/// and tracks the current directory so cd commands look consistent.
/// </summary>
internal sealed class ShellSession
{
    private readonly TerminalWriter terminal;

    public ShellSession(TerminalWriter terminal, string userName, string hostName)
    {
        this.terminal = terminal;
        UserName = userName;
        HostName = hostName;
    }

    public string UserName { get; }

    public string HostName { get; }

    /// <summary>The directory shown in the prompt. "~" is the home directory.</summary>
    public string CurrentDirectory { get; private set; } = "~";

    /// <summary>Draws the familiar Ubuntu style prompt: user@host:~/dir$</summary>
    public void WritePrompt()
    {
        terminal.Write($"{UserName}@{HostName}", TerminalWriter.Primary);
        terminal.Write(":", TerminalWriter.Muted);
        terminal.Write(CurrentDirectory, TerminalWriter.Accent);
        terminal.Write("$ ", TerminalWriter.Muted);
    }

    /// <summary>
    /// Shows the prompt, waits a moment as if thinking, types the command and presses Enter.
    /// </summary>
    public void RunCommand(string command)
    {
        WritePrompt();
        terminal.PauseRandom(350, 1400);
        terminal.TypeText(command);
        terminal.PauseRandom(120, 450);
        terminal.WriteLine();
    }

    /// <summary>Types a cd command if we are not already in that directory.</summary>
    public void ChangeDirectory(string directory)
    {
        if (directory == CurrentDirectory)
        {
            return;
        }

        RunCommand("cd " + directory);
        CurrentDirectory = directory;
    }

    /// <summary>Shows an empty prompt and presses Enter, which people do all the time.</summary>
    public void PressEnterOnEmptyPrompt()
    {
        WritePrompt();
        terminal.PauseRandom(200, 900);
        terminal.WriteLine();
    }

    /// <summary>Simulates pressing Ctrl+C to stop a long running command.</summary>
    public void InterruptWithControlC()
    {
        terminal.PauseRandom(300, 900);
        terminal.WriteLine("^C", TerminalWriter.Muted);
    }
}
