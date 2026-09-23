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
internal class ConceptSystemRepositoryTests
{
    private ConceptSystemRepository conceptSystemRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        conceptSystemRepository = new ConceptSystemRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [TestCase]
    public async Task GetAllReturns()
    {
        var data = (await conceptSystemRepository.GetAll()).ToList();
        var first = data.First();
        var last = data.Last();

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Count, Is.EqualTo(3));

        Assert.That(first, Is.Not.Null);
        Assert.That(first.Id, Is.EqualTo(1));
        Assert.That(first.Title, Is.EqualTo("Test 1 Title"));
        Assert.That(first.Collection, Is.Null);
        Assert.That(first.CreatedBy, Is.EqualTo("test1@test.ch"));
        Assert.That(first.ModifiedBy, Is.EqualTo("test2@test.ch"));

        Assert.That(last, Is.Not.Null);
        Assert.That(last.Id, Is.EqualTo(3));
        Assert.That(last.Title, Is.EqualTo("Test 3 Title"));
        Assert.That(last.Collection, Is.Null);
        Assert.That(last.CreatedBy, Is.EqualTo("test5@test.ch"));
        Assert.That(last.ModifiedBy, Is.EqualTo("test6@test.ch"));
    }

    [TestCase]
    public async Task GetAllDetailsReturns()
    {
        var data = (await conceptSystemRepository.GetAllDetails()).ToList();
        var first = data.First();
        var last = data.Last();

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Count, Is.EqualTo(3));

        Assert.That(first, Is.Not.Null);
        Assert.That(first.ConceptSystem.Id, Is.EqualTo(1));
        Assert.That(first.ConceptSystem.Title, Is.EqualTo("Test 1 Title"));
        Assert.That(first.ConceptSystem.CreatedBy, Is.EqualTo("test1@test.ch"));
        Assert.That(first.ConceptSystem.ModifiedBy, Is.EqualTo("test2@test.ch"));
        Assert.That(first.ConceptSystem.Collection, Is.Not.Null);
        Assert.That(first.ConceptSystem.Collection.Id, Is.EqualTo(1));
        Assert.That(first.EntryCount, Is.EqualTo(3));

        Assert.That(last, Is.Not.Null);
        Assert.That(last.ConceptSystem.Id, Is.EqualTo(3));
        Assert.That(last.ConceptSystem.Title, Is.EqualTo("Test 3 Title"));
        Assert.That(last.ConceptSystem.CreatedBy, Is.EqualTo("test5@test.ch"));
        Assert.That(last.ConceptSystem.ModifiedBy, Is.EqualTo("test6@test.ch"));
        Assert.That(last.ConceptSystem.Collection, Is.Not.Null);
        Assert.That(last.ConceptSystem.Collection.Id, Is.EqualTo(3));
        Assert.That(first.EntryCount, Is.EqualTo(3));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task GetByIdReturns(int id)
    {
        var data = await conceptSystemRepository.GetById(id);

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Id, Is.EqualTo(id));
    }

    [Test]
    public async Task CreateShouldAddConceptSystem()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

        var createContext = new DataContext(builder.Options);
        var repository = new ConceptSystemRepository(createContext);

        var newConceptSystem = new ConceptSystem
        {
            Title = "new cs",
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
        };

        await repository.Create(newConceptSystem);

        await createContext.SaveChangesAsync();

        Assert.That(newConceptSystem.Id, Is.Not.Default);
    }

    [Test]
    public async Task UpdateShouldUpdateConceptSystem()
    {
        var databaseConceptSystem = await conceptSystemRepository.GetById(1);
        var conceptSystemPositions = databaseConceptSystem.ConceptSystemPositions.ToList();
        var updatedConceptSystem = new ConceptSystem
        {
            Id = databaseConceptSystem.Id,
            Title = "1234567890",
            Created = DateTime.UtcNow,
            CreatedBy = "ItseMe",
            Modified = DateTime.UtcNow,
            ModifiedBy = "ItsemMe",
            ConceptSystemPositions = new List<ConceptSystemPosition>
            {
                conceptSystemPositions[0],
                conceptSystemPositions[1],
            }
        };

        conceptSystemRepository.Update(databaseConceptSystem, updatedConceptSystem);

        await context.SaveChangesAsync();

        var updatedDatabaseConceptSystem = await conceptSystemRepository.GetById(1);
        Assert.That(updatedDatabaseConceptSystem.Title, Is.EqualTo(updatedConceptSystem.Title));
        Assert.That(updatedDatabaseConceptSystem.Created, Is.EqualTo(updatedConceptSystem.Created));
        Assert.That(updatedDatabaseConceptSystem.CreatedBy, Is.EqualTo(updatedConceptSystem.CreatedBy));
        Assert.That(updatedDatabaseConceptSystem.ConceptSystemPositions, Is.EqualTo(updatedConceptSystem.ConceptSystemPositions));
    }

    [Test]
    public async Task UpdateShouldHandleAdditionalTranslations()
    {
        var databaseConceptSystem = await conceptSystemRepository.GetById(2);
        var conceptSystemPositions = databaseConceptSystem.ConceptSystemPositions.ToList();
        var updatedConceptSystem = new ConceptSystem
        {
            Id = databaseConceptSystem.Id,
            Title = "1234567890",
            Created = DateTime.UtcNow,
            CreatedBy = "ItseMe",
            Modified = DateTime.UtcNow,
            ModifiedBy = "ItsemMe",
            ConceptSystemPositions = new List<ConceptSystemPosition>
            {
                conceptSystemPositions[0],
                conceptSystemPositions[1],
                conceptSystemPositions[2],
                new()
                {
                    Value = "NewName"
                }
            }
        };

        conceptSystemRepository.Update(databaseConceptSystem, updatedConceptSystem);

        await context.SaveChangesAsync();

        var updatedDatabaseGlossary = await conceptSystemRepository.GetById(2);
        Assert.That(updatedDatabaseGlossary.ConceptSystemPositions, Is.EqualTo(updatedConceptSystem.ConceptSystemPositions));
    }

    [Test]
    public void DeleteRemovesConceptSystem()
    {
        var conceptSystemId = 12345;
        var conceptSystem = new ConceptSystem
        {
            Id = conceptSystemId,
            Title = "cs 1",
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
        };

        context.Add(conceptSystem);

        context.SaveChanges();

        Assert.That(context.ConceptSystems.SingleOrDefault(c => c.Id == conceptSystemId), Is.Not.Null);

        conceptSystemRepository.Delete(conceptSystem);

        context.SaveChanges();

        Assert.That(context.ConceptSystems.SingleOrDefault(c => c.Id == conceptSystemId), Is.Null);
    }

}
