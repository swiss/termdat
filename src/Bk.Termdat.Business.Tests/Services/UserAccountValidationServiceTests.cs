using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.CrossCutting.Exceptions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class UserAccountValidationServiceTests
{
    private UserAccountValidationService service;

    private readonly ICollectionRepository collectionRepositoryMock = Substitute.For<ICollectionRepository>();
    private readonly IGlossaryRepository glossaryRepositoryMock = Substitute.For<IGlossaryRepository>();

    private UserAccount userAccount;
    private UserAccountModification userAccountModification;

    [SetUp]
    public void SetUp()
    {
        userAccount = new UserAccount
        {
            Id = 1,
            AuthorizedCollections = new List<Collection> { new() { Id = 101, Code = "C01" }, new() { Id = 102, Code = "C02" } },
            AuthorizedGlossaries = new List<Glossary> { new() { Id = 201, Code = "G01" }, new() { Id = 202, Code = "G02" } }
        };
        userAccountModification = new UserAccountModification
        {
            Id = 1,
            AuthorizedCollectionIds = new List<int> { 101, 102 },
            AuthorizedGlossaryIds = new List<int> { 201 }
        };

        service = new UserAccountValidationService(
            collectionRepositoryMock,
            glossaryRepositoryMock
        );
    }

    [TearDown]
    public void TearDown()
    {
        collectionRepositoryMock.ClearSubstitute();
        glossaryRepositoryMock.ClearSubstitute();
    }

    #region collections

    [Test]
    public void ValidateCollections_WithNoneExistingCollection_ThrowsBusinessValidationException()
    {
        collectionRepositoryMock.GetById(101).ReturnsNull();
        Assert.That(() => service.ValidateCollections(userAccountModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public void ValidateCollections_WithInvalidCollection_ThrowsBusinessValidationException()
    {
        var collections = userAccount.AuthorizedCollections.Select(collection => new Collection { Id = collection.Id + 10 }).ToList();
        userAccount.AuthorizedCollections = collections;
        collectionRepositoryMock.GetByIds(userAccount.AuthorizedCollections.Select(y => y.Id)).Returns(collections);

        Assert.That(() => service.ValidateCollections(userAccountModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public async Task ValidateCollections_ValidEntries_LoadsCollections()
    {
        var collectionIds = Enumerable.Range(3, 5).ToList();
        userAccountModification.AuthorizedCollectionIds = collectionIds;
        var collections = collectionIds.Select(id => new Collection { Id = id }).ToList();
        userAccount.AuthorizedCollections = collections;

        collectionRepositoryMock.GetByIds(collectionIds).Returns(collections);

        var actual = await service.ValidateCollections(userAccountModification);

        Assert.That(actual, Is.EqualTo(collections));
        await collectionRepositoryMock.Received()
            .GetByIds(Arg.Is<IEnumerable<int>>(arg => arg.SequenceEqual(collectionIds)));
    }

    [Test]
    public async Task ValidateCollections_ListIsNull_ReturnsEmpty()
    {
        userAccountModification.AuthorizedCollectionIds = null;

        var result = await service.ValidateCollections(userAccountModification);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    #endregion collections

    #region glossaries

    [Test]
    public void ValidateGlossaries_WithNoneExistingCollection_ThrowsBusinessValidationException()
    {
        glossaryRepositoryMock.GetById(101).ReturnsNull();
        Assert.That(() => service.ValidateGlossaries(userAccountModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public void ValidateGlossaries_WithInvalidGlossary_ThrowsBusinessValidationException()
    {
        var glossaries = userAccount.AuthorizedGlossaries.Select(glossary => new Glossary { Id = glossary.Id + 10 }).ToList();
        userAccount.AuthorizedGlossaries = glossaries;
        glossaryRepositoryMock.GetByIds(userAccount.AuthorizedGlossaries.Select(y => y.Id)).Returns(glossaries);

        Assert.That(() => service.ValidateGlossaries(userAccountModification), Throws.InstanceOf<BusinessValidationException>());
    }

    [Test]
    public async Task ValidateGlossaries_ValidEntries_LoadsGlossaries()
    {
        var glossaryIds = Enumerable.Range(3, 5).ToList();
        userAccountModification.AuthorizedGlossaryIds = glossaryIds;
        var glossaries = glossaryIds.Select(id => new Glossary { Id = id }).ToList();
        userAccount.AuthorizedGlossaries = glossaries;

        glossaryRepositoryMock.GetByIds(glossaryIds).Returns(glossaries);

        var actual = await service.ValidateGlossaries(userAccountModification);

        Assert.That(actual, Is.EqualTo(glossaries));
        await glossaryRepositoryMock.Received()
            .GetByIds(Arg.Is<IEnumerable<int>>(arg => arg.SequenceEqual(glossaryIds)));
    }

    [Test]
    public async Task ValidateGlossaries_ListIsNull_ReturnsEmpty()
    {
        userAccountModification.AuthorizedGlossaryIds = null;

        var result = await service.ValidateGlossaries(userAccountModification);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    #endregion glossaries
}
