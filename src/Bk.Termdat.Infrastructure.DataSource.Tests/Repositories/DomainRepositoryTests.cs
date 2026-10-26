using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class DomainRepositoryTests
{
    private DomainRepository domainRepository;
    private DataContext context;

    [SetUp]
    public void SetUp()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        domainRepository = new DomainRepository(context);
    }

    [TearDown]
    public void TearDown()
    {
        context.Dispose();
    }

    [Test]
    public async Task GetAllShouldReturnDomains()
    {
        var data = (await domainRepository.GetAll()).ToList();

        Assert.That(data.Count, Is.EqualTo(4));
    }

    [Test]
    public async Task GetAllDetailsShouldReturnDomains()
    {
        var data = (await domainRepository.GetAllDetails()).ToList();

        Assert.That(data.Count, Is.EqualTo(4));
        Assert.That(data[0].EntryCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ShouldGetByIds()
    {
        var result = (await domainRepository.GetByIds(new List<int> { 1, 3 })).ToList();

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(d => d.Id == 1));
        Assert.That(result.Any(d => d.Id == 3));
        Assert.That(result.All(d => d.Translations.Any()));
    }

    [Test]
    public async Task ShouldGetById()
    {
        var result = await domainRepository.GetById(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Translations.Any());
    }

    [Test]
    public async Task CreateShouldAddDomain()
    {
        var newDomain = new Domain
        {
            SubjectId = context.Subjects.First().Id,
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Modified = DateTime.UtcNow,
            ModifiedBy = "update@me.com",
            Translations = new List<DomainTranslation>
            {
                new() { Id = 100, LanguageId = 1, Name = "DOMAIN 1" },
                new() { Id = 200, LanguageId = 2, Name = "DOMAIN 1 ALT" },
            }
        };

        await domainRepository.Create(newDomain);

        await context.SaveChangesAsync();

        Assert.That(newDomain.Id, Is.Not.Default);
    }

    [Test]
    public async Task UpdateShouldChangeDomain()
    {
        var now = DateTime.UtcNow;
        var newDomain = new Domain
        {
            Id = 1,
            SubjectId = context.Subjects.First().Id,
            Created = now,
            CreatedBy = "me@me.com",
            Modified = now,
            ModifiedBy = "update@me.com",
            Translations = new List<DomainTranslation>
            {
                new() { LanguageId = 1, Name = "DOMAIN 1 update" },
                new() { LanguageId = 2, Name = "DOMAIN 1 ALT" },
            }
        };
        var oldDomain = context.Domains.Single(d => d.Id == 1);

        domainRepository.Update(oldDomain, newDomain);

        await context.SaveChangesAsync();

        var changed = context.Domains.Single(d => d.Id == 1);
        Assert.That(changed.Id, Is.EqualTo(1));
        Assert.That(changed.CreatedBy, Is.EqualTo("me@me.com"));
        Assert.That(changed.Created, Is.EqualTo(now));
        Assert.That(changed.ModifiedBy, Is.EqualTo("update@me.com"));
        Assert.That(changed.Modified, Is.EqualTo(now));
        Assert.That(changed.Translations.Count, Is.EqualTo(2));
        Assert.That(changed.Translations.First().Name, Is.EqualTo("DOMAIN 1 update"));
    }

    [Test]
    public void ShouldRemoveDomain()
    {
        var domain = context.Domains.SingleOrDefault(d => d.Id == 1);
        Assert.That(context.Domains.Count, Is.EqualTo(4));
        Assert.That(domain, Is.Not.Null);
        Assert.That(context.Domains.Contains(domain), Is.True);

        domainRepository.Delete(domain);
        context.SaveChanges();

        Assert.That(context.Domains.Count, Is.EqualTo(3));
        Assert.That(context.Domains.Contains(domain), Is.False);
    }
}
