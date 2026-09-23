using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class LanguageServiceTests
{
    private LanguageService languageService;

    private readonly ILanguageRepository languageRepositoryMock = Substitute.For<ILanguageRepository>();
    private readonly IMapper mapperMock = Substitute.For<IMapper>();
    private readonly IIdentityService identityServiceMock = Substitute.For<IIdentityService>();
    private readonly IUnitOfWork unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private ILanguageTranslationMapper languageTranslationMapperMock;
    private IEnumerable<Language> languages;
    private Language language;
    private LanguageCreate languageCreate;
    private LanguageUpdate languageUpdate;
    private LanguageDetail languageDetail;

    [SetUp]
    public void Setup()
    {
        var email = "test@tester.ch";

        languageTranslationMapperMock = Substitute.For<ILanguageTranslationMapper>();
        languageService = new LanguageService(mapperMock, languageRepositoryMock, identityServiceMock, unitOfWorkMock, languageTranslationMapperMock);

        languages = new List<Language>
        {
            new()
            {
                Id = 1,
                IsoCode = "de",
                SortIndex = 1,
                IsUiLanguage = true,
                IsSystemLanguage = true,
                Translations = new List<LanguageTranslation>
                {
                    new() { Id = 1, Name = "Deutsch", LanguageId = 1 },
                    new() { Id = 2, Name = "Allemand", LanguageId = 2 },
                    new() { Id = 3, Name = "Tedesco", LanguageId = 3 },
                    new() { Id = 4, Name = "German", LanguageId = 4 },
                },
            },
            new()
            {
                Id = 2,
                IsoCode = "en",
                SortIndex = 1,
                IsUiLanguage = true,
                IsSystemLanguage = true,
                Translations = new List<LanguageTranslation>
                {
                    new() { Id = 5, Name = "Englisch", LanguageId = 1 },
                    new() { Id = 6, Name = "Anglais", LanguageId = 2 },
                    new() { Id = 7, Name = "Inglese", LanguageId = 3 },
                    new() { Id = 8, Name = "English", LanguageId = 4 },
                },
            },
            new()
            {
                Id = 20,
                IsoCode = "ru",
                SortIndex = 23,
                IsUiLanguage = false,
                IsSystemLanguage = false,
                Translations = new List<LanguageTranslation>
                {
                    new() { Id = 9, Name = "Russisch", LanguageId = 1 },
                    new() { Id = 10, Name = "Russe", LanguageId = 2 },
                    new() { Id = 11, Name = "Ruski", LanguageId = 3 },
                    new() { Id = 12, Name = "Russian", LanguageId = 4 },
                },
            }
        };
        languageCreate = new LanguageCreate
        {
            IsoCode = "PK",
            SortIndex = 321,
            IsUiLanguage = false,
            DescriptionDe = "de",
            DescriptionFr = "fr",
            DescriptionIt = "it",
            DescriptionEn = "en",
            DescriptionRm = "rm"
        };
        languageUpdate = new LanguageUpdate
        {
            Id = 1,
            IsoCode = "de",
            SortIndex = 432,
            IsUiLanguage = false,
            DescriptionDe = "test_de",
            DescriptionFr = "test_fr",
            DescriptionIt = "test_it",
            DescriptionEn = "test_en",
            DescriptionRm = "test_rm"
        };

        language = new()
        {
            Id = 1,
            IsoCode = languageCreate.IsoCode,
            IsUiLanguage = languageCreate.IsUiLanguage,
            SortIndex = 333
        };

        languageDetail = new()
        {
            Id = language.Id,
            IsoCode = language.IsoCode,
            IsUiLanguage = language.IsUiLanguage,
            SortIndex = language.SortIndex,
            CreatedBy = email,
            ModifiedBy = email,
            Name = null
        };

        languageRepositoryMock.GetById(1).Returns(languages.First());

        mapperMock.Map<Language>(languageCreate).Returns(languages.First());
        mapperMock.Map<LanguageDetail>(languages.First()).Returns(languageDetail);

    }

    [TearDown]
    public void TearDown()
    {
        mapperMock.ClearSubstitute();
        languageRepositoryMock.ClearSubstitute();
        identityServiceMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
    }

    [Test]
    public async Task GetAllDetailsShouldReturnLanguageDetails()
    {
        languageRepositoryMock.GetAllDetails().Returns(languages);

        var languageDetails = new List<LanguageDetail> {
            new()
            {
                Id = languages.First().Id,
                IsoCode = languages.First().IsoCode,
                Name = languages.First().Translations.First().Name,
                IsUiLanguage = languages.First().IsUiLanguage,
                Created = languages.First().Created,
            },
             new()
             {
                Id = languages.Last().Id,
                IsoCode = languages.Last().IsoCode,
                Name = languages.Last().Translations.First().Name,
                IsUiLanguage = languages.Last().IsUiLanguage,
                Created = languages.Last().Created,
            }
        };
        mapperMock.Map<IEnumerable<LanguageDetail>>(languages).Returns(languageDetails);

        var result = (await languageService.GetAllDetails()).ToList();
        Assert.That(result, Is.EqualTo(languageDetails));
    }

    [Test]
    public async Task CreateShouldReturnLanguageDetail()
    {
        var result = await languageService.Create(languageCreate);
        await languageRepositoryMock.Received().Create(languages.First());
        await unitOfWorkMock.Received().SaveChangesAsync();
        Assert.That(result, Is.EqualTo(languageDetail));
    }

    [Test]
    public async Task UpdateShouldThrowEntityNotFoundException()
    {
        languageRepositoryMock.GetById(languageUpdate.Id).Returns(Task.FromResult<Language>(null));
        await unitOfWorkMock.DidNotReceive().SaveChangesAsync();
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await languageService.Update(languageUpdate));
    }

    [Test]
    public async Task UpdateShouldReturnLanguageDetail()
    {
        var currLanguage = languages.First();
        mapperMock.Map<Language>(languageUpdate).Returns(currLanguage);
        mapperMock.Map<LanguageDetail>(currLanguage).Returns(languageDetail);

        var result = await languageService.Update(languageUpdate);
        Assert.That(result, Is.EqualTo(languageDetail));
    }

}
