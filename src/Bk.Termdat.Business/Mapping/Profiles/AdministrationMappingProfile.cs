using System.Collections.Generic;
using AutoMapper;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Mapping.Resolvers;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Mapping.Profiles;

public class AdministrationMappingProfile : Profile
{
    public AdministrationMappingProfile()
    {
        CreateCollectionMapping();
        CreateOfficeMapping();
        CreateGlossaryMapping();
        CreateLenochClassificationMapping();
        CreateSubjectDomainMapping();
        CreateConceptSystemMapping();
        CreateUserAccountMapping();
        CreateAdminInfoMapping();
        CreateStatisticMapping();
    }

    private void CreateUserAccountMapping()
    {
        CreateMap<DomainModels.UserAccount, ResourceModels.UserAccountDetail>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.HasRole, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.Role)))
            .ForMember(dest => dest.Role, opt => opt.MapFrom<RoleValueResolver>())
            .ForMember(dest => dest.LastAction, opt => opt.MapFrom(src => src.LastAction))
            .ForMember(dest => dest.Information, opt => opt.MapFrom(src => src.Information))
            .ForMember(dest => dest.Glossaries, opt => opt.MapFrom(src => src.AuthorizedGlossaries))
            .ForMember(dest => dest.Collections, opt => opt.MapFrom(src => src.AuthorizedCollections))
            .ForMember(dest => dest.HasFullAccess, opt => opt.MapFrom<HasFullAccessResolver>())
            .ForMember(dest => dest.IsWriter, opt => opt.MapFrom<IsWriterResolver>())
            .ForMember(dest => dest.IsSuperReader, opt => opt.MapFrom<IsSuperReaderResolver>());

        CreateMap<ResourceModels.IUserAccountModification, DomainModels.UserAccount>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Information, opt => opt.MapFrom(src => src.Information))
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.LastName, opt => opt.Ignore())
            .ForMember(dest => dest.FirstName, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.LastAction, opt => opt.Ignore())
            .ForMember(dest => dest.LastLogin, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorizedGlossaries, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorizedCollections, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore());
    }

    private void CreateConceptSystemMapping()
    {
        CreateMap<ConceptSystemWithEntryCount, ResourceModels.ConceptSystemDetail>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ConceptSystem.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.ConceptSystem.Title))
            .ForMember(dest => dest.Collection, opt => opt.MapFrom(src => src.ConceptSystem.Collection.Code))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.ConceptSystem.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.ConceptSystem.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.ConceptSystem.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ConceptSystem.ModifiedBy))
            .ForMember(dest => dest.EntryCount, opt => opt.MapFrom(src => src.EntryCount));

        CreateMap<DomainModels.ConceptSystem, ResourceModels.ConceptSystemDetail>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Collection, opt => opt.MapFrom(src => src.Collection.Code))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy))
            .ForMember(dest => dest.EntryCount, opt => opt.Ignore());

        CreateMap<DomainModels.ConceptSystem, ResourceModels.ConceptSystemUpdate>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.CollectionId, opt => opt.MapFrom(src => src.Collection.Id))
            .ForMember(dest => dest.Positions, opt => opt.MapFrom(src => src.ConceptSystemPositions));

        CreateMap<ResourceModels.ConceptSystemUpdate, DomainModels.ConceptSystem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.ConceptSystemPositions, opt => opt.MapFrom(src => src.Positions))
            .ForMember(dest => dest.Collection, opt => opt.Ignore())
            .ForMember(dest => dest.CollectionId, opt => opt.MapFrom(src => src.CollectionId))
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore());

        CreateMap<ResourceModels.ConceptSystemCreate, DomainModels.ConceptSystem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.ConceptSystemPositions, opt => opt.MapFrom(src => src.Positions))
            .ForMember(dest => dest.Collection, opt => opt.Ignore())
            .ForMember(dest => dest.CollectionId, opt => opt.MapFrom(src => src.CollectionId))
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore());

        CreateMap<ResourceModels.IConceptSystemPositionModification, DomainModels.ConceptSystemPosition>()
            .ForMember(dest => dest.SortIndex, opt => opt.Ignore())
            .ForMember(dest => dest.ConceptSystemId, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.ConceptSystem, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

        CreateMap<ResourceModels.ConceptSystemPositionUpdate, DomainModels.ConceptSystemPosition>()
            .IncludeBase<ResourceModels.IConceptSystemPositionModification, DomainModels.ConceptSystemPosition>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        CreateMap<ResourceModels.ConceptSystemPositionCreate, DomainModels.ConceptSystemPosition>()
            .IncludeBase<ResourceModels.IConceptSystemPositionModification, DomainModels.ConceptSystemPosition>();

        CreateMap<DomainModels.ConceptSystemPosition, ResourceModels.ConceptSystemPositionUpdate>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));
    }

    private void CreateLenochClassificationMapping()
    {
        CreateMap<EntityWithEntryCount<DomainModels.LenochClassification>, ResourceModels.LenochClassificationDetail>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Entity.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Entity.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Entity.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<EntityWithEntryCount<DomainModels.LenochClassification>, ResourceModels.LenochClassificationDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Entity.Translations))
            .ForMember(dest => dest.EntryCount, opt => opt.MapFrom(src => src.EntryCount))
            .ForMember(dest => dest.Subject, opt => opt.Ignore())
            .ForMember(dest => dest.Domain, opt => opt.Ignore());

        CreateMap<DomainModels.LenochClassificationSubjectDomainMapping, ResourceModels.LenochClassificationDetail>()
            .BeforeMap((src, _) =>
            {
                MappingUtil.AssertTranslationsLanguageNotNull(src.Subject.Translations);
                if (src.Domain != null)
                {
                    MappingUtil.AssertTranslationsLanguageNotNull(src.Domain.Translations);
                }
                if (src.Domain != null)
                {
                    MappingUtil.AssertTranslationsLanguageNotNull(src.Domain.Translations);
                }
            })
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.LenochClassificationId))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.LenochClassification != null ? src.LenochClassification.Code : null))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.LenochClassificationSubjectDomainMapping, ResourceModels.LenochClassificationDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.LenochClassification != null ? src.LenochClassification.Translations : null))
            .ForMember(dest => dest.EntryCount, opt => opt.Ignore())
            .ForMember(dest => dest.Subject, opt => opt.MapFrom<TranslationResolver<DomainModels.LenochClassificationSubjectDomainMapping, ResourceModels.LenochClassificationDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Subject.Translations))
            .ForMember(dest => dest.Domain, opt => opt.MapFrom<TranslationResolver<DomainModels.LenochClassificationSubjectDomainMapping, ResourceModels.LenochClassificationDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Domain != null ? src.Domain.Translations : null));
    }

    private void CreateGlossaryMapping()
    {
        CreateMap<GlossaryWithEntryCount, ResourceModels.GlossaryDetail>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Glossary.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Glossary.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Glossary.Code))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Glossary.Description))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<GlossaryWithEntryCount, ResourceModels.GlossaryDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Glossary.Translations))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.Glossary.CreatedBy))
            .ForMember(dest => dest.EntryCount, opt => opt.MapFrom(src => src.EntryCount));

        CreateMap<DomainModels.Glossary, ResourceModels.GlossaryDetail>()
            .IncludeBase<DomainModels.Glossary, ResourceModels.GlossarySummary>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.Glossary, ResourceModels.GlossaryDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.EntryCount, opt => opt.Ignore());

        CreateMap<ResourceModels.IGlossaryModification, DomainModels.Glossary>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Translations, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserAccounts, opt => opt.Ignore());

        CreateMap<ResourceModels.GlossaryCreate, DomainModels.Glossary>()
            .IncludeBase<ResourceModels.IGlossaryModification, DomainModels.Glossary>();

        CreateMap<ResourceModels.GlossaryUpdate, DomainModels.Glossary>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .IncludeBase<ResourceModels.IGlossaryModification, DomainModels.Glossary>();

        CreateMap<DomainModels.Glossary, ResourceModels.GlossaryUpdate>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy))
            .ForMember(dest => dest.DescriptionDe, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionFr, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionEn, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionIt, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionRm, opt => opt.Ignore())
            .AfterMap((src, dest) => LanguageDescriptionMapper.MapTranslationsToLanguageDescription(dest, src.Translations));
    }

    private void CreateOfficeMapping()
    {
        CreateMap<OfficeWithEntryCount, ResourceModels.OfficeDetail>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Office.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Office.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Office.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<OfficeWithEntryCount, ResourceModels.OfficeDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Office.Translations))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Office.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.Office.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Office.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.Office.ModifiedBy))
            .ForMember(dest => dest.EntryCount, opt => opt.MapFrom(src => src.EntryCount));

        CreateMap<DomainModels.Office, ResourceModels.OfficeUpdate>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.DescriptionDe, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionEn, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionFr, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionIt, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionRm, opt => opt.Ignore())
            .AfterMap((src, dest) => LanguageDescriptionMapper.MapTranslationsToLanguageDescription(dest, src.Translations));

        CreateMap<DomainModels.Office, ResourceModels.OfficeDetail>()
           .IncludeBase<DomainModels.Office, ResourceModels.OfficeSummary>()
           .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
           .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
           .ForMember(dest => dest.EntryCount, opt => opt.MapFrom(src => src.Entries.Count))
           .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified))
           .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy));

        CreateMap<ResourceModels.IOfficeModification, DomainModels.Office>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Translations, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<ResourceModels.OfficeUpdate, DomainModels.Office>()
          .IncludeBase<ResourceModels.IOfficeModification, DomainModels.Office>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        CreateMap<ResourceModels.OfficeCreate, DomainModels.Office>()
            .IncludeBase<ResourceModels.IOfficeModification, DomainModels.Office>();
    }

    private void CreateCollectionMapping()
    {
        CreateMap<CollectionWithEntryCount, ResourceModels.CollectionDetail>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Collection.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Collection.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Collection.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<CollectionWithEntryCount, ResourceModels.CollectionDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Collection.Translations))
            .ForMember(dest => dest.IsObsolete, opt => opt.MapFrom(src => src.Collection.IsObsolete))
            .ForMember(dest => dest.Classification, opt =>
            {
                opt.PreCondition(src => src.Collection.Classification != null);
                opt.MapFrom<TranslationResolver<CollectionWithEntryCount, ResourceModels.CollectionDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Collection.Classification.Translations);
            })
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Collection.Active))
            .ForMember(dest => dest.ActiveApi, opt => opt.MapFrom(src => src.Collection.ActiveApi))
            .ForMember(dest => dest.ExportLindas, opt => opt.MapFrom(src => src.Collection.ExportLindas))
            .ForMember(dest => dest.ConceptSystem, opt => opt.MapFrom(src => src.Collection.ConceptSystem))
            .ForMember(dest => dest.Deleted, opt => opt.MapFrom(src => src.Collection.Deleted))
            .ForMember(dest => dest.EntryCount, opt => opt.MapFrom(src => src.EntryCount));

        CreateMap<DomainModels.Collection, ResourceModels.CollectionDetail>()
            .IncludeBase<DomainModels.Collection, ResourceModels.CollectionSummary>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.Collection, ResourceModels.CollectionDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations))
            .ForMember(dest => dest.IsObsolete, opt => opt.MapFrom(src => src.IsObsolete))
            .ForMember(dest => dest.Classification, opt =>
            {
                opt.PreCondition(src => src.Classification != null);
                opt.MapFrom<TranslationResolver<DomainModels.Collection, ResourceModels.CollectionDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations);
            })
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active))
            .ForMember(dest => dest.ActiveApi, opt => opt.MapFrom(src => src.ActiveApi))
            .ForMember(dest => dest.ExportLindas, opt => opt.MapFrom(src => src.ExportLindas))
            .ForMember(dest => dest.ConceptSystem, opt => opt.MapFrom(src => src.ConceptSystem))
            .ForMember(dest => dest.EntryCount, opt => opt.Ignore());

        CreateMap<ResourceModels.ICollectionModification, DomainModels.Collection>()
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active))
            .ForMember(dest => dest.ActiveApi, opt => opt.MapFrom(src => src.ActiveApi))
            .ForMember(dest => dest.ExportLindas, opt => opt.MapFrom(src => src.ExportLindas))
            .ForMember(dest => dest.ClassificationId, opt => opt.MapFrom(src => src.ClassificationId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Documentation, opt => opt.MapFrom(src => src.Documentation))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Classification, opt => opt.Ignore())
            .ForMember(dest => dest.ConceptSystem, opt => opt.Ignore())
            .ForMember(dest => dest.Translations, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.RelatedEntries, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Deleted, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.IsObsolete, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserAccounts, opt => opt.Ignore());

        CreateMap<ResourceModels.CollectionCreate, DomainModels.Collection>()
            .IncludeBase<ResourceModels.ICollectionModification, DomainModels.Collection>();

        CreateMap<ResourceModels.CollectionUpdate, DomainModels.Collection>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .IncludeBase<ResourceModels.ICollectionModification, DomainModels.Collection>();

        CreateMap<DomainModels.Collection, ResourceModels.CollectionUpdate>()
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active))
            .ForMember(dest => dest.ActiveApi, opt => opt.MapFrom(src => src.ActiveApi))
            .ForMember(dest => dest.ExportLindas, opt => opt.MapFrom(src => src.ExportLindas))
            .ForMember(dest => dest.ClassificationId, opt => opt.MapFrom(src => src.ClassificationId))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Documentation, opt => opt.MapFrom(src => src.Documentation))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy))
            .ForMember(dest => dest.DescriptionDe, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionFr, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionEn, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionIt, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionRm, opt => opt.Ignore())
            .ForMember(dest => dest.IsObsolete, opt => opt.Ignore())
            .ForMember(dest => dest.ConceptSystem, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                if (src.ConceptSystem != null)
                {
                    dest.ConceptSystem = new ResourceModels.ConceptSystemSummary
                    {
                        Id = src.ConceptSystem.Id,
                        Title = src.ConceptSystem.Title
                    };
                }

                LanguageDescriptionMapper.MapTranslationsToLanguageDescription(dest, src.Translations);
            });
    }

    private void CreateSubjectDomainMapping()
    {
        CreateMap<DomainWithEntryCount, ResourceModels.DomainDetail>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Domain.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Domain.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainWithEntryCount, ResourceModels.DomainDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Domain.Translations))
            .ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.Domain.SubjectId))
            .ForMember(dest => dest.EntryCount, opt => opt.MapFrom(src => src.EntryCount))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Domain.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.Domain.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Domain.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.Domain.ModifiedBy));

        CreateMap<DomainModels.Domain, ResourceModels.DomainDetail>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.Domain, ResourceModels.DomainDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations))
            .ForMember(dest => dest.EntryCount, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy));

        CreateMap<DomainModels.Domain, ResourceModels.DomainModification>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.SubjectId))
            .ForMember(dest => dest.DescriptionIt, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionEn, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionDe, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionFr, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionRm, opt => opt.Ignore())
            .AfterMap((src, dest) => LanguageDescriptionMapper.MapTranslationsToLanguageDescription(dest, src.Translations));

        CreateMap<ResourceModels.DomainModification, DomainModels.Domain>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.SubjectId))
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Translations, opt => opt.Ignore())
            .ForMember(dest => dest.Subject, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.LenochClassificationSubjectDomainMappings, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore());

        CreateMap<SubjectWithEntryCount, ResourceModels.SubjectDetail>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Subject.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Subject.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<SubjectWithEntryCount, ResourceModels.SubjectDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Subject.Translations))
            .ForMember(dest => dest.Domains, opt => opt.MapFrom(src => src.Subject.Domains))
            .ForMember(dest => dest.EntryCount, opt => opt.MapFrom(src => src.EntryCount))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Subject.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.Subject.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Subject.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.Subject.ModifiedBy));

        CreateMap<DomainModels.Subject, ResourceModels.SubjectDetail>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom<TranslationResolver<DomainModels.Subject, ResourceModels.SubjectDetail>, IEnumerable<DomainModels.IEntityNameTranslation>>(src => src.Translations))
            .ForMember(dest => dest.EntryCount, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy));

        CreateMap<ResourceModels.SubjectModification, DomainModels.Subject>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Translations, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Domains, opt => opt.MapFrom(src => src.Domains))
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.LenochClassificationSubjectDomainMappings, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore());

        CreateMap<DomainModels.Subject, ResourceModels.SubjectModification>()
            .BeforeMap((src, _) => MappingUtil.AssertTranslationsLanguageNotNull(src.Translations))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DescriptionDe, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionEn, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionIt, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionFr, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionRm, opt => opt.Ignore())
            .AfterMap((src, dest) => LanguageDescriptionMapper.MapTranslationsToLanguageDescription(dest, src.Translations));
    }

    private void CreateAdminInfoMapping()
    {
        CreateMap<AdminInfoWithEntryCount, ResourceModels.AdminInfoDetail>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AdminInfo.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.AdminInfo.Code))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.AdminInfo.Description))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.AdminInfo.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.AdminInfo.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.AdminInfo.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.AdminInfo.ModifiedBy))
            .ForMember(dest => dest.EntryCount, opt => opt.MapFrom(src => src.EntryCount));

        CreateMap<ResourceModels.AdminInfoSummary, ResourceModels.AdminInfoCreate>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

        CreateMap<ResourceModels.AdminInfoSummary, DomainModels.AdminInfo>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore());

        CreateMap<DomainModels.AdminInfo, ResourceModels.AdminInfoDetail>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy))
            .ForMember(dest => dest.EntryCount, opt => opt.Ignore());

        CreateMap<DomainModels.AdminInfo, ResourceModels.AdminInfoUpdate>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

        CreateMap<ResourceModels.IAdminInfoModification, DomainModels.AdminInfo>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code.Trim()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<ResourceModels.AdminInfoUpdate, DomainModels.AdminInfo>()
          .IncludeBase<ResourceModels.IAdminInfoModification, DomainModels.AdminInfo>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        CreateMap<ResourceModels.AdminInfoCreate, DomainModels.AdminInfo>()
            .IncludeBase<ResourceModels.IAdminInfoModification, DomainModels.AdminInfo>();
    }

    private void CreateStatisticMapping()
    {
        CreateMap<ResourceModels.StatisticSummary, DomainModels.Statistic>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => src.SearchText))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.Languages))
            .ForMember(dest => dest.SearchFields, opt => opt.MapFrom(src => src.SearchFields))
            .ForMember(dest => dest.DateLastSearch, opt => opt.MapFrom(src => src.DateLastSearch))
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore());

        CreateMap<ResourceModels.StatisticSummary, ResourceModels.StatisticCreate>()
            .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => src.SearchText))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.Languages))
            .ForMember(dest => dest.SearchFields, opt => opt.MapFrom(src => src.SearchFields))
            .ForMember(dest => dest.DateLastSearch, opt => opt.MapFrom(src => src.DateLastSearch));

        CreateMap<ResourceModels.StatisticSummary, ResourceModels.StatisticUpdate>()
            .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => src.SearchText))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.Languages))
            .ForMember(dest => dest.SearchFields, opt => opt.MapFrom(src => src.SearchFields))
            .ForMember(dest => dest.DateLastSearch, opt => opt.MapFrom(src => src.DateLastSearch));

        CreateMap<DomainModels.Statistic, ResourceModels.StatisticSummary>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => src.SearchText))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.Languages))
            .ForMember(dest => dest.SearchFields, opt => opt.MapFrom(src => src.SearchFields))
            .ForMember(dest => dest.DateLastSearch, opt => opt.MapFrom(src => src.DateLastSearch));

        CreateMap<DomainModels.Statistic, ResourceModels.StatisticUpdate>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => src.SearchText))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.Languages))
            .ForMember(dest => dest.SearchFields, opt => opt.MapFrom(src => src.SearchFields))
            .ForMember(dest => dest.DateLastSearch, opt => opt.MapFrom(src => src.DateLastSearch));

        CreateMap<DomainModels.Statistic, ResourceModels.StatisticDetail>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => src.SearchText))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.Languages))
            .ForMember(dest => dest.SearchFields, opt => opt.MapFrom(src => src.SearchFields))
            .ForMember(dest => dest.DateLastSearch, opt => opt.MapFrom(src => src.DateLastSearch))
            .ForMember(dest => dest.NumberOfSearches, opt => opt.MapFrom(src => src.NumberOfSearches))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.Modified))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy));

        CreateMap<ResourceModels.IStatisticModification, DomainModels.Statistic>()
            .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => src.SearchText))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.Languages))
            .ForMember(dest => dest.SearchFields, opt => opt.MapFrom(src => src.SearchFields))
            .ForMember(dest => dest.DateLastSearch, opt => opt.MapFrom(src => src.DateLastSearch))
            .ForMember(dest => dest.NumberOfSearches, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Modified, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<ResourceModels.StatisticUpdate, DomainModels.Statistic>()
          .IncludeBase<ResourceModels.IStatisticModification, DomainModels.Statistic>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        CreateMap<ResourceModels.StatisticCreate, DomainModels.Statistic>()
            .IncludeBase<ResourceModels.IStatisticModification, DomainModels.Statistic>();
    }
}
