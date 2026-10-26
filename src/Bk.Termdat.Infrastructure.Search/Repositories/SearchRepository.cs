using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.CrossCutting.Exceptions;
using Bk.Termdat.Infrastructure.Search.Analyzers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Grouping;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CommentStatus = Bk.Termdat.Business.Resource.Models.CommentStatus;
using Models = Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Infrastructure.Search.Repositories;

public class SearchRepository : ISearchRepository
{
    private readonly ILogger<SearchRepository> logger;
    private readonly ISearchGateway searchGateway;
    private readonly SearchOptions options;
    private static readonly Dictionary<string, float> FieldSpecificBoosts = new()
    {
        { SearchGateway.TerminusField, 2f },
        { SearchGateway.NameField, 2f },
        { SearchGateway.AbbreviationField, 2f },
        { SearchGateway.PhraseologyField, 1.5f },
    };

    private static readonly string[] RecommendationFields = {
        SearchGateway.TerminusField,
        SearchGateway.NameField,
        SearchGateway.PhraseologyField
    };

    private static readonly string[] DefaultSearchFields = {
        SearchGateway.TerminusField,
        SearchGateway.AbbreviationField,
        SearchGateway.NameField,
        SearchGateway.PhraseologyField,
        SearchGateway.MetadataField
    };

    private static readonly string[] DefaultSearchKeywordFields = {
        SearchGateway.TerminusKeywordField,
        SearchGateway.AbbreviationKeywordField,
        SearchGateway.NameKeywordField,
        SearchGateway.PhraseologyKeywordField,
        SearchGateway.MetadataKeywordField
    };

    private static readonly string[] DefaultSearchEquivalentsFields = {
        SearchGateway.TerminusEquivalentsField,
        SearchGateway.NameEquivalentsField,
        SearchGateway.PhraseologyEquivalentsField,
        SearchGateway.MetadataEquivalentsField
    };

    private static readonly Regex DangerousWildCardRegex = new(@"[^A-Za-z0-9]\*", RegexOptions.None, TimeSpan.FromSeconds(1));

    private readonly IDictionary<string, Analyzer> fieldSpecificAnalyzers;

    public SearchRepository(ILogger<SearchRepository> logger, ISearchGateway searchGateway, IOptions<SearchOptions> options)
    {
        ArgumentNullException.ThrowIfNull(searchGateway);
        ArgumentNullException.ThrowIfNull(options);

        this.logger = logger;
        this.searchGateway = searchGateway;
        this.options = options.Value;

        fieldSpecificAnalyzers = new Dictionary<string, Analyzer>(StringComparer.CurrentCultureIgnoreCase)
        {
            { SearchGateway.CreatedByField, new UAX29URLEmailAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.ModifiedByField, new UAX29URLEmailAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.TerminusEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.AbbreviationEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.NameEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.PhraseologyEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.DefinitionEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.NoteEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.ContextEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.CountryEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.MetadataEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            // Source Fields
            { SearchGateway.TerminusSourceEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.AbbreviationSourceEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.NameSourceEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.PhraseologySourceEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.DefinitionSourceEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.NoteSourceEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.ContextSourceEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
            { SearchGateway.MetadataSourceEquivalentsField, new CharEquivalentsAnalyzer(searchGateway.LuceneVersion) },
        };
    }

    public SearchResult Search(SearchFilterParameter searchFilterParameter, SearchFields searchFields, bool includeDeleted = false, bool includeDeactivatedCollectionEntries = false)
    {
        ArgumentNullException.ThrowIfNull(searchFilterParameter);

        var (entryIds, totalEntryCount) = GetSearchResult(searchFilterParameter, searchFields, includeDeleted, includeDeactivatedCollectionEntries);

        return new SearchResult
        {
            EntryIds = entryIds,
            TotalEntryCount = totalEntryCount
        };
    }

    private (IEnumerable<int> entryIds, int totalEntryCount) GetSearchResult(SearchFilterParameter searchFilterParameter, SearchFields searchFields, bool includeDeleted, bool includeDeactivatedCollectionEntries)
    {
        if (searchFilterParameter.PageIndex < 1)
        {
            throw new ArgumentException(TechnicalTexts.ParameterCannotBeZeroOrSmaller, nameof(searchFilterParameter));
        }

        var allQueries = new BooleanQuery();

        // Search without phrase shall return everything and matches can be reduced by using the extended search
        if (!string.IsNullOrEmpty(searchFilterParameter.Phrase))
        {
            var query = GetSearchQuery(searchFilterParameter.Phrase, searchFields, searchFilterParameter.SourceLanguageIds);
            allQueries.Add(query, Occur.MUST);
        }
        else
        {
            var query = new MatchAllDocsQuery();
            allQueries.Add(query, Occur.MUST);
        }

        if (IsPrioritySearch(searchFilterParameter))
        {
            // if priority search is used, we get the criteria here and add them to a BooleanQuery
            var priorityQueries = GetPriorityQueries(searchFilterParameter);

            foreach (var priorityQuery in priorityQueries)
            {
                allQueries.Add(priorityQuery, Occur.SHOULD);
            }

            // special handling of the date range priority search required
            allQueries = GetPriorityNumericRangeQuery(allQueries, searchFilterParameter);
        }

        if (searchFilterParameter.HasMissingSource)
        {
            var missingSourceQuery = BuildMissingSourceQuery(searchFields);
            allQueries.Add(missingSourceQuery, Occur.MUST);
        }

        // fields used in priority search will not be added as filter criteria
        var filter = GetFilterExtended(searchFilterParameter, includeDeleted, includeDeactivatedCollectionEntries);

        var searcher = searchGateway.SearcherManager.Acquire();
        try
        {
            var groupingSearch = new GroupingSearch(SearchGateway.IdField);
            groupingSearch.SetAllGroups(true);
            groupingSearch.SetIncludeScores(true);
            groupingSearch.SetIncludeMaxScore(true);
            groupingSearch.SetFillSortFields(true);

            var sort = new Sort(
                SortField.FIELD_SCORE,
                new SortField(SearchGateway.LanguageCountSortField, SortFieldType.STRING, true),
                new SortField(SearchGateway.ModifiedSortField, SortFieldType.STRING, true)
            );
            groupingSearch.SetGroupSort(sort);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Lucene Search Query: {Queries}", allQueries);
            }

            var offset = (searchFilterParameter.PageIndex - 1) * searchFilterParameter.PageSize;

            ITopGroups<object> result;
            try
            {
                result = groupingSearch.Search(searcher, filter, allQueries, offset, searchFilterParameter.PageSize);
            }
            catch (BooleanQuery.TooManyClausesException ex)
            {
                throw new SearchException("Query to complex", ex);
            }

            var resultGroups = result.Groups
                .OrderByDescending(g => g.MaxScore)
                .ThenByDescending(g => g.ScoreDocs.Sum(sd => sd.Score)).ToList();

            var groupedIds = resultGroups
                .Select(resultGroup => resultGroup.ScoreDocs.First())
                .Select(groupedTopDoc => searcher.Doc(groupedTopDoc.Doc))
                .Select(foundDoc => int.Parse(foundDoc.Get(SearchGateway.IdField))).ToList();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Hits: {Count}", result.TotalGroupCount);
            }

            return (groupedIds, result.TotalGroupCount ?? 0);
        }
        finally
        {
            searchGateway.SearcherManager.Release(searcher);
        }
    }

    public IEnumerable<string> Recommend(string searchPhrase, IEnumerable<int> languageIds = null, bool includeDeleted = false, bool includeDeactivatedCollectionEntries = false)
    {
        if (searchPhrase is null || searchPhrase.Length < options.RecommendationMinLength)
        {
            return Array.Empty<string>();
        }

        var queries = GetRecommendQueries(searchPhrase);

        var filter = GetFilter(languageIds, includeDeleted, includeDeactivatedCollectionEntries);

        var searcher = searchGateway.SearcherManager.Acquire();
        try
        {
            var allHits = new List<KeyValuePair<string, ScoreDoc>>();

            foreach (var (field, query) in queries)
            {
                // Duplicates could exist, so maximum hits must be increased
                var docs = searcher.Search(query, filter, options.RecommendationResultSize * 5);
                allHits.AddRange(docs.ScoreDocs.Select(scoreDoc => new KeyValuePair<string, ScoreDoc>(field, scoreDoc)));
            }

            var results = new List<Recommendation>();
            foreach (var (field, scoreDoc) in allHits)
            {
                var foundDoc = searcher.Doc(scoreDoc.Doc);
                var recommendation = new Recommendation()
                {
                    Value = foundDoc.Get(field),
                    Score = scoreDoc.Score
                };
                var existingResult = results.FirstOrDefault(r => r.Value.Equals(recommendation.Value, StringComparison.OrdinalIgnoreCase));

                if (existingResult != null)
                {
                    existingResult.Score += scoreDoc.Score;
                    continue;
                }

                results.Add(recommendation);
            }

            return results
                .OrderBy(r => r.Value.Length)
                .ThenByDescending(r => r.Score)
                .Take(options.RecommendationResultSize)
                .Select(r => r.Value);
        }
        finally
        {
            searchGateway.SearcherManager.Release(searcher);
        }
    }

    public async Task ImportEntries(IAsyncEnumerable<IndexEntry> entries, CancellationToken stoppingToken)
    {
        var indexWriter = searchGateway.Writer;
        logger.LogDebug("Adding documents to index");

        await foreach (var entry in entries.WithCancellation(stoppingToken))
        {
            if (entry is null)
            {
                logger.LogWarning("An null entry cannot be indexed");
                continue;
            }

            try
            {
                indexWriter.AddDocument(GetDocument(entry));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while adding document (id: '{EntryId}-{LanguageId}-{SequenceId}') to index", entry.EntryId, entry.LanguageId, entry.SequenceId);
                logger.LogError("{Data}", JsonSerializer.Serialize(entry));
            }
        }

        indexWriter.Commit();
        searchGateway.SearcherManager.MaybeRefresh();
    }

    public void AddEntries(IEnumerable<IndexEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var indexWriter = searchGateway.Writer;

        foreach (var entry in entries)
        {
            indexWriter.AddDocument(GetDocument(entry));
        }

        indexWriter.Commit();
        searchGateway.SearcherManager.MaybeRefresh();
    }

    public void DeleteEntries(IEnumerable<int> entryIds)
    {
        var indexWriter = searchGateway.Writer;
        var ids = entryIds.ToList();

        foreach (var id in ids)
        {
            indexWriter.DeleteDocuments(new Term(SearchGateway.IdField, id.ToString(CultureInfo.InvariantCulture)));
        }

        indexWriter.Commit();
        searchGateway.SearcherManager.MaybeRefresh();
    }

    public void UpdateEntries(IEnumerable<IndexEntry> entries)
    {
        var indexWriter = searchGateway.Writer;
        var entryList = entries.ToList();
        if (entryList.Count == 0)
        {
            throw new InvalidOperationException($"Cannot update on an empty list of {nameof(entries)}.");
        }

        var entryIdsToDelete = entryList.Select(e => e.EntryId).Distinct().ToList();

        foreach (var entryId in entryIdsToDelete)
        {
            indexWriter.DeleteDocuments(new Term(SearchGateway.IdField, entryId.ToString(CultureInfo.InvariantCulture)));
        }

        foreach (var entry in entryList)
        {
            indexWriter.AddDocument(GetDocument(entry));
        }

        indexWriter.Commit();
        searchGateway.SearcherManager.MaybeRefresh();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Lucene.NET: Updated {Count} in index", entryList.Count);
        }
    }

    private static string ToIndexDateTime(DateTime dt)
    {
        return dt.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
    }

    private static string ToSortDateTime(DateTime dt)
    {
        return dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
    }

    private static long ToInt64DateTime(string dateTimeString)
    {
        var value = long.Parse(dateTimeString, CultureInfo.InvariantCulture);
        return value;
    }

    private static BytesRef ToIntBytesRef(string dateTimeString)
    {
        var value = ToInt64DateTime(dateTimeString);
        return new BytesRef(BitConverter.GetBytes(value));
    }

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
    private static Document GetDocument(IndexEntry entry)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
    {
        var commentStatus = string.IsNullOrWhiteSpace(entry.Comment) ? CommentStatus.Empty : CommentStatus.Filled;

        var fields = new List<Field>
        {
            // Metadata fields
            new StringField(SearchGateway.IdField, entry.EntryId.ToString(CultureInfo.InvariantCulture), Field.Store.YES),
            new StringField(SearchGateway.LanguageIdField, entry.LanguageId.ToString(CultureInfo.InvariantCulture), Field.Store.YES),
            new StringField(SearchGateway.SequenceIdField, entry.SequenceId.ToString(CultureInfo.InvariantCulture), Field.Store.YES),
            new StringField(SearchGateway.OfficeIdField, entry.OfficeId.ToString(CultureInfo.InvariantCulture), Field.Store.NO),
            new StringField(SearchGateway.CollectionIdField, entry.CollectionId.ToString(CultureInfo.InvariantCulture), Field.Store.NO),
            new StringField(SearchGateway.CollectionIsActive, entry.CollectionIsActive.ToString(), Field.Store.NO),
            new StringField(SearchGateway.StatusField, ((int)entry.Status).ToString(CultureInfo.InvariantCulture), Field.Store.NO),
            new StringField(SearchGateway.CommentStatusField, ((int)commentStatus).ToString(CultureInfo.InvariantCulture), Field.Store.NO),
            new StringField(SearchGateway.ReliabilityField, ((int)entry.Reliability).ToString(CultureInfo.InvariantCulture), Field.Store.NO),
            new StringField(SearchGateway.DeletedField, entry.Deleted.ToString(), Field.Store.NO),
            new Int64Field(SearchGateway.ModifiedField, ToInt64DateTime(ToIndexDateTime(entry.Modified)), Field.Store.NO),

            // Sort fields
            new SortedDocValuesField(SearchGateway.LanguageCountSortField, new BytesRef(entry.LanguageCount)),
            new SortedDocValuesField(SearchGateway.ModifiedSortField, ToIntBytesRef(ToSortDateTime(entry.Modified))),
        };

        if (!string.IsNullOrWhiteSpace(entry.CreatedBy))
        {
            fields.Add(new StringField(SearchGateway.CreatedByField, entry.CreatedBy, Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.ModifiedBy))
        {
            fields.Add(new StringField(SearchGateway.ModifiedByField, entry.ModifiedBy, Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.Terminus))
        {
            fields.Add(new TextField(SearchGateway.TerminusField, entry.Terminus, Field.Store.YES));
            fields.Add(new TextField(SearchGateway.TerminusEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.Terminus, entry.LanguageId), Field.Store.NO));
            fields.Add(new StringField(SearchGateway.TerminusKeywordField, entry.Terminus.ToUpperInvariant(), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.Name))
        {
            fields.Add(new TextField(SearchGateway.NameField, entry.Name, Field.Store.YES));
            fields.Add(new TextField(SearchGateway.NameEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.Name, entry.LanguageId), Field.Store.NO));
            fields.Add(new StringField(SearchGateway.NameKeywordField, entry.Name.ToUpperInvariant(), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.Abbreviation))
        {
            fields.Add(new TextField(SearchGateway.AbbreviationField, entry.Abbreviation, Field.Store.YES));
            fields.Add(new TextField(SearchGateway.AbbreviationEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.Abbreviation, entry.LanguageId), Field.Store.NO));
            fields.Add(new StringField(SearchGateway.AbbreviationKeywordField, entry.Abbreviation.ToUpperInvariant(), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.Phraseology))
        {
            fields.Add(new TextField(SearchGateway.PhraseologyField, entry.Phraseology, Field.Store.YES));
            fields.Add(new TextField(SearchGateway.PhraseologyEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.Phraseology, entry.LanguageId), Field.Store.NO));
            fields.Add(new StringField(SearchGateway.PhraseologyKeywordField, entry.Phraseology.ToUpperInvariant(), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.Definition))
        {
            fields.Add(new TextField(SearchGateway.DefinitionField, entry.Definition, Field.Store.YES));
            fields.Add(new TextField(SearchGateway.DefinitionEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.Definition, entry.LanguageId), Field.Store.NO));
            fields.Add(new StringField(SearchGateway.DefinitionKeywordField, entry.Definition.ToUpperInvariant(), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.Note))
        {
            fields.Add(new TextField(SearchGateway.NoteField, entry.Note, Field.Store.YES));
            fields.Add(new TextField(SearchGateway.NoteEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.Note, entry.LanguageId), Field.Store.NO));
            fields.Add(new StringField(SearchGateway.NoteKeywordField, entry.Note.ToUpperInvariant(), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.Context))
        {
            fields.Add(new TextField(SearchGateway.ContextField, entry.Context, Field.Store.YES));
            fields.Add(new TextField(SearchGateway.ContextEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.Context, entry.LanguageId), Field.Store.NO));
            fields.Add(new StringField(SearchGateway.ContextKeywordField, entry.Context.ToUpperInvariant(), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.Country))
        {
            fields.Add(new TextField(SearchGateway.CountryField, entry.Country, Field.Store.YES));
            fields.Add(new TextField(SearchGateway.CountryEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.Country, entry.LanguageId), Field.Store.NO));
            fields.Add(new StringField(SearchGateway.CountryKeywordField, entry.Country.ToUpperInvariant(), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.Metadata))
        {
            fields.Add(new TextField(SearchGateway.MetadataField, entry.Metadata, Field.Store.YES));
            fields.Add(new TextField(SearchGateway.MetadataEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.Metadata, entry.LanguageId), Field.Store.NO));
            fields.Add(new StringField(SearchGateway.MetadataKeywordField, entry.Metadata.ToUpperInvariant(), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.Comment))
        {
            fields.Add(new TextField(SearchGateway.CommentField, entry.Comment, Field.Store.YES));
        }

        if (!string.IsNullOrWhiteSpace(entry.TerminusSource))
        {
            fields.Add(new TextField(SearchGateway.TerminusSourceField, entry.TerminusSource, Field.Store.NO));
            fields.Add(new TextField(SearchGateway.TerminusSourceEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.TerminusSource, entry.LanguageId), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.NameSource))
        {
            fields.Add(new TextField(SearchGateway.NameSourceField, entry.NameSource, Field.Store.NO));
            fields.Add(new TextField(SearchGateway.NameSourceEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.NameSource, entry.LanguageId), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.AbbreviationSource))
        {
            fields.Add(new TextField(SearchGateway.AbbreviationSourceField, entry.AbbreviationSource, Field.Store.NO));
            fields.Add(new TextField(SearchGateway.AbbreviationSourceEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.AbbreviationSource, entry.LanguageId), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.PhraseologySource))
        {
            fields.Add(new TextField(SearchGateway.PhraseologySourceField, entry.PhraseologySource, Field.Store.NO));
            fields.Add(new TextField(SearchGateway.PhraseologySourceEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.PhraseologySource, entry.LanguageId), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.DefinitionSource))
        {
            fields.Add(new TextField(SearchGateway.DefinitionSourceField, entry.DefinitionSource, Field.Store.NO));
            fields.Add(new TextField(SearchGateway.DefinitionSourceEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.DefinitionSource, entry.LanguageId), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.NoteSource))
        {
            fields.Add(new TextField(SearchGateway.NoteSourceField, entry.NoteSource, Field.Store.NO));
            fields.Add(new TextField(SearchGateway.NoteSourceEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.NoteSource, entry.LanguageId), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.ContextSource))
        {
            fields.Add(new TextField(SearchGateway.ContextSourceField, entry.ContextSource, Field.Store.NO));
            fields.Add(new TextField(SearchGateway.ContextSourceEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.ContextSource, entry.LanguageId), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(entry.MetadataSource))
        {
            fields.Add(new TextField(SearchGateway.MetadataSourceField, entry.MetadataSource, Field.Store.NO));
            fields.Add(new TextField(SearchGateway.MetadataSourceEquivalentsField, CharEquivalentsAnalyzer.PreprocessEquivalentsText(entry.MetadataSource, entry.LanguageId), Field.Store.NO));
        }

        if (entry.Subjects?.Length > 0)
        {
            fields.AddRange(entry.Subjects.Select(d => new StringField(SearchGateway.SubjectsField, d.ToString(CultureInfo.InvariantCulture), Field.Store.YES)));
        }
        if (entry.Domains?.Length > 0)
        {
            fields.AddRange(entry.Domains.Select(d => new StringField(SearchGateway.DomainsField, d.ToString(CultureInfo.InvariantCulture), Field.Store.YES)));
        }
        if (entry.LenochClassifications?.Length > 0)
        {
            fields.AddRange(entry.LenochClassifications.Select(d => new StringField(SearchGateway.LenochClassificationsField, d.ToString(CultureInfo.InvariantCulture), Field.Store.YES)));
        }
        if (entry.Glossaries?.Length > 0)
        {
            fields.AddRange(entry.Glossaries.Select(g => new StringField(SearchGateway.GlossariesField, g.ToString(CultureInfo.InvariantCulture), Field.Store.YES)));
        }
        if (entry.References?.Length > 0)
        {
            fields.AddRange(entry.References.Select(r => new StringField(SearchGateway.ReferencesField, r.ToString(CultureInfo.InvariantCulture), Field.Store.YES)));
        }
        if (entry.RelatedCollections?.Length > 0)
        {
            fields.AddRange(entry.RelatedCollections.Select(c => new StringField(SearchGateway.RelatedCollectionsField, c.ToString(CultureInfo.InvariantCulture), Field.Store.YES)));
        }
        if (entry.AdminInfos?.Length > 0)
        {
            fields.AddRange(entry.AdminInfos.Select(a => new StringField(SearchGateway.AdminInfosField, a.ToString(CultureInfo.InvariantCulture), Field.Store.YES)));
        }

        var document = new Document();

        foreach (var field in fields)
        {
            if (FieldSpecificBoosts.TryGetValue(field.Name, out var boost))
            {
                field.Boost = boost;
            }

            document.Add(field);
        }

        return document;
    }

    private static string[] GetSearchFields(SearchFields fields)
    {
        var fieldList = new List<string>();

        if (fields.Terminus)
        {
            fieldList.Add(SearchGateway.TerminusField);
        }

        if (fields.Name)
        {
            fieldList.Add(SearchGateway.NameField);
        }

        if (fields.Abbreviation)
        {
            fieldList.Add(SearchGateway.AbbreviationField);
        }

        if (fields.Phraseology)
        {
            fieldList.Add(SearchGateway.PhraseologyField);
        }

        if (fields.Definition)
        {
            fieldList.Add(SearchGateway.DefinitionField);
        }

        if (fields.Note)
        {
            fieldList.Add(SearchGateway.NoteField);
        }

        if (fields.Context)
        {
            fieldList.Add(SearchGateway.ContextField);
        }

        if (fields.Country)
        {
            fieldList.Add(SearchGateway.CountryField);
        }

        if (fields.Metadata)
        {
            fieldList.Add(SearchGateway.MetadataField);
        }

        if (fields.Comment)
        {
            fieldList.Add(SearchGateway.CommentField);
        }

        if (fields.Source)
        {
            fieldList.AddRange(new[]
            {
                SearchGateway.TerminusSourceField,
                SearchGateway.NameSourceField,
                SearchGateway.AbbreviationSourceField,
                SearchGateway.PhraseologySourceField,
                SearchGateway.DefinitionSourceField,
                SearchGateway.NoteSourceField,
                SearchGateway.ContextSourceField,
                SearchGateway.MetadataSourceField
            });
        }

        return fieldList.Count == 0 ? DefaultSearchFields : fieldList.ToArray();
    }

    private static string[] GetSearchKeywordFields(SearchFields fields)
    {
        var fieldList = new List<string>();

        if (fields.Terminus)
        {
            fieldList.Add(SearchGateway.TerminusKeywordField);
        }

        if (fields.Name)
        {
            fieldList.Add(SearchGateway.NameKeywordField);
        }

        if (fields.Abbreviation)
        {
            fieldList.Add(SearchGateway.AbbreviationKeywordField);
        }

        if (fields.Phraseology)
        {
            fieldList.Add(SearchGateway.PhraseologyKeywordField);
        }

        if (fields.Definition)
        {
            fieldList.Add(SearchGateway.DefinitionKeywordField);
        }

        if (fields.Note)
        {
            fieldList.Add(SearchGateway.NoteKeywordField);
        }

        if (fields.Context)
        {
            fieldList.Add(SearchGateway.ContextKeywordField);
        }

        if (fields.Country)
        {
            fieldList.Add(SearchGateway.CountryKeywordField);
        }

        if (fields.Metadata)
        {
            fieldList.Add(SearchGateway.MetadataKeywordField);
        }

        return fieldList.Count == 0 ? DefaultSearchKeywordFields : fieldList.ToArray();
    }

    private static string[] GetSearchEquivalentFields(SearchFields fields)
    {
        var fieldList = new List<string>();

        if (fields.Terminus)
        {
            fieldList.Add(SearchGateway.TerminusEquivalentsField);
        }

        if (fields.Abbreviation)
        {
            fieldList.Add(SearchGateway.AbbreviationEquivalentsField);
        }

        if (fields.Name)
        {
            fieldList.Add(SearchGateway.NameEquivalentsField);
        }

        if (fields.Phraseology)
        {
            fieldList.Add(SearchGateway.PhraseologyEquivalentsField);
        }

        if (fields.Definition)
        {
            fieldList.Add(SearchGateway.DefinitionEquivalentsField);
        }

        if (fields.Note)
        {
            fieldList.Add(SearchGateway.NoteEquivalentsField);
        }

        if (fields.Context)
        {
            fieldList.Add(SearchGateway.ContextEquivalentsField);
        }

        if (fields.Country)
        {
            fieldList.Add(SearchGateway.CountryEquivalentsField);
        }

        if (fields.Metadata)
        {
            fieldList.Add(SearchGateway.MetadataEquivalentsField);
        }

        if (fields.Source)
        {
            fieldList.AddRange(new[]
            {
                SearchGateway.TerminusSourceEquivalentsField,
                SearchGateway.AbbreviationSourceEquivalentsField,
                SearchGateway.NameSourceEquivalentsField,
                SearchGateway.PhraseologySourceEquivalentsField,
                SearchGateway.DefinitionSourceEquivalentsField,
                SearchGateway.NoteSourceEquivalentsField,
                SearchGateway.ContextSourceEquivalentsField,
                SearchGateway.MetadataSourceEquivalentsField
            });
        }

        return fieldList.Count == 0 ? DefaultSearchEquivalentsFields : fieldList.ToArray();
    }


    private BooleanQuery GetSearchQuery(string searchPhrase, SearchFields searchFields, int[] sourceLanguageIds = null)
    {
        var fields = GetSearchFields(searchFields);
        var keywordFields = GetSearchKeywordFields(searchFields);
        var termsSanitized = searchPhrase.ToSanitizedLucenePhrases().ToList();
        var joinedTerms = string.Join(" ", termsSanitized);

        using var analyzer = new PerFieldAnalyzerWrapper(searchGateway.DefaultAnalyzer, fieldSpecificAnalyzers);
        var parser = new MultiFieldQueryParser(searchGateway.LuceneVersion, fields, analyzer)
        {
            MultiTermRewriteMethod = MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE,
            AllowLeadingWildcard = true,
            DefaultOperator = Operator.AND,
        };

        var (parsedQuery, equivalentsQuery, equivalentsQuerySanitized) = ParseSearchPhrase(searchPhrase, joinedTerms, searchFields, sourceLanguageIds ?? Array.Empty<int>(), parser);

        var query = new BooleanQuery
        {
            { parsedQuery, Occur.MUST },
        };

        if (joinedTerms == searchPhrase) // keyword query only makes sense for non lucene syntax queries
        {
            var keywordQuery = new BooleanQuery { Boost = options.QueryBoostVeryHighPriority };
            foreach (var field in keywordFields)
            {
                var keywordTermQuery = new TermQuery(new Term(field, joinedTerms.ToUpperInvariant()));
                keywordQuery.Add(keywordTermQuery, Occur.SHOULD);
            }
            query.Add(keywordQuery, Occur.SHOULD);
        }

        if (termsSanitized.Count > 1) // Phrase query only makes sense for multiple terms
        {
            var phraseQuery = parser.Parse($"\"{joinedTerms}\"");
            query.Add(phraseQuery, Occur.SHOULD);
        }

        var wrapperQuery = new BooleanQuery
        {
            { query, Occur.SHOULD },
            { equivalentsQuery, Occur.SHOULD },
            { equivalentsQuerySanitized, Occur.SHOULD }
        };

        return wrapperQuery;
    }

    private (Query, Query, Query) ParseSearchPhrase(string searchPhrase, string searchPhraseSanitzed, SearchFields searchFields, int[] languageIds, QueryParser parser)
    {
        try
        {
            if (DangerousWildCardRegex.IsMatch(searchPhrase))
            {
                searchPhrase = searchPhrase.ToSanitizedLucenePhrase();
            }

            return (parser.Parse(searchPhrase), GetCharEquivalentsQuery(searchPhrase, searchFields, languageIds), GetCharEquivalentsQuery(searchPhraseSanitzed, searchFields, languageIds));
        }
        catch (ParseException parseEx)
        {
            try
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(parseEx, "Could not create search query: '{Phrase}'", searchPhrase);
                }

                // Escape query and try again since it could be a valid query based on data in the system,
                // even tho lucene considers it to be invalid query parser syntax
                var sanitizedPhrase = searchPhrase.ToSanitizedLucenePhrase();
                return (parser.Parse(sanitizedPhrase), GetCharEquivalentsQuery(sanitizedPhrase, searchFields, languageIds), GetCharEquivalentsQuery(searchPhraseSanitzed, searchFields, languageIds));
            }
            catch (ParseException ex)
            {
                throw new SearchException($"Could not create search query even after escaping: '{searchPhrase}'.", ex);
            }
        }
    }

    private Query GetCharEquivalentsQuery(string searchPhrase, SearchFields searchFields, int[] languageIds)
    {
        const float boost = 0.25f;

        var equivalentFields = GetSearchEquivalentFields(searchFields);
        var languageIdList = languageIds.ToList();

        using var charEquivalentsAnalyzer = new CharEquivalentsAnalyzer(searchGateway.LuceneVersion);
        var charEquivalentsParser = new MultiFieldQueryParser(searchGateway.LuceneVersion, equivalentFields, charEquivalentsAnalyzer)
        {
            MultiTermRewriteMethod = MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE,
            AllowLeadingWildcard = true,
            DefaultOperator = Operator.AND,
        };

        var generalPreprocessedSearchPhrase = CharEquivalentsAnalyzer.PreprocessEquivalentsText(searchPhrase);
        var generalPreprocessedQuery = charEquivalentsParser.Parse(generalPreprocessedSearchPhrase);
        var groupedSearchPhrases = languageIdList.Select(id => (id, CharEquivalentsAnalyzer.PreprocessEquivalentsText(searchPhrase, id))).GroupBy(g => g.Item2).ToList();

        // If no language specific preprocessing happened, groupedSearchPhrases will only contain one entry and no language specific condition needs to be added
        if (languageIdList.Count == 0 || groupedSearchPhrases.Count == 1)
        {
            generalPreprocessedQuery.Boost = boost;

            return generalPreprocessedQuery;
        }

        var wrapperQuery = new BooleanQuery
        {
            Boost = boost
        };
        wrapperQuery.Add(generalPreprocessedQuery, Occur.SHOULD);

        foreach (var group in groupedSearchPhrases)
        {
            var phrase = group.Key;
            if (phrase == generalPreprocessedSearchPhrase)
            {
                continue;
            }

            var languageId = group.First().id;
            var languageQuery = new BooleanQuery
            {
                { charEquivalentsParser.Parse(phrase), Occur.MUST },
                { new TermQuery(new Term(SearchGateway.LanguageIdField, languageId.ToString(CultureInfo.InvariantCulture))), Occur.MUST }
            };
            wrapperQuery.Add(languageQuery, Occur.SHOULD);
        }

        return wrapperQuery;
    }

    private Dictionary<string, Query> GetRecommendQueries(string searchPhrase)
    {
        using var analyzer = new PerFieldAnalyzerWrapper(searchGateway.DefaultAnalyzer, fieldSpecificAnalyzers);
        var parsers = RecommendationFields
            .Select(field => new QueryParser(searchGateway.LuceneVersion, field, analyzer)
            {
                DefaultOperator = Operator.AND
            })
            .ToDictionary(parser => parser.Field);

        var phrases = searchPhrase.ToSanitizedLucenePhrases()
            .Take(6)
            .ToList();

        var queries = new Dictionary<string, Query>();

        foreach (var (field, parser) in parsers)
        {
            var joinedPhrases = string.Join(" ", phrases);

            var directQuery = parser.Parse(joinedPhrases);
            directQuery.Boost = options.QueryBoostMidPriority;

            var phraseQuery = parser.Parse($"\"{joinedPhrases}\"");
            phraseQuery.Boost = options.QueryBoostLowPriority;

            var wildCardQuery = parser.Parse(joinedPhrases + "*");

            var finalQuery = new BooleanQuery
            {
                {
                    directQuery, Occur.SHOULD
                },
                {
                    wildCardQuery, Occur.MUST
                },
                {
                    phraseQuery, Occur.SHOULD
                }
            };

            finalQuery.Boost = options.QueryBoostMidPriority;
            var finalQueryWrapped = CreateRecommendationWrapperQuery(finalQuery);

            queries.Add(field, finalQueryWrapped);
        }

        return queries;
    }

    private BooleanQuery CreateRecommendationWrapperQuery(Query query)
    {
        return new BooleanQuery
        {
            {
                query,
                Occur.MUST
            },
            {
                new TermQuery(new Term(SearchGateway.StatusField, ((int)Status.Validated).ToString(CultureInfo.InvariantCulture))) { Boost = options.QueryBoostHighestPriority },
                Occur.SHOULD
            },
            {
                new TermQuery(new Term(SearchGateway.OfficeIdField, options.AchOfficeId.ToString(CultureInfo.InvariantCulture))) { Boost = options.QueryBoostHighestPriority },
                Occur.SHOULD
            },
            {
                new TermQuery(new Term(SearchGateway.SequenceIdField, options.PreferredEntrySequenceId)) { Boost = options.QueryBoostLowestPriority},
                Occur.SHOULD
            },
            {
                new TermQuery(new Term(SearchGateway.ReliabilityField, ((int)Reliability.Approved).ToString(CultureInfo.InvariantCulture))) { Boost = options.QueryBoostMidPriority },
                Occur.SHOULD
            },
            {
                new TermQuery(new Term(SearchGateway.ReliabilityField, ((int)Reliability.Verified).ToString(CultureInfo.InvariantCulture))) { Boost = options.QueryBoostLowPriority },
                Occur.SHOULD
            },
            {
                new TermQuery(new Term(SearchGateway.ReliabilityField, ((int)Reliability.Translated).ToString(CultureInfo.InvariantCulture))) { Boost = options.QueryBoostVeryLowPriority },
                Occur.SHOULD
            },
            {
                new TermQuery(new Term(SearchGateway.ReliabilityField, ((int)Reliability.Tentative).ToString(CultureInfo.InvariantCulture))) { Boost = options.QueryBoostLowestPriority },
                Occur.SHOULD
            },
        };
    }

    private static BooleanFilter? GetFilter(IEnumerable<int> languageIds, bool includeDeleted, bool includeDeactivatedCollectionEntries)
    {
        var languageIdArray = languageIds?.ToArray() ?? Array.Empty<int>();
        if (languageIdArray.Length == 0 && includeDeleted)
        {
            return null;
        }

        var filter = new BooleanFilter();

        if (languageIdArray.Length > 0)
        {
            foreach (var language in languageIdArray)
            {
                filter.Add(new TermFilter(new Term(SearchGateway.LanguageIdField, language.ToString(CultureInfo.InvariantCulture))), Occur.SHOULD);
            }
        }

        if (!includeDeleted)
        {
            filter.Add(new TermFilter(new Term(SearchGateway.DeletedField, false.ToString())), Occur.MUST);
        }

        if (!includeDeactivatedCollectionEntries)
        {
            filter.Add(new TermFilter(new Term(SearchGateway.CollectionIsActive, true.ToString())), Occur.MUST);
        }

        return filter;
    }

    private List<Query> GetPriorityQueries(SearchFilterParameter searchFilterParameter)
    {
        var priorityQueries = new List<Query>();

        priorityQueries.AddRange(GetHighestPriorityQueries(searchFilterParameter));

        priorityQueries.AddRange(GetHighPriorityQueries(searchFilterParameter));

        priorityQueries.AddRange(GetMidPriorityQueries(searchFilterParameter));

        priorityQueries.AddRange(GetLowPriorityQueries(searchFilterParameter));

        return priorityQueries;
    }

    private List<Query> GetHighestPriorityQueries(SearchFilterParameter searchFilterParameter)
    {
        var priorityQueries = new List<Query>();

        if (searchFilterParameter.StatusPriority && searchFilterParameter.Status?.Length > 0)
        {
            priorityQueries.AddRange(searchFilterParameter.Status.Select(s => CreateBoostedQueryTerm(SearchGateway.StatusField, ((int)s).ToString(CultureInfo.InvariantCulture), options.QueryBoostHighestPriority * 1.5f * options.QueryBoostPriorityMultiplier)));
        }

        if (searchFilterParameter.OfficesPriority && searchFilterParameter.Offices?.Length > 0)
        {
            priorityQueries.AddRange(searchFilterParameter.Offices.Select(o => CreateBoostedQueryTerm(SearchGateway.OfficeIdField, o.ToString(CultureInfo.InvariantCulture), options.QueryBoostHighestPriority * options.QueryBoostPriorityMultiplier)));
        }

        if (searchFilterParameter.ReliabilityPriority && searchFilterParameter.Reliability?.Length > 0)
        {
            priorityQueries.AddRange(searchFilterParameter.Reliability.Select(r => CreateBoostedQueryTerm(SearchGateway.ReliabilityField, ((int)r).ToString(CultureInfo.InvariantCulture), 1.5f * options.QueryBoostHighestPriority * options.QueryBoostPriorityMultiplier)));
        }

        return priorityQueries;
    }

    private List<Query> GetHighPriorityQueries(SearchFilterParameter searchFilterParameter)
    {
        var priorityQueries = new List<Query>();

        // Boost indicates the priority factor, according to AC its: Languages, Status, Collections, Lenoch Classifications..
        if (searchFilterParameter.CollectionsPriority && searchFilterParameter.Collections?.Length > 0)
        {
            priorityQueries.AddRange(searchFilterParameter.Collections.Select(c => CreateBoostedQueryTerm(SearchGateway.CollectionIdField, c.ToString(CultureInfo.InvariantCulture), options.QueryBoostVeryHighPriority * 1.5f * options.QueryBoostPriorityMultiplier)));
        }

        if (searchFilterParameter.SubjectDomainsPriority && searchFilterParameter.SubjectDomains?.Length > 0)
        {
            priorityQueries.AddRange(GetSubjectDomainsBoostedQueries(searchFilterParameter.SubjectDomains, options.QueryBoostVeryHighPriority * options.QueryBoostPriorityMultiplier));
        }

        if (searchFilterParameter.LenochClassificationPriority && searchFilterParameter.LenochClassifications?.Length > 0)
        {
            priorityQueries.AddRange(searchFilterParameter.LenochClassifications.Select(lc => CreateBoostedQueryTerm(SearchGateway.LenochClassificationsField, lc.ToString(CultureInfo.InvariantCulture), 0.5f * options.QueryBoostVeryHighPriority * options.QueryBoostPriorityMultiplier)));
        }

        if (searchFilterParameter.DeletedPriority && searchFilterParameter.Deleted)
        {
            priorityQueries.Add(CreateBoostedQueryTerm(SearchGateway.DeletedField, searchFilterParameter.Deleted.ToString(), options.QueryBoostVeryHighPriority * 0.5f * options.QueryBoostPriorityMultiplier));
        }

        return priorityQueries;
    }

    private List<Query> GetMidPriorityQueries(SearchFilterParameter searchFilterParameter)
    {
        var priorityQueries = new List<Query>();

        if (searchFilterParameter.AdminInfosPriority && searchFilterParameter.AdminInfos?.Length > 0)
        {
            priorityQueries.AddRange(searchFilterParameter.AdminInfos.Select(o => CreateBoostedQueryTerm(SearchGateway.AdminInfosField, o.ToString(CultureInfo.InvariantCulture), options.QueryBoostHighPriority * options.QueryBoostPriorityMultiplier)));
        }

        return priorityQueries;
    }

    private List<Query> GetLowPriorityQueries(SearchFilterParameter searchFilterParameter)
    {
        var priorityQueries = new List<Query>();

        if (searchFilterParameter.CreatedByPriority && !string.IsNullOrEmpty(searchFilterParameter.CreatedBy))
        {
            priorityQueries.Add(CreateBoostedQueryTerm(SearchGateway.CreatedByField, searchFilterParameter.CreatedBy, options.QueryBoostLowPriority * 1.5f * options.QueryBoostPriorityMultiplier));
        }

        if (searchFilterParameter.ModifiedByPriority && !string.IsNullOrEmpty(searchFilterParameter.ModifiedBy))
        {
            priorityQueries.Add(CreateBoostedQueryTerm(SearchGateway.ModifiedByField, searchFilterParameter.ModifiedBy, options.QueryBoostLowPriority * options.QueryBoostPriorityMultiplier));
        }

        return priorityQueries;
    }

    private static BooleanQuery GetPriorityNumericRangeQuery(BooleanQuery allQueries, SearchFilterParameter searchFilterParameter)
    {
        // Boosting query sets all other queries to be lower rated, so this has to happen after adding all the other queries. NumericRangeQuery has no own Boost option.
        if (searchFilterParameter.ModifiedPeriodPriority && searchFilterParameter.ModifiedFrom != null && searchFilterParameter.ModifiedTo != null)
        {
            var startDateTime = ToInt64DateTime(ToIndexDateTime(searchFilterParameter.ModifiedFrom.Value));
            var endDateTime = ToInt64DateTime(ToIndexDateTime(searchFilterParameter.ModifiedTo.Value));

            allQueries.Add(new BoostingQuery(NumericRangeQuery.NewInt64Range(SearchGateway.ModifiedField, startDateTime, endDateTime, true, true), allQueries, 0.1f), Occur.SHOULD);
        }

        return allQueries;
    }

    private static BooleanFilter GetFilterExtended(SearchFilterParameter searchFilterParameter, bool includeDeleted, bool includeDeactivatedCollectionEntries)
    {
        var languageIdArray = searchFilterParameter.SourceLanguageIds?.ToArray() ?? Array.Empty<int>();
        var officeIdArray = searchFilterParameter.Offices?.ToArray() ?? Array.Empty<int>();
        var lenochClassificationIdArray = searchFilterParameter.LenochClassifications?.ToArray() ?? Array.Empty<int>();
        var collectionIdArray = searchFilterParameter.Collections?.ToArray() ?? Array.Empty<int>();
        var relatedCollectionIdArray = searchFilterParameter.RelatedCollections?.ToArray() ?? Array.Empty<int>();
        var glossaryIdArray = searchFilterParameter.Glossaries?.ToArray() ?? Array.Empty<int>();
        var subjectDomainsArray = searchFilterParameter.SubjectDomains?.ToArray() ?? Array.Empty<string>();

        var filter = new BooleanFilter();

        AddFilterIfSet(languageIdArray, () =>
        {
            var sourceLanguageFilter = new TermsFilter(languageIdArray.Select(i => new Term(SearchGateway.LanguageIdField, i.ToString(CultureInfo.InvariantCulture))).ToList());
            filter.Add(sourceLanguageFilter, Occur.MUST);
        }, false);
        AddFilterIfSet(searchFilterParameter.Status, () =>
        {
            var statusFilter = new TermsFilter(searchFilterParameter.Status.Select(i => new Term(SearchGateway.StatusField, ((int)i).ToString(CultureInfo.InvariantCulture))).ToList());
            filter.Add(statusFilter, Occur.MUST);
        }, searchFilterParameter.StatusPriority);
        AddFilterIfSet(officeIdArray, () =>
        {
            var officeFilter = new TermsFilter(officeIdArray.Select(i => new Term(SearchGateway.OfficeIdField, i.ToString(CultureInfo.InvariantCulture))).ToList());
            filter.Add(officeFilter, Occur.MUST);
        }, searchFilterParameter.OfficesPriority);
        AddFilterIfSet(lenochClassificationIdArray, () =>
        {
            var lenochClassificationFilter = new TermsFilter(lenochClassificationIdArray.Select(i => new Term(SearchGateway.LenochClassificationsField, i.ToString(CultureInfo.InvariantCulture))).ToList());
            filter.Add(lenochClassificationFilter, Occur.MUST);
        }, searchFilterParameter.LenochClassificationPriority);
        AddFilterIfSet(collectionIdArray, () =>
        {
            var collectionFilter = new TermsFilter(collectionIdArray.Select(i => new Term(SearchGateway.CollectionIdField, i.ToString(CultureInfo.InvariantCulture))).ToList());
            filter.Add(collectionFilter, searchFilterParameter.CollectionsExclude ? Occur.MUST_NOT : Occur.MUST);
        }, searchFilterParameter.CollectionsPriority, searchFilterParameter.CollectionsExclude);
        AddFilterIfSet(subjectDomainsArray, () =>
        {
            filter.Add(GetSubjectDomainsBooleanFilter(subjectDomainsArray), Occur.MUST);
        }, searchFilterParameter.SubjectDomainsPriority);
        AddFilterIfSet(relatedCollectionIdArray, () =>
        {
            var relatedCollectionFilter = new TermsFilter(relatedCollectionIdArray.Select(i => new Term(SearchGateway.RelatedCollectionsField, i.ToString(CultureInfo.InvariantCulture))).ToList());
            filter.Add(relatedCollectionFilter, Occur.MUST);
        }, false);
        AddFilterIfSet(glossaryIdArray, () =>
        {
            var glossaryFilter = new TermsFilter(glossaryIdArray.Select(i => new Term(SearchGateway.GlossariesField, i.ToString(CultureInfo.InvariantCulture))).ToList());
            filter.Add(glossaryFilter, searchFilterParameter.GlossariesExclude ? Occur.MUST_NOT : Occur.MUST);
        }, false, searchFilterParameter.GlossariesExclude);
        AddFilterIfSet(searchFilterParameter.AdminInfos, () =>
        {
            var adminInfoFilter = new TermsFilter(searchFilterParameter.AdminInfos.Select(i => new Term(SearchGateway.AdminInfosField, i.ToString(CultureInfo.InvariantCulture))).ToList());
            filter.Add(adminInfoFilter, searchFilterParameter.AdminInfosExclude ? Occur.MUST_NOT : Occur.MUST);
        }, searchFilterParameter.AdminInfosPriority, searchFilterParameter.AdminInfosExclude);
        AddFilterIfSet(filter, searchFilterParameter.CreatedBy, SearchGateway.CreatedByField, searchFilterParameter.CreatedByPriority, searchFilterParameter.CreatedByExclude);
        AddFilterIfSet(filter, searchFilterParameter.ModifiedBy, SearchGateway.ModifiedByField, searchFilterParameter.ModifiedByPriority, searchFilterParameter.ModifiedByExclude);
        AddFilterIfSet(searchFilterParameter.Reliability, () =>
        {
            var reliabilityFilter = new TermsFilter(searchFilterParameter.Reliability.Select(i => new Term(SearchGateway.ReliabilityField, ((int)i).ToString(CultureInfo.InvariantCulture))).ToList());
            filter.Add(reliabilityFilter, Occur.MUST);
        }, searchFilterParameter.ReliabilityPriority);
        if (searchFilterParameter.ModifiedFrom != null && searchFilterParameter.ModifiedTo != null && (searchFilterParameter.ModifiedPeriodExclude || !searchFilterParameter.ModifiedPeriodPriority))
        {
            var startDateTime = ToInt64DateTime(ToIndexDateTime(searchFilterParameter.ModifiedFrom.Value));
            var endDateTime = ToInt64DateTime(ToIndexDateTime(searchFilterParameter.ModifiedTo.Value));
            var occur = searchFilterParameter.ModifiedPeriodExclude ? Occur.MUST_NOT : Occur.MUST;
            filter.Add(NumericRangeFilter.NewInt64Range(SearchGateway.ModifiedField, startDateTime, endDateTime, true, true), occur);
        }

        if (searchFilterParameter.Deleted && !searchFilterParameter.DeletedPriority)
        {
            filter.Add(new TermFilter(new Term(SearchGateway.DeletedField, true.ToString())), Occur.MUST);
        }
        else if (!includeDeleted)
        {
            filter.Add(new TermFilter(new Term(SearchGateway.DeletedField, false.ToString())), Occur.MUST);
        }

        if (!includeDeactivatedCollectionEntries)
        {
            filter.Add(new TermFilter(new Term(SearchGateway.CollectionIsActive, true.ToString())), Occur.MUST);
        }

        AddFilterIfSet(searchFilterParameter.CommentStatus, () =>
        {
            var commentStatusFilter = new TermsFilter(searchFilterParameter.CommentStatus.Select(i => new Term(SearchGateway.CommentStatusField, ((int)i).ToString(CultureInfo.InvariantCulture))).ToList());
            filter.Add(commentStatusFilter, Occur.MUST);
        }, false);

        return filter.Clauses.Count == 0 ? new BooleanFilter() : filter;
    }

    private static BooleanFilter GetSubjectDomainsBooleanFilter(string[] subjectDomainsArray)
    {
        var finalFilter = new BooleanFilter();

        foreach (var subjectDomain in subjectDomainsArray)
        {
            var split = subjectDomain.Split('-');

            if (split.Length == 0)
            {
                throw new InvalidOperationException($"Invalid syntax used for search query: {nameof(SearchFilterParameter.SubjectDomains)}");
            }

            var subjectId = split[0];

            var subjectFilter = new TermFilter(new Term(SearchGateway.SubjectsField, subjectId));
            if (split.Length >= 2)
            {
                var domainId = split[1];

                finalFilter.Add(new BooleanFilter
                {
                    { subjectFilter, Occur.MUST },
                    { new TermFilter(new Term(SearchGateway.DomainsField, domainId)), Occur.MUST },
                }, Occur.SHOULD);
            }
            else
            {
                finalFilter.Add(subjectFilter, Occur.SHOULD);
            }
        }

        return finalFilter;
    }

    private static List<Query> GetSubjectDomainsBoostedQueries(string[] subjectDomainsArray, float boost)
    {
        var queries = new List<Query>();

        foreach (var subjectDomain in subjectDomainsArray)
        {
            var split = subjectDomain.Split('-');

            if (split.Length == 0)
            {
                throw new InvalidOperationException($"Invalid syntax used for search query: {nameof(SearchFilterParameter.SubjectDomains)}");
            }

            var subjectId = split[0];

            Query query;
            var subjectQuery = new TermQuery(new Term(SearchGateway.SubjectsField, subjectId));
            if (split.Length >= 2)
            {
                var domainId = split[1];
                query = new BooleanQuery
                {
                    { subjectQuery, Occur.MUST },
                    { new TermQuery(new Term(SearchGateway.DomainsField, domainId)), Occur.MUST },
                };
                query.Boost = boost;
            }
            else
            {
                query = new TermQuery(new Term(SearchGateway.SubjectsField, subjectId))
                {
                    Boost = boost
                };
            }
            queries.Add(query);
        }

        return queries;
    }

    private static void AddFilterIfSet<T>(T[] parameter, Action filterAction, bool priority, bool exclude = false)
    {
        if (parameter != null && parameter.Length > 0 && (exclude || !priority))
        {
            filterAction();
        }
    }

    private static void AddFilterIfSet(BooleanFilter filter, string parameter, string fieldName, bool priority, bool exclude = false)
    {
        if (!string.IsNullOrEmpty(parameter) && (exclude || !priority))
        {
            var occur = exclude ? Occur.MUST_NOT : Occur.MUST;
            filter.Add(new TermFilter(new Term(fieldName, parameter)), occur);
        }
    }

    private static TermQuery CreateBoostedQueryTerm(string field, string value, float boost)
    {
        return new TermQuery(new Term(field, value)) { Boost = boost };
    }

    private static bool IsPrioritySearch(SearchFilterParameter searchFilterParameter)
    {
        return searchFilterParameter.AdminInfosPriority ||
               searchFilterParameter.CollectionsPriority ||
               searchFilterParameter.LenochClassificationPriority ||
               searchFilterParameter.OfficesPriority ||
               searchFilterParameter.StatusPriority ||
               searchFilterParameter.SubjectDomainsPriority ||
               searchFilterParameter.CreatedByPriority ||
               searchFilterParameter.ModifiedByPriority ||
               searchFilterParameter.ModifiedPeriodPriority ||
               searchFilterParameter.ReliabilityPriority ||
               searchFilterParameter.DeletedPriority;
    }

    private static BooleanQuery BuildMissingSourceQuery(SearchFields fields)
    {
        var q = new BooleanQuery();

        if (fields.Terminus)
        {
            q.Add(BuildFieldMissingSource(nameof(Models.EntryLanguage.Terminus), nameof(Models.EntryLanguage.TerminusSource)), Occur.SHOULD);
        }

        if (fields.Abbreviation)
        {
            q.Add(BuildFieldMissingSource(nameof(Models.EntryLanguage.Abbreviation), nameof(Models.EntryLanguage.AbbreviationSource)), Occur.SHOULD);
        }

        if (fields.Name)
        {
            q.Add(BuildFieldMissingSource(nameof(Models.EntryLanguage.Name), nameof(Models.EntryLanguage.NameSource)), Occur.SHOULD);
        }

        if (fields.Note)
        {
            q.Add(BuildFieldMissingSource(nameof(Models.EntryLanguage.Note), nameof(Models.EntryLanguage.NoteSource)), Occur.SHOULD);
        }

        if (fields.Definition)
        {
            q.Add(BuildFieldMissingSource(nameof(Models.EntryLanguage.Definition), nameof(Models.EntryLanguage.DefinitionSource)), Occur.SHOULD);
        }

        if (fields.Phraseology)
        {
            q.Add(BuildFieldMissingSource(nameof(Models.EntryLanguage.Phraseology), nameof(Models.EntryLanguage.PhraseologySource)), Occur.SHOULD);
        }

        if (fields.Context)
        {
            q.Add(BuildFieldMissingSource(nameof(Models.EntryLanguage.Context), nameof(Models.EntryLanguage.ContextSource)), Occur.SHOULD);
        }

        return q;
    }

    private static BooleanQuery BuildFieldMissingSource(string field, string sourceField)
    {
        var q = new BooleanQuery
        {
            { new WildcardQuery(new Term(field, "?*")), Occur.MUST }
        };

        var sourceMissing = new BooleanQuery
        {
            { new MatchAllDocsQuery(), Occur.MUST },
            { new WildcardQuery(new Term(sourceField, "*")), Occur.MUST_NOT }
        };

        q.Add(sourceMissing, Occur.MUST);

        return q;
    }

    private sealed class Recommendation
    {
        public string Value { get; set; }
        public float Score { get; set; }
    }
}
