using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Repositories;

public interface IGlossaryRepository
{
    Task<IEnumerable<Glossary>> GetAll();
    Task<IEnumerable<Glossary>> GetAuthorizedByIds(IEnumerable<int> authorizedGlossaryIds);
    Task<IEnumerable<GlossaryWithEntryCount>> GetAllDetails();
    Task<IEnumerable<Glossary>> GetByIds(IEnumerable<int> glossaryIds);
    Task<Glossary> GetById(int id);
    Task Create(Glossary glossary);
    void Update(Glossary existing, Glossary update);
    void Delete(Glossary glossary);
}
