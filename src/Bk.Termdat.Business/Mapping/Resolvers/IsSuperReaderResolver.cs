using System;
using AutoMapper;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Options;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class IsSuperReaderResolver : IValueResolver<DomainModels.UserAccount, ResourceModels.UserAccountDetail, bool>
{
    private readonly AuthorizationOptions options;

    public IsSuperReaderResolver(IOptions<AuthorizationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.options = options.Value;
    }

    public bool Resolve(DomainModels.UserAccount source, ResourceModels.UserAccountDetail destination, bool destMember, ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Role == options.SuperReader;
    }
}
