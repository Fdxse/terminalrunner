using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// "git clone" with the counting / compressing / receiving progress lines
/// that update in place, followed by a short "git log".
/// </summary>
internal sealed class GitCloneScene : Scene
{
    public GitCloneScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override void Run()
    {
        string projectName = Random.Pick(FakeData.ProjectNames);
        string repositoryUrl = $"https://{Random.Pick(FakeData.GitHosts)}/{Random.Pick(FakeData.GitOrganizations)}/{projectName}.git";

        Shell.ChangeDirectory("~/src");
        Shell.RunCommand($"git clone {repositoryUrl}");
        Terminal.WriteLine($"Cloning into '{projectName}'...");
        Terminal.PauseRandom(400, 1200);

        int objectCount = Random.Next(2000, 90000);
        int compressedCount = objectCount * Random.Next(40, 80) / 100;
        double totalMegabytes = objectCount * (Random.NextDouble() * 0.004 + 0.001);
        double speedMegabytesPerSecond = Random.Next(3, 40) + Random.NextDouble();

        Terminal.WriteLine($"remote: Enumerating objects: {objectCount}, done.");
        ShowProgress(percent => $"remote: Counting objects: {percent,3}% ({objectCount * percent / 100}/{objectCount}){(percent == 100 ? ", done." : "")}", Random.Next(300, 900));
        ShowProgress(percent => $"remote: Compressing objects: {percent,3}% ({compressedCount * percent / 100}/{compressedCount}){(percent == 100 ? ", done." : "")}", Random.Next(400, 1200));

        int reusedCount = objectCount - Random.Next(0, 500);
        Terminal.WriteLine($"remote: Total {objectCount} (delta {objectCount / 3}), reused {reusedCount} (delta {objectCount / 3 - Random.Next(0, 50)}), pack-reused {Random.Next(0, 9999)}");

        ShowProgress(percent =>
        {
            double receivedMegabytes = totalMegabytes * percent / 100;
            string suffix = percent == 100 ? ", done." : "";
            return $"Receiving objects: {percent,3}% ({objectCount * percent / 100}/{objectCount}), {receivedMegabytes:0.00} MiB | {speedMegabytesPerSecond:0.00} MiB/s{suffix}";
        }, (int)Math.Clamp(totalMegabytes / speedMegabytesPerSecond * 1000, 1500, 9000));

        int deltaCount = objectCount / 3;
        ShowProgress(percent => $"Resolving deltas: {percent,3}% ({deltaCount * percent / 100}/{deltaCount}){(percent == 100 ? ", done." : "")}", Random.Next(500, 1500));
        Terminal.PauseRandom(500, 1000);

        // Look around in the fresh clone.
        Shell.ChangeDirectory($"~/src/{projectName}");
        Shell.RunCommand("git log --oneline -n 8");
        string[] commitMessages =
        [
            "Fix race condition in connection pool shutdown",
            "Bump dependencies",
            "Add retry with exponential backoff for upstream calls",
            "Refactor config loading into separate module",
            "Merge pull request #{0} from feature/metrics-endpoint",
            "Improve error messages for invalid payloads",
            "Remove deprecated v1 handlers",
            "Add integration tests for batch import",
            "Cache compiled templates",
            "Update CI pipeline to use cached build layers",
            "Handle SIGTERM gracefully",
            "Document environment variables in README",
        ];

        List<string> pickedMessages = Random.PickDistinct(commitMessages, 8);
        for (int commitIndex = 0; commitIndex < pickedMessages.Count; commitIndex++)
        {
            string decoration = commitIndex == 0 ? " (HEAD -> main, origin/main, origin/HEAD)" : "";
            Terminal.WriteSegments(
                (Random.Hex(7), TerminalWriter.Warning),
                (decoration, TerminalWriter.Accent),
                (" " + string.Format(pickedMessages[commitIndex], Random.Next(100, 2000)), TerminalWriter.Primary));
            Terminal.PauseRandom(10, 60);
        }
    }
}
