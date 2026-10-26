using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class SourceTemplateServiceTests
{
    private readonly ISourceTemplateRepository sourceTemplateRepositoryMock = Substitute.For<ISourceTemplateRepository>();
    private readonly IUserAccountRepository userAccountRepositoryMock = Substitute.For<IUserAccountRepository>();
    private readonly IIdentityService identityServiceMock = Substitute.For<IIdentityService>();
    private readonly IMapper mapperMock = Substitute.For<IMapper>();
    private readonly IUnitOfWork unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private SourceTemplateService service;
    private SourceTemplateModification sourceTemplateCreate;
    private SourceTemplateModification sourceTemplateUpdate;
    private SourceTemplate sourceTemplate1;
    private SourceTemplate sourceTemplate2;
    private IList<SourceTemplate> _sourceTemplates;
    private IList<SourceTemplateSummary> sourceTemplateDetails;
    private UserAccount user;

    [SetUp]
    public void SetUp()
    {
        service = new SourceTemplateService(sourceTemplateRepositoryMock, identityServiceMock, userAccountRepositoryMock, mapperMock, unitOfWorkMock);

        sourceTemplateCreate = new SourceTemplateModification()
        {
            TextDe = "Text De",
            TextFr = "Text Fr",
            TextIt = "Text It",
            TextEn = "Text En",
            TextRm = "Text Rm",
        };

        sourceTemplate1 = new SourceTemplate()
        {
            Id = 1,
            TextDe = "Text De",
            TextFr = "Text Fr",
            TextIt = "Text It",
            TextEn = "Text En",
            TextRm = "Text Rm",
            Created = new DateTime(2023, 1, 1),
            CreatedBy = "Creator"
        };

        sourceTemplateUpdate = new SourceTemplateModification()
        {
            TextDe = "Update De",
            TextFr = "Update Fr",
            TextIt = "Update It",
            TextEn = "Update En",
            TextRm = "Update Rm",
        };

        sourceTemplate2 = new SourceTemplate()
        {
            Id = 2,
            TextDe = "Update De",
            TextFr = "Update Fr",
            TextIt = "Update It",
            TextEn = "Update En",
            TextRm = "Update Rm",
            Modified = new DateTime(2024, 2, 2),
            ModifiedBy = "Modifier"
        };

        _sourceTemplates = new List<SourceTemplate>()
        {
            new()
            {
                Id = 1,
                TextDe = "Text De1",
                TextFr = "Text Fr1",
                TextIt = "Text It1",
                TextEn = "Text En1",
                TextRm = "Text Rm1",
                UserId = 1,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch"
            },
            new()
            {
                Id = 2,
                TextDe = "Text De2",
                TextFr = "Text Fr2",
                TextIt = "Text It2",
                TextEn = "Text En2",
                TextRm = "Text Rm2",
                UserId = 1,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch"
            },
            new()
            {
                Id = 3,
                TextDe = "",
                TextFr = "Text Fr3",
                TextIt = "Text It3",
                TextEn = "Text En3",
                TextRm = "Text Rm3",
                UserId = 1,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch"
            }
        };

        sourceTemplateDetails = new List<SourceTemplateSummary>()
        {
            new()
            {
                Id = 1,
                TextDe = "Text De1",
                TextFr = "Text Fr1",
                TextIt = "Text It1",
                TextEn = "Text En1",
                TextRm = "Text Rm1",
            },
            new()
            {
                Id = 2,
                TextDe = "",
                TextFr = "Text Fr2",
                TextIt = "Text It2",
                TextEn = "Text En2",
                TextRm = "Text Rm2",
            },
            new()
            {
                Id = 3,
                TextDe = "Text De3",
                TextFr = "Text Fr3",
                TextIt = "Text It3",
                TextEn = "Text En3",
                TextRm = "Text Rm3",
            }
        };

        user = new UserAccount
        {
            Id = 1,
            CreatedBy = "SYSTEM",
            Created = new DateTime(2026, 1, 1),
            Email = "test@test.ch"
        };

        userAccountRepositoryMock.GetByEmail(Arg.Any<string>()).Returns(user);

        mapperMock.Map<SourceTemplate>(sourceTemplateCreate).Returns(sourceTemplate1);
        mapperMock.Map<SourceTemplate>(sourceTemplateUpdate).Returns(sourceTemplate2);
        mapperMock.Map<IEnumerable<SourceTemplateSummary>>(_sourceTemplates).Returns(sourceTemplateDetails);
        sourceTemplateRepositoryMock.GetById(1).Returns(sourceTemplate1);
        sourceTemplateRepositoryMock.GetById(2).Returns(sourceTemplate2);
        sourceTemplateRepositoryMock.GetAll(1).Returns(_sourceTemplates);
    }

    [TearDown]
    public void TearDown()
    {
        sourceTemplateRepositoryMock.ClearSubstitute();
        userAccountRepositoryMock.ClearSubstitute();
        identityServiceMock.ClearSubstitute();
        mapperMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
    }

    [Test]
    public void CanGetAll()
    {
        var sourceTemplates = service.GetAllDefaults();

        Assert.That(sourceTemplates, Is.Not.Null);
        Assert.That(sourceTemplates.De, Is.InstanceOf<SourceTemplateDefault>());
        Assert.That(sourceTemplates.Fr, Is.InstanceOf<SourceTemplateDefault>());
        Assert.That(sourceTemplates.It, Is.InstanceOf<SourceTemplateDefault>());
        Assert.That(sourceTemplates.En, Is.InstanceOf<SourceTemplateDefault>());
        Assert.That(sourceTemplates.Rm, Is.InstanceOf<SourceTemplateDefault>());

        AssertSourceTemplateDefaults(sourceTemplates.De, new CultureInfo("de"));
        AssertSourceTemplateDefaults(sourceTemplates.Fr, new CultureInfo("fr"));
        AssertSourceTemplateDefaults(sourceTemplates.It, new CultureInfo("it"));
        AssertSourceTemplateDefaults(sourceTemplates.En, new CultureInfo("en"));
        AssertSourceTemplateDefaults(sourceTemplates.Rm, new CultureInfo("rm"));
    }

    private static void AssertSourceTemplateDefaults(SourceTemplateDefault sourceTemplate, CultureInfo cultureInfo)
    {
        BackendTexts.Culture = cultureInfo;

        Assert.That(sourceTemplate.FederalLawSr, Is.EqualTo(BackendTexts.SourceTemplateFederalLawSr));
        Assert.That(sourceTemplate.FederalLawAs, Is.EqualTo(BackendTexts.SourceTemplateFederalLawAs));
        Assert.That(sourceTemplate.FederalLawBbl, Is.EqualTo(BackendTexts.SourceTemplateFederalLawBbl));
        Assert.That(sourceTemplate.FederalCourt, Is.EqualTo(BackendTexts.SourceTemplateFederalCourt));
        Assert.That(sourceTemplate.FederalCouncilMessage, Is.EqualTo(BackendTexts.SourceTemplateBundesratMessage));
        Assert.That(sourceTemplate.Internet, Is.EqualTo(BackendTexts.SourceTemplateInternet));
    }

    [Test]
    public async Task Create_WhenCalled_ShouldSucceed()
    {
        SourceTemplate createdTemplate = null;
        sourceTemplateRepositoryMock.When(x => x.Create(Arg.Any<SourceTemplate>()))
            .Do(x => createdTemplate = x.Arg<SourceTemplate>());

        await service.Create(sourceTemplateCreate);

        mapperMock.Received(1).Map<SourceTemplate>(sourceTemplateCreate);
        identityServiceMock.Received(1).GetCurrentEmail();
        await userAccountRepositoryMock.Received(1).GetByEmail(Arg.Any<string>());
        await sourceTemplateRepositoryMock.Received(1).Create(sourceTemplate1);
        await unitOfWorkMock.Received(1).SaveChangesAsync();

        Assert.That(createdTemplate, Is.Not.Null);
        Assert.That(createdTemplate.Created, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
        Assert.That(createdTemplate.CreatedBy, Is.EqualTo(sourceTemplate1.CreatedBy));
    }

    [Test]
    public async Task Update_WhenCalled_ShouldSucceed()
    {
        SourceTemplate updatedTemplate = null;
        sourceTemplateRepositoryMock.When(x => x.Update(Arg.Any<SourceTemplate>(), Arg.Any<SourceTemplate>()))
            .Do(x => updatedTemplate = x.ArgAt<SourceTemplate>(1));

        await service.Update(1, sourceTemplateUpdate);

        mapperMock.Received(1).Map<SourceTemplate>(sourceTemplateUpdate);
        identityServiceMock.Received(1).GetCurrentEmail();
        await userAccountRepositoryMock.Received(1).GetByEmail(Arg.Any<string>());
        await sourceTemplateRepositoryMock.Received(1).GetById(1);

        sourceTemplateRepositoryMock.Received(1).Update(sourceTemplate1, sourceTemplate2);
        await unitOfWorkMock.Received(1).SaveChangesAsync();

        Assert.That(updatedTemplate, Is.Not.Null);
        Assert.That(updatedTemplate.Created, Is.EqualTo(sourceTemplate1.Created));
        Assert.That(updatedTemplate.CreatedBy, Is.EqualTo(sourceTemplate1.CreatedBy));
        Assert.That(updatedTemplate.Modified, Is.EqualTo(sourceTemplate2.Modified));
        Assert.That(updatedTemplate.ModifiedBy, Is.EqualTo(sourceTemplate2.ModifiedBy));
    }

    [Test]
    public async Task Delete_WhenCalled_ShouldSucceed()
    {
        SourceTemplate deletedTemplate = null;
        sourceTemplateRepositoryMock.When(x => x.Delete(Arg.Any<SourceTemplate>()))
            .Do(x => deletedTemplate = x.Arg<SourceTemplate>());

        await service.Delete(1);

        await sourceTemplateRepositoryMock.Received(1).GetById(1);
        sourceTemplateRepositoryMock.Received(1).Delete(sourceTemplate1);
        await unitOfWorkMock.Received(1).SaveChangesAsync();

        Assert.That(deletedTemplate, Is.Not.Null);
    }

    [Test]
    public async Task GetAllForCurrentUser_WhenCalled_ShouldReturnData()
    {
        var sourceTemplates = await service.GetAllForCurrentUser();

        identityServiceMock.Received(1).GetCurrentEmail();
        await sourceTemplateRepositoryMock.Received(1).GetAll(user.Id);
        mapperMock.Received(1).Map<IEnumerable<SourceTemplateSummary>>(Arg.Any<IEnumerable<SourceTemplate>>());

        Assert.That(sourceTemplates, Is.Not.Null);
        Assert.That(sourceTemplates.ToList(), Has.Count.EqualTo(3));
    }

    [Test]
    public async Task GetAllForCurrentUserAsText_WhenCalled_ShouldReturnData()
    {
        var sourceTemplates = await service.GetAllForCurrentUserAsText("de");

        identityServiceMock.Received(1).GetCurrentEmail();
        await sourceTemplateRepositoryMock.Received(1).GetAll(user.Id);

        Assert.That(sourceTemplates, Is.Not.Null);
        Assert.That(sourceTemplates.ToList(), Has.Count.EqualTo(2));
        Assert.That(sourceTemplates.First(), Is.EqualTo("Text De1"));
        Assert.That(sourceTemplates.Last(), Is.EqualTo("Text De2"));
    }
}
