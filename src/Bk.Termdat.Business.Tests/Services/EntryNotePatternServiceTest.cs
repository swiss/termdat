using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
public class EntryNotePatternServiceTest
{
    private IEntryNotePatternService service;
    private readonly IOptions<EntryNotePatternOptions> entryNotePatternOptionsMock = Substitute.For<IOptions<EntryNotePatternOptions>>();

    [SetUp]
    public void SetUp()
    {
        entryNotePatternOptionsMock.Value.Returns(new EntryNotePatternOptions
        {
            ObsoletePattern = "veraltet|désuet|disusato|antiquà|obsolete",
            RarePattern = "selten|rare|raro|rar|rare",
            ShouldAvoidPattern = "zu vermeiden|à éviter|da evitare|d'evitar|to avoid",
            StartingPattern = "USG:"
        });

        service = new EntryNotePatternService(entryNotePatternOptionsMock);
    }

    [TestCase("USG: disusato; HIS: 1915-1929 (a)(b)", true)]
    [TestCase("USG: désuet; HIS: 1915-1929 (a)(b)", true)]
    [TestCase("USG: veraltet; HIS: 1915-1929 veraltet (a)(b)", true)]
    [TestCase("USG: selten; HIS: 1915-1929 veraltet (a)(b)", false)]
    [TestCase("USG: to avoid, obsolete;", true)]
    [TestCase("USG: veraltet, selten;", true)]
    [TestCase("USG:antiquà;", true)]
    [TestCase("USG:veraltet, selten;", true)]
    [TestCase("USG:zu vermeiden, obsolete (a)(b)", true)]
    [TestCase("USG: d'evitar, désuet;", true)]
    [TestCase("REG: Kanton BE; USG: veraltet, EXP: Abteilung des Obergerichts; HIS: per 31.12.2010 aufgehoben", true)]
    [TestCase(null, false)]
    public void ShouldDetectIsObsoleteCorrectly(string text, bool expected)
    {
        Assert.That(service.IsObsolete(text), Is.EqualTo(expected));
    }

    [TestCase("USG: raro; HIS: 1915-1929 (a)(b)", true)]
    [TestCase("USG: rare; HIS: 1915-1929 (a)(b)", true)]
    [TestCase("USG: selten; HIS: 1915-1929 veraltet (a)(b)", true)]
    [TestCase("USG: veraltet; HIS: 1915-1929 selten (a)(b)", false)]
    [TestCase("USG: to avoid, rare;", true)]
    [TestCase("USG: selten, veraltet;", true)]
    [TestCase("USG:rar;", true)]
    [TestCase("USG:rar", true)]
    [TestCase("USG:veraltet, selten;", true)]
    [TestCase("USG:zu vermeiden, rare (a)(b)", true)]
    [TestCase("USG: d'evitar, rare;", true)]
    [TestCase("REG: Kanton BE; USG: selten, EXP: Abteilung des Obergerichts; HIS: per 31.12.2010 aufgehoben", true)]
    [TestCase("USG: häufig.Dieser Eintrag ist erstellt worden, weil er seltene Tiere beschreibt", false)]
    [TestCase("USG: häufig.Dieser Eintrag ist erstellt worden, weil er selten Tiere beschreibt", false)]
    [TestCase(null, false)]
    public void ShouldDetectIsRareCorrectly(string text, bool expected)
    {
        Assert.That(service.IsRare(text), Is.EqualTo(expected));
    }

    [TestCase("USG: à éviter; HIS: 1915-1929 (a)(b)", true)]
    [TestCase("USG: da evitare; HIS: 1915-1929 (a)(b)", true)]
    [TestCase("USG: d'evitar; HIS: 1915-1929 veraltet (a)(b)", true)]
    [TestCase("USG: veraltet; HIS: 1915-1929 zu vermeiden (a)(b)", false)]
    [TestCase("USG: rare, to avoid;", true)]
    [TestCase("USG: selten, zu vermeiden;", true)]
    [TestCase("USG:d'evitar;", true)]
    [TestCase("USG:zu vermeiden, selten;", true)]
    [TestCase("USG:rare, à éviter (a)(b)", true)]
    [TestCase("USG: antiquà, d'evitar;", true)]
    [TestCase("REG: Kanton BE; USG: zu vermeiden, EXP: Abteilung des Obergerichts; HIS: per 31.12.2010 aufgehoben", true)]
    [TestCase(null, false)]
    public void ShouldDetectAvoidCorrectly(string text, bool expected)
    {
        Assert.That(service.ShouldAvoid(text), Is.EqualTo(expected));
    }

    [TestCase("USG: veraltet", true, false, false)]
    [TestCase("USG: selten", false, true, false)]
    [TestCase("USG: zu vermeiden", false, false, true)]
    [TestCase("Anmerkung ohne Einstufung", false, false, false)]
    public void DetermineNotePatterns_ShouldCallChecksCorrectly(string note, bool isObsolete, bool isRare, bool shouldAvoid)
    {
        service = Substitute.For<EntryNotePatternService>(entryNotePatternOptionsMock);
        var (actualIsObsolete, actualIsRare, actualShouldAvoid) = service.DetermineEntryNotePatterns(note);
        Assert.Multiple(() =>
        {
            Assert.That(actualIsObsolete, Is.EqualTo(isObsolete));
            Assert.That(actualIsRare, Is.EqualTo(isRare));
            Assert.That(actualShouldAvoid, Is.EqualTo(shouldAvoid));
            service.Received(1).IsObsolete(note);
            service.Received(1).IsRare(note);
            service.Received(1).ShouldAvoid(note);
        });
    }
}
