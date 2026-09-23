using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class SubjectRepositoryTests
{
    private SubjectRepository subjectRepository;
    private DataContext context;

    [SetUp]
    public void Setup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        subjectRepository = new SubjectRepository(context);
    }

    [TearDown]
    public void TearDown()
    {
        context.Dispose();
    }

    [Test]
    public async Task ShouldGetAll()
    {
        var result = (await subjectRepository.GetAll()).ToList();

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[1].Id, Is.EqualTo(2));
        Assert.That(result[2].Id, Is.EqualTo(3));

        Assert.That(result[0].Translations, Is.Not.Empty);
        Assert.That(result[0].Translations.Count, Is.EqualTo(2));
        Assert.That(result[0].Domains, Is.Not.Empty);
        Assert.That(result[0].Domains.Count, Is.EqualTo(2));
        Assert.That(result[0].Domains.All(d => d.Translations.Any()));

        Assert.That(result[1].Translations, Is.Not.Empty);
        Assert.That(result[1].Translations.Count, Is.EqualTo(2));
        Assert.That(result[1].Domains, Is.Not.Empty);
        Assert.That(result[1].Domains.Count, Is.EqualTo(2));
        Assert.That(result[1].Domains.All(d => d.Translations.Any()));

        Assert.That(result[2].Translations, Is.Not.Empty);
        Assert.That(result[2].Translations.Count, Is.EqualTo(2));
        Assert.That(result[2].Domains, Is.Empty);
    }

    [Test]
    public async Task ShouldGetAllDetails()
    {
        var result = (await subjectRepository.GetAllDetails()).ToList();

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0].EntryCount, Is.EqualTo(1));
        Assert.That(result[1].EntryCount, Is.EqualTo(3));
        Assert.That(result[2].EntryCount, Is.EqualTo(1));

        Assert.That(result[0].Subject.Id, Is.EqualTo(1));
        Assert.That(result[1].Subject.Id, Is.EqualTo(2));
        Assert.That(result[2].Subject.Id, Is.EqualTo(3));
        Assert.That(result[0].Subject.Translations, Is.Not.Empty);
        Assert.That(result[0].Subject.Translations.Count, Is.EqualTo(2));

        Assert.That(result[1].Subject.Translations, Is.Not.Empty);
        Assert.That(result[1].Subject.Translations.Count, Is.EqualTo(2));

        Assert.That(result[2].Subject.Translations, Is.Not.Empty);
        Assert.That(result[2].Subject.Translations.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task ShouldGetById()
    {
        var result = (await subjectRepository.GetByIds(new List<int> { 1, 3 })).ToList();

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(d => d.Id == 1));
        Assert.That(result.Any(d => d.Id == 3));
        Assert.That(result.All(d => d.Translations.Any()));
    }

    [Test]
    public void ShouldRemoveSubject()
    {
        var subject = context.Subjects.SingleOrDefault(s => s.Id == 1);
        Assert.That(context.Subjects.Count, Is.EqualTo(3));
        Assert.That(subject, Is.Not.Null);
        Assert.That(context.Subjects.Contains(subject), Is.True);

        subjectRepository.Delete(subject);
        context.SaveChanges();

        Assert.That(context.Subjects.Count, Is.EqualTo(2));
        Assert.That(context.Subjects.Contains(subject), Is.False);
    }

    [Test]
    public async Task ShouldCreate()
    {
        Subject newSubject = new()
        {
            Translations = Array.Empty<SubjectTranslation>(),
            Created = DateTime.Now,
            CreatedBy = "creator@example.com",
            Modified = DateTime.Now,
            ModifiedBy = "modifier@example.com",
            Domains = Array.Empty<Domain>(),
            Entries = Array.Empty<Entry>(),
        };

        Assert.That(context.Subjects, Does.Not.Contain(newSubject));

        await subjectRepository.Create(newSubject);
        await context.SaveChangesAsync();

        Assert.That(context.Subjects, Does.Contain(newSubject));
        Assert.That(newSubject.Id, Is.Not.Default);
    }

    [Test]
    public async Task ShouldUpdate()
    {
        var oldSubject = await subjectRepository.GetById(1);
        var newSubject = new Subject
        {
            Id = oldSubject.Id,
            Translations = new List<SubjectTranslation>
            {
                new() { Id = 1, LanguageId = 1, SubjectId = 1, Name = "SUBJECT 1 CHANGED" },
                new() { Id = 2, LanguageId = 2, SubjectId = 1, Name = "SUBJECT 1 ALT CHANGED" },
            },
            Modified = DateTime.Now,
            ModifiedBy = "modifier@example.com",
        };

        subjectRepository.Update(oldSubject, newSubject);
        await context.SaveChangesAsync();

        var reloadedSubject = await subjectRepository.GetById(1);
        Assert.That(reloadedSubject.Modified, Is.EqualTo(newSubject.Modified));
        Assert.That(reloadedSubject.ModifiedBy, Is.EqualTo(newSubject.ModifiedBy));
        Assert.That(reloadedSubject.CreatedBy, Is.EqualTo(oldSubject.CreatedBy));
        Assert.That(reloadedSubject.Created, Is.EqualTo(oldSubject.Created));
        Assert.That(reloadedSubject.Translations.All(t => newSubject.Translations.Any(nt => nt.Id == t.Id && nt.Name == t.Name)), Is.True);
    }

    [Test]
    public async Task ShouldRemove()
    {
        var subject = await subjectRepository.GetById(1);
        var subjectCount = context.Subjects.Count();
        Assert.That(context.Subjects, Does.Contain(subject));

        subjectRepository.Delete(subject);
        await context.SaveChangesAsync();

        Assert.That(context.Subjects, Does.Not.Contain(subject));
        Assert.That(context.Subjects.Count(), Is.EqualTo(subjectCount - 1));
    }

    [Test]
    [TestCase(1, 1)]
    [TestCase(2, 3)]
    [TestCase(3, 1)]
    public async Task ShouldCountEntriesOfASubject(int subjectId, int expectedEntryCount)
    {
        var c = await subjectRepository.GetEntryCountById(subjectId);

        Assert.That(c, Is.EqualTo(expectedEntryCount));
    }
}
