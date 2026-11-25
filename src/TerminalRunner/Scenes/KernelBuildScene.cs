using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// A Linux kernel build ("make -j16"): a long stream of "CC", "AR" and "LD" lines,
/// a warning now and then, ending with the bzImage message.
/// </summary>
internal sealed class KernelBuildScene : Scene
{
    public KernelBuildScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override int Weight => 12;

    public override void Run()
    {
        Shell.ChangeDirectory("~/src/linux-6.12");
        int parallelJobs = Random.Pick([8, 12, 16, 24, 32]);
        Shell.RunCommand($"make -j{parallelJobs} bzImage modules");

        Terminal.WriteLine("  SYNC    include/config/auto.conf");
        Terminal.WriteLine("  HOSTCC  scripts/basic/fixdep");
        Terminal.WriteLine("  WRAP    arch/x86/include/generated/uapi/asm/bpf_perf_event.h");
        Terminal.WriteLine("  UPD     include/generated/utsversion.h");
        Terminal.Pause(300);

        int objectCount = Random.Next(120, 260);
        for (int objectIndex = 0; objectIndex < objectCount; objectIndex++)
        {
            string directory = Random.Pick(FakeData.KernelSourceDirectories);
            string fileName = Random.Pick(FakeData.KernelSourceFiles);

            // Most lines are plain compiles; the rest are archive and link steps.
            int stepRoll = Random.Next(100);
            if (stepRoll < 84)
            {
                Terminal.WriteLine($"  CC      {directory}/{fileName}.o");
            }
            else if (stepRoll < 90)
            {
                Terminal.WriteLine($"  CC [M]  {directory}/{fileName}.o");
            }
            else if (stepRoll < 95)
            {
                Terminal.WriteLine($"  AR      {directory}/built-in.a", TerminalWriter.Dim);
            }
            else
            {
                Terminal.WriteLine($"  AS      arch/x86/{Random.Pick(["entry/entry_64", "lib/memcpy_64", "kernel/head_64", "crypto/aesni-intel_asm"])}.o", TerminalWriter.Dim);
            }

            // Compilers warn. A lot.
            if (Random.Chance(3))
            {
                WriteCompilerWarning(directory, fileName);
            }

            Terminal.Pause(Random.Chance(5) ? Random.Next(250, 700) : Random.Next(10, 90));
        }

        Terminal.WriteLine("  LD      vmlinux.o");
        Terminal.PauseRandom(1200, 2500);
        Terminal.WriteLine("  MODPOST vmlinux.symvers");
        Terminal.WriteLine("  LD      .tmp_vmlinux.kallsyms1");
        Terminal.PauseRandom(600, 1200);
        Terminal.WriteLine("  KSYMS   .tmp_vmlinux.kallsyms1.S");
        Terminal.WriteLine("  LD      vmlinux");
        Terminal.PauseRandom(800, 1600);
        Terminal.WriteLine("  SORTTAB vmlinux");
        Terminal.WriteLine("  OBJCOPY arch/x86/boot/compressed/vmlinux.bin");
        Terminal.WriteLine("  ZSTD22  arch/x86/boot/compressed/vmlinux.bin.zst");
        Terminal.PauseRandom(900, 1800);
        Terminal.WriteLine("  LD      arch/x86/boot/setup.elf");
        Terminal.WriteLine("  OBJCOPY arch/x86/boot/setup.bin");
        Terminal.WriteLine("  BUILD   arch/x86/boot/bzImage");
        Terminal.Pause(300);
        Terminal.WriteLine($"Kernel: arch/x86/boot/bzImage is ready  (#{Random.Next(1, 40)})", TerminalWriter.Bright);
    }

    /// <summary>Prints a gcc style warning with the source line and caret marker.</summary>
    private void WriteCompilerWarning(string directory, string fileName)
    {
        int lineNumber = Random.Next(20, 4000);
        int columnNumber = Random.Next(5, 40);
        string variableName = Random.Pick(["ret", "tmp", "flags", "len", "offset", "priv", "status", "old_state"]);

        (string message, string flag) = Random.Next(4) switch
        {
            0 => ($"unused variable '{variableName}'", "-Wunused-variable"),
            1 => ($"'{variableName}' may be used uninitialized", "-Wmaybe-uninitialized"),
            2 => ("this statement may fall through", "-Wimplicit-fallthrough="),
            _ => ($"comparison of integer expressions of different signedness: 'int' and 'u32'", "-Wsign-compare"),
        };

        Terminal.WriteSegments(
            ($"{directory}/{fileName}.c:{lineNumber}:{columnNumber}: ", TerminalWriter.Bright),
            ("warning: ", TerminalWriter.Warning),
            (message + " ", TerminalWriter.Primary),
            ($"[{flag}]", TerminalWriter.Warning));
        Terminal.WriteLine($" {lineNumber,4} |         int {variableName};", TerminalWriter.Dim);
        Terminal.WriteLine($"      |             {new string('^', variableName.Length)}", TerminalWriter.Warning);
    }
}
