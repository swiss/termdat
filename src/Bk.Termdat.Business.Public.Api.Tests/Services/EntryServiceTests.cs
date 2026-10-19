using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Business.Public.Api.Tests.Services;

[TestFixture]
public class EntryServiceTests
{
    private IEntryService entryService;
    private IEntryRepository entryRepositoryMock;
    private ILanguageValidationService languageValidationServiceMock;
    private IOptions<PublicApiOptions> publicApiOptionsMock;
    private IMapper mapperMock;
    private ILogger<EntryService> logger;

    private Fixture fixture;
    private PublicApiOptions publicApiOptions;

    [SetUp]
    public void Setup()
    {
        fixture = new();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        entryRepositoryMock = Substitute.For<IEntryRepository>();
        languageValidationServiceMock = Substitute.For<ILanguageValidationService>();
        mapperMock = Substitute.For<IMapper>();
        logger = Substitute.For<ILogger<EntryService>>();

        publicApiOptionsMock = Substitute.For<IOptions<PublicApiOptions>>();
        publicApiOptions = fixture.Create<PublicApiOptions>();
        publicApiOptions.MaximumEntryIds = 100;
        publicApiOptionsMock.Value.Returns(publicApiOptions);

        entryService = new EntryService(entryRepositoryMock, languageValidationServiceMock, publicApiOptionsMock, mapperMock, logger);
    }


    [Test]
    public async Task GetByIdsShouldReturnEntries()
    {
        var entries = GenerateEntries();
        entries.ForEach(e =>
        {
            e.Collection.ActiveApi = true;
            e.OfficeId = publicApiOptions.DefaultOfficeIds.First();
        });
        var mappedEntryDetails = fixture.CreateMany<EntryDetail>();
        mapperMock.Map<IEnumerable<EntryDetail>>(null, null)
            .ReturnsForAnyArgs(mappedEntryDetails);
        var entryIds = fixture.CreateMany<int>().ToArray();

        entryRepositoryMock.GetByIdsForApiDetail(entryIds).Returns(entries);

        var languageIdIn = fixture.Create<int>();
        var languageIdOut = fixture.Create<int>();
        var inLanguageIsoCode = fixture.Create<string>();
        var outLanguageIsoCode = fixture.Create<string>();
        languageValidationServiceMock.ValidateLanguageIsoCode(inLanguageIsoCode).Returns(languageIdIn);
        languageValidationServiceMock.ValidateLanguageIsoCode(outLanguageIsoCode).Returns(languageIdOut);

        var result = (await entryService.GetByIds(entryIds, inLanguageIsoCode, outLanguageIsoCode)).ToList();

        await entryRepositoryMock.Received(1).GetByIdsForApiDetail(entryIds);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Is.EqualTo(mappedEntryDetails));
    }

    [Test]
    public async Task GetByIdsShouldFilterTranslations()
    {
        var entries = GenerateEntries();
        entries.ForEach(e =>
        {
            e.Collection.ActiveApi = true;
            e.OfficeId = publicApiOptions.DefaultOfficeIds.First();
        });
        var entryIds = fixture.CreateMany<int>().ToArray();

        entryRepositoryMock.GetByIdsForApiDetail(entryIds).Returns(entries);

        var languageIdIn = fixture.Create<int>();
        var languageIdOut = fixture.Create<int>();
        var inLanguageIsoCode = fixture.Create<string>();
        var outLanguageIsoCode = fixture.Create<string>();
        languageValidationServiceMock.ValidateLanguageIsoCode(inLanguageIsoCode).Returns(languageIdIn);
        languageValidationServiceMock.ValidateLanguageIsoCode(outLanguageIsoCode).Returns(languageIdOut);

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

        await entryService.GetByIds(entryIds, inLanguageIsoCode, outLanguageIsoCode);

        Assert.That(entries.SelectMany(e => e.Translations).Count(t => t.LanguageId == languageIdIn), Is.EqualTo(inLanguages));
        Assert.That(entries.SelectMany(e => e.Translations).Count(t => t.LanguageId == languageIdOut), Is.EqualTo(outLanguages));
    }

    [Test]
    public async Task GetByIdsShouldFilterOutNonActiveApi()
    {
        var entries = GenerateEntries();
        entries.ForEach(e =>
        {
            e.Collection.ActiveApi = false;
        });
        List<Entry> mappedEntries = null;
        mapperMock.WhenForAnyArgs(x => x.Map<IEnumerable<EntryDetail>>(null, null))
            .Do(x => mappedEntries = ((x[0] as IEnumerable<Entry>) ?? Array.Empty<Entry>()).ToList());
        var entryIds = fixture.CreateMany<int>().ToArray();

        entryRepositoryMock.GetByIdsForApiDetail(entryIds).Returns(entries);

        var languageIdIn = fixture.Create<int>();
        var languageIdOut = fixture.Create<int>();
        var inLanguageIsoCode = fixture.Create<string>();
        var outLanguageIsoCode = fixture.Create<string>();
        languageValidationServiceMock.ValidateLanguageIsoCode(inLanguageIsoCode).Returns(languageIdIn);
        languageValidationServiceMock.ValidateLanguageIsoCode(outLanguageIsoCode).Returns(languageIdOut);

        await entryService.GetByIds(entryIds, inLanguageIsoCode, outLanguageIsoCode);

        Assert.That(mappedEntries, Is.Not.Null);
        Assert.That(mappedEntries, Is.Empty);
    }

    [Test]
    public async Task GetByIdsShouldFilterOutNonDefaultOfficeIds()
    {
        var entries = GenerateEntries();
        entries.ForEach(e =>
        {
            e.OfficeId = publicApiOptions.DefaultOfficeIds.Max() + fixture.Create<int>();
        });
        List<Entry> mappedEntries = null;
        mapperMock.WhenForAnyArgs(x => x.Map<IEnumerable<EntryDetail>>(null, null))
            .Do(x => mappedEntries = ((x[0] as IEnumerable<Entry>) ?? Array.Empty<Entry>()).ToList());
        var entryIds = fixture.CreateMany<int>().ToArray();

        entryRepositoryMock.GetByIdsForApiDetail(entryIds).Returns(entries);

        var languageIdIn = fixture.Create<int>();
        var languageIdOut = fixture.Create<int>();
        var inLanguageIsoCode = fixture.Create<string>();
        var outLanguageIsoCode = fixture.Create<string>();
        languageValidationServiceMock.ValidateLanguageIsoCode(inLanguageIsoCode).Returns(languageIdIn);
        languageValidationServiceMock.ValidateLanguageIsoCode(outLanguageIsoCode).Returns(languageIdOut);

        await entryService.GetByIds(entryIds, inLanguageIsoCode, outLanguageIsoCode);

        Assert.That(mappedEntries, Is.Not.Null);
        Assert.That(mappedEntries, Is.Empty);
    }

    [TestCase(true, Status.Validated)]
    [TestCase(false, Status.InProgress)]
    public async Task GetByIds_DeletedOrNotValidatedEntries_ReturnsNoEntries(bool deleted, Status status)
    {
        var entries = GenerateEntries();
        entries.ForEach(e =>
        {
            e.Collection.ActiveApi = true;
            e.Deleted = deleted;
            e.Status = status;
        });
        List<Entry> mappedEntries = null;
        mapperMock.WhenForAnyArgs(x => x.Map<IEnumerable<EntryDetail>>(null, null))
            .Do(x => mappedEntries = ((x[0] as IEnumerable<Entry>) ?? Array.Empty<Entry>()).ToList());
        var entryIds = fixture.CreateMany<int>().ToArray();

        entryRepositoryMock.GetByIdsForApiDetail(entryIds).Returns(entries);

        var languageIdIn = fixture.Create<int>();
        var languageIdOut = fixture.Create<int>();
        var inLanguageIsoCode = fixture.Create<string>();
        var outLanguageIsoCode = fixture.Create<string>();
        languageValidationServiceMock.ValidateLanguageIsoCode(inLanguageIsoCode).Returns(languageIdIn);
        languageValidationServiceMock.ValidateLanguageIsoCode(outLanguageIsoCode).Returns(languageIdOut);

        await entryService.GetByIds(entryIds, inLanguageIsoCode, outLanguageIsoCode);

        Assert.That(mappedEntries, Is.Not.Null);
        Assert.That(mappedEntries, Is.Empty);
    }

    private static List<Entry> GenerateEntries()
    {
        return Enumerable.Range(1, 3).Select(id => new Entry
        {
            Id = id,
            Status = Status.Validated,
            Collection = new Collection(),
            Translations = Enumerable.Range(1, 3).Select(lid => new EntryLanguage
            {
                Id = lid,
                Language = new Language()
            }).ToList()

        }).ToList();
    }


    [Test]
    public void GetByIdsShouldThrowWithTooManyIds()
    {
        var entryIds = Enumerable.Range(1, publicApiOptions.MaximumEntryIds + 1).ToArray();

        var inLanguageIsoCode = fixture.Create<string>();
        var outLanguageIsoCode = fixture.Create<string>();

        Assert.ThrowsAsync<BusinessValidationException>(() => entryService.GetByIds(entryIds, inLanguageIsoCode, outLanguageIsoCode));
    }

}
