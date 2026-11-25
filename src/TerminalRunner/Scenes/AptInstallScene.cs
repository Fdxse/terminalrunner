using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// "sudo apt-get install ..." with the full Debian/Ubuntu output:
/// dependency resolution, downloads, unpacking and setup.
/// </summary>
internal sealed class AptInstallScene : Scene
{
    public AptInstallScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override int Weight => 12;

    public override void Run()
    {
        // The user asks for a couple of packages, apt pulls in a bunch of dependencies.
        List<string> allPackages = Random.PickDistinct(FakeData.AptPackages, Random.Next(6, 20));
        List<string> requestedPackages = allPackages.Take(Random.Next(1, 4)).ToList();
        var packageVersions = allPackages.ToDictionary(packageName => packageName, _ => Random.DebianVersion());

        if (Random.Chance(50))
        {
            RunAptUpdate();
        }

        Shell.RunCommand("sudo apt-get install -y " + string.Join(' ', requestedPackages));
        Terminal.WriteLineThenPause("Reading package lists... Done", 200, 700);
        Terminal.WriteLineThenPause("Building dependency tree... Done", 100, 400);
        Terminal.WriteLineThenPause("Reading state information... Done", 50, 200);

        Terminal.WriteLine("The following additional packages will be installed:");
        WriteWrappedPackageList(allPackages.Except(requestedPackages));
        Terminal.WriteLine("The following NEW packages will be installed:");
        WriteWrappedPackageList(allPackages);

        long totalKilobytes = 0;
        var packageSizes = new Dictionary<string, int>();
        foreach (string packageName in allPackages)
        {
            packageSizes[packageName] = Random.Next(12, 9000);
            totalKilobytes += packageSizes[packageName];
        }

        Terminal.WriteLine($"0 upgraded, {allPackages.Count} newly installed, 0 to remove and {Random.Next(0, 30)} not upgraded.");
        Terminal.WriteLine($"Need to get {totalKilobytes / 1024.0:0.0} MB of archives.");
        Terminal.WriteLine($"After this operation, {totalKilobytes * 3.4 / 1024.0:0.0} MB of additional disk space will be used.");
        Terminal.Pause(400);

        // Downloads.
        int downloadNumber = 1;
        foreach (string packageName in allPackages)
        {
            string section = Random.Pick(["main", "universe"]);
            string pocket = Random.Pick(["noble", "noble-updates", "noble-security"]);
            Terminal.WriteLine($"Get:{downloadNumber} http://archive.ubuntu.com/ubuntu {pocket}/{section} amd64 {packageName} amd64 {packageVersions[packageName]} [{packageSizes[packageName]} kB]");
            Terminal.Pause(Math.Min(900, 20 + packageSizes[packageName] / 12));
            downloadNumber++;
        }

        double downloadSeconds = Random.Next(1, 12) + Random.NextDouble();
        Terminal.WriteLine($"Fetched {totalKilobytes / 1024.0:0.0} MB in {downloadSeconds:0}s ({totalKilobytes / 1024.0 / downloadSeconds:0.0} MB/s)");
        Terminal.Pause(300);

        // Unpacking. The database line is printed once, like dpkg does.
        int filesInDatabase = Random.Next(60000, 240000);
        bool databaseLineWritten = false;
        foreach (string packageName in allPackages)
        {
            Terminal.WriteLine($"Selecting previously unselected package {packageName}.", TerminalWriter.Dim);
            if (!databaseLineWritten)
            {
                Terminal.WriteLine($"(Reading database ... {filesInDatabase} files and directories currently installed.)", TerminalWriter.Dim);
                databaseLineWritten = true;
            }

            Terminal.WriteLine($"Preparing to unpack .../{packageName}_{packageVersions[packageName]}_amd64.deb ...", TerminalWriter.Dim);
            Terminal.PauseRandom(40, 260);
            Terminal.WriteLine($"Unpacking {packageName} ({packageVersions[packageName]}) ...");
            Terminal.PauseRandom(60, 400);
        }

        // Configuring.
        foreach (string packageName in allPackages)
        {
            Terminal.WriteLine($"Setting up {packageName} ({packageVersions[packageName]}) ...");

            // Services print a little extra when they are enabled.
            if (Random.Chance(15))
            {
                Terminal.WriteLine($"Created symlink /etc/systemd/system/multi-user.target.wants/{packageName}.service → /usr/lib/systemd/system/{packageName}.service.", TerminalWriter.Dim);
            }

            Terminal.PauseRandom(80, 500);
        }

        Terminal.WriteLine($"Processing triggers for man-db (2.12.0-4build2) ...");
        Terminal.PauseRandom(500, 1500);
        Terminal.WriteLine($"Processing triggers for libc-bin (2.39-0ubuntu8.3) ...");
        Terminal.PauseRandom(200, 600);
    }

    /// <summary>"sudo apt update" with its Hit/Get lines.</summary>
    private void RunAptUpdate()
    {
        Shell.RunCommand("sudo apt update");
        string[] repositories =
        [
            "http://archive.ubuntu.com/ubuntu noble InRelease",
            "http://archive.ubuntu.com/ubuntu noble-updates InRelease",
            "http://archive.ubuntu.com/ubuntu noble-backports InRelease",
            "http://security.ubuntu.com/ubuntu noble-security InRelease",
            "https://download.docker.com/linux/ubuntu noble InRelease",
            "https://apt.postgresql.org/pub/repos/apt noble-pgdg InRelease",
        ];

        int lineNumber = 1;
        foreach (string repository in repositories)
        {
            string status = Random.Chance(60) ? "Hit" : "Get";
            Terminal.WriteLine(status == "Hit" ? $"Hit:{lineNumber} {repository}" : $"Get:{lineNumber} {repository} [{Random.Next(40, 130)}.{Random.Next(0, 9)} kB]");
            Terminal.PauseRandom(80, 600);
            lineNumber++;
        }

        Terminal.WriteLine($"Fetched {Random.Next(100, 2000)} kB in {Random.Next(1, 4)}s ({Random.Next(80, 900)} kB/s)");
        Terminal.WriteLineThenPause("Reading package lists... Done", 300, 900);
        Terminal.WriteLineThenPause("Building dependency tree... Done", 100, 300);
        Terminal.WriteLine("Reading state information... Done");
        Terminal.WriteLine($"{Random.Next(1, 40)} packages can be upgraded. Run 'apt list --upgradable' to see them.");
        Terminal.PauseRandom(400, 900);
    }

    /// <summary>Prints package names indented and wrapped to the window width, like apt does.</summary>
    private void WriteWrappedPackageList(IEnumerable<string> packageNames)
    {
        int maximumLineLength = Terminal.Width - 2;
        string currentLine = " ";

        foreach (string packageName in packageNames)
        {
            if (currentLine.Length + packageName.Length + 1 > maximumLineLength)
            {
                Terminal.WriteLine(currentLine);
                currentLine = " ";
            }

            currentLine += " " + packageName;
        }

        if (currentLine.Trim().Length > 0)
        {
            Terminal.WriteLine(currentLine);
        }

        Terminal.Pause(150);
    }
}
