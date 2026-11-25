using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// A pytest run: per-file rows of dots with percentages, then the colored summary bar.
/// </summary>
internal sealed class TestRunScene : Scene
{
    public TestRunScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override void Run()
    {
        string projectName = Random.Pick(FakeData.ProjectNames);
        Shell.ChangeDirectory($"~/src/{projectName}");
        Shell.RunCommand("python -m pytest -q --maxfail=5");

        List<string> testFiles = Random.PickDistinct(FakeData.PythonTestModules, Random.Next(5, FakeData.PythonTestModules.Length));
        var testCountPerFile = testFiles.ToDictionary(testFile => testFile, _ => Random.Next(4, 40));
        int totalTests = testCountPerFile.Values.Sum();

        Terminal.WriteLine(new string('=', 30) + " test session starts " + new string('=', 30), TerminalWriter.Bright);
        Terminal.WriteLine($"platform linux -- Python 3.12.{Random.Next(1, 8)}, pytest-8.3.{Random.Next(1, 5)}, pluggy-1.5.0", TerminalWriter.Primary);
        Terminal.WriteLine($"rootdir: /home/{Shell.UserName}/src/{projectName}");
        Terminal.WriteLine("plugins: cov-5.0.0, asyncio-0.24.0, xdist-3.6.1");
        Terminal.WriteLine($"collected {totalTests} items", TerminalWriter.Bright);
        Terminal.WriteLine();

        int testsRun = 0;
        int skippedCount = 0;
        foreach (string testFile in testFiles)
        {
            Terminal.Write(testFile + " ", TerminalWriter.Primary);

            for (int testIndex = 0; testIndex < testCountPerFile[testFile]; testIndex++)
            {
                // Nearly everything passes; an occasional test is skipped.
                if (Random.Chance(3))
                {
                    Terminal.Write("s", TerminalWriter.Warning);
                    skippedCount++;
                }
                else
                {
                    Terminal.Write(".", TerminalWriter.Primary);
                }

                testsRun++;
                Terminal.Pause(Random.Chance(5) ? Random.Next(300, 900) : Random.Next(15, 120));
            }

            // pytest shows overall progress at the right edge.
            Terminal.WriteLine($" [{testsRun * 100 / totalTests,3}%]", TerminalWriter.Primary);
        }

        double durationSeconds = Random.Next(3, 90) + Random.NextDouble();
        int passedCount = totalTests - skippedCount;
        string summary = skippedCount > 0
            ? $" {passedCount} passed, {skippedCount} skipped in {durationSeconds:0.00}s "
            : $" {passedCount} passed in {durationSeconds:0.00}s ";
        int sidePadding = Math.Max(3, (Math.Min(Terminal.Width, 100) - summary.Length) / 2);

        Terminal.WriteLine();
        Terminal.WriteLine(new string('=', sidePadding) + summary + new string('=', sidePadding), TerminalWriter.Primary);
    }
}
