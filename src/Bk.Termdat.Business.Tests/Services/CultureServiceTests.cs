using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class CultureServiceTests
{
    private ICultureService service;

    [SetUp]
    public void SetUp()
    {
        service = new CultureService();
    }

    [Test]
    public void ReturnsCorrectDefaultIsoCode()
    {
        Assert.That(service.GetDefaultIsoCode(), Is.EqualTo("DE"));
    }

    [Test]
    [SetCulture("")]
    public void GetCurrentIsoCode_WithNoCulturePresent_ReturnsFallbackIsoCode()
    {
        Assert.That(service.GetCurrentIsoCode(), Is.EqualTo("DE"));
    }

    [Test]
    [SetCulture("de")]
    public void ReturnsCorrectCurrentCultureIsoCodeForDe()
    {
        Assert.That(service.GetCurrentIsoCode(), Is.EqualTo("DE"));
    }

    [Test]
    [SetCulture("en")]
    public void ReturnsCorrectCurrentCultureIsoCodeForEn()
    {
        Assert.That(service.GetCurrentIsoCode(), Is.EqualTo("EN"));
    }

    [Test]
    [SetCulture("fr")]
    public void ReturnsCorrectCurrentCultureIsoCodeForFr()
    {
        Assert.That(service.GetCurrentIsoCode(), Is.EqualTo("FR"));
    }

    [Test]
    [SetCulture("it")]
    public void ReturnsCorrectCurrentCultureIsoCodeForIt()
    {
        Assert.That(service.GetCurrentIsoCode(), Is.EqualTo("IT"));
    }

    [Test]
    [SetCulture("rm")]
    public void ReturnsCorrectCurrentCultureIsoCodeForRm()
    {
        Assert.That(service.GetCurrentIsoCode(), Is.EqualTo("RM"));
    }
}
