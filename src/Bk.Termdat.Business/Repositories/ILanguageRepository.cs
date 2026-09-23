using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Repositories;

public interface ILanguageRepository
{
    Task<IEnumerable<LanguageTranslation>> GetTranslationByIsoCode(string isoCode);
    Task<IEnumerable<Language>> GetAll();
    Task<IEnumerable<Language>> GetAllDetails();
    Task<Language> GetById(int id);
    Task Create(Language language);
    void Update(Language existing, Language update);
    int GetNextSort();
    Task<IEnumerable<Language>> GetAllForValidation();
}
