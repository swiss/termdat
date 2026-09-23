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
using Bk.Termdat.CrossCutting.Exceptions;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Business.Public.Api.Tests.Services;

[TestFixture]
public class ClassificationServiceTests
{
    private ClassificationService classificationService;
    private IMapper mapperMock;
    private IClassificationRepository classificationRepositoryMock;
    private ILanguageValidationService languageValidationServiceMock;
    private Fixture fixture;

    [SetUp]
    public void SetUp()
    {
        classificationRepositoryMock = Substitute.For<IClassificationRepository>();
        languageValidationServiceMock = Substitute.For<ILanguageValidationService>();
        mapperMock = Substitute.For<IMapper>();
        classificationService = new ClassificationService(classificationRepositoryMock, languageValidationServiceMock, mapperMock);

        fixture = new();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Test]
    public async Task GetAllReturnsClassificationSummaries()
    {
        var classifications = Enumerable.Range(1, 3).Select(id => new Classification { Id = id });
        classificationRepositoryMock.GetAll().Returns(classifications);

        var isoCode = fixture.Create<string>();
        var languageId = fixture.Create<int>();
        languageValidationServiceMock.ValidateLanguageIsoCode(isoCode).Returns(languageId);

        var classificationSummaries = fixture.CreateMany<ClassificationSummary>();
        mapperMock.Map(classifications, Arg.Any<Action<IMappingOperationOptions<object, IEnumerable<ClassificationSummary>>>>())
            .Returns(classificationSummaries);

        var result = await classificationService.GetAll(isoCode);

        await classificationRepositoryMock.Received(1).GetAll();
        Assert.That(result, Is.EqualTo(classificationSummaries));
    }

    [Test]
    public async Task GetAllThrowsWithInvalidLanguage()
    {
        var isoCode = fixture.Create<string>();
        languageValidationServiceMock.When(x => x.ValidateLanguageIsoCode(isoCode)).Do(_ => throw new BusinessValidationException(string.Empty, string.Empty));

        Assert.ThrowsAsync<BusinessValidationException>(() => classificationService.GetAll(isoCode));
        await classificationRepositoryMock.DidNotReceive().GetAll();
    }

}
