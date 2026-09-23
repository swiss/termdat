using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Bk.Termdat.Business.Services;

public class IdentityService : IIdentityService
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly AuthorizationOptions options;

    public IdentityService(IHttpContextAccessor httpContextAccessor, IOptions<AuthorizationOptions> options)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.options = options.Value;
    }

    public string GetCurrentEmail()
    {
        return ExtractClaim(ClaimTypes.Email).ToLowerInvariant();
    }

    public string GetCurrentLastName()
    {
        return ExtractClaim(ClaimTypes.Surname);
    }

    public string GetCurrentFirstName()
    {
        return ExtractClaim(ClaimTypes.GivenName);
    }

    public string GetCurrentRole()
    {
        var roles = httpContextAccessor.HttpContext?.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        if (roles.Contains(options.Admin))
        {
            return options.Admin;
        }

        if (roles.Contains(options.SuperUser))
        {
            return options.SuperUser;
        }

        if (roles.Contains(options.SuperReader))
        {
            return options.SuperReader;
        }

        return roles.Contains(options.Writer) ? options.Writer : null;
    }

    private string ExtractClaim(string claimType)
    {
        var claim = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == claimType);
        return claim?.Value ?? throw new InvalidOperationException($"Claim with type '{claimType}' not found!");
    }
}
