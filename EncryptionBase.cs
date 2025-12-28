using System.Text;

namespace Encode;

public sealed record EncryptionPayload(byte[] Nonce, byte[] Tag, byte[] Ciphertext);

public abstract class EncryptionBase
{
    protected virtual int NonceSize => 12;
    protected virtual int TagSize => 16;

    public string EncryptToString(
        string input,
        bool isFile,
        byte[] key,
        byte[]? nonce,
        OutputFormat format,
        bool upperCaseHex,
        byte[]? associatedData = null)
    {
        byte[] plaintext = isFile
            ? File.ReadAllBytes(input)
            : Encoding.UTF8.GetBytes(input);

        var payload = Encrypt(plaintext, key, nonce, associatedData);
        return FormatPayload(payload, format, upperCaseHex);
    }

    public string EncryptToFile(
        string input,
        bool isFile,
        string outputPath,
        byte[] key,
        byte[]? nonce,
        OutputFormat format,
        bool upperCaseHex,
        byte[]? associatedData = null)
    {
        var payload = EncryptToString(input, isFile, key, nonce, format, upperCaseHex, associatedData);
        File.WriteAllText(outputPath, payload);
        return payload;
    }

    public string DecryptToString(
        string input,
        bool isFile,
        byte[] key,
        OutputFormat format,
        byte[]? associatedData = null)
    {
        var payloadText = isFile ? File.ReadAllText(input) : input;
        var payload = ParsePayload(payloadText, format);
        var plaintext = Decrypt(payload, key, associatedData);
        return Encoding.UTF8.GetString(plaintext);
    }

    public string DecryptToFile(
        string input,
        bool isFile,
        string outputPath,
        byte[] key,
        OutputFormat format,
        byte[]? associatedData = null)
    {
        var payloadText = isFile ? File.ReadAllText(input) : input;
        var payload = ParsePayload(payloadText, format);
        var plaintext = Decrypt(payload, key, associatedData);
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

    protected static string FormatPayload(EncryptionPayload payload, OutputFormat format, bool upperCaseHex)
    {
        var nonce = FormatBytes(payload.Nonce, format, upperCaseHex);
        var tag = FormatBytes(payload.Tag, format, upperCaseHex);
        var ciphertext = FormatBytes(payload.Ciphertext, format, upperCaseHex);
        return $"{nonce}.{tag}.{ciphertext}";
    }

    protected static EncryptionPayload ParsePayload(string input, OutputFormat format)
    {
        var trimmed = input.Trim();
        var parts = trimmed.Split('.', 3, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
        {
            throw new FormatException("Encrypted payload must be in the form nonce.tag.ciphertext.");
        }

        var nonce = ParseBytes(parts[0], format);
        var tag = ParseBytes(parts[1], format);
        var ciphertext = ParseBytes(parts[2], format);

        return new EncryptionPayload(nonce, tag, ciphertext);
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
}
