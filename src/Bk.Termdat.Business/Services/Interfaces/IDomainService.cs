using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IDomainService
{
    Task<IEnumerable<ILanguageDescription>> GetAllDescriptions();
    Task<DomainDetail> Create(DomainModification domainCreate);
    Task<DomainDetail> Update(DomainModification domainUpdate);
    Task Delete(int id);
}
