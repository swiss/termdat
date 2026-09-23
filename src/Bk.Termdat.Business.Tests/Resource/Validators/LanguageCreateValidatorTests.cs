using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using Bogus;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
public class LanguageCreateValidatorTests
{
    private LanguageCreateValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new LanguageCreateValidator();
    }

    [Test]
    public void ValidationShouldHaveErrorsForEmptyModel()
    {
        var model = new LanguageCreate();

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.IsoCode);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionDe);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionFr);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionEn);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionIt);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionRm);
    }

    [Test]
    public void ValidationShouldHaveErrorsWhenIsoCodeExceedsMaxLength()
    {
        var faker = new Faker();
        var model = new LanguageCreate
        {
            IsoCode = faker.Random.String(3)
        };

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.IsoCode);
    }

    [Test]
    public void ValidatorShouldHaveValidationErrorsWhenDescriptionExceedsMaxLength()
    {
        var faker = new Faker();
        var model = new LanguageCreate
        {
            IsoCode = faker.Random.String(1, 2),
            DescriptionDe = faker.Random.String(1001, 1002),
            DescriptionFr = faker.Random.String(1001, 1002),
            DescriptionEn = faker.Random.String(1001, 1002),
            DescriptionIt = faker.Random.String(1001, 1002),
            DescriptionRm = faker.Random.String(1001, 1002),
        };

        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(m => m.IsoCode);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionRm);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionFr);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionIt);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionRm);
        result.ShouldHaveValidationErrorFor(m => m.DescriptionEn);
    }

    [Test]
    public void ValidationShouldBeValid()
    {
        var faker = new Faker();
        var model = new LanguageCreate
        {
            IsoCode = faker.Random.String(1, 2),
            DescriptionDe = faker.Random.String(1, 200),
            DescriptionFr = faker.Random.String(1, 200),
            DescriptionEn = faker.Random.String(1, 200),
            DescriptionIt = faker.Random.String(1, 200),
            DescriptionRm = faker.Random.String(1, 200)
        };

        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(m => m.IsoCode);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionDe);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionFr);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionEn);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionIt);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionRm);
    }
}
