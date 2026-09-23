using System;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using Bk.Termdat.Business.Services.Interfaces;
using FluentValidation.TestHelper;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
internal class SearchQueryValidatorTests
{
    private readonly IAuthorizationService authenticationService = Substitute.For<IAuthorizationService>();
    private SearchQueryValidator validator;

    [SetUp]
    public void Setup()
    {
        authenticationService.ClearSubstitute();
        validator = new SearchQueryValidator(authenticationService);
    }

    [Test]
    public void ShouldNotHaveErrorWhenPhraseIsNull()
    {
        var model = new SearchQuery { Phrase = default };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(getDetailQuery => getDetailQuery.Phrase);
    }

    [Test]
    public void ShouldHaveErrorWhenPageIndexIsLessThanOne()
    {
        var model = new SearchQuery { PageIndex = default };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(getDetailQuery => getDetailQuery.PageIndex);
    }

    [TestCase(default(int))]
    [TestCase(101)]
    public void ShouldHaveErrorWhenPageSizeIsInvalid(int pageSize)
    {
        var model = new SearchQuery { PageSize = pageSize };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(getDetailQuery => getDetailQuery.PageSize);
    }

    [TestCase(101, false, false, false, true)]
    [TestCase(101, true, false, false, false)]
    [TestCase(101, false, true, false, false)]
    [TestCase(101, false, false, true, false)]
    [TestCase(251, false, false, true, true)]
    [TestCase(5001, true, false, false, true)]
    [TestCase(5001, false, true, false, true)]
    [TestCase(5001, false, false, true, true)]
    [TestCase(5001, false, false, false, true)]
    public void ShouldHaveErrorWhenPageSizeIsInvalidForRole(int pageSize, bool isAdmin, bool isSuperUser, bool isWriter, bool expectPageSizeError)
    {
        authenticationService.IsAdmin().Returns(isAdmin);
        authenticationService.IsSuperUser().Returns(isSuperUser);
        authenticationService.IsWriter().Returns(isWriter);

        var model = new SearchQuery { PageSize = pageSize, PageIndex = 1 };
        validator = new SearchQueryValidator(authenticationService);
        var result = validator.TestValidate(model);

        if (expectPageSizeError)
        {
            result.ShouldHaveValidationErrorFor(getDetailQuery => getDetailQuery.PageSize);
        }
        else
        {
            Assert.That(result.IsValid, Is.EqualTo(true));
        }
    }

    [TestCase(true, false, false, false)]
    [TestCase(false, true, false, true)]
    [TestCase(false, false, true, true)]
    public void Validate_WithDifferentRolesForAdminAttributes_ValidatesCorrectly(bool isAdmin, bool isSuperUser, bool isWriter, bool expectedError)
    {
        authenticationService.IsAdmin().Returns(isAdmin);
        authenticationService.IsSuperUser().Returns(isSuperUser);
        authenticationService.IsWriter().Returns(isWriter);

        var model = new SearchQuery
        {
            PageSize = 100,
            PageIndex = 1,
            ModifiedFrom = DateTime.Today,
            ModifiedTo = DateTime.Today,
            LenochClassifications = new[] { 1 },
            ModifiedPeriodPriority = true,
            LenochClassificationsPriority = true,
        };

        validator = new SearchQueryValidator(authenticationService);
        var result = validator.TestValidate(model);

        if (expectedError)
        {
            result.ShouldHaveValidationErrorFor(searchFilterParameter => searchFilterParameter.ModifiedFrom);
            result.ShouldHaveValidationErrorFor(searchFilterParameter => searchFilterParameter.LenochClassifications);
            result.ShouldHaveValidationErrorFor(searchFilterParameter => searchFilterParameter.ModifiedPeriodPriority);
            result.ShouldHaveValidationErrorFor(searchFilterParameter => searchFilterParameter.LenochClassificationsPriority);
        }
        else
        {
            Assert.That(result.IsValid, Is.True);
        }
    }

    [TestCase(true, false, false, false)]
    [TestCase(false, true, false, false)]
    [TestCase(false, false, true, false)]
    public void Validate_WithDifferentRolesForWriterAttributes_ValidatesCorrectly(bool isAdmin, bool isSuperUser, bool isWriter, bool expectedError)
    {
        authenticationService.IsAdmin().Returns(isAdmin);
        authenticationService.IsSuperUser().Returns(isSuperUser);
        authenticationService.IsWriter().Returns(isWriter);

        var model = new SearchQuery
        {
            PageSize = 100,
            PageIndex = 1,
            CreatedByPriority = true,
            ModifiedByPriority = true,
        };

        validator = new SearchQueryValidator(authenticationService);
        var result = validator.TestValidate(model);

        if (expectedError)
        {
            result.ShouldHaveValidationErrorFor(searchFilterParameter => searchFilterParameter.CreatedByPriority);
            result.ShouldHaveValidationErrorFor(searchFilterParameter => searchFilterParameter.ModifiedByPriority);
        }
        else
        {
            Assert.That(result.IsValid, Is.True);
        }
    }

    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    [TestCase(false, false, true)]
    public void ShouldHaveErrorForSuperUserSpecificAttributesForRole(bool isAdmin, bool isSuperUser, bool isWriter)
    {
        authenticationService.IsAdmin().Returns(isAdmin);
        authenticationService.IsSuperUser().Returns(isSuperUser);
        authenticationService.IsWriter().Returns(isWriter);

        var model = new SearchQuery
        {
            PageSize = 100,
            PageIndex = 1,
            AdminInfos = new[] { 1 },
            AdminInfosPriority = true,
            CommentStatus = new[] { CommentStatus.Empty }
        };

        validator = new SearchQueryValidator(authenticationService);
        var result = validator.TestValidate(model);

        Assert.That(result.IsValid, Is.True);
    }
}
