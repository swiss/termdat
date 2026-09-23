using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface ISourceTemplateService
{
    SourceTemplateDefaults GetAllDefaults();
    Task Create(SourceTemplateModification sourceTemplateCreate);
    Task Update(int id, SourceTemplateModification sourceTemplateUpdate);
    Task Delete(int id);
    Task<IEnumerable<SourceTemplateSummary>> GetAllForCurrentUser();
    Task<IEnumerable<string>> GetAllForCurrentUserAsText(string isoCode);
}
