using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// A Rust "cargo build --release": crate downloads, then "Compiling ..." lines with
/// the right-aligned status word, then the "Finished" summary.
/// </summary>
internal sealed class CargoBuildScene : Scene
{
    public CargoBuildScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override void Run()
    {
        string projectName = Random.Pick(FakeData.ProjectNames);
        Shell.ChangeDirectory($"~/src/{projectName}");
        Shell.RunCommand("cargo build --release");

        List<string> crates = Random.PickDistinct(FakeData.RustCrates, Random.Next(30, FakeData.RustCrates.Length));
        var crateVersions = crates.ToDictionary(crateName => crateName, _ => Random.Version(2));

        // Some crates need downloading first.
        if (Random.Chance(50))
        {
            WriteCargoStatus("Updating", "crates.io index");
            Terminal.PauseRandom(800, 2000);
            WriteCargoStatus("Locking", $"{crates.Count + Random.Next(5, 40)} packages to latest compatible versions");
            List<string> downloads = crates.Take(Random.Next(5, 15)).ToList();
            WriteCargoStatus("Downloading", $"{downloads.Count} crates ...");
            foreach (string crateName in downloads)
            {
                WriteCargoStatus("Downloaded", $"{crateName} v{crateVersions[crateName]}");
                Terminal.PauseRandom(30, 200);
            }
        }

        foreach (string crateName in crates)
        {
            WriteCargoStatus("Compiling", $"{crateName} v{crateVersions[crateName]}");

            // Big crates like syn or tokio take noticeably longer.
            Terminal.Pause(Random.Chance(10) ? Random.Next(900, 2500) : Random.Next(60, 450));
        }

        WriteCargoStatus("Compiling", $"{projectName} v{Random.Version(1)} (/home/{Shell.UserName}/src/{projectName})");
        Terminal.PauseRandom(2500, 5000);

        // A warning in your own crate is practically a tradition.
        if (Random.Chance(40))
        {
            Terminal.WriteSegments(("warning", TerminalWriter.Warning), (": unused import: `std::collections::HashMap`", TerminalWriter.Bright));
            Terminal.WriteSegments((" --> ", TerminalWriter.Accent), ($"src/{Random.Pick(["main", "lib", "handlers", "config", "db"])}.rs:{Random.Next(1, 20)}:5", TerminalWriter.Primary));
            Terminal.WriteLine("  |", TerminalWriter.Accent);
            Terminal.WriteLine();
        }

        double elapsedSeconds = Random.Next(20, 140) + Random.NextDouble();
        WriteCargoStatus("Finished", $"`release` profile [optimized] target(s) in {elapsedSeconds:0.00}s");
    }

    /// <summary>Cargo prints a bold green status word right-aligned in 12 columns.</summary>
    private void WriteCargoStatus(string status, string details)
    {
        Terminal.WriteSegments((status.PadLeft(12) + " ", TerminalWriter.Primary), (details, TerminalWriter.Bright));
    }
}
