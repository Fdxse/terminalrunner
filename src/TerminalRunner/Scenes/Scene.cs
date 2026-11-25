using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// One "thing that happens" in the fake terminal, such as an apt install or a compile.
/// A scene types its own command(s) at the prompt and prints the output.
/// </summary>
internal abstract class Scene
{
    protected Scene(TerminalWriter terminal, ShellSession shell, Random random)
    {
        Terminal = terminal;
        Shell = shell;
        Random = random;
    }

    protected TerminalWriter Terminal { get; }

    protected ShellSession Shell { get; }

    protected Random Random { get; }

    /// <summary>
    /// Relative weight used when picking the next scene. Higher means more often.
    /// </summary>
    public virtual int Weight => 10;

    /// <summary>Runs the scene from the prompt until its output is finished.</summary>
    public abstract void Run();

    /// <summary>
    /// Draws an in-place progress indicator from 0 to 100 percent.
    /// The formatter turns the current percent into the full line of text.
    /// </summary>
    protected void ShowProgress(Func<int, string> formatLine, int totalMilliseconds, ConsoleColor color = TerminalWriter.Primary)
    {
        int currentPercent = 0;
        while (currentPercent < 100)
        {
            Terminal.OverwriteLine(formatLine(currentPercent), color);

            // Progress moves in uneven steps, like real downloads do.
            int step = Random.Next(1, 9);
            currentPercent = Math.Min(100, currentPercent + step);
            Terminal.Pause(Math.Max(5, totalMilliseconds * step / 100));
        }

        Terminal.OverwriteLine(formatLine(100), color);
        Terminal.WriteLine();
    }

    /// <summary>Builds a text progress bar such as "[#####.....]".</summary>
    protected static string ProgressBar(int percent, int barWidth, char filledCharacter = '#', char emptyCharacter = '.')
    {
        int filledCount = barWidth * percent / 100;
        return new string(filledCharacter, filledCount) + new string(emptyCharacter, barWidth - filledCount);
    }
}
