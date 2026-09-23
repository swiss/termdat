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
/// Provides access to collections.
/// </summary>
[ApiVersion("2.0")]
[ApiController]
[Route("v{version:apiVersion}/collection")]
public class CollectionController : ControllerBase
{
    private readonly ICollectionService collectionService;

    public CollectionController(ICollectionService collectionService)
    {
        this.collectionService = collectionService;
    }

    /// <summary>
    /// Returns a list of collections available to the API.
    /// </summary>
    /// <param name="languageCode">Iso code of the language in which you want to receive the classifications. Supported is DE = German, FR = French, IT = Italian, RM = Romansh, EN = English. Case insensitive, e.g. "DE" / "De" / "de" all yield the same result</param>
    /// <returns>All collections</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CollectionSummary>), 200)]
    [SwaggerResponseExample(200, typeof(CollectionSummaryExamples))]
    [ProducesResponseType(400)]
    [SwaggerResponseExample(400, typeof(BadRequestExample))]
    [ProducesResponseType(500)]
    [SwaggerResponseExample(500, typeof(InternalServerErrorExample))]
    public async Task<IActionResult> GetCollections([Required] string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode))
        {
            return BadRequest();
        }

        var collectionSummaries = await collectionService.GetAllActiveApi(languageCode);
        return Ok(collectionSummaries);
    }
}
