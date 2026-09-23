using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Repositories;

public interface IEntryTemplateRepository
{
    Task<IEnumerable<EntryTemplate>> GetForUser(int userId);
    Task<EntryTemplate> GetById(int id);
    Task<bool> TemplateNameExistsForUser(int userId, string templateName);
    Task Create(EntryTemplate entryTemplate);
    void Update(EntryTemplate entryTemplate);
    void Delete(EntryTemplate entryTemplate);
}
