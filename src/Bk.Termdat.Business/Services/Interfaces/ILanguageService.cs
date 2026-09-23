using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface ILanguageService
{
    Task<IEnumerable<LanguageSummary>> GetTranslationByIsoCode(string isoCode);
    Task<IEnumerable<LanguageDetail>> GetAllDetails();
    Task<LanguageDetail> GetById(int id);
    Task<LanguageUpdate> GetDetail(int id);
    int GetNextSort();
    Task<LanguageDetail> Create(LanguageCreate languageCreate);
    Task<LanguageDetail> Update(LanguageUpdate languageUpdate);
}
