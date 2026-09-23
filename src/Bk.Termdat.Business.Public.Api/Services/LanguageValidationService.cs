using System;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.CrossCutting.Exceptions;

namespace Bk.Termdat.Business.Public.Api.Services;

public class LanguageValidationService : ILanguageValidationService
{
    private readonly ILanguageRepository languageRepository;

    public LanguageValidationService(ILanguageRepository languageRepository)
    {
        this.languageRepository = languageRepository;
    }

    public async Task<int> ValidateLanguageIsoCode(string languageIsoCode)
    {
        var languages = await languageRepository.GetAllForValidation();
        var language = languages.SingleOrDefault(l => string.Equals(l.IsoCode, languageIsoCode, StringComparison.InvariantCultureIgnoreCase)) ??
            throw new BusinessValidationException(string.Empty, $"The provided language '{languageIsoCode}' is not supported.");

        return language.Id;
    }

}
