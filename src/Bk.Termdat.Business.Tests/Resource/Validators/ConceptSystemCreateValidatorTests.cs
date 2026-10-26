
using System.Collections.Generic;
using System.Linq;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using Bogus;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
internal class ConceptSystemCreateValidatorTests
{
    private ConceptSystemCreateValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new ConceptSystemCreateValidator();
    }

    [Test]
    public void ValidationShouldHaveErrorsForEmptyModel()
    {
        var model = new ConceptSystemCreate();

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.Title);
        result.ShouldHaveValidationErrorFor(m => m.CollectionId);
    }

    [Test]
    public void ValidationShouldHaveErrorsWhenTitleExceedsMaxLength()
    {
        var faker = new Faker();
        var model = new ConceptSystemCreate
        {
            Title = faker.Random.String(1001, 1002),
            Positions = new List<ConceptSystemPositionCreate>
            {
                new()
                {
                    Value = faker.Random.String(1001, 1002)
                }
            }
        };

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.Title);
        result.ShouldHaveValidationErrorFor("Positions[0].Value");
    }

    [Test]
    public void ValidationShouldBeValid()
    {
        var faker = new Faker();
        var model = new ConceptSystemCreate
        {
            Title = faker.Random.String(1, 200),
            CollectionId = faker.Random.Number(1, 100),
            Positions = Enumerable.Range(0, faker.Random.Number(1, 4)).Select(_ => new ConceptSystemPositionCreate
            {
                Value = faker.Random.String(1, 200)
            }).ToList()
        };

        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(m => m.Title);
        result.ShouldNotHaveValidationErrorFor(m => m.CollectionId);
        result.ShouldNotHaveValidationErrorFor(m => m.Positions);
    }
}
