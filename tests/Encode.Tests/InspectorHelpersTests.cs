using Encode.Inspector;

namespace Encode.Tests;

public class InspectorHelpersTests
{
    [Test]
    public void TryReadEnvelopeMetadata_ReturnsStructuredOutput_ForKnownAlgorithm()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempPath, "abc");
            var file = new FileInfo(tempPath);

            var payload = """
                          {
                            "version": "1",
                            "alg": "A256GCM",
                            "kdf": "PBKDF2-SHA256",
                            "iter": 310000,
                            "salt": "salt-value",
                            "nonce": "ignored",
                            "tag": "ignored",
                            "ciphertext": "ignored",
                            "fmt": "base64"
                          }
                          """;

            var success = InspectorHelpers.TryReadEnvelopeMetadata(payload, file, out var inspection, out var errorMessage);

            Assert.That(success, Is.True);
            Assert.That(errorMessage, Is.Empty);
            Assert.That(inspection.Version.Value, Is.EqualTo("1"));
            Assert.That(inspection.Version.Legacy, Is.False);
            Assert.That(inspection.Algorithm.Id, Is.EqualTo("A256GCM"));
            Assert.That(inspection.Algorithm.Name, Is.EqualTo("AES-256-GCM"));
            Assert.That(inspection.Algorithm.KeySizeBytes, Is.EqualTo(32));
            Assert.That(inspection.Envelope.Kdf, Is.EqualTo("PBKDF2-SHA256"));
            Assert.That(inspection.Envelope.Iterations, Is.EqualTo(310000));
            Assert.That(inspection.Envelope.Salt, Is.EqualTo("salt-value"));
            Assert.That(inspection.Envelope.Format, Is.EqualTo("base64"));
            Assert.That(inspection.File.Path, Is.EqualTo(file.FullName));
            Assert.That(inspection.File.Name, Is.EqualTo(file.Name));
            Assert.That(inspection.File.SizeBytes, Is.EqualTo(file.Length));
            Assert.That(DateTime.TryParse(inspection.File.LastModifiedUtc, out _), Is.True);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Test]
    public void TryReadEnvelopeMetadata_AssignsLegacyVersion_WhenVersionMissing()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempPath, "abc");
            var file = new FileInfo(tempPath);

            var payload = """
                          {
                            "alg": "A256GCM",
                            "kdf": "none",
                            "iter": 0,
                            "salt": "",
                            "nonce": "ignored",
                            "tag": "ignored",
                            "ciphertext": "ignored",
                            "fmt": "base64"
                          }
                          """;

            var success = InspectorHelpers.TryReadEnvelopeMetadata(payload, file, out var inspection, out var errorMessage);

            Assert.That(success, Is.True);
            Assert.That(errorMessage, Is.Empty);
            Assert.That(inspection.Version.Value, Is.EqualTo("0"));
            Assert.That(inspection.Version.Legacy, Is.True);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Test]
    public void TryReadEnvelopeMetadata_LeavesUnknownAlgorithmDetailsEmpty()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempPath, "abc");
            var file = new FileInfo(tempPath);

            var payload = """
                          {
                            "version": "9",
                            "alg": "UNKNOWN",
                            "nonce": "ignored",
                            "tag": "ignored",
                            "ciphertext": "ignored",
                            "fmt": "base64"
                          }
                          """;

            var success = InspectorHelpers.TryReadEnvelopeMetadata(payload, file, out var inspection, out var errorMessage);

            Assert.That(success, Is.True);
            Assert.That(errorMessage, Is.Empty);
            Assert.That(inspection.Version.Value, Is.EqualTo("9"));
            Assert.That(inspection.Algorithm.Id, Is.EqualTo("UNKNOWN"));
            Assert.That(inspection.Algorithm.Name, Is.Null);
            Assert.That(inspection.Algorithm.KeySizeBytes, Is.Null);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [TestCase("not-json")]
    [TestCase("nonce.tag.ciphertext")]
    [TestCase("[]")]
    public void TryReadEnvelopeMetadata_ReturnsFalse_ForInvalidJson(string payload)
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempPath, "abc");
            var file = new FileInfo(tempPath);

            var success = InspectorHelpers.TryReadEnvelopeMetadata(payload, file, out _, out var errorMessage);

            Assert.That(success, Is.False);
            Assert.That(errorMessage, Is.EqualTo("File does not contain a recognized encrypted envelope."));
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Test]
    public void TryReadEnvelopeMetadata_OmitsOptionalFields_WhenMissingOrInvalid()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempPath, "abc");
            var file = new FileInfo(tempPath);

            var payload = """
                          {
                            "version": "1",
                            "alg": "A256GCM",
                            "kdf": "",
                            "iter": "not-a-number",
                            "salt": "",
                            "nonce": "ignored",
                            "tag": "ignored",
                            "ciphertext": "ignored",
                            "fmt": ""
                          }
                          """;

            var success = InspectorHelpers.TryReadEnvelopeMetadata(payload, file, out var inspection, out var errorMessage);

            Assert.That(success, Is.True);
            Assert.That(errorMessage, Is.Empty);
            Assert.That(inspection.Envelope.Kdf, Is.Null);
            Assert.That(inspection.Envelope.Iterations, Is.Null);
            Assert.That(inspection.Envelope.Salt, Is.Null);
            Assert.That(inspection.Envelope.Format, Is.Null);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
