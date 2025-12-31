namespace Encode.Tests;

public class HasherTests
{
    private const string SampleInput = "abc";

    private static IEnumerable<TestCaseData> Hashers()
    {
        yield return new TestCaseData(new Md5Hasher(), "900150983cd24fb0d6963f7d28e17f72")
            .SetName("Md5_abc");
        yield return new TestCaseData(new Sha1Hasher(), "a9993e364706816aba3e25717850c26c9cd0d89d")
            .SetName("Sha1_abc");
        yield return new TestCaseData(new Sha256Hasher(), "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")
            .SetName("Sha256_abc");
        yield return new TestCaseData(new Sha384Hasher(), "cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7")
            .SetName("Sha384_abc");
        yield return new TestCaseData(new Sha512Hasher(), "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f")
            .SetName("Sha512_abc");
    }

    [TestCaseSource(nameof(Hashers))]
    public void HashToString_UsesLowercaseHex(HasherBase hasher, string expectedHex)
    {
        var result = hasher.HashToString(SampleInput, isFile: false, OutputFormat.Hex, upperCaseHex: false);

        Assert.That(result, Is.EqualTo(expectedHex));
    }

    [TestCaseSource(nameof(Hashers))]
    public void HashToString_UsesUppercaseHex(HasherBase hasher, string expectedHex)
    {
        var result = hasher.HashToString(SampleInput, isFile: false, OutputFormat.Hex, upperCaseHex: true);

        Assert.That(result, Is.EqualTo(expectedHex.ToUpperInvariant()));
    }

    [TestCaseSource(nameof(Hashers))]
    public void HashToString_FromFileMatchesStringInput(HasherBase hasher, string expectedHex)
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempPath, SampleInput);

            var result = hasher.HashToString(tempPath, isFile: true, OutputFormat.Hex, upperCaseHex: false);

            Assert.That(result, Is.EqualTo(expectedHex));
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Test]
    public void HashToFile_WritesExpectedOutput()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            var hasher = new Md5Hasher();

            var result = hasher.HashToFile(SampleInput, isFile: false, tempPath, OutputFormat.Hex, upperCaseHex: false);
            var fileContents = File.ReadAllText(tempPath);

            Assert.That(result, Is.EqualTo(fileContents));
            Assert.That(result, Is.EqualTo("900150983cd24fb0d6963f7d28e17f72"));
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
