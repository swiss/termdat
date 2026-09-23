using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

public class CollectionValidationServiceTests
{
    private ICollectionValidationService service;
    private readonly IClassificationRepository classificationRepositoryMock = Substitute.For<IClassificationRepository>();
    private CollectionCreate collectionCreate;

    [SetUp]
    public void SetUp()
    {
        service = new CollectionValidationService(classificationRepositoryMock);

        collectionCreate = new CollectionCreate
        {
            ActiveApi = true,
            ClassificationId = 1,
            Code = "1234567890"
        };
    }

    [Test]
    public void ValidateClassificationThrowsBusinessException()
    {
        classificationRepositoryMock.GetById(1).ReturnsNull();
        Assert.That(() => service.ValidateClassification(collectionCreate), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public async Task ValidateClassificationReturnsNull()
    {
        var collectionCreateNull = new CollectionCreate
        {
            ActiveApi = false,
            ClassificationId = null
        };
        var result = await service.ValidateClassification(collectionCreateNull);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ValidateClassificationReturnsClassification()
    {
        var classification = new Classification
        {
            Id = 1
        };

        classificationRepositoryMock.GetById(1).Returns(classification);

        var result = await service.ValidateClassification(collectionCreate);
        Assert.That(result, Is.EqualTo(classification));
    }
}
