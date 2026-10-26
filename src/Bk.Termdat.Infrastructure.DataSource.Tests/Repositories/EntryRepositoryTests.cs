using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class EntryRepositoryTests
{
    private EntryRepository entryRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
    }

    [SetUp]
    public void SetUp()
    {
        entryRepository = new EntryRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [Test]
    public async Task CountReturns()
    {
        var count = await entryRepository.Count();

        Assert.That(count, Is.EqualTo(5));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task GetByIdForEntryDetailReturnsSearchDetail(int entryId)
    {
        var data = await entryRepository.GetByIdForEntryDetail(entryId);

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Id, Is.EqualTo(entryId));

        Assert.That(data.Collection, Is.Not.Null);
        Assert.That(data.CollectionId, Is.EqualTo(data.Collection.Id));
        Assert.That(data.Collection.Translations.Count, Is.EqualTo(3));
        Assert.That(data.Collection.Translations.All(t => t.Language != null));

        Assert.That(data.Office, Is.Not.Null);
        Assert.That(data.OfficeId, Is.EqualTo(data.Office.Id));
        Assert.That(data.Office.Translations.Count, Is.EqualTo(3));
        Assert.That(data.Office.Translations.All(t => t.Language != null));
    }

    [TestCase(1, 3, 0)]
    [TestCase(3, 0, 2)]
    public async Task ShouldIncludeReferencesForGetByIdForEntryDetail(int entryId, int referenceCount, int referencedByCount)
    {
        var entry = await entryRepository.GetByIdForEntryDetail(entryId);

        Assert.That(entry, Is.Not.Null);
        Assert.That(entry.References, Is.Not.Null);
        Assert.That(entry.References.Count, Is.EqualTo(referenceCount));
        Assert.That(entry.References.All(er => er.EntryId == entryId));
        Assert.That(entry.References.All(er => er.ReferencedEntry?.Translations != null));
        Assert.That(entry.References.All(er => er.ReferencedEntry.Translations.All(t => t.Language != null)));

        Assert.That(entry.ReferencedByEntries, Is.Not.Null);
        Assert.That(entry.ReferencedByEntries.Count, Is.EqualTo(referencedByCount));
        Assert.That(entry.ReferencedByEntries.All(er => er.ReferencedEntryId == entryId));
        Assert.That(entry.ReferencedByEntries.All(er => er.Entry?.Translations != null));
        Assert.That(entry.ReferencedByEntries.All(er => er.Entry.Translations.All(t => t.Language != null)));
    }

    [TestCase(1, 3)]
    [TestCase(3, 0)]
    public async Task ShouldIncludeReferencesForGetByIdForUpdate(int entryId, int referenceCount)
    {
        var entry = await entryRepository.GetByIdForUpdate(entryId);

        Assert.That(entry, Is.Not.Null);
        Assert.That(entry.References, Is.Not.Null);
        Assert.That(entry.References.Count, Is.EqualTo(referenceCount));
    }

    [Test]
    public async Task ShouldGetByIdForReferenceDetail()
    {
        var entry = await entryRepository.GetByIdForReferenceDetail(1);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry.Translations, Is.Not.Null);
        Assert.That(entry.Translations.Count, Is.EqualTo(3));
        Assert.That(entry.Translations.All(t => t.Language != null));
    }

    [Test]
    public async Task Update_WithEmptyExternalLinks_DeletesCorrectly()
    {
        var dbEntry = await entryRepository.GetByIdForUpdate(1);
        var updatedEntry = await entryRepository.GetByIdForEntryDetail(1);

        updatedEntry.ExternalLinks = [];

        entryRepository.Update(dbEntry, updatedEntry);
        await context.SaveChangesAsync();

        var afterUpdate = await entryRepository.GetByIdForEntryDetail(1);
        Assert.That(afterUpdate.ExternalLinks, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task Update_WithChangedExternalLink_UpdatesCorrectly()
    {
        var dbEntry = await entryRepository.GetByIdForUpdate(1);
        var updatedEntry = await entryRepository.GetByIdForEntryDetail(1);

        updatedEntry.ExternalLinks.Single().Url = "https://new.example.com";
        updatedEntry.ExternalLinks.Single().Type = ExternalLinkType.Wikidata;

        entryRepository.Update(dbEntry, updatedEntry);
        await context.SaveChangesAsync();

        var afterUpdate = await entryRepository.GetByIdForEntryDetail(1);
        Assert.That(afterUpdate.ExternalLinks, Has.Count.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(afterUpdate.ExternalLinks.Single().Url, Is.EqualTo("https://new.example.com"));
            Assert.That(afterUpdate.ExternalLinks.Single().Type, Is.EqualTo(ExternalLinkType.Wikidata));
        });
    }

    [Test]
    public async Task Update_WithAdditionalExternalLink_AddsCorrectly()
    {
        var dbEntry = await entryRepository.GetByIdForUpdate(1);
        var updatedEntry = await entryRepository.GetByIdForEntryDetail(1);

        updatedEntry.ExternalLinks.Add(
            new EntryExternalLink { Url = "https://added.example.com", EntryId = 1, Type = ExternalLinkType.Fedlex });

        entryRepository.Update(dbEntry, updatedEntry);
        await context.SaveChangesAsync();

        var afterUpdate = await entryRepository.GetByIdForEntryDetail(1);
        Assert.That(afterUpdate.ExternalLinks, Has.Count.EqualTo(2));

        Assert.That(afterUpdate.ExternalLinks.Last().Url, Is.EqualTo("https://added.example.com"));
        Assert.That(afterUpdate.ExternalLinks.Last().Type, Is.EqualTo(ExternalLinkType.Fedlex));

        context.EntryExternalLinks.Remove(await context.EntryExternalLinks.LastAsync());
        await context.SaveChangesAsync();
    }

    [TestCase(new[] { 1, 2 })]
    [TestCase(new[] { 2, 3, 4 })]
    [TestCase(new[] { 5, 8 })]
    public async Task ShouldGetByIds(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        var entries = (await entryRepository.GetByIds(idList)).ToList();
        Assert.That(entries, Is.Not.Empty);
        Assert.That(entries.All(e => idList.Contains(e.Id)));
    }

    [TestCase(new[] { 1, 2 })]
    [TestCase(new[] { 2, 3, 4 })]
    [TestCase(new[] { 5, 8 })]
    public async Task ShouldGetByIdsForApi(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        var entries = (await entryRepository.GetByIdsForApiDetail(idList)).ToList();
        Assert.That(entries, Is.Not.Empty);
        Assert.That(entries.All(e => idList.Contains(e.Id)));
    }

    [Test]
    public async Task GetByIdsForUpdate_WithValidIds_ReturnsManyToManyCollections()
    {
        var ids = new List<int> { 2, 3 };
        var result = (await entryRepository.GetByIdsForUpdate(ids)).ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0].LenochClassifications.Count, Is.EqualTo(1));
        Assert.That(result[0].LenochClassifications.All(d => d.Translations.Count == 3));
        Assert.That(result[0].LenochClassifications.All(d => d.Translations.All(t => t.Language != null)));

        Assert.That(result[0].RelatedCollections.Count, Is.EqualTo(0));
        Assert.That(result[0].RelatedCollections.All(rc => rc.Translations.Count == 3));
        Assert.That(result[0].RelatedCollections.All(rc => rc.Translations.All(t => t.Language != null)));

        Assert.That(result[0].Glossaries.Count, Is.EqualTo(1));
        Assert.That(result[0].Glossaries.All(rc => rc.Translations.Count == 3));
        Assert.That(result[0].Glossaries.All(rc => rc.Translations.All(t => t.Language != null)));

        Assert.That(result[1].LenochClassifications.Count, Is.EqualTo(3));
        Assert.That(result[1].LenochClassifications.All(d => d.Translations.Count == 3));
        Assert.That(result[1].LenochClassifications.All(d => d.Translations.All(t => t.Language != null)));

        Assert.That(result[1].RelatedCollections.Count, Is.EqualTo(2));
        Assert.That(result[1].RelatedCollections.All(rc => rc.Translations.Count == 3));
        Assert.That(result[1].RelatedCollections.All(rc => rc.Translations.All(t => t.Language != null)));

        Assert.That(result[1].Glossaries.Count, Is.EqualTo(2));
        Assert.That(result[1].Glossaries.All(rc => rc.Translations.Count == 3));
        Assert.That(result[1].Glossaries.All(rc => rc.Translations.All(t => t.Language != null)));
    }

    [Test]
    public async Task GetByIdsForUpdate_WithValidIds_ReturnsEntry()
    {
        var ids = new List<int> { 2, 3 };
        var result = (await entryRepository.GetByIdsForUpdate(ids)).ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0].Id, Is.EqualTo(ids[0]));
        Assert.That(result[0].Collection, Is.Not.Null);
        Assert.That(result[0].CollectionId, Is.EqualTo(result[0].Collection.Id));
        Assert.That(result[0].Collection.Translations.Count, Is.EqualTo(3));
        Assert.That(result[0].Collection.Translations.All(t => t.Language != null));

        Assert.That(result[1].Id, Is.EqualTo(ids[1]));
        Assert.That(result[0].Office, Is.Not.Null);
        Assert.That(result[0].OfficeId, Is.EqualTo(result[0].Office.Id));
        Assert.That(result[0].Office.Translations.Count, Is.EqualTo(3));
        Assert.That(result[0].Office.Translations.All(t => t.Language != null));
    }

    [Test]
    public async Task GetByIdForUpdate_WithValidId_ReturnsManyToManyCollections()
    {
        var data = await entryRepository.GetByIdForUpdate(3);
        Assert.That(data, Is.Not.Null);

        Assert.That(data.LenochClassifications.Count, Is.EqualTo(3));
        Assert.That(data.LenochClassifications.All(d => d.Translations.Count == 3));
        Assert.That(data.LenochClassifications.All(d => d.Translations.All(t => t.Language != null)));

        Assert.That(data.RelatedCollections.Count, Is.EqualTo(2));
        Assert.That(data.RelatedCollections.All(rc => rc.Translations.Count == 3));
        Assert.That(data.RelatedCollections.All(rc => rc.Translations.All(t => t.Language != null)));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task GetByIdForUpdateReturnsEntry(int entryId)
    {
        var data = await entryRepository.GetByIdForUpdate(entryId);

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Id, Is.EqualTo(entryId));

        Assert.That(data.Collection, Is.Not.Null);
        Assert.That(data.CollectionId, Is.EqualTo(data.Collection.Id));
        Assert.That(data.Collection.Translations.Count, Is.EqualTo(3));
        Assert.That(data.Collection.Translations.All(t => t.Language != null));

        Assert.That(data.Office, Is.Not.Null);
        Assert.That(data.OfficeId, Is.EqualTo(data.Office.Id));
        Assert.That(data.Office.Translations.Count, Is.EqualTo(3));
        Assert.That(data.Office.Translations.All(t => t.Language != null));
    }

    [Test]
    public async Task GetByExternalSystemId_WithValidId_ShouldReturnEntry()
    {
        var entry = await entryRepository.GetByExternalSystemId(DatabaseHelper.ExternalSystemId);

        Assert.That(entry, Is.Not.Null);
        Assert.That(entry.ExternalSystemId, Is.EqualTo(DatabaseHelper.ExternalSystemId));
    }

    [Test]
    public async Task GetByIdUpdateReturnsManyToManyCollections()
    {
        var data = await entryRepository.GetByIdForEntryDetail(3);
        Assert.That(data, Is.Not.Null);

        Assert.That(data.LenochClassifications.Count, Is.EqualTo(3));

        Assert.That(data.RelatedCollections.Count, Is.EqualTo(2));
    }

    [TestCase(new[] { 3 })]
    [TestCase(new[] { 1, 2 })]
    [TestCase(new[] { 1, 2, 3 })]
    public async Task GetByIdsForSearchResultsReturnsSearchResults(int[] entryIds)
    {
        var data = (await entryRepository.GetByIdsForSearchResultsAdminOrWriterOrSuperReader(entryIds)).ToArray();

        Assert.That(data, Is.Not.Null);

        var containsAllEntries = entryIds.All(id => data.Any(r => r.Id == id));
        Assert.That(containsAllEntries, Is.True);
    }

    [Test]
    public async Task GetByIdsForWriterSuperUser_WhenCalled_DoesNotReturnDeleteOrInactiveRecords()
    {
        var newEntry = new Entry()
        {
            Id = 6,
            OfficeId = 3,
            CollectionId = 1,
            Reliability = Reliability.FirstDraft,
            Comment = "Comment3",
            Deleted = true,
            Status = Status.InProgress,
            Identification = 300,
            Created = DateTime.UtcNow.AddDays(-20),
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow.AddDays(-10),
            ModifiedBy = "test@test.ch",
        };

        context.Add(newEntry);

        var entryIds = new[] { 1, 6 };

        var data = (await entryRepository.GetByIdsForSearchResults(entryIds)).ToArray();

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Length, Is.EqualTo(1));
        context.Remove(newEntry);
    }

    [TestCase(new[] { 3 })]
    [TestCase(new[] { 1, 2 })]
    [TestCase(new[] { 1, 2, 3 })]
    public async Task GetByIdsForApiDetailReturns(int[] entryIds)
    {
        var data = (await entryRepository.GetByIdsForApiDetail(entryIds)).ToArray();

        Assert.That(data, Is.Not.Null);

        var containsAllEntries = entryIds.All(id => data.Any(r => r.Id == id));
        Assert.That(containsAllEntries, Is.True);
        Assert.That(data.All(e => e.Collection != null));
        Assert.That(data.Select(e => e.Collection).All(c => c.Translations != null));
        Assert.That(data.All(e => e.Office != null));
        Assert.That(data.Select(e => e.Office).All(o => o.Translations != null));
        Assert.That(data.All(e => e.Translations != null));
        Assert.That(data.SelectMany(e => e.Translations).All(e => e.Language != null));
        Assert.That(data.All(e => e.References != null));
        Assert.That(data.All(e => e.Subjects != null));
        Assert.That(data.SelectMany(e => e.Subjects).All(s => s.Translations != null));
        Assert.That(data.All(e => e.Domains != null));
        Assert.That(data.SelectMany(e => e.Domains).All(d => d.Translations != null));
    }

    [TestCase(new[] { 3 })]
    [TestCase(new[] { 1, 2 })]
    [TestCase(new[] { 1, 2, 3 })]
    public async Task GetByIdsForApiSummaryReturns(int[] entryIds)
    {
        var data = (await entryRepository.GetByIdsForApiSummary(entryIds)).ToArray();

        Assert.That(data, Is.Not.Null);

        var containsAllEntries = entryIds.All(id => data.Any(r => r.Id == id));
        Assert.That(containsAllEntries, Is.True);
        Assert.That(data.All(e => e.Collection != null));
        Assert.That(data.Select(e => e.Collection).All(c => c.Translations != null));
        Assert.That(data.All(e => e.Office != null));
        Assert.That(data.Select(e => e.Office).All(o => o.Translations != null));
        Assert.That(data.All(e => e.Translations != null));
        Assert.That(data.All(e => e.References == null));
        Assert.That(data.All(e => e.Subjects == null));
        Assert.That(data.All(e => e.Domains == null));
    }

    [Test]
    public async Task GetPageForIndex_ReturnsIndexEntries()
    {
        var data = entryRepository.GetPageForIndex(0, 100);
        var result = new List<IndexEntry>();
        await foreach (var indexEntry in data)
        {
            result.Add(indexEntry);
        }

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count, Is.GreaterThanOrEqualTo(16));
        Assert.That(result.Any(r => r.Subjects.Any()));
        Assert.That(result.Any(r => r.Domains.Any()));
        Assert.That(result.Any(r => r.LenochClassifications.Any()));
        Assert.That(result.Any(r => r.Glossaries.Any()));
        Assert.That(result.All(r => r.CollectionId > 0));
        Assert.That(result.All(r => r.OfficeId > 0));
        Assert.That(result.All(r => r.LanguageId > 0));
        Assert.That(result.All(r => r.SequenceId > 0));
        Assert.That(result.All(r => r.EntryId > 0));
        Assert.That(result.First(r => r.CollectionId == 1).CollectionIsActive, Is.True);
        Assert.That(result.First(r => r.CollectionId == 2).CollectionIsActive, Is.False);
    }

    [Test]
    public async Task GetByIdForEntryDetailReturnsObsoleteFlags()
    {
        var data = await entryRepository.GetByIdForEntryDetail(2);
        Assert.That(data, Is.Not.Null);

        var translations = data.Translations.ToList();

        Assert.That(translations[0].IsObsolete, Is.EqualTo(true));
        Assert.That(translations[0].IsRare, Is.EqualTo(true));
        Assert.That(translations[0].ShouldAvoid, Is.EqualTo(true));

        Assert.That(translations[1].IsObsolete, Is.EqualTo(false));
        Assert.That(translations[1].IsRare, Is.EqualTo(false));
        Assert.That(translations[1].ShouldAvoid, Is.EqualTo(false));

        Assert.That(translations[2].IsObsolete, Is.EqualTo(false));
        Assert.That(translations[2].IsRare, Is.EqualTo(false));
        Assert.That(translations[2].ShouldAvoid, Is.EqualTo(false));
    }

    [Test]
    public async Task CreateAddsEntity()
    {
        var subject = await context.Subjects.FirstAsync();
        var domain = await context.Domains.FirstAsync();
        var adminInfos = await context.AdminInfos.FirstAsync();

        var newEntry = new Entry
        {
            OfficeId = 1,
            CollectionId = 1,
            Reliability = Reliability.Approved,
            Comment = "Comment new",
            Status = Status.Validated,
            Identification = 100,
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "test@test.ch",
            Subjects = new List<Subject> { subject },
            Domains = new List<Domain> { domain },
            AdminInfos = new List<AdminInfo> { adminInfos },
            Translations = new List<EntryLanguage>
            {
                new()
                {
                    LanguageId = 1, Sequence = 1, Terminus = "test",
                },
            }
        };

        await entryRepository.Create(newEntry);

        await context.SaveChangesAsync();

        Assert.That(newEntry.Id, Is.Not.Default);

        context.Remove(newEntry);
        await context.SaveChangesAsync();
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task GetByIdReturnsEntry(int id)
    {
        var entry = await entryRepository.GetById(id);

        Assert.That(entry, Is.Not.Null);
        Assert.That(entry.Id, Is.EqualTo(id));
    }

    [Test]
    public async Task GetByIdReturnsNullForUnknownEntryId()
    {
        var entry = await entryRepository.GetById(10000);
        Assert.That(entry, Is.Null);
    }

    [Test]
    public async Task GetByIdForIndexReturnsIndexEntries()
    {
        var indexEntries = (await entryRepository.GetByIdForIndex(1)).ToList();

        Assert.That(indexEntries, Is.Not.Empty);
        Assert.That(indexEntries.Count, Is.EqualTo(3));
        Assert.That(indexEntries.All(e => e.EntryId == 1));
    }

    [Test]
    public async Task GetByIdForIndexReturnsIndexEntriesWithRelatedCollection()
    {
        var indexEntries = (await entryRepository.GetByIdForIndex(1)).ToList();

        Assert.That(indexEntries, Is.Not.Empty);
        Assert.That(indexEntries.Count, Is.EqualTo(3));
        Assert.That(indexEntries.All(e => e.RelatedCollections.Length == 1));
    }

    [Test]
    public async Task GetByIdsForIndexReturnsIndexEntries()
    {
        var indexEntries = (await entryRepository.GetByIdsForIndex(new[] { 1, 2 })).ToList();

        Assert.That(indexEntries, Is.Not.Empty);
        Assert.That(indexEntries.Count, Is.EqualTo(6));
        Assert.That(indexEntries.All(e => e.EntryId is 1 or 2));
    }

    [Test]
    public async Task GetIdsByCollectionIdReturnsIds()
    {
        using var cleanContext = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        entryRepository = new EntryRepository(cleanContext);

        var indexEntries = (await entryRepository.GetIdsByCollectionId(1)).ToList();

        Assert.That(indexEntries, Is.Not.Empty);
        Assert.That(indexEntries.Count, Is.EqualTo(3));
        Assert.That(indexEntries, Is.EquivalentTo(new[] { 1, 3, 5 }));
    }

    [Test]
    public async Task GetIdsByGlossaryIdReturnsIds()
    {
        using var cleanContext = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        entryRepository = new EntryRepository(cleanContext);

        var indexEntries = (await entryRepository.GetIdsByGlossaryId(1)).ToList();

        Assert.That(indexEntries, Is.Not.Empty);
        Assert.That(indexEntries.Count, Is.EqualTo(2));
        Assert.That(indexEntries, Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public async Task DeleteRemovesEntry()
    {
        await context.Entries.AddAsync(new Entry
        {
            Id = 1000,
            OfficeId = 1,
            CollectionId = 1,
            Reliability = Reliability.Approved,
            Status = Status.Validated,
            Identification = 100,
            Created = DateTime.UtcNow.AddDays(-10),
            Modified = DateTime.UtcNow.AddDays(-5),
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
            Deleted = false,
        });

        await context.SaveChangesAsync();

        var entry = await context.Entries.SingleOrDefaultAsync(e => e.Id == 1000);
        Assert.That(entry, Is.Not.Null);

        entryRepository.Delete(entry);

        entry = await context.Entries.SingleOrDefaultAsync(e => e.Id == 1000);
        Assert.That(entry, Is.Not.Null);

        await context.SaveChangesAsync();

        entry = await context.Entries.SingleOrDefaultAsync(e => e.Id == 1000);
        Assert.That(entry, Is.Null);
    }

    [Test]
    public async Task GetByIdsForExportShouldReturnEntries()
    {
        var data = (await entryRepository.GetByIdsForExport(new[] { 1, 2, 3 })).ToList();

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Count, Is.EqualTo(3));
        Assert.That(data.All(d => d.Collection is not null));
        Assert.That(data.All(d => d.ConceptSystemPosition is not null));
    }

    [Test]
    public async Task GetByIdsForExportShouldIncludeTranslations()
    {
        var data = (await entryRepository.GetByIdsForExport(new[] { 1, 2, 3 })).ToList();

        Assert.That(data, Is.Not.Null);
        data.ForEach(item =>
        {
            Assert.That(item.Translations.All(t => t.Language is not null));

            Assert.That(item.Collection?.Translations, Is.Not.Null);
            Assert.That(item.Collection.Translations.All(t => t.Language is not null));

            Assert.That(item.Office?.Translations, Is.Not.Null);
            Assert.That(item.Office.Translations.All(t => t.Language is not null));

            Assert.That(item.LenochClassifications, Is.Not.Null);
            Assert.That(item.LenochClassifications.All(lc => lc.Translations.All(t => t.Language is not null)));

            Assert.That(item.Subjects, Is.Not.Null);
            Assert.That(item.Subjects.All(lc => lc.Translations.All(t => t.Language is not null)));

            Assert.That(item.Domains, Is.Not.Null);
            Assert.That(item.Domains.All(lc => lc.Translations.All(t => t.Language is not null)));

            Assert.That(item.RelatedCollections, Is.Not.Null);
            Assert.That(item.RelatedCollections.All(lc => lc.Translations.All(t => t.Language is not null)));

            Assert.That(item.Glossaries, Is.Not.Null);
            Assert.That(item.Glossaries.All(lc => lc.Translations.All(t => t.Language is not null)));

            Assert.That(item.References, Is.Not.Null);
            Assert.That(item.References.All(lc => lc.ReferencedEntry.Translations.All(t => t.Language is not null)));

            Assert.That(item.ReferencedByEntries, Is.Not.Null);
            Assert.That(item.ReferencedByEntries.All(lc => lc.Entry.Translations.All(t => t.Language is not null)));
        });
    }

    [Test]
    public async Task GetByIdsForExportShouldIncludePicture()
    {
        var data = (await entryRepository.GetByIdsForExport(new[] { 1, 2 })).ToList();

        Assert.That(data, Is.Not.Null);
        data.ForEach(item =>
        {
            Assert.That(item.Picture, Is.Not.Null);
            Assert.That(item.Picture.Translations.All(t => t.Language is not null));
            Assert.That(item.Picture.File, Is.Not.Null);
            Assert.That(item.Picture.File.Value, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetByIdsForExportShouldReturnInRequestedOrder()
    {
        var data = (await entryRepository.GetByIdsForExport(new[] { 2, 3, 1 })).ToList();

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Count, Is.EqualTo(3));
        Assert.That(data[0].Id, Is.EqualTo(2));
        Assert.That(data[1].Id, Is.EqualTo(3));
        Assert.That(data[2].Id, Is.EqualTo(1));
    }

    [Test]
    public async Task CanUpdate()
    {
        var dbEntry = await entryRepository.GetByIdForUpdate(5);
        var updatedEntry = await entryRepository.GetByIdForEntryDetail(5);

        updatedEntry.OfficeId = 1;
        updatedEntry.CollectionId = 2;
        updatedEntry.Reliability = Reliability.Approved;
        updatedEntry.Comment = "Updated comment";
        var firstLenochClassification = await context.LenochClassifications.FirstAsync();
        var lastLenochClassification = await context.LenochClassifications.LastAsync();
        updatedEntry.LenochClassifications = new List<LenochClassification> { firstLenochClassification, lastLenochClassification };
        updatedEntry.Identification = 1337;
        updatedEntry.Status = Status.Validated;
        var firstCollection = await context.Collections.FirstAsync();
        var lastCollection = await context.Collections.LastAsync();
        updatedEntry.RelatedCollections = new List<Collection> { firstCollection, lastCollection };
        var firstGlossary = await context.Glossaries.FirstAsync();
        var lastGlossary = await context.Glossaries.LastAsync();

        entryRepository.Update(dbEntry, updatedEntry);

        await context.SaveChangesAsync();

        var updatedEntryAfterUpdate = await entryRepository.GetByIdForEntryDetail(5);
        Assert.That(updatedEntryAfterUpdate.OfficeId, Is.EqualTo(1));
        Assert.That(updatedEntryAfterUpdate.Office.Id, Is.EqualTo(1));
        Assert.That(updatedEntryAfterUpdate.CollectionId, Is.EqualTo(2));
        Assert.That(updatedEntryAfterUpdate.Collection.Id, Is.EqualTo(2));
        Assert.That(updatedEntryAfterUpdate.Reliability, Is.EqualTo(Reliability.Approved));
        Assert.That(updatedEntryAfterUpdate.Status, Is.EqualTo(Status.Validated));
        Assert.That(updatedEntryAfterUpdate.Identification, Is.EqualTo(1337));
        Assert.That(updatedEntryAfterUpdate.Comment, Is.EqualTo("Updated comment"));
        Assert.That(updatedEntryAfterUpdate.LenochClassifications.Count, Is.EqualTo(2));
        Assert.That(updatedEntryAfterUpdate.LenochClassifications.All(d => d.Id == firstLenochClassification.Id || d.Id == lastLenochClassification.Id));
        Assert.That(updatedEntryAfterUpdate.RelatedCollections.Count, Is.EqualTo(2));
        Assert.That(updatedEntryAfterUpdate.RelatedCollections.All(d => d.Id == firstCollection.Id || d.Id == lastCollection.Id));
        Assert.That(updatedEntryAfterUpdate.Glossaries.All(d => d.Id == firstGlossary.Id || d.Id == lastGlossary.Id));
    }

    [Test]
    public async Task CanUpdateReferences()
    {
        var dbEntry = await entryRepository.GetByIdForUpdate(4);
        var updatedEntry = await entryRepository.GetByIdForEntryDetail(4);

        updatedEntry.References = new List<EntryReference>
        {
            new() { EntryId = 4, Type = ReferenceType.Specific, ReferencedEntryId = 1 },
            new() { EntryId = 4, Type = ReferenceType.None, ReferencedEntryId = 2 },
        };

        entryRepository.Update(dbEntry, updatedEntry);
        await context.SaveChangesAsync();

        var updatedEntryAfterUpdate = await entryRepository.GetByIdForEntryDetail(4);
        Assert.That(updatedEntryAfterUpdate.References, Is.Not.Empty);
        Assert.That(updatedEntryAfterUpdate.References.Count, Is.EqualTo(2));
        var references = updatedEntryAfterUpdate.References.ToList();

        Assert.That(references[0].EntryId, Is.EqualTo(4));
        Assert.That(references[0].ReferencedEntryId, Is.EqualTo(2));
        Assert.That(references[0].Type, Is.EqualTo(ReferenceType.None));

        Assert.That(references[1].EntryId, Is.EqualTo(4));
        Assert.That(references[1].ReferencedEntryId, Is.EqualTo(1));
        Assert.That(references[1].Type, Is.EqualTo(ReferenceType.Specific));

        dbEntry = await entryRepository.GetByIdForUpdate(4);

        references[0].ReferencedEntryId = 3;
        references[0].Type = ReferenceType.General;
        updatedEntry.References = new List<EntryReference>
        {
            references[0]
        };

        entryRepository.Update(dbEntry, updatedEntry);
        await context.SaveChangesAsync();

        updatedEntryAfterUpdate = await entryRepository.GetByIdForEntryDetail(4);
        Assert.That(updatedEntryAfterUpdate.References, Is.Not.Empty);
        Assert.That(updatedEntryAfterUpdate.References.Count, Is.EqualTo(1));
        references = updatedEntryAfterUpdate.References.ToList();

        Assert.That(references[0].EntryId, Is.EqualTo(4));
        Assert.That(references[0].ReferencedEntryId, Is.EqualTo(3));
        Assert.That(references[0].Type, Is.EqualTo(ReferenceType.General));

        dbEntry = await entryRepository.GetByIdForUpdate(4);
        updatedEntry.References = new List<EntryReference>();

        entryRepository.Update(dbEntry, updatedEntry);
        await context.SaveChangesAsync();

        updatedEntryAfterUpdate = await entryRepository.GetByIdForEntryDetail(4);
        Assert.That(updatedEntryAfterUpdate.References, Is.Empty);
    }

    [Test]
    public async Task CanUpdateWithMultipleAddedAndUpdatedAndDeletedEntryLanguages()
    {
        var dbEntry = await entryRepository.GetByIdForUpdate(5);
        var updatedEntry = await entryRepository.GetByIdForEntryDetail(5);

        var languages = updatedEntry.Translations.ToList();
        languages[0].Abbreviation = "Updated Abbreviation";
        languages[0].AbbreviationSource = "Updated AbbreviationSource";
        languages[0].Name = "Updated Name";
        languages[0].NameSource = "Updated NameSource";
        languages[0].Context = "Updated Context";
        languages[0].ContextSource = "Updated ContextSource";
        languages[0].Country = "Updated Country";
        languages[0].Terminus = "Updated Terminus";
        languages[0].TerminusSource = "Updated TerminusSource";
        languages[0].Phraseology = "Updated Phraseology";
        languages[0].PhraseologySource = "Updated PhraseologySource";
        languages[0].Definition = "Updated Definition";
        languages[0].DefinitionSource = "Updated DefinitionSource";
        languages[0].Metadata = "Updated Metadata";
        languages[0].MetadataSource = "Updated MetadataSource";
        languages[0].Note = "Updated Note";
        languages[0].NoteSource = "Updated NoteSource";
        languages[0].Sequence = 3;
        updatedEntry.Translations = new List<EntryLanguage>()
        {
            languages[0],
            languages[1],
            languages[3],
            new() {  EntryId = 3, LanguageId = 2, Sequence = 2, Terminus = "new terminus 1", },
            new() {  EntryId = 3, LanguageId = 2, Sequence = 1, Terminus = "new terminus 2", },
            new() {  EntryId = 3, LanguageId = 3, Sequence = 2, Name = "new name", },
        };

        entryRepository.Update(dbEntry, updatedEntry);

        await context.SaveChangesAsync();

        var updatedEntryAfterUpdate = await entryRepository.GetByIdForEntryDetail(5);

        var updatedTranslations = updatedEntryAfterUpdate.Translations.ToList();
        Assert.That(updatedTranslations.Count, Is.EqualTo(6));
        Assert.That(updatedTranslations.Count(t => t.LanguageId == 1), Is.EqualTo(1));
        Assert.That(updatedTranslations.Count(t => t.LanguageId == 2), Is.EqualTo(3));
        Assert.That(updatedTranslations.Count(t => t.LanguageId == 3), Is.EqualTo(2));
        // check language entry where all fields changed
        Assert.That(updatedTranslations[0].Id, Is.EqualTo(13));
        Assert.That(updatedTranslations[0].Abbreviation, Is.EqualTo("Updated Abbreviation"));
        Assert.That(updatedTranslations[0].AbbreviationSource, Is.EqualTo("Updated AbbreviationSource"));
        Assert.That(updatedTranslations[0].Name, Is.EqualTo("Updated Name"));
        Assert.That(updatedTranslations[0].NameSource, Is.EqualTo("Updated NameSource"));
        Assert.That(updatedTranslations[0].Context, Is.EqualTo("Updated Context"));
        Assert.That(updatedTranslations[0].ContextSource, Is.EqualTo("Updated ContextSource"));
        Assert.That(updatedTranslations[0].Country, Is.EqualTo("Updated Country"));
        Assert.That(updatedTranslations[0].Terminus, Is.EqualTo("Updated Terminus"));
        Assert.That(updatedTranslations[0].TerminusSource, Is.EqualTo("Updated TerminusSource"));
        Assert.That(updatedTranslations[0].Phraseology, Is.EqualTo("Updated Phraseology"));
        Assert.That(updatedTranslations[0].PhraseologySource, Is.EqualTo("Updated PhraseologySource"));
        Assert.That(updatedTranslations[0].Definition, Is.EqualTo("Updated Definition"));
        Assert.That(updatedTranslations[0].DefinitionSource, Is.EqualTo("Updated DefinitionSource"));
        Assert.That(updatedTranslations[0].Metadata, Is.EqualTo("Updated Metadata"));
        Assert.That(updatedTranslations[0].MetadataSource, Is.EqualTo("Updated MetadataSource"));
        Assert.That(updatedTranslations[0].Note, Is.EqualTo("Updated Note"));
        Assert.That(updatedTranslations[0].NoteSource, Is.EqualTo("Updated NoteSource"));
        Assert.That(updatedTranslations[0].LanguageId, Is.EqualTo(1));
        Assert.That(updatedTranslations[0].Sequence, Is.EqualTo(3));

        // Check language where no field changed
        Assert.That(updatedTranslations[1].Id, Is.EqualTo(14));
        Assert.That(updatedTranslations[1].Abbreviation, Is.Null);
        Assert.That(updatedTranslations[1].AbbreviationSource, Is.Null);
        Assert.That(updatedTranslations[1].Name, Is.Null);
        Assert.That(updatedTranslations[1].NameSource, Is.Null);
        Assert.That(updatedTranslations[1].Context, Is.Null);
        Assert.That(updatedTranslations[1].ContextSource, Is.Null);
        Assert.That(updatedTranslations[1].Country, Is.Null);
        Assert.That(updatedTranslations[1].Terminus, Is.EqualTo("site (1)"));
        Assert.That(updatedTranslations[1].TerminusSource, Is.Null);
        Assert.That(updatedTranslations[1].Phraseology, Is.EqualTo("En site, on peut tolérer une variation qui est provoquée par une différence de position lance eg/RP extrême [...]."));
        Assert.That(updatedTranslations[1].PhraseologySource, Is.Null);
        Assert.That(updatedTranslations[1].Definition, Is.Null);
        Assert.That(updatedTranslations[1].DefinitionSource, Is.Null);
        Assert.That(updatedTranslations[1].Metadata, Is.Null);
        Assert.That(updatedTranslations[1].MetadataSource, Is.Null);
        Assert.That(updatedTranslations[1].Note, Is.Null);
        Assert.That(updatedTranslations[1].NoteSource, Is.Null);
        Assert.That(updatedTranslations[1].LanguageId, Is.EqualTo(2));
        Assert.That(updatedTranslations[1].Sequence, Is.EqualTo(1));

        Assert.That(updatedTranslations[2].Id, Is.EqualTo(16));

        // Check new entry language
        Assert.That(updatedTranslations[3].Id, Is.EqualTo(19));
        Assert.That(updatedTranslations[3].Abbreviation, Is.Null);
        Assert.That(updatedTranslations[3].AbbreviationSource, Is.Null);
        Assert.That(updatedTranslations[3].Name, Is.EqualTo("new name"));
        Assert.That(updatedTranslations[3].NameSource, Is.Null);
        Assert.That(updatedTranslations[3].Context, Is.Null);
        Assert.That(updatedTranslations[3].ContextSource, Is.Null);
        Assert.That(updatedTranslations[3].Country, Is.Null);
        Assert.That(updatedTranslations[3].Terminus, Is.Null);
        Assert.That(updatedTranslations[3].TerminusSource, Is.Null);
        Assert.That(updatedTranslations[3].Phraseology, Is.Null);
        Assert.That(updatedTranslations[3].PhraseologySource, Is.Null);
        Assert.That(updatedTranslations[3].Definition, Is.Null);
        Assert.That(updatedTranslations[3].DefinitionSource, Is.Null);
        Assert.That(updatedTranslations[3].Metadata, Is.Null);
        Assert.That(updatedTranslations[3].MetadataSource, Is.Null);
        Assert.That(updatedTranslations[3].Note, Is.Null);
        Assert.That(updatedTranslations[3].NoteSource, Is.Null);
        Assert.That(updatedTranslations[3].LanguageId, Is.EqualTo(3));
        Assert.That(updatedTranslations[3].Sequence, Is.EqualTo(2));

        Assert.That(updatedTranslations[4].Id, Is.EqualTo(18));
        Assert.That(updatedTranslations[4].Terminus, Is.EqualTo("new terminus 2"));
        Assert.That(updatedTranslations[4].LanguageId, Is.EqualTo(2));
        Assert.That(updatedTranslations[4].Sequence, Is.EqualTo(1));

        Assert.That(updatedTranslations[5].Id, Is.EqualTo(17));
        Assert.That(updatedTranslations[5].Terminus, Is.EqualTo("new terminus 1"));
        Assert.That(updatedTranslations[5].LanguageId, Is.EqualTo(2));
        Assert.That(updatedTranslations[5].Sequence, Is.EqualTo(2));

        // Check delete
        Assert.That(await context.EntryLanguages.SingleOrDefaultAsync(l => l.Terminus == "will be deleted in unit test"), Is.EqualTo(null));
    }

    [Test]
    public async Task GetByIdsForWatchlist_WhenCalled_ShouldReturnWatchlistEntries()
    {
        var watchlistEntries = (await entryRepository.GetByIdsForWatchlist(new[] { 3, 1, 2 })).ToList();

        Assert.That(watchlistEntries, Is.Not.Null);
        Assert.That(watchlistEntries.Count, Is.EqualTo(3));

        Assert.That(watchlistEntries[0].Id, Is.EqualTo(3));
        Assert.That(watchlistEntries[1].Id, Is.EqualTo(1));
        Assert.That(watchlistEntries[2].Id, Is.EqualTo(2));

        Assert.That(watchlistEntries[0].ConceptSystemPosition, Is.InstanceOf<ConceptSystemPosition>());
        Assert.That(watchlistEntries[0].ConceptSystemPosition.Id, Is.EqualTo(2));
        Assert.That(watchlistEntries[0].Collection, Is.InstanceOf<Collection>());
        Assert.That(watchlistEntries[0].Collection.Translations.Count, Is.EqualTo(3));
        Assert.That(watchlistEntries[0].Collection.Translations.All(t => t.Language != null));
        Assert.That(watchlistEntries[0].Translations.Count, Is.EqualTo(4));
        Assert.That(watchlistEntries[0].Translations.All(t => t.Language != null));
    }

    [TestCase(new[] { 1 }, 2)]
    [TestCase(new[] { 2 }, 1)]
    [TestCase(new[] { 1, 2 }, 3)]
    public async Task GetIdsByConceptPositionIds_WhenCalled_ShouldReturnEntryIds(int[] ids, int expectedCount)
    {
        var entryIds = (await entryRepository.GetIdsByConceptPositionIds(ids)).ToList();

        Assert.That(entryIds, Is.Not.Null);
        Assert.That(entryIds.Count, Is.EqualTo(expectedCount));
    }

    [TestCase(2, 1)]
    [TestCase(3, 1)]
    [TestCase(4, 1)]
    public async Task GetIdByAdmininfoId_WhenCalled_ShouldReturnEntryIds(int id, int expectedCount)
    {
        var entryIds = (await entryRepository.GetIdsByAdminInfoId(id)).ToList();

        Assert.That(entryIds, Is.Not.Null);
        Assert.That(entryIds.Count, Is.EqualTo(expectedCount));
    }

    [Test]
    public async Task Update_WhenCalled_ShouldUpdateSubjects()
    {
        var dbEntry = await entryRepository.GetByIdForUpdate(1);
        var updatedEntry = await entryRepository.GetByIdForEntryDetail(1);

        updatedEntry.Subjects.Clear();
        updatedEntry.Domains.Clear();

        var subject1 = await context.Subjects.FirstAsync();
        var subject2 = await context.Subjects.LastAsync();

        updatedEntry.Subjects = new List<Subject> { subject1, subject2 };

        entryRepository.Update(dbEntry, updatedEntry);

        await context.SaveChangesAsync();

        var updatedEntryAfterUpdate = await entryRepository.GetByIdForEntryDetail(1);
        Assert.That(updatedEntryAfterUpdate.Subjects, Is.Not.Empty);
        Assert.That(updatedEntryAfterUpdate.Subjects.Count, Is.EqualTo(2));
        Assert.That(updatedEntryAfterUpdate.Subjects.First().Id, Is.EqualTo(subject1.Id));
        Assert.That(updatedEntryAfterUpdate.Subjects.Last().Id, Is.EqualTo(subject2.Id));
    }

    [Test]
    public async Task Update_WhenCalled_ShouldUpdateDomains()
    {
        var dbEntry = await entryRepository.GetByIdForUpdate(2);
        var updatedEntry = await entryRepository.GetByIdForEntryDetail(2);

        updatedEntry.Subjects.Clear();
        updatedEntry.Domains.Clear();

        var subject1 = await context.Subjects.Include(s => s.Domains).FirstAsync(s => s.Id == 1);
        var subject2 = await context.Subjects.Include(s => s.Domains).FirstAsync(s => s.Id == 2);
        var domain1 = subject1.Domains.Last();
        var domain2 = subject2.Domains.Last();

        updatedEntry.Subjects = new List<Subject> { subject1, subject2 };
        updatedEntry.Domains = new List<Domain> { domain1, domain2 };

        entryRepository.Update(dbEntry, updatedEntry);

        await context.SaveChangesAsync();

        var updatedEntryAfterUpdate = await entryRepository.GetByIdForEntryDetail(2);
        Assert.That(updatedEntryAfterUpdate.Subjects, Is.Not.Empty);
        Assert.That(updatedEntryAfterUpdate.Subjects.Count, Is.EqualTo(2));
        Assert.That(updatedEntryAfterUpdate.Subjects.Any(s => s.Id == subject1.Id));
        Assert.That(updatedEntryAfterUpdate.Subjects.Any(s => s.Id == subject2.Id));

        Assert.That(updatedEntryAfterUpdate.Domains, Is.Not.Empty);
        Assert.That(updatedEntryAfterUpdate.Domains.Count, Is.EqualTo(2));
        Assert.That(updatedEntryAfterUpdate.Domains.Any(s => s.Id == domain1.Id));
        Assert.That(updatedEntryAfterUpdate.Domains.Any(s => s.Id == domain2.Id));
    }

    [Test]
    public async Task GetExpiredLockedEntries_WhenCalled_ReturnsLockedEntries()
    {
        var bla = await context.Entries.ToListAsync();

        bla[0].Locked = DateTime.UtcNow.AddMinutes(-120);
        bla[1].Locked = DateTime.UtcNow.AddMinutes(-240);
        bla[2].Locked = DateTime.UtcNow.AddMinutes(-119);
        bla[3].Locked = DateTime.UtcNow.AddMinutes(-121);

        await context.SaveChangesAsync();

        var expiredLockedEntries = (await entryRepository.GetExpiredLockedEntries()).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(expiredLockedEntries, Has.Count.EqualTo(3));
            Assert.That(expiredLockedEntries, Has.Member(bla[0]));
            Assert.That(expiredLockedEntries, Has.Member(bla[1]));
            Assert.That(expiredLockedEntries, Has.Member(bla[3]));
        });

        bla[0].Locked = null;
        bla[1].Locked = null;
        bla[2].Locked = null;
        bla[3].Locked = null;
        await context.SaveChangesAsync();
    }
}
