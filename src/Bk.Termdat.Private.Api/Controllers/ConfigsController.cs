using System.Threading.Tasks;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/configs")]
public class ConfigsController : ControllerBase
{
    private readonly IOptions<FrontendOptions> _frontendOptions;

    public ConfigsController(IOptions<FrontendOptions> frontendOptions)
    {
        _frontendOptions = frontendOptions;
    }

    [HttpGet("frontend")]
    public Task<ActionResult> GetFrontendOptions()
    {
        return Task.FromResult<ActionResult>(Ok(_frontendOptions.Value));
    }
}
