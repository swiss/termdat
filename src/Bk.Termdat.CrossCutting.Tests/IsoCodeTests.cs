using NUnit.Framework;

namespace Bk.Termdat.CrossCutting.Tests;

internal class IsoCodeTests
{
    [TestCase("dE", IsoCode.De)]
    [TestCase("de", IsoCode.De)]
    [TestCase("DE", IsoCode.De)]
    [TestCase("De", IsoCode.De)]
    [TestCase("EN", IsoCode.En)]
    [TestCase("fr", IsoCode.Fr)]
    [TestCase("It", IsoCode.It)]
    [TestCase("rM", IsoCode.Rm)]
    public void EqualsTest(string codeText, IsoCode code)
    {
        Assert.That(code.IsEqualTo(codeText));
    }
}
