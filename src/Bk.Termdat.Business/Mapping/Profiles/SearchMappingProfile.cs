using System;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Mapping.Actions;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping.Profiles;

public class SearchMappingProfile : Profile
{
    public SearchMappingProfile()
    {
        CreateSearchFieldMapping();
        CreateSearchFilterParameterMapping();
    }

    private void CreateSearchFilterParameterMapping()
    {
        CreateMap<ResourceModels.SearchQuery, ResourceModels.SearchFilterParameter>()
            .ForMember(dest => dest.Phrase, opt => opt.MapFrom(src => src.Phrase))
            .ForMember(dest => dest.SourceLanguageIds, opt => opt.MapFrom(src => src.SourceLanguageIds))
            .ForMember(dest => dest.PageIndex, opt => opt.MapFrom(src => src.PageIndex))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.EntryIds, opt => opt.MapFrom(src => SplitEntryIds(src.EntryIds)))
            .ForMember(dest => dest.Offices, opt => opt.MapFrom(src => src.Offices))
            .ForMember(dest => dest.OfficesPriority, opt => opt.MapFrom(src => src.OfficesPriority))
            .ForMember(dest => dest.LenochClassifications, opt => opt.MapFrom(src => src.LenochClassifications))
            .ForMember(dest => dest.LenochClassificationPriority, opt => opt.MapFrom(src => src.LenochClassificationsPriority))
            .ForMember(dest => dest.Collections, opt => opt.MapFrom(src => src.Collections))
            .ForMember(dest => dest.CollectionsPriority, opt => opt.MapFrom(src => src.CollectionsPriority))
            .ForMember(dest => dest.SubjectDomains, opt => opt.MapFrom(src => src.SubjectDomains))
            .ForMember(dest => dest.SubjectDomainsPriority, opt => opt.MapFrom(src => src.SubjectDomainsPriority))
            .ForMember(dest => dest.RelatedCollections, opt => opt.MapFrom(src => src.RelatedCollections))
            .ForMember(dest => dest.Glossaries, opt => opt.MapFrom(src => src.Glossaries))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.StatusPriority, opt => opt.MapFrom(src => src.StatusPriority))
            .ForMember(dest => dest.AdminInfos, opt => opt.MapFrom(src => src.AdminInfos))
            .ForMember(dest => dest.AdminInfosPriority, opt => opt.MapFrom(src => src.AdminInfosPriority))
            .ForMember(dest => dest.CommentStatus, opt => opt.MapFrom(src => src.CommentStatus))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.ToLowerInvariant()))
            .ForMember(dest => dest.CreatedByPriority, opt => opt.MapFrom(src => src.CreatedByPriority))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy.ToLowerInvariant()))
            .ForMember(dest => dest.ModifiedByPriority, opt => opt.MapFrom(src => src.ModifiedByPriority))
            .ForMember(dest => dest.ModifiedFrom, opt => opt.MapFrom(src => src.ModifiedFrom))
            .ForMember(dest => dest.ModifiedTo, opt => opt.MapFrom(src => src.ModifiedTo))
            .ForMember(dest => dest.ModifiedPeriodPriority, opt => opt.MapFrom(src => src.ModifiedPeriodPriority))
            .ForMember(dest => dest.Reliability, opt => opt.MapFrom(src => src.Reliability))
            .ForMember(dest => dest.ReliabilityPriority, opt => opt.MapFrom(src => src.ReliabilityPriority))
            .ForMember(dest => dest.CollectionsExclude, opt => opt.MapFrom(src => src.CollectionsExclude))
            .ForMember(dest => dest.GlossariesExclude, opt => opt.MapFrom(src => src.GlossariesExclude))
            .ForMember(dest => dest.AdminInfosExclude, opt => opt.MapFrom(src => src.AdminInfosExclude))
            .ForMember(dest => dest.CreatedByExclude, opt => opt.MapFrom(src => src.CreatedByExclude))
            .ForMember(dest => dest.ModifiedByExclude, opt => opt.MapFrom(src => src.ModifiedByExclude))
            .ForMember(dest => dest.ModifiedPeriodExclude, opt => opt.MapFrom(src => src.ModifiedPeriodExclude))
            .ForMember(dest => dest.Deleted, opt => opt.MapFrom(src => src.Deleted))
            .ForMember(dest => dest.DeletedPriority, opt => opt.MapFrom(src => src.DeletedPriority))
            .ForMember(dest => dest.HasMissingSource, opt => opt.MapFrom(src => src.HasMissingSource))
            .BeforeMap<PopulateModifiedAndCreatedAction>();
    }

    private void CreateSearchFieldMapping()
    {
        CreateMap<ResourceModels.SearchQuery, ResourceModels.SearchFields>()
            .ForMember(dest => dest.Terminus, opt =>
            {
                opt.PreCondition(src => src.Fields != null);
                opt.MapFrom(src => src.Fields.Term);
            })
            .ForMember(dest => dest.Name, opt =>
            {
                opt.PreCondition(src => src.Fields != null);
                opt.MapFrom(src => src.Fields.Name);
            })
            .ForMember(dest => dest.Abbreviation, opt =>
            {
                opt.PreCondition(src => src.Fields != null);
                opt.MapFrom(src => src.Fields.Abbreviation);
            })
            .ForMember(dest => dest.Phraseology, opt =>
            {
                opt.PreCondition(src => src.Fields != null);
                opt.MapFrom(src => src.Fields.Phraseology);
            })
            .ForMember(dest => dest.Definition, opt =>
            {
                opt.PreCondition(src => src.Fields != null);
                opt.MapFrom(src => src.Fields.Definition);
            })
            .ForMember(dest => dest.Note, opt =>
            {
                opt.PreCondition(src => src.Fields != null);
                opt.MapFrom(src => src.Fields.Note);
            })
            .ForMember(dest => dest.Context, opt =>
            {
                opt.PreCondition(src => src.Fields != null);
                opt.MapFrom(src => src.Fields.Context);
            })
            .ForMember(dest => dest.Source, opt =>
            {
                opt.PreCondition(src => src.Fields != null);
                opt.MapFrom(src => src.Fields.Source);
            })
            .ForMember(dest => dest.Metadata, opt =>
            {
                opt.PreCondition(src => src.Fields != null);
                opt.MapFrom(src => src.Fields.Metadata);
            })
            .ForMember(dest => dest.Country, opt =>
            {
                opt.PreCondition(src => src.Fields != null);
                opt.MapFrom(src => src.Fields.Country);
            })
            .ForMember(dest => dest.Comment, opt =>
            {
                opt.PreCondition(src => src.Fields != null);
                opt.MapFrom(src => src.Fields.Comment);
            });
    }

    private int[] SplitEntryIds(string entryIds)
    {
        if (!string.IsNullOrEmpty(entryIds))
        {
            entryIds = entryIds.Replace(" ", "");
            var allIds = entryIds.Split(",").ToArray();
            var allIdsAsNumbers = Array.ConvertAll(allIds, int.Parse);
            return allIdsAsNumbers;
        }
        return null;
    }
}
