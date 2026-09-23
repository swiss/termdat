using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Public.Api.Examples;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Bk.Termdat.Public.Api.Controllers;

/// <summary>
/// Provides access to classifications.
/// </summary>
[ApiVersion("2.0")]
[ApiController]
[Route("v{version:apiVersion}/classification")]
public class ClassificationController : ControllerBase
{
    private readonly IClassificationService classificationService;

    public ClassificationController(IClassificationService classificationService)
    {
        this.classificationService = classificationService;
    }

    /// <summary>
    /// Returns a list of classifications available to the API.
    /// </summary>
    /// <param name="languageCode">Iso code of the language in which you want to receive the classifications. Supported is DE = German, FR = French, IT = Italian, RM = Romansh, EN = English. Case insensitive, e.g. "DE" / "De" / "de" all yield the same result</param>
    /// <returns>All classifications</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClassificationSummary>), 200)]
    [SwaggerResponseExample(200, typeof(ClassificationSummaryExamples))]
    [ProducesResponseType(400)]
    [SwaggerResponseExample(400, typeof(BadRequestExample))]
    [ProducesResponseType(500)]
    [SwaggerResponseExample(500, typeof(InternalServerErrorExample))]
    public async Task<IActionResult> GetClassifications([Required] string languageCode)
    {
        var classificationSummaries = await classificationService.GetAll(languageCode);
        return Ok(classificationSummaries);
    }
}
