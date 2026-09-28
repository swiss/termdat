using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using ClosedXML.Excel;
using Microsoft.Extensions.Options;
using Swiss.FCh.DocumentService.Client;
using Swiss.FCh.Utils.Services;

namespace Bk.Termdat.Business.Tests.Services;

public class TestableExcelDataService : ExcelDataService
{
    public TestableExcelDataService(
        IOptions<FileImportOptions> fileImportOptions,
        IOptions<EmailOptions> emailOptions,
        ILanguageRepository languageRepository,
        IEntryService entryService,
        IEmailService emailService,
        IDocumentService documentService,
        IEntryRepository entryRepository,
        ICultureService cultureService,
        ITermdatUrlService termdatUrlService) : base(
        fileImportOptions,
        emailOptions,
        null,
        languageRepository,
        entryService,
        emailService,
        documentService,
        entryRepository,
        cultureService,
        termdatUrlService)
    {
    }

    public async Task<IList<EntryCreate>> ComposeEntryCreateItemsAccessor(XLWorkbook workbook,
        int entryOfficeId, int entryCollectionId, Status status,
        Reliability reliability, IList<int> subjectIds, IList<int> domainIds, IList<int> glossaryIds,
        IList<AdminInfoSummary> adminInfos, string comment)
    {
        return await ComposeEntryCreateItems(workbook, entryOfficeId, entryCollectionId,
            status, reliability, subjectIds, domainIds, glossaryIds, adminInfos, comment);
    }
}
