using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Public.Api.Examples;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Bk.Termdat.Public.Api.Controllers;

/// <summary>
/// Provides access to the search functionality.
/// </summary>
[ApiVersion("2.0")]
[ApiController]
[Route("v{version:apiVersion}/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService searchService;
    private readonly IAnalyticsService analyticsService;

    public SearchController(ISearchService searchService, IAnalyticsService analyticsService)
    {
        this.searchService = searchService;
        this.analyticsService = analyticsService;
    }

    /// <summary>
    /// Returns a list of entries that match the given search parameters
    /// </summary>
    /// <param name="parameter">collection of search parameters</param>
    /// <returns>List of entries matching the search parameters ordered by relevance</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EntryDetail>), 200)]
    [SwaggerResponseExample(200, typeof(SearchExamples))]
    [ProducesResponseType(400)]
    [SwaggerResponseExample(400, typeof(BadRequestExample))]
    [ProducesResponseType(500)]
    [SwaggerResponseExample(500, typeof(InternalServerErrorExample))]
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchParameter parameter)
    {
        await analyticsService.TrackEvent("Public Search", "Perform", parameter.SearchTerm);
        var searchResults = await searchService.Search(parameter);
        return Ok(searchResults);
    }
}
