using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.CrossCutting.Exceptions;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Business.Public.Api.Tests.Services;

[TestFixture]
internal class LanguageValidationServiceTests
{
    private LanguageValidationService languageValidationService;
    private ILanguageRepository languageRepositoryMock;

    private Fixture fixture;

    [SetUp]
    public void SetUp()
    {
        fixture = new();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        languageRepositoryMock = Substitute.For<ILanguageRepository>();

        languageValidationService = new LanguageValidationService(languageRepositoryMock);
    }

    [Test]
    public async Task ValidateLanguageIsoCodeReturnsId()
    {
        var languages = fixture.CreateMany<Language>().ToList();
        var languageIsoCode = fixture.Create<string>();
        languages.First().IsoCode = languageIsoCode;

        languageRepositoryMock.GetAllForValidation().Returns(languages);

        var languageId = await languageValidationService.ValidateLanguageIsoCode(languageIsoCode);

        Assert.That(languageId, Is.EqualTo(languages.First().Id));
    }

    [Test]
    public void ValidateLanguageIsoCodeThrows()
    {
        var languages = fixture.CreateMany<Language>().ToList();
        var languageIsoCode = fixture.Create<string>();

        languageRepositoryMock.GetAllForValidation().Returns(languages);

        Assert.ThrowsAsync<BusinessValidationException>(() => languageValidationService.ValidateLanguageIsoCode(languageIsoCode));
    }
}
