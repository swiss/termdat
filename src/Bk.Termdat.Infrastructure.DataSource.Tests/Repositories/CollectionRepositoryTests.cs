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
internal class CollectionRepositoryTests
{
    private CollectionRepository collectionRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        collectionRepository = new CollectionRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [Test]
    public async Task GetAllDetailsShouldReturnCollections()
    {
        var data = (await collectionRepository.GetAllDetails()).ToList();

        Assert.That(data.Count, Is.EqualTo(7));
        Assert.That(data[0].EntryCount, Is.EqualTo(3));
        Assert.That(data[1].EntryCount, Is.EqualTo(1));
        Assert.That(data[2].EntryCount, Is.EqualTo(1));
        Assert.That(data[3].EntryCount, Is.Zero);
        Assert.That(data[4].EntryCount, Is.Zero);
        Assert.That(data[5].EntryCount, Is.Zero);
        Assert.That(data[6].EntryCount, Is.Zero);

        AssertCollections(data.Select(d => d.Collection).ToList());
    }

    [Test]
    public async Task CreateShouldAddCollection()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

        using var createContext = new DataContext(builder.Options);
        var repository = new CollectionRepository(createContext);

        var newCollection = new Collection
        {
            Active = true,
            ActiveApi = true,
            ClassificationId = 1,
            Code = "newCode",
            Description = "Beschreibung",
            Documentation = "Dokumentation",
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
        };

        await repository.Create(newCollection);

        await createContext.SaveChangesAsync();

        Assert.That(newCollection.Id, Is.Not.Default);
    }

    [Test]
    public async Task UpdateShouldUpdateCollection()
    {
        var databaseCollection = await collectionRepository.GetById(1);
        var translations = databaseCollection.Translations.ToList();
        var updatedCollection = new Collection
        {
            Id = databaseCollection.Id,
            Active = false,
            ActiveApi = false,
            ClassificationId = 1,
            ConceptSystem = databaseCollection.ConceptSystem,
            Code = "1234567890",
            Created = DateTime.UtcNow,
            CreatedBy = "ItseMe",
            Description = "Beschreibung",
            Documentation = "Dokumnatione",
            Modified = DateTime.UtcNow,
            ModifiedBy = "ItsemMe",
            Translations = new List<CollectionTranslation>
                {
                    translations[0],
                    translations[1],
                    translations[2]
                }
        };

        collectionRepository.Update(databaseCollection, updatedCollection);

        await context.SaveChangesAsync();

        var updatedDatabaseCollection = await collectionRepository.GetById(1);
        Assert.That(updatedDatabaseCollection.Active, Is.EqualTo(updatedCollection.Active));
        Assert.That(updatedDatabaseCollection.ActiveApi, Is.EqualTo(updatedCollection.ActiveApi));
        Assert.That(updatedDatabaseCollection.ClassificationId, Is.EqualTo(updatedCollection.ClassificationId));
        Assert.That(updatedDatabaseCollection.Classification, Is.EqualTo(updatedCollection.Classification));
        Assert.That(updatedDatabaseCollection.Code, Is.EqualTo(updatedCollection.Code));
        Assert.That(updatedDatabaseCollection.ConceptSystem, Is.EqualTo(updatedCollection.ConceptSystem));
        Assert.That(updatedDatabaseCollection.Created, Is.EqualTo(updatedCollection.Created));
        Assert.That(updatedDatabaseCollection.CreatedBy, Is.EqualTo(updatedCollection.CreatedBy));
        Assert.That(updatedDatabaseCollection.Description, Is.EqualTo(updatedCollection.Description));
        Assert.That(updatedDatabaseCollection.Documentation, Is.EqualTo(updatedCollection.Documentation));
        Assert.That(updatedDatabaseCollection.Translations, Is.EqualTo(updatedCollection.Translations));
    }

    [Test]
    public async Task UpdateShouldHandleAdditionalTranslations()
    {
        var databaseCollection = await collectionRepository.GetById(2);
        var translations = databaseCollection.Translations.ToList();
        var updatedCollection = new Collection
        {
            Id = databaseCollection.Id,
            Active = false,
            ActiveApi = false,
            ClassificationId = 1,
            ConceptSystem = databaseCollection.ConceptSystem,
            Code = "1234567890",
            Created = DateTime.UtcNow,
            CreatedBy = "ItseMe",
            Description = "Beschreibung",
            Documentation = "Dokumnatione",
            Modified = DateTime.UtcNow,
            ModifiedBy = "ItsemMe",
            Translations = new List<CollectionTranslation>
                {
                    translations[0],
                    translations[1],
                    translations[2],
                    new()
                    {
                        LanguageId = 4,
                        CollectionId = 1,
                        Name = "NewName"
                    }
                }
        };

        collectionRepository.Update(databaseCollection, updatedCollection);

        await context.SaveChangesAsync();

        var updatedDatabaseCollection = await collectionRepository.GetById(2);
        Assert.That(updatedDatabaseCollection.Translations, Is.EqualTo(updatedCollection.Translations));
    }

    [Test]
    public async Task GetAllReturns()
    {
        var data = (await collectionRepository.GetAll()).ToList();

        AssertCollections(data);
    }

    [Test]
    public async Task GetAllActiveReturns()
    {
        var data = (await collectionRepository.GetAllActive()).ToList();

        AssertCollections(data);
    }

    [Test]
    public async Task GetAccessibleActiveAndInactive_WhenCalled_ReturnsInactiveCollectionWithPermission()
    {
        var data = (await collectionRepository.GetAccessibleActiveAndInactive([2])).ToList();

        AssertCollections(data);

        Assert.That(data, Has.Count.EqualTo(3));

        Assert.That(data[1].Id, Is.Not.Default);
        Assert.That(data[1].Code, Is.EqualTo("ABC11"));

        Assert.That(data[1].Translations.Count, Is.EqualTo(3));
        Assert.That(data[1].Translations.All(t => t.Language != null));
        var translation = data[1].Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.CollectionId, Is.EqualTo(2));
        Assert.That(translation.Name, Is.EqualTo("Übersetzungsdatei der schweizerischen Bundesverwaltung (1991-2004, frühere Datenbank ALEXIS)"));

        translation = data[1].Translations.Last();
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.CollectionId, Is.EqualTo(2));
        Assert.That(translation.Name, Is.EqualTo("File di traduzione dell'Amministrazione federale (1991-2004, vecchia banca dati ALEXIS)"));
    }

    [Test]
    public async Task GetAllActiveApiReturns()
    {
        var data = (await collectionRepository.GetAllActiveApi()).ToList();

        Assert.That(data, Has.Count.EqualTo(1));
        AssertFirst(data.Single());
    }

    [Test]
    public async Task GetByIdsReturns()
    {
        var data = (await collectionRepository.GetByIds(new[] { 1, 36 })).ToList();

        AssertCollections(data);
    }

    [Test]
    public async Task GetByIdReturns()
    {
        var data = await collectionRepository.GetById(1);
        AssertFirst(data);
    }

    [Test]
    public async Task GetByIdForDeleteReturns()
    {
        var data = await collectionRepository.GetByIdForDelete(1);
        Assert.That(data, Is.Not.Null);
        Assert.That(data.Collection.Id, Is.EqualTo(1));
        Assert.That(data.Collection.Code, Is.EqualTo("ABC11"));
        Assert.That(data.EntryCount, Is.EqualTo(3));
    }

    [Test]
    public void DeleteRemovesCollection()
    {
        var colId = 12345;
        var collection = new Collection
        {
            Id = colId,
            Code = "code",
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
        };

        context.Add(collection);

        context.SaveChanges();

        Assert.That(context.Collections.SingleOrDefault(c => c.Id == colId), Is.Not.Null);

        collectionRepository.Delete(collection);

        context.SaveChanges();

        Assert.That(context.Collections.SingleOrDefault(c => c.Id == colId), Is.Null);
    }

    [Test]
    public async Task GetAuthorizedByIds_WhenCalled_ReturnsOnlyAuthorizedCollections()
    {
        var data = (await collectionRepository.GetAuthorizedByIds(new[] { 1, 3 })).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(data, Is.Not.Empty);
            Assert.That(data, Has.Count.EqualTo(1));

            Assert.That(data.First().Id, Is.EqualTo(1));
            Assert.That(data.First().Code, Is.EqualTo("ABC11"));
        });
    }

    [Test]
    public async Task GetIdsForLindasExport_WhenCalled_ShouldReturnActiveNotDeletedExportableIds()
    {
        var data = (await collectionRepository.GetIdsForLindasExport()).ToList();

        Assert.That(data, Is.Not.Empty);
        Assert.That(data, Has.Count.EqualTo(1));
        Assert.That(data.First(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetByIdForLindasExport_ForId_ShouldReturnCollection()
    {
        var data = await collectionRepository.GetByIdForLindasExport(1);

        Assert.That(data, Is.Not.Null);
    }

    private static void AssertCollections(List<Collection> data)
    {
        Assert.That(data, Is.Not.Null);

        var collection = data.First();

        AssertFirst(collection);

        collection = data.Last();

        Assert.That(data, Is.Not.Null);
        Assert.That(collection.Id, Is.Not.Default);

        Assert.That(collection.Code, Is.EqualTo("ABC15"));

        Assert.That(collection.Translations.Count, Is.EqualTo(3));
        Assert.That(collection.Translations.All(t => t.Language != null));
        var translation = collection.Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.CollectionId, Is.EqualTo(36));
        Assert.That(translation.Name,
            Is.EqualTo("Übersetzungsdatei der schweizerischen Bundesverwaltung (1991-2004, frühere Datenbank ALEXIS)"));

        translation = collection.Translations.Last();
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.CollectionId, Is.EqualTo(36));
        Assert.That(translation.Name,
            Is.EqualTo("File di traduzione dell'Amministrazione federale (1991-2004, vecchia banca dati ALEXIS)"));
    }

    private static void AssertFirst(Collection collection)
    {
        Assert.That(collection.Id, Is.Not.Default);

        Assert.That(collection.Code, Is.EqualTo("ABC11"));

        Assert.That(collection.Translations.Count, Is.EqualTo(3));
        Assert.That(collection.Translations.All(t => t.Language != null));
        var translation = collection.Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.CollectionId, Is.EqualTo(1));
        Assert.That(translation.Name, Is.EqualTo("ABC-Terminologie des VBS"));

        translation = collection.Translations.Last();
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.CollectionId, Is.EqualTo(1));
        Assert.That(translation.Name, Is.EqualTo("Terminologia NBC del DDPS"));
    }
}
