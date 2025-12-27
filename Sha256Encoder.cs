using System.Security.Cryptography;
using System.Text;

namespace Encode;

public sealed class Sha256Encoder : EncoderBase
{
    public override string EncodeToString(string input, bool isFile, OutputFormat format, bool upperCaseHex)
    {
        byte[] data = isFile
            ? File.ReadAllBytes(input)
            : Encoding.UTF8.GetBytes(input);

        byte[] hash = SHA256.HashData(data);
        return FormatBytes(hash, format, upperCaseHex);
    }
}
