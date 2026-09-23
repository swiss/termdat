using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IEntryTemplateService
{
    Task Create(EntryTemplateCreate entryTemplateCreate);
    Task<IEnumerable<EntryTemplateSummary>> GetForUser();
    Task<EntryModificationDetail> GetTemplateById(int id);
    Task Update(EntryTemplateUpdate entryTemplateUpdate);
    Task Delete(int id);
}
