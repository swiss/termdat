using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using Bogus;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class SubjectServiceTests
{
    private SubjectService subjectService;
    private readonly ISubjectRepository subjectRepositoryMock = Substitute.For<ISubjectRepository>();
    private readonly IDomainRepository domainRepositoryMock = Substitute.For<IDomainRepository>();
    private readonly IMapper mapperMock = Substitute.For<IMapper>();
    private readonly IUnitOfWork unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly ILanguageDescriptionMapper languageDescriptionMapperMock = Substitute.For<ILanguageDescriptionMapper>();
    private readonly IIdentityService identityServiceMock = Substitute.For<IIdentityService>();
    private readonly ILogger<SubjectService> loggerMock = Substitute.For<ILogger<SubjectService>>();
    private readonly IDomainService domainServiceMock = Substitute.For<IDomainService>();

    private IEnumerable<Subject> subjects;
    private IEnumerable<DomainWithEntryCount> domains;
    private IEnumerable<SubjectWithEntryCount> subjectWithEntryCoutns;
    private readonly string currentEmail = "me@me.com";
    private SubjectModification subjectModification;
    private SubjectDetail subjectDetail;

    [SetUp]
    public void SetUp()
    {
        subjectRepositoryMock.ClearSubstitute();
        mapperMock.ClearSubstitute();

        subjects = new List<Subject>
        {
            new() { Id = 1 },
            new() { Id = 2 },
            new() { Id = 3 },
        };
        subjectModification = new SubjectModification
        {
            Id = 1,
            DescriptionDe = "New Subject De",
            DescriptionEn = "New Subject En",
            DescriptionFr = "New Subject Fr",
            DescriptionIt = "New Subject It",
            DescriptionRm = "New Subject Rm",
            Domains = new List<DomainModification>()
        };
        domains = new List<Domain.Models.Domain>
        {
            new() { Id = 1, SubjectId = 1 },
            new() { Id = 2, SubjectId = 1 },
            new() { Id = 3, SubjectId = 1 },
            new() { Id = 4, SubjectId = 2 },
            new() { Id = 5, SubjectId = 2 }
        }.Select(d => new DomainWithEntryCount { Domain = d, EntryCount = d.Id - 1 });

        subjectWithEntryCoutns = subjects.Select(s => new SubjectWithEntryCount { Subject = s, EntryCount = s.Id - 1 });
        subjectDetail = new Faker<SubjectDetail>().Generate();

        subjectRepositoryMock.GetAll().Returns(subjects);
        subjectRepositoryMock.GetAllDetails().Returns(subjectWithEntryCoutns);
        subjectRepositoryMock.GetById(subjects.First().Id).Returns(subjects.First());

        domainRepositoryMock.GetAllDetails().Returns(domains);

        identityServiceMock.GetCurrentEmail().Returns(currentEmail);
        mapperMock.Map<SubjectModification>(subjects.First()).Returns(subjectModification);
        mapperMock.Map<Subject>(subjectModification).Returns(subjects.First());
        mapperMock.Map<SubjectDetail>(Arg.Any<Subject>()).Returns(subjectDetail);
        unitOfWorkMock
            .When(uow => uow.ExecuteInTransactionAsync(Arg.Any<Func<Task>>()))
            .Do(async x => await x.Arg<Func<Task>>()());

        subjectService = new SubjectService(
            subjectRepositoryMock,
            domainRepositoryMock,
            domainServiceMock,
            identityServiceMock,
            unitOfWorkMock,
            mapperMock,
            languageDescriptionMapperMock,
            loggerMock);
    }

    [TearDown]
    public void TearDown()
    {
        subjectRepositoryMock.ClearSubstitute();
        domainRepositoryMock.ClearSubstitute();
        mapperMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
        languageDescriptionMapperMock.ClearSubstitute();
        identityServiceMock.ClearSubstitute();
        loggerMock.ClearSubstitute();
        domainServiceMock.ClearSubstitute();
    }

    [Test]
    public void UpdateShouldThrow()
    {
        subjectRepositoryMock.GetById(1).Returns(null as Subject);
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await subjectService.Update(subjectModification));

        subjectModification.Id = null;
        Assert.ThrowsAsync<ArgumentException>(async () => await subjectService.Update(null));
        Assert.ThrowsAsync<ArgumentException>(async () => await subjectService.Update(subjectModification));
    }

    [Test]
    public async Task UpdateShouldReturnSubjectDetail()
    {
        var result = await subjectService.Update(subjectModification);

        Assert.That(subjectModification, Is.Not.Null);
        Assert.That(subjectModification.Id, Is.Not.Null);
        await subjectRepositoryMock.Received(2).GetById(subjectModification.Id.Value);
        mapperMock.Received(1).Map<Subject>(subjectModification);
        identityServiceMock.Received().GetCurrentEmail();
        await languageDescriptionMapperMock.Received(1).Map<SubjectTranslation>(subjectModification);
        subjectRepositoryMock.Received(1).Update(subjects.First(), subjects.First());
        await unitOfWorkMock.Received(1).SaveChangesAsync();

        Assert.That(result, Is.EqualTo(subjectDetail));
    }

    [Test]
    public async Task UpdateShouldUpdateDomains()
    {
        subjectModification.Domains = new List<DomainModification>()
        {
            new()
            {
                Id = 1,
                DescriptionDe = " De",
                DescriptionEn = "En",
                DescriptionFr = "Fr",
                DescriptionIt = "It",
                DescriptionRm = "Rm"
            },
            new()
            {
                DescriptionDe = " De",
                DescriptionEn = "En",
                DescriptionFr = "Fr",
                DescriptionIt = "It",
                DescriptionRm = "Rm"
            }
        };
        var result = await subjectService.Update(subjectModification);

        await domainServiceMock.Received(1).Update(subjectModification.Domains.First());
        await domainServiceMock.Received(1).Create(subjectModification.Domains.Last());
        await unitOfWorkMock.Received(1).SaveChangesAsync();

        Assert.That(result, Is.EqualTo(subjectDetail));
    }

    [Test]
    public void GetDetailShouldThrow()
    {
        subjectRepositoryMock.GetById(20).Returns(null as Subject);
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await subjectService.GetDetail(20));
    }

    [Test]
    public async Task GetDetailShouldReturnSubjectModification()
    {
        var result = await subjectService.GetDetail(1);
        await subjectRepositoryMock.Received().GetById(1);
        mapperMock.Received().Map<SubjectModification>(subjects.First());

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(subjectModification));
    }

    [Test]
    public async Task CreateShouldReturnOfficeDetail()
    {
        var subjectCreate = new SubjectModification
        {
            DescriptionDe = "New Subject De",
            DescriptionEn = "New Subject En",
            DescriptionFr = "New Subject Fr",
            DescriptionIt = "New Subject It",
            DescriptionRm = "New Subject Rm",
            Domains = new List<DomainModification>()
        };

        var result = await subjectService.Create(subjectCreate);

        await languageDescriptionMapperMock.Received(1).Map<SubjectTranslation>(subjectCreate);
        await subjectRepositoryMock.Received(1).Create(Arg.Any<Subject>());
        await unitOfWorkMock.Received().SaveChangesAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(subjectDetail));
    }

    [Test]
    public async Task CreateShouldCreateDomains()
    {
        subjectModification.Domains = new List<DomainModification>()
        {
            new()
            {
                DescriptionDe = " De",
                DescriptionEn = "En",
                DescriptionFr = "Fr",
                DescriptionIt = "It",
                DescriptionRm = "Rm"
            }
        };
        var result = await subjectService.Create(subjectModification);

        await domainServiceMock.Received(1).Create(subjectModification.Domains.First());
        await unitOfWorkMock.Received(1).SaveChangesAsync();

        Assert.That(result, Is.EqualTo(subjectDetail));
    }

    [Test]
    public async Task ShouldGetAll()
    {
        var mappedSubjects = new List<SubjectSummary>();
        mapperMock.Map<IEnumerable<SubjectSummary>>(subjects).Returns(mappedSubjects);

        var result = await subjectService.GetAll();

        Assert.That(result, Is.EqualTo(mappedSubjects));
    }

    [Test]
    public async Task ShouldGetAllDetails()
    {
        var mappedSubjects = new List<SubjectDetail>
        {
            new(){Id = 1},
            new(){Id = 2},
            new(){Id = 3}
        };
        mapperMock.Map<IEnumerable<SubjectDetail>>(subjectWithEntryCoutns).Returns(mappedSubjects);

        var domains1 = new List<DomainDetail>
        {
            new() {Id = 1, SubjectId = 1, Name = "Domain 1"},
            new() {Id =2, SubjectId = 1, Name = "Domain 2"},
            new() {Id = 3, SubjectId = 1, Name = "Domain 3"}
        };
        var empty = Array.Empty<DomainDetail>();
        mapperMock.Map<IEnumerable<DomainDetail>>(Arg.Any<object>()).Returns(domains1, empty);

        var result = await subjectService.GetAllDetails();

        Assert.That(result, Is.EquivalentTo(mappedSubjects));
        Assert.That(mappedSubjects.First().Domains, Is.EqualTo(domains1));
        Assert.That(mappedSubjects.Skip(1).First().Domains, Is.Empty);
    }

    [Test]
    public async Task ShouldDelete()
    {
        subjectRepositoryMock.GetEntryCountById(subjects.First().Id).Returns(0);

        await subjectService.Delete(subjects.First().Id);

        subjectRepositoryMock.Received(1).Delete(Arg.Is(subjects.First()));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task DeleteShouldThrowIfDomainIsNull()
    {
        subjectRepositoryMock.GetById(Arg.Any<int>()).Returns(null as Subject);

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await subjectService.Delete(subjects.First().Id));
        subjectRepositoryMock.DidNotReceive().Delete(Arg.Any<Subject>());
        await unitOfWorkMock.DidNotReceive().SaveChangesAsync();
    }

    [Test]
    public async Task DeleteShouldThrowIfDomainHasEntries()
    {
        subjectRepositoryMock.GetEntryCountById(subjects.First().Id).Returns(1);

        Assert.ThrowsAsync<BusinessValidationException>(async () => await subjectService.Delete(subjects.First().Id));
        subjectRepositoryMock.DidNotReceive().Delete(Arg.Any<Subject>());
        await unitOfWorkMock.DidNotReceive().SaveChangesAsync();
    }
}
