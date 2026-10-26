using System.Collections.Generic;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using Bogus;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
internal class UserAccountModificationValidatorTests
{
    private UserAccountModificationValidator validator;

    [SetUp]
    public void SetUp()
    {
        validator = new UserAccountModificationValidator();
    }

    [Test]
    public void Validate_ForEmptyModel_ShouldHaveErrors()
    {
        var model = new UserAccountModification();

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.AuthorizedCollectionIds);
    }

    [Test]
    public void Validate_ForEmptyAuthorizedCollectionIds_ShouldHaveErrors()
    {
        var model = new UserAccountModification
        {
            Id = 1,
            AuthorizedCollectionIds = new List<int>(),
            AuthorizedGlossaryIds = new List<int> { 2, 3 },
        };

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.AuthorizedCollectionIds);
    }

    [Test]
    public void Validate_ForInvalidInformation_ShouldHaveError()
    {
        var faker = new Faker();
        var model = new UserAccountModification
        {
            Id = 1,
            AuthorizedCollectionIds = new List<int> { 1 },
            AuthorizedGlossaryIds = new List<int> { 1 },
            Information = faker.Random.String(1001)
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Information);
    }
}
