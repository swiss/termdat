using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;

namespace Bk.Termdat.Business.Repositories;

public interface ISubjectRepository
{
    Task<IEnumerable<Subject>> GetAll();
    Task<IEnumerable<SubjectWithEntryCount>> GetAllDetails();
    Task<IEnumerable<Subject>> GetByIds(IEnumerable<int> subjectIds);
    Task<Subject> GetById(int id);
    Task Create(Subject subject);
    void Update(Subject existing, Subject update);
    void Delete(Subject subject);
    Task<int> GetEntryCountById(int id);
}
