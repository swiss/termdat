using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/news")]
public class NewsController : ControllerBase
{
    private readonly INewsService newsService;

    public NewsController(INewsService newsService)
    {
        this.newsService = newsService;
    }

    [HttpGet("")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> GetAll()
    {
        var result = await newsService.GetAll();
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<ActionResult> GetAllActive()
    {
        var result = await newsService.GetAllActive();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> GetDetail([FromRoute] int id)
    {
        var result = await newsService.GetDetails(id);
        return Ok(result);
    }

    [HttpPut("{id:int}/move-up")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> MoveUp([FromRoute] int id)
    {
        await newsService.MoveItemUp(id);

        return Ok();
    }

    [HttpPut("{id:int}/move-down")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> MoveDown([FromRoute] int id)
    {
        await newsService.MoveItemDown(id);

        return Ok();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] NewsItemUpdate newsItemUpdate)
    {
        if (newsItemUpdate is null)
        {
            return BadRequest();
        }
        if (newsItemUpdate.Id != id)
        {
            return BadRequest();
        }

        var updatedEntry = await newsService.Update(newsItemUpdate);

        return Ok(updatedEntry);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> Create([FromBody] NewsItemCreate newsItemCreate)
    {
        if (newsItemCreate is null)
        {
            return BadRequest();
        }

        var createdEntry = await newsService.Create(newsItemCreate);

        return Ok(createdEntry);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await newsService.Delete(id);

        return Ok();
    }
}
