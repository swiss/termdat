using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Infrastructure.Search.Repositories;
using Bogus;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using CommentStatus = Bk.Termdat.Business.Resource.Models.CommentStatus;

namespace Bk.Termdat.Infrastructure.Search.Tests.Repositories;

[TestFixture]
internal class SearchRepositoryTests
{
    private SearchRepository searchRepository;
    private SearchGateway gateway;
    private SearchFilterParameter searchFilterParameter;
    private SearchFields searchFields;
    private IOptions<SearchOptions> searchOptionsMock;
    private IList<IndexEntry> indexEntries;
    private readonly SearchOptions searchOptions = new()
    {
        RefreshDelay = 5.0f,
        PreferredEntrySequenceId = "1",
        AchOfficeId = 1,
        QueryBoostHighestPriority = 10.0f,
        QueryBoostVeryHighPriority = 7.5f,
        QueryBoostHighPriority = 5.0f,
        QueryBoostMidPriority = 4.0f,
        QueryBoostLowPriority = 3.0f,
        QueryBoostVeryLowPriority = 2.0f,
        QueryBoostLowestPriority = 1.5f,
        RecommendationMinLength = 4,
        RecommendationResultSize = 10,
        RecreateIndexOnStartUp = true,
        QueryBoostPriorityMultiplier = 10f
    };

    [SetUp]
    public void Setup()
    {
        Randomizer.Seed = new Random(8675309);

        searchOptionsMock = Substitute.For<IOptions<SearchOptions>>();
        searchOptionsMock.Value.Returns(searchOptions);

        gateway = new TestSearchGateway(Substitute.For<ILogger<SearchGateway>>(), searchOptionsMock, null);
        searchRepository = new SearchRepository(Substitute.For<ILogger<SearchRepository>>(), gateway, searchOptionsMock);
        indexEntries = TestHelpers.GenerateIndexEntries();
    }

    [TearDown]
    public void TearDown()
    {
        gateway.Dispose();
    }

    [Test]
    public async Task ImportEntries_AddsEntryToIndex()
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var query = new MatchAllDocsQuery();
        var searcher = gateway.SearcherManager.Acquire();

        var docs = searcher.Search(query, 100000);

        Assert.That(docs, Is.Not.Null);
        Assert.That(docs.TotalHits, Is.EqualTo(4000));

        var entry1 = GetIndexEntry(searcher.Doc(docs.ScoreDocs[0].Doc));

        Assert.That(entry1.EntryId, Is.EqualTo(indexEntries[0].EntryId));
        Assert.That(entry1.LanguageId, Is.EqualTo(indexEntries[0].LanguageId));
        Assert.That(entry1.SequenceId, Is.EqualTo(indexEntries[0].SequenceId));

        Assert.That(entry1.Terminus, Is.EqualTo(indexEntries[0].Terminus));
        Assert.That(entry1.Abbreviation, Is.EqualTo(indexEntries[0].Abbreviation));
        Assert.That(entry1.Name, Is.EqualTo(indexEntries[0].Name));
        Assert.That(entry1.Phraseology, Is.EqualTo(indexEntries[0].Phraseology));
        Assert.That(entry1.Definition, Is.EqualTo(indexEntries[0].Definition));
        Assert.That(entry1.Note, Is.EqualTo(indexEntries[0].Note));
        Assert.That(entry1.Context, Is.EqualTo(indexEntries[0].Context));
        Assert.That(entry1.Metadata, Is.EqualTo(indexEntries[0].Metadata));
        Assert.That(entry1.Country, Is.EqualTo(indexEntries[0].Country));
    }

    [Test]
    public async Task ImportEntries_AddsEntryToIndexForNullValues()
    {
        var indexEntry = indexEntries.First();
        indexEntry.CreatedBy = null;
        indexEntry.ModifiedBy = null;
        indexEntry.Comment = null;
        indexEntry.Terminus = null;
        indexEntry.Abbreviation = null;
        indexEntry.Name = null;
        indexEntry.Phraseology = null;
        indexEntry.Definition = null;
        indexEntry.Note = null;
        indexEntry.Context = null;
        indexEntry.Metadata = null;
        indexEntry.Country = null;

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var query = new MatchAllDocsQuery();
        var searcher = gateway.SearcherManager.Acquire();

        var docs = searcher.Search(query, 100000);

        Assert.That(docs, Is.Not.Null);
        Assert.That(docs.TotalHits, Is.EqualTo(4000));

        var entry1 = GetIndexEntry(searcher.Doc(docs.ScoreDocs[0].Doc));

        Assert.That(entry1.EntryId, Is.EqualTo(indexEntry.EntryId));
        Assert.That(entry1.LanguageId, Is.EqualTo(indexEntry.LanguageId));
        Assert.That(entry1.SequenceId, Is.EqualTo(indexEntry.SequenceId));
        Assert.That(entry1.Abbreviation, Is.EqualTo(null));
        Assert.That(entry1.Name, Is.EqualTo(null));
        Assert.That(entry1.Terminus, Is.EqualTo(null));
        Assert.That(entry1.Phraseology, Is.EqualTo(null));
        Assert.That(entry1.Context, Is.EqualTo(null));
        Assert.That(entry1.Country, Is.EqualTo(null));
        Assert.That(entry1.Definition, Is.EqualTo(null));
        Assert.That(entry1.Note, Is.EqualTo(null));
        Assert.That(entry1.Metadata, Is.EqualTo(null));
    }

    [Test]
    public async Task ImportEntries_DoesNotThrowExceptionIfSingleEntryGeneratesException()
    {
        var indexEntriesLocal = new[] { indexEntries[0], null, indexEntries[1], default, indexEntries[2], new IndexEntry(), indexEntries[3] }.ToAsyncEnumerable();
        await searchRepository.ImportEntries(indexEntriesLocal, CancellationToken.None);

        var query = new MatchAllDocsQuery();
        var searcher = gateway.SearcherManager.Acquire();

        var docs = searcher.Search(query, 100000);

        Assert.That(docs, Is.Not.Null);
        Assert.That(docs.TotalHits, Is.EqualTo(5));
    }

    [Test]
    public async Task ImportEntries_AddsMultipleEntriesToIndex()
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var query = new MatchAllDocsQuery();
        var searcher = gateway.SearcherManager.Acquire();

        var docs = searcher.Search(query, 100000);

        Assert.That(docs, Is.Not.Null);
        Assert.That(docs.TotalHits, Is.EqualTo(4000));
    }

    [Test]
    public void ImportEntries_AddsEntriesToIndex()
    {
        searchRepository.AddEntries(indexEntries);

        var query = new MatchAllDocsQuery();
        var searcher = gateway.SearcherManager.Acquire();

        var docs = searcher.Search(query, 100000);

        Assert.That(docs, Is.Not.Null);
        Assert.That(docs.TotalHits, Is.EqualTo(4000));
    }

    [Test]
    public async Task Search_ReturnsEntries()
    {
        var searchPhrase = "XSS";

        GenerateFilterParameterAndFields(searchPhrase);
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.GreaterThan(100));

        foreach (var hit in result.EntryIds)
        {
            var id = hit;
            var dataEntries = indexEntries.Where(d => d.EntryId == id).ToList();
            var hasMatchingEntry = dataEntries.Any(e =>
                e.Terminus.Contains(searchPhrase) ||
                e.Name.Contains(searchPhrase) ||
                e.Abbreviation.Contains(searchPhrase) ||
                e.Phraseology.Contains(searchPhrase));

            Assert.That(dataEntries, Is.Not.Empty);
            Assert.That(hasMatchingEntry, Is.True);
        }
    }

    [Test]
    public async Task Search_WithLimit_ReturnsLimitedEntries()
    {
        GenerateFilterParameterAndFields("XSS");
        searchFilterParameter.PageSize = 10;

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.EqualTo(10));
    }

    [Test]
    public async Task Search_WithLanguageFilter_ReturnsFilteredEntries()
    {
        var searchPhrase = "JSON";
        GenerateFilterParameterAndFields(searchPhrase);
        searchFilterParameter.SourceLanguageIds = [7];
        searchFilterParameter.PageSize = 4000;

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);

        foreach (var hit in result.EntryIds)
        {
            var id = hit;
            var dataEntries = indexEntries.Where(d => d.EntryId == id && d.LanguageId == 7).ToList();
            var hasMatchingEntry = dataEntries.Any(e =>
                e.Terminus.Contains(searchPhrase) ||
                e.Name.Contains(searchPhrase) ||
                e.Abbreviation.Contains(searchPhrase) ||
                e.Phraseology.Contains(searchPhrase));

            Assert.That(dataEntries, Is.Not.Empty);
            Assert.That(hasMatchingEntry, Is.True);
        }
    }

    [Test]
    public async Task Search_WithMultipleLanguageFilters_ReturnsFilteredEntriesInAnyOfTheLanguagesProvided()
    {
        var searchPhrase = "JSON";
        GenerateFilterParameterAndFields(searchPhrase);
        var multipleSourceLanguageIds = new[] { 7, 6 };
        var singleSourceLanguageId = new[] { 7 };
        searchFilterParameter.PageSize = 4000;

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        searchFilterParameter.SourceLanguageIds = multipleSourceLanguageIds;
        var resultMultiple = searchRepository.Search(searchFilterParameter, searchFields);

        searchFilterParameter.SourceLanguageIds = singleSourceLanguageId;
        var resultSingle = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(resultMultiple, Is.Not.Null);
        Assert.That(resultMultiple.EntryIds, Is.Not.Empty);
        Assert.That(resultMultiple.TotalEntryCount, Is.GreaterThan(0));
        Assert.That(resultSingle.EntryIds, Is.Not.Empty);
        Assert.That(resultSingle.TotalEntryCount, Is.GreaterThan(0));
        Assert.That(resultMultiple.TotalEntryCount, Is.GreaterThan(resultSingle.TotalEntryCount));

        foreach (var hit in resultMultiple.EntryIds)
        {
            var id = hit;
            var dataEntries = indexEntries.Where(d => d.EntryId == id && multipleSourceLanguageIds.Contains(d.LanguageId)).ToList();
            var hasMatchingEntry = dataEntries.Any(e =>
                e.Terminus.Contains(searchPhrase) ||
                e.Name.Contains(searchPhrase) ||
                e.Abbreviation.Contains(searchPhrase) ||
                e.Phraseology.Contains(searchPhrase));

            Assert.That(dataEntries, Is.Not.Empty);
            Assert.That(hasMatchingEntry, Is.True);
        }
    }

    [TestCase(0, null)]
    [TestCase(0, new[] { 7 })]
    public async Task Recommend_ReturnsEntriesForTerminus(int entryIndex, int[] languageIds)
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var phrase = indexEntries[entryIndex].Terminus;
        var result = searchRepository.Recommend(phrase, languageIds).ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThanOrEqualTo(1));
    }

    [TestCase(0, null)]
    [TestCase(0, new[] { 6 })]
    public async Task Recommend_ReturnsEntriesForName(int entryIndex, int[] languageIds)
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);
        var phrase = indexEntries[entryIndex].Name;

        var result = searchRepository.Recommend(phrase, languageIds).ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThanOrEqualTo(1));
    }

    [TestCase(0, null)]
    [TestCase(0, new[] { 7 })]
    public async Task Recommend_ReturnsEntriesForPhraseology(int entryIndex, int[] languageIds)
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var phrase = indexEntries[entryIndex].Phraseology;
        var result = searchRepository.Recommend(phrase, languageIds).ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task Recommend_PerformsWildcardSearchAndSortsHitsByLengthThenByScore()
    {
        var data = TestHelpers.GetStaticIndexEntries();
        data.Add(new IndexEntry
        {
            EntryId = 11,
            SequenceId = 1,
            LanguageId = 8,
            OfficeId = 3,
            CollectionId = 68,
            CollectionIsActive = true,
            RelatedCollections = [200],
            LenochClassifications = [200],
            Glossaries = [300],
            Status = Business.Domain.Models.Status.Validated,
            Reliability = Business.Domain.Models.Reliability.Approved,
            Terminus = "Unique Office3"
        });
        data.AddRange(indexEntries);
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        var results = searchRepository.Recommend("UNIQUE").ToList();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.EqualTo(10));

        Assert.That(results[0], Is.EqualTo("UNIQUE"));
        Assert.That(results[1], Is.EqualTo("UNIQUE Name"));
        Assert.That(results[2], Is.EqualTo("UNIQUEPARTIAL"));
        Assert.That(results[3], Is.EqualTo("Unique Office3"));
        Assert.That(results[4], Is.EqualTo("uNiQuE Office1"));
        Assert.That(results[5], Is.EqualTo("UNIQUE Terminus"));
        Assert.That(results[6], Is.EqualTo("UNIQUE Approved"));
        Assert.That(results[7], Is.EqualTo("UNIQUE Verified"));
        Assert.That(results[8], Is.EqualTo("UNIQUE Tentative"));
        Assert.That(results[9], Is.EqualTo("UNIQUE Validated"));
    }

    [Test]
    public async Task Recommend_WithActiveCollectionEntriesOnly_ShouldReturnFilteredResult()
    {
        var data = new List<IndexEntry>
        {
            new()
            {
                EntryId = 42,
                CollectionIsActive = true,
                Terminus = "Dummy Town"
            },
            new()
            {
                EntryId = 666,
                CollectionIsActive = false,
                Terminus = "Dummy Street"
            }
        };
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        var results = searchRepository.Recommend("Dummy", includeDeactivatedCollectionEntries: false).ToList();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results.Single(), Is.EqualTo("Dummy Town"));
    }

    [Test]
    public async Task Recommend_IncludeDeactivatedCollectionEntries_ShouldReturnResult()
    {
        var data = new List<IndexEntry>
        {
            new()
            {
                EntryId = 42,
                CollectionIsActive = true,
                Terminus = "Dummy Town"
            },
            new()
            {
                EntryId = 666,
                CollectionIsActive = false,
                Terminus = "Dummy Street"
            }
        };
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        var results = searchRepository.Recommend("Dummy", includeDeactivatedCollectionEntries: true).ToList();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results.First(), Is.EqualTo("Dummy Town"));
        Assert.That(results.Last(), Is.EqualTo("Dummy Street"));
    }

    [Test]
    public async Task Recommend_MultipleTermsAreUsedInConjunction()
    {
        var data = TestHelpers.GetStaticIndexEntries();
        data.AddRange(indexEntries);
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);
        var results = searchRepository.Recommend("Unique offi").ToList();
        var results2 = searchRepository.Recommend("UNIQUE te").ToList();

        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results[0], Is.EqualTo("uNiQuE Office1"));
        Assert.That(results[1], Is.EqualTo("uNiQuE Office3"));

        Assert.That(results2.Count, Is.EqualTo(2));
        Assert.That(results2[0], Is.EqualTo("UNIQUE Terminus"));
        Assert.That(results2[1], Is.EqualTo("UNIQUE Tentative"));
    }

    [Test]
    public async Task Recommend_DoesNotReturnDuplicates()
    {
        var data = TestHelpers.GetStaticIndexEntries();
        data.AddRange(indexEntries);
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        var results = searchRepository.Recommend("SUPERUNIQUE", [10]).ToList();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.EqualTo(1));

        Assert.That(results[0], Is.EqualTo("SUPERUNIQUE"));
    }

    [Test]
    public async Task Recommend_ReturnsEmptyResultForSearchesWithNullOrLessThenMinimumLength()
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var resultsNull = searchRepository.Recommend(null).ToList();
        var resultsBelowMin = searchRepository.Recommend("Com").ToList();

        Assert.That(resultsNull, Is.Empty);
        Assert.That(resultsBelowMin, Is.Empty);
    }

    [Test]
    public async Task Recommend_WithDeletedFlagFilter_ReturnsEntriesCorrectly()
    {
        const string searchPhrase = "UNIQUE";
        var data = indexEntries.ToArray();

        data[0].Name = searchPhrase;
        data[0].Deleted = true;

        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        var resultWithoutDeleted = searchRepository.Recommend(searchPhrase).ToList();
        var resultIncludingDeleted = searchRepository.Recommend(searchPhrase, null, true).ToList();

        Assert.That(resultWithoutDeleted, Is.Empty);
        Assert.That(resultIncludingDeleted, Is.Not.Empty);
        Assert.That(resultIncludingDeleted.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Recommend_PrioritizeValidatedEntries()
    {
        var data = TestHelpers.GetStaticIndexEntries();
        data.AddRange(indexEntries);
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        var results = searchRepository.Recommend("UNIQUE", [8]).ToList();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.EqualTo(2));

        Assert.That(results[0], Is.EqualTo("UNIQUE Validated"));
        Assert.That(results[1], Is.EqualTo("UNIQUE InProgress"));
    }

    [Test]
    public async Task Recommend_PrioritizeAchOfficeEntries()
    {
        var data = TestHelpers.GetStaticIndexEntries();
        data.AddRange(indexEntries);
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        var results = searchRepository.Recommend("UNIQUE", [7]).ToList();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results[0], Is.EqualTo("uNiQuE Office1"));
        Assert.That(results[1], Is.EqualTo("uNiQuE Office3"));
    }

    [Test]
    public async Task Recommend_PrioritizePreferredEntries()
    {
        var data = TestHelpers.GetStaticIndexEntries();
        data.AddRange(indexEntries);
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        var results = searchRepository.Recommend("PREFERRED").ToList();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.EqualTo(3));
        Assert.That(results[0], Is.EqualTo("PREFERRED 1"));
        Assert.That(results[1], Is.EqualTo("preferred 2"));
        Assert.That(results[2], Is.EqualTo("PrEfErReD 3"));
    }

    [Test]
    public async Task Recommend_PrioritizeEntriesByReliability()
    {
        var data = TestHelpers.GetStaticIndexEntries();

        foreach (var indexEntry in data)
        {
            indexEntry.Terminus = $"UNIQUE {(int)indexEntry.Reliability}";
        }

        data.AddRange(indexEntries);
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        var results = searchRepository.Recommend("UNIQUE", [6]).ToList();
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.EqualTo(5));
        Assert.That(results[0], Is.EqualTo("UNIQUE 4"));
        Assert.That(results[1], Is.EqualTo("UNIQUE 3"));
        Assert.That(results[2], Is.EqualTo("UNIQUE 2"));
        Assert.That(results[3], Is.EqualTo("UNIQUE 1"));
        Assert.That(results[4], Is.EqualTo("UNIQUE 0"));
    }

    [Test]
    public async Task Search_WithDeletedFlagFilter_ReturnsEntriesCorrectly()
    {
        const string searchPhrase = "UNIQUE";
        var data = indexEntries.ToArray();

        data[0].Abbreviation = searchPhrase;
        data[1].Abbreviation = searchPhrase;
        data[2].Abbreviation = searchPhrase;
        data[2].Deleted = true;
        data[3].Abbreviation = searchPhrase;
        data[3].Deleted = true;

        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        GenerateFilterParameterAndFields(searchPhrase);

        var resultWithoutDeleted = searchRepository.Search(searchFilterParameter, searchFields);
        var resultIncludingDeleted = searchRepository.Search(searchFilterParameter, searchFields, true);

        Assert.That(resultWithoutDeleted, Is.Not.Null);
        Assert.That(resultWithoutDeleted.TotalEntryCount, Is.EqualTo(2));
        Assert.That(resultWithoutDeleted.EntryIds.Count(), Is.EqualTo(2));
        Assert.That(resultIncludingDeleted, Is.Not.Null);
        Assert.That(resultIncludingDeleted.TotalEntryCount, Is.EqualTo(4));
        Assert.That(resultIncludingDeleted.EntryIds.Count(), Is.EqualTo(4));
    }

    [Test]
    public async Task Search_WithDeletedSearchButWithoutPriority_ReturnsEntriesCorrectly()
    {
        var data = indexEntries.ToArray();

        data[1].Deleted = true;
        data[2].Deleted = true;
        data[3].Deleted = true;

        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        GenerateFilterParameterAndFields("");

        searchFilterParameter.Deleted = true;
        searchFilterParameter.DeletedPriority = false;

        var resultIncludingDeleted = searchRepository.Search(searchFilterParameter, searchFields, true);

        Assert.That(resultIncludingDeleted, Is.Not.Null);
        Assert.That(resultIncludingDeleted.TotalEntryCount, Is.EqualTo(3));
        Assert.That(resultIncludingDeleted.EntryIds.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task Search_WithDeletedAndPrioritySearch_ReturnsEntriesCorrectly()
    {
        var data = indexEntries.ToArray();

        data[1].Deleted = true;
        data[2].Deleted = true;
        data[3].Deleted = true;

        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        GenerateFilterParameterAndFields("");

        searchFilterParameter.Deleted = true;
        searchFilterParameter.DeletedPriority = true;

        var resultIncludingDeleted = searchRepository.Search(searchFilterParameter, searchFields);
        Assert.That(resultIncludingDeleted, Is.Not.Null);
        Assert.That(resultIncludingDeleted.TotalEntryCount, Is.EqualTo(searchFilterParameter.PageSize));
        Assert.That(resultIncludingDeleted.EntryIds.Count(), Is.EqualTo(searchFilterParameter.PageSize));
    }

    [Test]
    public async Task Search_WithCommentStatus_ReturnsEntriesCorrectly()
    {
        var data = TestHelpers.GenerateIndexEntries(1000).ToList();

        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        searchFilterParameter = new SearchFilterParameter
        {
            Phrase = string.Empty,
            PageIndex = 1,
            PageSize = 1000,
            CommentStatus = [CommentStatus.Filled]
        };

        searchFields = new SearchFields
        {
            Name = true,
            Phraseology = true,
            Abbreviation = true,
        };

        var resultsTrue = searchRepository.Search(searchFilterParameter, searchFields);

        searchFilterParameter = new SearchFilterParameter
        {
            Phrase = string.Empty,
            PageIndex = 1,
            PageSize = 1000,
            CommentStatus = [CommentStatus.Empty]
        };

        searchFields = new SearchFields
        {
            Name = true,
            Phraseology = true,
            Abbreviation = true,
        };

        var resultsFalse = searchRepository.Search(searchFilterParameter, searchFields);

        searchFilterParameter = new SearchFilterParameter
        {
            Phrase = string.Empty,
            PageIndex = 1,
            PageSize = 1000,
            CommentStatus = [CommentStatus.Filled, CommentStatus.Empty]
        };

        searchFields = new SearchFields
        {
            Name = true,
            Phraseology = true,
            Abbreviation = true,
        };

        var resultsTrueAndFalse = searchRepository.Search(searchFilterParameter, searchFields);

        var expectedTrue = data.Count(d => !string.IsNullOrWhiteSpace(d.Comment));
        Assert.That(resultsTrue, Is.Not.Null);
        Assert.That(resultsTrue.TotalEntryCount, Is.EqualTo(expectedTrue));
        Assert.That(resultsTrue.EntryIds.Count(), Is.EqualTo(expectedTrue));

        var expectedFalse = data.Count(d => string.IsNullOrWhiteSpace(d.Comment));
        Assert.That(resultsFalse, Is.Not.Null);
        Assert.That(resultsFalse.TotalEntryCount, Is.EqualTo(expectedFalse));
        Assert.That(resultsFalse.EntryIds.Count(), Is.EqualTo(expectedFalse));

        var expectedTrueAndFalse = data.Count;
        Assert.That(resultsTrueAndFalse, Is.Not.Null);
        Assert.That(resultsTrueAndFalse.TotalEntryCount, Is.EqualTo(expectedTrueAndFalse));
        Assert.That(resultsTrueAndFalse.EntryIds.Count(), Is.EqualTo(expectedTrueAndFalse));
    }

    [Test]
    public async Task Search_ForFieldComment_ReturnsEntriesCorrectly()
    {
        var data = TestHelpers.GetStaticIndexEntries().ToArray();

        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        searchFilterParameter = new SearchFilterParameter
        {
            Phrase = "excellent contribution",
            PageIndex = 1,
            PageSize = 1000
        };

        searchFields = new SearchFields
        {
            Name = true,
            Phraseology = true,
            Abbreviation = true,
        };

        var resultsWithoutCommentField = searchRepository.Search(searchFilterParameter, searchFields);

        searchFilterParameter = new SearchFilterParameter
        {
            Phrase = "excellent contribution",
            PageIndex = 1,
            PageSize = 1000
        };

        searchFields = new SearchFields
        {
            Name = true,
            Phraseology = true,
            Abbreviation = true,
            Comment = true
        };

        var resultsWithCommentField = searchRepository.Search(searchFilterParameter, searchFields);

        searchFilterParameter = new SearchFilterParameter
        {
            Phrase = "excellent contribution",
            PageIndex = 1,
            PageSize = 1000,
            CommentStatus = [CommentStatus.Empty]
        };

        searchFields = new SearchFields
        {
            Name = true,
            Phraseology = true,
            Abbreviation = true,
            Comment = true
        };

        var resultsForImpossibleSearch = searchRepository.Search(searchFilterParameter, searchFields);

        var expectedWithoutComment = 0;
        Assert.That(resultsWithoutCommentField, Is.Not.Null);
        Assert.That(resultsWithoutCommentField.TotalEntryCount, Is.EqualTo(expectedWithoutComment));
        Assert.That(resultsWithoutCommentField.EntryIds.Count(), Is.EqualTo(expectedWithoutComment));

        var expectedWithComment = 1;
        Assert.That(resultsWithCommentField, Is.Not.Null);
        Assert.That(resultsWithCommentField.TotalEntryCount, Is.EqualTo(expectedWithComment));
        Assert.That(resultsWithCommentField.EntryIds.Count(), Is.EqualTo(expectedWithComment));

        var expectedImpossibleSearch = 0;
        Assert.That(resultsForImpossibleSearch, Is.Not.Null);
        Assert.That(resultsForImpossibleSearch.TotalEntryCount, Is.EqualTo(expectedImpossibleSearch));
        Assert.That(resultsForImpossibleSearch.EntryIds.Count(), Is.EqualTo(expectedImpossibleSearch));
    }

    [Test]
    public async Task DeletesEntriesCorrectly()
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var matchId1Query = new TermQuery(new Term(nameof(IndexEntry.EntryId), indexEntries[0].EntryId.ToString()));
        var matchId2Query = new TermQuery(new Term(nameof(IndexEntry.EntryId), indexEntries[1].EntryId.ToString()));
        var matchId3Query = new TermQuery(new Term(nameof(IndexEntry.EntryId), indexEntries[2].EntryId.ToString()));
        var totalQuery = new MatchAllDocsQuery();

        var searcher = gateway.SearcherManager.Acquire();

        var matchId1Docs = searcher.Search(matchId1Query, 100000);
        var matchId2Docs = searcher.Search(matchId2Query, 100000);
        var matchId3Docs = searcher.Search(matchId3Query, 100000);
        var total = searcher.Search(totalQuery, 100000);

        Assert.That(matchId1Docs.TotalHits, Is.EqualTo(4));
        Assert.That(matchId2Docs.TotalHits, Is.EqualTo(4));
        Assert.That(matchId3Docs.TotalHits, Is.EqualTo(4));
        Assert.That(total.TotalHits, Is.EqualTo(4000));

        searchRepository.DeleteEntries([indexEntries[0].EntryId, indexEntries[2].EntryId]);
        searcher = gateway.SearcherManager.Acquire();

        var matchId1AfterDeleteDocs = searcher.Search(matchId1Query, 100000);
        var matchId2AfterDeleteDocs = searcher.Search(matchId2Query, 100000);
        var matchId3AfterDeleteDocs = searcher.Search(matchId3Query, 100000);

        Assert.That(matchId1AfterDeleteDocs.TotalHits, Is.EqualTo(0));
        Assert.That(matchId2AfterDeleteDocs.TotalHits, Is.EqualTo(4));
        Assert.That(matchId3AfterDeleteDocs.TotalHits, Is.EqualTo(0));

        var totalAfterDelete = searcher.Search(new MatchAllDocsQuery(), 100000);
        Assert.That(totalAfterDelete.TotalHits, Is.EqualTo(3992));
    }

    [Test]
    public async Task UpdatesEntriesCorrectly()
    {
        indexEntries[0].LanguageId = 1;
        indexEntries[1].EntryId = indexEntries[0].EntryId;
        indexEntries[1].LanguageId = 2;
        indexEntries[2].EntryId = indexEntries[0].EntryId;
        indexEntries[2].LanguageId = 3;
        await searchRepository.ImportEntries(new[] { indexEntries[0], indexEntries[1], indexEntries[2], indexEntries[3], indexEntries[4], indexEntries[5] }.ToAsyncEnumerable(), CancellationToken.None);

        var matchQuery = new TermQuery(new Term(nameof(IndexEntry.EntryId), indexEntries[0].EntryId.ToString()));
        var searcher = gateway.SearcherManager.Acquire();
        var docs = searcher.Search(matchQuery, 100000);

        Assert.That(docs.TotalHits, Is.EqualTo(3));
        var oldEntry1 = GetIndexEntry(searcher.Doc(docs.ScoreDocs[0].Doc));

        indexEntries[0].Name = "NEW Name";
        indexEntries[0].Terminus = "NEW Terminus";
        indexEntries[0].Phraseology = "NEW Phraseology";
        indexEntries[0].Abbreviation = "NEW Abbreviation";

        searchRepository.UpdateEntries([indexEntries[0], indexEntries[1], indexEntries[2]]);
        searcher = gateway.SearcherManager.Acquire();

        var afterUpdateDocs = searcher.Search(matchQuery, 100000);
        Assert.That(afterUpdateDocs.TotalHits, Is.EqualTo(3));

        var entries = afterUpdateDocs.ScoreDocs.Select(d => GetIndexEntry(searcher.Doc(d.Doc))).ToList();

        var entry1 = entries.Single(e => e.EntryId == indexEntries[0].EntryId && e.LanguageId == indexEntries[0].LanguageId && e.SequenceId == indexEntries[0].SequenceId);
        var entry2 = entries.Single(e => e.EntryId == indexEntries[1].EntryId && e.LanguageId == indexEntries[1].LanguageId && e.SequenceId == indexEntries[1].SequenceId);
        var entry3 = entries.Single(e => e.EntryId == indexEntries[2].EntryId && e.LanguageId == indexEntries[2].LanguageId && e.SequenceId == indexEntries[2].SequenceId);

        Assert.That(entry1.Name, Is.Not.EqualTo(oldEntry1.Name));
        Assert.That(entry1.Terminus, Is.Not.EqualTo(oldEntry1.Terminus));
        Assert.That(entry1.Phraseology, Is.Not.EqualTo(oldEntry1.Phraseology));
        Assert.That(entry1.Abbreviation, Is.Not.EqualTo(oldEntry1.Abbreviation));

        AssertIndexEntry(entry1, indexEntries[0]);
        AssertIndexEntry(entry2, indexEntries[1]);
        AssertIndexEntry(entry3, indexEntries[2]);
    }

    [Test]
    public async Task UpdateCanUpdateMultipleIndexEntriesWithDifferentEntryIds()
    {
        indexEntries[0].LanguageId = 1;
        indexEntries[1].EntryId = indexEntries[0].EntryId;
        indexEntries[1].LanguageId = 2;
        indexEntries[2].EntryId = indexEntries[0].EntryId;
        indexEntries[2].LanguageId = 3;
        await searchRepository.ImportEntries(new[] { indexEntries[0], indexEntries[1], indexEntries[2], indexEntries[3], indexEntries[4], indexEntries[5] }.ToAsyncEnumerable(), CancellationToken.None);

        var matchQuery = new BooleanQuery
        {
            { new TermQuery(new Term(nameof(IndexEntry.EntryId), indexEntries[0].EntryId.ToString())), Occur.SHOULD },
            { new TermQuery(new Term(nameof(IndexEntry.EntryId), indexEntries[3].EntryId.ToString())), Occur.SHOULD }
        };
        var searcher = gateway.SearcherManager.Acquire();
        var docs = searcher.Search(matchQuery, 100000);

        Assert.That(docs.TotalHits, Is.EqualTo(4));
        var oldEntry1 = GetIndexEntry(searcher.Doc(docs.ScoreDocs[0].Doc));
        var oldEntry4 = GetIndexEntry(searcher.Doc(docs.ScoreDocs[3].Doc));

        indexEntries[0].Name = "NEW Name";
        indexEntries[0].Terminus = "NEW Terminus";
        indexEntries[0].Phraseology = "NEW Phraseology";
        indexEntries[0].Abbreviation = "NEW Abbreviation";
        indexEntries[3].Name = "NEW Name";
        indexEntries[3].Terminus = "NEW Terminus";
        indexEntries[3].Phraseology = "NEW Phraseology";
        indexEntries[3].Abbreviation = "NEW Abbreviation";

        searchRepository.UpdateEntries([indexEntries[0], indexEntries[1], indexEntries[2], indexEntries[3]]);
        searcher = gateway.SearcherManager.Acquire();

        var afterUpdateDocs = searcher.Search(matchQuery, 100000);
        Assert.That(afterUpdateDocs.TotalHits, Is.EqualTo(4));

        var entries = afterUpdateDocs.ScoreDocs.Select(d => GetIndexEntry(searcher.Doc(d.Doc))).ToList();

        var entry1 = entries.Single(e => e.EntryId == indexEntries[0].EntryId && e.LanguageId == indexEntries[0].LanguageId && e.SequenceId == indexEntries[0].SequenceId);
        var entry2 = entries.Single(e => e.EntryId == indexEntries[1].EntryId && e.LanguageId == indexEntries[1].LanguageId && e.SequenceId == indexEntries[1].SequenceId);
        var entry3 = entries.Single(e => e.EntryId == indexEntries[2].EntryId && e.LanguageId == indexEntries[2].LanguageId && e.SequenceId == indexEntries[2].SequenceId);
        var entry4 = entries.Single(e => e.EntryId == indexEntries[3].EntryId && e.LanguageId == indexEntries[3].LanguageId && e.SequenceId == indexEntries[3].SequenceId);

        Assert.That(entry1.Name, Is.Not.EqualTo(oldEntry1.Name));
        Assert.That(entry1.Terminus, Is.Not.EqualTo(oldEntry1.Terminus));
        Assert.That(entry1.Phraseology, Is.Not.EqualTo(oldEntry1.Phraseology));
        Assert.That(entry1.Abbreviation, Is.Not.EqualTo(oldEntry1.Abbreviation));
        Assert.That(entry4.Name, Is.Not.EqualTo(oldEntry4.Name));
        Assert.That(entry4.Terminus, Is.Not.EqualTo(oldEntry4.Terminus));
        Assert.That(entry4.Phraseology, Is.Not.EqualTo(oldEntry4.Phraseology));
        Assert.That(entry4.Abbreviation, Is.Not.EqualTo(oldEntry4.Abbreviation));


        AssertIndexEntry(entry1, indexEntries[0]);
        AssertIndexEntry(entry2, indexEntries[1]);
        AssertIndexEntry(entry3, indexEntries[2]);
    }

    [Test]
    public async Task UpdatesEntriesShouldRemoveLanguagesCorrectly()
    {
        indexEntries[0].LanguageId = 1;
        indexEntries[1].EntryId = indexEntries[0].EntryId;
        indexEntries[1].LanguageId = 2;
        indexEntries[2].EntryId = indexEntries[0].EntryId;
        indexEntries[2].LanguageId = 3;
        await searchRepository.ImportEntries(new[] { indexEntries[0], indexEntries[1], indexEntries[2], indexEntries[3] }.ToAsyncEnumerable(), CancellationToken.None);

        var matchQuery = new TermQuery(new Term(nameof(IndexEntry.EntryId), indexEntries[0].EntryId.ToString()));
        var searcher = gateway.SearcherManager.Acquire();
        var docs = searcher.Search(matchQuery, 100000);

        Assert.That(docs.TotalHits, Is.EqualTo(3));

        searchRepository.UpdateEntries([indexEntries[0]]);
        searcher = gateway.SearcherManager.Acquire();

        var afterUpdateDocs = searcher.Search(matchQuery, 100000);
        Assert.That(afterUpdateDocs.TotalHits, Is.EqualTo(1));

        var entries = afterUpdateDocs.ScoreDocs.Select(d => GetIndexEntry(searcher.Doc(d.Doc))).ToList();

        var entry1 = entries.Single(e => e.EntryId == indexEntries[0].EntryId && e.LanguageId == indexEntries[0].LanguageId && e.SequenceId == indexEntries[0].SequenceId);

        AssertIndexEntry(entry1, indexEntries[0]);
    }

    [Test]
    public void UpdatesEntriesShouldThrowWithEmpty()
    {
        var indexEntryList = Enumerable.Empty<IndexEntry>();
        Assert.That(() => searchRepository.UpdateEntries(indexEntryList), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public async Task Search_WithFilterAbbreviationReturnsResult()
    {
        const string phrase = "Abbreviation";
        var data = TestHelpers.GetStaticIndexEntries();
        data.Add(new IndexEntry
        {
            EntryId = 11,
            CollectionIsActive = true,
            Abbreviation = phrase
        });
        data.Add(new IndexEntry
        {
            EntryId = 12,
            CollectionIsActive = true,
            Terminus = phrase
        });
        data.AddRange(indexEntries);
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        searchFilterParameter = new SearchFilterParameter
        {
            Phrase = phrase,
            PageIndex = 1,
            PageSize = 1000,
        };

        var fields = new SearchFields
        {
            Terminus = false,
            Name = false,
            Abbreviation = true,
            Phraseology = false,
            Metadata = false
        };
        var result = searchRepository.Search(searchFilterParameter, fields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.TotalEntryCount, Is.EqualTo(1));
        Assert.That(result.EntryIds.First(), Is.EqualTo(11));
    }

    [Test]
    public async Task Search_WithActiveCollectionEntriesOnly_ShouldReturnFilteredResult()
    {
        var data = new List<IndexEntry>
        {
            new()
            {
                EntryId = 42,
                CollectionIsActive = true,
                Terminus = "A"
            },
            new()
            {
                EntryId = 666,
                CollectionIsActive = false,
                Terminus = "A"
            }
        };
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);
        searchFilterParameter = new SearchFilterParameter
        {
            Phrase = "A",
            PageIndex = 1,
            PageSize = 1000
        };

        var result = searchRepository.Search(searchFilterParameter, new SearchFields(), includeDeactivatedCollectionEntries: false);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.TotalEntryCount, Is.EqualTo(1));
        Assert.That(result.EntryIds.Single(), Is.EqualTo(42));
    }

    [Test]
    public async Task Search_WithDeactivatedCollectionEntries_ShouldReturnResult()
    {
        var data = new List<IndexEntry>
        {
            new()
            {
                EntryId = 42,
                CollectionIsActive = true,
                Terminus = "A"
            },
            new()
            {
                EntryId = 666,
                CollectionIsActive = false,
                Terminus = "A"
            }
        };
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);
        searchFilterParameter = new SearchFilterParameter
        {
            Phrase = "A",
            PageIndex = 1,
            PageSize = 1000
        };

        var result = searchRepository.Search(searchFilterParameter, new SearchFields(), includeDeactivatedCollectionEntries: true);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.TotalEntryCount, Is.EqualTo(2));
        Assert.That(result.EntryIds.First(), Is.EqualTo(42));
        Assert.That(result.EntryIds.Last(), Is.EqualTo(666));
    }

    [Test]
    public async Task Search_WithMutationDateRangeFindsHits()
    {
        var searchPhrase = "CSS";
        GenerateFilterParameterAndFields(searchPhrase);
        searchFilterParameter.ModifiedFrom = DateTime.UtcNow.AddDays(-15);
        searchFilterParameter.ModifiedTo = DateTime.UtcNow;

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);
        Assert.That(result.EntryIds.Count, Is.LessThan(244));

        foreach (var hit in result.EntryIds)
        {
            var id = hit;
            var dataEntries = indexEntries.Where(d => d.EntryId == id).ToList();
            var hasMatchingEntry = dataEntries.Any(e =>
                e.Terminus.Contains(searchPhrase) ||
                e.Name.Contains(searchPhrase) ||
                e.Abbreviation.Contains(searchPhrase) ||
                e.Phraseology.Contains(searchPhrase));

            Assert.That(dataEntries, Is.Not.Empty);
            Assert.That(hasMatchingEntry, Is.True);
        }
    }

    [Test]
    public async Task Search_WithMutationDateRangeAndPriorityFindsMoreHits()
    {
        var searchPhrase = "CSS";
        GenerateFilterParameterAndFields(searchPhrase);
        searchFilterParameter.ModifiedFrom = DateTime.UtcNow.AddDays(-390);
        searchFilterParameter.ModifiedTo = DateTime.UtcNow.AddDays(-370);

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);

        var entryCounter = result.EntryIds.Count();

        foreach (var hit in result.EntryIds)
        {
            var id = hit;
            var dataEntries = indexEntries.Where(d => d.EntryId == id).ToList();
            var hasMatchingEntry = dataEntries.Any(e =>
                e.Terminus.Contains(searchPhrase) ||
                e.Name.Contains(searchPhrase) ||
                e.Abbreviation.Contains(searchPhrase) ||
                e.Phraseology.Contains(searchPhrase));

            Assert.That(dataEntries, Is.Not.Empty);
            Assert.That(hasMatchingEntry, Is.True);
        }

        searchFilterParameter.ModifiedPeriodPriority = true;

        result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.GreaterThan(100));
        Assert.That(result.EntryIds.Count, Is.GreaterThan(entryCounter));

        var entryId = result.EntryIds.First();
        var dataEntriesModified = indexEntries.Where(d => d.EntryId == entryId).ToList();

        var hasMatchingModifiedEntry = dataEntriesModified.Any(e =>
            e.Modified > DateTime.UtcNow.AddDays(-382) && e.Modified > DateTime.UtcNow.AddDays(-378));

        Assert.That(dataEntriesModified, Is.Not.Empty);
        Assert.That(hasMatchingModifiedEntry, Is.True);
    }

    [Test]
    public async Task Search_WithPastMutationDateDoesNotFindHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.ModifiedFrom = DateTime.UtcNow.AddDays(-600);
        searchFilterParameter.ModifiedTo = DateTime.UtcNow.AddDays(-700);

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result.EntryIds, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithMutationDateRangeAndExcludeFindsNoHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.ModifiedFrom = DateTime.UtcNow.AddDays(-1000);
        searchFilterParameter.ModifiedTo = DateTime.UtcNow;

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var includedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(includedResult, Is.Not.Null);
        Assert.That(includedResult.EntryIds, Is.Not.Empty);

        searchFilterParameter.ModifiedPeriodExclude = true;
        var excludedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(excludedResult, Is.Not.Null);
        Assert.That(excludedResult.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithDomainDoesFindHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.LenochClassifications = [200];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.GreaterThan(100));
    }

    [Test]
    public async Task Search_WithDomainDoesNotFindHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.LenochClassifications = [400, 500, 600];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithExistingCollectionFindsHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.Collections = [68, 10301, 10299, 10297, 10292];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.GreaterThan(100));
    }

    [Test]
    public async Task Search_WithExistingCollectionAndPriorityFindsMoreHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.Collections = [68];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.AtLeast(1));

        var entries = result.EntryIds.Count();

        searchFilterParameter.CollectionsPriority = true;

        result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.AtLeast(entries + 1));
        Assert.That(result.EntryIds.Count, Is.GreaterThan(100));
    }

    [Test]
    public async Task Search_WithNonExistingCollectionFindsNoHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.Collections = [645, 343, 18890];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithExistingCollectionAndExcludeFindsNoHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.Collections = [68, 10301, 10299, 10297, 10292];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var includedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(includedResult, Is.Not.Null);
        Assert.That(includedResult.EntryIds, Is.Not.Empty);

        searchFilterParameter.CollectionsExclude = true;
        var excludedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(excludedResult, Is.Not.Null);
        Assert.That(excludedResult.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithExistingRelatedCollectionsFindsHits()
    {
        GenerateFilterParameterAndFields(string.Empty);
        searchFilterParameter.RelatedCollections = [200, 300];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.GreaterThan(100));
    }

    [Test]
    public async Task Search_WithNonExistingRelatedCollectionsFindsNoHits()
    {
        GenerateFilterParameterAndFields(string.Empty);
        searchFilterParameter.RelatedCollections = [201, 301];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithExistingGlossaryFindsHits()
    {
        GenerateFilterParameterAndFields(string.Empty);
        searchFilterParameter.Glossaries = [500, 600];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.GreaterThan(100));
    }

    [Test]
    public async Task Search_WithNonExistingGlossaryFindsNoHits()
    {
        GenerateFilterParameterAndFields(string.Empty);
        searchFilterParameter.Glossaries = [645, 343, 18890];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithExistingGlossaryAndExcludeFindsNoHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.Glossaries = [500];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var includedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(includedResult, Is.Not.Null);
        Assert.That(includedResult.EntryIds, Is.Not.Empty);

        searchFilterParameter.GlossariesExclude = true;
        var excludedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(excludedResult, Is.Not.Null);
        Assert.That(excludedResult.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithExistingCreatorFindsHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.CreatedBy = "test@test.ch";

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);
    }

    [Test]
    public async Task Serach_WithExistingCreatorAndPriorityFindsMoreHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.CreatedBy = "test@test.ch";

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);

        var entries = result.EntryIds.Count();

        searchFilterParameter.CreatedByPriority = true;

        result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.AtLeast(entries + 1));
        Assert.That(result.EntryIds.Count, Is.GreaterThan(100));
    }

    [Test]
    public async Task Search_WithNonExistingCreatorFindsNoHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.CreatedBy = "miro@test.ch";

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithExistingCreatorAndExcludeFindsNoHits()
    {
        GenerateFilterParameterAndFields(string.Empty);
        foreach (var entry in indexEntries)
        {
            entry.CreatedBy = "all@test.ch";
        }
        searchFilterParameter.CreatedBy = "all@test.ch";

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var includedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(includedResult, Is.Not.Null);
        Assert.That(includedResult.EntryIds, Is.Not.Empty);

        searchFilterParameter.CreatedByExclude = true;
        var excludedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(excludedResult, Is.Not.Null);
        Assert.That(excludedResult.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithExistingModifierFindsHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.ModifiedBy = "michael@test.ch";

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);
    }

    [Test]
    public async Task Search_WithExistingModifierAndPriorityFindsMoreHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.ModifiedBy = "michael@test.ch";

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);

        var entries = result.EntryIds.Count();

        searchFilterParameter.ModifiedByPriority = true;

        result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.AtLeast(entries + 1));
        Assert.That(result.EntryIds, Is.Not.Empty);
    }

    [Test]
    public async Task Search_WithExistingModifierAndWrongPriorityFieldsFindsNotMoreHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.ModifiedBy = "michael@test.ch";

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);

        var entries = result.EntryIds.Count();

        searchFilterParameter.CreatedByPriority = true;

        result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.EqualTo(entries));
    }

    [Test]
    public async Task Search_WithNonExistingModifierFindsNoHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.ModifiedBy = "louis@test.ch";

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithExistingModifierAndExcludeFindsNoHits()
    {
        GenerateFilterParameterAndFields(string.Empty);
        foreach (var entry in indexEntries)
        {
            entry.ModifiedBy = "all@test.ch";
        }
        searchFilterParameter.ModifiedBy = "all@test.ch";

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var includedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(includedResult, Is.Not.Null);
        Assert.That(includedResult.EntryIds, Is.Not.Empty);

        searchFilterParameter.ModifiedByExclude = true;
        var excludedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(excludedResult, Is.Not.Null);
        Assert.That(excludedResult.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithExistingAdminInfoFindsHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.AdminInfos = [1];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);
    }

    [Test]
    public async Task Search_WithNonExistingAdminInfoButPriorityFindsHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.AdminInfos = [666];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Empty);

        searchFilterParameter.AdminInfosPriority = true;

        result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.GreaterThan(100));
    }

    [Test]
    public async Task Search_WithNonExistingAdminInfoFindsNoHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.AdminInfos = [666];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result.EntryIds, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithExistingAdminInfoAndExcludeFindsNoHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.AdminInfos = [1];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var includedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(includedResult, Is.Not.Null);
        Assert.That(includedResult.EntryIds, Is.Not.Empty);

        searchFilterParameter.AdminInfosPriority = true;
        searchFilterParameter.AdminInfosExclude = true;
        var excludedResult = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(excludedResult, Is.Not.Null);
        Assert.That(excludedResult.EntryIds, Is.Empty);
    }

    [Test]
    public async Task Search_WithStatusValidatedFindsHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.Status = [Status.Validated];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);
    }

    [Test]
    public async Task Search_WithStatusValidatedAndPriorityFindsAllHits()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.Status = [Status.Validated];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);

        searchFilterParameter.StatusPriority = true;

        result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.GreaterThan(100));
    }

    [Test]
    public async Task Search_WithStatusValidatedAndInProgressMatchesTotalNumberOfEntries()
    {
        GenerateFilterParameterAndFields("CSS");
        searchFilterParameter.Status = [Status.Validated, Status.InProgress];

        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Not.Empty);

        Assert.That(result.EntryIds.Count, Is.GreaterThan(100));
    }

    [Test]
    public async Task Search_MultipleTerms_AreUsedInConjunction()
    {
        var data = TestHelpers.GetStaticIndexEntries();

        foreach (var indexEntry in data)
        {
            indexEntry.Terminus = $"term inus {indexEntries[0].Abbreviation}";
        }

        data.AddRange(indexEntries);
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        GenerateFilterParameterAndFields("TeRm INUS");
        var result = searchRepository.Search(searchFilterParameter, searchFields);

        GenerateFilterParameterAndFields($"TERM {indexEntries[0].Abbreviation}");
        var result2 = searchRepository.Search(searchFilterParameter, searchFields);

        GenerateFilterParameterAndFields(indexEntries[0].Abbreviation);
        var moreResults = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result.TotalEntryCount, Is.EqualTo(8));
        Assert.That(result2.TotalEntryCount, Is.EqualTo(result.TotalEntryCount));
        Assert.That(result.TotalEntryCount, Is.LessThan(moreResults.TotalEntryCount));
    }

    [Test]
    public async Task Search_MultipleTerms_PhraseHitsArePrioritizesOverConjunctionHits()
    {
        var data = TestHelpers.GetStaticIndexEntries();
        data[0].Terminus = "UNIQUE And Very Partial Extending Terminus";
        data[3].Terminus = "UNIQUE Super Terminus";
        data[4].Terminus = "UNIQUE Terminus";

        data.AddRange(indexEntries);
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        GenerateFilterParameterAndFields("UNIQUE Terminus");
        var result = searchRepository.Search(searchFilterParameter, searchFields);

        var ids = result.EntryIds.ToList();
        Assert.That(result.TotalEntryCount, Is.EqualTo(3));
        Assert.That(ids[0], Is.EqualTo(3));
        Assert.That(ids[1], Is.EqualTo(2));
        Assert.That(ids[2], Is.EqualTo(1));
    }

    [TestCaseSource(nameof(subjectAndDomainFilterTestCases))]
    public async Task Search_CanFilterSubjectsAndDomains(IEnumerable<string> subjectDomainIds, int expectedHits)
    {
        var data = TestHelpers.GetStaticIndexEntries();
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        GenerateFilterParameterAndFields("");
        searchFilterParameter.SubjectDomains = subjectDomainIds.ToArray();

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        var ids = result.EntryIds.ToList();
        Assert.That(result.TotalEntryCount, Is.EqualTo(expectedHits));
        Assert.That(ids.Count, Is.EqualTo(expectedHits));
    }

    [TestCaseSource(nameof(subjectAndDomainFilterTestCases))]
    public async Task Serach_CanUsePrioritySubjectsAndDomains(IEnumerable<string> subjectDomainIds, int expectedHits)
    {
        var data = TestHelpers.GetStaticIndexEntries();
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        GenerateFilterParameterAndFields("");
        searchFilterParameter.SubjectDomains = subjectDomainIds.ToArray();
        searchFilterParameter.SubjectDomainsPriority = true;

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        var ids = result.EntryIds.ToList();
        Assert.That(result.TotalEntryCount, Is.EqualTo(8));
        Assert.That(ids.Count, Is.EqualTo(8));
    }

    [Test]
    public async Task Serach_PriorityForSubjectAndDomainsWorks()
    {
        var data = TestHelpers.GetStaticIndexEntries();
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);

        GenerateFilterParameterAndFields("");
        searchFilterParameter.SubjectDomains =
        [
            "11-102", "10-101"
        ];
        searchFilterParameter.SubjectDomainsPriority = true;

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        var ids = result.EntryIds.ToList();
        Assert.That(result.TotalEntryCount, Is.EqualTo(8));
        Assert.That(ids.Count, Is.EqualTo(8));
        Assert.That(ids[0], Is.EqualTo(1));
        Assert.That(ids[1], Is.EqualTo(2));
    }

    [Test]
    public async Task Serach_CanUsePriorityForAllFields()
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        searchFields = new()
        {
            Terminus = true,
            Name = true,
            Abbreviation = true,
            Phraseology = true,
            Definition = true,
            Note = true,
            Context = true,
            Source = true,
            Metadata = true,
            Country = true,
            Comment = true,
        };

        searchFilterParameter = new()
        {
            StatusPriority = true,
            OfficesPriority = true,
            CollectionsPriority = true,
            LenochClassificationPriority = true,
            AdminInfosPriority = true,
            CreatedByPriority = true,
            ModifiedByPriority = true,
            ModifiedPeriodPriority = true,
            SubjectDomainsPriority = true,
            DeletedPriority = true,
            Status =
            [
                Status.Validated, Status.InProgress
            ],
            Offices =
            [
                1, 3
            ],
            Collections =
            [
                68, 10301
            ],
            LenochClassifications =
            [
                200, 300
            ],
            AdminInfos =
            [
                1, 2
            ],
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
            ModifiedFrom = DateTime.UtcNow.AddDays(-20),
            ModifiedTo = DateTime.UtcNow.AddDays(-1),
            Phrase = "XML",
            PageIndex = 1,
            PageSize = 1000,
        };

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.InstanceOf<SearchResult>());
        Assert.That(result.TotalEntryCount, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task Serach_CanUseFilterForAllFields()
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        searchFields = new()
        {
            Terminus = true,
            Name = true,
            Abbreviation = true,
            Phraseology = true,
            Definition = true,
            Note = true,
            Context = true,
            Source = true,
            Metadata = true,
            Country = true,
            Comment = true,
        };

        searchFilterParameter = new()
        {
            Status =
            [
                (Status)indexEntries[0].Status
            ],
            Offices =
            [
                indexEntries[0].OfficeId
            ],
            Collections =
            [
                indexEntries[0].CollectionId
            ],
            LenochClassifications =
            [
                indexEntries[0].LenochClassifications[0]
            ],
            AdminInfos =
            [
                indexEntries[0].AdminInfos[0]
            ],
            CommentStatus =
            [
                string.IsNullOrWhiteSpace(indexEntries[0].Comment) ? CommentStatus.Empty : CommentStatus.Filled
            ],
            CreatedBy = indexEntries[0].CreatedBy,
            ModifiedBy = indexEntries[0].ModifiedBy,
            ModifiedFrom = indexEntries[0].Modified.AddDays(-1),
            ModifiedTo = indexEntries[0].Modified.AddDays(+1),
            Phrase = indexEntries[0].Terminus,
            PageIndex = 1,
            PageSize = 1000,
        };

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.InstanceOf<SearchResult>());
        Assert.That(result.TotalEntryCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Serach_CanUseFilterAndPriorityCombinationForAllFields()
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        searchFields = new()
        {
            Terminus = true,
            Name = true,
            Abbreviation = true,
            Phraseology = true,
            Definition = true,
            Note = true,
            Context = true,
            Source = true,
            Metadata = true,
            Country = true,
            Comment = true,
        };

        searchFilterParameter = new()
        {
            StatusPriority = true,
            OfficesPriority = false,
            CollectionsPriority = true,
            LenochClassificationPriority = false,
            AdminInfosPriority = true,
            CreatedByPriority = false,
            ModifiedByPriority = true,
            ModifiedPeriodPriority = false,
            Status =
            [
                (Status)indexEntries[0].Status
            ],
            Offices =
            [
                indexEntries[0].OfficeId
            ],
            Collections =
            [
                indexEntries[0].CollectionId
            ],
            LenochClassifications =
            [
                indexEntries[0].LenochClassifications[0]
            ],
            AdminInfos =
            [
                indexEntries[0].AdminInfos[0]
            ],
            CommentStatus =
            [
                string.IsNullOrWhiteSpace(indexEntries[0].Comment) ? CommentStatus.Empty : CommentStatus.Filled
            ],
            CreatedBy = indexEntries[0].CreatedBy,
            ModifiedBy = indexEntries[0].ModifiedBy,
            ModifiedFrom = indexEntries[0].Modified.AddDays(-1),
            ModifiedTo = indexEntries[0].Modified.AddDays(+1),
            Phrase = indexEntries[0].Abbreviation,
            PageIndex = 1,
            PageSize = 1000,
        };

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.InstanceOf<SearchResult>());
        Assert.That(result.TotalEntryCount, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task Serach_ShouldFindReferences()
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);
        var entryWithReferenceId = indexEntries.First(i => i.References.Any() && !i.Deleted);

        var phrase = $"References: {entryWithReferenceId.References.First()}";

        searchFilterParameter = new()
        {
            Phrase = phrase,
            PageIndex = 1,
            PageSize = 1000,
        };
        searchFields = new()
        {
            Terminus = true,
            Name = true,
            Abbreviation = true,
            Phraseology = true,
            Definition = true,
            Note = true,
            Context = true,
            Source = true,
            Metadata = true,
            Country = true,
            Comment = true,
        };

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.InstanceOf<SearchResult>());
        Assert.That(result.TotalEntryCount, Is.GreaterThanOrEqualTo(1));
        Assert.That(result.EntryIds, Contains.Item(entryWithReferenceId.EntryId));
    }

    [TestCase("EntryId:[1 TO ]")]
    [TestCase("\"test")]
    [TestCase("bund\\")]
    [TestCase("bund^")]
    [TestCase("bund AND OR bund")]
    [TestCase("EntryId: 1 AND")]
    [TestCase("EntryId: 1 OR")]
    public async Task Serach_ShouldNotThrowQueryParserExceptionOnInvalidLuceneQuerySyntax(string searchPhrase)
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        GenerateFilterParameterAndFields(searchPhrase);

        Assert.DoesNotThrow(() => searchRepository.Search(searchFilterParameter, searchFields, true));
    }

    [TestCase("EntryId:[1 TO ]")]
    [TestCase("\"test")]
    [TestCase("bund\\")]
    [TestCase("bund^")]
    [TestCase("bund AND OR bund")]
    [TestCase("EntryId: 1 AND")]
    [TestCase("EntryId: 1 OR")]
    public async Task Recommend_ShouldNotThrowQueryParserExceptionOnInvalidLuceneQuerySyntax(string searchPhrase)
    {
        await searchRepository.ImportEntries(indexEntries.ToAsyncEnumerable(), CancellationToken.None);

        Assert.DoesNotThrow(() => searchRepository.Recommend(searchPhrase, [1, 2, 3]));
    }

    [TestCase("Gilliéron")]
    [TestCase("Gillieron")]
    public async Task Serach_ForEntryLanguageSources_ShouldFindEquivalents(string searchPhrase)
    {
        var data = TestHelpers.GetStaticIndexEntries();
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);
        GenerateFilterParameterAndFields(searchPhrase);
        searchFields.Source = true;
        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.TotalEntryCount, Is.EqualTo(4));
        Assert.That(result.EntryIds, Has.Member(1));
        Assert.That(result.EntryIds, Has.Member(2));
        Assert.That(result.EntryIds, Has.Member(3));
        Assert.That(result.EntryIds, Has.Member(4));
    }

    [Test]
    public async Task Serach_WithDashBeforeString_FindsHits()
    {
        var data = TestHelpers.GetStaticIndexEntries();
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);
        GenerateFilterParameterAndFields("Hundehalterinnen und -halter");

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Serach_WithDashBeforeString_FindsNoHitsWhenNotInIndex()
    {
        var data = TestHelpers.GetStaticIndexEntries();
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);
        GenerateFilterParameterAndFields("Hundehalter und -halterinnen");

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task Serach_WithHasMissingSourceTerminus_ShouldFindEntries()
    {
        var data = TestHelpers.GetStaticIndexEntries();
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);
        searchFilterParameter = new()
        {
            Phrase = "UNIQUE",
            PageIndex = 1,
            PageSize = 1000,
            HasMissingSource = true
        };

        searchFields = new()
        {
            Terminus = true,
            Name = false,
            Abbreviation = false,
            Phraseology = false,
            Definition = false,
            Note = false,
            Context = false,
            Source = false,
            Metadata = false,
            Country = false,
            Comment = false
        };

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.EqualTo(3));

        Assert.That(result.EntryIds, Is.EquivalentTo(new[] { 4, 5, 6 }));
    }

    [TestCase(true, false, false, false, false, false, false)]
    [TestCase(false, true, false, false, false, false, false)]
    [TestCase(false, false, true, false, false, false, false)]
    [TestCase(false, false, false, true, false, false, false)]
    [TestCase(false, false, false, false, true, false, false)]
    [TestCase(false, false, false, false, false, true, false)]
    [TestCase(false, false, false, false, false, false, true)]
    public async Task Serach_WithHasMissingSource_ShouldFindEntries(bool terminus, bool name, bool abbreviation, bool phraseology, bool definition, bool note, bool context)
    {
        var data = TestHelpers.GetStaticIndexEntries();
        await searchRepository.ImportEntries(data.ToAsyncEnumerable(), CancellationToken.None);
        searchFilterParameter = new()
        {
            Phrase = "verified",
            PageIndex = 1,
            PageSize = 1000,
            HasMissingSource = true
        };

        searchFields = new()
        {
            Terminus = terminus,
            Name = name,
            Abbreviation = abbreviation,
            Phraseology = phraseology,
            Definition = definition,
            Note = note,
            Context = context,
            Source = true,
            Metadata = true,
            Country = true,
            Comment = true
        };

        var result = searchRepository.Search(searchFilterParameter, searchFields);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds.Count, Is.EqualTo(1));

        Assert.That(result.EntryIds.First(), Is.EqualTo(4));
    }

    private void GenerateFilterParameterAndFields(string phrase)
    {
        searchFilterParameter = new()
        {
            Phrase = phrase,
            PageIndex = 1,
            PageSize = 1000,
        };

        searchFields = new()
        {
            Name = true,
            Phraseology = true,
            Abbreviation = true,
        };
    }

    private static IndexEntry GetIndexEntry(Document document)
    {
        if (!int.TryParse(document.Get(nameof(IndexEntry.EntryId)), out var entryId))
        {
            throw new FormatException($"Invalid {nameof(IndexEntry.EntryId)}.");
        }

        if (!int.TryParse(document.Get(nameof(IndexEntry.LanguageId)), out var languageId))
        {
            throw new FormatException($"Invalid {nameof(IndexEntry.LanguageId)}.");
        }

        if (!int.TryParse(document.Get(nameof(IndexEntry.SequenceId)), out var sequenceId))
        {
            throw new FormatException($"Invalid {nameof(IndexEntry.SequenceId)}.");
        }

        return new IndexEntry
        {
            EntryId = entryId,
            LanguageId = languageId,
            SequenceId = sequenceId,
            Terminus = document.Get(nameof(IndexEntry.Terminus)),
            Abbreviation = document.Get(nameof(IndexEntry.Abbreviation)),
            Name = document.Get(nameof(IndexEntry.Name)),
            Phraseology = document.Get(nameof(IndexEntry.Phraseology)),
            Definition = document.Get(nameof(IndexEntry.Definition)),
            Note = document.Get(nameof(IndexEntry.Note)),
            Context = document.Get(nameof(IndexEntry.Context)),
            Metadata = document.Get(nameof(IndexEntry.Metadata)),
            Country = document.Get(nameof(IndexEntry.Country)),
        };
    }

    private static void AssertIndexEntry(IndexEntry actual, IndexEntry expected)
    {
        Assert.That(actual.EntryId, Is.EqualTo(expected.EntryId));
        Assert.That(actual.LanguageId, Is.EqualTo(expected.LanguageId));
        Assert.That(actual.SequenceId, Is.EqualTo(expected.SequenceId));
        Assert.That(actual.Terminus, Is.EqualTo(expected.Terminus));
        Assert.That(actual.Abbreviation, Is.EqualTo(expected.Abbreviation));
        Assert.That(actual.Name, Is.EqualTo(expected.Name));
        Assert.That(actual.Phraseology, Is.EqualTo(expected.Phraseology));
        Assert.That(actual.Definition, Is.EqualTo(expected.Definition));
        Assert.That(actual.Note, Is.EqualTo(expected.Note));
        Assert.That(actual.Context, Is.EqualTo(expected.Context));
        Assert.That(actual.Metadata, Is.EqualTo(expected.Metadata));
        Assert.That(actual.Country, Is.EqualTo(expected.Country));
    }

    private static readonly object[] subjectAndDomainFilterTestCases =
    [
        new object[] { new[] { "10", "11" }, 5 },
        new object[] { new[] { "10" }, 2 },
        new object[] { new[] { "10-101" }, 1 },
        new object[] { new[] { "10-100", "10-101" }, 2 },
        new object[] { new[] { "11-102", "11-101" }, 2 },
        new object[] { new[] { "11-102", "11-101", "13" }, 4 }
    ];
}
