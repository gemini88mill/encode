using System.Security.Cryptography;
using Encode.Models;

namespace Encode;

public sealed class Aes256GcmEncrypter : EncrypterBase
{
    public const int KeySize = 32;

    protected override int NonceSize => 12;
    protected override int TagSize => 16;
    protected override string AlgorithmId => "A256GCM";
    protected override int KeySizeBytes => KeySize;

    protected override EncryptionPayload Encrypt(
        byte[] plaintext,
        byte[] key,
        byte[]? nonce,
        byte[]? associatedData)
    {
        ValidateKey(key);

        var nonceBytes = nonce ?? RandomNumberGenerator.GetBytes(NonceSize);
        ValidateNonce(nonceBytes);

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonceBytes, plaintext, ciphertext, tag, associatedData);

        return new EncryptionPayload(nonceBytes, tag, ciphertext);
    }

    protected override byte[] Decrypt(
        EncryptionPayload payload,
        byte[] key,
        byte[]? associatedData)
    {
        ValidateKey(key);
        ValidateNonce(payload.Nonce);
        ValidateTag(payload.Tag);

        var plaintext = new byte[payload.Ciphertext.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(payload.Nonce, payload.Ciphertext, payload.Tag, plaintext, associatedData);

        return plaintext;
    }

    private void ValidateKey(byte[] key)
    {
        if (key.Length != KeySizeBytes)
        {
            throw new ArgumentException($"Key must be {KeySizeBytes} bytes for AES-256-GCM.", nameof(key));
        }
    }
}
