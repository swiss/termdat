using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/source-templates")]
public class SourceTemplatesController : ControllerBase
{
    private readonly ISourceTemplateService sourceTemplateService;

    public SourceTemplatesController(ISourceTemplateService sourceTemplatesService)
    {
        sourceTemplateService = sourceTemplatesService;
    }

    [HttpGet("")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserPolicy)]
    [ProducesResponseType(typeof(SourceTemplateDefaults), StatusCodes.Status200OK)]
    public ActionResult GetAllDefaults()
    {
        var result = sourceTemplateService.GetAllDefaults();
        return Ok(result);
    }

    [HttpGet("current-user")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> GetAllForCurrentUser()
    {
        var sourceTemplates = await sourceTemplateService.GetAllForCurrentUser();
        return Ok(sourceTemplates);
    }

    [HttpGet("current-user/texts/{isoCode}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> GetAllForCurrentUserAsText([FromRoute] string isoCode)
    {
        var sourceTemplates = await sourceTemplateService.GetAllForCurrentUserAsText(isoCode);
        return Ok(sourceTemplates);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] SourceTemplateModification sourceTemplateUpdate)
    {
        await sourceTemplateService.Update(id, sourceTemplateUpdate);
        return Ok();
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> Create([FromBody] SourceTemplateModification sourceTemplateCreate)
    {
        await sourceTemplateService.Create(sourceTemplateCreate);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await sourceTemplateService.Delete(id);

        return Ok();
    }
}
