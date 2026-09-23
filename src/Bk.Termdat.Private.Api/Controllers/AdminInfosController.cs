using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/admin-infos")]
public class AdminInfosController : ControllerBase
{
    private readonly IAdminInfoService adminInfoService;

    public AdminInfosController(IAdminInfoService adminInfoService)
    {
        this.adminInfoService = adminInfoService;
    }

    [HttpGet("")]
    [Authorize(Policy = Policy.RequireAdminOrWriterOrSuperUserOrSuperReaderPolicy)]
    public async Task<ActionResult> GetAll()
    {
        var adminInfos = await adminInfoService.GetAll();
        return Ok(adminInfos);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetDetails([FromRoute] int id)
    {
        var result = await adminInfoService.GetDetails(id);
        return Ok(result);
    }

    [HttpGet("details")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetAllDetails()
    {
        var result = await adminInfoService.GetAllDetails();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Create([FromBody] AdminInfoCreate adminInfoCreate)
    {
        if (adminInfoCreate is null)
        {
            return BadRequest();
        }

        var createdAdminInfo = await adminInfoService.Create(adminInfoCreate);
        return Ok(createdAdminInfo);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] AdminInfoUpdate adminInfoUpdate)
    {
        if (adminInfoUpdate is null || adminInfoUpdate.Id != id)
        {
            return BadRequest();
        }

        var updatedEntry = await adminInfoService.Update(adminInfoUpdate);

        return Ok(updatedEntry);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await adminInfoService.Delete(id);

        return Ok();
    }
}
