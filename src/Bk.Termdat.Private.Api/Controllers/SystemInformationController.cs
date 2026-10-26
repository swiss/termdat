using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Version = Bk.Termdat.Business.Resource.Models.Version;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/system-information")]
public class SystemInformationController : ControllerBase
{
    private const string ApplicationVersionEnvVariableName = "APPLICATION_VERSION";

    [HttpGet("version")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Version), StatusCodes.Status200OK)]
    public ActionResult GetApplicationVersion()
    {
        var version = Environment.GetEnvironmentVariable(ApplicationVersionEnvVariableName);

        var parts = version?.Split('_') ?? [];
        if (parts.Length == 2)
        {
            version = $"{parts[0]} ({parts[1]})";
        }

        return Ok(new Version { ApplicationVersion = version });
    }
}
