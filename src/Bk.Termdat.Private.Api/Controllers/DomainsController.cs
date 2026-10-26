using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[Route("api/domains")]
[ApiController]
public class DomainsController : ControllerBase
{
    private readonly IDomainService domainService;

    public DomainsController(IDomainService domainService)
    {
        this.domainService = domainService;
    }

    [HttpGet("descriptions")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> GetAllDescriptions()
    {
        var descriptions = await domainService.GetAllDescriptions();
        return Ok(descriptions);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] DomainModification domainUpdate)
    {
        if (domainUpdate?.Id != id)
        {
            return BadRequest();
        }

        var domain = await domainService.Update(domainUpdate);

        return Ok(domain);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await domainService.Delete(id);
        return Ok();
    }
}
