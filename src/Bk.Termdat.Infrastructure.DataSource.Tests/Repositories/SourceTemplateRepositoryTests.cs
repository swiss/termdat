using System;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class SourceTemplateRepositoryTests
{
    private SourceTemplateRepository sourceTemplateRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        sourceTemplateRepository = new SourceTemplateRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [Test]
    public async Task GetAll_WhenCalled_ReturnsAllUserSourceTemplates()
    {
        var data = (await sourceTemplateRepository.GetAll(1)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(data, Is.Not.Empty);
            Assert.That(data, Has.Count.EqualTo(2));

            Assert.That(data.First().Id, Is.EqualTo(1));
            Assert.That(data.First().TextDe, Is.EqualTo("Text De1"));

            Assert.That(data.Last().Id, Is.EqualTo(2));
            Assert.That(data.Last().TextDe, Is.EqualTo("Text De2"));
        });
    }

    [Test]
    public async Task Create_WhenCalled_ShouldAddSourceTemplate()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

        using var createContext = new DataContext(builder.Options);
        var repository = new SourceTemplateRepository(createContext);

        var newSourceTemplate = new SourceTemplate()
        {
            TextDe = "Text De4",
            TextFr = "Text Fr4",
            TextIt = "Text It4",
            TextEn = "Text En4",
            TextRm = "Text Rm4",
            UserId = 1,
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "test@test.ch",
            User = new UserAccount()
        };

        await repository.Create(newSourceTemplate);

        await context.SaveChangesAsync();

        Assert.That(newSourceTemplate.Id, Is.Not.EqualTo(default(int)));
    }

    [Test]
    public void Update_WhenCalled_ShouldUpdateSourceTemplate()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

        using var createContext = new DataContext(builder.Options);
        var repository = new SourceTemplateRepository(createContext);

        var oldSourceTemplate = new SourceTemplate()
        {
            TextDe = "Text De4",
            TextFr = "Text Fr4",
            TextIt = "Text It4",
            TextEn = "Text En4",
            TextRm = "Text Rm4",
            UserId = 1,
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "test@test.ch",
            User = new UserAccount()
        };

        var updateSourceTemplate = new SourceTemplate()
        {
            TextDe = "Text De5",
            TextFr = "Text Fr5",
            TextIt = "Text It5",
            TextEn = "Text En5",
            TextRm = "Text Rm5",
            UserId = 1,
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "test@test.ch",
            User = new UserAccount()
        };

        repository.Update(oldSourceTemplate, updateSourceTemplate);

        context.SaveChangesAsync();

        Assert.Multiple(() =>
        {
            Assert.That(oldSourceTemplate.TextDe, Is.EqualTo("Text De5"));
            Assert.That(oldSourceTemplate.TextFr, Is.EqualTo("Text Fr5"));
            Assert.That(oldSourceTemplate.TextIt, Is.EqualTo("Text It5"));
            Assert.That(oldSourceTemplate.TextEn, Is.EqualTo("Text En5"));
            Assert.That(oldSourceTemplate.TextRm, Is.EqualTo("Text Rm5"));
        });
    }

    [Test]
    public async Task Delete_WhenCalled_ShouldRemoveSourceTemplate()
    {
        await context.SourceTemplates.AddAsync(
            new SourceTemplate()
            {
                Id = 6,
                TextDe = "Text De6",
                TextFr = "Text Fr6",
                TextIt = "Text It6",
                TextEn = "Text En6",
                TextRm = "Text Rm6",
                UserId = 1,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch"
            });

        await context.SaveChangesAsync();

        var sourceTemplate = await context.SourceTemplates.SingleOrDefaultAsync(e => e.Id == 6);
        Assert.That(sourceTemplate, Is.Not.Null);

        sourceTemplateRepository.Delete(sourceTemplate);

        sourceTemplate = await context.SourceTemplates.SingleOrDefaultAsync(e => e.Id == 6);
        Assert.That(sourceTemplate, Is.Not.Null);

        await context.SaveChangesAsync();

        sourceTemplate = await context.SourceTemplates.SingleOrDefaultAsync(e => e.Id == 6);
        Assert.That(sourceTemplate, Is.Null);
    }
}
