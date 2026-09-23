using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Mapping.Actions;
using Bk.Termdat.Business.Mapping.Resolvers;
using Bk.Termdat.Business.Resource.Models.Export;
using Bk.Termdat.Common.Resources;

namespace Bk.Termdat.Business.Mapping.Profiles;

public class ExportMappingProfile : Profile
{
    public ExportMappingProfile()
    {
        CreateMap<Entry, WatchlistExportEntry>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Office.Translations))
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Collection.Translations))
            .BeforeMap((src, _) => src.RelatedCollections.ToList().ForEach(item => MappingUtil.AssertTranslationsLanguageNotNull(item.Translations)))
            .BeforeMap((src, _) => src.Glossaries.ToList().ForEach(item => MappingUtil.AssertTranslationsLanguageNotNull(item.Translations)))
            .ForMember(dest => dest.Value, opt => opt.MapFrom<WatchlistExportEntryValueResolver>())
            .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatedKey, opt => opt.MapFrom(_ => BackendTexts.EntryCreated))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created.ToLocalTime().ToString(CultureInfo.CurrentCulture)))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.ModifiedKey, opt => opt.MapFrom(_ => BackendTexts.EntryModified))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified.ToLocalTime().ToString(CultureInfo.CurrentCulture)))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy))
            .ForMember(dest => dest.Office, opt => opt.MapFrom<OfficeDisplayNameResolver>())
            .ForMember(dest => dest.Picture, opt => opt.MapFrom(src => PictureToBase64String(src.Picture)))
            .ForMember(dest => dest.Collection, opt => opt.MapFrom<CollectionDisplayNameResolver>())
            .ForMember(dest => dest.RelatedCollectionsKey, opt => opt.MapFrom(_ => BackendTexts.EntryRelatedCollections))
            .ForMember(dest => dest.RelatedCollections, opt => opt.MapFrom<RelatedCollectionsResolver>())
            .ForMember(dest => dest.GlossariesKey, opt => opt.MapFrom(_ => BackendTexts.Glossaries))
            .ForMember(dest => dest.Glossaries, opt => opt.MapFrom<GlossariesResolver>())
            .ForMember(dest => dest.AdminInfosKey, opt => opt.MapFrom(_ => BackendTexts.EntryAdminInfo))
            .ForMember(dest => dest.AdminInfos, opt => opt.MapFrom(src => string.Join(", ", src.AdminInfos.Select(item => item.Code))))
            .ForMember(dest => dest.ConceptPositionKey, opt => opt.MapFrom(_ => BackendTexts.EntryConceptSystemPosition))
            .ForMember(dest => dest.ConceptPosition, opt => opt.MapFrom(src => src.ConceptSystemPosition.Value))
            .ForMember(dest => dest.CommentKey, opt => opt.MapFrom(src => BackendTexts.EntryComment))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => StatusToText(src.Status)))
            .ForMember(dest => dest.Reliability, opt => opt.MapFrom(src => $"{((int)src.Reliability) + 1} - {ReliabilityToText(src.Reliability)}"))
            .ForMember(dest => dest.TermdatClassifications, opt => opt.MapFrom<TermdatClassificationsResolver>())
            .AfterMap<ExportWatchlistEntryLanguageItemAction>();
    }

    private static string PictureToBase64String(Picture picture)
    {
        if (picture?.File is null)
        {
            return null;
        }

        return Convert.ToBase64String(picture.File.Value);
    }

    private static string StatusToText(Status status)
    {
        return status == Status.Validated ? BackendTexts.StatusValidated : BackendTexts.StatusInProgress;
    }

    private static string ReliabilityToText(Reliability reliability)
    {
        return reliability switch
        {
            Reliability.FirstDraft => BackendTexts.ReliabilityFirstDraft,
            Reliability.Tentative => BackendTexts.ReliabilityTentative,
            Reliability.Translated => BackendTexts.ReliabilityTranslated,
            Reliability.Verified => BackendTexts.ReliabilityVerified,
            Reliability.Approved => BackendTexts.ReliabilityApproved,
            _ => string.Empty
        };
    }
}
