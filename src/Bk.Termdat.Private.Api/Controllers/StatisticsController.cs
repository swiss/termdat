using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticService statisticService;

    public StatisticsController(IStatisticService statisticService)
    {
        this.statisticService = statisticService;
    }

    [HttpGet("")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetBySearchQuery([FromQuery] StatisticSearchQuery searchQuery)
    {
        var statistics = await statisticService.GetBySearchQuery(searchQuery);
        return Ok(statistics);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await statisticService.Delete(id);

        return Ok();
    }

    [HttpDelete]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Delete([FromQuery] StatisticSearchQuery searchQuery)
    {
        await statisticService.DeleteBySearchQuery(searchQuery);

        return Ok();
    }
}
