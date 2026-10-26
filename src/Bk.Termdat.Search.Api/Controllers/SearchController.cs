using System;
using System.Collections.Generic;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Search.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchRepository searchRepository;
    private readonly IAuthorizationService authorizationService;
    private readonly ILogger<SearchController> logger;

    public SearchController(ISearchRepository searchRepository, IAuthorizationService authorizationService, ILogger<SearchController> logger)
    {
        this.searchRepository = searchRepository;
        this.authorizationService = authorizationService;
        this.logger = logger;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchResult), StatusCodes.Status200OK)]
    public ActionResult Search([FromQuery] SearchFilterParameter searchQuery, [FromQuery] SearchFields searchFields)
    {
        if (searchQuery is null)
        {
            return BadRequest();
        }

        var isAdmin = authorizationService.IsAdmin();
        var isWriter = authorizationService.IsWriter();
        var isSuperReader = authorizationService.IsSuperReader();
        var result = searchRepository.Search(searchQuery, searchFields, includeDeleted: isAdmin, includeDeactivatedCollectionEntries: isAdmin || isWriter || isSuperReader);
        return Ok(result);
    }

    [HttpGet("recommend")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public ActionResult Recommend([FromQuery] string searchPhrase, [FromQuery] IEnumerable<int> languageIds = null)
    {
        if (searchPhrase is null)
        {
            return BadRequest();
        }

        try
        {
            var isAdmin = authorizationService.IsAdmin();
            var result = searchRepository.Recommend(searchPhrase, languageIds, includeDeleted: isAdmin, includeDeactivatedCollectionEntries: isAdmin);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while recommending '{SearchPhrase}'.", searchPhrase);
            return Ok(Array.Empty<string>());
        }
    }
}
