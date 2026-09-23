using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAuthorizationService = Bk.Termdat.Business.Services.Interfaces.IAuthorizationService;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/collections")]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService collectionService;
    private readonly IAuthorizationService authorizationService;

    public CollectionsController(ICollectionService collectionService, IAuthorizationService authorizationService)
    {
        this.collectionService = collectionService;
        this.authorizationService = authorizationService;
    }

    [HttpGet("")]
    public async Task<ActionResult> GetAll()
    {
        IEnumerable<CollectionSummary> result;
        if (authorizationService.IsAdmin())
        {
            result = await collectionService.GetAll();
        }
        else
        {
            if (authorizationService.IsWriter() || authorizationService.IsSuperReader())
            {
                result = await collectionService.GetAllForCurrentUser();
            }
            else
            {
                result = await collectionService.GetAllActive();
            }
        }

        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<ActionResult> GetAllActive()
    {
        var result = await collectionService.GetAllActive();
        return Ok(result);
    }

    [HttpGet("modification")]
    public async Task<ActionResult> GetCollectionsForModification()
    {
        var result = await collectionService.GetAllForModification();
        return Ok(result);
    }

    [HttpGet("details")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetAllDetails()
    {
        var result = await collectionService.GetAllDetails();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetDetail([FromRoute] int id)
    {
        var result = await collectionService.GetDetail(id);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] CollectionUpdate collectionUpdate)
    {
        if (collectionUpdate is null)
        {
            return BadRequest();
        }
        if (collectionUpdate.Id != id)
        {
            return BadRequest();
        }

        var updatedEntry = await collectionService.Update(collectionUpdate);

        return Ok(updatedEntry);
    }

    [HttpPost]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Create([FromBody] CollectionCreate collectionCreate)
    {
        if (collectionCreate is null)
        {
            return BadRequest();
        }

        var createdEntry = await collectionService.Create(collectionCreate);

        return Ok(createdEntry);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id, [FromQuery] bool soft = true)
    {
        await collectionService.Delete(id, soft);

        return Ok();
    }

    [HttpPost("restore")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Restore([FromBody] int id)
    {
        await collectionService.Restore(id);

        var collection = await collectionService.GetById(id);
        return Ok(collection);
    }
}
