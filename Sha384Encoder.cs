using System.Security.Cryptography;

namespace Encode;

public sealed class Sha384Encoder : HashEncoderBase
{
    protected override byte[] ComputeHash(byte[] data) => SHA384.HashData(data);
}
