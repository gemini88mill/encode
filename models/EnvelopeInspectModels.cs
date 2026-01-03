using System.Text.Json.Serialization;

namespace Encode.Models;

internal sealed record AlgorithmDetails(string Name, int KeySizeBytes);

internal sealed class EnvelopeJsonData
{
    public string? Version { get; init; }
    public string? Algorithm { get; init; }
    public string? Kdf { get; init; }
    public int Iterations { get; init; }
    public string? Salt { get; init; }
    public string? Format { get; init; }
}

internal sealed class EnvelopeInspectOutput
{
    [JsonPropertyName("version")]
    public EnvelopeVersionInfo Version { get; init; } = new();

    [JsonPropertyName("algorithm")]
    public EnvelopeAlgorithmInfo Algorithm { get; init; } = new();

    [JsonPropertyName("file")]
    public EnvelopeFileInfo File { get; init; } = new();

    [JsonPropertyName("envelope")]
    public EnvelopeMetadataInfo Envelope { get; init; } = new();
}

internal sealed class EnvelopeVersionInfo
{
    [JsonPropertyName("value")]
    public string Value { get; init; } = EnvelopeVersion.Legacy;

    [JsonPropertyName("legacy")]
    public bool Legacy { get; init; }
}

internal sealed class EnvelopeAlgorithmInfo
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = "unknown";

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("keySizeBytes")]
    public int? KeySizeBytes { get; set; }
}

internal sealed class EnvelopeFileInfo
{
    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("sizeBytes")]
    public long SizeBytes { get; init; }

    [JsonPropertyName("lastModifiedUtc")]
    public string LastModifiedUtc { get; init; } = string.Empty;
}

internal sealed class EnvelopeMetadataInfo
{
    [JsonPropertyName("kdf")]
    public string? Kdf { get; init; }

    [JsonPropertyName("iterations")]
    public int? Iterations { get; init; }

    [JsonPropertyName("salt")]
    public string? Salt { get; init; }

    [JsonPropertyName("format")]
    public string? Format { get; init; }
}
