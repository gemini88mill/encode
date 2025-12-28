using System.Security.Cryptography;

namespace Encode;

public sealed class Md5Encoder : HashEncoderBase
{
    protected override byte[] ComputeHash(byte[] data) => MD5.HashData(data);
}
