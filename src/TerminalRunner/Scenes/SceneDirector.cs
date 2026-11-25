using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// Decides what happens next. Shows the boot sequence once, then picks random
/// scenes forever (never the same one twice in a row), with the occasional
/// "clear" or empty Enter press in between to look like a real person.
/// </summary>
internal sealed class SceneDirector
{
    private readonly TerminalWriter terminal;
    private readonly Random random;
    private readonly AppOptions options;
    private readonly ShellSession shell;
    private readonly List<Scene> scenes;

    public SceneDirector(TerminalWriter terminal, Random random, AppOptions options)
    {
        this.terminal = terminal;
        this.random = random;
        this.options = options;

        shell = new ShellSession(terminal, random.Pick(FakeData.UserNames), random.Pick(FakeData.HostNames));

        // Every scene that can show up in the endless loop.
        scenes =
        [
            new AptInstallScene(terminal, shell, random),
            new KernelBuildScene(terminal, shell, random),
            new CargoBuildScene(terminal, shell, random),
            new GitCloneScene(terminal, shell, random),
            new DockerBuildScene(terminal, shell, random),
            new LogTailScene(terminal, shell, random),
            new KernelLogScene(terminal, shell, random),
            new PingScene(terminal, shell, random),
            new HexDumpScene(terminal, shell, random),
            new TestRunScene(terminal, shell, random),
            new SystemStatusScene(terminal, shell, random),
        ];
    }

    /// <summary>Runs until the user quits (which throws <see cref="QuitRequestedException"/>).</summary>
    public void Run()
    {
        if (options.ShowBootSequence)
        {
            new BootScene(terminal, shell, random).Run();
        }

        Scene? previousScene = null;
        int scenesSinceClear = 0;

        while (true)
        {
            Scene nextScene = PickNextScene(previousScene);
            nextScene.Run();
            previousScene = nextScene;
            scenesSinceClear++;

            // Short break between commands, like someone reading the output.
            terminal.PauseRandom(600, 2600);

            // Every now and then the "user" clears the screen or just hits Enter.
            if (scenesSinceClear >= 6 && random.Chance(30))
            {
                shell.RunCommand("clear");
                terminal.ClearScreen();
                scenesSinceClear = 0;
            }
            else if (random.Chance(12))
            {
                shell.PressEnterOnEmptyPrompt();
            }
        }
    }

    /// <summary>Weighted random pick that avoids repeating the previous scene.</summary>
    private Scene PickNextScene(Scene? previousScene)
    {
        List<Scene> candidates = scenes.Where(scene => scene != previousScene).ToList();
        int totalWeight = candidates.Sum(scene => scene.Weight);
        int roll = random.Next(totalWeight);

        foreach (Scene candidate in candidates)
        {
            roll -= candidate.Weight;
            if (roll < 0)
            {
                return candidate;
            }
        }

        return candidates[^1];
    }
}
