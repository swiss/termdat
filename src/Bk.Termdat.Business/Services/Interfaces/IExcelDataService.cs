using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IExcelDataService
{
    Task<(bool isValid, IList<KeyValuePair<string, string>> validationErrors, XLWorkbook workbook)> IsFileValid(
        IFormFile file);

    Task ImportDocument(XLWorkbook workbook, string importJobLabel, string currentUserEmail, int entryOfficeId, int entryCollectionId,
        Status status, Reliability reliability, IList<int> subjectIds, IList<int> domainIds, IList<int> glossaryIds,
        IList<AdminInfoSummary> adminInfos, string comment);

    Task<(string fileName, Stream content)> CreateExport(IReadOnlyList<int> ids);

    Task<(string fileName, Stream content)> CreateDuplicateCheckExport(IList<DuplicateCheckMatch> matches);
}

