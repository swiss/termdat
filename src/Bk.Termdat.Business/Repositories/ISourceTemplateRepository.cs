using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Repositories;

public interface ISourceTemplateRepository
{
    Task<SourceTemplate> GetById(int id);
    Task<IEnumerable<SourceTemplate>> GetAll(int userId);
    Task Create(SourceTemplate sourceTemplate);
    void Update(SourceTemplate existing, SourceTemplate update);
    void Delete(SourceTemplate sourceTemplate);
}
