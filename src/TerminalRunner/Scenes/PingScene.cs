using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// "ping -c N host" with per-packet timings and the statistics summary.
/// </summary>
internal sealed class PingScene : Scene
{
    public PingScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override int Weight => 6;

    public override void Run()
    {
        string target = Random.Pick(FakeData.PingTargets);
        string targetAddress = char.IsDigit(target[0]) ? target : Random.PrivateIpAddress();
        int packetCount = Random.Next(6, 16);

        Shell.RunCommand($"ping -c {packetCount} {target}");
        Terminal.WriteLine($"PING {target} ({targetAddress}) 56(84) bytes of data.");

        double baseLatency = Random.Next(1, 40) + Random.NextDouble();
        var latencies = new List<double>();

        for (int sequenceNumber = 1; sequenceNumber <= packetCount; sequenceNumber++)
        {
            // Latency jitters around a base value.
            double latency = Math.Max(0.1, baseLatency + (Random.NextDouble() - 0.5) * baseLatency * 0.4);
            latencies.Add(latency);
            Terminal.WriteLine($"64 bytes from {targetAddress}: icmp_seq={sequenceNumber} ttl={Random.Pick([54, 57, 63, 64, 117, 118])} time={latency:0.00} ms");

            // ping sends one packet per second; we shorten that a bit.
            Terminal.PauseRandom(600, 1000);
        }

        double average = latencies.Average();
        double deviation = Math.Sqrt(latencies.Average(latency => Math.Pow(latency - average, 2)));

        Terminal.WriteLine();
        Terminal.WriteLine($"--- {target} ping statistics ---", TerminalWriter.Bright);
        Terminal.WriteLine($"{packetCount} packets transmitted, {packetCount} received, 0% packet loss, time {(packetCount - 1) * 1000 + Random.Next(1, 30)}ms");
        Terminal.WriteLine($"rtt min/avg/max/mdev = {latencies.Min():0.000}/{average:0.000}/{latencies.Max():0.000}/{deviation:0.000} ms");
    }
}
