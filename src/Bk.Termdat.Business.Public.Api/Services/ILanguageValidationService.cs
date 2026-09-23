using System.Threading.Tasks;

namespace Bk.Termdat.Business.Public.Api.Services;

public interface ILanguageValidationService
{
    Task<int> ValidateLanguageIsoCode(string languageIsoCode);
}
