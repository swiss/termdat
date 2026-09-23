using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Mapping.Resolvers;
using Bk.Termdat.CrossCutting;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping.Profiles;

public class SharedMappingProfile : Profile
{
    public SharedMappingProfile()
    {
        CreateLanguageMapping();
        CreateOfficeSummaryMapping();
        CreateLenochClassificationSummaryMapping();
        CreateClassificationSummaryMapping();
        CreateCollectionSummaryMapping();
        CreateConceptSystemSummaryMapping();
        CreateConceptSystemPositionSummaryMapping();
        CreateUserAccountMapping();
        CreateGlossaryMapping();
        CreateSubjectMapping();
        CreateDomainMapping();
        CreateAdminInfoMapping();
        CreateSourceTemplateMapping();
        CreateNewsItemSummaryMapping();
    }

    private void CreateSourceTemplateMapping()
    {
        CreateMap<ResourceModels.SourceTemplateModification, DomainModels.SourceTemplate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TextDe, opt => opt.MapFrom(src => src.TextDe))
            .ForMember(dest => dest.TextFr, opt => opt.MapFrom(src => src.TextFr))
            .ForMember(dest => dest.TextIt, opt => opt.MapFrom(src => src.TextIt))
            .ForMember(dest => dest.TextEn, opt => opt.MapFrom(src => src.TextEn))
            .ForMember(dest => dest.TextRm, opt => opt.MapFrom(src => src.TextRm))
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore());

        CreateMap<DomainModels.SourceTemplate, ResourceModels.SourceTemplateModification>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TextDe, opt => opt.MapFrom(src => src.TextDe))
            .ForMember(dest => dest.TextFr, opt => opt.MapFrom(src => src.TextFr))
            .ForMember(dest => dest.TextIt, opt => opt.MapFrom(src => src.TextIt))
            .ForMember(dest => dest.TextEn, opt => opt.MapFrom(src => src.TextEn))
            .ForMember(dest => dest.TextRm, opt => opt.MapFrom(src => src.TextRm));

        CreateMap<DomainModels.SourceTemplate, ResourceModels.SourceTemplateSummary>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TextDe, opt => opt.MapFrom(src => src.TextDe))
            .ForMember(dest => dest.TextFr, opt => opt.MapFrom(src => src.TextFr))
            .ForMember(dest => dest.TextIt, opt => opt.MapFrom(src => src.TextIt))
            .ForMember(dest => dest.TextEn, opt => opt.MapFrom(src => src.TextEn))
            .ForMember(dest => dest.TextRm, opt => opt.MapFrom(src => src.TextRm));
    }

    private void CreateSubjectMapping()
    {
        CreateMap<DomainModels.Subject, ResourceModels.SubjectSummary>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.Subject, ResourceModels.SubjectSummary>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations))
            .ForMember(dest => dest.Domains, opt => opt.MapFrom(src => src.Domains));
    }

    private void CreateDomainMapping()
    {
        CreateMap<DomainModels.Domain, ResourceModels.DomainSummary>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.SubjectId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.Domain, ResourceModels.DomainSummary>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations));
    }

    private void CreateGlossaryMapping()
    {
        CreateMap<DomainModels.Glossary, ResourceModels.GlossarySummary>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.Glossary, ResourceModels.GlossarySummary>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations));
    }

    private void CreateUserAccountMapping()
    {
        CreateMap<DomainModels.UserAccount, ResourceModels.UserAccountSummary>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName));
    }

    private void CreateClassificationSummaryMapping()
    {
        CreateMap<DomainModels.Classification, ResourceModels.ClassificationSummary>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.Classification, ResourceModels.ClassificationSummary>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations));
    }

    private void CreateLenochClassificationSummaryMapping()
    {
        CreateMap<DomainModels.LenochClassification, ResourceModels.LenochClassificationSummary>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.LenochClassification, ResourceModels.LenochClassificationSummary>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations));
    }

    private void CreateOfficeSummaryMapping()
    {
        CreateMap<DomainModels.Office, ResourceModels.OfficeSummary>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.Office, ResourceModels.OfficeSummary>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations));
    }

    private void CreateConceptSystemSummaryMapping()
    {
        CreateMap<DomainModels.ConceptSystem, ResourceModels.ConceptSystemSummary>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));
    }

    private void CreateConceptSystemPositionSummaryMapping()
    {
        CreateMap<DomainModels.ConceptSystemPosition, ResourceModels.ConceptSystemPositionSummary>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));
    }

    private void CreateAdminInfoMapping()
    {
        CreateMap<DomainModels.AdminInfo, ResourceModels.AdminInfoSummary>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
    }

    private void CreateLanguageMapping()
    {
        CreateMap<DomainModels.LanguageTranslation, ResourceModels.LanguageSummary>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.LanguageId))
            .ForMember(dest => dest.IsUiLanguage, opt => opt.MapFrom(src => src.Language.IsUiLanguage))
            .ForMember(dest => dest.IsoCode, opt => opt.MapFrom(src => src.Language.IsoCode))
            .ForMember(dest => dest.SortIndex, opt => opt.MapFrom(src => src.Language.SortIndex));

        CreateMap<DomainModels.Language, ResourceModels.LanguageDetail>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IsUiLanguage, opt => opt.MapFrom(src => src.IsUiLanguage))
            .ForMember(dest => dest.IsoCode, opt => opt.MapFrom(src => src.IsoCode))
            .ForMember(dest => dest.SortIndex, opt => opt.MapFrom(src => src.SortIndex))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<LanguageTranslationResolver<DomainModels.Language, ResourceModels.LanguageDetail>, IEnumerable<DomainModels.LanguageTranslation>>(src => src.Translations))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy));

        CreateMap<ResourceModels.ILanguageModification, DomainModels.Language>()
            .ForMember(dest => dest.IsUiLanguage, opt => opt.MapFrom(src => src.IsUiLanguage))
            .ForMember(dest => dest.IsSystemLanguage, opt => opt.Ignore())
            .ForMember(dest => dest.IsoCode, opt => opt.MapFrom(src => src.IsoCode.ToUpper()))
            .ForMember(dest => dest.Translations, opt => opt.Ignore())
            .ForMember(dest => dest.SortIndex, opt => opt.MapFrom(src => src.SortIndex))
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<ResourceModels.LanguageCreate, DomainModels.Language>()
            .IncludeBase<ResourceModels.ILanguageModification, DomainModels.Language>();

        CreateMap<ResourceModels.LanguageUpdate, DomainModels.Language>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .IncludeBase<ResourceModels.ILanguageModification, DomainModels.Language>();

        CreateMap<DomainModels.Language, ResourceModels.LanguageUpdate>()
            .ForMember(dest => dest.IsUiLanguage, opt => opt.MapFrom(src => src.IsUiLanguage))
            .ForMember(dest => dest.IsoCode, opt => opt.MapFrom(src => src.IsoCode))
            .ForMember(dest => dest.SortIndex, opt => opt.MapFrom(src => src.SortIndex))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DescriptionDe, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionFr, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionEn, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionIt, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionRm, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                dest.DescriptionDe = src.Translations.FirstOrDefault(t => IsoCode.De.IsEqualTo(t.IsoCode))?.Name;
                dest.DescriptionFr = src.Translations.FirstOrDefault(t => IsoCode.Fr.IsEqualTo(t.IsoCode))?.Name;
                dest.DescriptionIt = src.Translations.FirstOrDefault(t => IsoCode.It.IsEqualTo(t.IsoCode))?.Name;
                dest.DescriptionRm = src.Translations.FirstOrDefault(t => IsoCode.Rm.IsEqualTo(t.IsoCode))?.Name;
                dest.DescriptionEn = src.Translations.FirstOrDefault(t => IsoCode.En.IsEqualTo(t.IsoCode))?.Name;
            });
    }

    private void CreateCollectionSummaryMapping()
    {
        CreateMap<DomainModels.Collection, ResourceModels.CollectionSummary>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.Collection, ResourceModels.CollectionSummary>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations))
            .ForMember(dest => dest.IsObsolete, opt => opt.MapFrom(src => src.IsObsolete))
            .ForMember(dest => dest.Deleted, opt => opt.MapFrom(src => src.Deleted))
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active))
            .ForMember(dest => dest.ActiveApi, opt => opt.MapFrom(src => src.ActiveApi))
            .ForMember(dest => dest.ExportLindas, opt => opt.MapFrom(src => src.ExportLindas))
            .ForMember(dest => dest.Classification, opt =>
            {
                opt.PreCondition(src => src.Classification != null);
                opt.MapFrom<TranslationResolver<DomainModels.Collection, ResourceModels.CollectionSummary>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Classification.Translations);
            });
    }

    private void CreateNewsItemSummaryMapping()
    {
        CreateMap<DomainModels.NewsItem, ResourceModels.NewsItemSummary>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled))
            .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src => src.ValidTo))
            .ForMember(dest => dest.SortIndex, opt => opt.MapFrom(src => src.SortIndex))
            .ForMember(dest => dest.Title, opt => opt.MapFrom<NewsItemTitleResolver<DomainModels.NewsItem, ResourceModels.NewsItemSummary>, IEnumerable<DomainModels.NewsItemTranslation>>(src => src.Translations))
            .ForMember(dest => dest.Text, opt => opt.MapFrom<NewsItemTextResolver<DomainModels.NewsItem, ResourceModels.NewsItemSummary>, IEnumerable<DomainModels.NewsItemTranslation>>(src => src.Translations))
            .ForMember(dest => dest.CanMoveUp, opt => opt.Ignore())
            .ForMember(dest => dest.CanMoveDown, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy));

        CreateMap<DomainModels.NewsItem, ResourceModels.NewsItemDetail>()
           .IncludeBase<DomainModels.NewsItem, ResourceModels.NewsItemSummary>()
            .ForMember(dest => dest.TitleDe, opt => opt.Ignore())
            .ForMember(dest => dest.TitleFr, opt => opt.Ignore())
            .ForMember(dest => dest.TitleIt, opt => opt.Ignore())
            .ForMember(dest => dest.TitleRm, opt => opt.Ignore())
            .ForMember(dest => dest.TitleEn, opt => opt.Ignore())
            .ForMember(dest => dest.TextDe, opt => opt.Ignore())
            .ForMember(dest => dest.TextFr, opt => opt.Ignore())
            .ForMember(dest => dest.TextIt, opt => opt.Ignore())
            .ForMember(dest => dest.TextRm, opt => opt.Ignore())
            .ForMember(dest => dest.TextEn, opt => opt.Ignore());

        CreateMap<ResourceModels.NewsItemCreate, DomainModels.NewsItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.Translations, opt => opt.Ignore())
            .ForMember(dest => dest.SortIndex, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore());

        CreateMap<ResourceModels.NewsItemUpdate, DomainModels.NewsItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled))
            .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src => src.ValidTo))
            .ForMember(dest => dest.SortIndex, opt => opt.MapFrom(src => src.SortIndex))
            .ForMember(dest => dest.Translations, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore());

        CreateMap<DomainModels.NewsItem, ResourceModels.NewsItemUpdate>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TitleDe, opt => opt.Ignore())
            .ForMember(dest => dest.TitleFr, opt => opt.Ignore())
            .ForMember(dest => dest.TitleIt, opt => opt.Ignore())
            .ForMember(dest => dest.TitleRm, opt => opt.Ignore())
            .ForMember(dest => dest.TitleEn, opt => opt.Ignore())
            .ForMember(dest => dest.TextDe, opt => opt.Ignore())
            .ForMember(dest => dest.TextFr, opt => opt.Ignore())
            .ForMember(dest => dest.TextIt, opt => opt.Ignore())
            .ForMember(dest => dest.TextRm, opt => opt.Ignore())
            .ForMember(dest => dest.TextEn, opt => opt.Ignore());
    }
}
