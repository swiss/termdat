using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Mapping.Actions;
using Bk.Termdat.Business.Mapping.Resolvers;
using Bk.Termdat.CrossCutting;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping.Profiles;

public class EntryMappingProfile : Profile
{
    public EntryMappingProfile()
    {
        CreateEntryMapping();
        CreateEntrySummaryMapping();
        CreateEntryLanguageSummaryMapping();
        CreateEntryLanguageMapping();
        CreateEntryLanguageDetailMapping();
        CreateEntryDetailMapping();
        CreatePictureMapping();
        CreateWatchlistMapping();
        CreateReferenceMapping();
        CreateExternalLinkMapping();
        CreateEntryTemplateMapping();
    }

    private void CreateEntryTemplateMapping()
    {
        CreateMap<DomainModels.EntryTemplate, ResourceModels.EntryTemplateSummary>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.Name));
    }

    private void CreateReferenceMapping()
    {
        CreateMap<ResourceModels.EntryModificationReference, DomainModels.EntryReference>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ReferencedEntryId, opt => opt.MapFrom(src => src.EntryId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.EntryId, opt => opt.Ignore())
            .ForMember(dest => dest.Entry, opt => opt.Ignore())
            .ForMember(dest => dest.ReferencedEntry, opt => opt.Ignore());

        CreateMap<DomainModels.Entry, ResourceModels.EntryReferenceDetail>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.Translations.Where(t => t.Sequence == 1)));

        CreateMap<DomainModels.EntryLanguage, ResourceModels.EntryReferenceDetailLanguage>()
            .ForMember(dest => dest.LanguageId, opt => opt.MapFrom(src => src.LanguageId))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(GetReferenceEntryLanguageText));

        CreateMap<DomainModels.EntryReference, ResourceModels.IEntryReferenceDetail>()
            .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.ReferencedEntryId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.ReferencedEntry.Translations.Where(t => t.Sequence == 1).OrderBy(t => t.Language != null ? t.Language.SortIndex : 0)));

        CreateMap<DomainModels.EntryReference, ResourceModels.EntryReferenceDetail>()
            .IncludeBase<DomainModels.EntryReference, ResourceModels.IEntryReferenceDetail>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        CreateMap<DomainModels.EntryReference, ResourceModels.EntryReferencedByDetail>()
            .IncludeBase<DomainModels.EntryReference, ResourceModels.IEntryReferenceDetail>()
            .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.EntryId))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.Entry.Translations.Where(t => t.Sequence == 1).OrderBy(t => t.Language != null ? t.Language.SortIndex : 0)));
    }

    private void CreateExternalLinkMapping()
    {
        CreateMap<ResourceModels.EntryModificationExternalLink, DomainModels.EntryExternalLink>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
            .ForMember(dest => dest.EntryId, opt => opt.Ignore())
            .ForMember(dest => dest.Entry, opt => opt.Ignore());

        CreateMap<DomainModels.EntryExternalLink, ResourceModels.ExternalLinkDetail>()
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
    }

    private void CreateEntryMapping()
    {
        CreateMap<DomainModels.EntryLanguage, DomainModels.EntryLanguage>()
            .ForMember(dest => dest.Entry, opt => opt.Ignore())
            .ForMember(dest => dest.EntryId, opt => opt.Ignore());

        CreateMap<ResourceModels.EntryCreate, DomainModels.Entry>()
            .IncludeBase<ResourceModels.IEntryModification, DomainModels.Entry>()
            .ForMember(dest => dest.ExternalSystemId, opt => opt.MapFrom(src => src.ExternalSystemId));

        CreateMap<ResourceModels.EntryTemplateCreate, DomainModels.Entry>()
            .IncludeBase<ResourceModels.IEntryModification, DomainModels.Entry>()
            .ForMember(dest => dest.ExternalSystemId, opt => opt.Ignore());

        CreateMap<ResourceModels.EntryUpdate, DomainModels.Entry>()
            .IncludeBase<ResourceModels.IEntryModification, DomainModels.Entry>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.MapFrom(src => src.ConcurrencyToken))
            .ForMember(dest => dest.ExternalSystemId, opt => opt.Ignore());

        CreateMap<ResourceModels.IEntryModification, DomainModels.Entry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Reliability, opt => opt.MapFrom(src => src.Reliability))
            .ForMember(dest => dest.Collection, opt => opt.Ignore())
            .ForMember(dest => dest.CollectionId, opt => opt.MapFrom(src => src.CollectionId))
            .ForMember(dest => dest.ConceptSystemPosition, opt => opt.Ignore())
            .ForMember(dest => dest.ConceptSystemPositionId, opt => opt.MapFrom(src => src.ConceptSystemPositionId))
            .ForMember(dest => dest.Office, opt => opt.Ignore())
            .ForMember(dest => dest.OfficeId, opt => opt.MapFrom(src => src.OfficeId))
            .ForMember(dest => dest.Glossaries, opt => opt.Ignore())
            .ForMember(dest => dest.RelatedCollections, opt => opt.Ignore())
            .ForMember(dest => dest.LenochClassifications, opt => opt.Ignore())
            .ForMember(dest => dest.Identification, opt => opt.MapFrom(src => src.Identification))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Deleted, opt => opt.Ignore())
            .ForMember(dest => dest.Picture, opt => opt.Ignore())
            .ForMember(dest => dest.Locked, opt => opt.Ignore())
            .ForMember(dest => dest.LockedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.LanguageDetails))
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.References, opt => opt.MapFrom(src => src.References))
            .ForMember(dest => dest.ReferencedByEntries, opt => opt.Ignore())
            .ForMember(dest => dest.Domains, opt => opt.Ignore())
            .ForMember(dest => dest.Subjects, opt => opt.Ignore())
            .ForMember(dest => dest.AdminInfos, opt => opt.Ignore())
            .ForMember(dest => dest.AdminInfo, opt => opt.Ignore())
            .ForMember(dest => dest.ExternalSystemId, opt => opt.Ignore());
    }

    private void CreateEntryLanguageMapping()
    {
        CreateMap<ResourceModels.EntryLanguageDetail, DomainModels.EntryLanguage>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.LanguageId, opt => opt.MapFrom(src => src.LanguageId))
            .ForMember(dest => dest.Language, opt => opt.Ignore())
            .ForMember(dest => dest.EntryId, opt => opt.Ignore())
            .ForMember(dest => dest.Entry, opt => opt.Ignore())
            .ForMember(dest => dest.Sequence, opt => opt.MapFrom(src => src.Sequence))
            .ForMember(dest => dest.Terminus, opt => opt.MapFrom(src => src.Terminus))
            .ForMember(dest => dest.TerminusSource, opt => opt.MapFrom(src => src.TerminusSource))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.NameSource, opt => opt.MapFrom(src => src.NameSource))
            .ForMember(dest => dest.Abbreviation, opt => opt.MapFrom(src => src.Abbreviation))
            .ForMember(dest => dest.AbbreviationSource, opt => opt.MapFrom(src => src.AbbreviationSource))
            .ForMember(dest => dest.Phraseology, opt => opt.MapFrom(src => src.Phraseology))
            .ForMember(dest => dest.PhraseologySource, opt => opt.MapFrom(src => src.PhraseologySource))
            .ForMember(dest => dest.Definition, opt => opt.MapFrom(src => src.Definition))
            .ForMember(dest => dest.DefinitionSource, opt => opt.MapFrom(src => src.DefinitionSource))
            .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
            .ForMember(dest => dest.NoteSource, opt => opt.MapFrom(src => src.NoteSource))
            .ForMember(dest => dest.Context, opt => opt.MapFrom(src => src.Context))
            .ForMember(dest => dest.ContextSource, opt => opt.MapFrom(src => src.ContextSource))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata))
            .ForMember(dest => dest.MetadataSource, opt => opt.MapFrom(src => src.MetadataSource))
            .ForMember(dest => dest.IsRare, opt => opt.MapFrom(src => src.IsRare))
            .ForMember(dest => dest.IsObsolete, opt => opt.MapFrom(src => src.IsObsolete))
            .ForMember(dest => dest.ShouldAvoid, opt => opt.MapFrom(src => src.ShouldAvoid))
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore());
    }

    private void CreateEntryLanguageDetailMapping()
    {
        CreateMap<DomainModels.EntryLanguage, ResourceModels.IEntryLanguageDetail>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.LanguageId, opt => opt.MapFrom(src => src.LanguageId))
            .ForMember(dest => dest.Sequence, opt => opt.MapFrom(src => src.Sequence))
            .ForMember(dest => dest.Terminus, opt => opt.MapFrom(src => src.Terminus))
            .ForMember(dest => dest.TerminusSource, opt => opt.MapFrom(src => src.TerminusSource))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.NameSource, opt => opt.MapFrom(src => src.NameSource))
            .ForMember(dest => dest.Abbreviation, opt => opt.MapFrom(src => src.Abbreviation))
            .ForMember(dest => dest.AbbreviationSource, opt => opt.MapFrom(src => src.AbbreviationSource))
            .ForMember(dest => dest.Phraseology, opt => opt.MapFrom(src => src.Phraseology))
            .ForMember(dest => dest.PhraseologySource, opt => opt.MapFrom(src => src.PhraseologySource))
            .ForMember(dest => dest.Definition, opt => opt.MapFrom(src => src.Definition))
            .ForMember(dest => dest.DefinitionSource, opt => opt.MapFrom(src => src.DefinitionSource))
            .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
            .ForMember(dest => dest.NoteSource, opt => opt.MapFrom(src => src.NoteSource))
            .ForMember(dest => dest.Context, opt => opt.MapFrom(src => src.Context))
            .ForMember(dest => dest.ContextSource, opt => opt.MapFrom(src => src.ContextSource))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata))
            .ForMember(dest => dest.MetadataSource, opt => opt.MapFrom(src => src.MetadataSource))
            .ForMember(dest => dest.IsRare, opt => opt.MapFrom(src => src.IsRare))
            .ForMember(dest => dest.IsObsolete, opt => opt.MapFrom(src => src.IsObsolete))
            .ForMember(dest => dest.ShouldAvoid, opt => opt.MapFrom(src => src.ShouldAvoid))
            .ReverseMap();

        CreateMap<DomainModels.EntryLanguage, ResourceModels.EntryLanguageDetail>()
            .IncludeBase<DomainModels.EntryLanguage, ResourceModels.IEntryLanguageDetail>()
            .ForMember(dest => dest.NoteTooltips, opt => opt.MapFrom<EntryLanguageNoteTooltipResolver>())
            .ForMember(dest => dest.References, opt =>
            {
                opt.PreCondition(src => src.Sequence == 1 && src.Entry?.References?.Count > 0);
                opt.MapFrom(src => CreateReferences(src.Entry, src.LanguageId));
            })
            .AfterMap<EntryLanguageDetailAuthorizationAction>();

        CreateMap<DomainModels.EntryLanguage, ResourceModels.EntryModificationLanguageDetail>()
            .IncludeBase<DomainModels.EntryLanguage, ResourceModels.IEntryLanguageDetail>()
            .ReverseMap();
    }

    private void CreateEntryLanguageSummaryMapping()
    {
        CreateMap<DomainModels.EntryLanguage, ResourceModels.EntryLanguageSummary>()
            .ForMember(dest => dest.LanguageId, opt => opt.MapFrom(src => src.LanguageId))
            .ForMember(dest => dest.Sequence, opt => opt.MapFrom(src => src.Language.SortIndex))
            .ForMember(dest => dest.Terminus, opt => opt.MapFrom(src => src.Terminus))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Phraseology, opt => opt.MapFrom(src => src.Phraseology))
            .ForMember(dest => dest.Abbreviation, opt => opt.MapFrom(src => src.Abbreviation))
            .ForMember(dest => dest.IsRare, opt => opt.MapFrom(src => src.IsRare))
            .ForMember(dest => dest.IsObsolete, opt => opt.MapFrom(src => src.IsObsolete))
            .ForMember(dest => dest.ShouldAvoid, opt => opt.MapFrom(src => src.ShouldAvoid));
    }

    private void CreateEntrySummaryMapping()
    {
        CreateMap<DomainModels.Entry, ResourceModels.EntrySummary>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Terms, opt => opt.MapFrom(src => src.Translations))
            .ForMember(dest => dest.Deleted, opt => opt.MapFrom(src => src.Deleted))
            .ForMember(dest => dest.IsMainOffice, opt => opt.MapFrom<EntrySummaryIsMainOfficeResolver>())
            .ForMember(dest => dest.Collection, opt => opt.MapFrom(src => src.Collection))
            .ForMember(dest => dest.CanBeAddedToWatchList, opt => opt.MapFrom<EntryCanBeAddedToWatchListResolver>());
    }

    private void CreateEntryDetailMapping()
    {
        CreateMap<DomainModels.Entry, ResourceModels.IEntryDetail>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Deleted, opt => opt.MapFrom(src => src.Deleted))
            .ForMember(dest => dest.Reliability, opt => opt.MapFrom(src => src.Reliability))
            .ForMember(dest => dest.Collection, opt => opt.MapFrom(src => src.Collection))
            .ForMember(dest => dest.ConceptSystemPosition, opt => opt.MapFrom(src => src.ConceptSystemPosition))
            .ForMember(dest => dest.Office, opt => opt.MapFrom(src => src.Office))
            .ForMember(dest => dest.RelatedCollections, opt => opt.MapFrom(src => src.RelatedCollections))
            .ForMember(dest => dest.LenochClassifications, opt => opt.MapFrom(src => src.LenochClassifications))
            .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects))
            .ForMember(dest => dest.Domains, opt => opt.MapFrom(src => src.Domains))
            .ForMember(dest => dest.Identification, opt => opt.MapFrom(src => src.Identification))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.MapFrom(src => src.ConcurrencyToken))
            .ForMember(dest => dest.Picture, opt => opt.MapFrom(src => src.Picture))
            .ForMember(dest => dest.Glossaries, opt => opt.MapFrom(src => src.Glossaries))
            .ForMember(dest => dest.AdminInfos, opt => opt.MapFrom(src => src.AdminInfos))
            .ForMember(dest => dest.CanDelete, opt => opt.MapFrom<EntryCanDeleteResolver>())
            .ForMember(dest => dest.CanRestore, opt => opt.MapFrom<EntryCanRestoreResolver>())
            .ForMember(dest => dest.CanEditCollection, opt => opt.MapFrom<EntryCanEditCollectionResolver>())
            .ForMember(dest => dest.CanEdit, opt => opt.MapFrom<EntryCanEditResolver>())
            .ForMember(dest => dest.ExternalLinkSummary, opt => opt.MapFrom(src => src.ExternalLinks))
            .ForMember(dest => dest.Locked, opt => opt.MapFrom<EntryLockedResolver>())
            .ForMember(dest => dest.LockedBy, opt => opt.MapFrom<EntryLockedByResolver>());

        CreateMap<DomainModels.EntryExternalLink, ResourceModels.EntryExternalLinkSummary>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url));

        CreateMap<DomainModels.Entry, ResourceModels.IEntryDetail<ResourceModels.EntryLanguageDetail>>()
            .IncludeBase<DomainModels.Entry, ResourceModels.IEntryDetail>()
            .ForMember(dest => dest.LanguageDetails, opt => opt.MapFrom(src => src.Translations.OrderBy(t => t.Language != null ? t.Language.SortIndex : 0).ThenBy(t => t.Sequence)));

        CreateMap<DomainModels.Entry, ResourceModels.IEntryDetail<ResourceModels.EntryModificationLanguageDetail>>()
            .IncludeBase<DomainModels.Entry, ResourceModels.IEntryDetail>()
            .ForMember(dest => dest.LanguageDetails, opt => opt.MapFrom(src => src.Translations.OrderBy(t => t.Language != null ? t.Language.SortIndex : 0).ThenBy(t => t.Sequence)));

        CreateMap<DomainModels.Entry, ResourceModels.EntryDetail>()
            .IncludeBase<DomainModels.Entry, ResourceModels.IEntryDetail<ResourceModels.EntryLanguageDetail>>()
            .ForMember(dest => dest.IsMainOffice, opt => opt.MapFrom<EntryDetailIsMainOfficeResolver>())
            .ForMember(dest => dest.ConceptUri, opt => opt.MapFrom<EntryDetailConceptUriResolver>())
            .ForMember(dest => dest.RelatedCollections, opt =>
            {
                opt.AllowNull();
                opt.MapFrom<EntryDetailRelatedCollectionsResolver>();
            })
            .ForMember(dest => dest.Identification, opt => opt.MapFrom<EntryDetailIdentificationResolver>())
            .ForMember(dest => dest.Comment, opt => opt.MapFrom<EntryDetailCommentResolver>())
            .ForMember(dest => dest.AdminInfos, opt => opt.MapFrom<EntryDetailAdminInfosResolver>())
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom<EntryDetailCreatedByResolver>())
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom<EntryDetailModifiedByResolver>())
            .ForMember(dest => dest.ReferencedBy, opt => opt.MapFrom(src => src.ReferencedByEntries.Where(er => !er.Entry.Deleted)));

        CreateMap<DomainModels.Entry, ResourceModels.EntryModificationDetail>()
            .IncludeBase<DomainModels.Entry, ResourceModels.IEntryDetail<ResourceModels.EntryModificationLanguageDetail>>()
            .ForMember(dest => dest.References, opt => opt.MapFrom(src => src.References.OrderBy(r => r.Type)));
    }

    private void CreatePictureMapping()
    {
        CreateMap<DomainModels.Picture, ResourceModels.PictureDetail>()
            .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.EntryId))
            .ForMember(dest => dest.EntityTag, opt => opt.MapFrom(src => src.EntityTag))
            .ForMember(dest => dest.DescriptionDe, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionFr, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionIt, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionRm, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionEn, opt => opt.Ignore())
            .ForMember(dest => dest.SourceDe, opt => opt.Ignore())
            .ForMember(dest => dest.SourceFr, opt => opt.Ignore())
            .ForMember(dest => dest.SourceIt, opt => opt.Ignore())
            .ForMember(dest => dest.SourceRm, opt => opt.Ignore())
            .ForMember(dest => dest.SourceEn, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                var translationDe = src.Translations.FirstOrDefault(t => IsoCode.De.IsEqualTo(t.Language.IsoCode));
                var translationFr = src.Translations.FirstOrDefault(t => IsoCode.Fr.IsEqualTo(t.Language.IsoCode));
                var translationIt = src.Translations.FirstOrDefault(t => IsoCode.It.IsEqualTo(t.Language.IsoCode));
                var translationRm = src.Translations.FirstOrDefault(t => IsoCode.Rm.IsEqualTo(t.Language.IsoCode));
                var translationEn = src.Translations.FirstOrDefault(t => IsoCode.En.IsEqualTo(t.Language.IsoCode));

                dest.DescriptionDe = translationDe?.Description;
                dest.DescriptionFr = translationFr?.Description;
                dest.DescriptionIt = translationIt?.Description;
                dest.DescriptionRm = translationRm?.Description;
                dest.DescriptionEn = translationEn?.Description;

                dest.SourceDe = translationDe?.Source;
                dest.SourceFr = translationFr?.Source;
                dest.SourceIt = translationIt?.Source;
                dest.SourceRm = translationRm?.Source;
                dest.SourceEn = translationEn?.Source;
            });
    }

    private void CreateWatchlistMapping()
    {
        CreateMap<DomainModels.Entry, ResourceModels.WatchlistEntry>()
            .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Identification, opt => opt.MapFrom(src => src.Identification))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Deleted, opt => opt.MapFrom(src => src.Deleted))
            .ForMember(dest => dest.Locked, opt => opt.MapFrom(src => src.Locked != null))
            .ForMember(dest => dest.ConceptSystemPosition, opt => opt.MapFrom(src => src.ConceptSystemPosition))
            .ForMember(dest => dest.Collection, opt => opt.MapFrom(src => src.Collection))
            .ForMember(dest => dest.IsMainOffice, opt => opt.MapFrom<WatchlistEntryIsMainOfficeResolver>())
            .ForMember(dest => dest.Value, opt => opt.MapFrom<EntryWatchlistValueResolver>())
            .ForMember(dest => dest.Glossaries, opt => opt.MapFrom<EntryWatchlistGlossariesResolver>())
            .ForMember(dest => dest.AdminInfos, opt => opt.MapFrom<EntryWatchlistAdminInfosResolver>());
    }

    private static List<ResourceModels.EntryReferenceSummary> CreateReferences(DomainModels.Entry entry, int languageId)
    {
        var references = new List<ResourceModels.EntryReferenceSummary>();

        var orderedReferences = entry.References.Where(er => !er.ReferencedEntry.Deleted).OrderBy(er => er.Type).ToList();

        foreach (var er in orderedReferences)
        {
            var synonym = er.ReferencedEntry?.Translations.Where(t => t.LanguageId == languageId).OrderBy(t => t.Sequence).FirstOrDefault();

            if (synonym == null)
            {
                continue;
            }

            references.Add(new ResourceModels.EntryReferenceSummary
            {
                Text = GetReferenceText(synonym),
                EntryId = er.ReferencedEntryId,
                Type = (ResourceModels.ReferenceType)er.Type
            });
        }

        return references;
    }

    private static string GetReferenceText(DomainModels.EntryLanguage entryLanguage)
    {
        if (!string.IsNullOrWhiteSpace(entryLanguage.Terminus))
        {
            return entryLanguage.Terminus;
        }

        if (!string.IsNullOrWhiteSpace(entryLanguage.Name))
        {
            return entryLanguage.Name;
        }

        return !string.IsNullOrWhiteSpace(entryLanguage.Phraseology) ? entryLanguage.Phraseology : entryLanguage.Abbreviation;
    }

    private static string GetReferenceEntryLanguageText(DomainModels.EntryLanguage entryLanguage, ResourceModels.EntryReferenceDetailLanguage _)
    {
        return GetReferenceText(entryLanguage);
    }
}
