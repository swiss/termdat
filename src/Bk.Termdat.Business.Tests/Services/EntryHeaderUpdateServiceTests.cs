using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using ResourceModel = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class EntryHeaderUpdateServiceTests
{
    private IEntryHeaderUpdateService service;

    private readonly IEntryValidationService entryValidationServiceMock = Substitute.For<IEntryValidationService>();
    private readonly IIdentityService identityServiceMock = Substitute.For<IIdentityService>();
    private readonly IUserAccountService userAccountServiceMock = Substitute.For<IUserAccountService>();
    private readonly IAdminInfoService adminInfoServiceMock = Substitute.For<IAdminInfoService>();
    private readonly IEntryRepository entryRepositoryMock = Substitute.For<IEntryRepository>();
    private readonly ISearchQueueClient searchQueueClientMock = Substitute.For<ISearchQueueClient>();
    private readonly IAuthorizationService authorizationService = Substitute.For<IAuthorizationService>();
    private readonly IUnitOfWork unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IMapper mapperMock = Substitute.For<IMapper>();
    private const string TestEmail = "unit@test.ch";

    private Entry entry;
    private Entry entry1;
    private Entry entry2;
    private Entry[] entriesForSourceUpdate;
    private Entry[] entriesForContentMove;

    private EntryHeaderUpdate entryHeaderUpdate;

    [SetUp]
    public void SetUp()
    {
        entry = CreateTestEntry();
        entryHeaderUpdate = new EntryHeaderUpdate
        {
            Ids = new List<int> { 1 },
            OfficeId = 2,
            CollectionId = 2,
            Status = ResourceModel.Status.InProgress,
            Reliability = ResourceModel.Reliability.Tentative,
            ReplaceSubjects = false
        };

        entryRepositoryMock.GetByIds(Arg.Any<IEnumerable<int>>()).Returns(new List<Entry> { entry });
        entryRepositoryMock.GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(new List<int> { 1 }))).Returns(new List<Entry> { entry });

        entry1 = CreateTestEntry(1);
        entry1.ConceptSystemPositionId = 1;
        entry2 = CreateTestEntry(2);
        entry2.ConceptSystemPositionId = 1;

        entryRepositoryMock.GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(new List<int> { 1, 2 }))).Returns(new List<Entry> { entry1, entry2 });

        identityServiceMock.GetCurrentEmail().Returns(TestEmail);
        entryValidationServiceMock.ValidateSubjects(Arg.Any<IEntryModification>()).Returns(new List<Subject>
        {
            new() { Id = 2 }
        });
        entryValidationServiceMock.ValidateDomains(Arg.Any<IEntryModification>()).Returns(new List<Domain.Models.Domain>
        {
            new() { Id = 4 }
        });
        entriesForSourceUpdate = [
            new()
            {
                Id = 1,
                OfficeId = 1,
                CollectionId = 1,
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 1,
                        Terminus = "Terminus 1",
                        Abbreviation = "Abbreviation 1",
                        Name = "Name 1",
                        Context = "Context 1",
                        Definition = "Definition 1",
                        Note = "Note 1",
                        Metadata = "Metadata 1",
                        Phraseology = "Phraseology 1",
                        TerminusSource = "TerminusSource 1",
                        AbbreviationSource = "AbbreviationSource 1",
                        NameSource = "NameSource 1",
                        ContextSource = "ContextSource 1",
                        DefinitionSource = "DefinitionSource 1",
                        NoteSource = "NoteSource 1",
                        MetadataSource = "MetadataSource 1",
                        PhraseologySource = "PhraseologySource 1"
                    },
                    new()
                    {
                        LanguageId = 2,
                        EntryId = 1,
                        Terminus = "Terminus 2",
                        Abbreviation = "Abbreviation 2",
                        Name = "Name 2",
                        Context = "Context 2",
                        Definition = "Definition 2",
                        Note = "Note 2",
                        Metadata = "Metadata 2",
                        Phraseology = "Phraseology 2",
                        TerminusSource = "TerminusSource 2",
                        AbbreviationSource = "AbbreviationSource 2",
                        NameSource = "NameSource 2",
                        ContextSource = "ContextSource 2",
                        DefinitionSource = "DefinitionSource 2",
                        NoteSource = "NoteSource 2",
                        MetadataSource = "MetadataSource 2",
                        PhraseologySource = "PhraseologySource 2"
                    }
                },
                Comment = "Test Replace test comment"
            },
            new()
            {
                Id = 2,
                OfficeId = 2,
                CollectionId = 3,
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 2,
                        Terminus = "Terminus 1",
                        Abbreviation = "Abbreviation 1",
                        Name = "Name 1",
                        Context = "Context 1",
                        Definition = "Definition 1",
                        Note = "Note 1",
                        Metadata = "Metadata 1",
                        Phraseology = "Phraseology 1",
                        TerminusSource = "TerminusSource 1",
                        AbbreviationSource = "AbbreviationSource 1",
                        NameSource = "NameSource 1",
                        ContextSource = "ContextSource 1",
                        DefinitionSource = "DefinitionSource 1",
                        NoteSource = "NoteSource 1",
                        MetadataSource = "MetadataSource 1",
                        PhraseologySource = "PhraseologySource 1"
                    },
                    new()
                    {
                        LanguageId = 2,
                        EntryId = 2,
                        Terminus = "Terminus 2",
                        Abbreviation = "Abbreviation 2",
                        Name = "Name 2",
                        Context = "Context 2",
                        Definition = "Definition 2",
                        Note = "Note 2",
                        Metadata = "Metadata 2",
                        Phraseology = "Phraseology 2",
                        TerminusSource = "TerminusSource 2",
                        AbbreviationSource = "AbbreviationSource 2",
                        NameSource = "NameSource 2",
                        ContextSource = "ContextSource 2",
                        DefinitionSource = "DefinitionSource 2",
                        NoteSource = "NoteSource 2",
                        MetadataSource = "MetadataSource 2",
                        PhraseologySource = "PhraseologySource 2"
                    },
                    new()
                    {
                        LanguageId = 3,
                        EntryId = 2,
                        Terminus = "Terminus 3",
                        Abbreviation = "Abbreviation 3",
                        Name = "Name 3",
                        Context = "Context 3",
                        Definition = "Definition 3",
                        Note = "Note 3",
                        Metadata = "Metadata 3",
                        Phraseology = "Phraseology 3",
                        TerminusSource = "TerminusSource 3",
                        AbbreviationSource = "AbbreviationSource 3",
                        NameSource = "NameSource 3",
                        ContextSource = "ContextSource 3",
                        DefinitionSource = "DefinitionSource 3",
                        NoteSource = "NoteSource 3",
                        MetadataSource = "MetadataSource 3",
                        PhraseologySource = "PhraseologySource 3"
                    }
                },
                Comment = "Test Test test comment"
            },
            new()
            {
                Id = 3,
                OfficeId = 2,
                CollectionId = 3,
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 3,
                        Terminus = "Terminus 1",
                        Abbreviation = "Abbreviation 1",
                        Name = "Name 1",
                        Context = "Context 1",
                        Definition = "Definition 1",
                        Note = "Note 1",
                        Metadata = "Metadata 1",
                        Phraseology = "Phraseology 1",
                        TerminusSource = "TerminusSource 1",
                        AbbreviationSource = "AbbreviationSource 1",
                        NameSource = "NameSource 1",
                        ContextSource = "ContextSource 1",
                        DefinitionSource = "DefinitionSource 1",
                        NoteSource = "NoteSource 1",
                        MetadataSource = "",
                        PhraseologySource = ""
                    },
                    new()
                    {
                        LanguageId = 2,
                        EntryId = 3,
                        Terminus = "Terminus 2",
                        Abbreviation = "Abbreviation 2",
                        Name = "Name 2",
                        Context = "Context 2",
                        Definition = "Definition 2",
                        Note = "Note 2",
                        Metadata = "Metadata 2",
                        Phraseology = "Phraseology 2",
                        TerminusSource = "TerminusSource 2",
                        AbbreviationSource = "AbbreviationSource 2",
                        NameSource = "NameSource 2",
                        ContextSource = "ContextSource 2",
                        DefinitionSource = "DefinitionSource 2",
                        NoteSource = "NoteSource 2",
                        MetadataSource = "MetadataSource 2",
                        PhraseologySource = "PhraseologySource 2"
                    },
                    new()
                    {
                        LanguageId = 3,
                        EntryId = 3,
                        Terminus = "Terminus 3",
                        Abbreviation = "Abbreviation 3",
                        Name = "Name 3",
                        Context = "Context 3",
                        Definition = "Definition 3",
                        Note = "Note 3",
                        Metadata = "Metadata 3",
                        Phraseology = "Phraseology 3",
                        TerminusSource = "TerminusSource 3",
                        AbbreviationSource = "AbbreviationSource 3",
                        NameSource = "NameSource 3",
                        ContextSource = "ContextSource 3",
                        DefinitionSource = "DefinitionSource 3",
                        NoteSource = "NoteSource 3",
                        MetadataSource = "MetadataSource 3",
                        PhraseologySource = "PhraseologySource 3"
                    }
                },
                Comment = "comment"
            },
            new()
            {
                Id = 4,
                OfficeId = 4,
                CollectionId = 5,
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 4,
                        Terminus = "Terminus 1",
                        Abbreviation = "",
                        Name = "",
                        Context = "",
                        Definition = "",
                        Note = "",
                        Metadata = "",
                        Phraseology = "",
                        TerminusSource = "",
                        AbbreviationSource = "",
                        NameSource = "",
                        ContextSource = "",
                        DefinitionSource = "",
                        NoteSource = "",
                        MetadataSource = "",
                        PhraseologySource = "",
                        Sequence = 1
                    },
                    new()
                    {
                        LanguageId = 2,
                        EntryId = 4,
                        Terminus = "Terminus 1",
                        Abbreviation = "",
                        Name = "",
                        Context = "",
                        Definition = "",
                        Note = "",
                        Metadata = "",
                        Phraseology = "",
                        TerminusSource = "",
                        AbbreviationSource = "",
                        NameSource = "",
                        ContextSource = "",
                        DefinitionSource = "",
                        NoteSource = "",
                        MetadataSource = "",
                        PhraseologySource = "",
                        Sequence = 1
                    }
                },
            }];

        entriesForContentMove = [
            new()
            {
                Id = 1,
                OfficeId = 1,
                CollectionId = 1,
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 1,
                        Terminus = "Terminus 1",
                        Abbreviation = "Abbreviation 1",
                        Name = "Name 1",
                        Context = "Context 1",
                        Definition = "",
                        Note = "Note 1",
                        Metadata = "Metadata 1",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 1",
                        AbbreviationSource = "AbbreviationSource 1",
                        NameSource = "NameSource 1",
                        ContextSource = "ContextSource 1",
                        DefinitionSource = "",
                        NoteSource = "",
                        MetadataSource = "MetadataSource 1",
                        PhraseologySource = "",
                        Sequence = 1
                    },
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 1,
                        Terminus = "Terminus 1",
                        Abbreviation = "Abbreviation 1",
                        Name = "",
                        Context = "Context 1",
                        Definition = "",
                        Note = "Note 1",
                        Metadata = "Metadata 1",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 1",
                        AbbreviationSource = "AbbreviationSource 1",
                        NameSource = "NameSource 1",
                        ContextSource = "ContextSource 1",
                        DefinitionSource = "DefinitionSource 1",
                        NoteSource = "",
                        MetadataSource = "MetadataSource 1",
                        PhraseologySource = "",
                        Sequence = 2
                    },
                    new()
                    {
                        LanguageId = 2,
                        EntryId = 1,
                        Terminus = "Terminus 2",
                        Abbreviation = "Abbreviation 2",
                        Name = "Name 2",
                        Context = "Context 2",
                        Definition = "",
                        Note = "Note 2",
                        Metadata = "Metadata 2",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 2",
                        AbbreviationSource = "AbbreviationSource 2",
                        NameSource = "NameSource 2",
                        ContextSource = "ContextSource 2",
                        DefinitionSource = "",
                        NoteSource = "NoteSource 2",
                        MetadataSource = "MetadataSource 2",
                        PhraseologySource = "",
                        Sequence = 1
                    }
                },
            },
            new()
            {
                Id = 2,
                OfficeId = 2,
                CollectionId = 3,
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 2,
                        Terminus = "Terminus 1",
                        Abbreviation = "Abbreviation 1",
                        Name = "Name 1",
                        Context = "Context 1",
                        Definition = "",
                        Note = "Note 1",
                        Metadata = "Metadata 1",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 1",
                        AbbreviationSource = "AbbreviationSource 1",
                        NameSource = "NameSource 1",
                        ContextSource = "ContextSource 1",
                        DefinitionSource = "",
                        NoteSource = "NoteSource 1",
                        MetadataSource = "MetadataSource 1",
                        PhraseologySource = "",
                        Sequence = 1
                    },
                    new()
                    {
                        LanguageId = 2,
                        EntryId = 2,
                        Terminus = "Terminus 2",
                        Abbreviation = "Abbreviation 2",
                        Name = "Name 2",
                        Context = "Context 2",
                        Definition = "",
                        Note = "Note 2",
                        Metadata = "Metadata 2",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 2",
                        AbbreviationSource = "AbbreviationSource 2",
                        NameSource = "NameSource 2",
                        ContextSource = "ContextSource 2",
                        DefinitionSource = "",
                        NoteSource = "NoteSource 2",
                        MetadataSource = "MetadataSource 2",
                        PhraseologySource = "",
                        Sequence = 1
                    },
                    new()
                    {
                        LanguageId = 3,
                        EntryId = 2,
                        Terminus = "Terminus 3",
                        Abbreviation = "Abbreviation 3",
                        Name = "Name 3",
                        Context = "Context 3",
                        Definition = "",
                        Note = "Note 3",
                        Metadata = "Metadata 3",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 3",
                        AbbreviationSource = "AbbreviationSource 3",
                        NameSource = "NameSource 3",
                        ContextSource = "ContextSource 3",
                        DefinitionSource = "",
                        NoteSource = "NoteSource 3",
                        MetadataSource = "MetadataSource 3",
                        PhraseologySource = "",
                        Sequence = 1
                    }
                },
            },
            new()
            {
                Id = 3,
                OfficeId = 2,
                CollectionId = 3,
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 3,
                        Terminus = "Terminus 1",
                        Abbreviation = "Abbreviation 1",
                        Name = "Name 1",
                        Context = "Context 1",
                        Definition = "",
                        Note = "Note 1",
                        Metadata = "Metadata 1",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 1",
                        AbbreviationSource = "AbbreviationSource 1",
                        NameSource = "NameSource 1",
                        ContextSource = "ContextSource 1",
                        DefinitionSource = "",
                        NoteSource = "NoteSource 1",
                        MetadataSource = "",
                        PhraseologySource = "",
                        Sequence = 1
                    },
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 3,
                        Terminus = "Terminus 1",
                        Abbreviation = "Abbreviation 1",
                        Name = "Name 1",
                        Context = "Context 1",
                        Definition = "Definition 1",
                        Note = "Note 1",
                        Metadata = "Metadata 1",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 1",
                        AbbreviationSource = "AbbreviationSource 1",
                        NameSource = "NameSource 1",
                        ContextSource = "ContextSource 1",
                        DefinitionSource = "DefinitionSource 1",
                        NoteSource = "NoteSource 1",
                        MetadataSource = "MetadataSource 1",
                        PhraseologySource = "",
                        Sequence = 2
                    },
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 3,
                        Terminus = "Terminus 1",
                        Abbreviation = "Abbreviation 1",
                        Name = "Name 1",
                        Context = "Context 1",
                        Definition = "Definition 1",
                        Note = "Note 1",
                        Metadata = "Metadata 1",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 1",
                        AbbreviationSource = "AbbreviationSource 1",
                        NameSource = "NameSource 1",
                        ContextSource = "ContextSource 1",
                        DefinitionSource = "DefinitionSource 1",
                        NoteSource = "NoteSource 1",
                        MetadataSource = "MetadataSource 1",
                        PhraseologySource = "",
                        Sequence = 3
                    }
                }
            },
            new()
            {
                Id = 4,
                OfficeId = 4,
                CollectionId = 5,
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 4,
                        Terminus = "Terminus 1",
                        Abbreviation = "",
                        Name = "",
                        Context = "",
                        Definition = "",
                        Note = "",
                        Metadata = "",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 1",
                        AbbreviationSource = "",
                        NameSource = "",
                        ContextSource = "",
                        DefinitionSource = "",
                        NoteSource = "",
                        MetadataSource = "",
                        PhraseologySource = "",
                        Sequence = 1
                    },
                    new()
                    {
                        LanguageId = 1,
                        EntryId = 4,
                        Terminus = "Terminus 1",
                        Abbreviation = "",
                        Name = "",
                        Context = "",
                        Definition = "",
                        Note = "",
                        Metadata = "",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 1",
                        AbbreviationSource = "",
                        NameSource = "",
                        ContextSource = "",
                        DefinitionSource = "",
                        NoteSource = "",
                        MetadataSource = "",
                        PhraseologySource = "",
                        Sequence = 2
                    },
                    new()
                    {
                        LanguageId = 2,
                        EntryId = 4,
                        Terminus = "Terminus 2",
                        Abbreviation = "Abbreviation 2",
                        Name = "",
                        Context = "",
                        Definition = "",
                        Note = "",
                        Metadata = "",
                        Phraseology = "",
                        TerminusSource = "TerminusSource 2",
                        AbbreviationSource = "AbbreviationSource 2",
                        NameSource = "",
                        ContextSource = "",
                        DefinitionSource = "",
                        NoteSource = "",
                        MetadataSource = "",
                        PhraseologySource = "",
                        Sequence = 1
                    }
                }
            }];

        authorizationService.CanEdit(Arg.Any<Entry>()).Returns(true);
        authorizationService.IsAdmin().Returns(true);
        service = new EntryHeaderUpdateService(entryValidationServiceMock,
            userAccountServiceMock,
            identityServiceMock,
            adminInfoServiceMock,
            unitOfWorkMock,
            entryRepositoryMock,
            searchQueueClientMock,
            authorizationService,
            mapperMock,
            new NullLogger<EntryHeaderUpdateService>());
    }

    [TearDown]
    public void TearDown()
    {
        entryValidationServiceMock.ClearSubstitute();
        identityServiceMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
        searchQueueClientMock.ClearSubstitute();
        entryRepositoryMock.ClearSubstitute();
        mapperMock.ClearSubstitute();
        userAccountServiceMock.ClearSubstitute();
        authorizationService.ClearSubstitute();
    }

    [Test]
    public void UpdateMandatoryHeaderFields_ForEntryWithoutPermission_ShouldThrowForbiddenException()
    {
        authorizationService.CanEdit(Arg.Is<Entry>(y => y.Id == 1)).Returns(false);
        entryRepositoryMock.GetByIdsForUpdate(Arg.Is(entryHeaderUpdate.Ids)).Returns([new() { Id = 1, Deleted = false }]);

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(new Entry());

        Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateMandatoryHeaderFields(entryHeaderUpdate));
    }

    [Test]
    public void UpdateMandatoryHeaderFields_ForEntryWithDeletedFlag_ShouldThrowCanNotBeChangedException()
    {
        entryRepositoryMock.GetByIdsForUpdate(Arg.Is(entryHeaderUpdate.Ids)).Returns([new() { Id = 1, Deleted = true }]);

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(new Entry());

        Assert.ThrowsAsync<CanNotBeChangedException>(() => service.UpdateMandatoryHeaderFields(entryHeaderUpdate));
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_ReplaceSubjectAndDomainReplace_CallsDependencies()
    {
        entryHeaderUpdate.SubjectIds = [2];
        entryHeaderUpdate.DomainIds = [4];
        entryHeaderUpdate.ReplaceSubjects = true;

        var mappedUpdateEntry = new Entry()
        {
            Id = entry.Id,
            Subjects = new List<Subject>() { new() { Id = 2 } },
            Domains = new List<Domain.Models.Domain>() { new() { Id = 4 } },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedUpdateEntry);

        var updateResult = await service.UpdateMandatoryHeaderFields(entryHeaderUpdate);

        Assert.That(mappedUpdateEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedUpdateEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.True);
        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(entryHeaderUpdate.Ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.SubjectIds.Equals(entryHeaderUpdate.SubjectIds) &&
            arg.DomainIds.Equals(entryHeaderUpdate.DomainIds)));
        entryRepositoryMock.Received(1).UpdateMandatoryHeaderFields(Arg.Any<Entry>(), Arg.Is<Entry>(x =>
            x.Subjects.Count() == 1 &&
            x.Domains.Count() == 1 &&
            x.Subjects.First().Id == 2 &&
            x.Domains.First().Id == 4));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_AddSubjectAndDomain_CallsDependencies()
    {
        entryHeaderUpdate.SubjectIds = [2];
        entryHeaderUpdate.DomainIds = [4];
        entryHeaderUpdate.ReplaceSubjects = false;

        var mappedUpdateEntry = new Entry()
        {
            Id = entry.Id,
            Subjects = new List<Subject>() { new() { Id = 2 } },
            Domains = new List<Domain.Models.Domain>() { new() { Id = 4 } },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedUpdateEntry);

        var updateResult = await service.UpdateMandatoryHeaderFields(entryHeaderUpdate);

        Assert.That(mappedUpdateEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedUpdateEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.True);
        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(entryHeaderUpdate.Ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.SubjectIds.Equals(entryHeaderUpdate.SubjectIds) &&
            arg.DomainIds.Equals(entryHeaderUpdate.DomainIds)));
        entryRepositoryMock.Received(1).UpdateMandatoryHeaderFields(Arg.Any<Entry>(), Arg.Is<Entry>(x =>
            x.Subjects.Count() == 2 &&
            x.Domains.Count() == 2 &&
            x.Subjects.First().Id == 1 &&
            x.Domains.First().Id == 11 &&
            x.Subjects.Last().Id == 2 &&
            x.Domains.Last().Id == 4));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_WithValidData_CallsDependencies()
    {
        entryHeaderUpdate.SubjectIds = [2];
        entryHeaderUpdate.DomainIds = [4];
        entryHeaderUpdate.ReplaceSubjects = true;

        var mappedEntry = new Entry()
        {
            Id = entry.Id,
            OfficeId = 2,
            CollectionId = 2,
            Reliability = Domain.Models.Reliability.Tentative,
            Status = Domain.Models.Status.InProgress,
            Subjects = new List<Subject>() { new() { Id = 2 } },
            Domains = new List<Domain.Models.Domain>() { new() { Id = 4 } },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateMandatoryHeaderFields(entryHeaderUpdate);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.True);
        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(entryHeaderUpdate.Ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.Id == entryHeaderUpdate.Ids[0] &&
            arg.OfficeId == entryHeaderUpdate.OfficeId &&
            arg.CollectionId == entryHeaderUpdate.CollectionId &&
            arg.Reliability == entryHeaderUpdate.Reliability &&
            arg.Status.Equals(entryHeaderUpdate.Status) &&
            arg.SubjectIds.Equals(entryHeaderUpdate.SubjectIds) &&
            arg.DomainIds.Equals(entryHeaderUpdate.DomainIds)));
        await entryValidationServiceMock.Received(1).ValidateCollection(Arg.Is<EntryUpdate>(u => u.CollectionId == entryHeaderUpdate.CollectionId));
        await entryValidationServiceMock.Received(1).ValidateOffice(Arg.Is<EntryUpdate>(u => u.OfficeId == entryHeaderUpdate.OfficeId));
        entryRepositoryMock.Received(1).UpdateMandatoryHeaderFields(Arg.Any<Entry>(), Arg.Is<Entry>(x =>
        x.ConcurrencyToken == entry.ConcurrencyToken &&
        x.Identification == entry.Identification &&
        x.AdminInfo == entry.AdminInfo &&
        x.Deleted == entry.Deleted &&
        x.Comment == entry.Comment &&
        x.ConceptSystemPosition.Id == entry.ConceptSystemPosition.Id &&
        x.ConceptSystemPositionId == entry.ConceptSystemPositionId &&
        x.Status == Domain.Models.Status.InProgress &&
        x.Reliability == Domain.Models.Reliability.Tentative &&
        x.CollectionId == 2 &&
        x.OfficeId == 2 &&
        x.Subjects.First().Id == 2 &&
        x.Domains.First().Id == 4));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_WithValidDataButWithoutChange_CallsDependenciesAndReturnsCorrectly()
    {
        entryHeaderUpdate.CollectionId = entry.CollectionId;
        entryHeaderUpdate.OfficeId = entry.OfficeId;
        entryHeaderUpdate.Reliability = ResourceModel.Reliability.Verified;
        entryHeaderUpdate.Status = ResourceModel.Status.Validated;
        entryHeaderUpdate.SubjectIds = [1];
        entryHeaderUpdate.DomainIds = [11];

        var mappedEntry = new Entry()
        {
            Id = entry.Id,
            OfficeId = entry.OfficeId,
            CollectionId = entry.CollectionId,
            Reliability = Domain.Models.Reliability.Verified,
            Status = Domain.Models.Status.Validated,
            Subjects = new List<Subject>() { new() { Id = 1 } },
            Domains = new List<Domain.Models.Domain>() { new() { Id = 11 } },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateMandatoryHeaderFields(entryHeaderUpdate);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.False);
        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(entryHeaderUpdate.Ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.Id == entryHeaderUpdate.Ids[0] &&
            arg.OfficeId == entryHeaderUpdate.OfficeId &&
            arg.CollectionId == entryHeaderUpdate.CollectionId &&
            arg.Reliability == entryHeaderUpdate.Reliability &&
            arg.Status.Equals(entryHeaderUpdate.Status) &&
            arg.SubjectIds.Equals(entryHeaderUpdate.SubjectIds) &&
            arg.DomainIds.Equals(entryHeaderUpdate.DomainIds)));
        await entryValidationServiceMock.Received(1).ValidateCollection(Arg.Is<EntryUpdate>(u => u.CollectionId == entryHeaderUpdate.CollectionId));
        await entryValidationServiceMock.Received(1).ValidateOffice(Arg.Is<EntryUpdate>(u => u.OfficeId == entryHeaderUpdate.OfficeId));
        entryRepositoryMock.Received(1).UpdateMandatoryHeaderFields(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x)));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_WithEmptyCollection_ShouldSetModified()
    {
        entryHeaderUpdate.CollectionId = null;

        var mappedEntry = new Entry()
        {
            Id = entry.Id,
            OfficeId = 2,
            CollectionId = entry.CollectionId,
            Reliability = Domain.Models.Reliability.Tentative,
            Status = Domain.Models.Status.InProgress,
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateMandatoryHeaderFields(entryHeaderUpdate);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.True);
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_WithoutStatus_ShouldSetModified()
    {
        entryHeaderUpdate.Status = null;

        var mappedEntry = new Entry()
        {
            Id = entry.Id,
            OfficeId = 2,
            CollectionId = 2,
            Reliability = Domain.Models.Reliability.Verified,
            Status = Domain.Models.Status.InProgress,
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateMandatoryHeaderFields(entryHeaderUpdate);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.True);
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_WithoutOffice_ShouldSetModified()
    {
        entryHeaderUpdate.OfficeId = null;

        var mappedEntry = new Entry()
        {
            Id = entry.Id,
            OfficeId = entry.OfficeId,
            CollectionId = 2,
            Reliability = Domain.Models.Reliability.Tentative,
            Status = Domain.Models.Status.Validated,
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateMandatoryHeaderFields(entryHeaderUpdate);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.True);
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_WithoutReliability_ShouldSetModified()
    {
        entryHeaderUpdate.Reliability = null;

        var mappedEntry = new Entry()
        {
            Id = entry.Id,
            OfficeId = entry.OfficeId,
            CollectionId = entry.CollectionId,
            Reliability = Domain.Models.Reliability.Verified,
            Status = Domain.Models.Status.InProgress,
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateMandatoryHeaderFields(entryHeaderUpdate);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.True);
    }

    [Test]
    public void UpdateMandatoryHeaderFields_AsNonAdminWithoutCollectionPermission_ShouldThrowForbiddenException()
    {
        authorizationService.CanEdit(Arg.Is<Entry>(y => y.Id == 1)).Returns(true);
        authorizationService.IsAdmin().Returns(false);
        entryRepositoryMock.GetByIdsForUpdate(Arg.Is(entryHeaderUpdate.Ids)).Returns([new() { Id = 1, Deleted = false }]);
        authorizationService.GetAuthorizedCollectionIds().Returns([4]);

        Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateMandatoryHeaderFields(entryHeaderUpdate));
    }

    [Test]
    public void UpdateChipFields_ForEntryWithoutPermission_ShouldThrowForbiddenException()
    {
        authorizationService.CanEdit(Arg.Is<Entry>(y => y.Id == 1)).Returns(false);
        var ids = new List<int> { 1 };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Is(entryHeaderUpdate.Ids)).Returns([new() { Id = 1, Deleted = false }]);

        var dto = new EntryChipFieldUpdate
        {
            Ids = ids
        };

        Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateGlossaries(dto));
    }

    [Test]
    public void UpdateChipFields_ForEntryWithDeletedFlag_ShouldThrowCanNotBeChangedException()
    {
        var ids = new List<int> { 1 };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Is(ids)).Returns([new() { Id = 1, Deleted = true }]);

        var dto = new EntryChipFieldUpdate
        {
            Ids = ids
        };

        Assert.ThrowsAsync<CanNotBeChangedException>(() => service.UpdateGlossaries(dto));
    }

    [Test]
    public void UpdateChipFields_WithValidData_ShouldSetModified()
    {
        var dto = new EntryChipFieldUpdate
        {
            Action = MultiEditAction.Add,
            Codes = new List<string> { "1", "2" },
            Ids = new List<int> { 1 }
        };

        var mappedEntry = new Entry()
        {
            Glossaries = new List<Glossary>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        service.UpdateGlossaries(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void UpdateGlossary_ForUnknownAction_ShouldThrowInvalidEnumArgumentException()
    {
        var dto = new EntryChipFieldUpdate
        {
            Ids = new List<int> { 1 },
            Codes = new List<string> { "2", "3" },
            Action = (MultiEditAction)5
        };

        Assert.ThrowsAsync<InvalidEnumArgumentException>(() => service.UpdateGlossaries(dto));
    }

    [Test]
    public void UpdateGlossary_AsNonAdminWithoutGlossaryPermission_ShouldThrowForbiddenException()
    {
        var dto = new EntryChipFieldUpdate
        {
            Ids = new List<int> { 1 },
            Codes = new List<string> { "2", "3" },
            Action = MultiEditAction.Add
        };

        authorizationService.IsAdmin().Returns(false);
        authorizationService.GetAuthorizedGlossaryIds().Returns([4]);

        Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateGlossaries(dto));
    }

    [Test]
    public void UpdateGlossary_ForInvalidCode_ShouldThrowInvalidEnumArgumentException()
    {
        var dto = new EntryChipFieldUpdate
        {
            Ids = new List<int> { 1 },
            Codes = new List<string> { "a" },
            Action = (MultiEditAction)5
        };

        Assert.ThrowsAsync<InvalidEnumArgumentException>(() => service.UpdateGlossaries(dto));
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public async Task UpdateGlossaries_WithAdd_ShouldCallServicesAndReturnDataWasChangedCorrectly(int affectedRows, bool expectedDataWasChanged)
    {
        var ids = new List<int> { 1 };

        var dto = new EntryChipFieldUpdate
        {
            Action = MultiEditAction.Add,
            Codes = new List<string> { "1", "2" },
            Ids = ids
        };
        var glossaryIds = new List<int> { 1, 2 };
        unitOfWorkMock.SaveChangesAsync().Returns(affectedRows);

        var mappedEntry = new Entry()
        {
            Glossaries = new List<Glossary>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateGlossaries(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.EqualTo(expectedDataWasChanged));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.GlossaryIds.SequenceEqual(glossaryIds)));
        await entryValidationServiceMock.Received(1).ValidateGlossaries(Arg.Is<EntryUpdate>(arg =>
            arg.GlossaryIds.SequenceEqual(glossaryIds)));
        entryRepositoryMock.Received(1).UpdateGlossaries(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x) &&
            x.Glossaries == mappedEntry.Glossaries));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public async Task UpdateGlossaries_WithSet_ShouldCallServicesAndReturnDataWasChangedCorrectly(int affectedRows, bool expectedDataWasChanged)
    {
        var ids = new List<int> { 1 };
        var dto = new EntryChipFieldUpdate
        {
            Action = MultiEditAction.Set,
            Codes = new List<string> { "2" },
            Ids = ids
        };
        var glossaryIds = new List<int> { 2 };

        unitOfWorkMock.SaveChangesAsync().Returns(affectedRows);

        var mappedEntry = new Entry()
        {
            Glossaries = new List<Glossary>
            {
                new() { Id = 2 }
            },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateGlossaries(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.EqualTo(expectedDataWasChanged));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.GlossaryIds.SequenceEqual(glossaryIds)));
        await entryValidationServiceMock.Received(1).ValidateGlossaries(Arg.Is<EntryUpdate>(arg =>
            arg.GlossaryIds.SequenceEqual(glossaryIds)));
        entryRepositoryMock.Received(1).UpdateGlossaries(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x) &&
            x.Glossaries == mappedEntry.Glossaries));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public async Task UpdateGlossaries_WithRemove_ShouldCallServicesAndReturnDataWasChangedCorrectly(int affectedRows, bool expectedDataWasChanged)
    {
        var ids = new List<int> { 1 };
        var dto = new EntryChipFieldUpdate
        {
            Action = MultiEditAction.Remove,
            Ids = ids
        };

        unitOfWorkMock.SaveChangesAsync().Returns(affectedRows);

        var mappedEntry = new Entry { Glossaries = new List<Glossary>() };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateGlossaries(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.EqualTo(expectedDataWasChanged));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.GlossaryIds.SequenceEqual(new List<int>())));
        await entryValidationServiceMock.Received(1).ValidateGlossaries(Arg.Is<EntryUpdate>(arg =>
            arg.GlossaryIds.SequenceEqual(new List<int>())));
        entryRepositoryMock.Received(1).UpdateGlossaries(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x) &&
            x.Glossaries == mappedEntry.Glossaries));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public void UpdateAdminInfos_ForEntryWithoutPermission_ShouldThrowForbiddenException()
    {
        authorizationService.CanEdit(Arg.Is<Entry>(y => y.Id == 1)).Returns(false);

        var dto = new EntryChipFieldInputUpdate
        {
            Ids = new List<int> { 1 },
            Codes = new List<AdminInfoSummary> {
                new() { Id = 1, Code = "Code1" },
                new() { Id = 2, Code = "Code2" },
            },
            Action = MultiEditAction.Add
        };

        Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateAdminInfos(dto));
    }

    [Test]
    public void UpdateAdminInfos_ForUnknownAction_ShouldThrowInvalidEnumArgumentException()
    {
        var dto = new EntryChipFieldInputUpdate
        {
            Ids = new List<int> { 1 },
            Codes = new List<AdminInfoSummary> {
                new() { Id = 1, Code = "Code1" },
                new() { Id = 2, Code = "Code2" },
            },
            Action = (MultiEditAction)5
        };

        Assert.ThrowsAsync<InvalidEnumArgumentException>(() => service.UpdateAdminInfos(dto));
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public async Task UpdateAdminInfos_WithAdd_ShouldCallServicesAndReturnDataWasChangedCorrectly(int affectedRows, bool expectedDataWasChanged)
    {
        var ids = new List<int> { 1 };
        var dto = new EntryChipFieldInputUpdate
        {
            Action = MultiEditAction.Add,
            Ids = ids,
            Codes = new List<AdminInfoSummary> {
                new() { Id = 1, Code = "Code1" },
                new() { Id = 2, Code = "Code2" },
            },
        };

        unitOfWorkMock.SaveChangesAsync().Returns(affectedRows);

        var mappedEntry = new Entry()
        {
            AdminInfos = new List<AdminInfo>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateAdminInfos(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.EqualTo(expectedDataWasChanged));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        entryRepositoryMock.Received(1).UpdateAdminInfos(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x) &&
            x.AdminInfos == mappedEntry.AdminInfos));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public async Task UpdateAdminInfos_WithSet_ShouldCallServicesAndReturnDataWasChangedCorrectly(int affectedRows, bool expectedDataWasChanged)
    {
        var ids = new List<int> { 1 };
        var dto = new EntryChipFieldInputUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            Codes = new List<AdminInfoSummary> {
                new() { Id = 1, Code = "Code1" },
                new() { Id = 2, Code = "Code2" },
            },
        };

        unitOfWorkMock.SaveChangesAsync().Returns(affectedRows);

        var mappedEntry = new Entry()
        {
            AdminInfos = new List<AdminInfo>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateAdminInfos(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.EqualTo(expectedDataWasChanged));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        entryRepositoryMock.Received(1).UpdateAdminInfos(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x) &&
            x.AdminInfos == mappedEntry.AdminInfos));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public async Task UpdateAdminInfos_WithRemove_ShouldCallServicesAndReturnDataWasChangedCorrectly(int affectedRows, bool expectedDataWasChanged)
    {
        var ids = new List<int> { 1 };
        var dto = new EntryChipFieldInputUpdate
        {
            Action = MultiEditAction.Remove,
            Ids = ids,
            Codes = new List<AdminInfoSummary> {
                new() { Id = 1, Code = "Code1" },
                new() { Id = 2, Code = "Code2" },
            },
        };

        unitOfWorkMock.SaveChangesAsync().Returns(affectedRows);

        var mappedEntry = new Entry()
        {
            AdminInfos = new List<AdminInfo>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateAdminInfos(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.EqualTo(expectedDataWasChanged));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg => arg.AdminInfos == null));
        entryRepositoryMock.Received(1).UpdateAdminInfos(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x) &&
            x.AdminInfos == mappedEntry.AdminInfos));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public void UpdateRelatedCollections_ForEntryWithoutPermission_ShouldThrowForbiddenException()
    {
        authorizationService.CanEdit(Arg.Is<Entry>(y => y.Id == 1)).Returns(false);

        var dto = new EntryChipFieldUpdate
        {
            Ids = new List<int> { 1 },
            Codes = new List<string> { "2", "3" },
            Action = MultiEditAction.Set
        };

        Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateRelatedCollections(dto));
    }

    [Test]
    public void UpdateRelatedCollections_ForUnknownAction_ShouldThrowInvalidEnumArgumentException()
    {
        var dto = new EntryChipFieldUpdate
        {
            Ids = new List<int> { 1 },
            Codes = new List<string> { "2", "3" },
            Action = (MultiEditAction)5
        };

        Assert.ThrowsAsync<InvalidEnumArgumentException>(() => service.UpdateRelatedCollections(dto));
    }

    [Test]
    public void UpdateRelatedCollections_AsNonAdminWithoutCollectionPermission_ShouldThrowForbiddenException()
    {
        var dto = new EntryChipFieldUpdate
        {
            Ids = new List<int> { 1 },
            Codes = new List<string> { "2", "3" },
            Action = MultiEditAction.Set
        };
        authorizationService.IsAdmin().Returns(false);
        authorizationService.GetAuthorizedCollectionIds().Returns([4]);

        Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateRelatedCollections(dto));
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public async Task UpdateRelatedCollections_WithAdd_ShouldCallServicesAndReturnDataWasChangedCorrectly(int affectedRows, bool expectedDataWasChanged)
    {
        var ids = new List<int> { 1 };
        var collectionIds = new List<int> { 1, 2, 3, 4, };
        var dto = new EntryChipFieldUpdate
        {
            Action = MultiEditAction.Add,
            Ids = ids,
            Codes = new List<string> { "3", "4" }
        };

        unitOfWorkMock.SaveChangesAsync().Returns(affectedRows);

        var mappedEntry = new Entry()
        {
            RelatedCollections = new List<Collection>
            {
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 },
                new() { Id = 4 }
            },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateRelatedCollections(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.EqualTo(expectedDataWasChanged));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.RelatedCollectionIds.SequenceEqual(collectionIds)));
        await entryValidationServiceMock.Received(1)
            .ValidateRelatedCollections(Arg.Is<EntryUpdate>(arg =>
                arg.RelatedCollectionIds.SequenceEqual(collectionIds)));
        entryRepositoryMock.Received(1).UpdateRelatedCollections(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x) &&
            x.RelatedCollections == mappedEntry.RelatedCollections));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public async Task UpdateRelatedCollections_WithSet_ShouldCallServicesAndReturnDataWasChangedCorrectly(int affectedRows, bool expectedDataWasChanged)
    {
        var ids = new List<int> { 1 };
        var collectionIds = new List<int> { 3, 4, };
        var dto = new EntryChipFieldUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            Codes = new List<string> { "3", "4" }
        };

        unitOfWorkMock.SaveChangesAsync().Returns(affectedRows);

        var mappedEntry = new Entry()
        {
            RelatedCollections = new List<Collection>
            {
                new() { Id = 3 },
                new() { Id = 4 }
            },
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateRelatedCollections(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.EqualTo(expectedDataWasChanged));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.RelatedCollectionIds.SequenceEqual(collectionIds)));
        await entryValidationServiceMock.Received(1)
            .ValidateRelatedCollections(Arg.Is<EntryUpdate>(arg =>
                arg.RelatedCollectionIds.SequenceEqual(collectionIds)));
        entryRepositoryMock.Received(1).UpdateRelatedCollections(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x) &&
            x.RelatedCollections == mappedEntry.RelatedCollections));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public async Task UpdateRelatedCollections_WithRemove_ShouldCallServicesAndReturnDataWasChangedCorrectly(int affectedRows, bool expectedDataWasChanged)
    {
        var ids = new List<int> { 1 };
        var dto = new EntryChipFieldUpdate
        {
            Action = MultiEditAction.Remove,
            Ids = ids,
        };

        unitOfWorkMock.SaveChangesAsync().Returns(affectedRows);

        var mappedEntry = new Entry { RelatedCollections = new List<Collection>() };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateRelatedCollections(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.EqualTo(expectedDataWasChanged));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.RelatedCollectionIds.SequenceEqual(new List<int>())));
        await entryValidationServiceMock.Received(1)
            .ValidateRelatedCollections(Arg.Is<EntryUpdate>(arg =>
                arg.RelatedCollectionIds.SequenceEqual(new List<int>())));
        entryRepositoryMock.Received(1).UpdateRelatedCollections(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x) &&
            x.RelatedCollections == mappedEntry.RelatedCollections));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public void UpdateComment_ForEntryWithoutPermission_ShouldThrowForbiddenException()
    {
        authorizationService.CanEdit(Arg.Is<Entry>(y => y.Id == 1)).Returns(false);
        var ids = new List<int> { 1 };
        var dto = new EntryCommentUpdate
        {
            Action = MultiEditAction.Add,
            Ids = ids,
            Comment = "Add comment"
        };

        Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateComment(dto));
    }

    [Test]
    public void UpdateComment_ForDeletedEntry_ShouldCallServicesCanNotBeChangedException()
    {
        var ids = new List<int> { 1 };
        var dto = new EntryCommentUpdate
        {
            Action = MultiEditAction.Add,
            Ids = ids,
            Comment = "Add comment"
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Is(ids)).Returns([new() { Id = 1, Deleted = true }]);

        Assert.ThrowsAsync<CanNotBeChangedException>(() => service.UpdateComment(dto));
    }

    [Test]
    public async Task UpdateComment_WithAdd_ShouldCallServices()
    {
        var ids = new List<int> { 1 };
        var dto = new EntryCommentUpdate
        {
            Action = MultiEditAction.Add,
            Ids = ids,
            Comment = "Add comment"
        };

        var mappedEntry = new Entry()
        {
            Comment = "comment" + Environment.NewLine + "Add comment"
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateComment(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.True);

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.Comment.Equals("comment" + Environment.NewLine + "Add comment")));
        entryRepositoryMock.Received(1).UpdateWithoutRelations(Arg.Any<Entry>(), Arg.Is<Entry>(x =>
        x.ConcurrencyToken == entry.ConcurrencyToken &&
        x.Status == entry.Status &&
        x.Reliability == entry.Reliability &&
        x.Identification == entry.Identification &&
        x.AdminInfo == entry.AdminInfo &&
        x.Deleted == entry.Deleted &&
        x.Office.Id == entry.Office.Id &&
        x.OfficeId == entry.OfficeId &&
        x.Collection.Id == entry.Collection.Id &&
        x.CollectionId == entry.CollectionId &&
        x.ConceptSystemPosition.Id == entry.ConceptSystemPosition.Id &&
        x.ConceptSystemPositionId == entry.ConceptSystemPositionId &&
        x.Comment == "comment" + Environment.NewLine + "Add comment"));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateComment_WithRemove_ShouldCallServices()
    {
        var ids = new List<int> { 1 };
        var dto = new EntryCommentUpdate
        {
            Action = MultiEditAction.Remove,
            Ids = ids,
            Comment = string.Empty
        };

        var mappedEntry = new Entry();

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateComment(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.True);

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            string.IsNullOrEmpty(arg.Comment)));
        entryRepositoryMock.Received(1).UpdateWithoutRelations(Arg.Any<Entry>(), Arg.Is<Entry>(x =>
        x.ConcurrencyToken == entry.ConcurrencyToken &&
        x.Status == entry.Status &&
        x.Reliability == entry.Reliability &&
        x.Identification == entry.Identification &&
        x.AdminInfo == entry.AdminInfo &&
        x.Deleted == entry.Deleted &&
        x.Office.Id == entry.Office.Id &&
        x.OfficeId == entry.OfficeId &&
        x.Collection.Id == entry.Collection.Id &&
        x.CollectionId == entry.CollectionId &&
        x.ConceptSystemPosition.Id == entry.ConceptSystemPosition.Id &&
        x.ConceptSystemPositionId == entry.ConceptSystemPositionId &&
        string.IsNullOrEmpty(x.Comment)));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateComment_WithSet_ShouldCallServices()
    {
        var ids = new List<int> { 1 };
        var dto = new EntryCommentUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            Comment = "Test comment"
        };

        var mappedEntry = new Entry()
        {
            Comment = "Test comment"
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateComment(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.True);

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
           arg.Comment.Equals("Test comment")));
        entryRepositoryMock.Received(1).UpdateWithoutRelations(Arg.Any<Entry>(), Arg.Is<Entry>(x =>
        x.ConcurrencyToken == entry.ConcurrencyToken &&
        x.Status == entry.Status &&
        x.Reliability == entry.Reliability &&
        x.Identification == entry.Identification &&
        x.AdminInfo == entry.AdminInfo &&
        x.Deleted == entry.Deleted &&
        x.Office.Id == entry.Office.Id &&
        x.OfficeId == entry.OfficeId &&
        x.Collection.Id == entry.Collection.Id &&
        x.CollectionId == entry.CollectionId &&
        x.ConceptSystemPosition.Id == entry.ConceptSystemPosition.Id &&
        x.ConceptSystemPositionId == entry.ConceptSystemPositionId &&
        x.Comment == "Test comment"));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateComment_RemoveWithoutChange_ShouldCallServicesAndReturnNoDataChange()
    {
        var ids = new List<int> { 1 };
        entry.Comment = string.Empty;
        var dto = new EntryCommentUpdate
        {
            Action = MultiEditAction.Remove,
            Ids = ids,
        };

        var mappedEntry = new Entry()
        {
            Comment = string.Empty
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateComment(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.False);

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            string.IsNullOrEmpty(arg.Comment)));
        entryRepositoryMock.Received(1).UpdateWithoutRelations(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x)));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateComment_SetWithoutChange_ShouldCallServicesAndReturnNoDataChange()
    {
        var ids = new List<int> { 1 };
        entry.Comment = "Set comment";
        var dto = new EntryCommentUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            Comment = "Set comment"
        };

        var mappedEntry = new Entry()
        {
            Comment = "Set comment"
        };

        mapperMock.Map<Entry>(Arg.Any<EntryUpdate>()).Returns(mappedEntry);

        var updateResult = await service.UpdateComment(dto);

        Assert.That(mappedEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(mappedEntry.Modified, Is.Not.EqualTo(default(DateTime)));
        Assert.That(updateResult.DataWasChanged, Is.False);

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        mapperMock.Received(1).Map<Entry>(Arg.Is<EntryUpdate>(arg =>
            arg.Comment.Equals("Set comment")));
        entryRepositoryMock.Received(1).UpdateWithoutRelations(Arg.Any<Entry>(), Arg.Is<Entry>(x => EntryHasOriginalValues(x)));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is(dto.Ids));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateConceptSystemPosition_ForEntryWithoutPermission_ShouldThrowForbiddenException()
    {
        authorizationService.CanEdit(Arg.Is<Entry>(y => y.Id == 1)).Returns(false);

        var ids = new List<int> { 1, 2 };
        var dto = new EntryConceptSystemPositionUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            ConceptSystemPositionId = 5
        };

        Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateConceptSystemPosition(dto));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateConceptSystemPosition_ForDeletedEntry_ShouldThrowCanNotBeChangedException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryConceptSystemPositionUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            ConceptSystemPositionId = 5
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Is(ids)).Returns(new List<Entry>
        {
            new() { Id = 1, Deleted = true },
            new() { Id = 2, Deleted = false }
        });
        Assert.ThrowsAsync<CanNotBeChangedException>(() => service.UpdateConceptSystemPosition(dto));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateConceptSystemPosition_WithAdd_ShouldThrowBusinessValidationException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryConceptSystemPositionUpdate
        {
            Action = MultiEditAction.Add,
            Ids = ids,
            ConceptSystemPositionId = 5
        };

        Assert.ThrowsAsync<BusinessValidationException>(() => service.UpdateConceptSystemPosition(dto));

        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [TestCase(5, true)]
    [TestCase(1, false)]
    public async Task UpdateConceptSystemPosition_WithSet_ShouldCallServices(int conceptSystemPositionId, bool expectedDataWasChanged)
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryConceptSystemPositionUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            ConceptSystemPositionId = conceptSystemPositionId
        };
        List<int> updatedIndexIds = null;

        await entryRepositoryMock.GetByIdsForIndex(Arg.Do<IEnumerable<int>>(x =>
        {
            updatedIndexIds = x.ToList();
        }));
        var updateResult = await service.UpdateConceptSystemPosition(dto);

        Assert.That(entry1.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(entry1.Modified, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-5)));
        Assert.That(entry1.ConceptSystemPositionId, Is.EqualTo(conceptSystemPositionId));
        Assert.That(entry2.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(entry2.Modified, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-5)));
        Assert.That(entry2.ConceptSystemPositionId, Is.EqualTo(conceptSystemPositionId));
        Assert.That(updateResult.DataWasChanged, Is.EqualTo(expectedDataWasChanged));
        Assert.That(updatedIndexIds, Is.EqualTo(ids));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.Received(1).SaveChangesAsync();

        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateConceptSystemPosition_WithRemove_ShouldCallServices()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryConceptSystemPositionUpdate
        {
            Action = MultiEditAction.Remove,
            Ids = ids,
            ConceptSystemPositionId = 5
        };

        List<int> updatedIndexIds = null;

        await entryRepositoryMock.GetByIdsForIndex(Arg.Do<IEnumerable<int>>(x =>
        {
            updatedIndexIds = x.ToList();
        }));

        var updateResult = await service.UpdateConceptSystemPosition(dto);

        Assert.That(entry1.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(entry1.Modified, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-5)));
        Assert.That(entry1.ConceptSystemPositionId, Is.Null);
        Assert.That(entry2.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(entry2.Modified, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-5)));
        Assert.That(entry2.ConceptSystemPositionId, Is.Null);
        Assert.That(updateResult.DataWasChanged, Is.True);
        Assert.That(updatedIndexIds, Is.EqualTo(ids));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.Received(1).SaveChangesAsync();

        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateConceptSystemPosition_WithLockInfos_ShouldCallServicesAndReturnLockInfos()
    {
        var dbEntry = CreateTestEntry(1);
        dbEntry.ConceptSystemPositionId = 1;
        var dbEntryLocked = CreateTestEntry(2);
        dbEntryLocked.ConceptSystemPositionId = 1;
        dbEntryLocked.Modified = new DateTime(2025, 2, 1);
        dbEntryLocked.Locked = DateTime.Now.AddHours(-1);

        entryRepositoryMock.GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(new List<int> { 1, 2 }))).Returns(new List<Entry> { dbEntry, dbEntryLocked });

        var ids = new List<int> { 1, 2 };
        var dto = new EntryConceptSystemPositionUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            ConceptSystemPositionId = 5
        };

        List<int> updatedIndexIds = null;

        await entryRepositoryMock.GetByIdsForIndex(Arg.Do<IEnumerable<int>>(x =>
        {
            updatedIndexIds = x.ToList();
        }));

        var updateResult = await service.UpdateConceptSystemPosition(dto);

        Assert.That(dbEntry.ModifiedBy, Is.EqualTo(TestEmail));
        Assert.That(dbEntry.Modified, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-5)));
        Assert.That(dbEntry.ConceptSystemPositionId, Is.EqualTo(5));
        Assert.That(dbEntryLocked.ModifiedBy, Is.Not.EqualTo(TestEmail));
        Assert.That(dbEntryLocked.Modified, Is.EqualTo(new DateTime(2025, 2, 1)));
        Assert.That(dbEntryLocked.ConceptSystemPositionId, Is.EqualTo(1));
        Assert.That(updatedIndexIds, Is.EqualTo([1]));

        Assert.That(updateResult.DataWasChanged, Is.True);
        Assert.That(updateResult.LockInfos.ToList(), Has.Count.EqualTo(1));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.Received(1).SaveChangesAsync();
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateSource_ForEntryWithoutPermission_ShouldThrowForbiddenException()
    {
        authorizationService.CanEdit(Arg.Is<Entry>(y => y.Id == 1)).Returns(false);

        var ids = new List<int> { 1, 2 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            UpdateValue = "newsource",
            TargetFields = [EntrySearchAndReplaceUpdate.TerminusSource, EntrySearchAndReplaceUpdate.DefinitionSource],
            LanguageIds = [1]
        };

        Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateSource(dto));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateSource_ForDeletedEntry_ShouldThrowCanNotBeChangedException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            UpdateValue = "newsource",
            TargetFields = [EntrySearchAndReplaceUpdate.TerminusSource, EntrySearchAndReplaceUpdate.DefinitionSource],
            LanguageIds = [1]
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Is(ids)).Returns(new List<Entry>
        {
            new() { Id = 1, Deleted = true },
            new() { Id = 2, Deleted = false }
        });
        Assert.ThrowsAsync<CanNotBeChangedException>(() => service.UpdateSource(dto));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateSource_WithRemove_ShouldThrowBusinessValidationException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Remove,
            Ids = ids,
            UpdateValue = "newsource",
            TargetFields = [EntrySearchAndReplaceUpdate.TerminusSource, EntrySearchAndReplaceUpdate.DefinitionSource],
            LanguageIds = [1]
        };

        Assert.ThrowsAsync<BusinessValidationException>(() => service.UpdateSource(dto));

        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateSource_WithEmptySource_ShouldThrowBusinessValidationException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            UpdateValue = " ",
            TargetFields = [EntrySearchAndReplaceUpdate.TerminusSource, EntrySearchAndReplaceUpdate.DefinitionSource],
            LanguageIds = [1]
        };

        Assert.ThrowsAsync<BusinessValidationException>(() => service.UpdateSource(dto));

        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [TestCase(EntrySearchAndReplaceUpdate.Abbreviation)]
    [TestCase(EntrySearchAndReplaceUpdate.Context)]
    [TestCase(EntrySearchAndReplaceUpdate.Definition)]
    [TestCase(EntrySearchAndReplaceUpdate.Name)]
    [TestCase(EntrySearchAndReplaceUpdate.Note)]
    [TestCase(EntrySearchAndReplaceUpdate.Phraseology)]
    [TestCase(EntrySearchAndReplaceUpdate.Terminus)]
    public async Task UpdateSource_WithInvalidSourceField_ShouldThrowBusinessValidationException(string sourceField)
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            UpdateValue = "newSource",
            TargetFields = [sourceField, EntrySearchAndReplaceUpdate.TerminusSource],
            LanguageIds = [1]
        };

        Assert.ThrowsAsync<BusinessValidationException>(() => service.UpdateSource(dto));

        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task UpdateSource_SetTerminusSource_ShouldUpdateSource()
    {
        var ids = new List<int> { 1, 2, 3 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            UpdateValue = "newTerminusSource",
            TargetFields = [EntrySearchAndReplaceUpdate.TerminusSource],
            LanguageIds = [1]
        };

        var resultDto = await TestSourceUpdate(dto, [1, 2, 3]);

        Assert.That(resultDto.NotFullyUpdatedEntryIds, Is.Empty);
        Assert.That(entriesForSourceUpdate.Where(e => ids.Contains(e.Id)).SelectMany(e => e.Translations).Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == dto.UpdateValue));
    }

    [Test]
    public async Task UpdateSource_SetTerminusSourceForRareLanguage_ShouldUpdateSource()
    {
        var ids = new List<int> { 1, 2, 3 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            UpdateValue = "newTerminusSource",
            TargetFields = [EntrySearchAndReplaceUpdate.TerminusSource],
            LanguageIds = [3]
        };

        var resultDto = await TestSourceUpdate(dto, [2, 3]);

        Assert.That(resultDto.NotFullyUpdatedEntryIds, Is.Empty);
        Assert.That(entriesForSourceUpdate.Where(e => ids.Contains(e.Id)).SelectMany(e => e.Translations).Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == dto.UpdateValue));
    }

    [Test]
    public async Task UpdateSource_SetAllSources_ShouldUpdateAllSources()
    {
        var ids = new List<int> { 1, 2, 3 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            UpdateValue = "newSource",
            TargetFields = [EntrySearchAndReplaceUpdate.DefinitionSource,
                EntrySearchAndReplaceUpdate.NoteSource,
                EntrySearchAndReplaceUpdate.ContextSource,
                EntrySearchAndReplaceUpdate.AbbreviationSource,
                EntrySearchAndReplaceUpdate.TerminusSource,
                EntrySearchAndReplaceUpdate.NameSource,
                EntrySearchAndReplaceUpdate.MetadataSource,
                EntrySearchAndReplaceUpdate.PhraseologySource],
            LanguageIds = [1, 2, 3]
        };

        var resultDto = await TestSourceUpdate(dto, [1, 2, 3]);
        var allAffectedTranslations = entriesForSourceUpdate.Take(3).SelectMany(e => e.Translations);

        Assert.That(resultDto.NotFullyUpdatedEntryIds, Is.Empty);
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == dto.UpdateValue));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == dto.UpdateValue));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.MetadataSource == dto.UpdateValue));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.NoteSource == dto.UpdateValue));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.NameSource == dto.UpdateValue));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.PhraseologySource == dto.UpdateValue));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.AbbreviationSource == dto.UpdateValue));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.ContextSource == dto.UpdateValue));
    }

    [Test]
    public async Task UpdateSource_AddForAllSourcesWhenDataIsNotEmpty_ShouldNotUpdateSource()
    {
        var ids = new List<int> { 1, 2, 3 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Add,
            Ids = ids,
            UpdateValue = "newTerminusSource",
            TargetFields = [EntrySearchAndReplaceUpdate.DefinitionSource,
                EntrySearchAndReplaceUpdate.NoteSource,
                EntrySearchAndReplaceUpdate.ContextSource,
                EntrySearchAndReplaceUpdate.AbbreviationSource,
                EntrySearchAndReplaceUpdate.TerminusSource,
                EntrySearchAndReplaceUpdate.NameSource,
                EntrySearchAndReplaceUpdate.MetadataSource,
                EntrySearchAndReplaceUpdate.PhraseologySource],
            LanguageIds = [2, 3]
        };

        var resultDto = await TestSourceUpdate(dto, []);
        var allAffectedTranslations = entriesForSourceUpdate.Take(3).SelectMany(e => e.Translations);

        Assert.That(resultDto.NotFullyUpdatedEntryIds, Is.EqualTo([1, 2, 3]));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == "DefinitionSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.NoteSource == "NoteSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.ContextSource == "ContextSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.AbbreviationSource == "AbbreviationSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == "TerminusSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.NameSource == "NameSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.MetadataSource == "MetadataSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.PhraseologySource == "PhraseologySource " + t.LanguageId));
    }

    [Test]
    public async Task UpdateSource_AddSource_ShouldUpdateEmptyValuesOnly()
    {
        var ids = new List<int> { 1, 2, 3 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Add,
            Ids = ids,
            UpdateValue = "newSource",
            TargetFields = [EntrySearchAndReplaceUpdate.MetadataSource,
                EntrySearchAndReplaceUpdate.PhraseologySource],
            LanguageIds = [1]
        };

        const int expectedChangedEntryId = 3;

        var resultDto = await TestSourceUpdate(dto, [expectedChangedEntryId]);
        var allAffectedTranslations = entriesForSourceUpdate.Where(e => e.Id == expectedChangedEntryId).SelectMany(e => e.Translations);
        var allNonAffectedTranslations = entriesForSourceUpdate.Where(e => ids.Contains(e.Id) && e.Id != expectedChangedEntryId).SelectMany(e => e.Translations);

        Assert.That(resultDto.NotFullyUpdatedEntryIds, Is.EqualTo([1, 2]));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.MetadataSource == "newSource"));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.PhraseologySource == "newSource"));
        Assert.That(allNonAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.MetadataSource == "MetadataSource " + t.LanguageId));
        Assert.That(allNonAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.PhraseologySource == "PhraseologySource " + t.LanguageId));
    }

    [Test]
    public async Task UpdateSource_AddSourceWhenThereAreOnlyNonEmptyDestinationValues_ShouldDoNothing()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Add,
            Ids = ids,
            UpdateValue = "newSource",
            TargetFields = [EntrySearchAndReplaceUpdate.MetadataSource,
                EntrySearchAndReplaceUpdate.PhraseologySource],
            LanguageIds = [1]
        };

        var resultDto = await TestSourceUpdate(dto, []);
        var allAffectedTranslations = entriesForSourceUpdate.Where(e => ids.Contains(e.Id)).SelectMany(e => e.Translations);

        Assert.That(resultDto.NotFullyUpdatedEntryIds, Is.EqualTo([1, 2]));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.MetadataSource == "MetadataSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.PhraseologySource == "PhraseologySource " + t.LanguageId));
    }

    [Test]
    public async Task UpdateSource_AddSourceWhenThereArSomeNonEmptyDestinationValues_ShouldIgnoreNonEmptyFields()
    {
        var ids = new List<int> { 3 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Add,
            Ids = ids,
            UpdateValue = "newSource",
            TargetFields = [EntrySearchAndReplaceUpdate.MetadataSource,
                EntrySearchAndReplaceUpdate.DefinitionSource],
            LanguageIds = [1]
        };

        var resultDto = await TestSourceUpdate(dto, [3]);
        var allAffectedTranslations = entriesForSourceUpdate.Where(e => ids.Contains(e.Id)).SelectMany(e => e.Translations);

        Assert.That(resultDto.NotFullyUpdatedEntryIds, Is.EqualTo([3]));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.MetadataSource == "newSource"));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == "DefinitionSource " + t.LanguageId));
    }

    [Test]
    public async Task UpdateSource_AddSourceWhenThereAreOnlyMissingFields_ShouldDoNothing()
    {
        var ids = new List<int> { 4 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Add,
            Ids = ids,
            UpdateValue = "newSource",
            TargetFields = [EntrySearchAndReplaceUpdate.AbbreviationSource],
            LanguageIds = [1, 2]
        };

        var resultDto = await TestSourceUpdate(dto, []);
        var allAffectedTranslations = entriesForSourceUpdate.Where(e => ids.Contains(e.Id)).SelectMany(e => e.Translations);

        Assert.That(resultDto.NotFullyUpdatedEntryIds, Is.EqualTo([4]));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.AbbreviationSource == string.Empty));
    }

    [Test]
    public async Task UpdateSource_SetSourceWhenSomeBaseFieldsAreMissing_ShouldUpdateEmptyValuesOnlyIfBaseFieldExists()
    {
        var ids = new List<int> { 4 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            UpdateValue = "newSource",
            TargetFields = [EntrySearchAndReplaceUpdate.TerminusSource,
                EntrySearchAndReplaceUpdate.AbbreviationSource],
            LanguageIds = [1]
        };

        const int expectedChangedEntryId = 4;

        var resultDto = await TestSourceUpdate(dto, [expectedChangedEntryId]);
        var allAffectedTranslations = entriesForSourceUpdate.Where(e => e.Id == expectedChangedEntryId).SelectMany(e => e.Translations);

        Assert.That(resultDto.DataWasChanged, Is.True);
        Assert.That(resultDto.NotFullyUpdatedEntryIds, Is.EqualTo([4]));
        Assert.That(allAffectedTranslations.Where(t => dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == "newSource"));
        Assert.That(allAffectedTranslations.Where(t => !dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == string.Empty));
        Assert.That(allAffectedTranslations.All(t => t.AbbreviationSource == string.Empty));
    }

    [TestCase(EntrySearchAndReplaceUpdate.AbbreviationSource)]
    [TestCase(EntrySearchAndReplaceUpdate.ContextSource)]
    [TestCase(EntrySearchAndReplaceUpdate.DefinitionSource)]
    [TestCase(EntrySearchAndReplaceUpdate.NameSource)]
    [TestCase(EntrySearchAndReplaceUpdate.NoteSource)]
    [TestCase(EntrySearchAndReplaceUpdate.PhraseologySource)]
    public async Task UpdateSource_SetSourceWhenSomeBaseFieldsAreMissing_ShouldIgnoreSourcesWithoutBaseField(string searchField)
    {
        var ids = new List<int> { 3, 4 };
        var dto = new EntrySourceUpdate
        {
            Action = MultiEditAction.Set,
            Ids = ids,
            UpdateValue = "newSource",
            TargetFields = [searchField],
            LanguageIds = [1, 2]
        };

        const int expectedChangedEntryId = 3;

        var resultDto = await TestSourceUpdate(dto, [expectedChangedEntryId]);
        var allAffectedTranslations = entriesForSourceUpdate.Where(e => e.Id == expectedChangedEntryId).SelectMany(e => e.Translations);

        Assert.That(resultDto.NotFullyUpdatedEntryIds, Is.EqualTo([4]));
        Assert.That(resultDto.DataWasChanged, Is.True);
    }

    [Test]
    public async Task MoveContent_ForEntryWithoutPermission_ShouldThrowForbiddenException()
    {
        authorizationService.CanEdit(Arg.Is<Entry>(y => y.Id == 1)).Returns(false);

        var ids = new List<int> { 1, 2 };
        var dto = new EntryContentMove
        {
            LanguageIds = [1],
            Ids = ids,
            SourceField = "note",
            DestinationField = "terminus",
            ForAllSynonyms = true
        };

        Assert.ThrowsAsync<ForbiddenException>(() => service.MoveContent(dto));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task MoveContent_ForDeletedEntry_ShouldThrowCanNotBeChangedException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryContentMove
        {
            LanguageIds = [1],
            Ids = ids,
            SourceField = "note",
            DestinationField = "terminus",
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Is(ids)).Returns(new List<Entry>
        {
            new() { Id = 1, Deleted = true },
            new() { Id = 2, Deleted = false }
        });
        Assert.ThrowsAsync<CanNotBeChangedException>(() => service.MoveContent(dto));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task MoveContent_WithEmptySourceField_ShouldThrowBusinessValidationException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryContentMove
        {
            LanguageIds = [1],
            Ids = ids,
            SourceField = "",
            DestinationField = "terminus",
            ForAllSynonyms = true
        };

        Assert.ThrowsAsync<BusinessValidationException>(() => service.MoveContent(dto));

        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task MoveContent_WithEmptyLanguageId_ShouldThrowBusinessValidationException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryContentMove
        {
            LanguageIds = [1],
            Ids = ids,
            SourceField = "",
            DestinationField = "terminus",
            ForAllSynonyms = true
        };

        Assert.ThrowsAsync<BusinessValidationException>(() => service.MoveContent(dto));

        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task MoveContent_WithEmptyDestinationField_ShouldThrowBusinessValidationException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryContentMove
        {
            LanguageIds = [1],
            Ids = ids,
            SourceField = "terminus",
            DestinationField = "",
            ForAllSynonyms = true
        };

        Assert.ThrowsAsync<BusinessValidationException>(() => service.MoveContent(dto));

        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task MoveContent_WithInvalidSourceField_ShouldThrowBusinessValidationException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryContentMove
        {
            LanguageIds = [1],
            Ids = ids,
            SourceField = "terminuss",
            DestinationField = "note",
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns(entriesForContentMove);
        Assert.ThrowsAsync<BusinessValidationException>(() => service.MoveContent(dto));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task MoveContent_WithInvalidDestinationField_ShouldThrowBusinessValidationException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryContentMove
        {
            LanguageIds = [1],
            Ids = ids,
            SourceField = "terminus",
            DestinationField = "notes",
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns(entriesForContentMove);
        Assert.ThrowsAsync<BusinessValidationException>(() => service.MoveContent(dto));

        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task MoveContent_WithSameSourceAndDestinationFields_ShouldThrowBusinessValidationException()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryContentMove
        {
            LanguageIds = [1],
            Ids = ids,
            SourceField = "terminus",
            DestinationField = "terminus",
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns(entriesForContentMove);
        Assert.ThrowsAsync<BusinessValidationException>(() => service.MoveContent(dto));

        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForUpdate(Arg.Is<List<int>>(arg => arg.SequenceEqual(ids)));
        await unitOfWorkMock.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        await entryRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdsForIndex(Arg.Any<IEnumerable<int>>());
        await searchQueueClientMock.DidNotReceiveWithAnyArgs().QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.DidNotReceiveWithAnyArgs().UpdateLastAction(TestEmail);
    }

    [Test]
    public async Task MoveContent_MoveAllValues_ShouldUpdateAllValues()
    {
        var ids = new List<int> { 1, 2, 3 };
        var dto = new EntryContentMove
        {
            Ids = ids,
            SourceField = EntrySearchAndReplaceUpdate.Context,
            DestinationField = EntrySearchAndReplaceUpdate.Phraseology,
            LanguageIds = [1, 2, 3],
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns(entriesForContentMove.Take(3));
        var result = await service.MoveContent(dto);
        var allAffectedTranslations = entriesForContentMove.Take(3).SelectMany(e => e.Translations);

        Assert.That(result.DataWasChanged, Is.True);
        Assert.That(result.NonAffectedEntryIds, Is.Empty);
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.ContextSource == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.PhraseologySource == "ContextSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Context == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Phraseology == "Context " + t.LanguageId));
    }

    [Test]
    public async Task MoveContent_MoveAllWithLockedEntries_ShouldUpdateAllNonLockedEntries()
    {
        var dbEntryLocked = CreateTestEntry(44);
        dbEntryLocked.Modified = new DateTime(2025, 2, 1);
        dbEntryLocked.Locked = DateTime.Now.AddHours(-1);

        var ids = new List<int> { 1, 2, 3 };
        var dto = new EntryContentMove
        {
            Ids = ids,
            SourceField = EntrySearchAndReplaceUpdate.Terminus,
            DestinationField = EntrySearchAndReplaceUpdate.Definition,
            LanguageIds = [1, 2, 3],
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns(entriesForContentMove.Take(3).Concat([dbEntryLocked]));
        var result = await service.MoveContent(dto);
        var allAffectedTranslations = entriesForContentMove.Take(2).SelectMany(e => e.Translations);

        Assert.That(result.DataWasChanged, Is.True);
        Assert.That(result.NonAffectedEntryIds, Is.EqualTo([3]));
        Assert.That(result.LockInfos.ToList(), Has.Count.EqualTo(1));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == "TerminusSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Terminus == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Definition == "Terminus " + t.LanguageId));
    }

    [Test]
    public async Task MoveContent_MoveAllButOnlyForOneLanguage_ShouldNotUpdateOtherLanguages()
    {
        var ids = new List<int> { 1, 2, 3 };
        var dto = new EntryContentMove
        {
            Ids = ids,
            SourceField = EntrySearchAndReplaceUpdate.Terminus,
            DestinationField = EntrySearchAndReplaceUpdate.Definition,
            LanguageIds = [1],
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns(entriesForContentMove.Take(3));
        var result = await service.MoveContent(dto);
        var allAffectedTranslations = entriesForContentMove.Take(2).SelectMany(e => e.Translations);

        Assert.That(result.DataWasChanged, Is.True);
        Assert.That(result.NonAffectedEntryIds, Is.EqualTo([3]));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == "TerminusSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Terminus == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Definition == "Terminus " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && !dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == "TerminusSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && !dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && !dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Terminus == "Terminus " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && !dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Definition == string.Empty));
    }

    [Test]
    public async Task MoveContent_NotAllDestinationFieldsAreEmpty_ShouldNotUpdateEntries()
    {
        var ids = new List<int> { 1 };
        var dto = new EntryContentMove
        {
            Ids = ids,
            SourceField = EntrySearchAndReplaceUpdate.Abbreviation,
            DestinationField = EntrySearchAndReplaceUpdate.Note,
            LanguageIds = [1],
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns(entriesForContentMove.Take(1));
        var result = await service.MoveContent(dto);
        var allAffectedTranslations = entriesForContentMove.Take(1).SelectMany(e => e.Translations);

        Assert.That(result.DataWasChanged, Is.False);
        Assert.That(result.NonAffectedEntryIds, Is.EqualTo([1]));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Abbreviation == "Abbreviation " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.AbbreviationSource == "AbbreviationSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.NoteSource == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Note == "Note " + t.LanguageId));
    }

    [Test]
    public async Task MoveContent_NotAllBaseFieldsAreEmpty_ShouldNotUpdateEntry()
    {
        var ids = new List<int> { 1 };
        var dto = new EntryContentMove
        {
            Ids = ids,
            SourceField = EntrySearchAndReplaceUpdate.Abbreviation,
            DestinationField = EntrySearchAndReplaceUpdate.Note,
            LanguageIds = [1],
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns(entriesForContentMove.Take(1));
        var result = await service.MoveContent(dto);
        var allAffectedTranslations = entriesForContentMove.Take(1).SelectMany(e => e.Translations);

        Assert.That(result.DataWasChanged, Is.False);
        Assert.That(result.NonAffectedEntryIds, Is.EqualTo([1]));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Abbreviation == "Abbreviation " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.AbbreviationSource == "AbbreviationSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.NoteSource == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Note == "Note " + t.LanguageId));
    }

    [Test]
    public async Task MoveContent_EmptySourceValueExists_ShouldNotUpdateEntries()
    {
        var ids = new List<int> { 1 };
        var dto = new EntryContentMove
        {
            Ids = ids,
            SourceField = EntrySearchAndReplaceUpdate.Name,
            DestinationField = EntrySearchAndReplaceUpdate.Phraseology,
            LanguageIds = [1],
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns(entriesForContentMove.Take(1));
        var result = await service.MoveContent(dto);
        var allAffectedTranslations = entriesForContentMove.Take(1).SelectMany(e => e.Translations);

        Assert.That(result.DataWasChanged, Is.False);
        Assert.That(result.NonAffectedEntryIds, Is.EqualTo([1]));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Phraseology == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.PhraseologySource == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence != 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Name == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence == 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Name == "Name " + t.LanguageId));
    }

    [Test]
    public async Task MoveContent_MoveOnlyFirstSynonym_ShouldMoveFirstEntriesOnly()
    {
        var ids = new List<int> { 1, 2 };
        var dto = new EntryContentMove
        {
            Ids = ids,
            SourceField = EntrySearchAndReplaceUpdate.Terminus,
            DestinationField = EntrySearchAndReplaceUpdate.Definition,
            LanguageIds = [1, 2, 3],
            ForAllSynonyms = false
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns(entriesForContentMove.Take(2));
        var result = await service.MoveContent(dto);
        var allAffectedTranslations = entriesForContentMove.Take(2).SelectMany(e => e.Translations);

        Assert.That(result.DataWasChanged, Is.True);
        Assert.That(result.NonAffectedEntryIds, Is.Empty);
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence == 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence == 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == "TerminusSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence == 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Terminus == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence == 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Definition == "Terminus " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence != 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == "TerminusSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence != 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == "DefinitionSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence != 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Terminus == "Terminus " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence != 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Definition == string.Empty));
    }

    [Test]
    public async Task MoveContent_MoveOnlyFirstSynonymWithOtherNonEmptySynonyms_ShouldMoveFirstEntryOnly()
    {
        var ids = new List<int> { 3 };
        var dto = new EntryContentMove
        {
            Ids = ids,
            SourceField = EntrySearchAndReplaceUpdate.Terminus,
            DestinationField = EntrySearchAndReplaceUpdate.Definition,
            LanguageIds = [1],
            ForAllSynonyms = false
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns([entriesForContentMove[2]]);
        var result = await service.MoveContent(dto);
        var allAffectedTranslations = entriesForContentMove[2].Translations;

        Assert.That(result.DataWasChanged, Is.True);
        Assert.That(result.NonAffectedEntryIds, Is.Empty);
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence == 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence == 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == "TerminusSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence == 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Terminus == string.Empty));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence == 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Definition == "Terminus " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence != 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == "TerminusSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence != 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == "DefinitionSource " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence != 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Terminus == "Terminus " + t.LanguageId));
        Assert.That(allAffectedTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence != 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Definition == "Definition " + t.LanguageId));
    }

    [Test]
    public async Task MoveContent_MoveFieldWhenNotSupportedForMandatoryFields_ShouldNotChangeData()
    {
        var ids = new List<int> { 4 };
        var dto = new EntryContentMove
        {
            Ids = ids,
            SourceField = EntrySearchAndReplaceUpdate.Terminus,
            DestinationField = EntrySearchAndReplaceUpdate.Definition,
            LanguageIds = [1],
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns([entriesForContentMove[3]]);
        var result = await service.MoveContent(dto);
        var allTranslations = entriesForContentMove[3].Translations;

        Assert.That(result.DataWasChanged, Is.False);
        Assert.That(result.NonAffectedEntryIds, Is.EqualTo([4]));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Terminus == "Terminus " + t.LanguageId));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == "TerminusSource " + t.LanguageId));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == string.Empty));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Definition == string.Empty));
    }

    [Test]
    public async Task MoveContent_MoveFieldWhenSupportedForMandatoryFields_ShouldChangeData()
    {
        var ids = new List<int> { 4 };
        var dto = new EntryContentMove
        {
            Ids = ids,
            SourceField = EntrySearchAndReplaceUpdate.Terminus,
            DestinationField = EntrySearchAndReplaceUpdate.Name,
            LanguageIds = [2],
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns([entriesForContentMove[3]]);
        var result = await service.MoveContent(dto);
        var allTranslations = entriesForContentMove[3].Translations;

        Assert.That(result.DataWasChanged, Is.True);
        Assert.That(result.NonAffectedEntryIds, Is.Empty);
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == string.Empty));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.NameSource == "TerminusSource " + t.LanguageId));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Name == "Terminus " + t.LanguageId));
    }

    [Test]
    public async Task MoveContent_MoveAllButOneFieldIsNotEmpty_ShouldNotChangeData()
    {
        var ids = new List<int> { 3 };
        var dto = new EntryContentMove
        {
            Ids = ids,
            SourceField = EntrySearchAndReplaceUpdate.Terminus,
            DestinationField = EntrySearchAndReplaceUpdate.Definition,
            LanguageIds = [1],
            ForAllSynonyms = true
        };
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns([entriesForContentMove[2]]);
        var result = await service.MoveContent(dto);
        var allTranslations = entriesForContentMove[2].Translations;

        Assert.That(result.DataWasChanged, Is.False);
        Assert.That(result.NonAffectedEntryIds, Is.EqualTo([3]));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Terminus == "Terminus " + t.LanguageId));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.TerminusSource == "TerminusSource " + t.LanguageId));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence == 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == string.Empty));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence == 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Definition == string.Empty));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence != 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.DefinitionSource == "DefinitionSource " + t.LanguageId));
        Assert.That(allTranslations.Where(t => ids.Contains(t.EntryId) && t.Sequence != 1 && dto.LanguageIds.Contains(t.LanguageId)).All(t => t.Definition == "Definition " + t.LanguageId));
    }

    private async Task<EntrySourceUpdateResult> TestSourceUpdate(EntrySourceUpdate dto, IEnumerable<int> expectedIdsForIndexUpdate)
    {
        entryRepositoryMock.GetByIdsForUpdate(Arg.Any<IEnumerable<int>>()).Returns(entriesForSourceUpdate.Where(y => dto.Ids.Contains(y.Id)));
        var resultDto = await service.UpdateSource(dto);

        Assert.That(resultDto.DataWasChanged, Is.EqualTo(expectedIdsForIndexUpdate.Any()));
        Assert.That(entriesForSourceUpdate.Where(e => expectedIdsForIndexUpdate.Contains(e.Id)).All(e => e.ModifiedBy == TestEmail));
        Assert.That(entriesForSourceUpdate.Where(e => !expectedIdsForIndexUpdate.Contains(e.Id)).All(e => e.ModifiedBy != TestEmail));
        await entryRepositoryMock.Received(1).GetByIdsForUpdate(Arg.Is<IEnumerable<int>>(arg => arg.SequenceEqual(dto.Ids)));
        await entryRepositoryMock.Received(1).GetByIdsForIndex(Arg.Is<IEnumerable<int>>(arg => arg.SequenceEqual(expectedIdsForIndexUpdate)));
        await searchQueueClientMock.Received(1).QueueIndexUpdate(Arg.Any<IEnumerable<IndexEntry>>());
        await userAccountServiceMock.Received(1).UpdateLastAction(TestEmail);
        identityServiceMock.Received(1).GetCurrentEmail();
        await unitOfWorkMock.Received().SaveChangesAsync();

        return resultDto;
    }

    private bool EntryHasOriginalValues(Entry x)
    {
        return
        x.ConcurrencyToken == entry.ConcurrencyToken &&
        x.Identification == entry.Identification &&
        x.AdminInfo == entry.AdminInfo &&
        x.Deleted == entry.Deleted &&
        x.Comment == entry.Comment &&
        x.ConceptSystemPosition.Id == entry.ConceptSystemPosition.Id &&
        x.ConceptSystemPositionId == entry.ConceptSystemPositionId &&
        x.Status == entry.Status &&
        x.Reliability == entry.Reliability &&
        x.CollectionId == entry.CollectionId &&
        x.OfficeId == entry.OfficeId;
    }

    private static Entry CreateTestEntry(int id = 1)
    {
        return new Entry
        {
            Id = id,
            OfficeId = 1,
            CollectionId = 1,
            ConceptSystemPositionId = 1,
            ModifiedBy = "evil@kanevil.com",
            Translations = new List<EntryLanguage>
            {
                new() { Id = 1, Sequence = 1, LanguageId = 1, Language = new Language { SortIndex = 1 } },
                new() { Id = 1, Sequence = 1, LanguageId = 2, Language = new Language { SortIndex = 1 } },
                new() { Id = 1, Sequence = 1, LanguageId = 3, Language = new Language { SortIndex = 1 } },
                new() { Id = 1, Sequence = 1, LanguageId = 4, Language = new Language { SortIndex = 1 } },
            },
            Collection = new Collection
            {
                Translations = new List<CollectionTranslation>
                {
                    new() { LanguageId = 1 },
                    new() { LanguageId = 2 },
                    new() { LanguageId = 3 },
                    new() { LanguageId = 4 }
                }
            },
            Office = new Office(),
            ConceptSystemPosition = new ConceptSystemPosition(),
            RelatedCollections = new List<Collection>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            },
            LenochClassifications = new List<LenochClassification>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            },
            Glossaries = new List<Glossary>
            {
                new() { Id = 1 }
            },
            AdminInfo = "info",
            AdminInfos = new List<AdminInfo>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            },
            Comment = "comment",
            ConcurrencyToken = 123,
            Identification = 1,
            Reliability = Domain.Models.Reliability.Verified,
            Status = Domain.Models.Status.Validated,
            Picture = new Picture(),
            Subjects = new List<Subject>
            {
                new() { Id = 1 }
            },
            Domains = new List<Domain.Models.Domain>
            {
                new() { Id = 11 }
            },
        };
    }
}
