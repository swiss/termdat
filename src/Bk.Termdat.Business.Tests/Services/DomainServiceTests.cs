using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
public class DomainServiceTests
{
    private DomainService domainService;
    private IDomainRepository domainRepositoryMock;
    private readonly IUnitOfWork unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IMapper mapperMock = Substitute.For<IMapper>();
    private readonly ILanguageDescriptionMapper languageDescriptionMapperMock = Substitute.For<ILanguageDescriptionMapper>();
    private readonly IIdentityService identityServiceMock = Substitute.For<IIdentityService>();
    private const string CurrentEmail = "Its@me.ch";
    private DomainModels.Domain domain;
    private DomainModification domainModification;
    private DomainDetail domainDetail;
    private IEnumerable<DomainModels.Domain> domains;
    private IEnumerable<DomainModification> domainModifications;

    [SetUp]
    public void SetUp()
    {
        domainModification = new() { Id = 1, DescriptionDe = "de", DescriptionEn = "en", DescriptionFr = "fr", DescriptionIt = "it", DescriptionRm = "rm", SubjectId = 2 };
        domainDetail = new() { Id = 1, SubjectId = 2, Name = "name", ModifiedBy = CurrentEmail, CreatedBy = CurrentEmail };
        domain = new()
        {
            Id = 1,
            SubjectId = 2,
            Entries = Array.Empty<DomainModels.Entry>()
        };
        domains = new List<DomainModels.Domain>
        {
            domain,
            new () {
                Id = 2,
                SubjectId = 2,
                Entries = Array.Empty<DomainModels.Entry>()
            }
        };
        domainModifications = new List<DomainModification>{
            domainModification, new ()
            {
                Id = 2,
                SubjectId = 2,
                DescriptionDe = "de",
                DescriptionFr = "fr",
                DescriptionIt = "it",
                DescriptionRm = "rm",
                DescriptionEn = "en"
            }
        };

        domainRepositoryMock = Substitute.For<IDomainRepository>();
        domainRepositoryMock.GetById(1).Returns(domain);
        domainRepositoryMock.GetAll().Returns(domains);
        identityServiceMock.GetCurrentEmail().ReturnsForAnyArgs(CurrentEmail);
        mapperMock.Map<DomainModels.Domain>(domainModification).Returns(domain);
        mapperMock.Map<DomainDetail>(domain).Returns(domainDetail);
        mapperMock.Map<IEnumerable<DomainModification>>(domains).Returns(domainModifications);

        domainService = new DomainService(domainRepositoryMock, unitOfWorkMock, mapperMock, identityServiceMock, languageDescriptionMapperMock);
    }

    [TearDown]
    public void TearDown()
    {
        mapperMock.ClearSubstitute();
        domainRepositoryMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
    }

    [Test]
    public async Task ShouldGetAllDescriptions()
    {
        var result = await domainService.GetAllDescriptions();

        await domainRepositoryMock.Received(1).GetAll();
        mapperMock.Received(1).Map<IEnumerable<DomainModification>>(domains);
        Assert.That(result, Is.EqualTo(domainModifications));
    }

    [Test]
    public void CreateShouldThrow()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () => await domainService.Create(null));
    }

    [Test]
    public async Task CreateShouldReturnDomainDetail()
    {
        var result = await domainService.Create(domainModification);

        mapperMock.Received().Map<DomainModels.Domain>(domainModification);
        await languageDescriptionMapperMock.Received().Map<DomainModels.DomainTranslation>(domainModification);
        await domainRepositoryMock.Received().Create(domain);
        await unitOfWorkMock.Received().SaveChangesAsync();
        await domainRepositoryMock.GetById(domain.Id);
        mapperMock.Received().Map<DomainDetail>(domain);

        Assert.That(result, Is.EqualTo(domainDetail));
    }

    [Test]
    public void UpdateShouldThrow()
    {
        domainRepositoryMock.GetById(1).Returns(null as DomainModels.Domain);
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await domainService.Update(domainModification));

        domainModification.Id = null;
        Assert.ThrowsAsync<ArgumentNullException>(async () => await domainService.Update(null));
        Assert.ThrowsAsync<ArgumentNullException>(async () => await domainService.Update(domainModification));
    }

    [Test]
    public async Task UpdateShouldReturnDomainDetail()
    {
        var result = await domainService.Update(domainModification);

        domainRepositoryMock.Received().Update(domain, domain);
        await unitOfWorkMock.Received().SaveChangesAsync();
        await domainRepositoryMock.Received().GetById(domain.Id);
        mapperMock.Received().Map<DomainDetail>(domain);

        Assert.That(result, Is.EqualTo(domainDetail));
    }

    [Test]
    public async Task ShouldDelete()
    {
        domainRepositoryMock.GetEntryCountById(1).Returns(0);

        await domainService.Delete(domain.Id);

        domainRepositoryMock.Received(1).Delete(Arg.Is(domain));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task DeleteShouldThrowIfDomainIsNull()
    {
        domainRepositoryMock.GetById(Arg.Any<int>()).Returns(null as DomainModels.Domain);

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await domainService.Delete(domain.Id));
        domainRepositoryMock.DidNotReceive().Delete(Arg.Any<DomainModels.Domain>());
        await unitOfWorkMock.DidNotReceive().SaveChangesAsync();
    }

    [Test]
    public async Task DeleteShouldThrowIfDomainHasEntries()
    {
        domainRepositoryMock.GetEntryCountById(domain.Id).Returns(1);

        Assert.ThrowsAsync<BusinessValidationException>(async () => await domainService.Delete(domain.Id));
        domainRepositoryMock.DidNotReceive().Delete(Arg.Any<DomainModels.Domain>());
        await unitOfWorkMock.DidNotReceive().SaveChangesAsync();
    }
}
