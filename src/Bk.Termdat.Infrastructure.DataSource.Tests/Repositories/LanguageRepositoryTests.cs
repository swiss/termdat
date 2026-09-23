using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Bogus;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class LanguageRepositoryTests
{
    private LanguageRepository languageRepository;
    private DataContext dataContext;
    private List<LanguageTranslation> languageTranslations = new();
    private readonly string[] isoCodes = { "DE", "EN", "FR", "IT", "RM" };

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var random = new Randomizer();
        var id = 0;
        var languageId = 1;
        var domainFaker = new Faker<LanguageTranslation>()
            .RuleFor(t => t.Name, _ => random.Word())
            .RuleFor(t => t.Id, _ => { id += 1; return id; })
            .RuleFor(t => t.IsoCode, _ => isoCodes[id - 1])
            .RuleFor(t => t.LanguageId, _ => languageId);

        languageTranslations = Enumerable.Range(1, 4).Select(_ => domainFaker.Generate()).ToList();

        var builder = new DbContextOptionsBuilder<DataContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString("n"));
        dataContext = new DataContext(builder.Options);

        dataContext.Languages.AddRange(languageTranslations.Select(lt => new Language { Id = lt.Id, IsUiLanguage = true, IsSystemLanguage = true, SortIndex = lt.Id, IsoCode = "de", CreatedBy = "test@test.ch", ModifiedBy = "test2@test.ch" }));

        dataContext.LanguageTranslations.AddRange(languageTranslations);
        dataContext.SaveChanges();

        languageRepository = new LanguageRepository(dataContext);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        dataContext.Dispose();
    }

    [Test]
    public async Task GetsByIsoCodeReturns()
    {
        var isoCode = isoCodes.First();
        var expected = languageTranslations.FindAll(l => l.IsoCode.Equals(isoCode));

        var result = (await languageRepository.GetTranslationByIsoCode(isoCode)).ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task CanGetAll()
    {
        var result = (await languageRepository.GetAll()).ToList();

        Assert.That(result.Count, Is.EqualTo(4));
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[3].Id, Is.EqualTo(4));
    }

    [Test]
    public async Task CanGetAllForValidation()
    {
        var result = (await languageRepository.GetAllForValidation()).ToList();

        Assert.That(result.Count, Is.EqualTo(4));
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[3].Id, Is.EqualTo(4));
    }

    [Test]
    public async Task CreateShouldAddLanguage()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

        var createContext = new DataContext(builder.Options);
        var repository = new LanguageRepository(createContext);

        var newLanguage = new Language
        {
            IsUiLanguage = true,
            IsSystemLanguage = false,
            SortIndex = 100,
            IsoCode = "pk",
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch"
        };

        await repository.Create(newLanguage);

        await createContext.SaveChangesAsync();

        Assert.That(newLanguage.Id, Is.Not.Default);
    }

    [Test]
    public async Task UpdateShouldUpdateLanguage()
    {
        var databaseLanguage = await languageRepository.GetById(1);
        var translations = databaseLanguage.Translations.ToList();
        var updatedLanguage = new Language
        {
            Id = databaseLanguage.Id,
            IsUiLanguage = false,
            IsSystemLanguage = false,
            IsoCode = "ru",
            SortIndex = 105,
            Created = DateTime.UtcNow,
            CreatedBy = "test",
            Modified = DateTime.UtcNow,
            ModifiedBy = "test2",
            Translations = new List<LanguageTranslation>
                {
                    translations[0],
                    translations[1],
                    translations[2],
                    translations[3],
                }
        };

        languageRepository.Update(databaseLanguage, updatedLanguage);

        await dataContext.SaveChangesAsync();

        var updatedDatabaseLangauge = await languageRepository.GetById(1);
        Assert.That(updatedDatabaseLangauge.IsUiLanguage, Is.EqualTo(updatedLanguage.IsUiLanguage));
        Assert.That(updatedDatabaseLangauge.SortIndex, Is.EqualTo(updatedLanguage.SortIndex));
        Assert.That(updatedDatabaseLangauge.IsoCode, Is.EqualTo(updatedLanguage.IsoCode));
        Assert.That(updatedDatabaseLangauge.Created, Is.EqualTo(updatedLanguage.Created));
        Assert.That(updatedDatabaseLangauge.CreatedBy, Is.EqualTo(updatedLanguage.CreatedBy));
    }
}
