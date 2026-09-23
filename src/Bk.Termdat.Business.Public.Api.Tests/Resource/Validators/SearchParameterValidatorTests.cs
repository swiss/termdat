using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Resource.Validators;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Public.Api.Tests.Resource.Validators;

[TestFixture]
public class SearchParameterValidatorTests
{
    private SearchParameterValidator validator;

    [SetUp]
    public void SetUp()
    {
        validator = new SearchParameterValidator();
    }

    [Test]
    public void ShouldNotHaveValidationErrorForMaxEntryCount()
    {
        var model = new SearchParameter();

        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(m => m.MaxEntryCount);
    }

    [TestCase(-1)]
    [TestCase(1001)]
    public void ShouldHaveValidationErrorForMaxEntryCount(int maxEntryCount)
    {
        var model = new SearchParameter
        {
            MaxEntryCount = maxEntryCount
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.MaxEntryCount);
    }
}
