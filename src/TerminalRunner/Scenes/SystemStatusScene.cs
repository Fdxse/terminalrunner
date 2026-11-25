using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// Quick system checks a sysadmin types all the time: df, free, uptime,
/// systemctl status and docker ps. One or two of them per run.
/// </summary>
internal sealed class SystemStatusScene : Scene
{
    public SystemStatusScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override int Weight => 9;

    public override void Run()
    {
        // Pick one or two different checks.
        List<Action> checks = Random.PickDistinct<Action>([ShowDiskUsage, ShowMemory, ShowUptime, ShowServiceStatus, ShowContainers], Random.Next(1, 3));
        foreach (Action check in checks)
        {
            check();
            Terminal.PauseRandom(500, 1500);
        }
    }

    /// <summary>"df -h"</summary>
    private void ShowDiskUsage()
    {
        Shell.RunCommand("df -h");
        Terminal.WriteLine("Filesystem      Size  Used Avail Use% Mounted on", TerminalWriter.Bright);
        Terminal.WriteLine($"tmpfs           1.6G  {Random.Next(1, 9)}.{Random.Next(0, 9)}M  1.6G   1% /run");
        WriteDiskLine("/dev/nvme0n1p2", 468, "/");
        Terminal.WriteLine("tmpfs           7.8G     0  7.8G   0% /dev/shm");
        Terminal.WriteLine("tmpfs           5.0M   12K  5.0M   1% /run/lock");
        Terminal.WriteLine("/dev/nvme0n1p1  1.1G  6.1M  1.1G   1% /boot/efi");
        WriteDiskLine("/dev/md0", 3600, "/srv/data");
        Terminal.WriteLine($"tmpfs           1.6G  {Random.Next(4, 200)}K  1.6G   1% /run/user/1000");
    }

    /// <summary>Writes a df line for a disk of the given size in GB with random usage.</summary>
    private void WriteDiskLine(string device, int sizeGigabytes, string mountPoint)
    {
        int usedPercent = Random.Next(8, 96);
        int usedGigabytes = sizeGigabytes * usedPercent / 100;
        string size = sizeGigabytes >= 1000 ? $"{sizeGigabytes / 1000.0:0.0}T" : $"{sizeGigabytes}G";
        string used = usedGigabytes >= 1000 ? $"{usedGigabytes / 1000.0:0.0}T" : $"{usedGigabytes}G";
        int available = sizeGigabytes - usedGigabytes;
        string availableText = available >= 1000 ? $"{available / 1000.0:0.0}T" : $"{available}G";

        // A nearly full disk is shown in yellow, the way you'd notice it in real life.
        Terminal.WriteLine($"{device,-15} {size,4}  {used,4} {availableText,5} {usedPercent,3}% {mountPoint}", usedPercent > 90 ? TerminalWriter.Warning : TerminalWriter.Primary);
    }

    /// <summary>"free -h"</summary>
    private void ShowMemory()
    {
        Shell.RunCommand("free -h");
        double totalGigabytes = Random.Pick([7.6, 15.5, 31.2, 62.8]);
        double usedGigabytes = totalGigabytes * (Random.Next(15, 80) / 100.0);
        double cacheGigabytes = (totalGigabytes - usedGigabytes) * 0.6;
        double freeGigabytes = totalGigabytes - usedGigabytes - cacheGigabytes;

        Terminal.WriteLine("               total        used        free      shared  buff/cache   available", TerminalWriter.Bright);
        Terminal.WriteLine($"Mem:          {totalGigabytes,5:0.0}Gi     {usedGigabytes,5:0.0}Gi     {freeGigabytes,5:0.0}Gi     {Random.Next(10, 900),4}Mi     {cacheGigabytes,5:0.0}Gi     {totalGigabytes - usedGigabytes,5:0.0}Gi");
        Terminal.WriteLine($"Swap:         4.0Gi      {Random.Next(0, 400),3}Mi      {Random.Next(36, 40) / 10.0:0.0}Gi");
    }

    /// <summary>"uptime"</summary>
    private void ShowUptime()
    {
        Shell.RunCommand("uptime");
        Terminal.WriteLine($" {DateTime.Now:HH:mm:ss} up {Random.Next(2, 400)} days, {Random.Next(0, 23),2}:{Random.Next(0, 59):00},  {Random.Next(1, 4)} users,  load average: {Random.NextDouble() * 4:0.00}, {Random.NextDouble() * 4:0.00}, {Random.NextDouble() * 4:0.00}");
    }

    /// <summary>"systemctl status" of a service, with its recent journal lines.</summary>
    private void ShowServiceStatus()
    {
        string serviceName = Random.Pick(["nginx", "postgresql@16-main", "docker", "redis-server", "mariadb", "ssh"]);
        Shell.RunCommand($"systemctl status {serviceName} --no-pager");

        DateTime startedAt = DateTime.Now.AddDays(-Random.Next(1, 60)).AddHours(-Random.Next(0, 23));
        int mainProcessId = Random.Next(700, 99999);

        Terminal.WriteSegments(("● ", TerminalWriter.Primary), ($"{serviceName}.service - {Random.Pick(FakeData.SystemdServices)}", TerminalWriter.Bright));
        Terminal.WriteLine($"     Loaded: loaded (/usr/lib/systemd/system/{serviceName}.service; enabled; preset: enabled)");
        Terminal.WriteSegments(("     Active: ", TerminalWriter.Primary), ("active (running)", TerminalWriter.Primary), ($" since {startedAt:ddd yyyy-MM-dd HH:mm:ss} UTC; {(DateTime.Now - startedAt).Days} days ago", TerminalWriter.Primary));
        Terminal.WriteLine($"       Docs: man:{serviceName.Split('@')[0]}(8)");
        Terminal.WriteLine($"   Main PID: {mainProcessId} ({serviceName.Split('@')[0]})");
        Terminal.WriteLine($"      Tasks: {Random.Next(3, 80)} (limit: {Random.Next(18000, 19000)})");
        Terminal.WriteLine($"     Memory: {Random.Next(8, 900)}.{Random.Next(0, 9)}M (peak: {Random.Next(900, 1500)}.{Random.Next(0, 9)}M)");
        Terminal.WriteLine($"        CPU: {Random.Next(1, 59)}min {Random.Next(0, 59)}.{Random.Next(100, 999)}s");
        Terminal.WriteLine($"     CGroup: /system.slice/{serviceName}.service");
        Terminal.WriteLine($"             └─{mainProcessId} \"{serviceName.Split('@')[0]}: master process\"", TerminalWriter.Dim);
        Terminal.WriteLine();

        int journalLineCount = Random.Next(3, 7);
        for (int journalLineIndex = 0; journalLineIndex < journalLineCount; journalLineIndex++)
        {
            Terminal.WriteLine($"{startedAt.AddSeconds(journalLineIndex):MMM dd HH:mm:ss} {Shell.HostName} systemd[1]: {Random.Pick(["Starting", "Started", "Reloading", "Reloaded"])} {serviceName}.service.", TerminalWriter.Dim);
            Terminal.PauseRandom(20, 80);
        }
    }

    /// <summary>"docker ps"</summary>
    private void ShowContainers()
    {
        Shell.RunCommand("docker ps");
        Terminal.WriteLine("CONTAINER ID   IMAGE                                    COMMAND                  CREATED        STATUS                  PORTS                    NAMES", TerminalWriter.Bright);

        List<string> projects = Random.PickDistinct(FakeData.ProjectNames, Random.Next(3, 8));
        foreach (string projectName in projects)
        {
            string image = $"registry.internal.lan/{projectName}:{Random.Version(3)}";
            string status = Random.Chance(80) ? $"Up {Random.Next(2, 40)} hours (healthy)" : $"Up {Random.Next(2, 59)} minutes";
            Terminal.WriteLine($"{Random.Hex(12)}   {Truncate(image, 40),-40} \"{Truncate("/usr/local/bin/entrypoint.sh", 22)}…\"   {Random.Next(2, 9)} days ago     {status,-23} 0.0.0.0:{Random.Next(3000, 9999)}->8080/tcp   {projectName}");
            Terminal.PauseRandom(10, 60);
        }
    }

    /// <summary>Cuts text to a maximum length.</summary>
    private static string Truncate(string text, int maximumLength)
    {
        return text.Length <= maximumLength ? text : text[..maximumLength];
    }
}
