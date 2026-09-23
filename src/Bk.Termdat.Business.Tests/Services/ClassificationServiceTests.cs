using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services;
using Bogus;
using NSubstitute;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
public class ClassificationServiceTests
{
    private ClassificationService classificationService;
    private IClassificationRepository classificationRepositoryMock;
    private IMapper mapperMock;
    private IEnumerable<DomainModels.Classification> classifications;

    [SetUp]
    public void SetUp()
    {
        mapperMock = Substitute.For<IMapper>();
        classificationRepositoryMock = Substitute.For<IClassificationRepository>();
        var faker = new Faker<DomainModels.Classification>();
        classifications = Enumerable.Range(1, 4).Select(_ => faker.Generate());

        classificationService = new ClassificationService(mapperMock, classificationRepositoryMock);
    }

    [Test]
    public async Task GetAllCallsRepository()
    {
        classificationRepositoryMock.GetAll().Returns(classifications);
        var mappedClassifications = new List<ResourceModels.ClassificationSummary>();
        mapperMock.Map<IEnumerable<ResourceModels.ClassificationSummary>>(classifications).Returns(mappedClassifications);

        var result = await classificationService.GetAll();

        Assert.That(result, Is.EqualTo(mappedClassifications));
    }
}
