using Bk.Termdat.Business.Resource.Validators;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
public class DuplicateCheckRequestValidatorTests
{
    private DuplicateCheckRequestValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new DuplicateCheckRequestValidator();
    }

    [Test]
    public void Validate_WhenOnlyCollectionIdIsSet_ShouldBeValid()
    {
        var request = new DuplicateCheckRequest
        {
            LanguageId = 1,
            CollectionId = 1,
            GlossaryId = null
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_WhenOnlyGlossaryIdIsSet_ShouldBeValid()
    {
        var request = new DuplicateCheckRequest
        {
            LanguageId = 1,
            CollectionId = null,
            GlossaryId = 1
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_WhenNeitherIdIsSet_ShouldBeInvalid()
    {
        var request = new DuplicateCheckRequest
        {
            LanguageId = 1,
            CollectionId = null,
            GlossaryId = null
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Test]
    public void Validate_WhenBothIdsAreSet_ShouldBeInvalid()
    {
        var request = new DuplicateCheckRequest
        {
            LanguageId = 1,
            CollectionId = 1,
            GlossaryId = 1
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x);
    }
}
