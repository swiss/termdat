using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using DomainModel = Bk.Termdat.Business.Domain.Models;
using ResourceModel = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class EntryServiceTests
{
    private EntryService service;

    private readonly IEntryValidationService entryValidationServiceMock = Substitute.For<IEntryValidationService>();
    private readonly IIdentityService identityServiceMock = Substitute.For<IIdentityService>();
    private readonly IAuthorizationService authorizationService = Substitute.For<IAuthorizationService>();
    private readonly IEntryRepository entryRepositoryMock = Substitute.For<IEntryRepository>();
    private readonly ISearchQueueClient searchQueueClientMock = Substitute.For<ISearchQueueClient>();
    private readonly IUnitOfWork unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IMapper mapperMock = Substitute.For<IMapper>();
    private readonly ILogger<EntryService> loggerMock = Substitute.For<ILogger<EntryService>>();
    private readonly IUserAccountService userAccountServiceMock = Substitute.For<IUserAccountService>();
    private readonly IPictureService pictureServiceMock = Substitute.For<IPictureService>();
    private readonly IAdminInfoService adminInfoServiceMock = Substitute.For<IAdminInfoService>();
    private readonly IFormFile formFileMock = Substitute.For<IFormFile>();
    private readonly IEntryNotePatternService entryNotePatternServiceMock = Substitute.For<IEntryNotePatternService>();
    private const string TestEmail = "unit@test.ch";
    private const string TestEmailReplace = "unit@test2.ch";

    private DomainModel.Entry entry;
    private ResourceModel.EntryCreate entryCreate;
    private ResourceModel.EntryUpdate entryUpdate;
    private ResourceModel.EntrySearchAndReplaceUpdate entrySearchAndReplaceUpdate;
    private ResourceModel.EntrySearchAndReplaceUpdate entrySearchAndReplaceUpdateWithNull;
    private List<DomainModel.Entry> entriesToReplace;
    private List<DomainModel.Entry> entriesToReplaceWithNull;
    private List<IndexEntry> indexEntries;
    private ResourceModel.PictureDetail picture;
    private ResourceModel.EntryReferenceDetail entryReferenceDetail;
    private ResourceModel.EntryModificationDetail entryModificationDetail;

    [SetUp]
    public void SetUp()
    {
        entry = CreateTestEntry();
        entriesToReplace = new()
        {
            new()
            {
                Id = 1,
                OfficeId = 1,
                CollectionId = 1,
                Translations = new List<DomainModel.EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        Definition = "Test Replace test def",
                        Country = "Test Replace test country",
                        Context = "Test Replace test context",
                        Note = "Test Replace test note",
                        Metadata = "Test Replace test metadata",
                        TerminusSource = "Test Replace test source",
                        AbbreviationSource = "Test Replace test source",
                        NameSource = "Test Replace test source",
                        ContextSource = "Test Replace test source",
                        DefinitionSource = "Test Replace test source",
                        NoteSource = "Test Replace test source",
                        MetadataSource = "Test Replace test source",
                        PhraseologySource = "Test Replace test source"
                    },
                    new()
                    {
                        LanguageId = 2,
                        Definition = "Test Replace test def",
                        Country = "Test Replace test country",
                        Context = "Test Replace test context",
                        Note = "Test Replace test note",
                        Metadata = "Test Replace test metadata",
                        TerminusSource = "Test Replace test source",
                        AbbreviationSource = "Test Replace test source",
                        NameSource = "Test Replace test source",
                        ContextSource = "Test Replace test source",
                        DefinitionSource = "Test Replace test source",
                        NoteSource = "Test Replace test source",
                        MetadataSource = "Test Replace test source",
                        PhraseologySource = "Test Replace test source"
                    }
                },
                Comment = "Test Replace test comment"
            },
            new()
            {
                Id = 2,
                OfficeId = 2,
                CollectionId = 3,
                Translations = new List<DomainModel.EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        Definition = "Test Test test def",
                        Country = "Test Test test country",
                        Context = "Test Test test context",
                        Note = "Test Test test note",
                        Metadata = "Test Test test metadata",
                        TerminusSource = "Test Test test source",
                        AbbreviationSource = "Test Test test source",
                        NameSource = "Test Test test source",
                        ContextSource = "Test Test test source",
                        DefinitionSource = "Test Test test source",
                        NoteSource = "Test Test test source",
                        MetadataSource = "Test Test test source",
                        PhraseologySource = "Test Test test source"
                    },
                    new()
                    {
                        LanguageId = 2,
                        Definition = "Test Test test def",
                        Country = "Test Test test country",
                        Context = "Test Test test context",
                        Note = "Test Test test note",
                        Metadata = "Test Test test metadata",
                        TerminusSource = "Test Test test source",
                        AbbreviationSource = "Test Test test source",
                        NameSource = "Test Test test source",
                        ContextSource = "Test Test test source",
                        DefinitionSource = "Test Test test source",
                        NoteSource = "Test Test test source",
                        MetadataSource = "Test Test test source",
                        PhraseologySource = "Test Test test source"
                    },
                    new()
                    {
                        LanguageId = 3,
                        Definition = "Test Test test def",
                        Country = "Test Test test country",
                        Context = "Test Test test context",
                        Note = "Test Test test note",
                        Metadata = "Test Test test metadata",
                        TerminusSource = "Test Test test source",
                        AbbreviationSource = "Test Test test source",
                        NameSource = "Test Test test source",
                        ContextSource = "Test Test test source",
                        DefinitionSource = "Test Test test source",
                        NoteSource = "Test Test test source",
                        MetadataSource = "Test Test test source",
                        PhraseologySource = "Test Test test source"
                    }
                },
                Comment = "Test Test test comment"
            },
            new()
            {
                Id = 3,
                OfficeId = 2,
                CollectionId = 3,
                Translations = new List<DomainModel.EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        Definition = "def",
                        Country = "country",
                        Context = "context",
                        Note = "note",
                        Metadata = "metadata",
                        TerminusSource = "source",
                        AbbreviationSource = "source",
                        NameSource = "source",
                        ContextSource = "source",
                        DefinitionSource = "source",
                        NoteSource = "source",
                        MetadataSource = "source",
                        PhraseologySource = "source"
                    },
                    new()
                    {
                        LanguageId = 2,
                        Definition = "def",
                        Country = "country",
                        Context = "context",
                        Note = "note",
                        Metadata = "metadata",
                        TerminusSource = "source",
                        AbbreviationSource = "source",
                        NameSource = "source",
                        ContextSource = "source",
                        DefinitionSource = "source",
                        NoteSource = "source",
                        MetadataSource = "source",
                        PhraseologySource = "source"
                    },
                    new()
                    {
                        LanguageId = 3,
                       Definition = "def",
                        Country = "country",
                        Context = "context",
                        Note = "note",
                        Metadata = "metadata",
                        TerminusSource = "source",
                        AbbreviationSource = "source",
                        NameSource = "source",
                        ContextSource = "source",
                        DefinitionSource = "source",
                        NoteSource = "source",
                        MetadataSource = "source",
                        PhraseologySource = "source"
                    }
                },
                Comment = "comment"
            },
        };
        entriesToReplaceWithNull = new()
        {
            new()
            {
                Id = 3,
                OfficeId = 1,
                CollectionId = 1,
                Translations = new List<DomainModel.EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        Definition = "Test Replace test def",
                        Country = "Test Replace test country",
                        Context = "Test Replace test context",
                        Note = "Test Replace test note",
                        Metadata = "Test Replace test metadata",
                        TerminusSource = "Test Replace test source",
                        AbbreviationSource = "Test Replace test source",
                        NameSource = "Test Replace test source",
                        ContextSource = "Test Replace test source",
                        DefinitionSource = "Test Replace test source",
                        NoteSource = "Test Replace test source",
                        MetadataSource = "Test Replace test source",
                        PhraseologySource = "Test Replace test source"
                    }
                },
                Comment = "Test Replace test comment"
            }
        };
        entryCreate = new()
        {
            OfficeId = 1,
            CollectionId = 1,
            ConceptSystemPositionId = 1,
            LanguageDetails = new List<ResourceModel.EntryModificationLanguageDetail> { new() { LanguageId = 1, Sequence = 1 } },
            References = new List<ResourceModel.EntryModificationReference> { new() { EntryId = 2, Type = ResourceModel.ReferenceType.General } },
            AdminInfos = new List<ResourceModel.AdminInfoSummary> { new() { Id = 2, Code = "Code2" } }
        };
        entryUpdate = new()
        {
            Id = 1,
            OfficeId = 1,
            CollectionId = 1,
            ConceptSystemPositionId = 1,
            LanguageDetails = new List<ResourceModel.EntryModificationLanguageDetail> { new() { LanguageId = 1, Sequence = 1 } },
            References = new List<ResourceModel.EntryModificationReference> { new() { EntryId = 2, Type = ResourceModel.ReferenceType.General } },
            RelatedCollectionIds = new List<int> { 1 },
            LenochClassificationIds = new List<int> { 1 },
            ConcurrencyToken = 999,
            PictureDeleted = false,
            AdminInfos = new List<ResourceModel.AdminInfoSummary> { new() { Id = 2, Code = "Code2" } }
        };
        entrySearchAndReplaceUpdate = new()
        {
            Ids = new List<int> { 1 },
            SearchValue = "Test",
            ReplaceValue = "Replace",
            LanguageIds = new List<int> { 1, 2 },
            SearchField = "note"
        };
        entrySearchAndReplaceUpdateWithNull = new()
        {
            Ids = new List<int> { 3 },
            SearchValue = "Test",
            ReplaceValue = null,
            LanguageIds = new List<int> { 1, 2 },
            SearchField = "note"
        };
        entryReferenceDetail = new()
        {
            Id = 1
        };
        entryModificationDetail = new()
        {
            Id = 1
        };
        indexEntries = new List<IndexEntry> { new() { EntryId = 1 } };

        mapperMock.Map<DomainModel.Entry>(entryCreate).Returns(entry);
        mapperMock.Map<DomainModel.Entry>(entryUpdate).Returns(entry);
        mapperMock.Map<DomainModel.Entry>(Arg.Any<ResourceModel.EntryUpdate>()).Returns(entry);
        mapperMock.Map<ResourceModel.EntryModificationDetail>(entry).Returns(entryModificationDetail);
        mapperMock.Map<ResourceModel.EntryReferenceDetail>(entry).Returns(entryReferenceDetail);
        mapperMock.Map(Arg.Any<DomainModel.Entry>(), Arg.Any<DomainModel.Entry>()).Returns(x => (DomainModel.Entry)x[1]);

        entryRepositoryMock.GetById(1).Returns(entry);
        entryRepositoryMock.GetByIds(Arg.Any<IEnumerable<int>>()).Returns(new List<DomainModel.Entry> { entry });
        entryRepositoryMock.GetByIdForEntryDetail(1).Returns(entry);
        entryRepositoryMock.GetByIdForReferenceDetail(1).Returns(entry);
        entryRepositoryMock.GetByIdForUpdate(1).Returns(entry);
        entryRepositoryMock.GetByIdsForReplace(Arg.Is(entrySearchAndReplaceUpdate.Ids), Arg.Any<string>(), Arg.Any<string>()).Returns(entriesToReplace);
        entryRepositoryMock.GetByIdsForReplace(Arg.Is(entrySearchAndReplaceUpdateWithNull.Ids), Arg.Any<string>(), Arg.Any<string>()).Returns(entriesToReplaceWithNull);
        entryRepositoryMock.GetByIdForIndex(1).Returns(indexEntries);
        entryRepositoryMock.GetByIdsForIndex(Arg.Any<IEnumerable<int>>()).Returns(indexEntries);

        pictureServiceMock.Update(Arg.Any<ResourceModel.PictureDetail>(), Arg.Any<IFormFile>()).ReturnsForAnyArgs(new DomainModel.Picture());
        pictureServiceMock.Create(Arg.Any<ResourceModel.PictureDetail>(), Arg.Any<IFormFile>()).ReturnsForAnyArgs(new DomainModel.Picture());

        adminInfoServiceMock.Create(Arg.Any<ResourceModel.AdminInfoCreate>()).ReturnsForAnyArgs(new ResourceModel.AdminInfoDetail());

        entryValidationServiceMock.ValidateRelatedCollections(entryUpdate).Returns(new List<DomainModel.Collection>
        {
            new() { Id = 1 }
        });
        entryValidationServiceMock.ValidateLenochClassification(entryUpdate).Returns(new List<DomainModel.LenochClassification>
        {
            new() { Id = 1 }
        });
        entryValidationServiceMock.ValidateSubjects(Arg.Any<ResourceModel.IEntryModification>()).Returns(new List<DomainModel.Subject>
        {
            new() { Id = 1 }
        });
        entryValidationServiceMock.ValidateDomains(Arg.Any<ResourceModel.IEntryModification>()).Returns(new List<DomainModel.Domain>
        {
            new() { Id = 1 }
        });
        identityServiceMock.GetCurrentEmail().Returns(TestEmail);
        authorizationService.IsAdmin().Returns(false);
        authorizationService.CanEdit(Arg.Any<DomainModel.Entry>()).Returns(true);
        authorizationService.CanDelete(Arg.Any<DomainModel.Entry>()).Returns(true);
        authorizationService.CanRestore(Arg.Any<DomainModel.Entry>()).Returns(true);

        formFileMock.Length.ReturnsForAnyArgs(100000);
        picture = new()
        {
            DescriptionDe = "DescriptionDe",
            SourceDe = "SourceDe",
            DescriptionIt = "DescriptionIt",
            SourceEn = "SourceEn",
        };

        service = new EntryService(entryValidationServiceMock, identityServiceMock, authorizationService, pictureServiceMock, userAccountServiceMock, adminInfoServiceMock, unitOfWorkMock, entryRepositoryMock, searchQueueClientMock, mapperMock, loggerMock, entryNotePatternServiceMock);
    }

    [TearDown]
    public void TearDown()
    {
        entryValidationServiceMock.ClearSubstitute();
        identityServiceMock.ClearSubstitute();
        authorizationService.ClearSubstitute();
        entryRepositoryMock.ClearSubstitute();
        searchQueueClientMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
        mapperMock.ClearSubstitute();
        formFileMock.ClearSubstitute();
        userAccountServiceMock.ClearSubstitute();
        pictureServiceMock.ClearSubstitute();
        adminInfoServiceMock.ClearSubstitute();
        loggerMock.ClearSubstitute();
        entryNotePatternServiceMock.ClearSubstitute();
    }

    [Test]
    public async Task CreateCallsDependencies()
    {
        mapperMock.Map<DomainModel.Entry>(entryCreate).Returns(entry);

        await service.Create(entryCreate);

        await entryRepositoryMock.Received().Create(entry);
        entryValidationServiceMock.Received().ValidateLanguageDetails(entryCreate);
        await entryValidationServiceMock.Received().ValidateCollection(entryCreate);
        await entryValidationServiceMock.Received().ValidateOffice(entryCreate);
        await entryValidationServiceMock.Received().ValidateConceptSystemPosition(entryCreate);
        await entryValidationServiceMock.Received().ValidateRelatedCollections(entryCreate);
        await entryValidationServiceMock.Received().ValidateLenochClassification(entryCreate);
        await entryValidationServiceMock.Received().ValidateGlossaries(entryCreate);
        await entryValidationServiceMock.Received(1).ValidateReferences(entryCreate, entry);
        await entryValidationServiceMock.Received(0).ValidatePicture(Arg.Any<DomainModel.Picture>());
        await entryValidationServiceMock.Received(1).ValidateSubjects(entryCreate);
        await entryValidationServiceMock.Received(1).ValidateDomains(entryCreate);
        entryValidationServiceMock.Received(1).AuthorizeCollection(entryCreate.CollectionId);
        entryValidationServiceMock.Received(1).AuthorizeGlossaries(entryCreate.GlossaryIds);
        await pictureServiceMock.Received(0).Create(Arg.Any<ResourceModel.PictureDetail>(), Arg.Any<IFormFile>());
        await pictureServiceMock.Received(0).Update(Arg.Any<ResourceModel.PictureDetail>(), Arg.Any<IFormFile>());
        await pictureServiceMock.Received(0).Delete(Arg.Any<int>());
        await unitOfWorkMock.Received().SaveChangesAsync();
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task CreateSetsCreatedAndModified()
    {
        mapperMock.Map<DomainModel.Entry>(entryCreate).Returns(entry);

        await service.Create(entryCreate);

        Assert.That(entry.CreatedBy, Is.EqualTo(TestEmail));
        Assert.That(entry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(entry.Created, Is.Not.EqualTo(default(DateTime)));
        Assert.That(entry.Modified, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public async Task Create_WithDefinedProcessingUser_SetsProcessingUser()
    {
        const string processingUser = "zyz";
        mapperMock.Map<DomainModel.Entry>(entryCreate).Returns(entry);

        await service.Create(entryCreate, processingUser);

        Assert.That(entry.CreatedBy, Is.EqualTo(processingUser));
        Assert.That(entry.ModifiedBy, Is.EqualTo(processingUser));
    }

    [Test]
    public async Task UpdateCallsDependencies()
    {
        await service.Update(entryUpdate);

        await entryRepositoryMock.Received().GetByIdForEntryDetail(entryUpdate.Id);
        await entryValidationServiceMock.Received().ValidateCollection(entryUpdate);
        await entryValidationServiceMock.Received().ValidateOffice(entryUpdate);
        await entryValidationServiceMock.Received().ValidateConceptSystemPosition(entryUpdate);
        await entryValidationServiceMock.Received().ValidateRelatedCollections(entryUpdate);
        await entryValidationServiceMock.Received().ValidateLenochClassification(entryUpdate);
        await entryValidationServiceMock.Received().ValidateGlossaries(entryUpdate);
        await entryValidationServiceMock.Received().ValidatePicture(Arg.Any<DomainModel.Picture>());
        await entryValidationServiceMock.Received(1).ValidateReferences(entryUpdate, entry);
        await entryValidationServiceMock.Received(1).ValidateSubjects(entryUpdate);
        await entryValidationServiceMock.Received(1).ValidateDomains(entryUpdate);
        entryValidationServiceMock.Received(1).ValidatePictureFile(null, null);
        entryValidationServiceMock.Received(1).AuthorizeCollection(entryUpdate.CollectionId, entry.CollectionId);
        entryValidationServiceMock.Received(1).AuthorizeGlossaries(entryUpdate.GlossaryIds, Arg.Any<IList<DomainModel.Glossary>>());
        await pictureServiceMock.Received(0).Create(Arg.Any<ResourceModel.PictureDetail>(), Arg.Any<IFormFile>());
        await pictureServiceMock.Received(0).Update(Arg.Any<ResourceModel.PictureDetail>(), Arg.Any<IFormFile>());
        await pictureServiceMock.Received(0).Delete(Arg.Any<int>());
        await unitOfWorkMock.Received().SaveChangesAsync();
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task ThrowsOnUpdateOfDeletedEntry()
    {
        entry.Deleted = true;

        Assert.ThrowsAsync<CanNotBeChangedException>(async () => await service.Update(entryUpdate));

        await entryRepositoryMock.Received(1).GetByIdForUpdate(Arg.Is(entryUpdate.Id));
    }

    [Test]
    public async Task Update_WhenUnauthorized_ThrowsForbiddenException()
    {
        authorizationService.CanEdit(Arg.Any<DomainModel.Entry>()).Returns(false);

        Assert.ThrowsAsync<ForbiddenException>(async () => await service.Update(entryUpdate));

        await entryRepositoryMock.Received(1).GetByIdForUpdate(Arg.Is(entryUpdate.Id));
        authorizationService.Received(1).CanEdit(entry);
    }

    [Test]
    public async Task Update_WhenLockedFromDifferentUser_ThrowsEntryLockedException()
    {
        entry.Locked = DateTime.UtcNow;
        entry.LockedBy = "test";

        Assert.ThrowsAsync<EntryLockedException>(async () => await service.Update(entryUpdate));

        await entryRepositoryMock.Received(1).GetByIdForUpdate(Arg.Is(entryUpdate.Id));
    }

    [Test]
    public async Task Update_WhenLockedByCurrentUser_UpdatesLockedTime()
    {
        entry.Locked = DateTime.UtcNow.AddMinutes(-5);
        entry.LockedBy = TestEmail;

        Assert.DoesNotThrowAsync(async () => await service.Update(entryUpdate));

        Assert.That(entry.Locked, Is.Not.Null);
        Assert.That(entry.Locked, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));

        await unitOfWorkMock.Received().SaveChangesAsync();
    }

    [Test]
    public async Task Update_WhenEntryIsNotLocked_DoesNotUpdateLockedTime()
    {
        Assert.DoesNotThrowAsync(async () => await service.Update(entryUpdate));

        Assert.That(entry.Locked, Is.Null);
        Assert.That(entry.LockedBy, Is.Null);

        await unitOfWorkMock.Received().SaveChangesAsync();
    }

    [Test]
    public async Task Update_SetsModifiedFields_ForValidUpdate()
    {
        await service.Update(entryUpdate);

        Assert.That(entry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(entry.Modified, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public async Task GetDetail_ForDeletedEntryAsNonAdmin_Throws()
    {
        entry.Deleted = true;

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await service.GetDetail(entry.Id));

        await entryRepositoryMock.Received(1).GetByIdForEntryDetail(Arg.Is(entry.Id));
    }

    [Test]
    public async Task GetDetail_ForDeletedEntryAsAdmin_Returns()
    {
        entry.Deleted = true;
        authorizationService.IsAdmin().Returns(true);

        await service.GetDetail(1);

        await entryRepositoryMock.Received(1).GetByIdForEntryDetail(Arg.Is(1));
    }

    [Test]
    public async Task GetDetail_ForWriterWithoutCollectionPermissions_RemovesComment()
    {
        var mapper = GetConfiguredMapper();
        var sut = new EntryService(entryValidationServiceMock, identityServiceMock, authorizationService,
            pictureServiceMock, userAccountServiceMock, adminInfoServiceMock, unitOfWorkMock, entryRepositoryMock,
            searchQueueClientMock, mapper, loggerMock, entryNotePatternServiceMock);
        entry.Comment = "Highly sensitive comment :-)";
        entryValidationServiceMock.When(s => s.AuthorizeCollection(entry.CollectionId))
            .Do(_ => throw new ForbiddenException());
        authorizationService.IsWriter().Returns(true);

        var actual = await sut.GetDetail(1);

        Assert.That(actual.Comment, Is.Null);
    }

    [Test]
    public async Task GetDetail_ForSuperReaderWithoutCollectionPermissions_RemovesCommentAndAdminInfos()
    {
        var mapper = GetConfiguredMapper();
        var sut = new EntryService(entryValidationServiceMock, identityServiceMock, authorizationService,
            pictureServiceMock, userAccountServiceMock, adminInfoServiceMock, unitOfWorkMock, entryRepositoryMock,
            searchQueueClientMock, mapper, loggerMock, entryNotePatternServiceMock);
        entry.Comment = "Highly sensitive comment :-)";
        entry.AdminInfos = [new DomainModel.AdminInfo() { Code = "adminInfo1" }];
        entryValidationServiceMock.When(s => s.AuthorizeCollection(entry.CollectionId))
            .Do(_ => throw new ForbiddenException());
        authorizationService.IsSuperReader().Returns(true);

        var actual = await sut.GetDetail(1);

        Assert.That(actual.Comment, Is.Null);
        Assert.That(actual.AdminInfos, Is.Empty);
    }

    [Test]
    public async Task GetDetail_ForWriterWithoutGlossariesPermissions_RemovesComment()
    {
        var mapper = GetConfiguredMapper();
        var sut = new EntryService(entryValidationServiceMock, identityServiceMock, authorizationService,
            pictureServiceMock, userAccountServiceMock, adminInfoServiceMock, unitOfWorkMock, entryRepositoryMock,
            searchQueueClientMock, mapper, loggerMock, entryNotePatternServiceMock);
        entry.Comment = "Highly sensitive comment :-)";
        entryValidationServiceMock.When(s => s.AuthorizeGlossaries(Arg.Any<IEnumerable<int>>()))
            .Do(_ => throw new ForbiddenException());
        authorizationService.IsWriter().Returns(true);

        var actual = await sut.GetDetail(1);

        Assert.That(actual.Comment, Is.Null);
    }

    [Test]
    public async Task GetDetail_ForWriterWithGlossariesAndCollectionPermissions_ReturnsComment()
    {
        var mapper = GetConfiguredMapper();
        var sut = new EntryService(entryValidationServiceMock, identityServiceMock, authorizationService,
            pictureServiceMock, userAccountServiceMock, adminInfoServiceMock, unitOfWorkMock, entryRepositoryMock,
            searchQueueClientMock, mapper, loggerMock, entryNotePatternServiceMock);
        entry.Comment = "Highly sensitive comment :-)";
        authorizationService.IsWriter().Returns(true);

        var actual = await sut.GetDetail(1);

        Assert.That(actual.Comment, Is.EqualTo(entry.Comment));
    }

    [Test]
    public async Task GetDetail_ForSuperReaderWithCollectionPermissions_ReturnsCommentAndAdminInfos()
    {
        var mapper = GetConfiguredMapper();
        var sut = new EntryService(entryValidationServiceMock, identityServiceMock, authorizationService,
            pictureServiceMock, userAccountServiceMock, adminInfoServiceMock, unitOfWorkMock, entryRepositoryMock,
            searchQueueClientMock, mapper, loggerMock, entryNotePatternServiceMock);
        entry.Comment = "Highly sensitive comment :-)";
        entry.AdminInfos = [new DomainModel.AdminInfo() { Code = "adminInfo1" }];
        authorizationService.IsSuperReader().Returns(true);

        var actual = await sut.GetDetail(1);

        Assert.That(actual.Comment, Is.EqualTo(entry.Comment));
        Assert.That(actual.AdminInfos, Has.Count.EqualTo(1));
        Assert.That(actual.AdminInfos.First().Code, Is.EqualTo("adminInfo1"));
    }

    [Test]
    public void RestoreMultiple_ForUnknownEntry_ThrowsEntityNotFoundException()
    {
        var ids = new[] { 1, 2, 3 };

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await service.RestoreMultiple(ids));
    }

    [Test]
    public void RestoreMultiple_WhenAnyEntryUnauthorized_ThrowsForbiddenException()
    {
        var ids = new[] { 1, 2, 3 };
        authorizationService.CanRestore(Arg.Any<DomainModel.Entry>()).Returns(false);
        entryRepositoryMock.GetByIds(Arg.Is(ids)).Returns(new List<DomainModel.Entry>
        {
            new() { Id = 1 },
            new() { Id = 2 },
            new() { Id = 3 },
        });

        Assert.ThrowsAsync<ForbiddenException>(async () => await service.RestoreMultiple(ids));
    }

    [Test]
    public void RestoreMultiple_WhenAnyEntryLocked_ReturnsLockedRecords()
    {
        var ids = new[] { 1, 2 };
        entryRepositoryMock.GetByIds(Arg.Is(ids)).Returns(new List<DomainModel.Entry>
        {
            new() { Id = 1 },
            new() { Id = 2, Locked = DateTime.UtcNow, LockedBy = "test"},
        });
        authorizationService.CanRestore(Arg.Any<DomainModel.Entry>()).Returns(true);

        var lockedEntries = service.RestoreMultiple(ids);

        Assert.That(lockedEntries.Result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task RestoreMultiple_WhenAuthorized_RestoresDeletedEntry()
    {
        var ids = new[] { 1, 4 };
        entryRepositoryMock.GetByIds(Arg.Is(ids)).Returns(new List<DomainModel.Entry>
        {
            new() { Id = 1, Deleted = true },
            new() { Id = 4, Deleted = true },
        });
        authorizationService.CanRestore(Arg.Any<DomainModel.Entry>()).Returns(true);

        await service.RestoreMultiple(ids);

        Assert.That(entry.Deleted, Is.False);

        await entryRepositoryMock.Received(1).GetByIds(Arg.Is(ids));
        authorizationService.Received(2).CanRestore(Arg.Any<DomainModel.Entry>());
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Is(indexEntries));
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task RestoreMultiple_ForExceptionInSearchRepository_Catches()
    {
        var ids = new[] { 1 };
        entryRepositoryMock.GetByIds(Arg.Is(ids)).Returns(new List<DomainModel.Entry>
        {
            new() { Id = 1 },
        });
        var exception = new ArgumentException("CATCH ME");
        searchQueueClientMock.When(x => x.QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>()))
            .Do(_ => throw exception);

        await service.RestoreMultiple(ids);

        Assert.That(entry.Deleted, Is.False);
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is<IEnumerable<int>>(arr => arr.SequenceEqual(ids)));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Is(indexEntries));
        loggerMock.Received(1).Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Any<Arg.AnyType>(), Arg.Any<Exception>(), Arg.Any<Func<Arg.AnyType, Exception, string>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task Update_AddsAndRemovesRelatedCollections_ForValidUpdate()
    {
        mapperMock.Map<DomainModel.Entry>(entryUpdate).Returns(entry);
        mapperMock.Map<DomainModel.Entry>(entry).Returns(entry);

        entryValidationServiceMock.ValidateRelatedCollections(entryUpdate).Returns(new List<DomainModel.Collection>
        {
            new() { Id = 1 }, // existing
            new() { Id = 3 } // new
        });

        await service.Update(entryUpdate);

        Assert.That(entry.RelatedCollections.Count, Is.EqualTo(2));
        Assert.That(entry.RelatedCollections.Any(rc => rc.Id == 1), Is.True);
        Assert.That(entry.RelatedCollections.Any(rc => rc.Id == 3), Is.True);
    }

    [Test]
    public async Task Update_AddsAndRemovesLenochClassifications_ForValidUpdate()
    {
        mapperMock.Map<DomainModel.Entry>(entryUpdate).Returns(entry);
        mapperMock.Map<DomainModel.Entry>(entry).Returns(entry);

        entryValidationServiceMock.ValidateLenochClassification(entryUpdate).Returns(new List<DomainModel.LenochClassification>
        {
            new() { Id = 1 }, // existing
            new() { Id = 3 } // new
        });

        await service.Update(entryUpdate);

        Assert.That(entry.LenochClassifications.Count, Is.EqualTo(2));
        Assert.That(entry.LenochClassifications.Any(rc => rc.Id == 1), Is.True);
        Assert.That(entry.LenochClassifications.Any(rc => rc.Id == 3), Is.True);
    }

    [Test]
    public async Task Update_WithConcurrencyConflict_ThrowsConcurrencyException()
    {
        mapperMock.Map<DomainModel.Entry>(entryUpdate).Returns(new DomainModel.Entry { ConcurrencyToken = entry.ConcurrencyToken + 1 });
        Assert.ThrowsAsync<ConcurrencyException>(async () => await service.Update(entryUpdate));

        await unitOfWorkMock.DidNotReceive().SaveChangesAsync();
    }

    [Test]
    public void Update_WhenNotFound_ThrowsEntityNotFoundException()
    {
        entryRepositoryMock.GetByIdForUpdate(entryUpdate.Id).ReturnsNull();

        Assert.That(() => service.Update(entryUpdate), Throws.InstanceOf<EntityNotFoundException>());
    }

    [Test]
    public async Task GetDetail_CallsRepository_ForExistingId()
    {
        await service.GetDetail(entryUpdate.Id);

        await entryRepositoryMock.Received(1).GetByIdForEntryDetail(1);
    }

    [Test]
    public async Task GetDetail_FiltersLanguages_ForLanguageArguments()
    {
        const int languageId1 = 1;
        const int languageId2 = 2;

        await service.GetDetail(1, new[] { languageId1 }, new[] { languageId2 });

        Assert.That(entry.Translations.All(t => t.LanguageId is languageId1 or languageId2), Is.True);
        Assert.That(entry.Collection.Translations.All(t => t.LanguageId is languageId1 or languageId2), Is.True);
    }

    [Test]
    public async Task GetDetail_SortsLanguages_ForValidId()
    {
        entry.Translations = new List<DomainModel.EntryLanguage>
        {
            new() {Id = 6, Sequence = 3, Language = new DomainModel.Language { SortIndex = 3} },
            new() {Id = 5, Sequence = 2, Language = new DomainModel.Language { SortIndex = 3} },
            new() {Id = 2, Sequence = 4, Language = new DomainModel.Language { SortIndex = 1} },
            new() {Id = 1, Sequence = 1, Language = new DomainModel.Language { SortIndex = 1} },
            new() {Id = 4, Sequence = 4, Language = new DomainModel.Language { SortIndex = 2} },
            new() {Id = 3, Sequence = 1, Language = new DomainModel.Language { SortIndex = 2} },
            new() {Id = 8, Sequence = 3, Language = new DomainModel.Language { SortIndex = 4} },
            new() {Id = 7, Sequence = 2, Language = new DomainModel.Language { SortIndex = 4} },
        };

        await service.GetDetail(1);

        var expectedIdSequence = Enumerable.Range(1, 8).ToArray();
        var actualIdSequence = entry.Translations.Select(e => e.Id).ToArray();

        Assert.That(actualIdSequence, Is.EquivalentTo(expectedIdSequence));
    }

    [Test]
    public void GetDetail_WhenNotFound_ThrowsEntityNotFoundException()
    {
        const int entryId = 13;
        entryRepositoryMock.GetByIdForEntryDetail(entryId).ReturnsNull();

        Assert.That(() => service.GetDetail(entryId), Throws.InstanceOf<EntityNotFoundException>());
    }

    [Test]
    public void DeleteMultipleSoft_WhenNotFound_ThrowsEntityNotFoundException()
    {
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await service.DeleteMultiple(new[] { 2, 3 }, true));
    }

    [Test]
    public async Task DeleteMultipleHard_WhenNotFound_DeletesExistingEntries()
    {
        var ids = new[] { 1, 2 };
        entry.Deleted = true;

        entryRepositoryMock.GetByIds(Arg.Is<IEnumerable<int>>(ids)).Returns(new List<DomainModel.Entry>
        {
            entry
        });

        await service.DeleteMultiple(ids, false);

        await entryRepositoryMock.Received(1).GetByIds(ids);
        entryRepositoryMock.Received(1).Delete(Arg.Is<DomainModel.Entry>(arr => arr.Equals(entry)));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(0).GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.Received(0).QueueIndexUpdate(Arg.Any<List<IndexEntry>>());
        await searchQueueClientMock.Received(1).QueueIndexDelete(Arg.Is<IEnumerable<int>>(arr => arr.SequenceEqual(new[] { 1 })));
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public void DeleteMultiple_WhenAnyEntryIsUnauthorized_ThrowsForbiddenException()
    {
        authorizationService.CanDelete(Arg.Any<DomainModel.Entry>()).Returns(true, false);
        entryRepositoryMock.GetByIds(Arg.Any<IEnumerable<int>>()).Returns(new List<DomainModel.Entry>
        {
            new() { Id = 2 },
            new() { Id = 3 },
            new() { Id = 4 },
        });

        Assert.ThrowsAsync<ForbiddenException>(async () => await service.DeleteMultiple(new[] { 2, 3, 4 }));

        Assert.ThrowsAsync<ForbiddenException>(async () => await service.DeleteMultiple(new[] { 2, 3, 4 }, false));
    }

    [Test]
    public void DeleteMultiple_WhenAnyEntryIsLocked_ReturnsLockedRecords()
    {
        authorizationService.CanDelete(Arg.Any<DomainModel.Entry>()).Returns(true, true, true);
        entryRepositoryMock.GetByIds(Arg.Any<IEnumerable<int>>()).Returns(new List<DomainModel.Entry>
        {
            new() { Id = 2, Deleted = true },
            new() { Id = 3, Deleted = true, Locked = DateTime.UtcNow, LockedBy = "test@termdat.ch" },
            new() { Id = 4, Deleted = true }
        });

        var notDeletedRecords = service.DeleteMultiple([2, 3, 4]);

        Assert.That(notDeletedRecords.Result.Count, Is.EqualTo(1));

        entryRepositoryMock.GetByIds(Arg.Any<IEnumerable<int>>()).Returns(new List<DomainModel.Entry>
            {
                new() { Id = 2, Deleted = true },
                new() { Id = 3, Deleted = true, Locked = DateTime.UtcNow, LockedBy = "test1@termdat.ch" },
                new() { Id = 4, Deleted = true, Locked = DateTime.UtcNow, LockedBy = "test2@termdat.ch" },
            });

        var notDeletedRecordsWithoutLockCheck = service.DeleteMultiple([2, 3, 4], false);
        Assert.That(notDeletedRecordsWithoutLockCheck.Result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task DeleteMultiple_WithHardForValidIds_CallsDependenciesAndDeletesEntries()
    {
        var ids = new[] { 1 };
        entry.Deleted = true;

        entryRepositoryMock.GetByIds(Arg.Any<IEnumerable<int>>()).Returns(new List<DomainModel.Entry>
        {
            entry
        });

        await service.DeleteMultiple(ids, false);

        await entryRepositoryMock.Received(1).GetByIds(ids);
        entryRepositoryMock.Received(1).Delete(Arg.Is<DomainModel.Entry>(arr => arr.Equals(entry)));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(0).GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.Received(0).QueueIndexUpdate(Arg.Any<List<IndexEntry>>());
        await searchQueueClientMock.Received(1).QueueIndexDelete(Arg.Is<IEnumerable<int>>(arr => arr.SequenceEqual(ids)));
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task DeleteMultiple_WithHardForExceptionInSearchRepository_Catches()
    {
        var ids = new[] { 1 };
        searchQueueClientMock.When(x => x.QueueIndexDelete(Arg.Is<IEnumerable<int>>(arr => arr.SequenceEqual(ids))))
            .Do(_ => throw new ArgumentException("CATCH ME"));

        entryRepositoryMock.GetByIds(Arg.Any<IEnumerable<int>>()).Returns(new List<DomainModel.Entry>
        {
            new() { Id = 1, Deleted = true }
        });

        await service.DeleteMultiple(ids, false);

        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await searchQueueClientMock.Received(1).QueueIndexDelete(Arg.Is<IEnumerable<int>>(arr => arr.SequenceEqual(ids)));
        loggerMock.Received(1).Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Any<Arg.AnyType>(), Arg.Any<Exception>(), Arg.Any<Func<Arg.AnyType, Exception, string>>());
    }

    [Test]
    public async Task DeleteMultiple_WithHardDeleteButSomeNotSoftDeleted_ShouldReturnUpdateInfo()
    {
        var ids = new[] { 1, 2, 3 };

        entryRepositoryMock.GetByIds(Arg.Any<IEnumerable<int>>()).Returns(new List<DomainModel.Entry>
        {
            new() { Id = 1, Deleted = true },
            new() { Id = 2, Deleted = false },
            new() { Id = 3, Deleted = true },
        });

        var result = (await service.DeleteMultiple(ids, false)).ToList();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].NotSoftDeletedEntryId, Is.EqualTo(2));
    }

    [Test]
    public async Task DeleteMultiple_WithSoftForExceptionInSearchRepository_Catches()
    {
        var exception = new ArgumentException("CATCH ME");
        searchQueueClientMock.When(x => x.QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>()))
            .Do(_ => throw exception);
        var ids = new[] { 1 };

        await service.DeleteMultiple(ids);

        Assert.That(entry.Deleted, Is.True);
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is<IEnumerable<int>>(arr => arr.SequenceEqual(ids)));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Is(indexEntries));
        loggerMock.Received(1).Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Any<Arg.AnyType>(), Arg.Any<Exception>(), Arg.Any<Func<Arg.AnyType, Exception, string>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task DeleteMultiple_WithSoftForValidIds_CallsDependenciesAndMarksEntryAsDeletedAndUpdatesModified()
    {
        var ids = new[] { 1 };

        await service.DeleteMultiple(ids);

        Assert.That(entry.Deleted, Is.True);
        Assert.That(entry.Modified, Is.GreaterThan(DateTime.UtcNow.AddDays(-5)));
        await entryRepositoryMock.Received(1).GetByIds(ids);
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is<IEnumerable<int>>(arr => arr.SequenceEqual(ids)));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Is(indexEntries));
        await searchQueueClientMock.Received(0).QueueIndexDelete(ids);
    }

    [Test]
    public async Task UpdateDeletesPictureWhenDeletedFlagIsProvided()
    {
        entryUpdate.PictureDeleted = true;

        await service.Update(entryUpdate);

        await pictureServiceMock.Received(1).Delete(entryUpdate.Id);
        await pictureServiceMock.Received(0).Create(Arg.Any<ResourceModel.PictureDetail>(), Arg.Any<IFormFile>());
        await pictureServiceMock.Received(0).Update(Arg.Any<ResourceModel.PictureDetail>(), Arg.Any<IFormFile>());
    }

    [Test]
    public async Task UpdateCreatesPictureWhenNoneExistsAndFilePlusPictureIsProvided()
    {
        entryUpdate.Picture = picture;
        entryUpdate.Image = formFileMock;
        await service.Update(entryUpdate);

        await pictureServiceMock.Received(0).Delete(Arg.Any<int>());
        await pictureServiceMock.Received(1).Create(picture, formFileMock);
        await pictureServiceMock.Received(0).Update(Arg.Any<ResourceModel.PictureDetail>(), Arg.Any<IFormFile>());
        await entryValidationServiceMock.Received(1).ValidatePicture(Arg.Is<DomainModel.Picture>(p => p != null));
    }

    [Test]
    public async Task UpdateUpdatesPictureWhenOneExistsAndPictureIsProvided()
    {
        entryUpdate.Picture = picture;
        entryUpdate.Image = formFileMock;

        entry.Picture = new()
        {
            EntryId = 1,
            ContentType = "image/jpg",
            FileExtension = ".jpg"
        };
        entryRepositoryMock.GetByIdForUpdate(1).Returns(entry);

        await service.Update(entryUpdate);

        await pictureServiceMock.Received(0).Delete(Arg.Any<int>());
        await pictureServiceMock.Received(0).Create(Arg.Any<ResourceModel.PictureDetail>(), Arg.Any<IFormFile>());
        await pictureServiceMock.Received(1).Update(picture, formFileMock);
        await entryValidationServiceMock.Received(1).ValidatePicture(Arg.Is<DomainModel.Picture>(p => p != null));
    }

    [Test]
    public async Task GetModificationDetail_WhenAuthorized_ReturnsEntryForModification()
    {
        var detail = await service.GetModificationDetail(1);

        await entryRepositoryMock.Received(1).GetByIdForEntryDetail(1);
        mapperMock.Received(1).Map<ResourceModel.EntryModificationDetail>(entry);

        Assert.That(detail, Is.InstanceOf<ResourceModel.EntryModificationDetail>());
        Assert.That(detail, Is.EqualTo(entryModificationDetail));
    }

    [Test]
    public void GetModificationDetail_WhenUnauthorized_ThrowsForbiddenException()
    {
        authorizationService.CanEdit(Arg.Any<DomainModel.Entry>()).Returns(false);

        Assert.ThrowsAsync<ForbiddenException>(async () => await service.GetModificationDetail(1));
    }

    [Test]
    public void GetModificationDetail_WhenLocked_ThrowsEntryLockedExceptionForDifferentUsers()
    {
        entry.Locked = DateTime.UtcNow;
        entry.LockedBy = "other@mail.ch";

        Assert.ThrowsAsync<EntryLockedException>(async () => await service.GetModificationDetail(1));
    }

    [Test]
    public async Task GetModificationDetail_WhenLocked_ReturnsForSameUser()
    {
        entry.Locked = DateTime.UtcNow;
        entry.LockedBy = TestEmail;

        var detail = await service.GetModificationDetail(1);

        await entryRepositoryMock.Received(1).GetByIdForEntryDetail(1);
        mapperMock.Received(1).Map<ResourceModel.EntryModificationDetail>(entry);

        Assert.That(detail, Is.InstanceOf<ResourceModel.EntryModificationDetail>());
        Assert.That(detail, Is.EqualTo(entryModificationDetail));
    }

    [Test]
    public async Task ShouldGetReferenceDetail()
    {
        var reference = await service.GetReferenceDetail(1);

        await entryRepositoryMock.Received(1).GetByIdForReferenceDetail(1);
        mapperMock.Received(1).Map<ResourceModel.EntryReferenceDetail>(entry);

        Assert.That(reference, Is.InstanceOf<ResourceModel.EntryReferenceDetail>());
        Assert.That(reference, Is.EqualTo(entryReferenceDetail));
    }

    [Test]
    public void ShouldThrowWhenModificationDetailIsNotFound()
    {
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await service.GetModificationDetail(2));
    }

    [Test]
    public void ShouldThrowWhenReferenceDetailIsNotFound()
    {
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await service.GetReferenceDetail(2));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldDefinition_Success()
    {
        await TestMultipleSearchAndReplace("definition");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Translations.
            Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.Definition == "Replace Replace test def")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldNote_Success()
    {
        await TestMultipleSearchAndReplace("note");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Translations.
            Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.Note == "Replace Replace test note")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldContext_Success()
    {
        await TestMultipleSearchAndReplace("context");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Translations.
            Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.Context == "Replace Replace test context")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldDefinitionSource_Success()
    {
        await TestMultipleSearchAndReplace("definitionSource");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Translations.
            Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.DefinitionSource == "Replace Replace test source")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldNoteSource_Success()
    {
        await TestMultipleSearchAndReplace("noteSource");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Translations.
            Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.NoteSource == "Replace Replace test source")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldContextSource_Success()
    {
        await TestMultipleSearchAndReplace("contextSource");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Translations.
            Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.ContextSource == "Replace Replace test source")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldMetadataSource_Success()
    {
        await TestMultipleSearchAndReplace("metadataSource");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Translations.
            Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.MetadataSource == "Replace Replace test source")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldTerminusSource_Success()
    {
        await TestMultipleSearchAndReplace("terminusSource");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Translations.
            Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.TerminusSource == "Replace Replace test source")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldNameSource_Success()
    {
        await TestMultipleSearchAndReplace("nameSource");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Translations.
            Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.NameSource == "Replace Replace test source")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldAbbreviationSource_Success()
    {
        await TestMultipleSearchAndReplace("abbreviationSource");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Translations.
            Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.AbbreviationSource == "Replace Replace test source")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldPhraseologySourceSource_Success()
    {
        await TestMultipleSearchAndReplace("phraseologySource");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Translations.
            Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.PhraseologySource == "Replace Replace test source")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldMetadata_Success()
    {
        await TestMultipleSearchAndReplace("metadata");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).
            All(x => x.Translations.Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.Metadata == "Replace Replace test metadata")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldCountry_Success()
    {
        await TestMultipleSearchAndReplace("country");
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).
            All(x => x.Translations.Where(y => entrySearchAndReplaceUpdate.LanguageIds.Contains(y.LanguageId)).All(y => y.Country == "Replace Replace test country")));
    }

    [Test]
    public async Task MultipleSearchAndReplace_FieldComment_Success()
    {
        identityServiceMock.GetCurrentEmail().Returns(TestEmailReplace);
        entrySearchAndReplaceUpdate.SearchField = "comment";

        var affectedRows = await service.MultipleSearchAndReplace(entrySearchAndReplaceUpdate);

        Assert.That(affectedRows.EntryCount, Is.EqualTo(2));
        Assert.That(affectedRows.AffectedOccurrences, Is.EqualTo(3));
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.ModifiedBy == TestEmailReplace));
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.Comment == "Replace Replace test comment"));
        Assert.That(entriesToReplace.Where(IsNotAffectedByReplace).All(x => x.ModifiedBy is null));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Is(indexEntries));
        await unitOfWorkMock.Received().SaveChangesAsync();
    }

    [Test]
    public async Task MultipleSearchAndReplace_NotFound_SuccessWithoutEffect()
    {
        identityServiceMock.GetCurrentEmail().Returns(TestEmailReplace);

        var entrySearchAndReplaceUpdateNotFound = new ResourceModel.EntrySearchAndReplaceUpdate()
        {
            Ids = new List<int> { 1 },
            SearchValue = "TestX",
            ReplaceValue = "Replace",
            LanguageIds = new List<int> { 1, 2 },
            SearchField = "comment"
        };

        var affectedRows = await service.MultipleSearchAndReplace(entrySearchAndReplaceUpdateNotFound);

        Assert.That(affectedRows.EntryCount, Is.EqualTo(0));
        Assert.That(affectedRows.AffectedOccurrences, Is.EqualTo(0));
        Assert.That(entriesToReplace.TrueForAll(x => x.ModifiedBy == null));
        Assert.That(entriesToReplace[0].Comment, Is.EqualTo("Test Replace test comment"));
        Assert.That(entriesToReplace[1].Comment, Is.EqualTo("Test Test test comment"));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Is(indexEntries));
        await unitOfWorkMock.Received().SaveChangesAsync();
    }

    [Test]
    public async Task MultipleSearchAndReplace_WithDeletedEntries_Throws()
    {
        entriesToReplace[0].Deleted = true;

        Assert.ThrowsAsync<CanNotBeChangedException>(async () => await service.MultipleSearchAndReplace(entrySearchAndReplaceUpdate));

        await entryRepositoryMock.Received(1).GetByIdsForReplace(Arg.Is(entrySearchAndReplaceUpdate.Ids), Arg.Is(entrySearchAndReplaceUpdate.SearchField), Arg.Is(entrySearchAndReplaceUpdate.SearchValue));
    }

    [Test]
    public async Task MultipleSearchAndReplace_WithNullReplaceText_Success()
    {
        identityServiceMock.GetCurrentEmail().Returns(TestEmailReplace);

        var affectedRows = await service.MultipleSearchAndReplace(entrySearchAndReplaceUpdateWithNull);

        Assert.That(affectedRows.EntryCount, Is.EqualTo(1));
        Assert.That(affectedRows.AffectedOccurrences, Is.EqualTo(1));
        Assert.That(entriesToReplaceWithNull.Where(IsAffectedByReplace).All(x => x.ModifiedBy == TestEmailReplace));
        Assert.That(entriesToReplaceWithNull.Where(IsAffectedByReplace).
                       All(x => x.Translations.Where(y => entrySearchAndReplaceUpdateWithNull.LanguageIds.Contains(y.LanguageId)).All(y => y.Note == " Replace test note")));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Is(indexEntries));
        await unitOfWorkMock.Received().SaveChangesAsync();
    }

    [Test]
    public async Task MultipleSearchAndReplace_WhenAnyEntriesAreLocked_AreNotUpdated()
    {
        entryRepositoryMock.GetByIdsForReplace(Arg.Any<IEnumerable<int>>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(new List<DomainModel.Entry>
            {
                new() { Id = 2, Locked = DateTime.UtcNow },
                new() { Id = 3, Locked = DateTime.UtcNow },
                new() { Id = 4, Locked = DateTime.UtcNow },
            });

        identityServiceMock.GetCurrentEmail().Returns(TestEmailReplace);

        var searchField = "metadata";

        entrySearchAndReplaceUpdate.SearchField = searchField;

        var affectedRows = await service.MultipleSearchAndReplace(entrySearchAndReplaceUpdate);

        Assert.That(affectedRows.EntryCount, Is.EqualTo(0));
        Assert.That(affectedRows.AffectedOccurrences, Is.EqualTo(0));
        Assert.That(entriesToReplace.Where(IsNotAffectedByReplace).All(x => x.ModifiedBy is null));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Is(indexEntries));
        await unitOfWorkMock.Received().SaveChangesAsync();
    }

    [TestCase("Das ist ein Hund", "Hund", "Tisch", "Das ist ein Tisch", 1)]
    [TestCase("Das ist ein Hund", "ein Hund", "eine Katze", "Das ist eine Katze", 1)]
    [TestCase("Die sind Hunde und Hunde", "Hund", "Katze", "Die sind Hunde und Hunde", 0)]
    [TestCase("Die sind Hunde und HUNDE", "HUNDE", "Katzen", "Die sind Hunde und Katzen", 1)]
    [TestCase("Das ist ein Hund", "ein", "der", "Das ist der Hund", 1)]
    [TestCase("Das (ist) ein Hund", "ist", "", "Das () ein Hund", 1)]
    [TestCase("Das (ist) ein Hund", "(ist)", "", "Das  ein Hund", 1)]
    [TestCase("Das ist ein Hund", "Das ist ein Hund", "", "", 1)]
    [TestCase("Das ist ein Hund", "ist", "", "Das  ein Hund", 1)]
    [TestCase("Das ist Hund", "s", "", "Das ist Hund", 0)]
    [TestCase("Das ist Hund", "und", "", "Das ist Hund", 0)]
    [TestCase("Das ist ein HUND", "Hund", "", "Das ist ein HUND", 0)]
    [TestCase("Das ist ein hunD", "Hund", "", "Das ist ein hunD", 0)]
    [TestCase("Das ist-ein Hund", "ist", "", "Das -ein Hund", 1)]
    [TestCase("Das -ist ein Hund", "ist", "", "Das - ein Hund", 1)]
    [TestCase("Das ist-ein Hund", "ein", "", "Das ist- Hund", 1)]
    [TestCase("Das ist- ein Hund", "ist-", "", "Das  ein Hund", 1)]
    [TestCase("Das ist- ein Hund", "Das", "", " ist- ein Hund", 1)]
    [TestCase("Das ist Hund und Hund und Hund", "Hund", "", "Das ist  und  und ", 3)]
    [TestCase("Das ist Hund und Hunde und Hund", "Hund", "", "Das ist  und Hunde und ", 2)]
    [TestCase("Das ist Hund und hund und Hund", "Hund", "", "Das ist  und hund und ", 2)]
    [TestCase("Test ([BK, Ausserparlamentarische Kommissionen EFD, 2014-07-10, Internet](http://www.admin.ch/ch/d/cf/ko/index_efd.html)) Ende", "([BK, Ausserparlamentarische Kommissionen EFD, 2014-07-10, Internet](http://www.admin.ch/ch/d/cf/ko/index_efd.html))", "BK, Ausserparlamentarische Kommissionen EFD ([Internet, 2023-05-02](https://www.admin.ch/ch/d/cf/ko/departement_6.html))", "Test BK, Ausserparlamentarische Kommissionen EFD ([Internet, 2023-05-02](https://www.admin.ch/ch/d/cf/ko/departement_6.html)) Ende", 1)]
    [TestCase("Test beim Eidgenössischen Finanzdepartement (EFD) Ende", "beim Eidgenössischen Finanzdepartement (EFD)", "beim Eidgenössischen Finanzdepartement", "Test beim Eidgenössischen Finanzdepartement Ende", 1)]
    [TestCase("(Kolumbus-)Kommission", "(Kolumbus-)Kommission", "Colombokommission", "Colombokommission", 1)]
    [TestCase("HIS: seit 1913 ?", "HIS: seit 1913 ?", "HIS: seit 1913?", "HIS: seit 1913?", 1)]
    [TestCase("Das ist ein Hund,", "Hund", "Tisch", "Das ist ein Tisch,", 1)]
    [TestCase("Das ist ein Hund;", "Hund", "Tisch", "Das ist ein Tisch;", 1)]
    [TestCase("Das ist, ein Hund", "ist", "hier", "Das hier, ein Hund", 1)]
    [TestCase("Das ist; ein Hund", "ist", "hier", "Das hier; ein Hund", 1)]
    [TestCase("Das ,ist ein Hund", "ist", "hier", "Das ,hier ein Hund", 1)]
    [TestCase("Das ;ist ein Hund", "ist", "hier", "Das ;hier ein Hund", 1)]
    [TestCase("Test ([BK, Ausserparlamentarische Kommissionen EFD, 2014-07-10, Internet](http://www.admin.ch/ch/d/cf/ko/index_efd.html)) Ende", "http://www.admin.ch/ch/d/cf/ko/index_efd.html", "http://www.admin.ch/ch/d/cf/ko/departement_6.html", "Test ([BK, Ausserparlamentarische Kommissionen EFD, 2014-07-10, Internet](http://www.admin.ch/ch/d/cf/ko/departement_6.html)) Ende", 1)]
    [TestCase("Test ([BK, Ausserparlamentarische Kommissionen EFD, 2014-07-10, Internet](http://www.admin.ch/ch/d/cf/ko/index_efd.html)) Ende", "2014-07-10", "2015-10-22", "Test ([BK, Ausserparlamentarische Kommissionen EFD, 2015-10-22, Internet](http://www.admin.ch/ch/d/cf/ko/index_efd.html)) Ende", 1)]
    [TestCase("Test ([BK, Ausserparlamentarische Kommissionen EFDwww.admin.ch, 2014-07-10, Internet](http://www.admin.ch/ch/d/cf/ko/index_efd.html)) Ende", "www.admin.ch", "www.test.org", "Test ([BK, Ausserparlamentarische Kommissionen EFDwww.admin.ch, 2014-07-10, Internet](http://www.test.org/ch/d/cf/ko/index_efd.html)) Ende", 1)]
    [TestCase("Test ([BK, Ausserparlamentarische Kommissionen EFD, 2014-07-10, Internet](http://www.admin.ch/ch/d/cf/ko/index_efd.html)) Ende", "http://www.admin.ch/ch/d/cf/ko", "http://www.test.ord/de/d/ct/ok", "Test ([BK, Ausserparlamentarische Kommissionen EFD, 2014-07-10, Internet](http://www.test.ord/de/d/ct/ok/index_efd.html)) Ende", 1)]
    [TestCase("Test ([BK, Ausserparlamentarische Kommissionen EFD, 2014-07-10, Internet](http://www.admin.ch/ch/d/cf/ko/index_efd.html)) Ende", "http", "https", "Test ([BK, Ausserparlamentarische Kommissionen EFD, 2014-07-10, Internet](https://www.admin.ch/ch/d/cf/ko/index_efd.html)) Ende", 1)]
    [TestCase("Test (BK, Ausserparlamentarische Kommissionen EFD, Internet, 2014-07-10)", "2014-07-10", "2015-10-22", "Test (BK, Ausserparlamentarische Kommissionen EFD, Internet, 2015-10-22)", 1)]
    [TestCase("Test:2024-01-01", "2024-01-01", "2025-02-02", "Test:2025-02-02", 1)]
    [TestCase("Test-2024-01-01(", "2024-01-01", "2025-02-02", "Test-2025-02-02(", 1)]
    [TestCase("Test-2024-01-01( und ohne Separator ignorieren2024-01-01hier", "2024-01-01", "2025-02-02", "Test-2025-02-02( und ohne Separator ignorieren2024-01-01hier", 1)]
    [TestCase("Test)2024-01-01(", "2024-01-01", "2025-02-02", "Test)2025-02-02(", 1)]
    [TestCase("Test.2024-01-01_", "2024-01-01", "2025-02-02", "Test.2025-02-02_", 1)]
    [TestCase("Test;2024-01-01-", "2024-01-01", "2025-02-02", "Test;2025-02-02-", 1)]
    [TestCase("Test%2024-01-01&", "2024-01-01", "2025-02-02", "Test%2025-02-02&", 1)]
    [TestCase("Test\\2024-01-01/", "2024-01-01", "2025-02-02", "Test\\2025-02-02/", 1)]
    [TestCase("Test>2024-01-01<", "2024-01-01", "2025-02-02", "Test>2025-02-02<", 1)]
    [TestCase("Test=2024-01-01#", "2024-01-01", "2025-02-02", "Test=2025-02-02#", 1)]
    [TestCase("Hier sind viele Datums, 2024-01-01# :2024-01-01] -2024-01-01( und ohne Separator ignorieren2024-01-01hier )2024-01-01( .2024-01-01_ ;2024-01-01- %2024-01-01& \\2024-01-01/ >2024-01-01< =2024-01-01# =2024-01-01", "2024-01-01", "2025-02-02", "Hier sind viele Datums, 2025-02-02# :2025-02-02] -2025-02-02( und ohne Separator ignorieren2024-01-01hier )2025-02-02( .2025-02-02_ ;2025-02-02- %2025-02-02& \\2025-02-02/ >2025-02-02< =2025-02-02# =2025-02-02", 11)]
    public async Task MultipleSearchAndReplace_WhenCalled_ReplacesProperly(string sourceText, string searchText, string replaceText, string expectedText, int expectedOccurences)
    {
        identityServiceMock.GetCurrentEmail().Returns(TestEmailReplace);

        var entryListToReplace = new List<DomainModel.Entry>()
        {
            new()
            {
                Id = 3,
                OfficeId = 1,
                CollectionId = 1,
                Comment = sourceText
            },
        };

        var entrySearchAndReplace = new ResourceModel.EntrySearchAndReplaceUpdate
        {
            Ids = new List<int> { 3 },
            SearchValue = searchText,
            ReplaceValue = replaceText,
            SearchField = "comment"
        };

        entryRepositoryMock.GetByIdsForReplace(Arg.Is(entrySearchAndReplace.Ids), Arg.Any<string>(), Arg.Any<string>()).Returns(entryListToReplace);

        var affectedRows = await service.MultipleSearchAndReplace(entrySearchAndReplace);

        Assert.That(affectedRows.AffectedOccurrences, Is.EqualTo(expectedOccurences));

        Assert.That(entryListToReplace[0].Comment, Is.EqualTo(expectedText));

        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Is(indexEntries));
        await unitOfWorkMock.Received().SaveChangesAsync();
    }

    [Test]
    public async Task MultipleSearchAndReplace_WithNote_ShouldDetermineEntryNotePatterns()
    {
        var entryListToReplace = new List<DomainModel.Entry>
        {
            new()
            {
                Id = 3,
                OfficeId = 1,
                CollectionId = 1,
                Translations = new List<DomainModel.EntryLanguage>
                {
                    new() { LanguageId = 1, Note = "Anmerkung Test" }
                }
            },
        };
        var entrySearchAndReplace = new ResourceModel.EntrySearchAndReplaceUpdate
        {
            Ids = new List<int> { 3 },
            SearchValue = "Anmerkung",
            ReplaceValue = "Ersatz Test",
            SearchField = ResourceModel.EntrySearchAndReplaceUpdate.Note,
            LanguageIds = new[] { 1 }
        };
        entryRepositoryMock.GetByIdsForReplace(Arg.Is(entrySearchAndReplace.Ids), Arg.Any<string>(), Arg.Any<string>()).Returns(entryListToReplace);

        await service.MultipleSearchAndReplace(entrySearchAndReplace);

        entryNotePatternServiceMock.Received(1).DetermineEntryNotePatterns(Arg.Any<string>());
    }

    [Test]
    public async Task Lock_WhenCalled_UnlocksEntry()
    {
        Assert.Multiple(() =>
        {
            Assert.That(entry.Locked, Is.Null);
            Assert.That(entry.LockedBy, Is.Null);
        });

        var result = await service.Lock(entry.Id);

        Assert.Multiple(() =>
        {
            Assert.That(entry.Locked, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
            Assert.That(entry.LockedBy, Is.EqualTo(TestEmail));
            Assert.That(result, Is.EqualTo(entry.ConcurrencyToken));
        });

        await entryRepositoryMock.Received(1).GetByIdForUpdate(entry.Id);
        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task Lock_WhenUnauthorized_ThrowsForbiddenException()
    {
        authorizationService.CanEdit(Arg.Any<DomainModel.Entry>()).Returns(false);

        Assert.ThrowsAsync<ForbiddenException>(async () => await service.Lock(entry.Id));

        await entryRepositoryMock.Received(1).GetByIdForUpdate(Arg.Is(entry.Id));
        authorizationService.Received(1).CanEdit(entry);
    }

    [Test]
    public async Task Lock_WhenDeleted_ThrowsCanNotBeChangedException()
    {
        entry.Deleted = true;

        Assert.ThrowsAsync<CanNotBeChangedException>(async () => await service.Lock(entry.Id));

        await entryRepositoryMock.Received(1).GetByIdForUpdate(Arg.Is(entry.Id));
    }

    [Test]
    public async Task Lock_ForUnknownEntry_ThrowsEntityNotFoundException()
    {
        entryRepositoryMock.GetByIdForUpdate(Arg.Any<int>()).ReturnsNullForAnyArgs();

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await service.Lock(entry.Id));

        await entryRepositoryMock.Received(1).GetByIdForUpdate(Arg.Is(entry.Id));
    }

    [Test]
    public async Task Unlock_WhenCalled_UnlocksEntry()
    {
        entry.Locked = DateTime.UtcNow.AddMinutes(-1);
        entry.LockedBy = TestEmail;

        var result = await service.Unlock(entry.Id);

        Assert.Multiple(() =>
        {
            Assert.That(entry.Locked, Is.Null);
            Assert.That(entry.LockedBy, Is.Null);
            Assert.That(result, Is.EqualTo(entry.ConcurrencyToken));
        });

        await entryRepositoryMock.Received(1).GetByIdForUpdate(entry.Id);
        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task Unlock_WhenUnauthorized_ThrowsForbiddenException()
    {
        authorizationService.CanEdit(Arg.Any<DomainModel.Entry>()).Returns(false);

        Assert.ThrowsAsync<ForbiddenException>(async () => await service.Unlock(entry.Id));

        await entryRepositoryMock.Received(1).GetByIdForUpdate(Arg.Is(entry.Id));
        authorizationService.Received(1).CanEdit(entry);
    }

    [Test]
    public async Task Unlock_WhenDeleted_ThrowsCanNotBeChangedException()
    {
        entry.Deleted = true;

        Assert.ThrowsAsync<CanNotBeChangedException>(async () => await service.Unlock(entry.Id));

        await entryRepositoryMock.Received(1).GetByIdForUpdate(Arg.Is(entry.Id));
    }

    [Test]
    public async Task Unlock_ForUnknownEntry_ThrowsEntityNotFoundException()
    {
        entryRepositoryMock.GetByIdForUpdate(Arg.Any<int>()).ReturnsNullForAnyArgs();

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await service.Unlock(entry.Id));

        await entryRepositoryMock.Received(1).GetByIdForUpdate(Arg.Is(entry.Id));
    }

    [Test]
    public async Task UnlockExpiredEntries_WhenCalled_UnlocksAllLockedEntriesOlderThan120Minutes()
    {
        var lockedEntries = new List<DomainModel.Entry>
        {
            new() { Id = 10, Locked = DateTime.UtcNow.AddMinutes(-120), LockedBy = TestEmail },
            new() { Id = 11, Locked = DateTime.UtcNow.AddMinutes(-120), LockedBy = TestEmail },
            new() { Id = 12, Locked = DateTime.UtcNow.AddMinutes(-120), LockedBy = TestEmail },
        };
        entryRepositoryMock.GetExpiredLockedEntries(Arg.Any<int>()).Returns(lockedEntries);

        await service.UnlockExpiredEntries();

        Assert.Multiple(() =>
        {
            Assert.That(lockedEntries[0].Locked, Is.Null);
            Assert.That(lockedEntries[0].LockedBy, Is.Null);

            Assert.That(lockedEntries[1].Locked, Is.Null);
            Assert.That(lockedEntries[1].LockedBy, Is.Null);

            Assert.That(lockedEntries[2].Locked, Is.Null);
            Assert.That(lockedEntries[2].LockedBy, Is.Null);
        });

        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    private static bool IsAffectedByReplace(DomainModel.Entry entry)
    {
        return entry.Id is 1 or 2;
    }

    private static bool IsNotAffectedByReplace(DomainModel.Entry entry)
    {
        return entry.Id == 3;
    }

    private async Task TestMultipleSearchAndReplace(string searchField)
    {
        identityServiceMock.GetCurrentEmail().Returns(TestEmailReplace);
        entrySearchAndReplaceUpdate.SearchField = searchField;

        var affectedRows = await service.MultipleSearchAndReplace(entrySearchAndReplaceUpdate);

        Assert.That(affectedRows.EntryCount, Is.EqualTo(2));
        Assert.That(affectedRows.AffectedOccurrences, Is.EqualTo(6));
        Assert.That(entriesToReplace.Where(IsAffectedByReplace).All(x => x.ModifiedBy == TestEmailReplace));
        Assert.That(entriesToReplace.Where(IsNotAffectedByReplace).All(x => x.ModifiedBy is null));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Is(indexEntries));
        await unitOfWorkMock.Received().SaveChangesAsync();
    }

    private static DomainModel.Entry CreateTestEntry()
    {
        return new DomainModel.Entry
        {
            Id = 1,
            OfficeId = 1,
            CollectionId = 1,
            ConceptSystemPositionId = 1,
            ModifiedBy = "evil@kanevil.com",
            Translations = new List<DomainModel.EntryLanguage>
            {
                new() { Id = 1, Sequence = 1, LanguageId = 1, Language = new DomainModel.Language { SortIndex = 1} },
                new() { Id = 1, Sequence = 1, LanguageId = 2, Language = new DomainModel.Language { SortIndex = 1} },
                new() { Id = 1, Sequence = 1, LanguageId = 3, Language = new DomainModel.Language { SortIndex = 1} },
                new() { Id = 1, Sequence = 1, LanguageId = 4, Language = new DomainModel.Language { SortIndex = 1} },
            },
            Collection = new DomainModel.Collection
            {
                Translations = new List<DomainModel.CollectionTranslation>
                {
                    new() { LanguageId = 1 },
                    new() { LanguageId = 2 },
                    new() { LanguageId = 3 },
                    new() { LanguageId = 4 }
                }
            },
            Office = new DomainModel.Office(),
            ConceptSystemPosition = new DomainModel.ConceptSystemPosition(),
            RelatedCollections = new List<DomainModel.Collection>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            },
            LenochClassifications = new List<DomainModel.LenochClassification>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            },
            AdminInfos = new List<DomainModel.AdminInfo>
            {
                new() { Id = 1, Code = "Code1"},
                new() { Id = 2, Code = "Code2"},
            },
            Glossaries = new List<DomainModel.Glossary>
            {
                new() { Id = 1 }
            },
            ConcurrencyToken = 10
        };
    }

    private static IMapper GetConfiguredMapper()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        return new MapperConfiguration(x =>
        {
            x.CreateMap<DomainModel.Entry, ResourceModel.EntryDetail>();
            x.CreateMap<DomainModel.Office, ResourceModel.OfficeSummary>();
            x.CreateMap<DomainModel.Collection, ResourceModel.CollectionSummary>();
            x.CreateMap<DomainModel.ConceptSystemPosition, ResourceModel.ConceptSystemPositionSummary>();
            x.CreateMap<DomainModel.LenochClassification, ResourceModel.LenochClassificationSummary>();
            x.CreateMap<DomainModel.Glossary, ResourceModel.GlossarySummary>();
            x.CreateMap<DomainModel.AdminInfo, ResourceModel.AdminInfoSummary>();
        }, loggerFactory).CreateMapper();
    }
}
