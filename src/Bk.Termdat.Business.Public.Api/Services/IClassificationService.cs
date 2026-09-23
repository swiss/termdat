using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Public.Api.Resource.Models;

namespace Bk.Termdat.Business.Public.Api.Services;

public interface IClassificationService
{
    Task<IEnumerable<ClassificationSummary>> GetAll(string languageIsoCode);
}
