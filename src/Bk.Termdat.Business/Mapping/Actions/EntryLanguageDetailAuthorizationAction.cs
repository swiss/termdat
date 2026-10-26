using System;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;

namespace Bk.Termdat.Business.Mapping.Actions;

public class EntryLanguageDetailAuthorizationAction : IMappingAction<EntryLanguage, EntryLanguageDetail>
{
    private readonly IAuthorizationService authorizationService;

    public EntryLanguageDetailAuthorizationAction(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public void Process(EntryLanguage source, EntryLanguageDetail destination, ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(destination);

        if (authorizationService.IsAdmin() || authorizationService.IsSuperUser() || authorizationService.IsWriter())
        {
            return;
        }

        destination.Metadata = null;
        destination.MetadataSource = null;
    }
}
