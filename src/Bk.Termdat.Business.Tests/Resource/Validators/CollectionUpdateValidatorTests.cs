
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using Bogus;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
internal class CollectionUpdateValidatorTests
{
    private CollectionUpdateValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new CollectionUpdateValidator();
    }

    [Test]
    public void ValidationShouldHaveErrorsForEmptyModel()
    {
        var model = new CollectionUpdate();

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.Id);
        result.ShouldHaveValidationErrorFor(m => m.Code);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionDe);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionFr);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionEn);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionIt);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionRm);
        result.ShouldNotHaveValidationErrorFor(m => m.ActiveApi);
        result.ShouldNotHaveValidationErrorFor(m => m.Active);
        result.ShouldNotHaveValidationErrorFor(m => m.ClassificationId);
        result.ShouldNotHaveValidationErrorFor(m => m.IsObsolete);
    }

    [Test]
    public void ValidationShouldHaveErrorsWhenCodeExceedsMaxLength()
    {
        var faker = new Faker();
        var model = new CollectionUpdate
        {
            Code = faker.Random.String(11)
        };

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.Code);
        result.ShouldNotHaveValidationErrorFor(m => m.ActiveApi);
        result.ShouldNotHaveValidationErrorFor(m => m.Active);
        result.ShouldNotHaveValidationErrorFor(m => m.ClassificationId);
        result.ShouldNotHaveValidationErrorFor(m => m.IsObsolete);
    }

    [Test]
    public void ValidatorShouldHaveValidationErrorsWhenDescriptionExceedsMaxLength()
    {
        var faker = new Faker();
        var model = new CollectionUpdate
        {
            Code = faker.Random.String(1, 5),
            DescriptionDe = faker.Random.String(1001, 1002),
            DescriptionFr = faker.Random.String(1001, 1002),
            DescriptionEn = faker.Random.String(1001, 1002),
            DescriptionIt = faker.Random.String(1001, 1002),
            DescriptionRm = faker.Random.String(1001, 1002),
            Documentation = faker.Random.String(1001, 1002),
            Description = faker.Random.String(1001, 1002)
        };

        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(m => m.Code);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionRm);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionFr);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionIt);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionRm);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionEn);
        result.ShouldHaveValidationErrorFor(m => m.Documentation);
        result.ShouldHaveValidationErrorFor(m => m.Description);
    }

    [Test]
    public void ValidationShouldBeValid()
    {
        var faker = new Faker();
        var model = new CollectionUpdate
        {
            Id = faker.Random.Int(1),
            Code = faker.Random.String(1, 10),
            DescriptionDe = faker.Random.String(1, 200),
            DescriptionFr = faker.Random.String(1, 200),
            DescriptionEn = faker.Random.String(1, 200),
            DescriptionIt = faker.Random.String(1, 200),
            DescriptionRm = faker.Random.String(1, 200),
            Description = faker.Random.String(1, 200)
        };

        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(m => m.Id);
        result.ShouldNotHaveValidationErrorFor(m => m.Code);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionDe);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionFr);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionEn);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionIt);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionRm);
        result.ShouldNotHaveValidationErrorFor(m => m.Documentation);
        result.ShouldNotHaveValidationErrorFor(m => m.Description);
        result.ShouldNotHaveValidationErrorFor(m => m.ActiveApi);
        result.ShouldNotHaveValidationErrorFor(m => m.Active);
        result.ShouldNotHaveValidationErrorFor(m => m.ClassificationId);
        result.ShouldNotHaveValidationErrorFor(m => m.IsObsolete);
    }
}
