using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// "sudo dmesg --follow": kernel messages trickling in on a long running machine.
/// </summary>
internal sealed class KernelLogScene : Scene
{
    public KernelLogScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override int Weight => 7;

    public override void Run()
    {
        Shell.RunCommand(Random.Chance(50) ? "sudo dmesg --follow" : "sudo dmesg | tail -n 60");

        // The machine has been up for a while, so timestamps are large.
        double secondsSinceBoot = Random.Next(20000, 900000) + Random.NextDouble();
        int messageCount = Random.Next(30, 60);

        for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
        {
            secondsSinceBoot += Random.NextDouble() * 3;
            Terminal.Write(FakeData.KernelTimestamp(secondsSinceBoot) + " ", TerminalWriter.Dim);
            Terminal.WriteLine(FakeData.KernelMessage(Random));
            Terminal.Pause(Random.Chance(15) ? Random.Next(400, 1400) : Random.Next(20, 140));
        }

        Shell.InterruptWithControlC();
    }
}
