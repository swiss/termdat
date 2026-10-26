using System.Threading.Tasks;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/lenoch-classifications")]
public class LenochClassificationsController : ControllerBase
{
    private readonly ILenochClassificationService lenochClassificationService;

    public LenochClassificationsController(ILenochClassificationService lenochClassificationService)
    {
        this.lenochClassificationService = lenochClassificationService;
    }

    [HttpGet("")]
    public async Task<ActionResult> GetAll()
    {
        var result = await lenochClassificationService.GetAll();
        return Ok(result);
    }

    [HttpGet("details")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminPolicy)]
    public async Task<ActionResult> GetAllDetails()
    {
        var result = await lenochClassificationService.GetAllDetails();
        return Ok(result);
    }
}
