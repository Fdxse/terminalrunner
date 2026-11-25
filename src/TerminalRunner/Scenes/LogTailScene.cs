using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// "tail -f" on an nginx access log: a stream of HTTP requests, stopped with Ctrl+C.
/// </summary>
internal sealed class LogTailScene : Scene
{
    public LogTailScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override void Run()
    {
        Shell.RunCommand("sudo tail -f /var/log/nginx/access.log");

        DateTime logTime = DateTime.Now.AddSeconds(-Random.Next(5, 60));
        int lineCount = Random.Next(40, 110);

        for (int logLineIndex = 0; logLineIndex < lineCount; logLineIndex++)
        {
            logTime = logTime.AddMilliseconds(Random.Next(5, 1500));
            int statusCode = PickStatusCode();
            string path = Random.Pick(FakeData.WebPaths)
                .Replace("{id}", Random.Next(1, 99999).ToString())
                .Replace("{hash}", Random.Hex(8));

            // Color the line by status: errors stand out, the rest stays green.
            ConsoleColor lineColor = statusCode >= 500 ? TerminalWriter.Error
                : statusCode >= 400 ? TerminalWriter.Warning
                : TerminalWriter.Primary;

            Terminal.WriteLine(
                $"{Random.PublicIpAddress()} - - [{logTime:dd/MMM/yyyy:HH:mm:ss} +0000] \"{Random.Pick(FakeData.HttpMethods)} {path} HTTP/1.1\" {statusCode} {Random.Next(0, 48000)} \"-\" \"{Random.Pick(FakeData.UserAgents)}\"",
                lineColor);

            // Traffic comes in bursts.
            Terminal.Pause(Random.Chance(20) ? Random.Next(300, 1500) : Random.Next(15, 180));
        }

        Shell.InterruptWithControlC();
    }

    /// <summary>Mostly 200s, with the realistic sprinkle of redirects and errors.</summary>
    private int PickStatusCode()
    {
        int roll = Random.Next(100);
        return roll switch
        {
            < 78 => 200,
            < 83 => 304,
            < 86 => 301,
            < 88 => 201,
            < 93 => 404,
            < 96 => 401,
            < 98 => 499,
            _ => Random.Pick([500, 502, 503, 504]),
        };
    }
}
