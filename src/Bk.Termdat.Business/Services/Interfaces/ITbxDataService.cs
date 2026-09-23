using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bk.Termdat.Business.Resource.Models;
using Microsoft.AspNetCore.Http;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface ITbxDataService
{
    Task<(bool isValid, IList<KeyValuePair<string, string>> validationErrors, XDocument document)> IsFileValid(
        IFormFile file);

    Task ImportDocument(XDocument document, string importJobLabel, string currentUserEmail, int entryOfficeId, int entryCollectionId,
        Status status, Reliability reliability, IList<int> subjectIds, IList<int> domainIds, IList<int> glossaryIds,
        IList<AdminInfoSummary> adminInfos, string comment);
}
