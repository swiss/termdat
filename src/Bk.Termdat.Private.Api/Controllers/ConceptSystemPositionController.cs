using System.Threading.Tasks;
using Bk.Termdat.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/concept-system-positions")]
public class ConceptSystemPositionController : ControllerBase
{
    private readonly IConceptSystemPositionService conceptSystemPositionService;

    public ConceptSystemPositionController(IConceptSystemPositionService conceptSystemPositionService)
    {
        this.conceptSystemPositionService = conceptSystemPositionService;
    }

    [HttpGet("")]
    public async Task<ActionResult> GetByCollectionId([FromQuery] int collectionId)
    {
        var result = await conceptSystemPositionService.GetByCollectionId(collectionId);
        return Ok(result);
    }
}
