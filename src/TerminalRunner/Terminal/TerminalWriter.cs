using System.Diagnostics;
using System.Text;
using TerminalRunner.Platform;

namespace TerminalRunner.Terminal;

/// <summary>
/// All console output goes through this class. It handles colors, typing effects,
/// in-place progress lines, and timed pauses. While pausing it also reads the keyboard
/// (pause, speed, fullscreen, quit) and keeps the power manager ticking.
/// </summary>
internal sealed class TerminalWriter
{
    // The "phosphor" palette: mostly green on black, with a few accents.
    public const ConsoleColor Primary = ConsoleColor.Green;
    public const ConsoleColor Dim = ConsoleColor.DarkGreen;
    public const ConsoleColor Bright = ConsoleColor.White;
    public const ConsoleColor Muted = ConsoleColor.Gray;
    public const ConsoleColor Accent = ConsoleColor.Cyan;
    public const ConsoleColor Warning = ConsoleColor.Yellow;
    public const ConsoleColor Error = ConsoleColor.Red;

    /// <summary>Default width/height used when the real console size cannot be read (e.g. redirected output).</summary>
    private const int FallbackWidth = 100;
    private const int FallbackHeight = 30;

    /// <summary>How long each sleep inside a pause is, so key presses are handled quickly.</summary>
    private const int InputPollIntervalMilliseconds = 25;

    private readonly Random random;
    private readonly PowerManager powerManager;
    private readonly WindowManager windowManager;

    private double speedMultiplier;
    private bool isPaused;
    private bool canReadKeys;
    private volatile bool quitRequested;

    private ConsoleColor originalForeground;
    private ConsoleColor originalBackground;

    public TerminalWriter(Random random, double speedMultiplier, PowerManager powerManager, WindowManager windowManager)
    {
        this.random = random;
        this.speedMultiplier = speedMultiplier;
        this.powerManager = powerManager;
        this.windowManager = windowManager;

        // Keyboard handling only works when we have a real console for input.
        canReadKeys = !Console.IsInputRedirected;
    }

    /// <summary>Current console width in columns (never less than 40).</summary>
    public int Width
    {
        get
        {
            try
            {
                int windowWidth = Console.WindowWidth;
                return windowWidth >= 40 ? windowWidth : FallbackWidth;
            }
            catch (IOException)
            {
                return FallbackWidth;
            }
        }
    }

    /// <summary>Current console height in rows (never less than 10).</summary>
    public int Height
    {
        get
        {
            try
            {
                int windowHeight = Console.WindowHeight;
                return windowHeight >= 10 ? windowHeight : FallbackHeight;
            }
            catch (IOException)
            {
                return FallbackHeight;
            }
        }
    }

    /// <summary>Prepares the console: UTF-8, black background, green text, hidden cursor.</summary>
    public void Initialize()
    {
        originalForeground = Console.ForegroundColor;
        originalBackground = Console.BackgroundColor;

        TryConsoleOperation(() => Console.OutputEncoding = Encoding.UTF8);
        TryConsoleOperation(() => Console.Title = "bash");

        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = Primary;

        // Clear paints the whole window with the current (black) background.
        TryConsoleOperation(Console.Clear);
        TryConsoleOperation(() => Console.CursorVisible = false);
    }

    /// <summary>Puts the console back the way we found it.</summary>
    public void Restore()
    {
        TryConsoleOperation(() =>
        {
            Console.ForegroundColor = originalForeground;
            Console.BackgroundColor = originalBackground;
            Console.WriteLine();
            Console.CursorVisible = true;
        });
    }

    /// <summary>Asks the app to quit at the next pause (used by the Ctrl+C handler).</summary>
    public void RequestQuit()
    {
        quitRequested = true;
    }

    /// <summary>Clears the screen, like the "clear" command.</summary>
    public void ClearScreen()
    {
        Console.BackgroundColor = ConsoleColor.Black;
        TryConsoleOperation(Console.Clear);
    }

    /// <summary>Writes text without a line break.</summary>
    public void Write(string text, ConsoleColor color = Primary)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
    }

    /// <summary>Writes text followed by a line break.</summary>
    public void WriteLine(string text = "", ConsoleColor color = Primary)
    {
        Write(text, color);
        Console.WriteLine();
    }

    /// <summary>Writes several differently colored pieces on one line, then a line break.</summary>
    public void WriteSegments(params (string Text, ConsoleColor Color)[] segments)
    {
        foreach ((string text, ConsoleColor color) in segments)
        {
            Write(text, color);
        }

        Console.WriteLine();
    }

    /// <summary>Writes a line and then pauses a random time, the bread and butter of every scene.</summary>
    public void WriteLineThenPause(string text, int minimumMilliseconds, int maximumMilliseconds, ConsoleColor color = Primary)
    {
        WriteLine(text, color);
        PauseRandom(minimumMilliseconds, maximumMilliseconds);
    }

    /// <summary>
    /// Replaces the current line (using carriage return). Used for progress bars and counters.
    /// The text is cut to the window width so it never wraps onto a new line.
    /// </summary>
    public void OverwriteLine(string text, ConsoleColor color = Primary)
    {
        int usableWidth = Width - 1;
        string fittedText = text.Length > usableWidth ? text[..usableWidth] : text.PadRight(usableWidth);
        Write("\r" + fittedText, color);
    }

    /// <summary>Types text one character at a time, like a human at the keyboard.</summary>
    public void TypeText(string text, ConsoleColor color = Bright)
    {
        foreach (char character in text)
        {
            Write(character.ToString(), color);

            // Most keys come quickly, but now and then the "typist" hesitates.
            int keystrokeDelay = random.Next(100) < 6 ? random.Next(180, 420) : random.Next(35, 115);
            Pause(keystrokeDelay);
        }
    }

    /// <summary>Pauses for a random time between the two limits (in milliseconds, before speed scaling).</summary>
    public void PauseRandom(int minimumMilliseconds, int maximumMilliseconds)
    {
        Pause(random.Next(minimumMilliseconds, maximumMilliseconds + 1));
    }

    /// <summary>
    /// Waits for the given time (divided by the speed multiplier) while handling key presses.
    /// This is also where the user can pause and where quitting is detected.
    /// </summary>
    public void Pause(int milliseconds)
    {
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            ProcessInput();
            powerManager.Tick();

            // While paused, just keep polling; the countdown does not progress.
            if (isPaused)
            {
                stopwatch.Restart();
                Thread.Sleep(InputPollIntervalMilliseconds);
                continue;
            }

            double scaledTarget = milliseconds / speedMultiplier;
            double remainingMilliseconds = scaledTarget - stopwatch.Elapsed.TotalMilliseconds;
            if (remainingMilliseconds <= 0)
            {
                return;
            }

            Thread.Sleep((int)Math.Ceiling(Math.Min(remainingMilliseconds, InputPollIntervalMilliseconds)));
        }
    }

    /// <summary>Reads any pending key presses and reacts to them.</summary>
    private void ProcessInput()
    {
        if (quitRequested)
        {
            throw new QuitRequestedException();
        }

        if (!canReadKeys)
        {
            return;
        }

        try
        {
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                HandleKey(keyInfo);
            }
        }
        catch (InvalidOperationException)
        {
            // Some hosts do not support reading keys; stop trying instead of failing.
            canReadKeys = false;
        }

        if (quitRequested)
        {
            throw new QuitRequestedException();
        }
    }

    /// <summary>Maps a key press to an action.</summary>
    private void HandleKey(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.Q:
            case ConsoleKey.Escape:
                quitRequested = true;
                return;

            case ConsoleKey.Spacebar:
            case ConsoleKey.P:
                isPaused = !isPaused;
                return;

            case ConsoleKey.F:
            case ConsoleKey.F11:
                windowManager.ToggleFullscreen();
                return;

            case ConsoleKey.Add:
            case ConsoleKey.OemPlus:
                ChangeSpeed(1.5);
                return;

            case ConsoleKey.Subtract:
            case ConsoleKey.OemMinus:
                ChangeSpeed(1 / 1.5);
                return;
        }

        // Keyboard layouts differ, so also check the typed character for + and -.
        if (keyInfo.KeyChar == '+')
        {
            ChangeSpeed(1.5);
        }
        else if (keyInfo.KeyChar == '-')
        {
            ChangeSpeed(1 / 1.5);
        }
    }

    /// <summary>Multiplies the speed by the factor, keeping it within the allowed range.</summary>
    private void ChangeSpeed(double factor)
    {
        speedMultiplier = Math.Clamp(speedMultiplier * factor, AppOptions.MinimumSpeed, AppOptions.MaximumSpeed);
    }

    /// <summary>
    /// Runs a console operation that may not be supported everywhere
    /// (for example when output is redirected) and ignores those failures.
    /// </summary>
    private static void TryConsoleOperation(Action operation)
    {
        try
        {
            operation();
        }
        catch (Exception exception) when (exception is IOException or PlatformNotSupportedException or InvalidOperationException)
        {
            // Not fatal: the app still works, it just looks slightly less polished.
        }
    }
}
