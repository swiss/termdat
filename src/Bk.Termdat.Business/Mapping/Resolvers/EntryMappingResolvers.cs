using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Options;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping.Resolvers;

public class EntrySummaryIsMainOfficeResolver : IValueResolver<DomainModels.Entry, ResourceModels.EntrySummary, bool>
{
    private readonly PrivateOptions privateOptions;

    public EntrySummaryIsMainOfficeResolver(IOptions<PrivateOptions> privateOptions)
    {
        this.privateOptions = privateOptions.Value;
    }

    public bool Resolve(DomainModels.Entry source, ResourceModels.EntrySummary destination, bool destMember, ResolutionContext context)
    {
        return source.OfficeId == privateOptions.AchOfficeId;
    }
}

public class EntryDetailIsMainOfficeResolver : IValueResolver<DomainModels.Entry, ResourceModels.EntryDetail, bool>
{
    private readonly PrivateOptions privateOptions;

    public EntryDetailIsMainOfficeResolver(IOptions<PrivateOptions> privateOptions)
    {
        this.privateOptions = privateOptions.Value;
    }

    public bool Resolve(DomainModels.Entry source, ResourceModels.EntryDetail destination, bool destMember, ResolutionContext context)
    {
        return source.OfficeId == privateOptions.AchOfficeId;
    }
}

public class WatchlistEntryIsMainOfficeResolver : IValueResolver<DomainModels.Entry, ResourceModels.WatchlistEntry, bool>
{
    private readonly PrivateOptions privateOptions;

    public WatchlistEntryIsMainOfficeResolver(IOptions<PrivateOptions> privateOptions)
    {
        this.privateOptions = privateOptions.Value;
    }

    public bool Resolve(DomainModels.Entry source, ResourceModels.WatchlistEntry destination, bool destMember, ResolutionContext context)
    {
        return source.OfficeId == privateOptions.AchOfficeId;
    }
}

public class EntryCanBeAddedToWatchListResolver : IValueResolver<DomainModels.Entry, ResourceModels.EntrySummary, bool>
{
    private readonly IAuthorizationService authorizationService;

    public EntryCanBeAddedToWatchListResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public bool Resolve(DomainModels.Entry source, ResourceModels.EntrySummary destination, bool destMember, ResolutionContext context)
    {
        return authorizationService.CanAddToWatchList(source);
    }
}

public class EntryCanDeleteResolver : IValueResolver<DomainModels.Entry, ResourceModels.IEntryDetail, bool>
{
    private readonly IAuthorizationService authorizationService;

    public EntryCanDeleteResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public bool Resolve(DomainModels.Entry source, ResourceModels.IEntryDetail destination, bool destMember, ResolutionContext context)
    {
        return authorizationService.CanDelete(source);
    }
}

public class EntryCanRestoreResolver : IValueResolver<DomainModels.Entry, ResourceModels.IEntryDetail, bool>
{
    private readonly IAuthorizationService authorizationService;

    public EntryCanRestoreResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public bool Resolve(DomainModels.Entry source, ResourceModels.IEntryDetail destination, bool destMember, ResolutionContext context)
    {
        return authorizationService.CanRestore(source);
    }
}

public class EntryCanEditCollectionResolver : IValueResolver<DomainModels.Entry, ResourceModels.IEntryDetail, bool>
{
    private readonly IAuthorizationService authorizationService;

    public EntryCanEditCollectionResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public bool Resolve(DomainModels.Entry source, ResourceModels.IEntryDetail destination, bool destMember, ResolutionContext context)
    {
        return authorizationService.HasAccessToCollection(source.CollectionId);
    }
}

public class EntryCanEditResolver : IValueResolver<DomainModels.Entry, ResourceModels.IEntryDetail, bool>
{
    private readonly IAuthorizationService authorizationService;

    public EntryCanEditResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public bool Resolve(DomainModels.Entry source, ResourceModels.IEntryDetail destination, bool destMember, ResolutionContext context)
    {
        return authorizationService.CanEdit(source);
    }
}

public class EntryLockedResolver : IValueResolver<DomainModels.Entry, ResourceModels.IEntryDetail, DateTime?>
{
    private readonly IAuthorizationService authorizationService;

    public EntryLockedResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public DateTime? Resolve(DomainModels.Entry source, ResourceModels.IEntryDetail destination, DateTime? destMember, ResolutionContext context)
    {
        return authorizationService.CanEdit(source) ? source.Locked : null;
    }
}

public class EntryLockedByResolver : IValueResolver<DomainModels.Entry, ResourceModels.IEntryDetail, string>
{
    private readonly IAuthorizationService authorizationService;

    public EntryLockedByResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public string Resolve(DomainModels.Entry source, ResourceModels.IEntryDetail destination, string destMember, ResolutionContext context)
    {
        return authorizationService.CanEdit(source) ? source.LockedBy : null;
    }
}

public class EntryDetailConceptUriResolver : IValueResolver<DomainModels.Entry, ResourceModels.EntryDetail, string>
{
    private readonly PrivateOptions privateOptions;

    public EntryDetailConceptUriResolver(IOptions<PrivateOptions> privateOptions)
    {
        this.privateOptions = privateOptions.Value;
    }

    public string Resolve(DomainModels.Entry source, ResourceModels.EntryDetail destination, string destMember, ResolutionContext context)
    {
        if (source.Collection == null || !source.Collection.Active || source.Collection.Deleted || !source.Collection.ExportLindas)
        {
            return null;
        }

        return string.Format(privateOptions.ConceptUriFormat, source.Id);
    }
}

public class EntryDetailRelatedCollectionsResolver : IValueResolver<DomainModels.Entry, ResourceModels.EntryDetail, IList<ResourceModels.CollectionSummary>>
{
    private readonly IAuthorizationService authorizationService;

    public EntryDetailRelatedCollectionsResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public IList<ResourceModels.CollectionSummary> Resolve(DomainModels.Entry source, ResourceModels.EntryDetail destination, IList<ResourceModels.CollectionSummary> destMember, ResolutionContext context)
    {
        if (!authorizationService.IsAdmin() && !authorizationService.IsSuperUser())
        {
            return null;
        }

        var relatedCollections = source.RelatedCollections?.Where(rc => !rc.Deleted).ToList();
        return relatedCollections == null
            ? null
            : context.Mapper.Map<IList<ResourceModels.CollectionSummary>>(relatedCollections);
    }
}

public class EntryDetailIdentificationResolver : IValueResolver<DomainModels.Entry, ResourceModels.EntryDetail, int?>
{
    private readonly IAuthorizationService authorizationService;

    public EntryDetailIdentificationResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public int? Resolve(DomainModels.Entry source, ResourceModels.EntryDetail destination, int? destMember, ResolutionContext context)
    {
        return authorizationService.IsAdmin() || authorizationService.IsSuperUser() ? source.Identification : null;
    }
}

public class EntryDetailCommentResolver : IValueResolver<DomainModels.Entry, ResourceModels.EntryDetail, string>
{
    private readonly IAuthorizationService authorizationService;

    public EntryDetailCommentResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public string Resolve(DomainModels.Entry source, ResourceModels.EntryDetail destination, string destMember, ResolutionContext context)
    {
        return authorizationService.IsAdmin() || authorizationService.IsSuperUser() || authorizationService.IsWriter() || authorizationService.IsSuperReader()
            ? source.Comment
            : null;
    }
}

public class EntryDetailAdminInfosResolver : IValueResolver<DomainModels.Entry, ResourceModels.EntryDetail, IList<ResourceModels.AdminInfoSummary>>
{
    private readonly IAuthorizationService authorizationService;

    public EntryDetailAdminInfosResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public IList<ResourceModels.AdminInfoSummary> Resolve(DomainModels.Entry source, ResourceModels.EntryDetail destination, IList<ResourceModels.AdminInfoSummary> destMember, ResolutionContext context)
    {
        if (!authorizationService.IsAdmin() && !authorizationService.IsSuperUser() && !authorizationService.IsWriter() && !authorizationService.IsSuperReader())
        {
            return null;
        }

        return source.AdminInfos == null
            ? null
            : context.Mapper.Map<IList<ResourceModels.AdminInfoSummary>>(source.AdminInfos);
    }
}

public class EntryDetailCreatedByResolver : IValueResolver<DomainModels.Entry, ResourceModels.EntryDetail, string>
{
    private readonly IAuthorizationService authorizationService;

    public EntryDetailCreatedByResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public string Resolve(DomainModels.Entry source, ResourceModels.EntryDetail destination, string destMember, ResolutionContext context)
    {
        return authorizationService.IsAdmin() || authorizationService.IsSuperUser() || authorizationService.IsWriter() ? source.CreatedBy : null;
    }
}

public class EntryDetailModifiedByResolver : IValueResolver<DomainModels.Entry, ResourceModels.EntryDetail, string>
{
    private readonly IAuthorizationService authorizationService;

    public EntryDetailModifiedByResolver(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    public string Resolve(DomainModels.Entry source, ResourceModels.EntryDetail destination, string destMember, ResolutionContext context)
    {
        return authorizationService.IsAdmin() || authorizationService.IsSuperUser() || authorizationService.IsWriter() ? source.ModifiedBy : null;
    }
}

public class EntryLanguageNoteTooltipResolver : IValueResolver<DomainModels.EntryLanguage, ResourceModels.EntryLanguageDetail, IList<ResourceModels.NoteSubCodeTooltip>>
{
    private readonly INoteSubCodeTranslationService _service;

    public EntryLanguageNoteTooltipResolver(INoteSubCodeTranslationService service)
    {
        _service = service;
    }

    public IList<ResourceModels.NoteSubCodeTooltip> Resolve(
        DomainModels.EntryLanguage source,
        ResourceModels.EntryLanguageDetail destination,
        IList<ResourceModels.NoteSubCodeTooltip> destMember,
        ResolutionContext context)
    {
        return _service.GetTooltips(source.Note);
    }
}

public class EntryWatchlistGlossariesResolver : IValueResolver<DomainModels.Entry, ResourceModels.WatchlistEntry, string>
{
    public string Resolve(
        DomainModels.Entry source,
        ResourceModels.WatchlistEntry destination,
        string destMember,
        ResolutionContext context)
    {
        return string.Join(", ", source.Glossaries.Select(x => x.Code));
    }
}

public class EntryWatchlistAdminInfosResolver : IValueResolver<DomainModels.Entry, ResourceModels.WatchlistEntry, string>
{
    public string Resolve(
        DomainModels.Entry source,
        ResourceModels.WatchlistEntry destination,
        string destMember,
        ResolutionContext context)
    {
        return string.Join(", ", source.AdminInfos.Select(x => x.Code));
    }
}
