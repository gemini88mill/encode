using System.Text;

namespace Encode;

public sealed class Base64Encoder : EncoderBase
{
    protected override string Encode(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    protected override string Decode(string input)
    {
        var bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(bytes);
    }
}
