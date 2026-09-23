using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bk.Termdat.Private.Api.Controllers;

[ApiController]
[Route("api/user-accounts")]
public class UserAccountsController : ControllerBase
{
    private readonly IUserAccountService userAccountService;

    public UserAccountsController(IUserAccountService userAccountService)
    {
        this.userAccountService = userAccountService;
    }

    [HttpGet("")]
    [Authorize(Policy = Policy.RequireAdminOrSuperUserPolicy)]
    public async Task<ActionResult> GetAll()
    {
        var result = await userAccountService.GetAll();
        return Ok(result);
    }

    [HttpPost("")]
    [Authorize]
    public async Task<ActionResult> CreateOrUpdate()
    {
        var userAccount = await userAccountService.CreateOrUpdate();

        return Ok(userAccount);
    }

    [HttpPut("")]
    [Authorize]
    public async Task<ActionResult> Update([FromBody] UserAccountModification dto)
    {
        await userAccountService.Update(dto);

        return Ok();
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetDetail([FromRoute] int id)
    {
        var result = await userAccountService.GetDetail(id);
        return Ok(result);
    }

    [HttpGet("details")]
    [Authorize(Policy = Policy.RequireAdminPolicy)]
    public async Task<ActionResult> GetAllDetails()
    {
        var result = await userAccountService.GetAllDetails();
        return Ok(result);
    }
}
