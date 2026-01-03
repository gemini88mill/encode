namespace Encode;

internal static class EnvelopeVersion
{
    public const string Legacy = "0";
    public const string Current = "1";

    public static string Normalize(string? version)
    {
        return string.IsNullOrWhiteSpace(version) ? Legacy : version.Trim();
    }
}
