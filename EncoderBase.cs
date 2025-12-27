namespace Encode;

public abstract class EncoderBase
{
    public abstract string EncodeToString(string input, bool isFile);

    public string EncodeToFile(string input, bool isFile, string outputPath)
    {
        var output = EncodeToString(input, isFile);
        File.WriteAllText(outputPath, output);
        return output;
    }
}
