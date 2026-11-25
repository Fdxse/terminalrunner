using System.Globalization;
using TerminalRunner.Platform;
using TerminalRunner.Scenes;
using TerminalRunner.Terminal;

namespace TerminalRunner;

/// <summary>
/// Entry point. Parses the command line, prepares the console, keeps the
/// screen awake and then lets the <see cref="SceneDirector"/> print fake
/// Linux activity until the user quits.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        // Linux tools print numbers with a dot ("12.34"), so never use the local (e.g. Swedish) decimal comma.
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        // Parse command line options first so bad input fails fast with a helpful message.
        AppOptions options;
        try
        {
            options = AppOptions.Parse(args);
        }
        catch (ArgumentException parseError)
        {
            Console.Error.WriteLine(parseError.Message);
            Console.Error.WriteLine();
            AppOptions.PrintUsage(Console.Error);
            return 1;
        }

        if (options.ShowHelp)
        {
            AppOptions.PrintUsage(Console.Out);
            return 0;
        }

        // A fixed seed makes the "random" output reproducible, which is handy for testing.
        Random random = options.Seed.HasValue ? new Random(options.Seed.Value) : new Random();

        // The power manager stops the display and the computer from going to sleep.
        using var powerManager = new PowerManager(options.SendKeepAliveKeystroke);
        var windowManager = new WindowManager();
        var terminal = new TerminalWriter(random, options.Speed, powerManager, windowManager);

        // Ctrl+C should exit cleanly (restore colors, release the sleep block) instead of killing the process.
        Console.CancelKeyPress += (sender, cancelEventArgs) =>
        {
            cancelEventArgs.Cancel = true;
            terminal.RequestQuit();
        };

        try
        {
            terminal.Initialize();
            powerManager.Start();

            if (options.StartFullscreen)
            {
                // Give the console window a moment to appear and get focus before toggling fullscreen.
                Thread.Sleep(400);
                windowManager.ToggleFullscreen();
            }

            var director = new SceneDirector(terminal, random, options);
            director.Run();
        }
        catch (QuitRequestedException)
        {
            // Normal way out: the user pressed Q, Esc or Ctrl+C.
        }
        finally
        {
            terminal.Restore();
        }

        return 0;
    }
}
