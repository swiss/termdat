using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
public class FileImportServiceTests
{
    [SetUp]
    public void SetUp()
    {
        tbxDataServiceMock = Substitute.For<ITbxDataService>();
        excelDataServiceMock = Substitute.For<IExcelDataService>();
        formFileMock = Substitute.For<IFormFile>();
    }

    private ITbxDataService tbxDataServiceMock;
    private IExcelDataService excelDataServiceMock;
    private IFormFile formFileMock;

    [Test]
    public async Task IsFileValid_WithTbxFile_ShouldDelegateToTbxDataService()
    {
        var sut = CreateService();
        var xDocument = new XDocument();
        var expectedErrors = new List<KeyValuePair<string, string>>();

        formFileMock.FileName.Returns("test.tBx");
        tbxDataServiceMock.IsFileValid(formFileMock).Returns((true, expectedErrors, xDocument));

        var (isValid, validationErrors, document, fileType) = await sut.IsFileValid(formFileMock);

        await tbxDataServiceMock.Received(1).IsFileValid(formFileMock);
        await excelDataServiceMock.DidNotReceive().IsFileValid(Arg.Any<IFormFile>());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(validationErrors, Is.EqualTo(expectedErrors));
            Assert.That(document, Is.EqualTo(xDocument));
            Assert.That(fileType, Is.EqualTo("tbx"));
        }
    }

    [Test]
    public async Task IsFileValid_WithInvalidTbxFile_ShouldReturnValidationErrors()
    {
        var sut = CreateService();
        var expectedErrors = new List<KeyValuePair<string, string>> { new("A", "B") };

        formFileMock.FileName.Returns("invalid.tbx");
        tbxDataServiceMock.IsFileValid(formFileMock).Returns((false, expectedErrors, null));

        var (isValid, validationErrors, document, fileType) = await sut.IsFileValid(formFileMock);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationErrors, Has.Count.EqualTo(1));
            Assert.That(validationErrors[0].Key, Is.EqualTo("A"));
            Assert.That(validationErrors[0].Value, Is.EqualTo("B"));
            Assert.That(document, Is.Null);
            Assert.That(fileType, Is.EqualTo("tbx"));
        }
    }

    [Test]
    public async Task IsFileValid_WithXlsxFile_ShouldDelegateToExcelDataService()
    {
        var sut = CreateService();
        var workbook = new XLWorkbook();
        var expectedErrors = new List<KeyValuePair<string, string>>();

        formFileMock.FileName.Returns("test.xLSx");
        excelDataServiceMock.IsFileValid(formFileMock).Returns((true, expectedErrors, workbook));

        var (isValid, validationErrors, document, fileType) = await sut.IsFileValid(formFileMock);

        await excelDataServiceMock.Received(1).IsFileValid(formFileMock);
        await tbxDataServiceMock.DidNotReceive().IsFileValid(Arg.Any<IFormFile>());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(validationErrors, Is.EqualTo(expectedErrors));
            Assert.That(document, Is.EqualTo(workbook));
            Assert.That(fileType, Is.EqualTo("xlsx"));
        }
    }

    [Test]
    public async Task IsFileValid_WithInvalidXlsxFile_ShouldReturnValidationErrors()
    {
        var sut = CreateService();
        var expectedErrors = new List<KeyValuePair<string, string>> { new("A", "B") };

        formFileMock.FileName.Returns("invalid.xlsx");
        excelDataServiceMock.IsFileValid(formFileMock).Returns((false, expectedErrors, null));

        var (isValid, validationErrors, document, fileType) = await sut.IsFileValid(formFileMock);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationErrors, Has.Count.EqualTo(1));
            Assert.That(validationErrors[0].Key, Is.EqualTo("A"));
            Assert.That(validationErrors[0].Value, Is.EqualTo("B"));
            Assert.That(document, Is.Null);
            Assert.That(fileType, Is.EqualTo("xlsx"));
        }
    }

    [Test]
    public async Task IsFileValid_WithUnsupportedFileType_ShouldReturnValidationError()
    {
        var sut = CreateService();
        formFileMock.FileName.Returns("document.pdf");

        var (isValid, validationErrors, document, fileType) = await sut.IsFileValid(formFileMock);

        await tbxDataServiceMock.DidNotReceive().IsFileValid(Arg.Any<IFormFile>());
        await excelDataServiceMock.DidNotReceive().IsFileValid(Arg.Any<IFormFile>());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationErrors, Has.Count.EqualTo(1));
            Assert.That(validationErrors[0].Key, Is.EqualTo("FileType"));
            Assert.That(validationErrors[0].Value,
                Is.EqualTo("Unsupported file type. Only .tbx and .xlsx files are supported"));
            Assert.That(document, Is.Null);
            Assert.That(fileType, Is.Null);
        }
    }

    [Test]
    public async Task ImportDocument_WithTbxDocument_ShouldDelegateToTbxDataService()
    {
        var sut = CreateService();
        var xDocument = new XDocument();
        var adminInfos = new List<AdminInfoSummary>();

        await sut.ImportDocument(
            xDocument, "tbx", "TBX-Import-XYZ", "test@test.com",
            1, 2, Status.InProgress, Reliability.FirstDraft,
            new List<int> { 3 }, new List<int> { 4 }, new List<int> { 5 },
            adminInfos, "Test comment");

        await tbxDataServiceMock.Received(1).ImportDocument(
            xDocument, "TBX-Import-XYZ", "test@test.com",
            1, 2, Status.InProgress, Reliability.FirstDraft,
            Arg.Is<IList<int>>(list => list.Count == 1 && list[0] == 3),
            Arg.Is<IList<int>>(list => list.Count == 1 && list[0] == 4),
            Arg.Is<IList<int>>(list => list.Count == 1 && list[0] == 5),
            adminInfos, "Test comment");
        await excelDataServiceMock.DidNotReceive().ImportDocument(
            Arg.Any<XLWorkbook>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<Status>(), Arg.Any<Reliability>(),
            Arg.Any<IList<int>>(), Arg.Any<IList<int>>(), Arg.Any<IList<int>>(),
            Arg.Any<IList<AdminInfoSummary>>(), Arg.Any<string>());
    }

    [Test]
    public async Task ImportDocument_WithXlsxDocument_ShouldDelegateToExcelDataService()
    {
        var sut = CreateService();
        var workbook = new XLWorkbook();
        var adminInfos = new List<AdminInfoSummary>();

        await sut.ImportDocument(
            workbook, "xlsx", "XLSX-Import-XYZ", "test@test.com",
            1, 2, Status.InProgress, Reliability.FirstDraft,
            new List<int> { 3 }, new List<int> { 4 }, new List<int> { 5 },
            adminInfos, "Test comment");

        await excelDataServiceMock.Received(1).ImportDocument(
            workbook, "XLSX-Import-XYZ", "test@test.com",
            1, 2, Status.InProgress, Reliability.FirstDraft,
            Arg.Is<IList<int>>(list => list.Count == 1 && list[0] == 3),
            Arg.Is<IList<int>>(list => list.Count == 1 && list[0] == 4),
            Arg.Is<IList<int>>(list => list.Count == 1 && list[0] == 5),
            adminInfos, "Test comment");
        await tbxDataServiceMock.DidNotReceive().ImportDocument(
            Arg.Any<XDocument>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<Status>(), Arg.Any<Reliability>(),
            Arg.Any<IList<int>>(), Arg.Any<IList<int>>(), Arg.Any<IList<int>>(),
            Arg.Any<IList<AdminInfoSummary>>(), Arg.Any<string>());
    }

    [Test]
    public void ImportDocument_WithInvalidDocumentTypeForTbx_ShouldThrowInvalidOperationException()
    {
        var sut = CreateService();
        var workbook = new XLWorkbook();

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.ImportDocument(
                workbook, "tbx", "TBX-Import-XYZ", "test@test.com",
                1, 2, Status.InProgress, Reliability.FirstDraft,
                new List<int>(), new List<int>(), new List<int>(),
                new List<AdminInfoSummary>(), "Test comment"));

        Assert.That(exception?.Message, Is.EqualTo("Document is not a valid XDocument for TBX import"));
    }

    [Test]
    public void ImportDocument_WithInvalidDocumentTypeForXlsx_ShouldThrowInvalidOperationException()
    {
        var sut = CreateService();
        var xDocument = new XDocument();

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.ImportDocument(
                xDocument, "xlsx", "XLSX-Import-XYZ", "test@test.com",
                1, 2, Status.InProgress, Reliability.FirstDraft,
                new List<int>(), new List<int>(), new List<int>(),
                new List<AdminInfoSummary>(), "Test comment"));

        Assert.That(exception?.Message, Is.EqualTo("Document is not a valid XLWorkbook for Excel import"));
    }

    [Test]
    public void ImportDocument_WithUnsupportedFileType_ShouldThrowInvalidOperationException()
    {
        var sut = CreateService();
        var document = new object();

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.ImportDocument(
                document, "pdf", "PDF-Import-XYZ", "test@test.com",
                1, 2, Status.InProgress, Reliability.FirstDraft,
                new List<int>(), new List<int>(), new List<int>(),
                new List<AdminInfoSummary>(), "Test comment"));

        Assert.That(exception?.Message, Is.EqualTo("Unsupported file type: pdf"));
    }

    private FileImportService CreateService()
    {
        return new FileImportService(tbxDataServiceMock, excelDataServiceMock);
    }
}
