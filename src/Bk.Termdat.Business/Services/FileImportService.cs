using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

namespace Bk.Termdat.Business.Services;

public class FileImportService : IFileImportService
{
    private readonly IExcelDataService excelDataService;
    private readonly ITbxDataService tbxDataService;

    public FileImportService(ITbxDataService tbxDataService, IExcelDataService excelDataService)
    {
        this.tbxDataService = tbxDataService;
        this.excelDataService = excelDataService;
    }

    public async Task<(bool isValid, IList<KeyValuePair<string, string>> validationErrors, object document, string
        fileType)> IsFileValid(
        IFormFile file)
    {
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        switch (fileExtension)
        {
            case ".tbx":
                var (tbxIsValid, tbxErrors, tbxDocument) = await tbxDataService.IsFileValid(file);
                return (tbxIsValid, tbxErrors, tbxDocument, "tbx");

            case ".xlsx":
                var (excelIsValid, excelErrors, excelWorkbook) = await excelDataService.IsFileValid(file);
                return (excelIsValid, excelErrors, excelWorkbook, "xlsx");

            default:
                var errors = new List<KeyValuePair<string, string>>
                {
                    new("FileType", "Unsupported file type. Only .tbx and .xlsx files are supported")
                };
                return (false, errors, null, null);
        }
    }

    public async Task ImportDocument(object document, string fileType, string importJobLabel, string currentUserEmail,
        int entryOfficeId, int entryCollectionId,
        Status status, Reliability reliability, IList<int> subjectIds, IList<int> domainIds, IList<int> glossaryIds,
        IList<AdminInfoSummary> adminInfos, string comment)
    {
        switch (fileType)
        {
            case "tbx":
                if (document is not XDocument tbxDocument)
                {
                    throw new InvalidOperationException("Document is not a valid XDocument for TBX import");
                }

                await tbxDataService.ImportDocument(tbxDocument, importJobLabel, currentUserEmail, entryOfficeId,
                    entryCollectionId,
                    status, reliability, subjectIds, domainIds, glossaryIds, adminInfos, comment);
                break;

            case "xlsx":
                if (document is not XLWorkbook excelWorkbook)
                {
                    throw new InvalidOperationException("Document is not a valid XLWorkbook for Excel import");
                }

                await excelDataService.ImportDocument(excelWorkbook, importJobLabel, currentUserEmail, entryOfficeId,
                    entryCollectionId,
                    status, reliability, subjectIds, domainIds, glossaryIds, adminInfos, comment);
                break;

            default:
                throw new InvalidOperationException($"Unsupported file type: {fileType}");
        }
    }
}
