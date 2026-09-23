using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Messages;
using Bk.Termdat.Business.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.Service.Tests;

[TestFixture]
internal class SearchIndexEntryEventConsumerTests
{
    private SearchIndexEntryEventConsumer searchIndexEntryEventConsumer = null!;

    private readonly ISearchRepository searchRepository = Substitute.For<ISearchRepository>();
    private readonly IEntryRepository entryRepository = Substitute.For<IEntryRepository>();
    private readonly Fixture fixture = new();

    [SetUp]
    public void SetUp()
    {
        searchRepository.ClearSubstitute();
        entryRepository.ClearSubstitute();

        searchIndexEntryEventConsumer = new SearchIndexEntryEventConsumer(searchRepository, entryRepository, Substitute.For<ILogger<SearchIndexEntryEventConsumer>>());
        entryRepository.GetByIdsForIndex(Arg.Any<IEnumerable<int>>()).ReturnsForAnyArgs(new List<IndexEntry>
        {
            new(), new(), new()
        });
    }

    [TearDown]
    public void TearDown()
    {
        searchRepository.ClearSubstitute();
    }

    [Test]
    public async Task Consume_WithSearchIndexEntryAddEvent_ShouldAddEntries()
    {
        var indexEntries = fixture.CreateMany<IndexEntry>().ToArray();
        IndexEntry[] actualIndexEntries = null;
        searchRepository.AddEntries(Arg.Do<IEnumerable<IndexEntry>>(x => { actualIndexEntries = x.ToArray(); }));

        await searchIndexEntryEventConsumer.Consume(new SearchIndexEntryAddEvent { IndexEntries = indexEntries });

        Assert.That(actualIndexEntries, Is.Not.Null);
        foreach (var actualIndexEntry in actualIndexEntries)
        {
            var indexEntry = indexEntries.Single(e => e.EntryId == actualIndexEntry.EntryId);

            Assert.That(actualIndexEntry, Is.EqualTo(indexEntry).Using(IndexEntryComparer));
        }
    }

    [Test]
    public async Task Consume_WithEmptySearchIndexEntryAddEvent_ShouldNotAddEntries()
    {
        var indexEntries = Array.Empty<IndexEntry>();

        await searchIndexEntryEventConsumer.Consume(new SearchIndexEntryAddEvent { IndexEntries = indexEntries });

        searchRepository.Received(0).AddEntries(Arg.Any<IndexEntry[]>());
    }

    [Test]
    public async Task Consume_WithSearchIndexEntryUpdateEvent_ShouldUpdateEntries()
    {
        var indexEntries = fixture.CreateMany<IndexEntry>().ToArray();
        IndexEntry[] actualIndexEntries = null;
        searchRepository.UpdateEntries(Arg.Do<IEnumerable<IndexEntry>>(x =>
        {
            actualIndexEntries = x.ToArray();
        }));

        await searchIndexEntryEventConsumer.Consume(new SearchIndexEntryUpdateEvent { IndexEntries = indexEntries });

        Assert.That(actualIndexEntries, Is.Not.Null);
        foreach (var actualIndexEntry in actualIndexEntries)
        {
            var indexEntry = indexEntries.Single(e => e.EntryId == actualIndexEntry.EntryId);

            Assert.That(actualIndexEntry, Is.EqualTo(indexEntry).Using(IndexEntryComparer));
        }
    }

    [Test]
    public async Task Consume_WithEmptySearchIndexEntryUpdateEvent_ShouldNotUpdateEntries()
    {
        var indexEntries = Array.Empty<IndexEntry>();

        await searchIndexEntryEventConsumer.Consume(new SearchIndexEntryUpdateEvent { IndexEntries = indexEntries });

        searchRepository.Received(0).UpdateEntries(Arg.Any<IndexEntry[]>());
    }

    [Test]
    public async Task Consume_WithSearchIndexEntryDeleteEvent_ShouldDeleteEntries()
    {
        var entryIds = fixture.CreateMany<int>().ToArray();
        int[] actualEntryIds = null;
        searchRepository.DeleteEntries(Arg.Do<IEnumerable<int>>(x =>
        {
            actualEntryIds = x.ToArray();
        }));

        await searchIndexEntryEventConsumer.Consume(new SearchIndexEntryDeleteEvent { EntryIds = entryIds });

        Assert.That(actualEntryIds, Is.Not.Null);
        Assert.That(actualEntryIds, Is.EquivalentTo(entryIds));
    }

    [Test]
    public async Task Consume_WithEmptySearchIndexEntryDeleteEvent_ShouldNotDeleteEntries()
    {
        var entryIds = Array.Empty<int>();
        await searchIndexEntryEventConsumer.Consume(new SearchIndexEntryDeleteEvent { EntryIds = entryIds });

        searchRepository.Received(0).DeleteEntries(Arg.Any<int[]>());
    }

    [Test]
    public async Task Consume_WithSearchIndexEntryUpdateByEntryIdsEvent_ShouldUpdateEntries()
    {
        var entryIds = fixture.CreateMany<int>().ToList();
        await searchIndexEntryEventConsumer.Consume(new SearchIndexEntryUpdateByEntryIdsEvent { EntryIds = entryIds });

        await entryRepository.Received(1).GetByIdsForIndex(Arg.Is<IEnumerable<int>>(e => entryIds.TrueForAll(e.Contains)));
        searchRepository.Received(1).UpdateEntries(Arg.Is<List<IndexEntry>>(entries => entries.Count == 3));
    }

    [Test]
    public async Task Consume_WithEmptySearchIndexEntryUpdateByEntryIdsEvent_ShouldNotUpdateEntries()
    {
        var entryIds = Array.Empty<int>();

        await searchIndexEntryEventConsumer.Consume(new SearchIndexEntryUpdateByEntryIdsEvent { EntryIds = entryIds });

        await entryRepository.Received(0).GetByIdsForIndex(Arg.Any<int[]>());
        searchRepository.Received(0).UpdateEntries(Arg.Any<IndexEntry[]>());
    }

    private sealed class IndexEntryEqualityComparer : IEqualityComparer<IndexEntry>
    {
        public bool Equals(IndexEntry x, IndexEntry y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null)
            {
                return false;
            }

            if (y is null)
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            var equals = x.EntryId == y.EntryId &&
                          x.OfficeId == y.OfficeId &&
                          x.CollectionId == y.CollectionId &&
                          x.Status == y.Status &&
                          x.Comment == y.Comment &&
                          x.LanguageId == y.LanguageId &&
                          x.SequenceId == y.SequenceId &&
                          x.Terminus == y.Terminus &&
                          x.TerminusSource == y.TerminusSource &&
                          x.Name == y.Name &&
                          x.NameSource == y.NameSource &&
                          x.Abbreviation == y.Abbreviation &&
                          x.AbbreviationSource == y.AbbreviationSource &&
                          x.Phraseology == y.Phraseology &&
                          x.PhraseologySource == y.PhraseologySource &&
                          x.Definition == y.Definition &&
                          x.DefinitionSource == y.DefinitionSource &&
                          x.Note == y.Note &&
                          x.NoteSource == y.NoteSource &&
                          x.Context == y.Context &&
                          x.ContextSource == y.ContextSource &&
                          x.Country == y.Country &&
                          x.Metadata == y.Metadata &&
                          x.MetadataSource == y.MetadataSource &&
                          x.CreatedBy == y.CreatedBy &&
                          x.Modified.Equals(y.Modified) &&
                          x.ModifiedBy == y.ModifiedBy &&
                          x.Deleted == y.Deleted;

            Assert.That(x.LenochClassifications, Is.EquivalentTo(y.LenochClassifications));

            return equals;
        }

        public int GetHashCode(IndexEntry obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.EntryId);
            hashCode.Add(obj.OfficeId);
            hashCode.Add(obj.CollectionId);
            hashCode.Add(obj.LenochClassifications);
            hashCode.Add(obj.Status);
            hashCode.Add(obj.Comment);
            hashCode.Add(obj.AdminInfos);
            hashCode.Add(obj.LanguageId);
            hashCode.Add(obj.SequenceId);
            hashCode.Add(obj.Terminus);
            hashCode.Add(obj.TerminusSource);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.NameSource);
            hashCode.Add(obj.Abbreviation);
            hashCode.Add(obj.AbbreviationSource);
            hashCode.Add(obj.Phraseology);
            hashCode.Add(obj.PhraseologySource);
            hashCode.Add(obj.Definition);
            hashCode.Add(obj.DefinitionSource);
            hashCode.Add(obj.Note);
            hashCode.Add(obj.NoteSource);
            hashCode.Add(obj.Context);
            hashCode.Add(obj.ContextSource);
            hashCode.Add(obj.Country);
            hashCode.Add(obj.Metadata);
            hashCode.Add(obj.MetadataSource);
            hashCode.Add(obj.CreatedBy);
            hashCode.Add(obj.Modified);
            hashCode.Add(obj.ModifiedBy);
            hashCode.Add(obj.Deleted);
            return hashCode.ToHashCode();
        }
    }

    public static IEqualityComparer<IndexEntry> IndexEntryComparer { get; } = new IndexEntryEqualityComparer();
}
