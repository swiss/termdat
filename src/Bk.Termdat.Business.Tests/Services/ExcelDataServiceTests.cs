using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.CrossCutting.Exceptions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Swiss.FCh.DocumentService.Client;
using Swiss.FCh.DocumentService.Client.Models;
using Swiss.FCh.Utils.Models;
using Swiss.FCh.Utils.Services;
using ExternalLinkType = Bk.Termdat.Business.Domain.Models.ExternalLinkType;
using ReferenceType = Bk.Termdat.Business.Domain.Models.ReferenceType;
using Reliability = Bk.Termdat.Business.Resource.Models.Reliability;
using Status = Bk.Termdat.Business.Resource.Models.Status;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class ExcelDataServiceTests
{
    [SetUp]
    public void SetUp()
    {
        fileImportOptionsMock.Value.Returns(new FileImportOptions { Email = "test-import-user@test.com" });
        emailOptionsMock.Value.Returns(new EmailOptions { SenderAddress = "test-sender@test.com" });
    }

    private readonly IOptions<FileImportOptions> fileImportOptionsMock = Substitute.For<IOptions<FileImportOptions>>();
    private readonly IOptions<EmailOptions> emailOptionsMock = Substitute.For<IOptions<EmailOptions>>();

    [Test]
    public async Task IsFileValid_WithOnlyHeaderRow_ShouldReturnValidationError()
    {
        var sut = CreateService();
        using var workbook = CreateWorkbookWithHeaderOnly();
        var formFileMock = CreateFormFile(workbook);

        var (isValid, validationErrors, _) = await sut.IsFileValid(formFileMock);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationErrors, Has.Count.EqualTo(1));
            Assert.That(validationErrors.First().Value, Does.Contain("at least a header row and one data row"));
        }
    }

    [Test]
    public async Task IsFileValid_WithInsufficientColumns_ShouldReturnValidationError()
    {
        var sut = CreateService();
        using var workbook = CreateWorkbookWithInsufficientColumns();
        var formFileMock = CreateFormFile(workbook);
        var (isValid, validationErrors, _) = await sut.IsFileValid(formFileMock);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationErrors, Has.Count.EqualTo(1));
            Assert.That(validationErrors.First().Value, Does.Contain("at least 21 columns"));
        }
    }

    [Test]
    public async Task IsFileValid_WithValidWorkbook_ShouldReturnSuccess()
    {
        var sut = CreateService();
        using var workbook = CreateValidWorkbookWithTestData();
        var formFileMock = CreateFormFile(workbook);

        var (isValid, validationErrors, resultWorkbook) = await sut.IsFileValid(formFileMock);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(validationErrors, Is.Empty);
            Assert.That(resultWorkbook, Is.Not.Null);
        }
    }

    [Test]
    public async Task ComposeEntryCreateItems_WithValidData_ShouldMapToEntryCreate()
    {
        var sut = CreateTestableService();
        using var workbook = CreateValidWorkbookWithTestData();

        var actual = await sut.ComposeEntryCreateItemsAccessor(
            workbook, 1, 2,
            Status.InProgress, Reliability.Tentative,
            new List<int> { 3, 4 }, new List<int> { 5, 6 }, new List<int> { 7, 8 },
            new List<AdminInfoSummary> { new AdminInfoDetail { Code = "TestImport" } }, "Test comment");

        Assert.That(actual, Has.Count.EqualTo(2));
        var firstEntry = actual[0];
        var secondEntry = actual[1];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(firstEntry.OfficeId, Is.EqualTo(1));
            Assert.That(firstEntry.CollectionId, Is.EqualTo(2));
            Assert.That(firstEntry.Status, Is.EqualTo(Status.InProgress));
            Assert.That(firstEntry.Reliability, Is.EqualTo(Reliability.Tentative));
            Assert.That(firstEntry.SubjectIds, Is.EquivalentTo(new List<int> { 3, 4 }));
            Assert.That(firstEntry.DomainIds, Is.EquivalentTo(new List<int> { 5, 6 }));
            Assert.That(firstEntry.GlossaryIds, Is.EquivalentTo(new List<int> { 7, 8 }));
            Assert.That(firstEntry.Comment, Is.EqualTo("Test comment"));
            Assert.That(firstEntry.AdminInfos.First().Code, Is.EqualTo("TestImport"));
            Assert.That(firstEntry.LanguageDetails, Has.Count.EqualTo(2));
            Assert.That(firstEntry.LanguageDetails[0].LanguageId, Is.EqualTo(1));
            Assert.That(firstEntry.LanguageDetails[0].Sequence, Is.EqualTo(1));
            Assert.That(firstEntry.LanguageDetails[1].LanguageId, Is.EqualTo(2));
            Assert.That(firstEntry.LanguageDetails[1].Sequence, Is.EqualTo(2));
            Assert.That(secondEntry.OfficeId, Is.EqualTo(1));
            Assert.That(secondEntry.CollectionId, Is.EqualTo(2));
            Assert.That(secondEntry.Status, Is.EqualTo(Status.InProgress));
            Assert.That(secondEntry.Reliability, Is.EqualTo(Reliability.Tentative));
            Assert.That(secondEntry.SubjectIds, Is.EquivalentTo(new List<int> { 3, 4 }));
            Assert.That(secondEntry.DomainIds, Is.EquivalentTo(new List<int> { 5, 6 }));
            Assert.That(secondEntry.GlossaryIds, Is.EquivalentTo(new List<int> { 7, 8 }));
            Assert.That(secondEntry.Comment, Is.EqualTo("Test comment"));
            Assert.That(secondEntry.AdminInfos.First().Code, Is.EqualTo("TestImport"));
            Assert.That(secondEntry.LanguageDetails, Has.Count.EqualTo(1));
            Assert.That(secondEntry.LanguageDetails[0].LanguageId, Is.EqualTo(3));
            Assert.That(secondEntry.LanguageDetails[0].Sequence, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task ComposeEntryCreateItems_WithMultipleDefinitions_ShouldSetDefinitionOnlyOnFirstSynonym()
    {
        var sut = CreateTestableService();
        using var workbook = CreateWorkbookWithMultipleDefinitions();

        var actual = await sut.ComposeEntryCreateItemsAccessor(
            workbook, 1, 2,
            Status.InProgress, Reliability.Tentative,
            new List<int>(), new List<int>(), new List<int>(),
            new List<AdminInfoSummary>(), string.Empty);

        Assert.That(actual, Has.Count.EqualTo(1));
        Assert.That(actual[0].LanguageDetails, Has.Count.EqualTo(2));
        var firstDetail = actual[0].LanguageDetails[0];
        var secondDetail = actual[0].LanguageDetails[1];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(firstDetail.Definition, Is.EqualTo("Test Definition"));
            Assert.That(firstDetail.DefinitionSource, Is.EqualTo("Test Definition Source"));
            Assert.That(secondDetail.Definition, Is.Null);
            Assert.That(secondDetail.DefinitionSource, Is.Null);
        }
    }

    [Test]
    public void ComposeEntryCreateItems_WithInvalidLanguage_ShouldThrowBusinessValidationException()
    {
        var sut = CreateTestableService();
        using var workbook = CreateWorkbookWithInvalidLanguage();

        var ex = Assert.ThrowsAsync<BusinessValidationException>(async () =>
            await sut.ComposeEntryCreateItemsAccessor(
                workbook, 1, 2,
                Status.InProgress, Reliability.Tentative,
                new List<int>(), new List<int>(), new List<int>(),
                new List<AdminInfoSummary>(), string.Empty));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ex?.ModelKey, Is.EqualTo("Language"));
            Assert.That(ex.Message, Does.Contain("is missing or not supported"));
        }
    }

    [Test]
    public void ComposeEntryCreateItems_WithMissingSequence_ShouldThrowBusinessValidationException()
    {
        var sut = CreateTestableService();
        using var workbook = CreateWorkbookWithMissingSequence();

        var ex = Assert.ThrowsAsync<BusinessValidationException>(async () =>
            await sut.ComposeEntryCreateItemsAccessor(
                workbook, 1, 2,
                Status.InProgress, Reliability.Tentative,
                new List<int>(), new List<int>(), new List<int>(),
                new List<AdminInfoSummary>(), string.Empty));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ex?.ModelKey, Is.EqualTo("Sequence"));
            Assert.That(ex.Message, Does.Contain("must be greater than 0"));
        }
    }

    [Test]
    public async Task ComposeEntryCreateItems_WithAllFields_ShouldMapAllProperties()
    {
        var sut = CreateTestableService();
        using var workbook = CreateWorkbookWithAllFields();

        var actual = await sut.ComposeEntryCreateItemsAccessor(
            workbook, 1, 2,
            Status.InProgress, Reliability.Tentative,
            new List<int>(), new List<int>(), new List<int>(),
            new List<AdminInfoSummary>(), string.Empty);

        Assert.That(actual, Has.Count.EqualTo(1));
        Assert.That(actual[0].LanguageDetails, Has.Count.EqualTo(1));
        var detail = actual[0].LanguageDetails[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(detail.Sequence, Is.EqualTo(1));
            Assert.That(detail.LanguageId, Is.EqualTo(1));
            Assert.That(detail.Name, Is.EqualTo("Test Name"));
            Assert.That(detail.NameSource, Is.EqualTo("Test Name Source"));
            Assert.That(detail.Terminus, Is.EqualTo("Test Terminus"));
            Assert.That(detail.TerminusSource, Is.EqualTo("Test Terminus Source"));
            Assert.That(detail.Phraseology, Is.EqualTo("Test Phraseology"));
            Assert.That(detail.PhraseologySource, Is.EqualTo("Test Phraseology Source"));
            Assert.That(detail.Abbreviation, Is.EqualTo("Test Abbreviation"));
            Assert.That(detail.AbbreviationSource, Is.EqualTo("Test Abbreviation Source"));
            Assert.That(detail.Definition, Is.EqualTo("Test Definition"));
            Assert.That(detail.DefinitionSource, Is.EqualTo("Test Definition Source"));
            Assert.That(detail.Note, Is.EqualTo("Test Note"));
            Assert.That(detail.NoteSource, Is.EqualTo("Test Note Source"));
            Assert.That(detail.Context, Is.EqualTo("Test Context"));
            Assert.That(detail.ContextSource, Is.EqualTo("Test Context Source"));
            Assert.That(detail.Metadata, Is.EqualTo("Test Metadata"));
            Assert.That(detail.MetadataSource, Is.EqualTo("Test Metadata Source"));
            Assert.That(detail.Country, Is.EqualTo("CH"));
        }
    }

    [Test]
    public async Task ComposeEntryCreateItems_WithEmptyGroupKey_ShouldSkipRow()
    {
        var sut = CreateTestableService();
        using var workbook = CreateWorkbookWithEmptyGroupKey();

        var actual = await sut.ComposeEntryCreateItemsAccessor(
            workbook, 1, 2,
            Status.InProgress, Reliability.Tentative,
            new List<int>(), new List<int>(), new List<int>(),
            new List<AdminInfoSummary>(), string.Empty);

        Assert.That(actual, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ImportDocument_WithValidData_ShouldImportEntriesSuccessfully()
    {
        const string currentUserEmail = "user@test.com";
        const string importJobLabel = "Excel-Import-Test";
        var entryServiceMock = Substitute.For<IEntryService>();
        var emailServiceMock = Substitute.For<IEmailService>();
        Email emailToSend = null;
        emailServiceMock.Send(Arg.Do<Email>(v => emailToSend = v)).Returns(Task.CompletedTask);
        var sut = CreateService(entryServiceMock, emailServiceMock);
        using var workbook = CreateValidWorkbookWithTestData();

        await sut.ImportDocument(workbook, importJobLabel, currentUserEmail, 1, 2,
            Status.InProgress, Reliability.Tentative,
            new List<int> { 3, 4 }, new List<int> { 5, 6 }, new List<int> { 7, 8 },
            new List<AdminInfoSummary> { new AdminInfoDetail { Code = "A" } }, "Test Comment");

        await entryServiceMock.Received(2).Create(Arg.Any<EntryCreate>(), "test-import-user@test.com");
        await emailServiceMock.Received(1).Send(Arg.Is(emailToSend));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(emailToSend.From.Address, Is.EqualTo("test-sender@test.com"));
            Assert.That(emailToSend.To.First().Address, Is.EqualTo(currentUserEmail));
            Assert.That(emailToSend.Subject, Is.EqualTo(importJobLabel));
            Assert.That(emailToSend.HtmlMessage, Does.Contain("2"));
        }
    }

    [Test]
    public async Task CreateExport_WithCompleteData_ShouldGenerateExcelWithAllFields()
    {
        // Set English culture for stable test environment
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var englishCulture = new CultureInfo("en-US");
        CultureInfo.CurrentCulture = englishCulture;
        CultureInfo.CurrentUICulture = englishCulture;

        try
        {
            var entryRepositoryMock = Substitute.For<IEntryRepository>();
            var documentServiceMock = Substitute.For<IDocumentService>();
            var cultureServiceMock = Substitute.For<ICultureService>();
            var termdatUrlServiceMock = Substitute.For<ITermdatUrlService>();
            Spreadsheet capturedSpreadsheet = null;

            documentServiceMock.CreateExcel(Arg.Do<Spreadsheet>(s => capturedSpreadsheet = s))
                .Returns(Task.FromResult<Stream>(new MemoryStream()));

            cultureServiceMock.GetTranslation(Arg.Any<IEnumerable<IEntityNameTranslation>>())
                .Returns(callInfo =>
                {
                    var translations = callInfo.Arg<IEnumerable<IEntityNameTranslation>>();
                    return translations?.FirstOrDefault()?.Name ?? string.Empty;
                });

            termdatUrlServiceMock.GetTermdatUrl(1).Returns("https://termdat.ch/Entry/1");

            var entries = CreateTestEntries();
            entryRepositoryMock.GetByIdsForExport(Arg.Any<IReadOnlyList<int>>()).Returns(entries);

            var sut = CreateService(entryRepository: entryRepositoryMock, documentService: documentServiceMock,
                cultureService: cultureServiceMock, termdatUrlService: termdatUrlServiceMock);

            var (fileName, stream) = await sut.CreateExport(new List<int> { 1 });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(fileName, Does.Contain("Export-"));
                Assert.That(fileName, Does.EndWith(".xlsx"));
                Assert.That(stream, Is.Not.Null);
                Assert.That(capturedSpreadsheet, Is.Not.Null);
                Assert.That(capturedSpreadsheet.HeaderCells, Has.Count.EqualTo(35));
                Assert.That(capturedSpreadsheet.HeaderCells[0].Format, Is.EqualTo(CellFormat.Bold));
                Assert.That(capturedSpreadsheet.BodyCells, Has.Count.EqualTo(3));

                var firstRow = capturedSpreadsheet.BodyCells[0];
                Assert.That(firstRow[0].Text, Is.EqualTo("1"));
                Assert.That(firstRow[1].Text, Is.EqualTo("1"));
                Assert.That(firstRow[1].FormatType, Is.EqualTo(CellFormatTypes.Number));
                Assert.That(firstRow[2].Text, Is.EqualTo("DE"));
                Assert.That(firstRow[3].Text, Is.EqualTo("https://termdat.ch/Entry/1"));
                Assert.That(firstRow[3].Hyperlink, Is.EqualTo("https://termdat.ch/Entry/1"));
                Assert.That(firstRow[4].Text, Is.EqualTo("O1 - Office A"));
                Assert.That(firstRow[5].Text, Is.EqualTo("C1 - Coll A"));
                Assert.That(firstRow[6].Text, Is.EqualTo("In progress"));
                Assert.That(firstRow[7].Text, Is.EqualTo("2 - Provisional"));
                Assert.That(firstRow[8].Text, Is.EqualTo($"Subj A||Dom A|Dom B{Environment.NewLine}Subj B||Dom C"));
                Assert.That(firstRow[9].Text, Is.EqualTo("RC1 - Rel A"));
                Assert.That(firstRow[10].Text, Is.EqualTo("G1 - Gloss A"));
                Assert.That(firstRow[11].Text, Is.EqualTo("Comment A"));
                Assert.That(firstRow[12].Text, Is.EqualTo("AI1"));
                Assert.That(firstRow[13].Text, Is.EqualTo("GEN,2|SPE,3"));
                Assert.That(firstRow[14].Text, Is.EqualTo("Wikidata,https://w.org|Fedlex,https://f.org|Lindas,https://l.org"));
                Assert.That(firstRow[15].Text, Is.EqualTo("Pos A"));
                Assert.That(firstRow[16].Text, Is.EqualTo("2024-01-01T10:00:00.0000000"));
                Assert.That(firstRow[16].FormatType, Is.EqualTo(CellFormatTypes.Date));
                Assert.That(firstRow[17].Text, Is.EqualTo("2024-01-02T11:00:00.0000000"));
                Assert.That(firstRow[17].FormatType, Is.EqualTo(CellFormatTypes.Date));
                Assert.That(firstRow[18].Text, Is.EqualTo("Name A"));
                Assert.That(firstRow[19].Text, Is.EqualTo("Src A"));
                Assert.That(firstRow[20].Text, Is.EqualTo("Term A"));
                Assert.That(firstRow[21].Text, Is.EqualTo("TSrc A"));
                Assert.That(firstRow[22].Text, Is.EqualTo("Phr A"));
                Assert.That(firstRow[23].Text, Is.EqualTo("PSrc A"));
                Assert.That(firstRow[24].Text, Is.EqualTo("Abb A"));
                Assert.That(firstRow[25].Text, Is.EqualTo("ASrc A"));
                Assert.That(firstRow[26].Text, Is.EqualTo("Def A"));
                Assert.That(firstRow[27].Text, Is.EqualTo("DSrc A"));
                Assert.That(firstRow[28].Text, Is.EqualTo("Note A"));
                Assert.That(firstRow[29].Text, Is.EqualTo("NSrc A"));
                Assert.That(firstRow[30].Text, Is.EqualTo("Ctx A"));
                Assert.That(firstRow[31].Text, Is.EqualTo("CSrc A"));
                Assert.That(firstRow[32].Text, Is.EqualTo("Meta A"));
                Assert.That(firstRow[33].Text, Is.EqualTo("MSrc A"));
                Assert.That(firstRow[34].Text, Is.EqualTo("CH"));

                var secondRow = capturedSpreadsheet.BodyCells[1];
                Assert.That(secondRow[0].Text, Is.EqualTo("1"));
                Assert.That(secondRow[1].Text, Is.EqualTo("1"));
                Assert.That(secondRow[2].Text, Is.EqualTo("FR"));
                Assert.That(secondRow[18].Text, Is.EqualTo("Name B"));
                Assert.That(secondRow[19].Text, Is.EqualTo("Src B"));
                Assert.That(secondRow[20].Text, Is.EqualTo("Term B"));

                var thirdRow = capturedSpreadsheet.BodyCells[2];
                Assert.That(thirdRow[0].Text, Is.EqualTo("1"));
                Assert.That(thirdRow[1].Text, Is.EqualTo("2"));
                Assert.That(thirdRow[2].Text, Is.EqualTo("FR"));
                Assert.That(thirdRow[18].Text, Is.EqualTo("Name C"));
                Assert.That(thirdRow[19].Text, Is.EqualTo("Src C"));
                Assert.That(thirdRow[20].Text, Is.EqualTo("Term C"));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Test]
    public async Task CreateDuplicateCheckExport_ShouldCreateSpreadsheet()
    {
        var documentServiceMock = Substitute.For<IDocumentService>();

        var sut = CreateService(documentService: documentServiceMock);

        var matches = new List<DuplicateCheckMatch>
    {
        new()
        {
            EntryId = 1,
            MatchedKeys = ["Key1", "Key2"],
            MatchedEntryIds = [2, 3]
        }
    };

        var stream = new MemoryStream();

        documentServiceMock
            .CreateExcel(Arg.Any<Spreadsheet>())
            .Returns(stream);

        var (_, content) = await sut.CreateDuplicateCheckExport(matches);

        Assert.That(content, Is.SameAs(stream));

        await documentServiceMock.Received(1).CreateExcel(
            Arg.Is<Spreadsheet>(x =>
                x.HeaderCells.Count == 4 &&
                x.BodyCells.Count == 1 &&
                x.BodyCells[0].Count == 4 &&
                x.BodyCells[0][0].Text == "1" &&
                x.BodyCells[0][1].Text == "Key1, Key2" &&
                x.BodyCells[0][2].Text == "2" &&
                x.BodyCells[0][3].Text == "3"));
    }

    [Test]
    public async Task CreateDuplicateCheckExport_ShouldCreateHeaderForMaximumDuplicateCount()
    {
        var documentServiceMock = Substitute.For<IDocumentService>();

        var sut = CreateService(documentService: documentServiceMock);

        var matches = new List<DuplicateCheckMatch>
    {
        new()
        {
            EntryId = 1,
            MatchedKeys = ["A"],
            MatchedEntryIds = [2]
        },
        new()
        {
            EntryId = 3,
            MatchedKeys = ["B"],
            MatchedEntryIds = [4, 5, 6]
        }
    };

        documentServiceMock
            .CreateExcel(Arg.Any<Spreadsheet>())
            .Returns(new MemoryStream());

        await sut.CreateDuplicateCheckExport(matches);

        await documentServiceMock.Received(1).CreateExcel(
            Arg.Is<Spreadsheet>(x => x.HeaderCells.Count == 5));
    }

    [Test]
    public async Task CreateDuplicateCheckExport_ShouldReturnFileName()
    {
        var documentServiceMock = Substitute.For<IDocumentService>();

        var sut = CreateService(documentService: documentServiceMock);

        var matches = new List<DuplicateCheckMatch>();

        documentServiceMock
            .CreateExcel(Arg.Any<Spreadsheet>())
            .Returns(new MemoryStream());

        var (fileName, _) = await sut.CreateDuplicateCheckExport(matches);

        var expected = $"{DateTime.Now:yyyyMMdd}_{BackendTexts.ResourceManager.GetString("DuplicateCheck.Export.XLSX.FileName")}.xlsx";

        Assert.That(fileName, Is.EqualTo(expected));
    }

    private static List<Entry> CreateTestEntries()
    {
        return
        [
            new Entry
            {
                Id = 1,
                Status = Domain.Models.Status.InProgress,
                Reliability = Domain.Models.Reliability.Tentative,
                Comment = "Comment A",
                Created = new DateTime(2024, 1, 1, 10, 0, 0),
                Modified = new DateTime(2024, 1, 2, 11, 0, 0),
                Office = new Office
                {
                    Code = "O1",
                    Translations = new List<OfficeTranslation> { new() { Name = "Office A" } }
                },
                Collection = new Collection
                {
                    Code = "C1",
                    Translations = new List<CollectionTranslation> { new() { Name = "Coll A" } }
                },
                ConceptSystemPosition = new ConceptSystemPosition { Value = "Pos A" },
                Subjects = new List<Subject>
                {
                    new()
                    {
                        Id = 1, Translations = new List<SubjectTranslation> { new() { Name = "Subj A" } }
                    },
                    new()
                    {
                        Id = 2, Translations = new List<SubjectTranslation> { new() { Name = "Subj B" } }
                    }
                },
                Domains = new List<Domain.Models.Domain>
                {
                    new()
                    {
                        Id = 1,
                        SubjectId = 1,
                        Translations = new List<DomainTranslation> { new() { Name = "Dom A" } }
                    },
                    new()
                    {
                        Id = 2,
                        SubjectId = 1,
                        Translations = new List<DomainTranslation> { new() { Name = "Dom B" } }
                    },
                    new()
                    {
                        Id = 3,
                        SubjectId = 2,
                        Translations = new List<DomainTranslation> { new() { Name = "Dom C" } }
                    }
                },
                RelatedCollections = new List<Collection>
                {
                    new()
                    {
                        Code = "RC1",
                        Translations = new List<CollectionTranslation> { new() { Name = "Rel A" } }
                    }
                },
                Glossaries = new List<Glossary>
                {
                    new()
                    {
                        Code = "G1",
                        Translations = new List<GlossaryTranslation> { new() { Name = "Gloss A" } }
                    }
                },
                AdminInfos = new List<AdminInfo> { new() { Code = "AI1" } },
                References = new List<EntryReference>
                {
                    new()
                    {
                        Type = ReferenceType.General,
                        ReferencedEntryId = 2,
                        ReferencedEntry = new Entry { Deleted = false }
                    },
                    new()
                    {
                        Type = ReferenceType.Specific,
                        ReferencedEntryId = 3,
                        ReferencedEntry = new Entry { Deleted = false }
                    }
                },
                ExternalLinks = new List<EntryExternalLink>
                {
                    new() { Type = ExternalLinkType.Wikidata, Url = "https://w.org" },
                    new() { Type = ExternalLinkType.Fedlex, Url = "https://f.org" },
                    new() { Type = ExternalLinkType.Lindas, Url = "https://l.org" }
                },
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        Sequence = 1,
                        Language = new Language { IsoCode = "de" },
                        Name = "Name A",
                        NameSource = "Src A",
                        Terminus = "Term A",
                        TerminusSource = "TSrc A",
                        Phraseology = "Phr A",
                        PhraseologySource = "PSrc A",
                        Abbreviation = "Abb A",
                        AbbreviationSource = "ASrc A",
                        Definition = "Def A",
                        DefinitionSource = "DSrc A",
                        Note = "Note A",
                        NoteSource = "NSrc A",
                        Context = "Ctx A",
                        ContextSource = "CSrc A",
                        Metadata = "Meta A",
                        MetadataSource = "MSrc A",
                        Country = "CH"
                    },
                    new()
                    {
                        Sequence = 1,
                        Language = new Language { IsoCode = "fr" },
                        Name = "Name B",
                        NameSource = "Src B",
                        Terminus = "Term B"
                    },
                    new()
                    {
                        Sequence = 2,
                        Language = new Language { IsoCode = "fr" },
                        Name = "Name C",
                        NameSource = "Src C",
                        Terminus = "Term C"
                    }
                }
            }
        ];
    }

    private ExcelDataService CreateService(IEntryService entryService = null, IEmailService emailService = null,
        IDocumentService documentService = null, IEntryRepository entryRepository = null,
        ICultureService cultureService = null, ITermdatUrlService termdatUrlService = null)
    {
        var languageRepositoryMock = Substitute.For<ILanguageRepository>();
        languageRepositoryMock.GetAll().Returns(new List<Language>
        {
            new() { Id = 1, IsoCode = "de" }, new() { Id = 2, IsoCode = "fr" }, new() { Id = 3, IsoCode = "en" }
        });

        return new ExcelDataService(
            fileImportOptionsMock,
            emailOptionsMock,
            new NullLogger<ExcelDataService>(),
            languageRepositoryMock,
            entryService ?? Substitute.For<IEntryService>(),
            emailService ?? Substitute.For<IEmailService>(),
            documentService ?? Substitute.For<IDocumentService>(),
            entryRepository ?? Substitute.For<IEntryRepository>(),
            cultureService ?? Substitute.For<ICultureService>(),
            termdatUrlService ?? Substitute.For<ITermdatUrlService>()
        );
    }

    private TestableExcelDataService CreateTestableService()
    {
        var languageRepositoryMock = Substitute.For<ILanguageRepository>();
        languageRepositoryMock.GetAll().Returns(new List<Language>
        {
            new() { Id = 1, IsoCode = "de" }, new() { Id = 2, IsoCode = "fr" }, new() { Id = 3, IsoCode = "en" }
        });

        return new TestableExcelDataService(
            fileImportOptionsMock,
            emailOptionsMock,
            languageRepositoryMock,
            Substitute.For<IEntryService>(),
            Substitute.For<IEmailService>(),
            Substitute.For<IDocumentService>(),
            Substitute.For<IEntryRepository>(),
            Substitute.For<ICultureService>(),
            Substitute.For<ITermdatUrlService>());
    }

    private static IFormFile CreateFormFile(XLWorkbook workbook)
    {
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var formFileMock = Substitute.For<IFormFile>();
        formFileMock.OpenReadStream().Returns(stream);
        formFileMock.FileName.Returns("test.xlsx");
        return formFileMock;
    }


    private static XLWorkbook CreateWorkbookWithHeaderOnly()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        AddHeaders(worksheet);

        return workbook;
    }

    private static XLWorkbook CreateWorkbookWithInsufficientColumns()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        // Only 10 columns
        for (var i = 1; i <= 10; i++)
        {
            worksheet.Cell(1, i).Value = $"Col{i}";
        }

        worksheet.Cell(2, 1).Value = "1";

        return workbook;
    }

    private static XLWorkbook CreateValidWorkbookWithTestData()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        AddHeaders(worksheet);

        // Entry 1, Language 1
        worksheet.Cell(2, 1).Value = "1"; // GroupKey
        worksheet.Cell(2, 3).Value = "1"; // Sequence
        worksheet.Cell(2, 4).Value = "De"; // Language
        worksheet.Cell(2, 5).Value = "Name DE 1"; // Name
        worksheet.Cell(2, 7).Value = "Terminus 1"; // Terminus

        // Entry 1, Language 2
        worksheet.Cell(3, 1).Value = "1"; // GroupKey
        worksheet.Cell(3, 3).Value = "2"; // Sequence
        worksheet.Cell(3, 4).Value = "fR"; // Language
        worksheet.Cell(3, 5).Value = "Name FR 2"; // Name
        worksheet.Cell(3, 7).Value = "Terminus 2"; // Terminus

        // Entry 2, Language 1
        worksheet.Cell(4, 1).Value = "2"; // GroupKey
        worksheet.Cell(4, 3).Value = "1"; // Sequence
        worksheet.Cell(4, 4).Value = "EN"; // Language
        worksheet.Cell(4, 5).Value = "Name EN 3"; // Name
        worksheet.Cell(4, 7).Value = "Terminus 3"; // Terminus

        return workbook;
    }

    private static XLWorkbook CreateWorkbookWithMultipleDefinitions()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        AddHeaders(worksheet);

        worksheet.Cell(2, 1).Value = "1";
        worksheet.Cell(2, 3).Value = "1";
        worksheet.Cell(2, 4).Value = "de";
        worksheet.Cell(2, 13).Value = "Test Definition";
        worksheet.Cell(2, 14).Value = "Test Definition Source";

        worksheet.Cell(3, 1).Value = "1";
        worksheet.Cell(3, 3).Value = "2";
        worksheet.Cell(3, 4).Value = "de";
        worksheet.Cell(3, 13).Value = "Definition should be ignored";
        worksheet.Cell(3, 14).Value = "Definition Source should be ignored";

        return workbook;
    }

    private static XLWorkbook CreateWorkbookWithInvalidLanguage()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        AddHeaders(worksheet);

        worksheet.Cell(2, 1).Value = "1";
        worksheet.Cell(2, 3).Value = "1";
        worksheet.Cell(2, 4).Value = "invalid_lang";

        return workbook;
    }

    private static XLWorkbook CreateWorkbookWithMissingSequence()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        AddHeaders(worksheet);

        worksheet.Cell(2, 1).Value = "1";
        worksheet.Cell(2, 3).Value = ""; // Missing sequence
        worksheet.Cell(2, 4).Value = "de";

        return workbook;
    }

    private static XLWorkbook CreateWorkbookWithAllFields()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        AddHeaders(worksheet);

        worksheet.Cell(2, 1).Value = "1"; // GroupKey
        worksheet.Cell(2, 3).Value = "1"; // Sequence
        worksheet.Cell(2, 4).Value = "de"; // Language
        worksheet.Cell(2, 5).Value = "Test Name";
        worksheet.Cell(2, 6).Value = "Test Name Source";
        worksheet.Cell(2, 7).Value = "Test Terminus";
        worksheet.Cell(2, 8).Value = "Test Terminus Source";
        worksheet.Cell(2, 9).Value = "Test Phraseology";
        worksheet.Cell(2, 10).Value = "Test Phraseology Source";
        worksheet.Cell(2, 11).Value = "Test Abbreviation";
        worksheet.Cell(2, 12).Value = "Test Abbreviation Source";
        worksheet.Cell(2, 13).Value = "Test Definition";
        worksheet.Cell(2, 14).Value = "Test Definition Source";
        worksheet.Cell(2, 15).Value = "Test Note";
        worksheet.Cell(2, 16).Value = "Test Note Source";
        worksheet.Cell(2, 17).Value = "Test Context";
        worksheet.Cell(2, 18).Value = "Test Context Source";
        worksheet.Cell(2, 19).Value = "Test Metadata";
        worksheet.Cell(2, 20).Value = "Test Metadata Source";
        worksheet.Cell(2, 21).Value = "CH";

        return workbook;
    }

    private static XLWorkbook CreateWorkbookWithEmptyGroupKey()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        AddHeaders(worksheet);

        // Row with empty GroupKey (should be skipped)
        worksheet.Cell(2, 1).Value = "";
        worksheet.Cell(2, 3).Value = "1";
        worksheet.Cell(2, 4).Value = "de";

        // Row with valid GroupKey
        worksheet.Cell(3, 1).Value = "1";
        worksheet.Cell(3, 3).Value = "2";
        worksheet.Cell(3, 4).Value = "de";

        return workbook;
    }

    private static void AddHeaders(IXLWorksheet worksheet)
    {
        worksheet.Cell(1, 1).Value = "GroupKey";
        worksheet.Cell(1, 2).Value = "EntryId";
        worksheet.Cell(1, 3).Value = "Sequence";
        worksheet.Cell(1, 4).Value = "Language";
        worksheet.Cell(1, 5).Value = "Name";
        worksheet.Cell(1, 6).Value = "NameSource";
        worksheet.Cell(1, 7).Value = "Terminus";
        worksheet.Cell(1, 8).Value = "TerminusSource";
        worksheet.Cell(1, 9).Value = "Phraseology";
        worksheet.Cell(1, 10).Value = "PhraseologySource";
        worksheet.Cell(1, 11).Value = "Abbreviation";
        worksheet.Cell(1, 12).Value = "AbbreviationSource";
        worksheet.Cell(1, 13).Value = "Definition";
        worksheet.Cell(1, 14).Value = "DefinitionSource";
        worksheet.Cell(1, 15).Value = "Note";
        worksheet.Cell(1, 16).Value = "NoteSource";
        worksheet.Cell(1, 17).Value = "Context";
        worksheet.Cell(1, 18).Value = "ContextSource";
        worksheet.Cell(1, 19).Value = "Metadata";
        worksheet.Cell(1, 20).Value = "MetadataSource";
        worksheet.Cell(1, 21).Value = "Country";
    }
}
