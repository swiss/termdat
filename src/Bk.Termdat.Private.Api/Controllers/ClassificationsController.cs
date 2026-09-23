using System.Threading.Tasks;
using Bk.Termdat.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/classifications")]
public class ClassificationsController : ControllerBase
{
    private readonly IClassificationService classificationService;

    public ClassificationsController(IClassificationService classificationService)
    {
        this.classificationService = classificationService;
    }

    [HttpGet("")]
    public async Task<ActionResult> GetAll()
    {
        var result = await classificationService.GetAll();
        return Ok(result);
    }
}
