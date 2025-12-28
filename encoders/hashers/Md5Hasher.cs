using System.Security.Cryptography;

namespace Encode;

public sealed class Md5Hasher : HasherBase
{
    protected override byte[] ComputeHash(byte[] data) => MD5.HashData(data);
}
