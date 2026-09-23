using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IGlossaryService
{
    Task<IEnumerable<GlossaryDetail>> GetAllDetails();
    Task<IEnumerable<GlossarySummary>> GetAll();
    Task<IEnumerable<GlossarySummary>> GetAllForModification();
    Task<GlossaryUpdate> GetDetail(int id);
    Task<GlossaryDetail> Create(GlossaryCreate glossaryCreate);
    Task<GlossaryDetail> Update(GlossaryUpdate glossaryUpdate);
    Task Delete(int id);
}
