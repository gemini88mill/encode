using System.Security.Cryptography;

namespace Encode;

public sealed class Sha512Encoder : HashEncoderBase
{
    protected override byte[] ComputeHash(byte[] data) => SHA512.HashData(data);
}
