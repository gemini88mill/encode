using System.Text.Json;
using Encode.Models;

namespace Encode.Inspector;

internal static class InspectorHelpers
{
    private static readonly Dictionary<string, AlgorithmDetails> AlgorithmMetadata = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A256GCM"] = new AlgorithmDetails("AES-256-GCM", Aes256GcmEncrypter.KeySize)
    };

    public static bool TryReadEnvelopeMetadata(
        string payloadText,
        FileInfo file,
        out EnvelopeInspectOutput inspection,
        out string errorMessage)
    {
        inspection = new EnvelopeInspectOutput();
        errorMessage = string.Empty;

        if (!TryParseEnvelopeJson(payloadText, out var envelopeData))
        {
            errorMessage = "File does not contain a recognized encrypted envelope.";
            return false;
        }

        var versionValue = EnvelopeVersion.Normalize(envelopeData.Version);
        var algorithmId = string.IsNullOrWhiteSpace(envelopeData.Algorithm)
            ? "unknown"
            : envelopeData.Algorithm;

        var algorithmInfo = new EnvelopeAlgorithmInfo
        {
            Id = algorithmId
        };

        if (AlgorithmMetadata.TryGetValue(algorithmId, out var details))
        {
            algorithmInfo.Name = details.Name;
            algorithmInfo.KeySizeBytes = details.KeySizeBytes;
        }

        inspection = new EnvelopeInspectOutput
        {
            Version = new EnvelopeVersionInfo
            {
                Value = versionValue,
                Legacy = string.Equals(versionValue, EnvelopeVersion.Legacy, StringComparison.OrdinalIgnoreCase)
            },
            Algorithm = algorithmInfo,
            File = new EnvelopeFileInfo
            {
                Path = file.FullName,
                Name = file.Name,
                SizeBytes = file.Length,
                LastModifiedUtc = file.LastWriteTimeUtc.ToString("O")
            },
            Envelope = new EnvelopeMetadataInfo
            {
                Kdf = string.IsNullOrWhiteSpace(envelopeData.Kdf) ? null : envelopeData.Kdf,
                Iterations = envelopeData.Iterations > 0 ? envelopeData.Iterations : null,
                Salt = string.IsNullOrWhiteSpace(envelopeData.Salt) ? null : envelopeData.Salt,
                Format = string.IsNullOrWhiteSpace(envelopeData.Format) ? null : envelopeData.Format
            }
        };

        return true;
    }

    private static bool TryParseEnvelopeJson(string payloadText, out EnvelopeJsonData data)
    {
        data = new EnvelopeJsonData();
        var trimmed = payloadText.Trim();

        if (!trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var root = document.RootElement;
            data = new EnvelopeJsonData
            {
                Version = TryGetString(root, "version"),
                Algorithm = TryGetString(root, "alg"),
                Kdf = TryGetString(root, "kdf"),
                Iterations = TryGetInt(root, "iter"),
                Salt = TryGetString(root, "salt"),
                Format = TryGetString(root, "fmt")
            };

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }

    private static int TryGetInt(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.Number
            && property.TryGetInt32(out var value))
        {
            return value;
        }

        return 0;
    }
}
