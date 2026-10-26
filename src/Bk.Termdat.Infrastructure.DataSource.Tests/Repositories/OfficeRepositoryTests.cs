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
internal class OfficeRepositoryTests
{
    private OfficeRepository officeRepository;
    private DataContext context;

    [SetUp]
    public void Setup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        officeRepository = new OfficeRepository(context);
    }

    [TearDown]
    public void TearDown()
    {
        context.Dispose();
    }

    [Test]
    public async Task CreateShouldAddOffice()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
           .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

        using var createContext = new DataContext(builder.Options);
        var repository = new OfficeRepository(createContext);

        var newOffice = new Office
        {
            Code = "code",
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Modified = DateTime.UtcNow,
            ModifiedBy = "me@update.com",
        };

        await repository.Create(newOffice);

        await createContext.SaveChangesAsync();

        Assert.That(newOffice.Id, Is.Not.Default);
    }

    [Test]
    public async Task UpdateShouldUpdateOffice()
    {
        var databaseOffice = await officeRepository.GetById(1);
        var translations = databaseOffice.Translations.ToArray();
        var updatedOffice = new Office
        {
            Code = "qwe12",
            ConcurrencyToken = databaseOffice.ConcurrencyToken,
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Id = databaseOffice.Id,
            Modified = DateTime.UtcNow,
            ModifiedBy = "me@me.kong",
            Translations = new List<OfficeTranslation>
            {
                translations[0],
                translations[1],
                translations[2]
            }
        };

        officeRepository.Update(databaseOffice, updatedOffice);

        await context.SaveChangesAsync();

        var result = await officeRepository.GetById(1);
        Assert.That(result.Code, Is.EqualTo(updatedOffice.Code));
        Assert.That(result.Created, Is.EqualTo(updatedOffice.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(updatedOffice.CreatedBy));
        Assert.That(result.Id, Is.EqualTo(updatedOffice.Id));
        Assert.That(result.Modified, Is.EqualTo(updatedOffice.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(updatedOffice.ModifiedBy));
        Assert.That(result.Translations, Is.EqualTo(updatedOffice.Translations));
    }

    [Test]
    public async Task UpdateShouldHandleAdditionalTranslations()
    {
        var databaseOffice = await officeRepository.GetById(2);
        var translations = databaseOffice.Translations.ToArray();
        var updatedOffice = new Office
        {
            Code = "qwe12",
            ConcurrencyToken = databaseOffice.ConcurrencyToken,
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Id = databaseOffice.Id,
            Modified = DateTime.UtcNow,
            ModifiedBy = "me@me.kong",
            Translations = new List<OfficeTranslation>
            {
                translations[0],
                translations[1],
                translations[2],
                new()
                {
                    LanguageId = 4,
                    OfficeId = 1,
                    Name = "newOne"
                }
            }
        };

        officeRepository.Update(databaseOffice, updatedOffice);

        var result = await officeRepository.GetById(2);
        Assert.That(databaseOffice.Translations, Is.EqualTo(result.Translations));
    }

    [Test]
    public async Task UpdateShouldThrowExceptionForMultipleTranslations()
    {
        var databaseOffice = await officeRepository.GetById(2);
        var translations = databaseOffice.Translations.ToArray();
        var updatedOffice = new Office
        {
            Code = "qwe12",
            ConcurrencyToken = databaseOffice.ConcurrencyToken,
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Id = databaseOffice.Id,
            Modified = DateTime.UtcNow,
            ModifiedBy = "me@me.kong",
            Translations = new List<OfficeTranslation>
            {
                translations[0],
            }
        };
        databaseOffice.Translations = new List<OfficeTranslation>
        {
            translations[0],
            translations[0]
        };

        Assert.Throws<InvalidOperationException>(() => officeRepository.Update(databaseOffice, updatedOffice));
    }

    [Test]
    public async Task GetAllReturns()
    {
        var data = (await officeRepository.GetAll()).ToList();
        Assert.That(data, Is.Not.Null);

        var office = data.First();
        AssertFirst(office);

        office = data.Last();
        Assert.That(data, Is.Not.Null);
        Assert.That(office.Id, Is.Not.Default);

        Assert.That(office.Code, Is.EqualTo("ALX"));

        Assert.That(office.Translations.Count, Is.EqualTo(3));
        Assert.That(office.Translations.All(t => t.Language != null));
        var translation = office.Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.OfficeId, Is.EqualTo(2));
        Assert.That(translation.Name, Is.EqualTo("Übersetzerinnen und Übersetzer der Bundesverwaltung"));

        translation = office.Translations.Last();
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.OfficeId, Is.EqualTo(2));
        Assert.That(translation.Name, Is.EqualTo("Traduttrici e traduttori dell'Amministrazione federale"));
    }

    [Test]
    public async Task GetAllDetailsShouldReturnOffices()
    {
        var data = (await officeRepository.GetAllDetails()).ToList();

        Assert.That(data.Count, Is.EqualTo(2));
        Assert.That(data[0].EntryCount, Is.EqualTo(2));
        Assert.That(data[1].EntryCount, Is.EqualTo(3));

        AssertOffices(data.Select(d => d.Office).ToList());
    }

    [Test]
    public async Task GetByIdReturns()
    {
        var data = await officeRepository.GetById(1);
        AssertFirst(data);

        var data2 = await officeRepository.GetById(2);
        Assert.That(data2.Id, Is.Not.Default);
        Assert.That(data2.Id, Is.EqualTo(2));
        Assert.That(data2.Code, Is.EqualTo("ALX"));
    }

    [Test]
    public async Task GetEntryCountByIdReturns()
    {
        var entryCountById1 = await officeRepository.GetEntryCountById(1);
        Assert.That(entryCountById1, Is.EqualTo(2));

        var entryCountById2 = await officeRepository.GetEntryCountById(2);
        Assert.That(entryCountById2, Is.EqualTo(3));
    }

    [Test]
    public void DeleteRemovesOffice()
    {
        var officeCount = context.Offices.Count();
        var office = context.Offices.First();

        officeRepository.Delete(office);
        context.SaveChanges();

        Assert.That(context.Offices.Count(), Is.EqualTo(officeCount - 1));
        Assert.That(context.Offices, Is.Not.Contains(office));
    }

    private static void AssertOffices(List<Office> data)
    {
        Assert.That(data, Is.Not.Null);

        var office = data.First();

        AssertFirst(office);

        office = data.Last();

        Assert.That(data, Is.Not.Null);
        Assert.That(office.Id, Is.Not.Default);

        Assert.That(office.Code, Is.EqualTo("ALX"));

        Assert.That(office.Translations.Count, Is.EqualTo(3));
        Assert.That(office.Translations.All(t => t.Language != null));
        var translation = office.Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.OfficeId, Is.EqualTo(2));
        Assert.That(translation.Name,
            Is.EqualTo("Übersetzerinnen und Übersetzer der Bundesverwaltung"));

        translation = office.Translations.Last();

        Assert.That(office.Code, Is.EqualTo("ALX"));
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.OfficeId, Is.EqualTo(2));
        Assert.That(translation.Name,
            Is.EqualTo("Traduttrici e traduttori dell'Amministrazione federale"));
    }

    private static void AssertFirst(Office office)
    {
        Assert.That(office.Id, Is.Not.Default);

        Assert.That(office.Code, Is.EqualTo("ACH"));

        Assert.That(office.Translations.Count, Is.EqualTo(3));
        Assert.That(office.Translations.All(t => t.Language != null));
        var translation = office.Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.OfficeId, Is.EqualTo(1));
        Assert.That(translation.Name, Is.EqualTo("Schweizerische Bundesverwaltung"));

        translation = office.Translations.Last();
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.OfficeId, Is.EqualTo(1));
        Assert.That(translation.Name, Is.EqualTo("Amministrazione federale svizzera"));
    }
}
