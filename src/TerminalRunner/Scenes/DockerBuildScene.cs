using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// "docker build" with BuildKit style output: numbered steps, timings,
/// cached layers, and the final image export.
/// </summary>
internal sealed class DockerBuildScene : Scene
{
    public DockerBuildScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override void Run()
    {
        string projectName = Random.Pick(FakeData.ProjectNames);
        string baseImage = Random.Pick(FakeData.DockerBaseImages);
        string imageTag = $"registry.internal.lan/{projectName}:{Random.Version(3)}";

        Shell.ChangeDirectory($"~/src/{projectName}");
        Shell.RunCommand($"docker build -t {imageTag} .");

        // The Dockerfile instructions that will be "executed".
        string[] instructions =
        [
            $"FROM {baseImage}",
            "WORKDIR /app",
            "RUN apt-get update && apt-get install -y --no-install-recommends ca-certificates curl && rm -rf /var/lib/apt/lists/*",
            "COPY package*.json ./",
            "RUN npm ci --omit=dev",
            "COPY . .",
            "RUN make release",
            "RUN useradd --system --uid 10001 appuser",
            "USER appuser",
        ];

        double elapsedSeconds = 0.1;
        int totalSteps = instructions.Length + 4;
        int finishedSteps = 0;

        Terminal.WriteLine($"[+] Building {elapsedSeconds:0.0}s (0/{totalSteps})", TerminalWriter.Accent);
        WriteBuildStep("[internal] load build definition from Dockerfile", ref elapsedSeconds, ref finishedSteps, 0.0, 0.1);
        WriteBuildStep("=> transferring dockerfile: 612B", ref elapsedSeconds, ref finishedSteps, 0.0, 0.0, countsAsStep: false);
        WriteBuildStep($"[internal] load metadata for docker.io/library/{baseImage}", ref elapsedSeconds, ref finishedSteps, 0.5, 1.6);
        WriteBuildStep("[internal] load .dockerignore", ref elapsedSeconds, ref finishedSteps, 0.0, 0.1);
        WriteBuildStep("[internal] load build context", ref elapsedSeconds, ref finishedSteps, 0.1, 0.9);
        WriteBuildStep($"=> transferring context: {Random.Next(1, 90)}.{Random.Next(0, 99):00}MB", ref elapsedSeconds, ref finishedSteps, 0.0, 0.0, countsAsStep: false);

        for (int instructionIndex = 0; instructionIndex < instructions.Length; instructionIndex++)
        {
            string stepLabel = $"[{instructionIndex + 1,2}/{instructions.Length}] {instructions[instructionIndex]}";

            // Early layers are usually cached on a rebuild.
            bool isCached = instructionIndex < 3 && Random.Chance(70);
            if (isCached)
            {
                Terminal.WriteSegments((" => ", TerminalWriter.Accent), ("CACHED ", TerminalWriter.Accent), (stepLabel, TerminalWriter.Primary));
                finishedSteps++;
                Terminal.PauseRandom(30, 120);
                continue;
            }

            WriteBuildStep(stepLabel, ref elapsedSeconds, ref finishedSteps, 0.2, 14.0);

            // The heavy RUN steps print a few lines of their own output.
            if (instructions[instructionIndex].StartsWith("RUN npm", StringComparison.Ordinal))
            {
                Terminal.WriteLine($"#{instructionIndex + 6} {Random.Next(5, 40)}.{Random.Next(0, 9)} added {Random.Next(200, 1400)} packages, and audited {Random.Next(1400, 1600)} packages in {Random.Next(8, 40)}s", TerminalWriter.Dim);
                Terminal.WriteLine($"#{instructionIndex + 6} {Random.Next(5, 40)}.{Random.Next(0, 9)} found 0 vulnerabilities", TerminalWriter.Dim);
            }
            else if (instructions[instructionIndex].StartsWith("RUN make", StringComparison.Ordinal))
            {
                int outputLineCount = Random.Next(4, 10);
                for (int outputLineIndex = 0; outputLineIndex < outputLineCount; outputLineIndex++)
                {
                    // Advance the clock a little so the build log timestamps keep moving.
                    elapsedSeconds += Random.NextDouble() * 0.8;
                    Terminal.WriteLine($"#{instructionIndex + 6} {elapsedSeconds:0.000} CC {Random.Pick(FakeData.KernelSourceFiles)}.o", TerminalWriter.Dim);
                    Terminal.PauseRandom(40, 200);
                }
            }
        }

        WriteBuildStep("exporting to image", ref elapsedSeconds, ref finishedSteps, 0.5, 2.5);
        Terminal.WriteLine($" => => exporting layers                                                     {Random.Next(1, 4)}.{Random.Next(0, 9)}s", TerminalWriter.Dim);
        Terminal.WriteLine($" => => writing image sha256:{Random.Hex(64)}", TerminalWriter.Dim);
        Terminal.WriteLine($" => => naming to {imageTag}", TerminalWriter.Dim);
        Terminal.Pause(300);
        Terminal.WriteLine($"[+] Building {elapsedSeconds:0.0}s ({totalSteps}/{totalSteps}) FINISHED", TerminalWriter.Accent);
    }

    /// <summary>Writes one " => step  1.2s" line and pauses about as long as the step "took".</summary>
    private void WriteBuildStep(string description, ref double elapsedSeconds, ref int finishedSteps, double minimumSeconds, double maximumSeconds, bool countsAsStep = true)
    {
        double stepSeconds = minimumSeconds + Random.NextDouble() * (maximumSeconds - minimumSeconds);
        elapsedSeconds += stepSeconds;

        // Keep timings right-aligned like BuildKit does.
        const string prefix = " => ";
        int padding = Math.Max(1, Terminal.Width - prefix.Length - description.Length - 8);
        string timing = $"{stepSeconds:0.0}s";
        string line = description + new string(' ', padding) + timing;

        Terminal.WriteSegments((prefix, TerminalWriter.Accent), (line.Length > Terminal.Width - 5 ? description : line, TerminalWriter.Primary));

        if (countsAsStep)
        {
            finishedSteps++;
        }

        // Do not make the viewer wait the full time of slow steps.
        Terminal.Pause((int)Math.Min(stepSeconds * 300, 3000));
    }
}
