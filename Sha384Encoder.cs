using System.Security.Cryptography;
using System.Text;

namespace Encode;

public sealed class Sha384Encoder : EncoderBase
{
    public override string EncodeToString(string input, bool isFile)
    {
        byte[] data = isFile
            ? File.ReadAllBytes(input)
            : Encoding.UTF8.GetBytes(input);

        byte[] hash = SHA384.HashData(data);
        return Convert.ToBase64String(hash);
    }
}
