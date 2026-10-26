using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Private.Api.Controllers;
using Bk.Termdat.Private.Api.HostedServices;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Private.Api.Tests.Controllers;

[TestFixture]
internal class FilesControllerTests
{
    [SetUp]
    public void Setup()
    {
        fileImportServiceMock = Substitute.For<IFileImportService>();
        backgroundTaskQueueMock = Substitute.For<IBackgroundTaskQueue>();
        identityServiceMock = Substitute.For<IIdentityService>();
        authorizationServiceMock = Substitute.For<IAuthorizationService>();
    }

    private IFileImportService fileImportServiceMock;
    private IBackgroundTaskQueue backgroundTaskQueueMock;
    private IIdentityService identityServiceMock;
    private IAuthorizationService authorizationServiceMock;
    private readonly IFormFile fileMock = Substitute.For<IFormFile>();
    private readonly IOptions<FileImportOptions> optionsMock = Substitute.For<IOptions<FileImportOptions>>();

    [Test]
    public async Task Import_WithValidTbxImportRequestData_ShouldReturnOkResult()
    {
        var xDocumentMock = new XDocument();
        var importRequest = new FileImportRequest
        {
            File = fileMock,
            EntryOffice = 1,
            EntryCollection = 2,
            Status = Status.InProgress,
            Reliability = Reliability.FirstDraft,
            SubjectIds = new List<int>(0),
            DomainIds = new List<int>(0),
            Comment = "A"
        };
        identityServiceMock.GetCurrentEmail().Returns("A");
        fileMock.FileName.Returns("file.tbx");
        fileImportServiceMock.IsFileValid(fileMock).Returns((true, null, xDocumentMock, "tbx"));
        optionsMock.Value.Returns(new FileImportOptions { Email = "dummy" });
        var sut = CreateSystemUnderTest();

        var result = await sut.Import(importRequest);

        await backgroundTaskQueueMock.Received(1)
            .QueueBackgroundWorkItemAsync(Arg.Any<Func<CancellationToken, IServiceProvider, Task>>());
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        var importResponse = response.Value as FileImportResponse;
        Assert.That(importResponse, Is.Not.Null);
        Assert.That(importResponse.AdminInfoCode.StartsWith("TBX-Import-", StringComparison.Ordinal), Is.True);
    }

    [Test]
    public async Task Import_WithValidExcelImportRequestData_ShouldReturnOkResult()
    {
        using var xlWorkbook = new XLWorkbook();
        var importRequest = new FileImportRequest
        {
            File = fileMock,
            EntryOffice = 1,
            EntryCollection = 2,
            Status = Status.InProgress,
            Reliability = Reliability.FirstDraft,
            SubjectIds = new List<int>(0),
            DomainIds = new List<int>(0),
            Comment = "A"
        };
        identityServiceMock.GetCurrentEmail().Returns("A");
        fileMock.FileName.Returns("file.xlsx");
        fileImportServiceMock.IsFileValid(fileMock).Returns((true, null, xlWorkbook, "xlsx"));
        optionsMock.Value.Returns(new FileImportOptions { Email = "dummy" });
        var sut = CreateSystemUnderTest();

        var result = await sut.Import(importRequest);

        await backgroundTaskQueueMock.Received(1)
            .QueueBackgroundWorkItemAsync(Arg.Any<Func<CancellationToken, IServiceProvider, Task>>());
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        var importResponse = response.Value as FileImportResponse;
        Assert.That(importResponse, Is.Not.Null);
        Assert.That(importResponse.AdminInfoCode.StartsWith("XLSX-Import-", StringComparison.Ordinal), Is.True);
    }

    [Test]
    public async Task Import_WithInvalidFile_ShouldReturnBadRequestWithInvalidFileMessage()
    {
        identityServiceMock.GetCurrentEmail().Returns("A");
        fileMock.FileName.Returns("file.xyz");
        var request = new FileImportRequest { File = fileMock };
        fileImportServiceMock.IsFileValid(fileMock)
            .Returns((false, new List<KeyValuePair<string, string>> { new("A", "B") }, null, null));
        var sut = CreateSystemUnderTest();

        var result = await sut.Import(request);

        var response = result as BadRequestObjectResult;
        Assert.That(response, Is.Not.Null);
        var validationProblemDetails = response.Value as ValidationProblemDetails;
        Assert.That(validationProblemDetails, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(validationProblemDetails.Errors.First().Key, Is.EqualTo("A"));
            Assert.That(validationProblemDetails.Errors.First().Value.First(), Is.EqualTo("B"));
        });
        await backgroundTaskQueueMock.DidNotReceive()
            .QueueBackgroundWorkItemAsync(Arg.Any<Func<CancellationToken, IServiceProvider, Task>>());
    }

    [Test]
    public async Task Import_WithInvalidProcessingUser_ShouldReturnBadRequest()
    {
        identityServiceMock.GetCurrentEmail().Returns("");
        var sut = CreateSystemUnderTest();

        var result = await sut.Import(new FileImportRequest());

        var response = result as BadRequestObjectResult;
        Assert.That(response, Is.Not.Null);
        var validationProblemDetails = response.Value as ValidationProblemDetails;
        Assert.That(validationProblemDetails, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(validationProblemDetails.Errors.First().Key, Is.EqualTo("UnexpectedError"));
            Assert.That(validationProblemDetails.Errors.First().Value.First(),
                Is.EqualTo("Current user email is not available"));
        });
        await backgroundTaskQueueMock.DidNotReceive()
            .QueueBackgroundWorkItemAsync(Arg.Any<Func<CancellationToken, IServiceProvider, Task>>());
    }

    [Test]
    public async Task Import_WithUnexpectedError_ShouldReturnBadRequest()
    {
        identityServiceMock.GetCurrentEmail().Returns("A");
        fileMock.FileName.Returns("file.tbx");
        fileImportServiceMock.IsFileValid(fileMock).Throws(new ArgumentException("test"));
        var request = new FileImportRequest { File = fileMock };
        var sut = CreateSystemUnderTest();

        var result = await sut.Import(request);

        var response = result as BadRequestObjectResult;
        Assert.That(response, Is.Not.Null);
        var validationProblemDetails = response.Value as ValidationProblemDetails;
        Assert.That(validationProblemDetails, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(validationProblemDetails.Errors.First().Key, Is.EqualTo("UnexpectedError"));
            Assert.That(validationProblemDetails.Errors.First().Value.First(),
                Is.EqualTo("Unexpected error during import task creation attempt"));
        });
        await backgroundTaskQueueMock.DidNotReceive()
            .QueueBackgroundWorkItemAsync(Arg.Any<Func<CancellationToken, IServiceProvider, Task>>());
    }

    private FilesController CreateSystemUnderTest()
    {
        return new FilesController(fileImportServiceMock, backgroundTaskQueueMock, new NullLogger<FilesController>(),
            identityServiceMock, authorizationServiceMock);
    }
}
