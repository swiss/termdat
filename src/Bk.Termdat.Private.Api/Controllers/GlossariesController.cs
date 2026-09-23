using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/glossaries")]
public class GlossariesController : ControllerBase
{
    private readonly IGlossaryService glossaryService;

    public GlossariesController(IGlossaryService glossaryService)
    {
        this.glossaryService = glossaryService;
    }

    [HttpGet("")]
    public async Task<ActionResult> GetAll()
    {
        var result = await glossaryService.GetAll();
        return Ok(result);
    }

    [HttpGet("modification")]
    public async Task<ActionResult> GetGlossariesForModification()
    {
        var result = await glossaryService.GetAllForModification();
        return Ok(result);
    }

    [HttpGet("details")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetAllDetails()
    {
        var result = await glossaryService.GetAllDetails();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetDetail([FromRoute] int id)
    {
        var result = await glossaryService.GetDetail(id);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] GlossaryUpdate glossaryUpdate)
    {
        if (glossaryUpdate is null)
        {
            return BadRequest();
        }
        if (glossaryUpdate.Id != id)
        {
            return BadRequest();
        }

        var updatedEntry = await glossaryService.Update(glossaryUpdate);

        return Ok(updatedEntry);
    }

    [HttpPost]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Create([FromBody] GlossaryCreate glossaryCreate)
    {
        if (glossaryCreate is null)
        {
            return BadRequest();
        }

        var createdEntry = await glossaryService.Create(glossaryCreate);

        return Ok(createdEntry);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await glossaryService.Delete(id);

        return Ok();
    }
}
