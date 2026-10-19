using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[Route("api/concept-systems")]
[ApiController]
public class ConceptSystemsController : ControllerBase
{
    private readonly IConceptSystemService conceptSystemService;

    public ConceptSystemsController(IConceptSystemService conceptSystemService)
    {
        this.conceptSystemService = conceptSystemService;
    }

    [HttpGet("")]
    public async Task<ActionResult> GetAll()
    {
        var result = await conceptSystemService.GetAll();
        return Ok(result);
    }

    [HttpGet("details")]
    [Authorize(Policy = Policy.RequireAdminOrWriterOrSuperUserOrSuperReaderPolicy)]
    public async Task<ActionResult> GetAllDetails()
    {
        var result = await conceptSystemService.GetAllDetails();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetDetail([FromRoute] int id)
    {
        var result = await conceptSystemService.GetDetail(id);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] ConceptSystemUpdate conceptSystemUpdate)
    {
        if (conceptSystemUpdate is null)
        {
            return BadRequest();
        }
        if (conceptSystemUpdate.Id != id)
        {
            return BadRequest();
        }

        var updatedConceptSystem = await conceptSystemService.Update(conceptSystemUpdate);

        return Ok(updatedConceptSystem);
    }

    [HttpPost]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Create([FromBody] ConceptSystemCreate conceptSystemCreate)
    {
        if (conceptSystemCreate is null)
        {
            return BadRequest();
        }

        var createdEntry = await conceptSystemService.Create(conceptSystemCreate);

        return Ok(createdEntry);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await conceptSystemService.Delete(id);

        return Ok();
    }
}
