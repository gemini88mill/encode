using System.Text.Json.Serialization;
using Encode;

namespace Encode.Models;

public sealed record EncryptionPayload(byte[] Nonce, byte[] Tag, byte[] Ciphertext);

public sealed record EncryptionEnvelopeMetadata(
    string Version,
    string Algorithm,
    string Kdf,
    int Iterations,
    byte[] Salt,
    OutputFormat Format);

public sealed record ParsedEncryptionEnvelope(EncryptionEnvelopeMetadata Metadata, EncryptionPayload Payload);

internal sealed class EncryptionEnvelopeData
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("alg")]
    public string Algorithm { get; set; } = string.Empty;

    [JsonPropertyName("kdf")]
    public string Kdf { get; set; } = "none";

    [JsonPropertyName("iter")]
    public int Iterations { get; set; }

    [JsonPropertyName("salt")]
    public string Salt { get; set; } = string.Empty;

    [JsonPropertyName("nonce")]
    public string Nonce { get; set; } = string.Empty;

    [JsonPropertyName("tag")]
    public string Tag { get; set; } = string.Empty;

    [JsonPropertyName("ciphertext")]
    public string Ciphertext { get; set; } = string.Empty;

    [JsonPropertyName("fmt")]
    public string Format { get; set; } = string.Empty;
}
