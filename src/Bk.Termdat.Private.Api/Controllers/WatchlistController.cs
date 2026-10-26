using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Private.Api.Controllers;

#pragma warning disable S6960 // SMELL: Controller has multiple responsibilities

[ApiController]
[Route("api/watchlist")]
public class WatchlistController : ControllerBase
{
    private readonly IWatchlistService watchlistService;
    private readonly IExcelDataService excelDataService;
    private readonly ILogger<WatchlistController> logger;
    private const string ExcelMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public WatchlistController(IWatchlistService watchlistService, IExcelDataService excelDataService, ILogger<WatchlistController> logger)
    {
        this.watchlistService = watchlistService;
        this.excelDataService = excelDataService;
        this.logger = logger;
    }

    [HttpPost("")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserOrSuperReaderPolicy)]
    public async Task<ActionResult> GetEntries([FromBody] WatchlistQuery query)
    {
        if (query?.EntryIds is null)
        {
            logger.LogWarning("{Name} cannot be null", nameof(query.EntryIds));
            return BadRequest();
        }

        var ids = query.EntryIds.ToList();

        var entries = await watchlistService.GetEntries(ids);

        return Ok(entries);
    }

    [HttpPost("export:pdf")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserOrSuperReaderPolicy)]
    public async Task<ActionResult> ExportPdf([FromBody] WatchlistQuery query)
    {
        if (query?.EntryIds is null)
        {
            logger.LogWarning("query.EntryIds cannot be null");
            return BadRequest();
        }

        if (!query.EntryIds.Any())
        {
            return BadRequest();
        }

        var stream = await watchlistService.CreateExportPdf(query.EntryIds);
        return File(stream, MediaTypeNames.Application.Pdf, $"TERMDAT_Export_{DateTime.Now:yyyyMMdd-HHmm}.pdf");
    }

    [HttpPost("export:xlsx")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterPolicy)]
    public async Task<ActionResult> ExportXlsx([FromBody] WatchlistQuery query)
    {
        if (query?.EntryIds is null || !query.EntryIds.Any())
        {
            logger.LogWarning("query.EntryIds cannot be null or empty");
            return BadRequest();
        }

        var (fileName, content) = await excelDataService.CreateExport(query.EntryIds.ToList());

        return File(content, ExcelMimeType, fileName);
    }
}
#pragma warning restore S6960
