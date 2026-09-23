using System.Collections.Generic;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using Bogus;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
public class EntryUpdateValidatorTests
{
    private EntryUpdateValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new EntryUpdateValidator();
    }

    [Test]
    public void ShouldHaveErrorWhenMaximumLengthExceeded()
    {
        var faker = new Faker();
        var model = new EntryUpdate
        {
            Comment = faker.Random.String(4001)
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(entryUpdate => entryUpdate.Comment);
    }

    [Test]
    public void ShouldHaveErrorWhenCollectionIdIsNotSet()
    {
        var model = new EntryUpdate();
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(entryUpdate => entryUpdate.CollectionId);
    }

    [Test]
    public void ShouldHaveErrorWhenCollectionIdIsDefault()
    {
        var model = new EntryUpdate
        {
            CollectionId = default
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(entryUpdate => entryUpdate.CollectionId);
    }

    [Test]
    public void ShouldHaveErrorWhenOfficeIsNotSet()
    {
        var model = new EntryUpdate();
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(entryUpdate => entryUpdate.OfficeId);
    }

    [Test]
    public void ShouldHaveErrorWhenOfficeIdIsDefault()
    {
        var model = new EntryUpdate
        {
            OfficeId = default
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(entryUpdate => entryUpdate.OfficeId);
    }

    [Test]
    public void ShouldHaveErrorWhenLanguageDetailsIsNull()
    {
        var model = new EntryUpdate();
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(entryUpdate => entryUpdate.LanguageDetails);
    }

    [Test]
    public void ShouldHaveErrorWhenLanguageDetailsIsEmpty()
    {
        var model = new EntryUpdate
        {
            LanguageDetails = new List<EntryModificationLanguageDetail>()
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(entryUpdate => entryUpdate.LanguageDetails);
    }

    [Test]
    public void ShouldHaveErrorWhenLanguageDetailsHasIds()
    {
        var model = new EntryUpdate
        {
            LanguageDetails = new List<EntryModificationLanguageDetail>
            {
                new()
                {
                    Id = 1
                }
            }
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(entryUpdate => entryUpdate.LanguageDetails);
    }

    [TestCase(new[] { 0 })]
    [TestCase(new[] { 1, 2, 0 })]
    public void ShouldHaveErrorWhenLenochClassificationIdsIsInvalid(IList<int> lenochClassificationIds)
    {
        var model = new EntryUpdate
        {
            LenochClassificationIds = lenochClassificationIds
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(entryCreate => entryCreate.LenochClassificationIds);
    }

    [TestCase(null)]
    [TestCase(new int[] { })]
    [TestCase(new[] { 0 })]
    [TestCase(new[] { 1, 2, 0 })]
    public void ShouldHaveErrorWhenSubjectIdsIsInvalid(IList<int> subjectIds)
    {
        var model = new EntryUpdate
        {
            SubjectIds = subjectIds
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(entryCreate => entryCreate.SubjectIds);
    }

    [TestCase(new[] { 0 })]
    [TestCase(new[] { 1, 2, 0 })]
    public void ShouldHaveErrorWhenDomainIdsIsInvalid(IList<int> domainIds)
    {
        var model = new EntryUpdate
        {
            DomainIds = domainIds
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(entryCreate => entryCreate.DomainIds);
    }

    [Test]
    public void ShouldNotHaveErrorForValidModel()
    {
        var model = new EntryUpdate
        {
            Id = 1,
            OfficeId = 1,
            CollectionId = 1,
            ConceptSystemPositionId = 1,
            LanguageDetails = new List<EntryModificationLanguageDetail>
            {
                new()
                {
                    LanguageId = 1,
                    Sequence = 1,
                    Terminus = "Test"
                }
            },
            SubjectIds = new List<int> { 1 },
        };

        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
