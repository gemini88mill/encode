namespace Encode;

public sealed class UrlEncoder : EncoderBase
{
    public override string EncodeToString(string input, bool isFile, OutputFormat format, bool upperCaseHex)
    {
        var text = isFile ? File.ReadAllText(input) : input;
        return Uri.EscapeDataString(text);
    }
}
