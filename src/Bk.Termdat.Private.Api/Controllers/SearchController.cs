using System;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ILogger<SearchController> logger;
    private readonly ISearchQueryService searchQueryService;
    private readonly IDuplicateCheckService duplicateCheckService;
    private const string ExcelMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public SearchController(ISearchQueryService searchQueryService, IDuplicateCheckService duplicateCheckService, ILogger<SearchController> logger)
    {
        this.logger = logger;
        this.searchQueryService = searchQueryService;
        this.duplicateCheckService = duplicateCheckService;
    }

    [HttpGet("search")]
    public async Task<ActionResult> Search([FromQuery] SearchQuery searchQuery)
    {
        if (searchQuery is null)
        {
            return BadRequest();
        }

        try
        {
            var result = await searchQueryService.Search(searchQuery);
            return Ok(result);
        }
        catch (AutoMapperMappingException ex)
        {
            logger.LogError(ex, "Error while mapping search query, maybe invalid EntryIds were searched: '{searchQuery}'", searchQuery);
            return BadRequest();
        }
    }

    [HttpGet("recommend")]
    public async Task<ActionResult> Recommend([FromQuery] RecommendQuery recommendQuery)
    {
        if (recommendQuery is null)
        {
            return BadRequest();
        }

        try
        {
            var result = await searchQueryService.Recommend(recommendQuery.Phrase, recommendQuery.SourceLanguageIds);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while recommending '{Phrase}'", recommendQuery.Phrase);
            return Ok(Array.Empty<string>());
        }
    }

    [HttpPost("duplicate-check")]
    public async Task<ActionResult> GetDuplicateEntryIds([FromBody] DuplicateCheckRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var duplicateCheckResponse = await duplicateCheckService.GetDuplicateEntryIds(request);
        return Ok(duplicateCheckResponse);
    }

    [HttpPost("duplicate-check/xlsx")]
    public async Task<ActionResult> ExportDuplicateCheckXlsx([FromBody] DuplicateCheckRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var (fileName, content) = await duplicateCheckService.ExportDuplicateCheckXlsx(request);

        if (content is null)
        {
            return NoContent();
        }

        return File(content, ExcelMimeType, fileName);
    }
}
