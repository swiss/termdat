using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
internal class GetDetailQueryValidatorTests
{
    private GetDetailQueryValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new GetDetailQueryValidator();
    }

    [Test]
    public void ShouldNotHaveErrorWhenSourceLanguageIsNull()
    {
        var model = new GetDetailQuery { SourceLanguageIds = default };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(getDetailQuery => getDetailQuery.SourceLanguageIds);
    }

    [Test]
    public void ShouldHaveErrorWhenSourceLanguageHasZeroIds()
    {
        var model = new GetDetailQuery { SourceLanguageIds = new[] { default(int) } };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(getDetailQuery => getDetailQuery.SourceLanguageIds);
    }

    [Test]
    public void ShouldNotHaveErrorWhenTargetLanguageIsNull()
    {
        var model = new GetDetailQuery { TargetLanguageIds = default };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(getDetailQuery => getDetailQuery.TargetLanguageIds);
    }

    [Test]
    public void ShouldHaveErrorWhenTargetLanguageHasZeroIds()
    {
        var model = new GetDetailQuery { TargetLanguageIds = new[] { default(int) } };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(getDetailQuery => getDetailQuery.TargetLanguageIds);
    }
}
