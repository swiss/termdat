using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/entries")]
public class EntriesController : ControllerBase
{
    private readonly IEntryHeaderUpdateService entryHeaderUpdateService;
    private readonly IEntryService entryService;
    private readonly ILogger<EntriesController> logger;

    public EntriesController(IEntryService entryService, IEntryHeaderUpdateService entryHeaderUpdateService,
        ILogger<EntriesController> logger)
    {
        this.entryService = entryService;
        this.entryHeaderUpdateService = entryHeaderUpdateService;
        this.logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetDetail([FromRoute] int id, [FromQuery] GetDetailQuery getDetailQuery)
    {
        if (getDetailQuery is null)
        {
            return BadRequest();
        }

        var result =
            await entryService.GetDetail(id, getDetailQuery.SourceLanguageIds, getDetailQuery.TargetLanguageIds);

        return Ok(result);
    }

    [HttpGet("{id}/modification")]
    public async Task<ActionResult> GetModificationDetail([FromRoute] int id)
    {
        try
        {
            var result = await entryService.GetModificationDetail(id);

            return Ok(result);
        }
        catch (EntryLockedException)
        {
            logger.LogWarning("Entry with id='{Id}' is locked and cannot be accessed using /modification", id);

            return BadRequest();
        }
    }

    [HttpPut("{id}/lock")]
    [Authorize(Policy = Policy.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> Lock([FromRoute] int id)
    {
        var concurrencyToken = await entryService.Lock(id);

        return Ok(new LockResult { IsLocked = true, ConcurrencyToken = concurrencyToken });
    }

    [HttpPut("{id}/unlock")]
    [Authorize(Policy = Policy.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> Unlock([FromRoute] int id)
    {
        var concurrencyToken = await entryService.Unlock(id);

        return Ok(new LockResult { IsLocked = false, ConcurrencyToken = concurrencyToken });
    }

    [HttpPost]
    [Authorize(Policy = Policy.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> Create([FromBody] EntryCreate entryCreate)
    {
        if (entryCreate is null)
        {
            return BadRequest();
        }

        try
        {
            var createdEntry = await entryService.Create(entryCreate);
            return CreatedAtAction(nameof(GetDetail), new { id = createdEntry.Id }, createdEntry);
        }
        catch (ForbiddenException)
        {
            // Note: Handle subItem-level permissions as BadRequest (e.g. glossaries), because the whole create process itself is not forbidden.
            return BadRequest();
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policy.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromForm] EntryUpdate entryUpdate)
    {
        if (entryUpdate is null)
        {
            return BadRequest();
        }

        if (entryUpdate.Id != id)
        {
            return BadRequest();
        }

        try
        {
            var updatedEntry = await entryService.Update(entryUpdate);
            return Ok(updatedEntry);
        }
        catch (ForbiddenException)
        {
            // Note: Handle subItem-level permissions as BadRequest (e.g. glossaries), because the whole update process itself is not forbidden.
            return BadRequest();
        }
    }

    [HttpPut("multiple-search-and-replace")]
    [Authorize(Policy = Policy.RequireAdminOrWriterPolicy)]
    public async Task<ActionResult> MultipleSearchAndReplace([FromForm] EntrySearchAndReplaceUpdate dto)
    {
        if (dto?.Ids is null || !dto.Ids.Any() || dto?.LanguageIds is null || !dto.LanguageIds.Any() ||
            string.IsNullOrWhiteSpace(dto.SearchField) || string.IsNullOrWhiteSpace(dto.SearchValue))
        {
            return BadRequest();
        }

        var affectedRows = await entryService.MultipleSearchAndReplace(dto);

        return Ok(affectedRows);
    }

    [HttpPut("update-mandatory-header-fields")]
    [Authorize(Policy = Policy.RequireAdminOrWriterPolicy)]
    public async Task<ActionResult> UpdateMandatoryHeaderFields([FromForm] EntryHeaderUpdate entryHeaderUpdate)
    {
        if (entryHeaderUpdate?.Ids is null || entryHeaderUpdate.Ids.Count == 0)
        {
            return BadRequest();
        }

        var lockInfos = await entryHeaderUpdateService.UpdateMandatoryHeaderFields(entryHeaderUpdate);

        return Ok(lockInfos);
    }

    [HttpPut("update-multiple-admin-infos")]
    [Authorize(Policy = Policy.RequireAdminOrWriterPolicy)]
    public async Task<ActionResult> UpdateMultipleAdminInfos([FromBody] EntryChipFieldInputUpdate dto)
    {
        if (dto?.Ids is null || !dto.Ids.Any())
        {
            return BadRequest();
        }

        var lockInfos = await entryHeaderUpdateService.UpdateAdminInfos(dto);

        return Ok(lockInfos);
    }

    [HttpPut("update-multiple-glossaries")]
    [Authorize(Policy = Policy.RequireAdminOrWriterPolicy)]
    public async Task<ActionResult> UpdateMultipleGlossaries([FromBody] EntryChipFieldUpdate dto)
    {
        if (dto?.Ids is null || !dto.Ids.Any())
        {
            return BadRequest();
        }

        var lockInfos = await entryHeaderUpdateService.UpdateGlossaries(dto);

        return Ok(lockInfos);
    }

    [HttpPut("update-multiple-related-collections")]
    [Authorize(Policy = Policy.RequireAdminOrWriterPolicy)]
    public async Task<ActionResult> UpdateMultipleRelatedCollections([FromBody] EntryChipFieldUpdate dto)
    {
        if (dto?.Ids is null || !dto.Ids.Any())
        {
            return BadRequest();
        }

        var lockInfos = await entryHeaderUpdateService.UpdateRelatedCollections(dto);

        return Ok(lockInfos);
    }


    [HttpPut("update-multiple-comment")]
    [Authorize(Policy = Policy.RequireAdminOrWriterPolicy)]
    public async Task<ActionResult> UpdateMultipleComment([FromBody] EntryCommentUpdate dto)
    {
        if (dto?.Ids is null || !dto.Ids.Any())
        {
            return BadRequest();
        }

        var lockInfos = await entryHeaderUpdateService.UpdateComment(dto);

        return Ok(lockInfos);
    }

    [HttpPut("update-multiple-concept-system-position")]
    [Authorize(Policy = Policy.RequireAdminOrWriterPolicy)]
    public async Task<ActionResult> UpdateMultipleConceptSystemPosition([FromBody] EntryConceptSystemPositionUpdate dto)
    {
        if (dto?.Ids is null || !dto.Ids.Any())
        {
            return BadRequest();
        }
        try
        {
            var lockInfos = await entryHeaderUpdateService.UpdateConceptSystemPosition(dto);

            return Ok(lockInfos);
        }
        catch (BusinessValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("update-multiple-source")]
    [Authorize(Policy = Policy.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> UpdateSource([FromBody] EntrySourceUpdate dto)
    {
        if (dto?.Ids is null || !dto.Ids.Any())
        {
            return BadRequest();
        }
        try
        {
            var lockInfos = await entryHeaderUpdateService.UpdateSource(dto);

            return Ok(lockInfos);
        }
        catch (BusinessValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("move-content")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> MoveContent([FromBody] EntryContentMove dto)
    {
        if (dto?.Ids is null || !dto.Ids.Any())
        {
            return BadRequest();
        }
        try
        {
            var lockInfos = await entryHeaderUpdateService.MoveContent(dto);

            return Ok(lockInfos);
        }
        catch (BusinessValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpDelete("{id}")]
    [Authorize(Policy = Policy.RequireAdminOrSuperUserPolicy)]
    public async Task<ActionResult> Delete([FromRoute] int id, [FromQuery] bool soft = true)
    {
        var updateInfos = await entryService.DeleteMultiple([id], soft);
        return Ok(updateInfos);
    }

    [HttpDelete("multi-delete")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> DeleteMultiple([FromBody] int[] ids, [FromQuery] bool soft = true)
    {
        var lockInfos = await entryService.DeleteMultiple(ids, soft);
        return Ok(lockInfos);
    }

    [HttpPost("restore")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Restore([FromBody] int id)
    {
        var entry = await entryService.RestoreMultiple(new[] { id });
        return Ok(entry);
    }

    [HttpPost("multi-restore")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> RestoreMultiple([FromBody] int[] ids)
    {
        var lockInfos = await entryService.RestoreMultiple(ids);
        return Ok(lockInfos);
    }

    [HttpGet("{id}/reference")]
    [Authorize(Policy = Policy.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> GetReferenceDetail([FromRoute] int id)
    {
        var result = await entryService.GetReferenceDetail(id);

        return Ok(result);
    }
}
