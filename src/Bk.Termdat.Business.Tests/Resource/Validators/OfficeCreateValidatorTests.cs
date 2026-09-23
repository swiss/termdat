using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using Bogus;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
public class OfficeCreateValidatorTests
{
    private OfficeCreateValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new OfficeCreateValidator();
    }

    [Test]
    public void ValidatorShouldHaveErrorsForEmptyModel()
    {
        var model = new OfficeCreate();

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.Code);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionDe);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionFr);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionIt);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionRm);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionEn);
    }

    [Test]
    public void ValidatorShouldHaveErrorsWhenCodeExceedsMaxLength()
    {
        var faker = new Faker();
        var model = new OfficeCreate
        {
            Code = faker.Random.String(6)
        };

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.Code);
    }

    [Test]
    public void ValidatorShouldHaveValidationErrorsWhenDescriptionExceedsMaxLength()
    {
        var faker = new Faker();
        var model = new OfficeCreate
        {
            Code = faker.Random.String(1, 5),
            DescriptionDe = faker.Random.String(1001, 1002),
            DescriptionFr = faker.Random.String(1001, 1002),
            DescriptionEn = faker.Random.String(1001, 1002),
            DescriptionIt = faker.Random.String(1001, 1002),
            DescriptionRm = faker.Random.String(1001, 1002),
        };

        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(m => m.Code);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionRm);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionFr);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionIt);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionRm);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionEn);
    }

    [Test]
    public void ValidatorShouldBeValid()
    {
        var faker = new Faker();
        var model = new OfficeCreate
        {
            Code = faker.Random.String(1, 5),
            DescriptionDe = faker.Random.String(1, 100),
            DescriptionFr = faker.Random.String(1, 100),
            DescriptionEn = faker.Random.String(1, 100),
            DescriptionIt = faker.Random.String(1, 100),
            DescriptionRm = faker.Random.String(1, 100),
        };

        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(m => m.Code);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionRm);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionFr);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionIt);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionRm);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionEn);
    }
}
