using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
internal class RecommendQueryValidatorTests
{
    private RecommendQueryValidator validator;

    [SetUp]
    public void Setup()
    {
        validator = new RecommendQueryValidator();
    }

    [Test]
    public void ShouldHaveErrorWhenPhraseIsNull()
    {
        var model = new RecommendQuery { Phrase = default };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(getDetailQuery => getDetailQuery.Phrase);
    }

    [TestCase("")]
    [TestCase("1")]
    [TestCase("22")]
    [TestCase("*")]
    [TestCase("?")]
    [TestCase("Kz")]
    public void ShouldHaveErrorWhenPhraseIsInvalid(string phrase)
    {
        var model = new RecommendQuery { Phrase = phrase };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(getDetailQuery => getDetailQuery.Phrase);
    }


    [TestCase("*test")]
    [TestCase("?test")]
    [TestCase("Test")]
    [TestCase("bundes")]
    public void ShouldRecommendWhenPhraseIsValid(string phrase)
    {
        var model = new RecommendQuery { Phrase = phrase };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
