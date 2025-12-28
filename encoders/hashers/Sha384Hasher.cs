using System.Security.Cryptography;

namespace Encode;

public sealed class Sha384Hasher : HasherBase
{
    protected override byte[] ComputeHash(byte[] data) => SHA384.HashData(data);
}
