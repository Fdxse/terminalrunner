using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// A fake system start: kernel messages, systemd units starting, then a login and the MOTD.
/// Shown once when the app starts.
/// </summary>
internal sealed class BootScene : Scene
{
    public BootScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override void Run()
    {
        WriteKernelMessages();
        WriteSystemdStartup();
        WriteLoginAndMessageOfTheDay();
    }

    /// <summary>The first second or two of boot: a fast burst of kernel log lines.</summary>
    private void WriteKernelMessages()
    {
        double secondsSinceBoot = 0;

        Terminal.WriteLine($"{FakeData.KernelTimestamp(secondsSinceBoot)} Linux version 6.8.0-{Random.Next(40, 60)}-generic (buildd@lcy02-amd64-{Random.Next(10, 120):000}) (x86_64-linux-gnu-gcc-13 (Ubuntu 13.2.0-23ubuntu4) 13.2.0, GNU ld (GNU Binutils for Ubuntu) 2.42) #{Random.Next(40, 60)}-Ubuntu SMP PREEMPT_DYNAMIC", TerminalWriter.Primary);
        Terminal.WriteLine($"{FakeData.KernelTimestamp(secondsSinceBoot)} Command line: BOOT_IMAGE=/vmlinuz-6.8.0-generic root=UUID={Random.Uuid()} ro quiet splash", TerminalWriter.Primary);
        Terminal.Pause(200);

        int messageCount = Random.Next(45, 75);
        for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
        {
            secondsSinceBoot += Random.NextDouble() * 0.09;
            Terminal.Write(FakeData.KernelTimestamp(secondsSinceBoot) + " ", TerminalWriter.Dim);
            Terminal.WriteLine(FakeData.KernelMessage(Random));

            // Boot output comes in bursts with tiny gaps.
            Terminal.Pause(Random.Chance(8) ? Random.Next(150, 450) : Random.Next(8, 45));
        }
    }

    /// <summary>systemd's "[  OK  ] Started ..." lines.</summary>
    private void WriteSystemdStartup()
    {
        Terminal.WriteLine();
        Terminal.WriteSegments(("Welcome to ", TerminalWriter.Primary), ("Ubuntu 24.04.1 LTS", TerminalWriter.Bright), ("!", TerminalWriter.Primary));
        Terminal.WriteLine();
        Terminal.Pause(400);

        List<string> services = Random.PickDistinct(FakeData.SystemdServices, Random.Next(18, 30));
        List<string> targets = Random.PickDistinct(FakeData.SystemdTargets, Random.Next(6, 10));
        int targetIndex = 0;

        for (int serviceIndex = 0; serviceIndex < services.Count; serviceIndex++)
        {
            WriteSystemdStatus("Started", services[serviceIndex]);

            // Sprinkle "Reached target" lines in between the services.
            if (targetIndex < targets.Count && Random.Chance(30))
            {
                WriteSystemdStatus("Reached target", targets[targetIndex]);
                targetIndex++;
            }
        }

        WriteSystemdStatus("Reached target", "Multi-User System");
        Terminal.Pause(700);
    }

    /// <summary>Writes one systemd status line, nearly always "OK".</summary>
    private void WriteSystemdStatus(string verb, string unitDescription)
    {
        // Rarely a unit is slow to start, which real boots show as a yellow line first.
        if (Random.Chance(4))
        {
            Terminal.WriteSegments(("[ ", TerminalWriter.Muted), ("***", TerminalWriter.Warning), ("  ] ", TerminalWriter.Muted), ($"A start job is running for {unitDescription} ({Random.Next(1, 9)}s / 1min 30s)", TerminalWriter.Primary));
            Terminal.PauseRandom(600, 1500);
        }

        Terminal.WriteSegments(("[  ", TerminalWriter.Muted), ("OK", TerminalWriter.Primary), ("  ] ", TerminalWriter.Muted), ($"{verb} ", TerminalWriter.Primary), (unitDescription, TerminalWriter.Bright), (".", TerminalWriter.Primary));
        Terminal.PauseRandom(20, 160);
    }

    /// <summary>The console login and the Ubuntu "message of the day".</summary>
    private void WriteLoginAndMessageOfTheDay()
    {
        Terminal.ClearScreen();
        Terminal.WriteLine();
        Terminal.WriteLine($"Ubuntu 24.04.1 LTS {Shell.HostName} tty1", TerminalWriter.Primary);
        Terminal.WriteLine();
        Terminal.Write($"{Shell.HostName} login: ");
        Terminal.PauseRandom(600, 1200);
        Terminal.TypeText(Shell.UserName, TerminalWriter.Primary);
        Terminal.WriteLine();
        Terminal.Write("Password: ");
        Terminal.PauseRandom(1200, 2000);
        Terminal.WriteLine();
        Terminal.Pause(600);

        DateTime lastLogin = DateTime.Now.AddHours(-Random.Next(2, 60)).AddMinutes(-Random.Next(0, 60));
        Terminal.WriteLine("Welcome to Ubuntu 24.04.1 LTS (GNU/Linux 6.8.0-49-generic x86_64)", TerminalWriter.Primary);
        Terminal.WriteLine();
        Terminal.WriteLine(" * Documentation:  https://help.ubuntu.com", TerminalWriter.Dim);
        Terminal.WriteLine(" * Management:     https://landscape.canonical.com", TerminalWriter.Dim);
        Terminal.WriteLine(" * Support:        https://ubuntu.com/pro", TerminalWriter.Dim);
        Terminal.WriteLine();
        Terminal.WriteLine($" System information as of {DateTime.Now:ddd MMM d HH:mm:ss} UTC {DateTime.Now:yyyy}", TerminalWriter.Primary);
        Terminal.WriteLine();
        Terminal.WriteLine($"  System load:  {Random.NextDouble() * 2:0.00}              Processes:             {Random.Next(180, 420)}", TerminalWriter.Primary);
        Terminal.WriteLine($"  Usage of /:   {Random.Next(12, 78)}.{Random.Next(0, 10)}% of 467.89GB   Users logged in:       {Random.Next(0, 3)}", TerminalWriter.Primary);
        Terminal.WriteLine($"  Memory usage: {Random.Next(8, 70)}%               IPv4 address for eth0: {Random.PrivateIpAddress()}", TerminalWriter.Primary);
        Terminal.WriteLine($"  Swap usage:   {Random.Next(0, 5)}%", TerminalWriter.Primary);
        Terminal.WriteLine();
        Terminal.WriteLine($"{Random.Next(0, 40)} updates can be applied immediately.", TerminalWriter.Primary);
        Terminal.WriteLine("To see these additional updates run: apt list --upgradable", TerminalWriter.Primary);
        Terminal.WriteLine();
        Terminal.WriteLine($"Last login: {lastLogin:ddd MMM d HH:mm:ss yyyy} from {Random.PrivateIpAddress()}", TerminalWriter.Primary);
        Terminal.Pause(800);
    }
}
