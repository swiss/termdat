using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class LenochClassificationRepositoryTests
{
    private LenochClassificationRepository lenochClassificationRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        lenochClassificationRepository = new LenochClassificationRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [Test]
    public async Task ShouldGetAll()
    {
        var data = (await lenochClassificationRepository.GetAll()).ToList();
        AssertLenochClassification(data);
    }

    [Test]
    public async Task ShouldGetByIds()
    {
        var data = (await lenochClassificationRepository.GetByIds(new[] { 1, 3 })).ToList();
        AssertLenochClassification(data);
    }

    [Test]
    public async Task ShouldGetAllDetails()
    {
        var data = (await lenochClassificationRepository.GetAllDetails()).ToList();
        var lenochClassifications = data.Select(d => d.Entity).ToList();
        AssertLenochClassification(lenochClassifications);
        Assert.That(data.All(d => d.EntryCount > 0));
    }

    private static void AssertLenochClassification(List<LenochClassification> data)
    {
        var lenochClassification = data.First();

        Assert.That(data, Is.Not.Null);
        Assert.That(lenochClassification.Id, Is.Not.Default);

        Assert.That(lenochClassification.Code, Is.EqualTo("AD"));

        Assert.That(lenochClassification.Translations.Count, Is.EqualTo(3));
        Assert.That(lenochClassification.Translations.All(t => t.Language != null));
        var translation = lenochClassification.Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.LenochClassificationId, Is.EqualTo(1));
        Assert.That(translation.Name, Is.EqualTo("ÖFFENTLICHE UND PRIVATE VERWALTUNG"));

        translation = lenochClassification.Translations.Last();
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.LenochClassificationId, Is.EqualTo(1));
        Assert.That(translation.Name, Is.EqualTo("AMMINISTRAZIONE PUBBLICA E PRIVATA"));

        lenochClassification = data.Last();

        Assert.That(data, Is.Not.Null);
        Assert.That(lenochClassification.Id, Is.Not.Default);

        Assert.That(lenochClassification.Code, Is.EqualTo("ORB"));

        Assert.That(lenochClassification.Translations.Count, Is.EqualTo(3));
        Assert.That(lenochClassification.Translations.All(t => t.Language != null));
        translation = lenochClassification.Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.LenochClassificationId, Is.EqualTo(3));
        Assert.That(translation.Name,
            Is.EqualTo("IAEA, INTERNATIONALE ATOMENERGIEAGENTUR (auch: AT3/AT8/DEK/JU3/JU6/PO8)"));

        translation = lenochClassification.Translations.Last();
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.LenochClassificationId, Is.EqualTo(3));
        Assert.That(translation.Name, Is.EqualTo("AIEA AGENZIA INTERNAZIONALE PER L'ENERGIA ATOMICA"));
    }
}
