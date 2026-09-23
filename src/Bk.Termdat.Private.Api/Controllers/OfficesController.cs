using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/offices")]
public class OfficesController : ControllerBase
{
    private readonly IOfficeService officeService;

    public OfficesController(IOfficeService officeService)
    {
        this.officeService = officeService;
    }

    [HttpGet("")]
    public async Task<ActionResult> GetAll()
    {
        var result = await officeService.GetAll();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetDetails([FromRoute] int id)
    {
        var result = await officeService.GetDetails(id);
        return Ok(result);
    }

    [HttpGet("details")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetAllDetails()
    {
        var result = await officeService.GetAllDetails();
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] OfficeUpdate officeUpdate)
    {
        if (officeUpdate is null || officeUpdate.Id != id)
        {
            return BadRequest();
        }

        var updatedEntry = await officeService.Update(officeUpdate);

        return Ok(updatedEntry);
    }

    [HttpPost]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Create([FromBody] OfficeCreate officeCreate)
    {
        if (officeCreate is null)
        {
            return BadRequest();
        }

        var createdOffice = await officeService.Create(officeCreate);
        return Ok(createdOffice);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await officeService.Delete(id);

        return Ok();
    }
}
