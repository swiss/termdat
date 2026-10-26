using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Public.Api.Mapping.Actions;
using Bk.Termdat.Business.Public.Api.Mapping.Resolvers;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Public.Api.Mapping;

public class PublicApiMappingProfile : Profile
{
    public const string LanguageId = "languageId";
    public const string LanguageIsoCode = "languageIsoCode";

    public PublicApiMappingProfile()
    {
        AllowNullCollections = true;

        CreateClassificationMapping();

        CreateCollectionMapping();

        CreateOfficeMapping();

        CreateEntryMapping();

        CreateSearchParameterMapping();
    }

    private void CreateSearchParameterMapping()
    {
        CreateMap<SearchParameter, ResourceModels.SearchFilterParameter>()
            .ForMember(dest => dest.Phrase, opt => opt.MapFrom(src => src.SearchTerm))
            .ForMember(dest => dest.SourceLanguageIds, opt => opt.MapFrom(src => Array.Empty<int>()))
            .ForMember(dest => dest.EntryIds, opt => opt.MapFrom(src => Array.Empty<int>()))
            .ForMember(dest => dest.Offices, opt => opt.MapFrom(src => Array.Empty<int>()))
            .ForMember(dest => dest.OfficesPriority, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.LenochClassifications, opt => opt.MapFrom(src => Array.Empty<int>()))
            .ForMember(dest => dest.LenochClassificationPriority, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Collections, opt => opt.MapFrom(src => src.CollectionIds))
            .ForMember(dest => dest.CollectionsPriority, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.RelatedCollections, opt => opt.MapFrom(src => Array.Empty<int>()))
            .ForMember(dest => dest.Glossaries, opt => opt.MapFrom(src => Array.Empty<int>()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => new[] { ResourceModels.Status.Validated }))
            .ForMember(dest => dest.StatusPriority, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.AdminInfos, opt => opt.MapFrom(src => Array.Empty<string>()))
            .ForMember(dest => dest.AdminInfosPriority, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CommentStatus,
                opt => opt.MapFrom(src => Array.Empty<ResourceModels.CommentStatus>()))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByPriority, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedByPriority, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.ModifiedFrom, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedTo, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedPeriodPriority, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Reliability, opt => opt.MapFrom(src => Array.Empty<ResourceModels.Reliability>()))
            .ForMember(dest => dest.ReliabilityPriority, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Deleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedPriority, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.HasMissingSource, opt => opt.Ignore())
            .ForMember(dest => dest.PageIndex, opt => opt.MapFrom(src => 1))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.MaxEntryCount))
            .ForMember(dest => dest.SubjectDomains, opt => opt.Ignore())
            .ForMember(dest => dest.SubjectDomainsPriority, opt => opt.Ignore())
            .ForMember(dest => dest.CollectionsExclude, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.GlossariesExclude, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.AdminInfosExclude, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.CreatedByExclude, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.ModifiedByExclude, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.ModifiedPeriodExclude, opt => opt.MapFrom(_ => false));

        CreateMap<FieldSelection, ResourceModels.SearchFields>()
            .ForMember(dest => dest.Terminus, opt => opt.MapFrom(src => src.Terminus))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Abbreviation, opt => opt.MapFrom(src => src.Abbreviation))
            .ForMember(dest => dest.Phraseology, opt => opt.MapFrom(src => src.Phraseology))
            .ForMember(dest => dest.Definition, opt => opt.MapFrom(src => src.Definition))
            .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
            .ForMember(dest => dest.Context, opt => opt.MapFrom(src => src.Context))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            // The entry comment is internal and must not be searchable through the public API
            .ForMember(dest => dest.Comment, opt => opt.Ignore());
    }

    private void CreateEntryMapping()
    {
        CreateMap<DomainModels.Entry, EntryDetail>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Url, opt => opt.MapFrom<EntryDetailUrlResolver>())
            .ForMember(dest => dest.Status, opt => opt.MapFrom<EntryDetailStatusResolver>())
            .ForMember(dest => dest.Reliability, opt => opt.MapFrom<EntryDetailReliabilityResolver>())
            .ForMember(dest => dest.Office, opt => opt.MapFrom(src => src.Office))
            .ForMember(dest => dest.Collection, opt => opt.MapFrom(src => src.Collection))
            .ForMember(dest => dest.Classification, opt =>
            {
                opt.PreCondition(src => src.Collection.Classification != null);
                opt.MapFrom(src => src.Collection.Classification);
            })
            .ForMember(dest => dest.Subject, opt => opt.MapFrom((src, _, _, context) => CreateSubjects(src, context)))
            .ForMember(dest => dest.LanguageDetails, opt =>
            {
                opt.PreCondition(src => src.Collection.Translations != null);
                opt.MapFrom(src => src.Translations);
            })
            .ForMember(dest => dest.Hits, opt => opt.Ignore());

        CreateMap<DomainModels.EntryLanguage, EntryLanguageDetail>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.LanguageIsoCode, opt => opt.MapFrom(src => src.Language.IsoCode))
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
            .ForMember(dest => dest.References, opt => opt.Ignore())
            .AfterMap<EntryLanguageReferencesAction>();
    }

    private void CreateClassificationMapping()
    {
        CreateMap<DomainModels.Classification, ClassificationSummary>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Text, opt => opt.MapFrom((src, _, _, context) =>
            {
                if (context.Items.TryGetValue(LanguageId, out var languageId))
                {
                    return GetTranslation(src.Translations, (int)languageId);
                }

                throw new InvalidOperationException($"Cannot map without '{LanguageId}'.");
            }));
    }

    private void CreateOfficeMapping()
    {
        CreateMap<DomainModels.Office, OfficeSummary>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Text, opt => opt.MapFrom((src, _, _, context) =>
            {
                if (context.Items.TryGetValue(LanguageId, out var languageId))
                {
                    return GetTranslation(src.Translations, (int)languageId);
                }

                throw new InvalidOperationException($"Cannot map without '{LanguageId}'.");
            }));
    }

    private void CreateCollectionMapping()
    {
        CreateMap<DomainModels.Collection, CollectionSummary>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Text, opt => opt.MapFrom((src, _, _, context) =>
            {
                if (context.Items.TryGetValue(LanguageId, out var languageId))
                {
                    return GetTranslation(src.Translations, (int)languageId);
                }

                throw new InvalidOperationException($"Cannot map without '{LanguageId}'.");
            }));
    }

    private static string GetTranslation(IEnumerable<DomainModels.IEntityNameTranslation> srcTranslations, int languageId)
    {
        var translationList = srcTranslations.ToList();

        var translation = translationList.FirstOrDefault(t => t.LanguageId == languageId);

        return translation?.Name ?? string.Empty;
    }

    private static List<SubjectSummary> CreateSubjects(DomainModels.Entry entry, ResolutionContext context)
    {
        if (entry.Subjects == null && entry.Domains == null)
        {
#pragma warning disable S1168
            return null;
#pragma warning restore S1168
        }

        if (entry.Subjects == null || entry.Domains == null)
        {
            throw new AutoMapperMappingException(
                "Can only map entry subjects, when both subjects and domains are set or both are null.");
        }
        if (!context.Items.TryGetValue(LanguageId, out var languageIdValue))
        {
            throw new InvalidOperationException($"Cannot map without '{LanguageId}'.");
        }

        var languageId = (int)languageIdValue;

        var subjects = new List<SubjectSummary>();

        foreach (var subject in entry.Subjects)
        {
            var domainForThisSubject = entry.Domains.Where(d => d.SubjectId == subject.Id);

            subjects.Add(new SubjectSummary
            {
                Id = subject.Id,
                Text = GetTranslation(subject.Translations, languageId),
                Domains = domainForThisSubject.Select(d => new DomainSummary
                {
                    Id = d.Id,
                    Text = GetTranslation(d.Translations, languageId)
                }).ToList()
            });
        }

        return subjects;
    }
}
