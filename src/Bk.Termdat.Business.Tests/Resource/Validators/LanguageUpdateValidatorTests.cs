
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using Bogus;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
internal class LanguageUpdateValidatorTests
{
    private LanguageUpdateValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new LanguageUpdateValidator();
    }

    [Test]
    public void ValidationShouldHaveErrorsForEmptyModel()
    {
        var model = new LanguageUpdate();

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.Id);
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
        var model = new LanguageUpdate
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
        var model = new LanguageUpdate
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
        var model = new LanguageUpdate
        {
            Id = faker.Random.Number(1, 10),
            IsoCode = faker.Random.String(1, 2),
            DescriptionDe = faker.Random.String(1, 200),
            DescriptionFr = faker.Random.String(1, 200),
            DescriptionEn = faker.Random.String(1, 200),
            DescriptionIt = faker.Random.String(1, 200),
            DescriptionRm = faker.Random.String(1, 200)
        };

        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(m => m.Id);
        result.ShouldNotHaveValidationErrorFor(m => m.IsoCode);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionDe);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionFr);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionEn);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionIt);
        result.ShouldNotHaveValidationErrorFor(m => m.DescriptionRm);
    }
}
