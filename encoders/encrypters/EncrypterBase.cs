using System.Text;
using System.Text.Json;
using Encode.Models;

namespace Encode;

public abstract class EncrypterBase
{
    private static readonly JsonSerializerOptions EnvelopeSerializationOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null
    };

    public int RequiredKeySize => KeySizeBytes;
    public string Algorithm => AlgorithmId;

    protected virtual int NonceSize => 12;
    protected virtual int TagSize => 16;
    protected abstract string AlgorithmId { get; }
    protected abstract int KeySizeBytes { get; }

    public string EncryptToString(
        string input,
        bool isFile,
        byte[] key,
        byte[]? nonce,
        OutputFormat format,
        bool upperCaseHex,
        EncryptionEnvelopeMetadata envelopeMetadata,
        byte[]? associatedData = null)
    {
        byte[] plaintext = isFile
            ? File.ReadAllBytes(input)
            : Encoding.UTF8.GetBytes(input);

        var payload = Encrypt(plaintext, key, nonce, associatedData);
        return FormatPayload(payload, envelopeMetadata, format, upperCaseHex);
    }

    public string EncryptToFile(
        string input,
        bool isFile,
        string outputPath,
        byte[] key,
        byte[]? nonce,
        OutputFormat format,
        bool upperCaseHex,
        EncryptionEnvelopeMetadata envelopeMetadata,
        byte[]? associatedData = null)
    {
        var payload = EncryptToString(input, isFile, key, nonce, format, upperCaseHex, envelopeMetadata, associatedData);
        File.WriteAllText(outputPath, payload);
        return payload;
    }

    public ParsedEncryptionEnvelope ReadEnvelope(
        string input,
        bool isFile,
        OutputFormat format)
    {
        var payloadText = isFile ? File.ReadAllText(input) : input;
        return ParsePayload(payloadText, format);
    }

    public string DecryptParsedEnvelopeToString(
        ParsedEncryptionEnvelope envelope,
        byte[] key,
        byte[]? associatedData = null)
    {
        ValidateAlgorithm(envelope.Metadata.Algorithm);

        var plaintext = Decrypt(envelope.Payload, key, associatedData);
        return Encoding.UTF8.GetString(plaintext);
    }

    public string DecryptParsedEnvelopeToFile(
        ParsedEncryptionEnvelope envelope,
        string outputPath,
        byte[] key,
        byte[]? associatedData = null)
    {
        ValidateAlgorithm(envelope.Metadata.Algorithm);

        var plaintext = Decrypt(envelope.Payload, key, associatedData);
        File.WriteAllBytes(outputPath, plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }

    protected abstract EncryptionPayload Encrypt(
        byte[] plaintext,
        byte[] key,
        byte[]? nonce,
        byte[]? associatedData);

    protected abstract byte[] Decrypt(
        EncryptionPayload payload,
        byte[] key,
        byte[]? associatedData);

    protected void ValidateNonce(byte[] nonce)
    {
        if (nonce.Length != NonceSize)
        {
            throw new ArgumentException($"Nonce must be {NonceSize} bytes.", nameof(nonce));
        }
    }

    protected void ValidateTag(byte[] tag)
    {
        if (tag.Length != TagSize)
        {
            throw new ArgumentException($"Tag must be {TagSize} bytes.", nameof(tag));
        }
    }

    protected static string FormatBytes(byte[] bytes, OutputFormat format, bool upperCaseHex)
    {
        return format switch
        {
            OutputFormat.Base64 => Convert.ToBase64String(bytes),
            OutputFormat.Hex => upperCaseHex
                ? Convert.ToHexString(bytes)
                : Convert.ToHexString(bytes).ToLowerInvariant(),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported output format.")
        };
    }

    protected static byte[] ParseBytes(string value, OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Base64 => Convert.FromBase64String(value),
            OutputFormat.Hex => Convert.FromHexString(value),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported output format.")
        };
    }

    private static string FormatPayload(
        EncryptionPayload payload,
        EncryptionEnvelopeMetadata envelopeMetadata,
        OutputFormat format,
        bool upperCaseHex)
    {
        if (envelopeMetadata.Format != format)
        {
            throw new ArgumentException("Envelope metadata format does not match requested output format.", nameof(envelopeMetadata));
        }

        var envelope = new EncryptionEnvelopeData
        {
            Version = envelopeMetadata.Version,
            Algorithm = envelopeMetadata.Algorithm,
            Kdf = envelopeMetadata.Kdf,
            Iterations = envelopeMetadata.Iterations,
            Salt = envelopeMetadata.Salt.Length == 0
                ? string.Empty
                : FormatBytes(envelopeMetadata.Salt, format, upperCaseHex),
            Nonce = FormatBytes(payload.Nonce, format, upperCaseHex),
            Tag = FormatBytes(payload.Tag, format, upperCaseHex),
            Ciphertext = FormatBytes(payload.Ciphertext, format, upperCaseHex),
            Format = format.ToString().ToLowerInvariant()
        };

        return JsonSerializer.Serialize(envelope, EnvelopeSerializationOptions);
    }

    private ParsedEncryptionEnvelope ParsePayload(string input, OutputFormat formatHint)
    {
        var trimmed = input.Trim();

        if (trimmed.StartsWith("{"))
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<EncryptionEnvelopeData>(trimmed, EnvelopeSerializationOptions);
                if (envelope is not null)
                {
                    return ParseEnvelope(envelope, formatHint);
                }
            }
            catch (JsonException)
            {
                // Fall back to legacy parsing below.
            }
        }

        var legacyPayload = ParseLegacyPayload(trimmed, formatHint);
        var legacyMetadata = new EncryptionEnvelopeMetadata(
            EnvelopeVersion.Legacy,
            AlgorithmId,
            "none",
            0,
            Array.Empty<byte>(),
            formatHint);

        return new ParsedEncryptionEnvelope(legacyMetadata, legacyPayload);
    }

    private ParsedEncryptionEnvelope ParseEnvelope(EncryptionEnvelopeData envelope, OutputFormat formatHint)
    {
        var resolvedFormat = ResolveFormat(envelope.Format, formatHint);
        var nonce = ParseBytes(envelope.Nonce, resolvedFormat);
        var tag = ParseBytes(envelope.Tag, resolvedFormat);
        var ciphertext = ParseBytes(envelope.Ciphertext, resolvedFormat);
        var saltBytes = string.IsNullOrWhiteSpace(envelope.Salt)
            ? Array.Empty<byte>()
            : ParseBytes(envelope.Salt, resolvedFormat);

        var metadata = new EncryptionEnvelopeMetadata(
            EnvelopeVersion.Normalize(envelope.Version),
            envelope.Algorithm,
            envelope.Kdf,
            envelope.Iterations,
            saltBytes,
            resolvedFormat);

        var payload = new EncryptionPayload(nonce, tag, ciphertext);
        return new ParsedEncryptionEnvelope(metadata, payload);
    }

    private EncryptionPayload ParseLegacyPayload(string input, OutputFormat format)
    {
        var parts = input.Split('.', 3, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
        {
            throw new FormatException("Encrypted payload must be in the form nonce.tag.ciphertext.");
        }

        var nonce = ParseBytes(parts[0], format);
        var tag = ParseBytes(parts[1], format);
        var ciphertext = ParseBytes(parts[2], format);

        return new EncryptionPayload(nonce, tag, ciphertext);
    }

    private OutputFormat ResolveFormat(string? formatText, OutputFormat fallback)
    {
        if (!string.IsNullOrWhiteSpace(formatText)
            && Enum.TryParse<OutputFormat>(formatText, true, out var parsed))
        {
            return parsed;
        }

        return fallback;
    }

    private void ValidateAlgorithm(string? envelopeAlgorithm)
    {
        if (!string.IsNullOrWhiteSpace(envelopeAlgorithm)
            && !string.Equals(envelopeAlgorithm, AlgorithmId, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException($"Envelope algorithm '{envelopeAlgorithm}' does not match expected '{AlgorithmId}'.");
        }
    }
}
