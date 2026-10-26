using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/subjects")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        this.subjectService = subjectService;
    }

    [HttpGet("")]
    public async Task<ActionResult> GetAll()
    {
        var result = await subjectService.GetAll();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> GetDetail([FromRoute] int id)
    {
        var result = await subjectService.GetDetail(id);
        return Ok(result);
    }

    [HttpGet("details")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> GetAllDetails()
    {
        var subjects = await subjectService.GetAllDetails();
        return Ok(subjects);
    }

    [HttpPost("")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> Create([FromBody] SubjectModification subjectCreate)
    {
        if (subjectCreate is null)
        {
            return BadRequest();
        }

        var subject = await subjectService.Create(subjectCreate);
        return Ok(subject);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] SubjectModification subjectUpdate)
    {
        if (subjectUpdate?.Id != id)
        {
            return BadRequest();
        }

        var subject = await subjectService.Update(subjectUpdate);

        return Ok(subject);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await subjectService.Delete(id);
        return Ok();
    }
}
