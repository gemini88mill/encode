using System.Security.Cryptography;

namespace Encode;

public sealed class Sha1Hasher : HasherBase
{
    protected override byte[] ComputeHash(byte[] data) => SHA1.HashData(data);
}
