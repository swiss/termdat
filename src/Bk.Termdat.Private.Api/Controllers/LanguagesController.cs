using System;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/languages")]
public class LanguagesController : ControllerBase
{
    private readonly ILanguageService languageService;

    public LanguagesController(ILanguageService languageService)
    {
        this.languageService = languageService;
    }

    [HttpGet("details")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetAllDetails()
    {
        var result = await languageService.GetAllDetails();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetDetail([FromRoute] int id)
    {
        var result = await languageService.GetDetail(id);
        return Ok(result);
    }

    [HttpGet("get-by-iso-code")]
    public async Task<ActionResult> GetByIsoCode([FromQuery] string isoCode)
    {
        if (string.IsNullOrEmpty(isoCode))
        {
            return BadRequest(new ArgumentNullException(nameof(isoCode)));
        }

        var result = await languageService.GetTranslationByIsoCode(isoCode);
        return Ok(result);
    }

    [HttpGet("next-sort")]
    public int NextSort()
    {
        var result = languageService.GetNextSort();
        return result;
    }


    [HttpPut("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] LanguageUpdate languageUpdate)
    {
        if (languageUpdate?.Id != id)
        {
            return BadRequest();
        }

        var updatedLanguage = await languageService.Update(languageUpdate);

        return Ok(updatedLanguage);
    }

    [HttpPost]
    [Authorize(Policy = Policy.RequireAdminOrWriterOrSuperUserPolicy)]
    public async Task<ActionResult> Create([FromBody] LanguageCreate languageCreate)
    {
        if (languageCreate is null)
        {
            return BadRequest();
        }

        var createdLanguage = await languageService.Create(languageCreate);

        return Ok(createdLanguage);
    }
}
