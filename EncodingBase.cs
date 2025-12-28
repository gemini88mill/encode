namespace Encode;

public abstract class EncodingBase
{
    public string TransformToString(string input, bool isFile, bool decode)
    {
        var text = isFile ? File.ReadAllText(input) : input;
        return decode ? Decode(text) : Encode(text);
    }

    public string TransformToFile(string input, bool isFile, string outputPath, bool decode)
    {
        var output = TransformToString(input, isFile, decode);
        File.WriteAllText(outputPath, output);
        return output;
    }

    protected abstract string Encode(string input);

    protected abstract string Decode(string input);
}
