using System.Text;

namespace Encode;

public abstract class HasherBase
{
    public string HashToString(string input, bool isFile, OutputFormat format, bool upperCaseHex)
    {
        byte[] data = isFile
            ? File.ReadAllBytes(input)
            : Encoding.UTF8.GetBytes(input);

        byte[] hash = ComputeHash(data);
        return FormatBytes(hash, format, upperCaseHex);
    }

    public string HashToFile(string input, bool isFile, string outputPath, OutputFormat format, bool upperCaseHex)
    {
        var output = HashToString(input, isFile, format, upperCaseHex);
        File.WriteAllText(outputPath, output);
        return output;
    }

    protected abstract byte[] ComputeHash(byte[] data);

    protected static string FormatBytes(byte[] bytes, OutputFormat format, bool upperCaseHex)
    {
        return format switch
        {
            OutputFormat.Base64 => Convert.ToBase64String(bytes),
            OutputFormat.Hex => upperCaseHex
                ? Convert.ToHexString(bytes)
                : Convert.ToHexString(bytes).ToLowerInvariant(),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported output format.")
        };
    }
}
