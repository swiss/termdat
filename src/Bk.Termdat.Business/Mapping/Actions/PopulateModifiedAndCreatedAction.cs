using System;
using AutoMapper;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Actions;

public class PopulateModifiedAndCreatedAction : IMappingAction<SearchQuery, SearchFilterParameter>
{
    private readonly IAuthorizationService authorizationService;
    private readonly IIdentityService identityService;

    public PopulateModifiedAndCreatedAction(IAuthorizationService authorizationService, IIdentityService identityService)
    {
        this.authorizationService = authorizationService;
        this.identityService = identityService;
    }

    public void Process(SearchQuery source, SearchFilterParameter destination, ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!authorizationService.IsWriter())
        {
            return;
        }

        if (source.CreatedByUser)
        {
            source.CreatedBy = identityService.GetCurrentEmail();
        }

        if (source.ModifiedByUser)
        {
            source.ModifiedBy = identityService.GetCurrentEmail();
        }
    }
}
