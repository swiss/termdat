using System;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class EntryTemplateRepositoryTests
{
    private EntryTemplateRepository entryTemplateRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        entryTemplateRepository = new EntryTemplateRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [Test]
    public async Task GetForUser_WhenCalled_ReturnsAllUserEntryTemplates()
    {
        var data = (await entryTemplateRepository.GetForUser(1)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(data, Is.Not.Empty);
            Assert.That(data, Has.Count.EqualTo(2));

            Assert.That(data.First().Id, Is.EqualTo(2));
            Assert.That(data.First().Name, Is.EqualTo("TemplateA"));

            Assert.That(data.Last().Id, Is.EqualTo(1));
            Assert.That(data.Last().Name, Is.EqualTo("TemplateZ"));

            Assert.That(data, Is.Ordered.Ascending.By(nameof(EntryTemplate.Name)));
        });
    }

    [Test]
    public async Task GetById_WhenCalled_ReturnsEntryTemplate()
    {
        var data = await entryTemplateRepository.GetById(1);

        Assert.Multiple(() =>
        {
            Assert.That(data.Id, Is.EqualTo(1));
            Assert.That(data.Name, Is.EqualTo("TemplateZ"));
            Assert.That(data.UserAccountId, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Create_WhenCalled_AddsEntryTemplate()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

        using var createContext = new DataContext(builder.Options);
        var repository = new EntryTemplateRepository(createContext);

        var newEntryTemplate = new EntryTemplate
        {
            Name = "Template1",
            Template = "",
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Modified = DateTime.UtcNow,
            ModifiedBy = "me@me.com"
        };

        await repository.Create(newEntryTemplate);

        Assert.That(newEntryTemplate.Id, Is.Not.EqualTo(default(int)));
    }

    [Test]
    public async Task Update_WhenCalled_ShouldUpdateEntryTemplate()
    {
        var databaseEntryTranslation = await entryTemplateRepository.GetById(1);

        var updatedEntryTemplate = new EntryTemplate
        {
            Id = databaseEntryTranslation.Id,
            Name = "MyEntryTemplate",
            Template = "{\"Identification\": 333,\"Office\": { \"Name\": \"OfficeNameTest2\" },\"Domains\": [ { \"Name\": \"DomainNameTest2\" } ],\"Comment\": \"CommentTest2\"}",
            UserAccountId = 22,
            ConcurrencyToken = databaseEntryTranslation.ConcurrencyToken,
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Modified = DateTime.UtcNow,
            ModifiedBy = "me@me.kong",
        };

        context.ChangeTracker.Clear();
        entryTemplateRepository.Update(updatedEntryTemplate);

        await context.SaveChangesAsync();

        var result = await entryTemplateRepository.GetById(1);
        Assert.That(result.Id, Is.EqualTo(updatedEntryTemplate.Id));
        Assert.That(result.Name, Is.EqualTo(updatedEntryTemplate.Name));
        Assert.That(result.Template, Is.EqualTo(updatedEntryTemplate.Template));
        Assert.That(result.Created, Is.EqualTo(updatedEntryTemplate.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(updatedEntryTemplate.CreatedBy));
        Assert.That(result.Modified, Is.EqualTo(updatedEntryTemplate.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(updatedEntryTemplate.ModifiedBy));
    }

    [Test]
    public async Task Delete_WhenCalled_RemovesEntryTemplate()
    {
        var entryTemplateCount = await context.EntryTemplates.CountAsync();

        var newEntryTemplate = new EntryTemplate
        {
            Name = "Template333",
            UserAccountId = 22,
            Template = "",
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Modified = DateTime.UtcNow,
            ModifiedBy = "me@me.com"
        };

        await entryTemplateRepository.Create(newEntryTemplate);

        var entryTemplate = await context.EntryTemplates.LastAsync();

        entryTemplateRepository.Delete(entryTemplate);
        await context.SaveChangesAsync();

        Assert.That(await context.EntryTemplates.CountAsync(), Is.EqualTo(entryTemplateCount));
        Assert.That(context.EntryTemplates, Is.Not.Contains(entryTemplate));
    }
}
