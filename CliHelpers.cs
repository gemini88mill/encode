using System;
using System.Security.Cryptography;
using System.Text;

namespace Encode;

internal static class CliHelpers
{
    public static bool TryParseBytes(string value, OutputFormat format, out byte[] bytes)
    {
        try
        {
            bytes = format switch
            {
                OutputFormat.Base64 => Convert.FromBase64String(value),
                OutputFormat.Hex => Convert.FromHexString(value),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported format.")
            };
            return true;
        }
        catch (FormatException)
        {
            bytes = Array.Empty<byte>();
            return false;
        }
        catch (ArgumentException)
        {
            bytes = Array.Empty<byte>();
            return false;
        }
    }

    public static byte[] DeriveKeyFromPassword(string password)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        return SHA256.HashData(passwordBytes);
    }
}
