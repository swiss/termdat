using System;
using System.IO;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Infrastructure.Search.Analyzers;
using J2N.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Directory = Lucene.Net.Store.Directory;

namespace Bk.Termdat.Infrastructure.Search;

public class SearchGateway : ISearchGateway
{
    public LuceneVersion LuceneVersion { get; }
    public Analyzer DefaultAnalyzer { get; protected set; }
    public IndexWriter Writer { get; protected set; }
    public SearcherManager SearcherManager { get; protected set; }

    public const string IdField = nameof(IndexEntry.EntryId);
    public const string OfficeIdField = nameof(IndexEntry.OfficeId);
    public const string LenochClassificationsField = nameof(IndexEntry.LenochClassifications);
    public const string GlossariesField = nameof(IndexEntry.Glossaries);
    public const string ReferencesField = nameof(IndexEntry.References);
    public const string SubjectsField = nameof(IndexEntry.Subjects);
    public const string DomainsField = nameof(IndexEntry.Domains);
    public const string AdminInfosField = nameof(IndexEntry.AdminInfos);
    public const string CollectionIdField = nameof(IndexEntry.CollectionId);
    public const string CollectionIsActive = nameof(IndexEntry.CollectionIsActive);
    public const string RelatedCollectionsField = nameof(IndexEntry.RelatedCollections);
    public const string StatusField = nameof(IndexEntry.Status);
    public const string CommentStatusField = nameof(CommentStatus);
    public const string ReliabilityField = nameof(IndexEntry.Reliability);
    public const string CreatedByField = nameof(IndexEntry.CreatedBy);
    public const string ModifiedByField = nameof(IndexEntry.ModifiedBy);
    public const string ModifiedField = nameof(IndexEntry.Modified);
    public const string LanguageIdField = nameof(IndexEntry.LanguageId);
    public const string SequenceIdField = nameof(IndexEntry.SequenceId);
    public const string TerminusField = nameof(IndexEntry.Terminus);
    public const string TerminusEquivalentsField = nameof(IndexEntry.Terminus) + EquivalentsFieldSuffix;
    public const string TerminusSourceField = nameof(IndexEntry.TerminusSource);
    public const string TerminusSourceEquivalentsField = nameof(IndexEntry.TerminusSource) + EquivalentsFieldSuffix;
    public const string AbbreviationField = nameof(IndexEntry.Abbreviation);
    public const string AbbreviationEquivalentsField = nameof(IndexEntry.Abbreviation) + EquivalentsFieldSuffix;
    public const string AbbreviationSourceField = nameof(IndexEntry.AbbreviationSource);
    public const string AbbreviationSourceEquivalentsField = nameof(IndexEntry.AbbreviationSource) + EquivalentsFieldSuffix;
    public const string NameField = nameof(IndexEntry.Name);
    public const string NameEquivalentsField = nameof(IndexEntry.Name) + EquivalentsFieldSuffix;
    public const string NameSourceField = nameof(IndexEntry.NameSource);
    public const string NameSourceEquivalentsField = nameof(IndexEntry.NameSource) + EquivalentsFieldSuffix;
    public const string PhraseologyField = nameof(IndexEntry.Phraseology);
    public const string PhraseologyEquivalentsField = nameof(IndexEntry.Phraseology) + EquivalentsFieldSuffix;
    public const string PhraseologySourceField = nameof(IndexEntry.PhraseologySource);
    public const string PhraseologySourceEquivalentsField = nameof(IndexEntry.PhraseologySource) + EquivalentsFieldSuffix;
    public const string DefinitionField = nameof(IndexEntry.Definition);
    public const string DefinitionEquivalentsField = nameof(IndexEntry.Definition) + EquivalentsFieldSuffix;
    public const string DefinitionSourceField = nameof(IndexEntry.DefinitionSource);
    public const string DefinitionSourceEquivalentsField = nameof(IndexEntry.DefinitionSource) + EquivalentsFieldSuffix;
    public const string NoteField = nameof(IndexEntry.Note);
    public const string NoteEquivalentsField = nameof(IndexEntry.Note) + EquivalentsFieldSuffix;
    public const string NoteSourceField = nameof(IndexEntry.NoteSource);
    public const string NoteSourceEquivalentsField = nameof(IndexEntry.NoteSource) + EquivalentsFieldSuffix;
    public const string ContextField = nameof(IndexEntry.Context);
    public const string ContextEquivalentsField = nameof(IndexEntry.Context) + EquivalentsFieldSuffix;
    public const string ContextSourceField = nameof(IndexEntry.ContextSource);
    public const string ContextSourceEquivalentsField = nameof(IndexEntry.ContextSource) + EquivalentsFieldSuffix;
    public const string CountryField = nameof(IndexEntry.Country);
    public const string CountryEquivalentsField = nameof(IndexEntry.Country) + EquivalentsFieldSuffix;
    public const string MetadataField = nameof(IndexEntry.Metadata);
    public const string MetadataEquivalentsField = nameof(IndexEntry.Metadata) + EquivalentsFieldSuffix;
    public const string MetadataSourceField = nameof(IndexEntry.MetadataSource);
    public const string MetadataSourceEquivalentsField = nameof(IndexEntry.MetadataSource) + EquivalentsFieldSuffix;
    public const string CommentField = nameof(IndexEntry.Comment);
    public const string DeletedField = nameof(IndexEntry.Deleted);
    public const string TerminusKeywordField = nameof(IndexEntry.Terminus) + KeywordFieldSuffix;
    public const string AbbreviationKeywordField = nameof(IndexEntry.Abbreviation) + KeywordFieldSuffix;
    public const string NameKeywordField = nameof(IndexEntry.Name) + KeywordFieldSuffix;
    public const string PhraseologyKeywordField = nameof(IndexEntry.Phraseology) + KeywordFieldSuffix;
    public const string DefinitionKeywordField = nameof(IndexEntry.Definition) + KeywordFieldSuffix;
    public const string MetadataKeywordField = nameof(IndexEntry.Metadata) + KeywordFieldSuffix;
    public const string ContextKeywordField = nameof(IndexEntry.Context) + KeywordFieldSuffix;
    public const string NoteKeywordField = nameof(IndexEntry.Note) + KeywordFieldSuffix;
    public const string CountryKeywordField = nameof(IndexEntry.Country) + KeywordFieldSuffix;
    public const string LanguageCountSortField = "LanguageCountSort";
    public const string ModifiedSortField = nameof(IndexEntry.Modified) + "Sort";

    protected Directory Directory { get; set; }
    protected Dictionary<string, Analyzer> FieldSpecificAnalyzer { get; set; }
    protected IndexWriterConfig IndexWriterConfig { get; set; }

    protected ILogger<SearchGateway> Logger { get; }

    private const string KeywordFieldSuffix = "Keyword";
    private const string EquivalentsFieldSuffix = "Eq";

    public SearchGateway(ILogger<SearchGateway> logger, IOptions<SearchOptions> searchOptions, string indexName = "termdat_index", Func<string, Directory>? directoryFactory = null)
    {
        ArgumentNullException.ThrowIfNull(searchOptions);

        Logger = logger;
        var options = searchOptions.Value;

        LuceneVersion = LuceneVersion.LUCENE_48;

        FieldSpecificAnalyzer = new(StringComparer.CurrentCultureIgnoreCase)
        {
            { CreatedByField, new UAX29URLEmailAnalyzer(LuceneVersion) },
            { ModifiedByField, new UAX29URLEmailAnalyzer(LuceneVersion) },
            { TerminusEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { AbbreviationEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { NameEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { PhraseologyEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { DefinitionEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { NoteEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { ContextEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { CountryEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { MetadataEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            // Source fields
            { TerminusSourceEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { AbbreviationSourceEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { NameSourceEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { PhraseologySourceEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { DefinitionSourceEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { NoteSourceEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { ContextSourceEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },
            { MetadataSourceEquivalentsField, new CharEquivalentsAnalyzer(LuceneVersion) },

        };

        DefaultAnalyzer = new StandardAnalyzer(LuceneVersion, Lucene.Net.Analysis.Util.CharArraySet.Empty);
#pragma warning disable CA2000
        var perFieldAnalyzerWrapper = new PerFieldAnalyzerWrapper(DefaultAnalyzer, FieldSpecificAnalyzer);
        IndexWriterConfig = new IndexWriterConfig(LuceneVersion, perFieldAnalyzerWrapper)
#pragma warning restore CA2000
        {
            OpenMode = options.RecreateIndexOnStartUp ? OpenMode.CREATE : OpenMode.CREATE_OR_APPEND
        };

        InitializeIndex(indexName, directoryFactory);

        BooleanQuery.MaxClauseCount = int.MaxValue;
    }

    private void InitializeIndex(
        string indexName,
        Func<string, Directory>? directoryFactory)
    {
        if (directoryFactory is null)
        {
            var indexPath = Path.Join(Path.GetTempPath(), indexName);

            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("Creating index in '{IndexPath}'", indexPath);
            }

            var directoryInfo = System.IO.Directory.CreateDirectory(indexPath);
            Directory = FSDirectory.Open(directoryInfo);
        }
        else
        {
            Directory = directoryFactory(indexName);
        }

        Writer = new IndexWriter(Directory, IndexWriterConfig);
        SearcherManager = new SearcherManager(Writer, true, null);
    }

    public void MaybeRefreshSearcherManager()
    {
        Logger.LogInformation("Calling maybe refresh");
        SearcherManager.MaybeRefresh();
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            SearcherManager?.Dispose();
            Writer?.Dispose();
            Directory?.Dispose();
            DefaultAnalyzer?.Dispose();
        }
    }
}
