namespace Encode;

public sealed class UrlEncoder : EncoderBase
{
    protected override string Encode(string input) => Uri.EscapeDataString(input);

    protected override string Decode(string input) => Uri.UnescapeDataString(input);
}
