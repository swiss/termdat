using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Microsoft.AspNetCore.Http;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IFileImportService
{
    Task<(bool isValid, IList<KeyValuePair<string, string>> validationErrors, object document, string fileType)> IsFileValid(
        IFormFile file);

    Task ImportDocument(object document, string fileType, string importJobLabel, string currentUserEmail, int entryOfficeId, int entryCollectionId,
        Status status, Reliability reliability, IList<int> subjectIds, IList<int> domainIds, IList<int> glossaryIds,
        IList<AdminInfoSummary> adminInfos, string comment);
}

