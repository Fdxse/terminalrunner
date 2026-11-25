using System.Globalization;

namespace TerminalRunner;

/// <summary>
/// Settings controlled from the command line.
/// </summary>
internal sealed class AppOptions
{
    /// <summary>Toggle the console into fullscreen right after start.</summary>
    public bool StartFullscreen { get; private set; }

    /// <summary>Output speed multiplier. 1.0 is normal, 2.0 is twice as fast.</summary>
    public double Speed { get; private set; } = 1.0;

    /// <summary>Optional seed for the random generator (gives the same output every run).</summary>
    public int? Seed { get; private set; }

    /// <summary>Show the fake boot sequence before the first prompt.</summary>
    public bool ShowBootSequence { get; private set; } = true;

    /// <summary>
    /// Periodically send a harmless F15 key press. This resets the Windows idle timer,
    /// which stops screen savers and auto-lock in addition to display sleep.
    /// </summary>
    public bool SendKeepAliveKeystroke { get; private set; } = true;

    /// <summary>Print usage and exit.</summary>
    public bool ShowHelp { get; private set; }

    /// <summary>Lowest and highest allowed speed multipliers.</summary>
    public const double MinimumSpeed = 0.1;
    public const double MaximumSpeed = 10.0;

    /// <summary>
    /// Parses the command line arguments. Throws <see cref="ArgumentException"/> on invalid input.
    /// </summary>
    public static AppOptions Parse(string[] args)
    {
        var options = new AppOptions();

        for (int argumentIndex = 0; argumentIndex < args.Length; argumentIndex++)
        {
            string argument = args[argumentIndex].ToLowerInvariant();

            switch (argument)
            {
                case "-f":
                case "--fullscreen":
                    options.StartFullscreen = true;
                    break;

                case "-s":
                case "--speed":
                    string speedText = ReadValue(args, ref argumentIndex, argument);
                    if (!double.TryParse(speedText, NumberStyles.Float, CultureInfo.InvariantCulture, out double speed)
                        || speed < MinimumSpeed || speed > MaximumSpeed)
                    {
                        throw new ArgumentException(
                            $"Invalid speed '{speedText}'. Use a number between {MinimumSpeed} and {MaximumSpeed}, e.g. 1.5");
                    }
                    options.Speed = speed;
                    break;

                case "--seed":
                    string seedText = ReadValue(args, ref argumentIndex, argument);
                    if (!int.TryParse(seedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int seed))
                    {
                        throw new ArgumentException($"Invalid seed '{seedText}'. Use a whole number.");
                    }
                    options.Seed = seed;
                    break;

                case "--no-boot":
                    options.ShowBootSequence = false;
                    break;

                case "--no-keypress":
                    options.SendKeepAliveKeystroke = false;
                    break;

                case "-h":
                case "-?":
                case "/?":
                case "--help":
                    options.ShowHelp = true;
                    break;

                default:
                    throw new ArgumentException($"Unknown option '{args[argumentIndex]}'.");
            }
        }

        return options;
    }

    /// <summary>
    /// Returns the value that follows an option such as "--speed 2", advancing the index past it.
    /// </summary>
    private static string ReadValue(string[] args, ref int argumentIndex, string optionName)
    {
        if (argumentIndex + 1 >= args.Length)
        {
            throw new ArgumentException($"Option '{optionName}' needs a value.");
        }

        argumentIndex++;
        return args[argumentIndex];
    }

    /// <summary>Writes the help text.</summary>
    public static void PrintUsage(TextWriter output)
    {
        output.WriteLine("TerminalRunner - prints endless, realistic looking Linux terminal activity.");
        output.WriteLine();
        output.WriteLine("Usage: TerminalRunner [options]");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -f, --fullscreen     Start in fullscreen (sends Alt+Enter to the console window)");
        output.WriteLine("  -s, --speed <n>      Speed multiplier, 0.1 - 10 (default 1)");
        output.WriteLine("      --seed <n>       Fixed random seed for reproducible output");
        output.WriteLine("      --no-boot        Skip the fake boot sequence at start");
        output.WriteLine("      --no-keypress    Do not send the periodic F15 keep-alive key press");
        output.WriteLine("  -h, --help           Show this help");
        output.WriteLine();
        output.WriteLine("Keys while running:");
        output.WriteLine("  F or F11             Toggle fullscreen / windowed");
        output.WriteLine("  Space or P           Pause / resume output");
        output.WriteLine("  + / -                Faster / slower");
        output.WriteLine("  Q, Esc or Ctrl+C     Quit");
    }
}
