using System.Threading.Tasks;
using Bk.Termdat.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/pictures")]
public class PicturesController : ControllerBase
{
    private readonly IPictureService pictureService;

    public PicturesController(IPictureService pictureService)
    {
        this.pictureService = pictureService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetPicture([FromRoute] int id)
    {
        var picture = await pictureService.GetPicture(id);

        if (picture == null)
        {
            return NotFound();
        }

        if (HttpContext.Request.Headers.Keys.Contains("If-None-Match") && picture.EntityTag == HttpContext.Request.Headers["If-None-Match"].ToString())
        {
            return new StatusCodeResult(304);
        }

        return new FileContentResult(picture.File.Value, picture.ContentType)
        {
            EntityTag = new EntityTagHeaderValue($"\"{picture.EntityTag}\""),
        };
    }
}
