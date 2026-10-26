using System;
using AutoMapper;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Options;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class RoleValueResolver : IValueResolver<DomainModels.UserAccount, ResourceModels.UserAccountDetail, string>
{
    private readonly AuthorizationOptions options;

    public RoleValueResolver(IOptions<AuthorizationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.options = options.Value;
    }

    public string Resolve(DomainModels.UserAccount source, ResourceModels.UserAccountDetail destination, string destMember, ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.Role == options.Admin)
        {
            return BackendTexts.RoleAdmin;
        }

        if (source.Role == options.SuperUser)
        {
            return BackendTexts.RoleSuperUser;
        }

        if (source.Role == options.Writer)
        {
            return BackendTexts.RoleWriter;
        }

        if (source.Role == options.SuperReader)
        {
            return BackendTexts.RoleSuperReader;
        }

        return BackendTexts.RoleNone;
    }
}
