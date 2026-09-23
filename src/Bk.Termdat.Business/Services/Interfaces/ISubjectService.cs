using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface ISubjectService
{
    Task<IEnumerable<SubjectDetail>> GetAllDetails();
    Task<IEnumerable<SubjectSummary>> GetAll();
    Task<SubjectModification> GetDetail(int id);
    Task<SubjectDetail> Create(SubjectModification subjectCreate);
    Task<SubjectDetail> Update(SubjectModification subjectUpdate);
    Task Delete(int id);
}
