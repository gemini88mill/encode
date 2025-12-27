using System.Security.Cryptography;
using System.Text;

namespace Encode;

public sealed class Md5Encoder : EncoderBase
{
    public override string EncodeToString(string input, bool isFile, OutputFormat format, bool upperCaseHex)
    {
        byte[] data = isFile
            ? File.ReadAllBytes(input)
            : Encoding.UTF8.GetBytes(input);

        byte[] hash = MD5.HashData(data);
        return FormatBytes(hash, format, upperCaseHex);
    }
}
