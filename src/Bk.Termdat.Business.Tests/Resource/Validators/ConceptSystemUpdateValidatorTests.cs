using System.Collections.Generic;
using System.Linq;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using Bogus;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
public class ConceptSystemUpdateValidatorTests
{
    private ConceptSystemUpdateValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new ConceptSystemUpdateValidator();
    }

    [Test]
    public void ValidationShouldHaveErrorsForEmptyModel()
    {
        var model = new ConceptSystemUpdate();

        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(m => m.Id);
        result.ShouldHaveValidationErrorFor(m => m.Title);
        result.ShouldHaveValidationErrorFor(m => m.CollectionId);
    }

    [Test]
    public void ValidationShouldHaveErrorsWhenTitleExceedsMaxLength()
    {
        var faker = new Faker();
        var model = new ConceptSystemUpdate
        {
            Title = faker.Random.String(1001, 1002),
            Positions = new List<ConceptSystemPositionUpdate>
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
        var model = new ConceptSystemUpdate
        {
            Id = faker.Random.Number(1, 1000),
            Title = faker.Random.String(1, 200),
            CollectionId = faker.Random.Number(1, 100),
            Positions = Enumerable.Range(0, faker.Random.Number(1, 4)).Select(i => new ConceptSystemPositionUpdate
            {
                Id = i,
                Value = faker.Random.String(1, 200)
            }).ToList()
        };

        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(m => m.Id);
        result.ShouldNotHaveValidationErrorFor(m => m.Title);
        result.ShouldNotHaveValidationErrorFor(m => m.CollectionId);
        result.ShouldNotHaveValidationErrorFor(m => m.Positions);
    }
}
