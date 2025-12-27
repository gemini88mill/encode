using System.Security.Cryptography;
using System.Text;

namespace Encode;

public sealed class Sha512Encoder : EncoderBase
{
    public override string EncodeToString(string input, bool isFile)
    {
        byte[] data = isFile
            ? File.ReadAllBytes(input)
            : Encoding.UTF8.GetBytes(input);

        byte[] hash = SHA512.HashData(data);
        return Convert.ToBase64String(hash);
    }
}
