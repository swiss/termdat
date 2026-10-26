using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/entry-templates")]
public class EntryTemplatesController : ControllerBase
{
    private readonly IEntryTemplateService entryTemplateService;

    public EntryTemplatesController(IEntryTemplateService entryTemplateService)
    {
        this.entryTemplateService = entryTemplateService;
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> GetTemplateById([FromRoute] int id)
    {
        var result = await entryTemplateService.GetTemplateById(id);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> GetForUser()
    {
        var result = await entryTemplateService.GetForUser();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> CreateTemplate([FromBody] EntryTemplateCreate entryTemplateDetail)
    {
        if (entryTemplateDetail is null)
        {
            return BadRequest();
        }

        await entryTemplateService.Create(entryTemplateDetail);

        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] EntryTemplateUpdate entryTemplateUpdate)
    {
        if (entryTemplateUpdate is null || entryTemplateUpdate.Id != id)
        {
            return BadRequest();
        }

        await entryTemplateService.Update(entryTemplateUpdate);

        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await entryTemplateService.Delete(id);

        return Ok();
    }
}
