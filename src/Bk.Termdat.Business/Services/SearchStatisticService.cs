using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Business.Services;

public class SearchStatisticService : ISearchStatisticService
{
    private readonly IStatisticService statisticService;
    private readonly ILanguageRepository languageRepository;
    private readonly PrivateOptions privateOptions;

    public SearchStatisticService(IStatisticService statisticService, ILanguageRepository languageRepository, IOptions<PrivateOptions> privateOptions)
    {
        ArgumentNullException.ThrowIfNull(privateOptions);

        this.privateOptions = privateOptions.Value;
        this.statisticService = statisticService;
        this.languageRepository = languageRepository;
    }

    public async Task<StatisticSummary> LogUnsuccessfulSearch(SearchFilterParameter searchFilterParameter, SearchFields searchFields)
    {
        ArgumentNullException.ThrowIfNull(searchFilterParameter);

        if (!CheckLogConditions(searchFilterParameter, searchFields))
        {
            return null;
        }
        var summary = await CreateSummaryFromSearchParameters(searchFilterParameter, searchFields);
        await statisticService.CreateOrUpdateStatistic(summary);
        return summary;
    }

    private bool CheckLogConditions(SearchFilterParameter searchFilterParameter, SearchFields searchFields)
    {
        return CheckFilterParameter(searchFilterParameter) && CheckSearchFields(searchFields);
    }

    private bool CheckFilterParameter(SearchFilterParameter searchFilterParameter)
    {
        return !(searchFilterParameter.AdminInfos.Length > 0 ||
            searchFilterParameter.Collections.Length > 0 ||
            searchFilterParameter.CommentStatus.Length > 0 ||
            searchFilterParameter.CreatedBy != null ||
            searchFilterParameter.Glossaries.Length > 0 ||
            searchFilterParameter.LenochClassifications.Length > 0 ||
            searchFilterParameter.ModifiedBy != null ||
            searchFilterParameter.ModifiedFrom.HasValue ||
            searchFilterParameter.ModifiedTo.HasValue ||
            searchFilterParameter.Offices.Length != 1 || (!searchFilterParameter.Offices.Any(o => o == privateOptions.AchOfficeId)) ||
            !searchFilterParameter.OfficesPriority ||
            searchFilterParameter.RelatedCollections.Length > 0 ||
            searchFilterParameter.Status.Length != 1 || (searchFilterParameter.Status.Length == 1 && !searchFilterParameter.Status.Any(s => s == Status.Validated)) ||
            !searchFilterParameter.StatusPriority ||
            searchFilterParameter.Reliability.Length > 0 ||
            searchFilterParameter.SubjectDomains.Length > 0
        );
    }

    private static bool CheckSearchFields(SearchFields searchFields)
    {
        // in any of these fields have to be searched to return true
        return searchFields.Abbreviation || searchFields.Name || searchFields.Metadata ||
                    searchFields.Phraseology || searchFields.Terminus;
    }

    private async Task<StatisticSummary> CreateSummaryFromSearchParameters(SearchFilterParameter searchFilterParameter, SearchFields searchFields)
    {
        var summary = new StatisticSummary
        {
            SearchText = searchFilterParameter.Phrase,
            NumberOfSearches = 0,
            Languages = await CreateLanguageString(searchFilterParameter),
            SearchFields = CreateSearchFieldString(searchFields),
            DateLastSearch = DateTime.UtcNow
        };

        return summary;
    }

    private async Task<string> CreateLanguageString(SearchFilterParameter searchFilterParameter)
    {
        var languages = (await languageRepository.GetAll()).ToList();

        var languageStrings = languages.Where(l => searchFilterParameter.SourceLanguageIds.Contains(l.Id)).OrderBy(l => l.SortIndex).Select(l => l.IsoCode);

        return string.Join(", ", languageStrings);
    }

    private static string CreateSearchFieldString(SearchFields searchFields)
    {
        var sb = new StringBuilder();

        // (AB = Abkürzung, Na = Name, Md = Metadaten, PH = Phraseologie, VE = Terminus)

        if (searchFields.Abbreviation)
        {
            sb.Append("AB, ");
        }
        if (searchFields.Name)
        {
            sb.Append("Na, ");
        }
        if (searchFields.Metadata)
        {
            sb.Append("Md, ");
        }
        if (searchFields.Phraseology)
        {
            sb.Append("PH, ");
        }
        if (searchFields.Terminus)
        {
            sb.Append("VE, ");
        }

        var searchFieldsString = sb.ToString();

        if (searchFieldsString.Length > 0)
        {
            searchFieldsString = searchFieldsString.Substring(0, searchFieldsString.Length - 2);
        }

        return searchFieldsString;
    }
}

