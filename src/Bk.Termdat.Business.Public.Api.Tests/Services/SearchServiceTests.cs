using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using PrivateApiResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Public.Api.Tests.Services;

[TestFixture]
internal class SearchServiceTests
{
    private SearchService searchService;
    private IEntryRepository entryRepositoryMock;
    private ICollectionRepository collectionRepositoryMock;
    private IClassificationRepository classificationRepositoryMock;
    private ILanguageValidationService languageValidationServiceMock;
    private ISearchApiClient searchApiClientMock;
    private IMapper mapperMock;
    private IOptions<PublicApiOptions> publicApiOptionsMock;
    private ILogger<SearchService> loggerMock;

    private Fixture fixture;
    private PublicApiOptions publicApiOptions;

    [SetUp]
    public void SetUp()
    {
        fixture = new();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        entryRepositoryMock = Substitute.For<IEntryRepository>();
        collectionRepositoryMock = Substitute.For<ICollectionRepository>();
        classificationRepositoryMock = Substitute.For<IClassificationRepository>();
        languageValidationServiceMock = Substitute.For<ILanguageValidationService>();
        searchApiClientMock = Substitute.For<ISearchApiClient>();
        mapperMock = Substitute.For<IMapper>();

        publicApiOptionsMock = Substitute.For<IOptions<PublicApiOptions>>();
        publicApiOptions = fixture.Create<PublicApiOptions>();
        publicApiOptionsMock.Value.Returns(publicApiOptions);

        loggerMock = Substitute.For<ILogger<SearchService>>();
        searchService = new SearchService(
            entryRepositoryMock,
            collectionRepositoryMock,
            classificationRepositoryMock,
            languageValidationServiceMock,
            searchApiClientMock,
            mapperMock,
            publicApiOptionsMock,
            loggerMock);
    }

    [Test]
    public async Task SearchForDetailReturnsResults()
    {
        var searchParameter = fixture.Create<SearchParameter>();
        searchParameter.ReturnType = ReturnType.Detail;

        collectionRepositoryMock.GetAllActiveApi().Returns(searchParameter.CollectionIds.Select(id => new Collection { Id = id }));
        var classifications = searchParameter.ClassificationIds.Select(id => new Classification { Id = id, Code = fixture.Create<string>() }).ToList();
        classifications.First().Code = publicApiOptions.DefaultVariaClassification;
        classificationRepositoryMock.GetAll().Returns(classifications);

        var searchFilterParameter = fixture.Create<PrivateApiResourceModels.SearchFilterParameter>();
        mapperMock.Map<PrivateApiResourceModels.SearchFilterParameter>(searchParameter).Returns(searchFilterParameter);
        var searchFields = fixture.Create<PrivateApiResourceModels.SearchFields>();
        mapperMock.Map<PrivateApiResourceModels.SearchFields>(searchParameter.Field).Returns(searchFields);

        var languageIdIn = fixture.Create<int>();
        var languageIdOut = fixture.Create<int>();
        languageValidationServiceMock.ValidateLanguageIsoCode(searchParameter.InLanguageCode).Returns(languageIdIn);
        languageValidationServiceMock.ValidateLanguageIsoCode(searchParameter.OutLanguageCode).Returns(languageIdOut);

        var searchResult = fixture.Create<PrivateApiResourceModels.SearchResult>();
        searchApiClientMock.Search(searchFilterParameter, searchFields).Returns(searchResult);

        var entries = fixture.CreateMany<Entry>();
        entryRepositoryMock.GetByIdsForApiDetail(searchResult.EntryIds).Returns(entries);

        var entryDetails = fixture.CreateMany<EntryDetail>().ToList();
        mapperMock.Map<IEnumerable<EntryDetail>>(null, null)
            .ReturnsForAnyArgs(entryDetails);

        var result = await searchService.Search(searchParameter);

        await searchApiClientMock.Received(1).Search(searchFilterParameter, searchFields);
        await entryRepositoryMock.Received(1).GetByIdsForApiDetail(searchResult.EntryIds);
        await languageValidationServiceMock.Received(1).ValidateLanguageIsoCode(searchParameter.OutLanguageCode);
        await languageValidationServiceMock.Received(1).ValidateLanguageIsoCode(searchParameter.InLanguageCode);
        Assert.That(searchFilterParameter.SourceLanguageIds, Is.EquivalentTo(new[] { languageIdIn }));
        Assert.That(searchFilterParameter.Offices, Is.EquivalentTo(publicApiOptions.DefaultOfficeIds));
        Assert.That(result, Is.EqualTo(entryDetails));
    }

    [Test]
    public async Task SearchForDetailFiltersTranslations()
    {
        var searchParameter = fixture.Create<SearchParameter>();
        searchParameter.ReturnType = ReturnType.Detail;

        collectionRepositoryMock.GetAllActiveApi().Returns(searchParameter.CollectionIds.Select(id => new Collection { Id = id }));
        var classifications = searchParameter.ClassificationIds.Select(id => new Classification { Id = id, Code = fixture.Create<string>() }).ToList();
        classifications.First().Code = publicApiOptions.DefaultVariaClassification;
        classificationRepositoryMock.GetAll().Returns(classifications);

        var searchFilterParameter = fixture.Create<PrivateApiResourceModels.SearchFilterParameter>();
        mapperMock.Map<PrivateApiResourceModels.SearchFilterParameter>(searchParameter).Returns(searchFilterParameter);
        var searchFields = fixture.Create<PrivateApiResourceModels.SearchFields>();
        mapperMock.Map<PrivateApiResourceModels.SearchFields>(searchParameter.Field).Returns(searchFields);

        var languageIdIn = fixture.Create<int>();
        var languageIdOut = fixture.Create<int>();
        languageValidationServiceMock.ValidateLanguageIsoCode(searchParameter.InLanguageCode).Returns(languageIdIn);
        languageValidationServiceMock.ValidateLanguageIsoCode(searchParameter.OutLanguageCode).Returns(languageIdOut);

        var searchResult = fixture.Create<PrivateApiResourceModels.SearchResult>();
        searchApiClientMock.Search(searchFilterParameter, searchFields).Returns(searchResult);

        var entries = fixture.CreateMany<Entry>().ToList();
        entryRepositoryMock.GetByIdsForApiDetail(searchResult.EntryIds).Returns(entries);

        var inOutOrMax = 0;
        var inLanguages = 0;
        var outLanguages = 0;
        foreach (var t in entries.SelectMany(e => e.Translations))
        {
            switch (inOutOrMax % 3)
            {
                case 0:
                    t.LanguageId = languageIdIn;
                    inLanguages++;
                    break;
                case 1:
                    t.LanguageId = languageIdOut;
                    outLanguages++;
                    break;
                default:
                    t.LanguageId = int.MaxValue;
                    break;
            }

            inOutOrMax++;
        }


        await searchService.Search(searchParameter);

        Assert.That(entries.SelectMany(e => e.Translations).Count(t => t.LanguageId == languageIdIn), Is.EqualTo(inLanguages));
        Assert.That(entries.SelectMany(e => e.Translations).Count(t => t.LanguageId == languageIdOut), Is.EqualTo(outLanguages));
    }

    [Test]
    public async Task SearchForSummaryReturnsResults()
    {
        var searchParameter = fixture.Create<SearchParameter>();
        searchParameter.ReturnType = ReturnType.Summary;

        collectionRepositoryMock.GetAllActiveApi().Returns(searchParameter.CollectionIds.Select(id => new Collection { Id = id }));
        var classifications = searchParameter.ClassificationIds.Select(id => new Classification { Id = id, Code = fixture.Create<string>() }).ToList();
        classifications.First().Code = publicApiOptions.DefaultVariaClassification;
        classificationRepositoryMock.GetAll().Returns(classifications);

        var searchFilterParameter = fixture.Create<PrivateApiResourceModels.SearchFilterParameter>();
        mapperMock.Map<PrivateApiResourceModels.SearchFilterParameter>(searchParameter).Returns(searchFilterParameter);
        var searchFields = fixture.Create<PrivateApiResourceModels.SearchFields>();
        mapperMock.Map<PrivateApiResourceModels.SearchFields>(searchParameter.Field).Returns(searchFields);

        var languageIdIn = fixture.Create<int>();
        var languageIdOut = fixture.Create<int>();
        languageValidationServiceMock.ValidateLanguageIsoCode(searchParameter.InLanguageCode).Returns(languageIdIn);
        languageValidationServiceMock.ValidateLanguageIsoCode(searchParameter.OutLanguageCode).Returns(languageIdOut);

        var searchResult = fixture.Create<PrivateApiResourceModels.SearchResult>();
        searchApiClientMock.Search(searchFilterParameter, searchFields).Returns(searchResult);

        var entries = fixture.CreateMany<Entry>();
        entryRepositoryMock.GetByIdsForApiSummary(searchResult.EntryIds).Returns(entries);

        var entryDetails = fixture.CreateMany<EntryDetail>().ToList();
        mapperMock.Map<IEnumerable<EntryDetail>>(null, null)
            .ReturnsForAnyArgs(entryDetails);

        var result = await searchService.Search(searchParameter);

        await searchApiClientMock.Received(1).Search(searchFilterParameter, searchFields);
        await entryRepositoryMock.Received(1).GetByIdsForApiSummary(searchResult.EntryIds);
        await languageValidationServiceMock.Received(1).ValidateLanguageIsoCode(searchParameter.OutLanguageCode);
        await languageValidationServiceMock.Received(1).ValidateLanguageIsoCode(searchParameter.InLanguageCode);
        Assert.That(searchFilterParameter.SourceLanguageIds, Is.EquivalentTo(new[] { languageIdIn }));
        Assert.That(searchFilterParameter.Offices, Is.EquivalentTo(publicApiOptions.DefaultOfficeIds));
        Assert.That(result, Is.EqualTo(entryDetails));
    }

    [TestCase(false, false, false, false, false, false, false, false, false, false, false)]
    [TestCase(true, false, false, false, false, false, false, false, false, false, false)]
    [TestCase(false, true, false, false, false, false, false, false, false, false, false)]
    [TestCase(false, false, true, false, false, false, false, false, false, false, false)]
    [TestCase(false, false, false, true, false, false, false, false, false, false, false)]
    [TestCase(false, false, false, false, true, false, false, false, false, false, false)]
    [TestCase(false, false, false, false, false, true, false, false, false, false, false)]
    [TestCase(false, false, false, false, false, false, true, false, false, false, false)]
    [TestCase(false, false, false, false, false, false, false, true, false, false, false)]
    [TestCase(false, false, false, false, false, false, false, false, true, false, false)]
    [TestCase(false, false, false, false, false, false, false, false, false, true, false)]
    [TestCase(false, false, false, false, false, false, false, false, false, false, true)]
    [TestCase(true, true, true, true, true, true, true, true, true, true, true)]
    public async Task Search_SummaryWithSelectedSearchFields_ReturnsHitsOfSelectedFields(
        bool terminus,
        bool name,
        bool abbreviation,
        bool phraseology,
        bool definition,
        bool note,
        bool context,
        bool source,
        bool metadata,
        bool country,
        bool comment)
    {
        var searchParameter = fixture.Create<SearchParameter>();
        searchParameter.ReturnType = ReturnType.Summary;

        collectionRepositoryMock.GetAllActiveApi().Returns(searchParameter.CollectionIds.Select(id => new Collection { Id = id }));
        var classifications = searchParameter.ClassificationIds.Select(id => new Classification { Id = id, Code = fixture.Create<string>() }).ToList();
        classifications.First().Code = publicApiOptions.DefaultVariaClassification;
        classificationRepositoryMock.GetAll().Returns(classifications);

        var searchFilterParameter = fixture.Create<PrivateApiResourceModels.SearchFilterParameter>();
        mapperMock.Map<PrivateApiResourceModels.SearchFilterParameter>(searchParameter).Returns(searchFilterParameter);
        var searchFields = new PrivateApiResourceModels.SearchFields
        {
            Terminus = terminus,
            Name = name,
            Abbreviation = abbreviation,
            Phraseology = phraseology,
            Definition = definition,
            Note = note,
            Context = context,
            Source = source,
            Metadata = metadata,
            Country = country,
            Comment = comment
        };
        mapperMock.Map<PrivateApiResourceModels.SearchFields>(searchParameter.Field).Returns(searchFields);

        var languageIdIn = fixture.Create<int>();
        var languageIdOut = fixture.Create<int>();
        languageValidationServiceMock.ValidateLanguageIsoCode(searchParameter.InLanguageCode).Returns(languageIdIn);
        languageValidationServiceMock.ValidateLanguageIsoCode(searchParameter.OutLanguageCode).Returns(languageIdOut);

        var searchResult = fixture.Create<PrivateApiResourceModels.SearchResult>();
        searchApiClientMock.Search(searchFilterParameter, searchFields).Returns(searchResult);

        var entryId = default(int);
        var entries = fixture.CreateMany<Entry>().ToList();
        entries.ForEach(e =>
        {
            e.Id = entryId++;
        });
        entryRepositoryMock.GetByIdsForApiSummary(searchResult.EntryIds).Returns(entries);

        entryId = default;
        var entryDetails = fixture.CreateMany<EntryDetail>().ToList();
        entryDetails.ForEach(e =>
        {
            e.Id = entryId++;
        });

        mapperMock.Map<IEnumerable<EntryDetail>>(null, null)
            .ReturnsForAnyArgs(entryDetails);

        var result = (await searchService.Search(searchParameter)).ToList();

        await searchApiClientMock.Received(1).Search(searchFilterParameter, searchFields);
        await entryRepositoryMock.Received(1).GetByIdsForApiSummary(searchResult.EntryIds);
        await languageValidationServiceMock.Received(1).ValidateLanguageIsoCode(searchParameter.OutLanguageCode);
        await languageValidationServiceMock.Received(1).ValidateLanguageIsoCode(searchParameter.InLanguageCode);
        Assert.That(searchFilterParameter.SourceLanguageIds, Is.EquivalentTo(new[] { languageIdIn }));
        Assert.That(searchFilterParameter.Offices, Is.EquivalentTo(publicApiOptions.DefaultOfficeIds));
        Assert.That(result, Is.EqualTo(entryDetails));
        if (terminus)
        {
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("terminus", StringComparison.Ordinal) && kvp.Value.StartsWith("Terminus", StringComparison.Ordinal)))));
        }
        else
        {
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("terminus", StringComparison.Ordinal) && kvp.Value.StartsWith("Terminus", StringComparison.Ordinal)))));
        }
        if (name)
        {
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("name", StringComparison.Ordinal) && kvp.Value.StartsWith("Name", StringComparison.Ordinal)))));
        }
        else
        {
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("name", StringComparison.Ordinal) && kvp.Value.StartsWith("Name", StringComparison.Ordinal)))));
        }
        if (abbreviation)
        {
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("abbreviation", StringComparison.Ordinal) && kvp.Value.StartsWith("Abbreviation", StringComparison.Ordinal)))));
        }
        else
        {
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("abbreviation", StringComparison.Ordinal) && kvp.Value.StartsWith("Abbreviation", StringComparison.Ordinal)))));
        }
        if (phraseology)
        {
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("phraseology", StringComparison.Ordinal) && kvp.Value.StartsWith("Phraseology", StringComparison.Ordinal)))));
        }
        else
        {
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("phraseology", StringComparison.Ordinal) && kvp.Value.StartsWith("Phraseology", StringComparison.Ordinal)))));
        }
        if (definition)
        {
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("definition", StringComparison.Ordinal) && kvp.Value.StartsWith("Definition", StringComparison.Ordinal)))));
        }
        else
        {
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("definition", StringComparison.Ordinal) && kvp.Value.StartsWith("Definition", StringComparison.Ordinal)))));
        }
        if (note)
        {
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("note", StringComparison.Ordinal) && kvp.Value.StartsWith("Note", StringComparison.Ordinal)))));
        }
        else
        {
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("note", StringComparison.Ordinal) && kvp.Value.StartsWith("Note", StringComparison.Ordinal)))));
        }
        if (context)
        {
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("context", StringComparison.Ordinal) && kvp.Value.StartsWith("Context", StringComparison.Ordinal)))));
        }
        else
        {
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("context", StringComparison.Ordinal) && kvp.Value.StartsWith("Context", StringComparison.Ordinal)))));
        }
        if (source)
        {
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("abbreviationsource", StringComparison.Ordinal) && kvp.Value.StartsWith("AbbreviationSource", StringComparison.Ordinal)))));
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("contextsource", StringComparison.Ordinal) && kvp.Value.StartsWith("ContextSource", StringComparison.Ordinal)))));
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("definitionsource", StringComparison.Ordinal) && kvp.Value.StartsWith("DefinitionSource", StringComparison.Ordinal)))));
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("metadatasource", StringComparison.Ordinal) && kvp.Value.StartsWith("MetadataSource", StringComparison.Ordinal)))));
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("notesource", StringComparison.Ordinal) && kvp.Value.StartsWith("NoteSource", StringComparison.Ordinal)))));
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("namesource", StringComparison.Ordinal) && kvp.Value.StartsWith("NameSource", StringComparison.Ordinal)))));
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("phraseologysource", StringComparison.Ordinal) && kvp.Value.StartsWith("PhraseologySource", StringComparison.Ordinal)))));
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("terminussource", StringComparison.Ordinal) && kvp.Value.StartsWith("TerminusSource", StringComparison.Ordinal)))));
        }
        else
        {
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("abbreviationsource", StringComparison.Ordinal) && kvp.Value.StartsWith("AbbreviationSource", StringComparison.Ordinal)))));
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("contextsource", StringComparison.Ordinal) && kvp.Value.StartsWith("ContextSource", StringComparison.Ordinal)))));
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("definitionsource", StringComparison.Ordinal) && kvp.Value.StartsWith("DefinitionSource", StringComparison.Ordinal)))));
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("metadatasource", StringComparison.Ordinal) && kvp.Value.StartsWith("MetadataSource", StringComparison.Ordinal)))));
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("notesource", StringComparison.Ordinal) && kvp.Value.StartsWith("NoteSource", StringComparison.Ordinal)))));
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("namesource", StringComparison.Ordinal) && kvp.Value.StartsWith("NameSource", StringComparison.Ordinal)))));
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("phraseologysource", StringComparison.Ordinal) && kvp.Value.StartsWith("PhraseologySource", StringComparison.Ordinal)))));
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("terminussource", StringComparison.Ordinal) && kvp.Value.StartsWith("TerminusSource", StringComparison.Ordinal)))));
        }
        if (metadata)
        {
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("metadata", StringComparison.Ordinal) && kvp.Value.StartsWith("Metadata", StringComparison.Ordinal)))));
        }
        else
        {
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("metadata", StringComparison.Ordinal) && kvp.Value.StartsWith("Metadata", StringComparison.Ordinal)))));
        }
        if (country)
        {
            Assert.That(result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("country", StringComparison.Ordinal) && kvp.Value.StartsWith("Country", StringComparison.Ordinal)))));
        }
        else
        {
            Assert.That(!result.All(r => r.Hits.Any(h => h.Any(kvp => kvp.Key.Equals("country", StringComparison.Ordinal) && kvp.Value.StartsWith("Country", StringComparison.Ordinal)))));
        }
        // The entry comment is internal and must never be returned as a hit, even if the comment field was searched
        Assert.That(result.All(r => r.Hits.All(h => !h.ContainsKey("comment"))));
    }

}
