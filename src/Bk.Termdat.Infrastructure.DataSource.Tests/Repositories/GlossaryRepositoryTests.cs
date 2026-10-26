using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class GlossaryRepositoryTests
{
    private GlossaryRepository glossaryRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        glossaryRepository = new GlossaryRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [Test]
    public async Task GetAllDetailsShouldReturnGlossaries()
    {
        var data = (await glossaryRepository.GetAllDetails()).ToList();

        Assert.That(data.Count, Is.EqualTo(3));
        Assert.That(data[0].EntryCount, Is.EqualTo(2));
        Assert.That(data[1].EntryCount, Is.EqualTo(1));
        Assert.That(data[2].EntryCount, Is.EqualTo(1));

        AssertGlossaries(data.Select(d => d.Glossary).ToList());
    }

    [Test]
    public async Task GetAllReturns()
    {
        var data = (await glossaryRepository.GetAll()).ToList();

        AssertGlossaries(data);
    }

    [Test]
    public async Task GetByIdReturns()
    {
        var data = await glossaryRepository.GetById(1);
        AssertFirst(data);
    }

    [Test]
    public async Task CreateShouldAddGlossaries()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

        using var createContext = new DataContext(builder.Options);
        var repository = new GlossaryRepository(createContext);

        var newGlossary = new Glossary
        {
            Code = "newCode",
            Description = "Beschreibung",
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
        };

        await repository.Create(newGlossary);

        await createContext.SaveChangesAsync();

        Assert.That(newGlossary.Id, Is.Not.Default);
    }

    [Test]
    public async Task UpdateShouldUpdateGlossary()
    {
        var databaseGlossary = await glossaryRepository.GetById(1);
        var translations = databaseGlossary.Translations.ToList();
        var updatedGlossary = new Glossary
        {
            Id = databaseGlossary.Id,
            Code = "1234567890",
            Description = "Beschreibung",
            Created = DateTime.UtcNow,
            CreatedBy = "ItseMe",
            Modified = DateTime.UtcNow,
            ModifiedBy = "ItsemMe",
            Translations = new List<GlossaryTranslation>
                {
                    translations[0],
                    translations[1],
                    translations[2]
                }
        };

        glossaryRepository.Update(databaseGlossary, updatedGlossary);

        await context.SaveChangesAsync();

        var updatedDatabaseGlossary = await glossaryRepository.GetById(1);
        Assert.That(updatedDatabaseGlossary.Code, Is.EqualTo(updatedGlossary.Code));
        Assert.That(updatedDatabaseGlossary.Description, Is.EqualTo(updatedGlossary.Description));
        Assert.That(updatedDatabaseGlossary.Created, Is.EqualTo(updatedGlossary.Created));
        Assert.That(updatedDatabaseGlossary.CreatedBy, Is.EqualTo(updatedGlossary.CreatedBy));
        Assert.That(updatedDatabaseGlossary.Translations, Is.EqualTo(updatedGlossary.Translations));
    }

    [Test]
    public async Task UpdateShouldHandleAdditionalTranslations()
    {
        var databaseGlossary = await glossaryRepository.GetById(2);
        var translations = databaseGlossary.Translations.ToList();
        var updatedGlossary = new Glossary
        {
            Id = databaseGlossary.Id,
            Code = "1234567890",
            Description = "Beschreibung",
            Created = DateTime.UtcNow,
            CreatedBy = "ItseMe",
            Modified = DateTime.UtcNow,
            ModifiedBy = "ItsemMe",
            Translations = new List<GlossaryTranslation>
                {
                    translations[0],
                    translations[1],
                    translations[2],
                    new()
                    {
                        LanguageId = 4,
                        GlossaryId = 1,
                        Name = "NewName"
                    }
                }
        };

        glossaryRepository.Update(databaseGlossary, updatedGlossary);

        await context.SaveChangesAsync();

        var updatedDatabaseGlossary = await glossaryRepository.GetById(2);
        Assert.That(updatedDatabaseGlossary.Translations, Is.EqualTo(updatedGlossary.Translations));
    }

    [Test]
    public void DeleteRemovesGlossary()
    {
        var glossaryId = 12345;
        var glossary = new Glossary
        {
            Id = glossaryId,
            Code = "code",
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
        };

        context.Add(glossary);

        context.SaveChanges();

        Assert.That(context.Glossaries.SingleOrDefault(c => c.Id == glossaryId), Is.Not.Null);

        glossaryRepository.Delete(glossary);

        context.SaveChanges();

        Assert.That(context.Glossaries.SingleOrDefault(c => c.Id == glossaryId), Is.Null);
    }

    [Test]
    public async Task GetAuthorizedByIds_WhenCalled_ReturnsAuthorizedGlossaries()
    {
        var data = (await glossaryRepository.GetAuthorizedByIds(new[] { 1, 3 })).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(data, Is.Not.Empty);
            Assert.That(data, Has.Count.EqualTo(2));

            Assert.That(data.First().Id, Is.EqualTo(1));
            Assert.That(data.First().Code, Is.EqualTo("Glo1"));

            Assert.That(data.Last().Id, Is.EqualTo(3));
            Assert.That(data.Last().Code, Is.EqualTo("Glo3"));
        });
    }

    private static void AssertGlossaries(List<Glossary> data)
    {
        Assert.That(data, Is.Not.Null);

        var glossary = data.First();

        AssertFirst(glossary);

        glossary = data.Last();

        Assert.That(data, Is.Not.Null);
        Assert.That(glossary.Id, Is.Not.Default);

        Assert.That(glossary.Code, Is.EqualTo("Glo3"));

        Assert.That(glossary.Translations.Count, Is.EqualTo(3));
        Assert.That(glossary.Translations.All(t => t.Language != null));
        var translation = glossary.Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.GlossaryId, Is.EqualTo(3));
        Assert.That(translation.Name, Is.EqualTo("Glossary 3"));

        translation = glossary.Translations.Last();
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.GlossaryId, Is.EqualTo(3));
        Assert.That(translation.Name, Is.EqualTo("Glossario 3"));
    }

    private static void AssertFirst(Glossary glossary)
    {
        Assert.That(glossary.Id, Is.Not.Default);

        Assert.That(glossary.Code, Is.EqualTo("Glo1"));

        Assert.That(glossary.Translations.Count, Is.EqualTo(3));
        Assert.That(glossary.Translations.All(t => t.Language != null));
        var translation = glossary.Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.GlossaryId, Is.EqualTo(1));
        Assert.That(translation.Name, Is.EqualTo("Glossary 1"));

        translation = glossary.Translations.Last();
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.GlossaryId, Is.EqualTo(1));
        Assert.That(translation.Name, Is.EqualTo("Glossario 1"));
    }
}
