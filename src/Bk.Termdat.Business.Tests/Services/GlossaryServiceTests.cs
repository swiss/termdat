using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
public class GlossaryServiceTests
{
    private IGlossaryService glossaryService;
    private IGlossaryRepository glossaryRepositoryMock;
    private IMapper mapperMock;
    private IEntryRepository entryRepositoryMock;
    private IIdentityService identityServiceMock;
    private IAuthorizationService authorizationServiceMock;
    private ISearchQueueClient searchQueueClientMock;
    private IUnitOfWork unitOfWorkMock;
    private ILanguageDescriptionMapper languageDescriptionMapperMock;
    private IList<DomainModels.Glossary> glossaries;
    private DomainModels.Glossary glossary;
    private GlossaryDetail glossaryDetail;
    private GlossaryCreate glossaryCreate;
    private GlossaryUpdate glossaryUpdate;
    private const string CurrentEmail = "Its@me.ch";

    [SetUp]
    public void SetUp()
    {
        mapperMock = Substitute.For<IMapper>();
        glossaryRepositoryMock = Substitute.For<IGlossaryRepository>();
        entryRepositoryMock = Substitute.For<IEntryRepository>();
        identityServiceMock = Substitute.For<IIdentityService>();
        authorizationServiceMock = Substitute.For<IAuthorizationService>();
        searchQueueClientMock = Substitute.For<ISearchQueueClient>();
        unitOfWorkMock = Substitute.For<IUnitOfWork>();
        languageDescriptionMapperMock = Substitute.For<ILanguageDescriptionMapper>();

        glossaries = new List<DomainModels.Glossary>
        {
            new()
            {
                Id = 1,
                Code = "123",
                Description = "Description",
            },
            new()
            {
                Id = 2,
                Code = "abc",
                Description = "Description 2",
            }
        };

        glossaryCreate = new()
        {
            Code = "123",
            DescriptionDe = "de",
            DescriptionEn = "en",
            DescriptionFr = "fr",
            DescriptionIt = "it",
            DescriptionRm = "rm",
            Description = "description",
        };

        glossaryUpdate = new()
        {
            Id = 1,
            Code = "123",
            DescriptionDe = "de",
            DescriptionEn = "en",
            DescriptionFr = "fr",
            DescriptionIt = "it",
            DescriptionRm = "rm",
            Description = "description",
        };

        glossary = new()
        {
            Id = 1,
            Code = glossaryCreate.Code,
            Description = glossaryCreate.Description,
            Entries = new List<DomainModels.Entry>
            {
                new() { Id = 10 },
                new() { Id = 11 }
            }
        };

        glossaryDetail = new()
        {
            Id = glossary.Id,
            Code = glossary.Code,
            CreatedBy = CurrentEmail,
            Description = glossary.Description,
            EntryCount = 0,
            Name = "de"
        };

        identityServiceMock.GetCurrentEmail().ReturnsForAnyArgs(CurrentEmail);
        mapperMock.Map<DomainModels.Glossary>(glossaryCreate).Returns(glossary);
        languageDescriptionMapperMock.Map<DomainModels.GlossaryTranslation>(glossaryCreate).Returns(Array.Empty<DomainModels.GlossaryTranslation>());
        glossaryRepositoryMock.GetById(1).Returns(glossary);
        mapperMock.Map<GlossaryDetail>(glossary).Returns(glossaryDetail);
        mapperMock.Map<GlossaryUpdate>(glossary).Returns(glossaryUpdate);

        identityServiceMock.GetCurrentEmail().ReturnsForAnyArgs(CurrentEmail);

        glossaryService = new GlossaryService(
            glossaryRepositoryMock,
            entryRepositoryMock,
            identityServiceMock,
            searchQueueClientMock,
            unitOfWorkMock,
            languageDescriptionMapperMock,
            mapperMock,
            authorizationServiceMock,
            Substitute.For<ILogger<GlossaryService>>()
        );
    }

    [TearDown]
    public void TearDown()
    {
        glossaryRepositoryMock.ClearSubstitute();
        entryRepositoryMock.ClearSubstitute();
        identityServiceMock.ClearSubstitute();
        searchQueueClientMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
        languageDescriptionMapperMock.ClearSubstitute();
        authorizationServiceMock.ClearSubstitute();
        mapperMock.ClearSubstitute();
    }

    [Test]
    public async Task GetAllDetailsShouldReturnGlossaryDetails()
    {
        var glossaryWithEntryCount = glossaries.Select(g => new GlossaryWithEntryCount
        {
            Glossary = g,
            EntryCount = g.Id == 1 ? 12 : 0
        });
        glossaryRepositoryMock.GetAllDetails().Returns(glossaryWithEntryCount);
        var glossaryDetails = new List<GlossaryDetail> {
            new()
            {
                Code = glossaries.First().Code,
                Description = glossaries.First().Description,
                EntryCount = 12
            },
            new()
            {
                Code = glossaries.Last().Code,
                Description = glossaries.Last().Description,
                EntryCount = 0
            }
        };
        mapperMock.Map<IEnumerable<GlossaryDetail>>(glossaryWithEntryCount).Returns(glossaryDetails);

        var result = (await glossaryService.GetAllDetails()).ToList();
        Assert.That(result, Is.EqualTo(glossaryDetails));
    }

    [Test]
    public async Task GetAllReturnsAllGlossaries()
    {
        glossaryRepositoryMock.GetAll().Returns(glossaries);
        var mappedGlossaries = new List<GlossarySummary>();
        mapperMock.Map<IEnumerable<GlossarySummary>>(glossaries).Returns(mappedGlossaries);

        var result = await glossaryService.GetAll();

        Assert.That(result, Is.EqualTo(mappedGlossaries));
    }

    [Test]
    public async Task CreateShouldReturnGlossaryDetail()
    {
        var result = await glossaryService.Create(glossaryCreate);
        await languageDescriptionMapperMock.Received().Map<DomainModels.GlossaryTranslation>(glossaryCreate);
        await glossaryRepositoryMock.Received().Create(glossary);
        await unitOfWorkMock.Received().SaveChangesAsync();
        Assert.That(result, Is.EqualTo(glossaryDetail));
        Assert.That(glossary.ModifiedBy, Is.EqualTo(CurrentEmail));
        Assert.That(glossary.CreatedBy, Is.EqualTo(CurrentEmail));
    }

    [Test]
    public async Task UpdateShouldThrowEntityNotFoundException()
    {
        glossaryRepositoryMock.GetById(glossaryUpdate.Id).Returns(Task.FromResult<DomainModels.Glossary>(null));
        await unitOfWorkMock.DidNotReceive().SaveChangesAsync();
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await glossaryService.Update(glossaryUpdate));
    }

    [Test]
    public async Task UpdateShouldReturnGlossaryDetail()
    {
        mapperMock.Map<DomainModels.Glossary>(glossaryUpdate).Returns(glossary);
        mapperMock.Map<GlossaryDetail>(glossary).Returns(glossaryDetail);

        var result = await glossaryService.Update(glossaryUpdate);

        await languageDescriptionMapperMock.Received().Map<DomainModels.GlossaryTranslation>(glossaryUpdate);
        glossaryRepositoryMock.Received().Update(glossary, glossary);
        await unitOfWorkMock.Received().SaveChangesAsync();

        Assert.That(result, Is.EqualTo(glossaryDetail));
        Assert.That(glossary.ModifiedBy, Is.EqualTo(CurrentEmail));
    }

    [Test]
    public async Task GetDetailShouldThrowEntityNotFoundException()
    {
        glossaryRepositoryMock.GetById(0).Returns(Task.FromResult<DomainModels.Glossary>(null));
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await glossaryService.GetDetail(0));

        await glossaryRepositoryMock.Received().GetById(0);
    }

    [Test]
    public async Task GetDetailShouldReturnGlossaryUpdate()
    {
        var result = await glossaryService.GetDetail(1);

        Assert.That(result, Is.EqualTo(glossaryUpdate));

        await glossaryRepositoryMock.Received().GetById(1);
    }

    [Test]
    public async Task GetAllForModification_ForWriter_ReturnsOnlyAuthorized()
    {
        var authorizedIds = new[] { 1, 2 };
        authorizationServiceMock.IsWriter().Returns(true);
        authorizationServiceMock.GetAuthorizedGlossaryIds().Returns(authorizedIds);
        glossaryRepositoryMock.GetAuthorizedByIds(Arg.Any<IEnumerable<int>>()).Returns(glossaries);

        var mappedGlossaries = new List<GlossarySummary>();
        mapperMock.Map<IEnumerable<GlossarySummary>>(glossaries).Returns(mappedGlossaries);

        var result = await glossaryService.GetAllForModification();
        Assert.That(result, Is.EqualTo(mappedGlossaries));

        authorizationServiceMock.Received(1).GetAuthorizedGlossaryIds();
        await glossaryRepositoryMock.Received(1).GetAuthorizedByIds(authorizedIds);
    }

    [Test]
    public async Task GetAllForModification_ForOtherRoles_ReturnsOnlyAuthorized()
    {
        authorizationServiceMock.IsWriter().Returns(false);
        glossaryRepositoryMock.GetAll().Returns(glossaries);

        var mappedGlossaries = new List<GlossarySummary>();
        mapperMock.Map<IEnumerable<GlossarySummary>>(glossaries).Returns(mappedGlossaries);

        var result = await glossaryService.GetAllForModification();
        Assert.That(result, Is.EqualTo(mappedGlossaries));

        authorizationServiceMock.Received(0).GetAuthorizedGlossaryIds();
        await glossaryRepositoryMock.Received(0).GetAuthorizedByIds(Arg.Any<IEnumerable<int>>());
    }
}
