using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Public.Api.Examples;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Bk.Termdat.Public.Api.Controllers;

/// <summary>
/// Provides access to entries.
/// </summary>
[ApiVersion("2.0")]
[ApiController]
[Route("v{version:apiVersion}/entry")]
public class EntryController : ControllerBase
{
    private readonly IEntryService entryService;
    private readonly IAnalyticsService analyticsService;

    public EntryController(IEntryService entryService, IAnalyticsService analyticsService)
    {
        this.entryService = entryService;
        this.analyticsService = analyticsService;
    }

    /// <summary>
    /// Returns a list of entries to the given entry id's.
    /// </summary>
    /// <param name="parameter">parameters</param>
    /// <returns>Entry details matching the provided entry id's.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EntryDetail>), 200)]
    [SwaggerResponseExample(200, typeof(GetEntriesExamples))]
    [ProducesResponseType(404)]
    [SwaggerResponseExample(404, typeof(NotFoundExample))]
    [ProducesResponseType(400)]
    [SwaggerResponseExample(400, typeof(BadRequestExample))]
    [ProducesResponseType(500)]
    [SwaggerResponseExample(500, typeof(InternalServerErrorExample))]
    public async Task<IActionResult> GetEntries([FromQuery] GetEntriesParameter parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        await analyticsService.TrackEvent("Public Search", "Perform");

        var entryDetails = await entryService.GetByIds(parameter.EntryIds, parameter.InLanguageCode, parameter.OutLanguageCode);

        return Ok(entryDetails);
    }
}
