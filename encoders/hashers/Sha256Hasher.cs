using System.Security.Cryptography;

namespace Encode;

public sealed class Sha256Hasher : HasherBase
{
    protected override byte[] ComputeHash(byte[] data) => SHA256.HashData(data);
}
