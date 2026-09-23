using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using Bk.Termdat.Business.Services.Interfaces;
using FluentValidation.TestHelper;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
internal class SearchFilterParameterValidatorTests
{
    private readonly IAuthorizationService authenticationService = Substitute.For<IAuthorizationService>();
    private SearchFilterParameterValidator validator;

    [SetUp]
    public void Setup()
    {
        authenticationService.ClearSubstitute();
        validator = new SearchFilterParameterValidator();
    }

    [Test]
    public void ShouldHaveErrorWhenPageIndexIsLessThanOne()
    {
        var model = new SearchFilterParameter { PageIndex = default };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(searchFilterParameter => searchFilterParameter.PageIndex);
    }

    [TestCase(default(int))]
    [TestCase(5001)]
    public void ShouldHaveErrorWhenPageSizeIsInvalid(int pageSize)
    {
        var model = new SearchFilterParameter { PageSize = pageSize };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(searchFilterParameter => searchFilterParameter.PageSize);
    }
}
