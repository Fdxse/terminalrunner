using System.Text;
using TerminalRunner.Content;
using TerminalRunner.Terminal;

namespace TerminalRunner.Scenes;

/// <summary>
/// "hexdump -C" of a binary file: offsets, hex bytes and the ASCII column.
/// Looks wonderfully mysterious scrolling by.
/// </summary>
internal sealed class HexDumpScene : Scene
{
    private const int BytesPerLine = 16;

    public HexDumpScene(TerminalWriter terminal, ShellSession shell, Random random)
        : base(terminal, shell, random)
    {
    }

    public override int Weight => 6;

    public override void Run()
    {
        string fileName = Random.Pick(["firmware.bin", "/dev/nvme0n1p1", "core.dump", "blob.dat", "/boot/initrd.img"]);
        int lineCount = Random.Next(40, 90);
        Shell.RunCommand($"sudo hexdump -C {fileName} | head -n {lineCount}");

        // Start at an aligned random offset.
        long offset = Random.Next(0, 4096) * BytesPerLine;
        var lineBytes = new byte[BytesPerLine];

        for (int lineIndex = 0; lineIndex < lineCount; lineIndex++)
        {
            FillWithPlausibleBytes(lineBytes);

            Terminal.Write($"{offset:x8}  ", TerminalWriter.Dim);
            Terminal.Write(FormatHexColumns(lineBytes), TerminalWriter.Primary);
            Terminal.WriteLine($" |{FormatAsciiColumn(lineBytes)}|", TerminalWriter.Accent);

            offset += BytesPerLine;
            Terminal.PauseRandom(15, 70);
        }
    }

    /// <summary>
    /// Real binaries are not uniformly random: they contain runs of zeros and bits of text.
    /// Mixing those in makes the dump look much more authentic.
    /// </summary>
    private void FillWithPlausibleBytes(byte[] buffer)
    {
        int kindRoll = Random.Next(100);
        if (kindRoll < 15)
        {
            Array.Clear(buffer);
        }
        else if (kindRoll < 30)
        {
            string text = Random.Pick(["GNU C Library v2", "/lib64/ld-linux-", "libc.so.6\0\0\0\0\0\0\0", "__cxa_finalize\0\0", ".shstrtab\0.text\0", "Copyright (c) 20"]);
            Encoding.ASCII.GetBytes(text.PadRight(BytesPerLine, '\0').AsSpan(0, BytesPerLine), buffer);
        }
        else
        {
            Random.NextBytes(buffer);
        }
    }

    /// <summary>Formats 16 bytes as "xx xx xx xx xx xx xx xx  xx xx ..." (with the gap in the middle).</summary>
    private static string FormatHexColumns(byte[] buffer)
    {
        var builder = new StringBuilder();
        for (int byteIndex = 0; byteIndex < buffer.Length; byteIndex++)
        {
            builder.Append(buffer[byteIndex].ToString("x2")).Append(' ');
            if (byteIndex == 7)
            {
                builder.Append(' ');
            }
        }

        return builder.ToString();
    }

    /// <summary>Printable characters as-is, everything else as a dot.</summary>
    private static string FormatAsciiColumn(byte[] buffer)
    {
        var builder = new StringBuilder(buffer.Length);
        foreach (byte value in buffer)
        {
            builder.Append(value >= 32 && value < 127 ? (char)value : '.');
        }

        return builder.ToString();
    }
}
