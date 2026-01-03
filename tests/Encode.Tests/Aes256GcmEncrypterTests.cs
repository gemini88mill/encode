using Encode.Models;

namespace Encode.Tests;

public class Aes256GcmEncrypterTests
{
    private static readonly byte[] TestKey = Enumerable.Range(1, Aes256GcmEncrypter.KeySize).Select(value => (byte)value).ToArray();

    [Test]
    public void EncryptAndDecrypt_RoundTripsPlaintext()
    {
        var encrypter = new Aes256GcmEncrypter();
        var metadata = new EncryptionEnvelopeMetadata(
            EnvelopeVersion.Current,
            encrypter.Algorithm,
            "none",
            0,
            Array.Empty<byte>(),
            OutputFormat.Base64);

        var payload = encrypter.EncryptToString(
            "hello",
            isFile: false,
            key: TestKey,
            nonce: null,
            format: OutputFormat.Base64,
            upperCaseHex: false,
            envelopeMetadata: metadata);

        var parsed = encrypter.ReadEnvelope(payload, isFile: false, OutputFormat.Base64);
        var plaintext = encrypter.DecryptParsedEnvelopeToString(parsed, TestKey);

        Assert.That(parsed.Metadata.Version, Is.EqualTo(EnvelopeVersion.Current));
        Assert.That(parsed.Metadata.Algorithm, Is.EqualTo(encrypter.Algorithm));
        Assert.That(plaintext, Is.EqualTo("hello"));
    }

    [Test]
    public void EncryptToString_ThrowsForInvalidKeySize()
    {
        var encrypter = new Aes256GcmEncrypter();
        var metadata = new EncryptionEnvelopeMetadata(
            EnvelopeVersion.Current,
            encrypter.Algorithm,
            "none",
            0,
            Array.Empty<byte>(),
            OutputFormat.Base64);

        var shortKey = new byte[16];

        Assert.Throws<ArgumentException>(() =>
            encrypter.EncryptToString(
                "hello",
                isFile: false,
                key: shortKey,
                nonce: null,
                format: OutputFormat.Base64,
                upperCaseHex: false,
                envelopeMetadata: metadata));
    }

    [Test]
    public void DecryptParsedEnvelopeToString_ThrowsForInvalidNonceSize()
    {
        var encrypter = new Aes256GcmEncrypter();
        var metadata = new EncryptionEnvelopeMetadata(
            EnvelopeVersion.Current,
            encrypter.Algorithm,
            "none",
            0,
            Array.Empty<byte>(),
            OutputFormat.Base64);

        var payload = new EncryptionPayload(new byte[8], new byte[16], Array.Empty<byte>());
        var envelope = new ParsedEncryptionEnvelope(metadata, payload);

        Assert.Throws<ArgumentException>(() => encrypter.DecryptParsedEnvelopeToString(envelope, TestKey));
    }

    [Test]
    public void DecryptParsedEnvelopeToString_ThrowsForInvalidTagSize()
    {
        var encrypter = new Aes256GcmEncrypter();
        var metadata = new EncryptionEnvelopeMetadata(
            EnvelopeVersion.Current,
            encrypter.Algorithm,
            "none",
            0,
            Array.Empty<byte>(),
            OutputFormat.Base64);

        var payload = new EncryptionPayload(new byte[12], new byte[8], Array.Empty<byte>());
        var envelope = new ParsedEncryptionEnvelope(metadata, payload);

        Assert.Throws<ArgumentException>(() => encrypter.DecryptParsedEnvelopeToString(envelope, TestKey));
    }
}
