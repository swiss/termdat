using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Bk.Termdat.Infrastructure.Service;

public class SearchApiClient : ISearchApiClient
{
    private const string RequestHeaderAuthorization = "Authorization";
    private readonly ILogger<SearchApiClient> logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly SearchApiOptions searchApiOptions;

    public SearchApiClient(ILogger<SearchApiClient> logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<SearchApiOptions> searchApiOptions)
    {
        ArgumentNullException.ThrowIfNull(searchApiOptions);

        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
        this.httpContextAccessor = httpContextAccessor;
        this.searchApiOptions = searchApiOptions.Value;
    }

    public async Task<bool> IsAlive()
    {
        using var client = httpClientFactory.CreateClient();

        try
        {
            var builder = new UriBuilder(searchApiOptions.BaseUrl) { Path = "/api/systemstatus" };
            var response = await client.GetAsync(builder.ToString());
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get SearchService IsAlive status. Error={Message}", ex.Message);
            return false;
        }
    }

    public async Task<SearchResult> Search(SearchFilterParameter searchFilterParameter, SearchFields searchFields)
    {
        ArgumentNullException.ThrowIfNull(searchFilterParameter);
        ArgumentNullException.ThrowIfNull(searchFields);

        var query = CreateSearchQuery(searchFilterParameter, searchFields);

        var queryString = QueryHelpers.AddQueryString(searchApiOptions.UrnSearch, query.Where(c => c.Value != StringValues.Empty));
        return await GetFromJson<SearchResult>(queryString);
    }

    public async Task<IEnumerable<string>> Recommend(string searchPhrase, IEnumerable<int> languageIds = null)
    {
        var query = CreateRecommendQuery(searchPhrase, languageIds);

        var queryString = QueryHelpers.AddQueryString(searchApiOptions.UrnRecommend, query.Where(c => c.Value != StringValues.Empty));
        return await GetFromJson<IEnumerable<string>>(queryString);
    }

    private static Dictionary<string, StringValues> CreateRecommendQuery(string searchPhrase, IEnumerable<int> languageIds)
    {
        return new Dictionary<string, StringValues>
        {
            { nameof(searchPhrase), searchPhrase },
            { nameof(languageIds), CreateStringValue(languageIds)}
        };
    }

    private static Dictionary<string, StringValues> CreateSearchQuery(SearchFilterParameter searchFilterParameter, SearchFields searchFields)
    {
        return new Dictionary<string, StringValues>
        {
            { nameof(SearchFilterParameter.Phrase), searchFilterParameter.Phrase },
            {
                nameof(SearchFilterParameter.SourceLanguageIds),
                CreateStringValue(searchFilterParameter.SourceLanguageIds)
            },
            { nameof(SearchFilterParameter.Offices), CreateStringValue(searchFilterParameter.Offices) },
            {
                nameof(SearchFilterParameter.OfficesPriority),
                CreateStringValue(searchFilterParameter.OfficesPriority)
            },
            { nameof(SearchFilterParameter.LenochClassifications), CreateStringValue(searchFilterParameter.LenochClassifications) },
            {
                nameof(SearchFilterParameter.LenochClassificationPriority),
                CreateStringValue(searchFilterParameter.LenochClassificationPriority)
            },
            { nameof(SearchFilterParameter.Collections), CreateStringValue(searchFilterParameter.Collections) },
            {
                nameof(SearchFilterParameter.CollectionsPriority),
                CreateStringValue(searchFilterParameter.CollectionsPriority)
            },
            { nameof(SearchFilterParameter.CollectionsExclude), CreateStringValue(searchFilterParameter.CollectionsExclude) },
            { nameof(SearchFilterParameter.SubjectDomains), searchFilterParameter.SubjectDomains },
            {
                nameof(SearchFilterParameter.SubjectDomainsPriority),
                CreateStringValue(searchFilterParameter.SubjectDomainsPriority)
            },
            { nameof(SearchFilterParameter.RelatedCollections), CreateStringValue(searchFilterParameter.RelatedCollections) },
            { nameof(SearchFilterParameter.Glossaries), CreateStringValue(searchFilterParameter.Glossaries) },
            { nameof(SearchFilterParameter.GlossariesExclude), CreateStringValue(searchFilterParameter.GlossariesExclude) },
            { nameof(SearchFilterParameter.Status), CreateStringValue(searchFilterParameter.Status) },
            {
                nameof(SearchFilterParameter.StatusPriority),
                CreateStringValue(searchFilterParameter.StatusPriority)
            },
            { nameof(SearchFilterParameter.AdminInfos), CreateStringValue(searchFilterParameter.AdminInfos) },
            {
                nameof(SearchFilterParameter.AdminInfosPriority),
                CreateStringValue(searchFilterParameter.AdminInfosPriority)
            },
            { nameof(SearchFilterParameter.AdminInfosExclude), CreateStringValue(searchFilterParameter.AdminInfosExclude) },
            { nameof(SearchFilterParameter.CommentStatus), CreateStringValue(searchFilterParameter.CommentStatus) },
            { nameof(SearchFilterParameter.CreatedBy), searchFilterParameter.CreatedBy },
            {
                nameof(SearchFilterParameter.CreatedByPriority),
                CreateStringValue(searchFilterParameter.CreatedByPriority)
            },
            { nameof(SearchFilterParameter.CreatedByExclude), CreateStringValue(searchFilterParameter.CreatedByExclude) },
            { nameof(SearchFilterParameter.ModifiedBy), searchFilterParameter.ModifiedBy },
            {
                nameof(SearchFilterParameter.ModifiedByPriority),
                CreateStringValue(searchFilterParameter.ModifiedByPriority)
            },
            { nameof(SearchFilterParameter.ModifiedByExclude), CreateStringValue(searchFilterParameter.ModifiedByExclude) },
            { nameof(SearchFilterParameter.ModifiedFrom), CreateStringValue(searchFilterParameter.ModifiedFrom) },
            { nameof(SearchFilterParameter.ModifiedTo), CreateStringValue(searchFilterParameter.ModifiedTo) },
            {
                nameof(SearchFilterParameter.ModifiedPeriodPriority),
                CreateStringValue(searchFilterParameter.ModifiedPeriodPriority)
            },
            { nameof(SearchFilterParameter.ModifiedPeriodExclude), CreateStringValue(searchFilterParameter.ModifiedPeriodExclude) },
            { nameof(SearchFilterParameter.Reliability), CreateStringValue(searchFilterParameter.Reliability) },
            {
                nameof(SearchFilterParameter.ReliabilityPriority),
                CreateStringValue(searchFilterParameter.ReliabilityPriority)
            },
            { nameof(SearchFilterParameter.Deleted), CreateStringValue(searchFilterParameter.Deleted) },
            {
                nameof(SearchFilterParameter.DeletedPriority),
                CreateStringValue(searchFilterParameter.DeletedPriority)
            },
            { nameof(SearchFilterParameter.HasMissingSource), CreateStringValue(searchFilterParameter.HasMissingSource) },
            { nameof(SearchFilterParameter.EntryIds), CreateStringValue(searchFilterParameter.EntryIds) },
            { nameof(SearchFilterParameter.PageIndex), CreateStringValue(searchFilterParameter.PageIndex) },
            { nameof(SearchFilterParameter.PageSize), CreateStringValue(searchFilterParameter.PageSize) },
            { nameof(SearchFields.Terminus), CreateStringValue(searchFields.Terminus) },
            { nameof(SearchFields.Name), CreateStringValue(searchFields.Name) },
            { nameof(SearchFields.Abbreviation), CreateStringValue(searchFields.Abbreviation) },
            { nameof(SearchFields.Phraseology), CreateStringValue(searchFields.Phraseology) },
            { nameof(SearchFields.Definition), CreateStringValue(searchFields.Definition) },
            { nameof(SearchFields.Note), CreateStringValue(searchFields.Note) },
            { nameof(SearchFields.Context), CreateStringValue(searchFields.Context) },
            { nameof(SearchFields.Source), CreateStringValue(searchFields.Source) },
            { nameof(SearchFields.Metadata), CreateStringValue(searchFields.Metadata) },
            { nameof(SearchFields.Country), CreateStringValue(searchFields.Country) },
            { nameof(SearchFields.Comment), CreateStringValue(searchFields.Comment) }
        };
    }

    private static StringValues CreateStringValue(IEnumerable<int> ids)
    {
        var arrayOfIds = ids != null ? ids as int[] ?? ids.ToArray() : Array.Empty<int>();
        if (arrayOfIds.Length == 0)
        {
            return StringValues.Empty;
        }

        return arrayOfIds.Select(CreateStringValue).ToArray();

    }

    private static StringValues CreateStringValue(Status[] status)
    {
        if (status == null || status.Length == 0)
        {
            return StringValues.Empty;
        }

        return status.Select(CreateStringValue).ToArray();
    }

    private static StringValues CreateStringValue(Reliability[] reliability)
    {
        if (reliability == null || reliability.Length == 0)
        {
            return StringValues.Empty;
        }

        return reliability.Select(CreateStringValue).ToArray();
    }

    private static StringValues CreateStringValue(CommentStatus[] commentStatus)
    {
        if (commentStatus == null || commentStatus.Length == 0)
        {
            return StringValues.Empty;
        }

        return commentStatus.Select(CreateStringValue).ToArray();
    }

    private static StringValues CreateStringValue(bool boolean)
    {
        return boolean.ToString(CultureInfo.InvariantCulture);
    }

    private static string CreateStringValue(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }

    private static string CreateStringValue(Status status)
    {
        return status.ToString("G");
    }

    private static string CreateStringValue(Reliability reliability)
    {
        return reliability.ToString("G");
    }

    private static string CreateStringValue(CommentStatus status)
    {
        return status.ToString("G");
    }

    private static string CreateStringValue(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return StringValues.Empty;
        }

        return dateTime.Value.ToString("O");
    }

    private async Task<T> GetFromJson<T>(string requestUri)
    {
        var client = httpClientFactory.CreateClient(SearchApiOptions.HttpClientName);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        if (httpContextAccessor.HttpContext != null && httpContextAccessor.HttpContext.Request.Headers.TryGetValue(RequestHeaderAuthorization, out var authorizationValue))
        {
            request.Headers.Add(RequestHeaderAuthorization, authorizationValue.ToArray());
        }

        try
        {
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new SearchException("Your query cannot be processed", ex);
            }

            throw new SearchApiException("Unexpected error on Search API.", ex);

        }
        catch (TaskCanceledException ex)
        {
            throw new SearchTimeoutException(
                "Search timed out, query is to complex or search is under heavy load.", ex);
        }
        catch (Exception ex) when (ex is JsonException or ArgumentNullException or NotSupportedException)
        {
            throw new SearchApiException("Unexpected error on Search API.", ex);
        }
    }
}
