using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services;
using NSubstitute;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
public class LenochClassificationServiceTests
{
    private LenochClassificationService lenochClassificationService;
    private ILenochClassificationRepository lenochClassificationRepositoryMock;
    private ILenochClassificationSubjectDomainMappingRepository lenochClassificationSubjectDomainMappingRepositoryMock;
    private IMapper mapperMock;
    private IEnumerable<DomainModels.LenochClassification> lenochClassifications;
    private IEnumerable<DomainModels.LenochClassificationSubjectDomainMapping> mappings;

    [SetUp]
    public void SetUp()
    {
        mapperMock = Substitute.For<IMapper>();
        lenochClassificationRepositoryMock = Substitute.For<ILenochClassificationRepository>();
        lenochClassificationSubjectDomainMappingRepositoryMock = Substitute.For<ILenochClassificationSubjectDomainMappingRepository>();
        lenochClassifications = new List<DomainModels.LenochClassification>
        {
            new() { Id = 1 },
            new() { Id = 2 },
            new() { Id = 3 },
            new() { Id = 4 },

        };
        mappings = new List<DomainModels.LenochClassificationSubjectDomainMapping>
        {
            new() { Id = 1, LenochClassificationId = 1, SubjectId = 1 },
            new() { Id = 2, LenochClassificationId = 1, SubjectId = 1, DomainId = 1},
            new() { Id = 3, LenochClassificationId = 2, SubjectId = 2, DomainId = 2},
            new() { Id = 4, LenochClassificationId = 2, SubjectId = 2, DomainId = 3},
        };

        lenochClassificationService = new LenochClassificationService(lenochClassificationRepositoryMock, lenochClassificationSubjectDomainMappingRepositoryMock, mapperMock);
    }

    [Test]
    public async Task ShouldReturnAllLenochClassifications()
    {
        lenochClassificationRepositoryMock.GetAll().Returns(lenochClassifications);
        var mappedLenochClassifications = new List<ResourceModels.LenochClassificationSummary> { new() };
        mapperMock.Map<IEnumerable<ResourceModels.LenochClassificationSummary>>(lenochClassifications).Returns(mappedLenochClassifications);

        var result = await lenochClassificationService.GetAll();

        Assert.That(result, Is.EqualTo(mappedLenochClassifications));
        await lenochClassificationRepositoryMock.Received(1).GetAll();
        mapperMock.Received(1).Map<IEnumerable<ResourceModels.LenochClassificationSummary>>(Arg.Is(lenochClassifications));
    }

    [Test]
    public async Task ShouldReturnAllLenochClassificationDetails()
    {
        lenochClassificationRepositoryMock.GetAllDetails().Returns(lenochClassifications.Select(lc => new EntityWithEntryCount<DomainModels.LenochClassification> { Entity = lc, EntryCount = 1 }));
        lenochClassificationSubjectDomainMappingRepositoryMock.GetAll().Returns(mappings);
        mapperMock.Map<ResourceModels.LenochClassificationDetail>(Arg.Any<DomainModels.LenochClassificationSubjectDomainMapping>()).Returns(x => new ResourceModels.LenochClassificationDetail
        {
            Id = x.Arg<DomainModels.LenochClassificationSubjectDomainMapping>().LenochClassificationId,
            Code = x.Arg<DomainModels.LenochClassificationSubjectDomainMapping>().LenochClassificationId.ToString()
        });
        mapperMock.Map<ResourceModels.LenochClassificationDetail>(Arg.Any<EntityWithEntryCount<DomainModels.LenochClassification>>()).Returns(x => new ResourceModels.LenochClassificationDetail
        {
            Id = x.Arg<EntityWithEntryCount<DomainModels.LenochClassification>>().Entity.Id,
            Code = x.Arg<EntityWithEntryCount<DomainModels.LenochClassification>>().Entity.Id.ToString()
        });

        var result = (await lenochClassificationService.GetAllDetails()).ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(6));
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[1].Id, Is.EqualTo(1));
        Assert.That(result[2].Id, Is.EqualTo(2));
        Assert.That(result[3].Id, Is.EqualTo(2));
        Assert.That(result[4].Id, Is.EqualTo(3));
        Assert.That(result[5].Id, Is.EqualTo(4));
        await lenochClassificationRepositoryMock.Received(1).GetAllDetails();
        await lenochClassificationSubjectDomainMappingRepositoryMock.Received(1).GetAll();
    }
}
