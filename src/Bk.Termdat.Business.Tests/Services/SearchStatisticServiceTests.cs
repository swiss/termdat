using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using Status = Bk.Termdat.Business.Resource.Models.Status;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class SearchStatisticServiceTests
{
    private SearchStatisticService searchStatisticService;

    private IStatisticService statisticServiceMock;
    private ILanguageRepository languageRepositoryMock;
    private readonly PrivateOptions optionsMock = new();
    private readonly IOptions<PrivateOptions> privateOptionsMock = Substitute.For<IOptions<PrivateOptions>>();
    private IEnumerable<Language> languages;

    private SearchFilterParameter normalSearch;
    private SearchFilterParameter extendedSearch;
    private SearchFields validFields;
    private SearchFields invalidFields;

    [SetUp]
    public void SetUp()
    {
        statisticServiceMock = Substitute.For<IStatisticService>();
        languageRepositoryMock = Substitute.For<ILanguageRepository>();

        optionsMock.AchOfficeId = 1;
        privateOptionsMock.Value.Returns(optionsMock);

        languages = new List<Language>
        {
            new()
            {
                Id = 1,
                IsoCode = "DE",
                SortIndex = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "pp",
                Modified = DateTime.UtcNow,
                ModifiedBy = "pp",
            },
            new()
            {
                Id = 2,
                IsoCode = "FR",
                SortIndex = 3,
                Created = DateTime.UtcNow,
                CreatedBy = "pp",
                Modified = DateTime.UtcNow,
                ModifiedBy = "pp",
            },
            new()
            {
                Id = 3,
                IsoCode = "IT",
                SortIndex = 4,
                Created = DateTime.UtcNow,
                CreatedBy = "pp",
                Modified = DateTime.UtcNow,
                ModifiedBy = "pp",
            },
            new()
            {
                Id = 4,
                IsoCode = "EN",
                SortIndex = 2,
                Created = DateTime.UtcNow,
                CreatedBy = "pp",
                Modified = DateTime.UtcNow,
                ModifiedBy = "pp",
            }
        };

        normalSearch = new SearchFilterParameter
        {
            Phrase = "NormalSearchIsLogged",
            Offices = new int[] { 1 },
            OfficesPriority = true,
            Status = new Status[] { Status.Validated },
            StatusPriority = true,
            SourceLanguageIds = new int[] { 1, 2, 4 },
            AdminInfos = new int[] { },
            Collections = new int[] { },
            Glossaries = new int[] { },
            RelatedCollections = new int[] { },
            CommentStatus = new CommentStatus[] { },
            LenochClassifications = new int[] { },
            SubjectDomains = new string[] { },
            Reliability = new Business.Resource.Models.Reliability[] { },
            ReliabilityPriority = false,
        };

        extendedSearch = new SearchFilterParameter
        {
            Phrase = "ExtendedSearchWillNotBeLogged",
            Offices = new int[] { 1 },
            Status = new Status[] { Status.Validated },
            AdminInfos = new int[] { 1, 2, 3 },
        };

        validFields = new SearchFields
        {
            Abbreviation = true,
            Name = true,
            Metadata = true,
            Phraseology = true,
            Terminus = true,
        };

        invalidFields = new SearchFields
        {
            Abbreviation = false,
            Name = false,
            Metadata = false,
            Phraseology = false,
            Terminus = false,
            Comment = true,
            Country = true,
            Definition = true,
            Context = true,
            Note = true,
            Source = true,
        };

        languageRepositoryMock.GetAll().Returns(languages);

        searchStatisticService = new SearchStatisticService(statisticServiceMock, languageRepositoryMock, privateOptionsMock);
    }

    [TearDown]
    public void TearDown()
    {
        statisticServiceMock.ClearSubstitute();
        languageRepositoryMock.ClearSubstitute();
        privateOptionsMock.ClearSubstitute();
    }

    [Test]
    public async Task LogUnsuccessfulSearch_WhenCalledWithValidSearch_ShouldCallStatisticService()
    {
        var summary = await searchStatisticService.LogUnsuccessfulSearch(normalSearch, validFields);

        await statisticServiceMock.Received(1).CreateOrUpdateStatistic(summary);

        Assert.That(summary.SearchText, Is.EqualTo("NormalSearchIsLogged"));
        Assert.That(summary.Languages, Is.EqualTo("DE, EN, FR"));
        Assert.That(summary.SearchFields, Is.EqualTo("AB, Na, Md, PH, VE"));
        Assert.That(summary.NumberOfSearches, Is.EqualTo(0));
        Assert.That(summary.DateLastSearch, Is.Not.EqualTo(new DateTime()));
    }

    [Test]
    public async Task LogUnsuccessfulSearch_WhenCalledWithExtendedSearch_ShouldNotCallStatisticService()
    {
        var summary = await searchStatisticService.LogUnsuccessfulSearch(extendedSearch, validFields);

        await statisticServiceMock.Received(0).CreateOrUpdateStatistic(summary);

        Assert.That(summary, Is.Null);
    }

    [Test]
    public async Task LogUnsuccessfulSearch_WhenCalledWithInvalidFields_ShouldNotCallStatisticService()
    {
        var summary = await searchStatisticService.LogUnsuccessfulSearch(normalSearch, invalidFields);

        await statisticServiceMock.Received(0).CreateOrUpdateStatistic(summary);

        Assert.That(summary, Is.Null);
    }

    [Test]
    public async Task LogUnsuccessfulSearch_WhenCalledWithTwoLanguagesOneField_ShouldCallStatisticService()
    {
        normalSearch.SourceLanguageIds = new int[] { 3, 4 };

        validFields.Abbreviation = false;
        validFields.Name = false;
        validFields.Metadata = false;
        validFields.Phraseology = true;
        validFields.Terminus = false;

        var summary = await searchStatisticService.LogUnsuccessfulSearch(normalSearch, validFields);

        await statisticServiceMock.Received(1).CreateOrUpdateStatistic(summary);

        Assert.That(summary.SearchText, Is.EqualTo("NormalSearchIsLogged"));
        Assert.That(summary.Languages, Is.EqualTo("EN, IT"));
        Assert.That(summary.SearchFields, Is.EqualTo("PH"));
        Assert.That(summary.NumberOfSearches, Is.EqualTo(0));
        Assert.That(summary.DateLastSearch, Is.Not.EqualTo(new DateTime()));
    }
}
