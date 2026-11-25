using System.Text;

namespace TerminalRunner.Content;

/// <summary>
/// Small helpers on top of <see cref="Random"/> that make the fake data code easier to read.
/// </summary>
internal static class RandomExtensions
{
    private const string HexDigits = "0123456789abcdef";

    /// <summary>Returns a random item from the list.</summary>
    public static T Pick<T>(this Random random, IReadOnlyList<T> items)
    {
        return items[random.Next(items.Count)];
    }

    /// <summary>Returns several different random items from the list.</summary>
    public static List<T> PickDistinct<T>(this Random random, IReadOnlyList<T> items, int count)
    {
        // Shuffle a copy and take the first "count" items.
        var shuffled = items.ToList();
        random.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(shuffled));
        return shuffled.Take(Math.Min(count, shuffled.Count)).ToList();
    }

    /// <summary>Returns true with the given probability in percent (0-100).</summary>
    public static bool Chance(this Random random, int percent)
    {
        return random.Next(100) < percent;
    }

    /// <summary>Returns a random lowercase hex string of the given length.</summary>
    public static string Hex(this Random random, int length)
    {
        var builder = new StringBuilder(length);
        for (int digitIndex = 0; digitIndex < length; digitIndex++)
        {
            builder.Append(HexDigits[random.Next(HexDigits.Length)]);
        }

        return builder.ToString();
    }

    /// <summary>Returns a version string like "1.24.3".</summary>
    public static string Version(this Random random, int maximumMajor = 5)
    {
        return $"{random.Next(0, maximumMajor + 1)}.{random.Next(0, 30)}.{random.Next(0, 20)}";
    }

    /// <summary>Returns a Debian/Ubuntu style package version like "2.4.1-1ubuntu0.3".</summary>
    public static string DebianVersion(this Random random)
    {
        string upstream = random.Version(9);
        return random.Next(3) switch
        {
            0 => $"{upstream}-{random.Next(1, 6)}",
            1 => $"{upstream}-{random.Next(1, 4)}ubuntu{random.Next(0, 3)}.{random.Next(1, 9)}",
            _ => $"{upstream}-{random.Next(1, 4)}build{random.Next(1, 4)}",
        };
    }

    /// <summary>Returns a private network IPv4 address.</summary>
    public static string PrivateIpAddress(this Random random)
    {
        return random.Next(3) switch
        {
            0 => $"10.{random.Next(0, 255)}.{random.Next(0, 255)}.{random.Next(1, 255)}",
            1 => $"192.168.{random.Next(0, 3)}.{random.Next(2, 255)}",
            _ => $"172.{random.Next(16, 32)}.{random.Next(0, 255)}.{random.Next(1, 255)}",
        };
    }

    /// <summary>Returns a public looking IPv4 address.</summary>
    public static string PublicIpAddress(this Random random)
    {
        return $"{random.Next(23, 220)}.{random.Next(0, 255)}.{random.Next(0, 255)}.{random.Next(1, 255)}";
    }

    /// <summary>Returns a MAC address like "3c:7c:3f:a1:09:be".</summary>
    public static string MacAddress(this Random random)
    {
        var octets = new string[6];
        for (int octetIndex = 0; octetIndex < octets.Length; octetIndex++)
        {
            octets[octetIndex] = random.Hex(2);
        }

        return string.Join(':', octets);
    }

    /// <summary>Returns a UUID-looking string.</summary>
    public static string Uuid(this Random random)
    {
        return $"{random.Hex(8)}-{random.Hex(4)}-{random.Hex(4)}-{random.Hex(4)}-{random.Hex(12)}";
    }
}
