using System;
using AutoMapper;
using Bk.Termdat.Business.Mapping.Actions;
using Bk.Termdat.Business.Mapping.Profiles;
using Bk.Termdat.Business.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Tests.Mapping.Profiles;

internal class SearchMappingProfileTests
{
    private IMapper mapper;
    private ResourceModels.SearchQuery searchQuery;
    private readonly IAuthorizationService authorizationServiceMock = Substitute.For<IAuthorizationService>();
    private readonly IIdentityService identityServiceMock = Substitute.For<IIdentityService>();

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => authorizationServiceMock);
        services.AddScoped(_ => identityServiceMock);
        services.AddTransient<PopulateModifiedAndCreatedAction>();
        var provider = services.BuildServiceProvider();

        var config = new MapperConfiguration(c =>
        {
            c.ConstructServicesUsing(provider.GetRequiredService);
            c.AddProfile(new SearchMappingProfile());
        }, LoggerFactory.Create(_ => { }));
        mapper = config.CreateMapper();

        searchQuery = new ResourceModels.SearchQuery()
        {
            Phrase = "Test",
            SourceLanguageIds = new[]
            {
                1, 2
            },
            TargetLanguageIds = new[]
            {
                3, 4
            },
            PageIndex = 1,
            PageSize = 50,
            EntryIds = "100,200,300",
            Offices = new[]
            {
                10, 11
            },
            OfficesPriority = true,
            LenochClassifications = new[]
            {
                20, 21
            },
            LenochClassificationsPriority = true,
            Collections = new[]
            {
                30, 31
            },
            CollectionsPriority = true,
            RelatedCollections = new[]
            {
                40, 41
            },
            Glossaries = new[]
            {
                50, 51
            },
            SubjectDomains = new[]
            {
                "40-30", "41-31"
            },
            SubjectDomainsPriority = true,
            Status = new[]
            {
                ResourceModels.Status.Validated
            },
            StatusPriority = true,
            AdminInfos = new[]
            {
               1, 2
            },
            AdminInfosPriority = true,
            CommentStatus = new[]
            {
                ResourceModels.CommentStatus.Filled
            },
            CreatedBy = "CreatedBy",
            CreatedByPriority = true,
            ModifiedBy = "ModifiedBy",
            ModifiedByPriority = true,
            ModifiedFrom = DateTime.Today.AddDays(-5),
            ModifiedTo = DateTime.Today,
            ModifiedPeriodPriority = true
        };
    }

    [TearDown]
    public void TearDown()
    {
        authorizationServiceMock.ClearSubstitute();
    }

    [Test]
    public void ValidateMappingConfiguration()
    {
        var mapperConfig = mapper.ConfigurationProvider;
        mapperConfig.AssertConfigurationIsValid();
    }

    [Test]
    public void ShouldMapSearchFilterParameter()
    {
        var result = mapper.Map<ResourceModels.SearchFilterParameter>(searchQuery);

        Assert.Multiple(() =>
        {
            AssertSearchFilterParameterMapping(result);
            Assert.That(result.CreatedBy, Is.EqualTo(searchQuery.CreatedBy.ToLowerInvariant()));
            Assert.That(result.ModifiedBy, Is.EqualTo(searchQuery.ModifiedBy.ToLowerInvariant()));
        });
    }

    [TestCaseSource(nameof(_searchFieldsTestCases))]
    public void ShouldMapSearchFields(ResourceModels.SearchQueryFields fields)
    {
        searchQuery.Fields = fields;
        var result = mapper.Map<ResourceModels.SearchFields>(searchQuery);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Terminus, Is.EqualTo(fields.Term));
        Assert.That(result.Name, Is.EqualTo(fields.Name));
        Assert.That(result.Abbreviation, Is.EqualTo(fields.Abbreviation));
        Assert.That(result.Phraseology, Is.EqualTo(fields.Phraseology));
        Assert.That(result.Definition, Is.EqualTo(fields.Definition));
        Assert.That(result.Note, Is.EqualTo(fields.Note));
        Assert.That(result.Context, Is.EqualTo(fields.Context));
        Assert.That(result.Source, Is.EqualTo(fields.Source));
        Assert.That(result.Metadata, Is.EqualTo(fields.Metadata));
        Assert.That(result.Country, Is.EqualTo(fields.Country));
        Assert.That(result.Comment, Is.EqualTo(fields.Comment));
    }

    [Test]
    public void ShouldMapSearchFieldsFromNull()
    {
        searchQuery.Fields = null;
        var result = mapper.Map<ResourceModels.SearchFields>(searchQuery);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Terminus, Is.True);
        Assert.That(result.Name, Is.True);
        Assert.That(result.Abbreviation, Is.True);
        Assert.That(result.Phraseology, Is.True);
        Assert.That(result.Definition, Is.False);
        Assert.That(result.Note, Is.False);
        Assert.That(result.Context, Is.False);
        Assert.That(result.Source, Is.False);
        Assert.That(result.Metadata, Is.True);
        Assert.That(result.Country, Is.False);
        Assert.That(result.Comment, Is.False);
    }

    [TestCase(false, false)]
    [TestCase(true, true)]
    public void ShouldMapMetadata(bool metadata, bool expected)
    {
        searchQuery.Fields = new ResourceModels.SearchQueryFields { Metadata = metadata };
        var result = mapper.Map<ResourceModels.SearchFields>(searchQuery);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Metadata, Is.EqualTo(expected));
    }

    [Test]
    public void Map_ForRoleWriter_UpdatesCreatedByAndModifiedByFields()
    {
        const string email = "writer@test.ch";
        authorizationServiceMock.IsWriter().Returns(true);
        identityServiceMock.GetCurrentEmail().Returns(email);

        searchQuery.CreatedByUser = true;
        searchQuery.ModifiedByUser = true;

        var result = mapper.Map<ResourceModels.SearchFilterParameter>(searchQuery);

        Assert.Multiple(() =>
        {
            AssertSearchFilterParameterMapping(result);
            Assert.That(result.CreatedBy, Is.EqualTo(email));
            Assert.That(result.ModifiedBy, Is.EqualTo(email));
        });
    }

    [Test]
    public void Map_WithInvalidEntryIds_ThrowsMappingException()
    {
        searchQuery.EntryIds = "UngültigerWert;Invalid,Geht Nicht";

        Assert.Throws<AutoMapperMappingException>(() => mapper.Map<ResourceModels.SearchFilterParameter>(searchQuery));
    }

    private void AssertSearchFilterParameterMapping(ResourceModels.SearchFilterParameter result)
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Phrase, Is.EqualTo(searchQuery.Phrase));
        Assert.That(result.SourceLanguageIds, Is.EqualTo(searchQuery.SourceLanguageIds));
        Assert.That(result.PageIndex, Is.EqualTo(searchQuery.PageIndex));
        Assert.That(result.PageSize, Is.EqualTo(searchQuery.PageSize));
        Assert.That(result.EntryIds, Is.EqualTo(new[] { 100, 200, 300 }));
        Assert.That(result.Offices, Is.EqualTo(searchQuery.Offices));
        Assert.That(result.OfficesPriority, Is.EqualTo(searchQuery.OfficesPriority));
        Assert.That(result.LenochClassifications, Is.EqualTo(searchQuery.LenochClassifications));
        Assert.That(result.LenochClassificationPriority, Is.EqualTo(searchQuery.LenochClassificationsPriority));
        Assert.That(result.Collections, Is.EqualTo(searchQuery.Collections));
        Assert.That(result.CollectionsPriority, Is.EqualTo(searchQuery.CollectionsPriority));
        Assert.That(result.RelatedCollections, Is.EqualTo(searchQuery.RelatedCollections));
        Assert.That(result.Glossaries, Is.EqualTo(searchQuery.Glossaries));
        Assert.That(result.SubjectDomains, Is.EqualTo(searchQuery.SubjectDomains));
        Assert.That(result.SubjectDomainsPriority, Is.EqualTo(searchQuery.SubjectDomainsPriority));
        Assert.That(result.Status, Is.EqualTo(searchQuery.Status));
        Assert.That(result.StatusPriority, Is.EqualTo(searchQuery.StatusPriority));
        Assert.That(result.AdminInfos, Is.EqualTo(searchQuery.AdminInfos));
        Assert.That(result.AdminInfosPriority, Is.EqualTo(searchQuery.AdminInfosPriority));
        Assert.That(result.CommentStatus, Is.EqualTo(searchQuery.CommentStatus));
        Assert.That(result.CreatedByPriority, Is.EqualTo(searchQuery.CreatedByPriority));
        Assert.That(result.ModifiedByPriority, Is.EqualTo(searchQuery.ModifiedByPriority));
        Assert.That(result.ModifiedFrom, Is.EqualTo(searchQuery.ModifiedFrom));
        Assert.That(result.ModifiedTo, Is.EqualTo(searchQuery.ModifiedTo));
        Assert.That(result.ModifiedPeriodPriority, Is.EqualTo(searchQuery.ModifiedPeriodPriority));
    }

    private static readonly ResourceModels.SearchQueryFields[] _searchFieldsTestCases = {
        new() { Term = true },
        new() { Name = true },
        new() { Abbreviation = true },
        new() { Phraseology = true },
        new() { Definition = true },
        new() { Note = true },
        new() { Context = true },
        new() { Source = true },
        new() { Metadata = true },
        new() { Country = true },
        new() { Comment = true },
        new() { Term = true, Name = true, Abbreviation = true, Phraseology = true, Definition = true, Note = true, Context = true, Source = true,  Metadata = true , Country = true, Comment = true},
    };
}
