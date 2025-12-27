namespace Encode;

public abstract class EncoderBase
{
    public abstract string EncodeToString(string input, bool isFile, OutputFormat format, bool upperCaseHex);

    public string EncodeToFile(string input, bool isFile, string outputPath, OutputFormat format, bool upperCaseHex)
    {
        var output = EncodeToString(input, isFile, format, upperCaseHex);
        File.WriteAllText(outputPath, output);
        return output;
    }

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
