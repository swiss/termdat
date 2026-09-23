using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Mapping.Profiles;
using Bk.Termdat.Business.Mapping.Resolvers;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting;
using Bk.Termdat.CrossCutting.Configuration;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;
using ResourceModels = Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Tests.Mapping.Profiles;

internal class AdministrationMappingProfileTests
{
    private IMapper mapper;
    private readonly ICultureService cultureServiceMock = Substitute.For<ICultureService>();
    private readonly IOptions<PrivateOptions> privateOptionsMock = Substitute.For<IOptions<PrivateOptions>>();
    private readonly IOptions<AuthorizationOptions> authOptionsMock = Substitute.For<IOptions<AuthorizationOptions>>();
    private AuthorizationOptions options = null!;

    [SetUp]
    public void SetUp()
    {
        options = new AuthorizationOptions
        {
            Allow = nameof(AuthorizationOptions.Allow),
            Writer = nameof(AuthorizationOptions.Writer),
            SuperUser = nameof(AuthorizationOptions.SuperUser),
            SuperReader = nameof(AuthorizationOptions.SuperReader),
            Admin = nameof(AuthorizationOptions.Admin)
        };

        authOptionsMock.Value.Returns(options);

        cultureServiceMock.GetTranslation(Arg.Any<IEnumerable<DomainModels.IEntityNameTranslation>>()).Returns(x =>
        {
            var translations = x.Arg<IEnumerable<DomainModels.IEntityNameTranslation>>();
            return translations.FirstOrDefault(t => t.Language.IsoCode == "current")?.Name;
        });

        var services = new ServiceCollection();
        services.AddSingleton(cultureServiceMock);
        services.AddSingleton(authOptionsMock);
        services.AddTransient(typeof(TranslationResolver<,>));
        services.AddTransient(typeof(LanguageTranslationResolver<,>));
        services.AddTransient<RoleValueResolver>();
        services.AddTransient<HasFullAccessResolver>();
        services.AddTransient<IsWriterResolver>();
        services.AddTransient<IsSuperReaderResolver>();
        var provider = services.BuildServiceProvider();

        var config = new MapperConfiguration(c =>
        {
            c.ConstructServicesUsing(provider.GetRequiredService);
            c.AddProfile(new AdministrationMappingProfile());
            c.AddProfile(new SharedMappingProfile());
        }, LoggerFactory.Create(_ => { }));

        mapper = config.CreateMapper();
    }

    [TearDown]
    public void TearDown()
    {
        cultureServiceMock.ClearSubstitute();
        privateOptionsMock.ClearSubstitute();
        authOptionsMock.ClearSubstitute();
    }

    [Test]
    public void ValidateMappingConfiguration()
    {
        var mapperConfig = mapper.ConfigurationProvider;
        mapperConfig.AssertConfigurationIsValid();
    }

    [Test]
    public void ShouldMapDomainWithEntryCountToDomainDetail()
    {
        var domainWithEntryCount = new DomainWithEntryCount
        {
            Domain = new DomainModels.Domain
            {
                Id = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "me@me.com",
                Modified = DateTime.UtcNow,
                ModifiedBy = "you@me.com",
                SubjectId = 1,
                Translations = new List<DomainModels.DomainTranslation>
                {
                    new()
                    {
                        Name = "Default Translation",
                        Language = new DomainModels.Language { IsoCode = "default" }
                    },
                    new()
                    {
                        Name = "Current Translation",
                        Language = new DomainModels.Language { IsoCode = "current" }
                    }
                }
            },
            EntryCount = 1
        };

        var result = mapper.Map<ResourceModels.DomainDetail>(domainWithEntryCount);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Modified, Is.EqualTo(domainWithEntryCount.Domain.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(domainWithEntryCount.Domain.ModifiedBy));
        Assert.That(result.Created, Is.EqualTo(domainWithEntryCount.Domain.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(domainWithEntryCount.Domain.CreatedBy));
        Assert.That(result.SubjectId, Is.EqualTo(domainWithEntryCount.Domain.SubjectId));
        Assert.That(result.EntryCount, Is.EqualTo(domainWithEntryCount.EntryCount));
        Assert.That(result.Name, Is.EqualTo("Current Translation"));
    }

    [Test]
    public void ShouldMapDomainToDomainDetail()
    {
        var domain = new DomainModels.Domain
        {
            Id = 1,
            Created = DateTime.Now,
            CreatedBy = "me@me.com",
            Modified = DateTime.Now,
            ModifiedBy = "you@me.com",
            Translations = new List<DomainModels.DomainTranslation>
            {
                new()
                {
                    Name = "Default Translation",
                    Language = new DomainModels.Language { IsoCode = "default" }
                },
                new()
                {
                    Name = "Current Translation",
                    Language = new DomainModels.Language { IsoCode = "current" }
                }
            }
        };

        var result = mapper.Map<ResourceModels.DomainDetail>(domain);
        Assert.That(result.Id, Is.EqualTo(domain.Id));
        Assert.That(result.Name, Is.EqualTo("Current Translation"));
        Assert.That(result.Created, Is.EqualTo(domain.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(domain.CreatedBy));
        Assert.That(result.Modified, Is.EqualTo(domain.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(domain.ModifiedBy));
    }

    [Test]
    public void ShouldMapDomainToDomainModification()
    {
        var domain = new DomainModels.Domain
        {
            Created = DateTime.Now,
            CreatedBy = "me@me.to",
            Entries = [new DomainModels.Entry()],
            Id = 1,
            Modified = DateTime.Now,
            ModifiedBy = "you@me.to",
            SubjectId = 2,
            Translations = new List<DomainModels.DomainTranslation>
            {
                new() { Name = "de", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.De) } },
                new() { Name = "fr", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.Fr) } },
                new() { Name = "it", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.It) } },
                new() { Name = "rm", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.Rm) } },
                new() { Name = "en", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.En) } }
            }
        };

        var result = mapper.Map<ResourceModels.DomainModification>(domain);
        Assert.That(result.Id, Is.EqualTo(domain.Id));
        Assert.That(result.SubjectId, Is.EqualTo(domain.SubjectId));
        Assert.That(result.DescriptionDe, Is.EqualTo("de"));
        Assert.That(result.DescriptionFr, Is.EqualTo("fr"));
        Assert.That(result.DescriptionIt, Is.EqualTo("it"));
        Assert.That(result.DescriptionRm, Is.EqualTo("rm"));
        Assert.That(result.DescriptionEn, Is.EqualTo("en"));
    }

    [Test]
    public void ShouldMapDomainModificationToDomain()
    {
        var domainModification = new ResourceModels.DomainModification
        {
            Id = 1,
            SubjectId = 1,
            DescriptionDe = "de",
            DescriptionEn = "en",
            DescriptionFr = "fr",
            DescriptionIt = "it",
            DescriptionRm = "rm"
        };

        var result = mapper.Map<DomainModels.Domain>(domainModification);
        Assert.That(result.Id, Is.EqualTo(domainModification.Id));
        Assert.That(result.SubjectId, Is.EqualTo(domainModification.SubjectId));
    }

    [Test]
    public void ShouldMapSubjectWithEntryCountToSubjectDetail()
    {
        var subjectWithEntryCount = new SubjectWithEntryCount
        {
            EntryCount = 1,
            Subject = new DomainModels.Subject
            {
                Id = 1,
                Created = DateTime.Now,
                CreatedBy = "me@me.com",
                Modified = DateTime.Now,
                ModifiedBy = "you@me.com",
                Domains = new List<DomainModels.Domain>
                {
                    new() { Id = 1, Translations = new List<DomainModels.DomainTranslation>() }
                },
                Translations = new List<DomainModels.SubjectTranslation>
                {
                    new()
                    {
                        Name = "Default Translation",
                        Language = new DomainModels.Language { IsoCode = "default" }
                    },
                    new()
                    {
                        Name = "Current Translation",
                        Language = new DomainModels.Language { IsoCode = "current" }
                    }
                }
            }
        };

        var result = mapper.Map<ResourceModels.SubjectDetail>(subjectWithEntryCount);
        Assert.That(result.Id, Is.EqualTo(subjectWithEntryCount.Subject.Id));
        Assert.That(result.Created, Is.EqualTo(subjectWithEntryCount.Subject.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(subjectWithEntryCount.Subject.CreatedBy));
        Assert.That(result.Modified, Is.EqualTo(subjectWithEntryCount.Subject.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(subjectWithEntryCount.Subject.ModifiedBy));
        Assert.That(result.EntryCount, Is.EqualTo(subjectWithEntryCount.EntryCount));
        Assert.That(result.Name, Is.EqualTo("Current Translation"));
        Assert.That(result.Domains, Is.Not.Null);
        var domains = result.Domains;
        Assert.That(domains.Count, Is.EqualTo(1));
        Assert.That(domains.First().Id, Is.EqualTo(1));
    }

    [Test]
    public void ShouldMapSubjectToSubjectDetail()
    {
        var subject = new DomainModels.Subject
        {
            Created = DateTime.Now,
            CreatedBy = "me@me.to",
            Entries = [new DomainModels.Entry()],
            Id = 1,
            Modified = DateTime.Now,
            ModifiedBy = "you@me.to",
            Domains =
            [
                new DomainModels.Domain
                {
                        Id = 1, SubjectId = 1, Translations = new List<DomainModels.DomainTranslation>()
                    }
            ],
            Translations = new List<DomainModels.SubjectTranslation>
            {
                new()
                {
                    Name = "Default Translation",
                    Language = new DomainModels.Language { IsoCode = "default" }
                },
                new()
                {
                    Name = "Current Translation",
                    Language = new DomainModels.Language { IsoCode = "current" }
                }
            }
        };

        var result = mapper.Map<ResourceModels.SubjectDetail>(subject);
        Assert.That(result.Id, Is.EqualTo(subject.Id));
        Assert.That(result.Modified, Is.EqualTo(subject.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(subject.ModifiedBy));
        Assert.That(result.Created, Is.EqualTo(subject.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(subject.CreatedBy));
        Assert.That(result.Name, Is.EqualTo("Current Translation"));
    }

    [Test]
    public void ShouldMapSubjectModificationToSubject()
    {
        var subjectModification = new ResourceModels.SubjectModification
        {
            Id = 1,
            Domains = new List<ResourceModels.DomainModification> { new() { Id = 2, SubjectId = 1 } }
        };

        var result = mapper.Map<DomainModels.Subject>(subjectModification);
        Assert.That(result.Id, Is.EqualTo(subjectModification.Id));
        Assert.That(result.Domains, Is.Not.Null);
        var domains = result.Domains;
        Assert.That(domains.Count, Is.EqualTo(1));
        Assert.That(domains.First().Id, Is.EqualTo(2));
        Assert.That(domains.First().SubjectId, Is.EqualTo(subjectModification.Id));
    }

    [Test]
    public void ShouldMapSubjectToSubjectModification()
    {
        var subject = new DomainModels.Subject
        {
            Created = DateTime.Now,
            CreatedBy = "me@me.to",
            Entries = [new DomainModels.Entry()],
            Id = 1,
            Modified = DateTime.Now,
            ModifiedBy = "you@me.to",
            Domains =
            [
                new DomainModels.Domain
                {
                        Id = 1, SubjectId = 1, Translations = new List<DomainModels.DomainTranslation>()
                    }
            ],
            Translations = new List<DomainModels.SubjectTranslation>
            {
                new() { Name = "de", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.De) } },
                new() { Name = "fr", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.Fr) } },
                new() { Name = "it", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.It) } },
                new() { Name = "rm", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.Rm) } },
                new() { Name = "en", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.En) } }
            }
        };

        var result = mapper.Map<ResourceModels.SubjectModification>(subject);
        Assert.That(result.Id, Is.EqualTo(subject.Id));
        Assert.That(result.DescriptionDe, Is.EqualTo("de"));
        Assert.That(result.DescriptionFr, Is.EqualTo("fr"));
        Assert.That(result.DescriptionIt, Is.EqualTo("it"));
        Assert.That(result.DescriptionRm, Is.EqualTo("rm"));
        Assert.That(result.DescriptionEn, Is.EqualTo("en"));
        Assert.That(result.Domains, Is.Not.Null);
        Assert.That(result.Domains.Count, Is.EqualTo(1));
        Assert.That(result.Domains.First().Id, Is.EqualTo(subject.Domains.First().Id));
        Assert.That(result.Domains.First().SubjectId, Is.EqualTo(subject.Domains.First().SubjectId));
    }

    [Test]
    public void ShouldMapOfficeToOfficeDetail()
    {
        var office = new DomainModels.Office
        {
            Id = 1,
            Code = "Value",
            Translations = new List<DomainModels.OfficeTranslation>
            {
                new()
                {
                    Name = "Default Translation",
                    Language = new DomainModels.Language { IsoCode = "default" }
                },
                new()
                {
                    Name = "Current Translation",
                    Language = new DomainModels.Language { IsoCode = "current" }
                }
            },
            Entries = new List<DomainModels.Entry> { new(), new(), new() }
        };

        var result = mapper.Map<ResourceModels.OfficeDetail>(office);
        Assert.That(result.Id, Is.EqualTo(office.Id));
        Assert.That(result.Code, Is.EqualTo(office.Code));
        Assert.That(result.Created, Is.EqualTo(office.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(office.CreatedBy));
        Assert.That(result.Modified, Is.EqualTo(office.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(office.ModifiedBy));
        Assert.That(result.EntryCount, Is.EqualTo(3));
    }

    [Test]
    public void ShouldMapOfficeCreateToOffice()
    {
        var officeCreate = new ResourceModels.OfficeCreate { Code = "Code" };

        var result = mapper.Map<DomainModels.Office>(officeCreate);
        Assert.That(result.Code, Is.EqualTo(officeCreate.Code));
    }

    [Test]
    public void ShouldMapOfficeToOfficeUpdate()
    {
        var office = new DomainModels.Office
        {
            Code = "code",
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Entries = null,
            Id = 1,
            Modified = DateTime.UtcNow,
            ModifiedBy = "you@me.com",
            Translations = new List<DomainModels.OfficeTranslation>
            {
                new() { Language = new DomainModels.Language { IsoCode = "de" }, Name = "de" },
                new() { Language = new DomainModels.Language { IsoCode = "fr" }, Name = "fr" },
                new() { Language = new DomainModels.Language { IsoCode = "it" }, Name = "it" },
                new() { Language = new DomainModels.Language { IsoCode = "en" }, Name = "en" },
                new() { Language = new DomainModels.Language { IsoCode = "rm" }, Name = "rm" }
            }
        };

        var result = mapper.Map<ResourceModels.OfficeUpdate>(office);
        Assert.That(result.Code, Is.EqualTo(office.Code));
        Assert.That(result.DescriptionDe, Is.EqualTo("de"));
        Assert.That(result.DescriptionEn, Is.EqualTo("en"));
        Assert.That(result.DescriptionFr, Is.EqualTo("fr"));
        Assert.That(result.DescriptionIt, Is.EqualTo("it"));
        Assert.That(result.DescriptionRm, Is.EqualTo("rm"));
        Assert.That(result.Id, Is.EqualTo(office.Id));
    }

    [Test]
    public void ShouldMapOfficeUpdateToOffice()
    {
        var officeUpdate = new ResourceModels.OfficeUpdate
        {
            Id = 1,
            Code = "code",
            DescriptionDe = "de",
            DescriptionFr = "fr",
            DescriptionIt = "it",
            DescriptionRm = "rm",
            DescriptionEn = "en"
        };

        var result = mapper.Map<DomainModels.Office>(officeUpdate);
        Assert.That(result.Id, Is.EqualTo(officeUpdate.Id));
        Assert.That(result.Code, Is.EqualTo(officeUpdate.Code));
    }

    [Test]
    public void ShouldMapGlossarySummaryWithCurrentLanguage()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");
        var glossary = new DomainModels.Glossary
        {
            Id = 1,
            Code = "g1",
            Translations = new List<DomainModels.GlossaryTranslation>
            {
                new()
                {
                    Name = "Default Translation",
                    Language = new DomainModels.Language { IsoCode = "default" }
                },
                new()
                {
                    Name = "Current Translation",
                    Language = new DomainModels.Language { IsoCode = "current" }
                }
            }
        };

        var glossarySummary = mapper.Map<ResourceModels.GlossarySummary>(glossary);


        Assert.That(glossarySummary.Id, Is.EqualTo(glossary.Id));
        Assert.That(glossarySummary.Code, Is.EqualTo(glossary.Code));
        Assert.That(glossarySummary.Name, Is.EqualTo("Current Translation"));
    }

    [Test]
    public void ShouldMapGlossaryDetailWithCurrentLanguage()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");
        var glossary = new DomainModels.Glossary
        {
            Id = 1,
            Code = "g1",
            Description = "Glossary description",
            Modified = DateTime.UtcNow.AddDays(-1),
            Created = DateTime.UtcNow.AddDays(-4),
            ModifiedBy = "bölli mann",
            CreatedBy = "konami mann",
            Translations = new List<DomainModels.GlossaryTranslation>
            {
                new()
                {
                    Name = "Default Translation",
                    Language = new DomainModels.Language { IsoCode = "default" }
                },
                new()
                {
                    Name = "Current Translation",
                    Language = new DomainModels.Language { IsoCode = "current" }
                }
            }
        };

        var glossaryDetail = mapper.Map<ResourceModels.GlossaryDetail>(glossary);

        Assert.That(glossaryDetail.Id, Is.EqualTo(glossary.Id));
        Assert.That(glossaryDetail.Code, Is.EqualTo(glossary.Code));
        Assert.That(glossaryDetail.Description, Is.EqualTo(glossary.Description));
        Assert.That(glossaryDetail.Name, Is.EqualTo("Current Translation"));
        Assert.That(glossaryDetail.CreatedBy, Is.EqualTo(glossary.CreatedBy));
        Assert.That(glossaryDetail.EntryCount, Is.Zero);
    }

    [Test]
    public void ShouldMapGlossaryToGlossaryUpdate()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");
        var glossary = new DomainModels.Glossary
        {
            Id = 1,
            Code = "g1",
            Description = "Glossary description",
            Modified = DateTime.UtcNow.AddDays(-1),
            Created = DateTime.UtcNow.AddDays(-4),
            ModifiedBy = "bölli mann",
            CreatedBy = "konami mann",
            Translations = new List<DomainModels.GlossaryTranslation>
            {
                new() { Name = "de", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.De) } },
                new() { Name = "en", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.En) } },
                new() { Name = "fr", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.Fr) } },
                new() { Name = "it", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.It) } },
                new() { Name = "rm", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.Rm) } }
            }
        };

        var result = mapper.Map<ResourceModels.GlossaryUpdate>(glossary);


        Assert.That(result.Id, Is.EqualTo(glossary.Id));
        Assert.That(result.Code, Is.EqualTo(glossary.Code));
        Assert.That(result.Description, Is.EqualTo(glossary.Description));
        Assert.That(result.DescriptionDe, Is.EqualTo("de"));
        Assert.That(result.DescriptionEn, Is.EqualTo("en"));
        Assert.That(result.DescriptionFr, Is.EqualTo("fr"));
        Assert.That(result.DescriptionIt, Is.EqualTo("it"));
        Assert.That(result.DescriptionRm, Is.EqualTo("rm"));
        Assert.That(result.Created, Is.EqualTo(glossary.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(glossary.CreatedBy));
        Assert.That(result.Modified, Is.EqualTo(glossary.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(glossary.ModifiedBy));
    }

    [Test]
    public void ShouldMapFromGlossaryCreateToGlossary()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");
        var glossaryCreate = new ResourceModels.GlossaryCreate { Code = "g1", Description = "Glossary description" };

        var glossary = mapper.Map<DomainModels.Glossary>(glossaryCreate);

        Assert.That(glossary.Code, Is.EqualTo(glossaryCreate.Code));
        Assert.That(glossary.Description, Is.EqualTo(glossaryCreate.Description));

    }

    [Test]
    public void ShouldMapFromGlossaryUpdateToGlossary()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");
        var glossaryUpdate = new ResourceModels.GlossaryUpdate
        {
            Id = 1,
            Code = "g1",
            Description = "Glossary description"
        };

        var glossary = mapper.Map<DomainModels.Glossary>(glossaryUpdate);

        Assert.That(glossary.Id, Is.EqualTo(glossaryUpdate.Id));
        Assert.That(glossary.Code, Is.EqualTo(glossaryUpdate.Code));
        Assert.That(glossary.Description, Is.EqualTo(glossaryUpdate.Description));
    }

    [Test]
    public void ShouldMapCollectionCreate()
    {
        var collectionCreate = new ResourceModels.CollectionCreate
        {
            Active = true,
            ActiveApi = false,
            ExportLindas = true,
            ClassificationId = 1,
            Code = "12345",
            Description = "Description",
            Documentation = "Doku"
        };

        var result = mapper.Map<DomainModels.Collection>(collectionCreate);
        Assert.That(result.Active, Is.EqualTo(collectionCreate.Active));
        Assert.That(result.ActiveApi, Is.EqualTo(collectionCreate.ActiveApi));
        Assert.That(result.ClassificationId, Is.EqualTo(collectionCreate.ClassificationId));
        Assert.That(result.Code, Is.EqualTo(collectionCreate.Code));
        Assert.That(result.Description, Is.EqualTo(collectionCreate.Description));
        Assert.That(result.Documentation, Is.EqualTo(collectionCreate.Documentation));
        Assert.That(result.ExportLindas, Is.EqualTo(collectionCreate.ExportLindas));
    }

    [Test]
    public void ShouldMapCollectionDetailWithCurrentLang()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");
        var collection = new DomainModels.Collection
        {
            Id = 1,
            Code = "Value",
            IsObsolete = true,
            Deleted = true,
            Active = true,
            ActiveApi = true,
            ExportLindas = true,
            Description = "Description",
            Documentation = "Documentation",
            Created = new DateTime(2022, 1, 1, 1, 1, 1),
            Modified = new DateTime(2022, 1, 1, 1, 1, 1),
            CreatedBy = "CreatedBy",
            ModifiedBy = "ModifiedBy",
            Translations =
                new List<DomainModels.CollectionTranslation>
                {
                    new()
                    {
                        Name = "Default Translation", Language = new DomainModels.Language { IsoCode = "default" }
                    },
                    new() { Name = "Current Translation", Language = new DomainModels.Language { IsoCode = "current" } }
                },
            Classification = new()
            {
                Code = "Classification",
                Id = 2,
                Translations = new List<DomainModels.ClassificationTranslation>
                {
                    new()
                    {
                        Name = "Classification Default Translation",
                        Language = new DomainModels.Language { IsoCode = "default" }
                    },
                    new()
                    {
                        Name = "Classification Current Translation",
                        Language = new DomainModels.Language { IsoCode = "current" }
                    }
                }
            },
            ConceptSystem = new() { Id = 3 }
        };

        var collectionWithCount = new CollectionWithEntryCount { Collection = collection, EntryCount = 3 };

        var result = mapper.Map<ResourceModels.CollectionDetail>(collectionWithCount);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(collection.Id));
        Assert.That(result.Code, Is.EqualTo(collection.Code));
        Assert.That(result.IsObsolete, Is.EqualTo(collection.IsObsolete));
        Assert.That(result.Deleted, Is.EqualTo(collection.Deleted));
        Assert.That(result.Active, Is.EqualTo(collection.Active));
        Assert.That(result.ActiveApi, Is.EqualTo(collection.ActiveApi));
        Assert.That(result.ExportLindas, Is.EqualTo(collection.ExportLindas));
        Assert.That(result.Name, Is.EqualTo(collection.Translations.Last().Name));
        Assert.That(result.Classification, Is.EqualTo(collection.Classification.Translations.Last().Name));
        Assert.That(result.ConceptSystem, Is.Not.Null);
        Assert.That(result.ConceptSystem.Id, Is.EqualTo(collection.ConceptSystem.Id));
        Assert.That(result.EntryCount, Is.EqualTo(collectionWithCount.EntryCount));
    }

    [Test]
    public void ShouldMapCollectionUpdateToCollection()
    {
        var collectionUpdate = new ResourceModels.CollectionUpdate
        {
            Active = true,
            ActiveApi = true,
            ExportLindas = true,
            ClassificationId = 1,
            Code = "1234567890",
            DescriptionDe = "de",
            DescriptionFr = "fr",
            DescriptionEn = "en",
            DescriptionIt = "it",
            DescriptionRm = "rm",
            Description = "Description",
            Documentation = "docu",
            Id = 3,
            IsObsolete = false
        };

        var result = mapper.Map<DomainModels.Collection>(collectionUpdate);
        Assert.That(result.Id, Is.EqualTo(collectionUpdate.Id));
        Assert.That(result.Active, Is.EqualTo(collectionUpdate.Active));
        Assert.That(result.ActiveApi, Is.EqualTo(collectionUpdate.ActiveApi));
        Assert.That(result.ExportLindas, Is.EqualTo(collectionUpdate.ExportLindas));
        Assert.That(result.ClassificationId, Is.EqualTo(collectionUpdate.ClassificationId));
        Assert.That(result.Code, Is.EqualTo(collectionUpdate.Code));
        Assert.That(result.Description, Is.EqualTo(collectionUpdate.Description));
        Assert.That(result.Documentation, Is.EqualTo(collectionUpdate.Documentation));
    }

    [Test]
    public void ShouldMapCollectionToCollectionUpdate()
    {
        var collection = new DomainModels.Collection
        {
            Id = 1,
            Active = true,
            ActiveApi = true,
            ExportLindas = true,
            ClassificationId = 2,
            Code = "1234567890",
            Description = "Description",
            Documentation = "documentation",
            Translations = new List<DomainModels.CollectionTranslation>
            {
                new() { Name = "de", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.De) } },
                new() { Name = "en", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.En) } },
                new() { Name = "fr", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.Fr) } },
                new() { Name = "it", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.It) } },
                new() { Name = "rm", Language = new DomainModels.Language { IsoCode = nameof(IsoCode.Rm) } }
            },
            ConceptSystem = new DomainModels.ConceptSystem { Id = 3 },
            Created = DateTime.UtcNow,
            CreatedBy = "test",
            Modified = DateTime.UtcNow,
            ModifiedBy = "test"
        };

        var result = mapper.Map<ResourceModels.CollectionUpdate>(collection);
        Assert.That(result.Id, Is.EqualTo(collection.Id));
        Assert.That(result.Active, Is.EqualTo(collection.Active));
        Assert.That(result.ActiveApi, Is.EqualTo(collection.ActiveApi));
        Assert.That(result.ExportLindas, Is.EqualTo(collection.ExportLindas));
        Assert.That(result.ClassificationId, Is.EqualTo(collection.ClassificationId));
        Assert.That(result.Code, Is.EqualTo(collection.Code));
        Assert.That(result.DescriptionDe, Is.EqualTo("de"));
        Assert.That(result.DescriptionEn, Is.EqualTo("en"));
        Assert.That(result.DescriptionFr, Is.EqualTo("fr"));
        Assert.That(result.DescriptionIt, Is.EqualTo("it"));
        Assert.That(result.DescriptionRm, Is.EqualTo("rm"));
        Assert.That(result.Description, Is.EqualTo(collection.Description));
        Assert.That(result.Documentation, Is.EqualTo(collection.Documentation));
        Assert.That(result.Created, Is.EqualTo(collection.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(collection.CreatedBy));
        Assert.That(result.Modified, Is.EqualTo(collection.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(collection.ModifiedBy));
    }

    [Test]
    public void ShouldMapCollectionSummaryWithCurrentLang()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");
        var collection = new DomainModels.Collection
        {
            Id = 1,
            Code = "Value",
            IsObsolete = false,
            ExportLindas = true,
            Deleted = true,
            Translations =
                new List<DomainModels.CollectionTranslation>
                {
                    new()
                    {
                        Name = "Default Translation", Language = new DomainModels.Language { IsoCode = "default" }
                    },
                    new() { Name = "Current Translation", Language = new DomainModels.Language { IsoCode = "current" } }
                },
            Classification = new()
            {
                Code = "Classification",
                Id = 1,
                Translations = new List<DomainModels.ClassificationTranslation>
                {
                    new()
                    {
                        Name = "Classification Default Translation",
                        Language = new DomainModels.Language { IsoCode = "default" }
                    },
                    new()
                    {
                        Name = "Classification Current Translation",
                        Language = new DomainModels.Language { IsoCode = "current" }
                    }
                }
            }
        };

        var result = mapper.Map<ResourceModels.CollectionSummary>(collection);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(collection.Id));
        Assert.That(result.Code, Is.EqualTo(collection.Code));
        Assert.That(result.IsObsolete, Is.EqualTo(collection.IsObsolete));
        Assert.That(result.ExportLindas, Is.EqualTo(collection.ExportLindas));
        Assert.That(result.Name, Is.EqualTo(collection.Translations.Last().Name));
        Assert.That(result.Classification, Is.EqualTo(collection.Classification.Translations.Last().Name));
    }

    [Test]
    public void ShouldMapOfficeDetailWithCurrentLanguage()
    {
        cultureServiceMock.GetCurrentIsoCode().Returns("current");

        var officeWithEntryCount = new OfficeWithEntryCount
        {
            Office = new()
            {
                Id = 1,
                Code = "Value",
                Created = new DateTime(2022, 1, 1, 1, 1, 1),
                Modified = new DateTime(2022, 1, 1, 1, 1, 1),
                CreatedBy = "CreatedBy",
                ModifiedBy = "ModifiedBy",
                Translations = new List<DomainModels.OfficeTranslation>
                {
                    new()
                    {
                        Name = "Default Translation",
                        Language = new DomainModels.Language { IsoCode = "default" }
                    },
                    new()
                    {
                        Name = "Current Translation",
                        Language = new DomainModels.Language { IsoCode = "current" }
                    }
                }
            },
            EntryCount = 200
        };

        var result = mapper.Map<ResourceModels.OfficeDetail>(officeWithEntryCount);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(officeWithEntryCount.Office.Id));
        Assert.That(result.Code, Is.EqualTo(officeWithEntryCount.Office.Code));
        Assert.That(result.Created, Is.EqualTo(officeWithEntryCount.Office.Created));
        Assert.That(result.Modified, Is.EqualTo(officeWithEntryCount.Office.Modified));
        Assert.That(result.CreatedBy, Is.EqualTo(officeWithEntryCount.Office.CreatedBy));
        Assert.That(result.ModifiedBy, Is.EqualTo(officeWithEntryCount.Office.ModifiedBy));
        Assert.That(result.Name, Is.EqualTo(officeWithEntryCount.Office.Translations.Last().Name));
        Assert.That(result.EntryCount, Is.EqualTo(officeWithEntryCount.EntryCount));
    }

    [Test]
    public void ThrowsOnOfficeDetailWhenOfficeLanguageIsNull()
    {
        void Act() => mapper.Map<ResourceModels.OfficeDetail>(new OfficeWithEntryCount
        {
            Office = new DomainModels.Office()
        });

        var exception = Assert.Throws<AutoMapperMappingException>(Act);
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Cannot map translation without provided Language"));
    }

    [Test]
    public void ShouldMapConceptSystemDetail()
    {
        var conceptSystem = new DomainModels.ConceptSystem
        {
            Id = 1,
            Title = "test",
            Collection = new() { Code = "col1" },
            Created = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "creator",
            Modified = DateTime.UtcNow,
            ModifiedBy = "modifier"
        };

        var result = mapper.Map<ResourceModels.ConceptSystemDetail>(conceptSystem);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(conceptSystem.Id));
        Assert.That(result.Title, Is.EqualTo(conceptSystem.Title));
        Assert.That(result.Collection, Is.EqualTo(conceptSystem.Collection.Code));
        Assert.That(result.Created, Is.EqualTo(conceptSystem.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(conceptSystem.CreatedBy));
        Assert.That(result.Modified, Is.EqualTo(conceptSystem.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(conceptSystem.ModifiedBy));
        Assert.That(result.EntryCount, Is.EqualTo(0));
    }

    [Test]
    public void ShouldMapConceptSystemDetailWithEntryCount()
    {
        var conceptSystem = new DomainModels.ConceptSystem
        {
            Id = 1,
            Title = "test",
            Collection = new() { Code = "col1" },
            Created = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "creator",
            Modified = DateTime.UtcNow,
            ModifiedBy = "modifier"
        };
        var conceptSystemWithEntryCount = new ConceptSystemWithEntryCount
        {
            ConceptSystem = conceptSystem,
            EntryCount = 13
        };

        var result = mapper.Map<ResourceModels.ConceptSystemDetail>(conceptSystemWithEntryCount);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(conceptSystem.Id));
        Assert.That(result.Title, Is.EqualTo(conceptSystem.Title));
        Assert.That(result.Collection, Is.EqualTo(conceptSystem.Collection.Code));
        Assert.That(result.Created, Is.EqualTo(conceptSystem.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(conceptSystem.CreatedBy));
        Assert.That(result.Modified, Is.EqualTo(conceptSystem.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(conceptSystem.ModifiedBy));
        Assert.That(result.EntryCount, Is.EqualTo(conceptSystemWithEntryCount.EntryCount));
    }

    [Test]
    public void ShouldMapConceptSystemUpdate()
    {
        var conceptSystemPosition = new DomainModels.ConceptSystemPosition { Id = 100, Value = "pos 1" };
        var conceptSystem = new DomainModels.ConceptSystem
        {
            Id = 1,
            Title = "test",
            Collection = new() { Id = 10 },
            ConceptSystemPositions = new List<DomainModels.ConceptSystemPosition> { conceptSystemPosition }
        };

        var result = mapper.Map<ResourceModels.ConceptSystemUpdate>(conceptSystem);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(conceptSystem.Id));
        Assert.That(result.Title, Is.EqualTo(conceptSystem.Title));
        Assert.That(result.CollectionId, Is.EqualTo(conceptSystem.Collection.Id));
        var conceptSystemPositionUpdate = result.Positions.First();
        Assert.That(conceptSystemPositionUpdate.Id, Is.EqualTo(conceptSystemPosition.Id));
        Assert.That(conceptSystemPositionUpdate.Value, Is.EqualTo(conceptSystemPosition.Value));
    }

    [Test]
    public void ShouldMapConceptSystemFromUpdate()
    {
        var systemPositionUpdate = new ResourceModels.ConceptSystemPositionUpdate { Id = 100, Value = "pos 1" };
        var conceptSystemUpdate = new ResourceModels.ConceptSystemUpdate
        {
            Id = 1,
            Title = "test",
            Positions = new List<ResourceModels.ConceptSystemPositionUpdate> { systemPositionUpdate }
        };

        var result = mapper.Map<DomainModels.ConceptSystem>(conceptSystemUpdate);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(conceptSystemUpdate.Id));
        Assert.That(result.Title, Is.EqualTo(conceptSystemUpdate.Title));
        Assert.That(result.Collection, Is.Null);
        Assert.That(result.CollectionId, Is.EqualTo(0));
        Assert.That(result.Modified, Is.EqualTo(default(DateTime)));
        Assert.That(result.ModifiedBy, Is.Null);
        Assert.That(result.Created, Is.EqualTo(default(DateTime)));
        Assert.That(result.CreatedBy, Is.Null);
        var conceptSystemPosition = result.ConceptSystemPositions.First();
        Assert.That(conceptSystemPosition.Id, Is.EqualTo(systemPositionUpdate.Id));
        Assert.That(conceptSystemPosition.Value, Is.EqualTo(systemPositionUpdate.Value));
        Assert.That(conceptSystemPosition.ConceptSystem, Is.Null);
        Assert.That(conceptSystemPosition.ConceptSystemId, Is.EqualTo(0));
    }

    [Test]
    public void ShouldMapConceptSystemFromCreate()
    {
        var systemPositionUpdate = new ResourceModels.ConceptSystemPositionCreate { Value = "pos 1" };
        var conceptSystemUpdate = new ResourceModels.ConceptSystemCreate
        {
            Title = "test",
            Positions = new List<ResourceModels.ConceptSystemPositionCreate> { systemPositionUpdate }
        };

        var result = mapper.Map<DomainModels.ConceptSystem>(conceptSystemUpdate);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(0));
        Assert.That(result.Title, Is.EqualTo(conceptSystemUpdate.Title));
        Assert.That(result.Collection, Is.Null);
        Assert.That(result.CollectionId, Is.EqualTo(0));
        Assert.That(result.Modified, Is.EqualTo(default(DateTime)));
        Assert.That(result.ModifiedBy, Is.Null);
        Assert.That(result.Created, Is.EqualTo(default(DateTime)));
        Assert.That(result.CreatedBy, Is.Null);
        var conceptSystemPosition = result.ConceptSystemPositions.First();
        Assert.That(conceptSystemPosition.Id, Is.EqualTo(0));
        Assert.That(conceptSystemPosition.Value, Is.EqualTo(systemPositionUpdate.Value));
        Assert.That(conceptSystemPosition.ConceptSystem, Is.Null);
        Assert.That(conceptSystemPosition.ConceptSystemId, Is.EqualTo(0));
    }

    [Test]
    public void Map_WithUserAccount_MapsToUserAccountDetail()
    {
        var userAccount = GetUserAccount();

        var mappedDetail = mapper.Map<ResourceModels.UserAccountDetail>(userAccount);

        Assert.Multiple(() =>
        {
            Assert.That(mappedDetail, Is.Not.Null);
            Assert.That(mappedDetail.Email, Is.EqualTo(userAccount.Email));
            Assert.That(mappedDetail.LastName, Is.EqualTo(userAccount.LastName));
            Assert.That(mappedDetail.FirstName, Is.EqualTo(userAccount.FirstName));
            Assert.That(mappedDetail.Role, Is.EqualTo(BackendTexts.RoleAdmin));
            Assert.That(mappedDetail.LastAction, Is.EqualTo(userAccount.LastAction));

            Assert.That(mappedDetail.Glossaries, Is.Not.Empty);
            Assert.That(mappedDetail.Collections, Is.Not.Empty);

            Assert.That(mappedDetail.Glossaries.First().Code,
                Is.EqualTo(userAccount.AuthorizedGlossaries.First().Code));
            Assert.That(mappedDetail.Collections.First().Code,
                Is.EqualTo(userAccount.AuthorizedCollections.First().Code));
        });
    }

    [TestCaseSource(nameof(roleMapTestCases))]
    public void Map_WithUserAccount_MapsRolesCorrectly(bool hasRole, string role, string expectedRoleText)
    {
        var userAccount = GetUserAccount();
        userAccount.Role = role;

        var mappedDetail = mapper.Map<ResourceModels.UserAccountDetail>(userAccount);

        Assert.Multiple(() =>
        {
            Assert.That(mappedDetail, Is.Not.Null);
            Assert.That(mappedDetail.HasRole, Is.EqualTo(hasRole));
            Assert.That(mappedDetail.Role, Is.EqualTo(expectedRoleText));
        });
    }

    [TestCaseSource(nameof(hasFullAccessMapTestCases))]
    public void Map_WithUserAccount_MapsHasFullAccessCorrectly(string role, bool expectedAccess)
    {
        var userAccount = GetUserAccount();
        userAccount.Role = role;

        var mappedDetail = mapper.Map<ResourceModels.UserAccountDetail>(userAccount);

        Assert.Multiple(() =>
        {
            Assert.That(mappedDetail, Is.Not.Null);
            Assert.That(mappedDetail.HasFullAccess, Is.EqualTo(expectedAccess));
        });
    }

    [TestCaseSource(nameof(isWriterAccessMapTestCases))]
    public void Map_WithUserAccount_MapsIsWriterCorrectly(string role, bool expectedAccess)
    {
        var userAccount = GetUserAccount();
        userAccount.Role = role;

        var mappedDetail = mapper.Map<ResourceModels.UserAccountDetail>(userAccount);

        Assert.Multiple(() =>
        {
            Assert.That(mappedDetail, Is.Not.Null);
            Assert.That(mappedDetail.IsWriter, Is.EqualTo(expectedAccess));
        });
    }

    [Test]
    public void Map_FromUserModification_MapsCorrectly()
    {
        var userAccountModification = new ResourceModels.UserAccountModification
        {
            Id = 100,
            AuthorizedCollectionIds = Enumerable.Range(3, 5).ToList(),
            AuthorizedGlossaryIds = Enumerable.Range(1, 3).ToList()
        };

        var result = mapper.Map<DomainModels.UserAccount>(userAccountModification);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(userAccountModification.Id));
    }

    [Test]
    public void Map_FromAdminInfo_MapsCorrectly()
    {
        var adminInfo = new DomainModels.AdminInfo
        {
            Id = 13,
            Code = "Test",
            Description = "Description",
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "pp@bit.admin.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "ls@bit.admin.ch"
        };

        var adminInfoDetail = mapper.Map<ResourceModels.AdminInfoDetail>(adminInfo);

        Assert.That(adminInfoDetail, Is.Not.Null);
        Assert.That(adminInfoDetail.Id, Is.EqualTo(adminInfo.Id));
        Assert.That(adminInfoDetail.Code, Is.EqualTo(adminInfo.Code));
        Assert.That(adminInfoDetail.Description, Is.EqualTo(adminInfo.Description));
        Assert.That(adminInfoDetail.CreatedBy, Is.EqualTo(adminInfo.CreatedBy));
        Assert.That(adminInfoDetail.ModifiedBy, Is.EqualTo(adminInfo.ModifiedBy));
    }

    [Test]
    public void Map_FromAdminInfoSummaryToAdminInfo_MapsCorrectly()
    {
        var adminInfo = new ResourceModels.AdminInfoSummary { Id = 13, Code = "Test", Description = "Description" };

        var result = mapper.Map<DomainModels.AdminInfo>(adminInfo);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(adminInfo.Id));
        Assert.That(result.Code, Is.EqualTo(adminInfo.Code));
        Assert.That(result.Description, Is.EqualTo(adminInfo.Description));
    }

    [Test]
    public void Map_FromAdminInfoSummaryToAdminInfoCreate_MapsCorrectly()
    {
        var adminInfo = new ResourceModels.AdminInfoSummary { Id = 13, Code = "Test", Description = "Description" };

        var result = mapper.Map<ResourceModels.AdminInfoCreate>(adminInfo);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Code, Is.EqualTo(adminInfo.Code));
        Assert.That(result.Description, Is.EqualTo(adminInfo.Description));
    }

    [Test]
    public void Map_FromAdminInfoToAdminInfoEntryCount_MapsCorrectly()
    {
        var adminInfo = new DomainModels.AdminInfo
        {
            Id = 13,
            Code = "Test",
            Description = "Description",
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "pp@bit.admin.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "ls@bit.admin.ch"
        };

        var adminInfoWithEntryCount = new AdminInfoWithEntryCount { AdminInfo = adminInfo, EntryCount = 201 };

        var result = mapper.Map<ResourceModels.AdminInfoDetail>(adminInfoWithEntryCount);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(adminInfoWithEntryCount.AdminInfo.Id));
        Assert.That(result.Code, Is.EqualTo(adminInfoWithEntryCount.AdminInfo.Code));
        Assert.That(result.Description, Is.EqualTo(adminInfoWithEntryCount.AdminInfo.Description));
        Assert.That(result.Created, Is.EqualTo(adminInfoWithEntryCount.AdminInfo.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(adminInfoWithEntryCount.AdminInfo.CreatedBy));
        Assert.That(result.Modified, Is.EqualTo(adminInfoWithEntryCount.AdminInfo.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(adminInfoWithEntryCount.AdminInfo.ModifiedBy));
        Assert.That(result.EntryCount, Is.EqualTo(adminInfoWithEntryCount.EntryCount));
    }

    [Test]
    public void Map_FromAdminInfoToAdminInfoDetail_MapsCorrectly()
    {
        var adminInfo = new DomainModels.AdminInfo
        {
            Id = 13,
            Code = "Test",
            Description = "Description",
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "pp@bit.admin.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "ls@bit.admin.ch"
        };

        var result = mapper.Map<ResourceModels.AdminInfoDetail>(adminInfo);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(adminInfo.Id));
        Assert.That(result.Code, Is.EqualTo(adminInfo.Code));
        Assert.That(result.Description, Is.EqualTo(adminInfo.Description));
        Assert.That(result.Created, Is.EqualTo(adminInfo.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(adminInfo.CreatedBy));
        Assert.That(result.Modified, Is.EqualTo(adminInfo.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(adminInfo.ModifiedBy));
    }

    [Test]
    public void Map_FromAdminInfoToAdminInfoUpdate_MapsCorrectly()
    {
        var adminInfo = new DomainModels.AdminInfo
        {
            Id = 13,
            Code = "Test",
            Description = "Description",
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "pp@bit.admin.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "ls@bit.admin.ch"
        };

        var result = mapper.Map<ResourceModels.AdminInfoUpdate>(adminInfo);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(adminInfo.Id));
        Assert.That(result.Code, Is.EqualTo(adminInfo.Code));
        Assert.That(result.Description, Is.EqualTo(adminInfo.Description));
    }

    [Test]
    public void Map_FromAdminInfoCreateToAdminInfo_MapsCorrectly()
    {
        var adminInfo = new ResourceModels.AdminInfoCreate { Code = "Test", Description = "Description" };

        var result = mapper.Map<DomainModels.AdminInfo>(adminInfo);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Code, Is.EqualTo(adminInfo.Code));
        Assert.That(result.Description, Is.EqualTo(adminInfo.Description));
    }

    [Test]
    public void Map_FromAdminInfoCreateToAdminInfo_MapsAndTrimsCorrectly()
    {
        var adminInfo = new ResourceModels.AdminInfoCreate { Code = " Test ", Description = "Description" };

        var result = mapper.Map<DomainModels.AdminInfo>(adminInfo);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Code, Is.EqualTo("Test"));
        Assert.That(result.Description, Is.EqualTo(adminInfo.Description));
    }

    [Test]
    public void Map_FromAdminInfoUpdateToAdminInfo_MapsCorrectly()
    {
        var adminInfo = new ResourceModels.AdminInfoUpdate { Id = 1, Code = "Test", Description = "Description" };

        var result = mapper.Map<DomainModels.AdminInfo>(adminInfo);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Code, Is.EqualTo(adminInfo.Code));
        Assert.That(result.Description, Is.EqualTo(adminInfo.Description));
    }

    [Test]
    public void Map_FromAdminInfoUpdateToAdminInfo_MapsAndTrimsCorrectly()
    {
        var adminInfo = new ResourceModels.AdminInfoUpdate { Id = 1, Code = "  Test ", Description = "Description" };

        var result = mapper.Map<DomainModels.AdminInfo>(adminInfo);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Code, Is.EqualTo("Test"));
        Assert.That(result.Description, Is.EqualTo(adminInfo.Description));
    }

    [Test]
    public void Map_FromStatisticSummaryToStatistic_MapsCorrectly()
    {
        var statisticSummary = new ResourceModels.StatisticSummary
        {
            Id = 1,
            SearchText = "Rapunzel",
            SearchFields = "AB, Na, Md, PH, VE",
            Languages = "DE, FR, IT, RM, EN",
            NumberOfSearches = 71,
            DateLastSearch = DateTime.UtcNow
        };

        var result = mapper.Map<DomainModels.Statistic>(statisticSummary);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.SearchText, Is.EqualTo(statisticSummary.SearchText));
        Assert.That(result.SearchFields, Is.EqualTo(statisticSummary.SearchFields));
        Assert.That(result.Languages, Is.EqualTo(statisticSummary.Languages));
        Assert.That(result.NumberOfSearches, Is.EqualTo(statisticSummary.NumberOfSearches));
        Assert.That(result.DateLastSearch, Is.EqualTo(statisticSummary.DateLastSearch));
    }

    [Test]
    public void Map_FromStatisticToStatisticSummary_MapsCorrectly()
    {
        var statistic = new DomainModels.Statistic
        {
            Id = 2,
            SearchText = "Stiefelkater",
            SearchFields = "AB, Na, PH, VE",
            Languages = "DE, FR, IT",
            NumberOfSearches = 92,
            DateLastSearch = DateTime.UtcNow,
            Created = DateTime.UtcNow,
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow,
            ModifiedBy = "test@test.ch"
        };

        var result = mapper.Map<ResourceModels.StatisticSummary>(statistic);
        Assert.That(result.Id, Is.EqualTo(2));
        Assert.That(result.SearchText, Is.EqualTo(statistic.SearchText));
        Assert.That(result.SearchFields, Is.EqualTo(statistic.SearchFields));
        Assert.That(result.Languages, Is.EqualTo(statistic.Languages));
        Assert.That(result.NumberOfSearches, Is.EqualTo(statistic.NumberOfSearches));
        Assert.That(result.DateLastSearch, Is.EqualTo(statistic.DateLastSearch));
    }

    [Test]
    public void Map_FromStatisticToStatisticUpdate_MapsCorrectly()
    {
        var statistic = new DomainModels.Statistic
        {
            Id = 3,
            SearchText = "FrauHolle",
            SearchFields = "AB, Na, PH, VE",
            Languages = "DE, FR, IT, EN",
            NumberOfSearches = 15,
            DateLastSearch = DateTime.UtcNow,
            Created = DateTime.UtcNow,
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow,
            ModifiedBy = "test2@test.ch"
        };

        var result = mapper.Map<ResourceModels.StatisticUpdate>(statistic);
        Assert.That(result.Id, Is.EqualTo(3));
        Assert.That(result.SearchText, Is.EqualTo(statistic.SearchText));
        Assert.That(result.SearchFields, Is.EqualTo(statistic.SearchFields));
        Assert.That(result.Languages, Is.EqualTo(statistic.Languages));
        Assert.That(result.DateLastSearch, Is.EqualTo(statistic.DateLastSearch));
    }

    [Test]
    public void Map_FromStatisticUpdateToStatistic_MapsCorrectly()
    {
        var statisticSummary = new ResourceModels.StatisticUpdate
        {
            Id = 1,
            SearchText = "KissThatFrog",
            SearchFields = "AB, PH, VE",
            Languages = "DE, FR, IT, RM, EN",
            DateLastSearch = DateTime.UtcNow
        };

        var result = mapper.Map<DomainModels.Statistic>(statisticSummary);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.SearchText, Is.EqualTo(statisticSummary.SearchText));
        Assert.That(result.SearchFields, Is.EqualTo(statisticSummary.SearchFields));
        Assert.That(result.Languages, Is.EqualTo(statisticSummary.Languages));
        Assert.That(result.DateLastSearch, Is.EqualTo(statisticSummary.DateLastSearch));
    }

    [Test]
    public void Map_FromStatisticToStatisticDetail_MapsCorrectly()
    {
        var statistic = new DomainModels.Statistic
        {
            Id = 3,
            SearchText = "PeterPan",
            SearchFields = "AB",
            Languages = "DE",
            NumberOfSearches = 55,
            DateLastSearch = DateTime.UtcNow,
            Created = DateTime.UtcNow,
            CreatedBy = "test@test.ch",
            Modified = DateTime.UtcNow,
            ModifiedBy = "test2@test.ch"
        };

        var result = mapper.Map<ResourceModels.StatisticDetail>(statistic);
        Assert.That(result.Id, Is.EqualTo(3));
        Assert.That(result.SearchText, Is.EqualTo(statistic.SearchText));
        Assert.That(result.SearchFields, Is.EqualTo(statistic.SearchFields));
        Assert.That(result.Languages, Is.EqualTo(statistic.Languages));
        Assert.That(result.DateLastSearch, Is.EqualTo(statistic.DateLastSearch));
        Assert.That(result.NumberOfSearches, Is.EqualTo(statistic.NumberOfSearches));
        Assert.That(result.Created, Is.EqualTo(statistic.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(statistic.CreatedBy));
        Assert.That(result.Modified, Is.EqualTo(statistic.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(statistic.ModifiedBy));
    }

    private static DomainModels.UserAccount GetUserAccount()
    {
        var faker = new Faker();
        var userAccount = new DomainModels.UserAccount
        {
            Id = faker.Random.Number(0, 100),
            Created = faker.Date.Recent(),
            CreatedBy = faker.Person.FullName,
            Modified = faker.Date.Recent(),
            ModifiedBy = faker.Person.FullName,
            Email = faker.Internet.Email(),
            LastName = faker.Person.LastName,
            FirstName = faker.Person.FirstName,
            Role = nameof(AuthorizationOptions.Admin),
            LastAction = faker.Date.Recent(),
            LastLogin = faker.Date.Recent(),
            AuthorizedGlossaries = new List<DomainModels.Glossary>
            {
                new()
                {
                    Id = 10,
                    Code = "ABC",
                    Translations =
                        new List<DomainModels.GlossaryTranslation>
                        {
                            new() { Name = "Glossary", Language = new() }
                        }
                }
            },
            AuthorizedCollections = new List<DomainModels.Collection>
            {
                new()
                {
                    Id = 20,
                    Code = "XYZ",
                    Translations = new List<DomainModels.CollectionTranslation>
                    {
                        new() { Name = "Collection", Language = new() }
                    }
                }
            }
        };

        return userAccount;
    }

    private static readonly object[] roleMapTestCases =
    [
        new object[] { true, nameof(AuthorizationOptions.Admin), BackendTexts.RoleAdmin },
        new object[] { true, nameof(AuthorizationOptions.SuperUser), BackendTexts.RoleSuperUser },
        new object[] { true, nameof(AuthorizationOptions.SuperReader), BackendTexts.RoleSuperReader },
        new object[] { true, nameof(AuthorizationOptions.Writer), BackendTexts.RoleWriter },
        new object[] { true, nameof(AuthorizationOptions.Allow), BackendTexts.RoleNone },
        new object[] { false, string.Empty, BackendTexts.RoleNone },
        new object[] { true, "asdf", BackendTexts.RoleNone }, new object[] { false, null, BackendTexts.RoleNone }
    ];

    private static readonly object[] hasFullAccessMapTestCases =
    [
        new object[] { nameof(AuthorizationOptions.Admin), true },
        new object[] { nameof(AuthorizationOptions.SuperUser), true },
        new object[] { nameof(AuthorizationOptions.SuperReader), false },
        new object[] { nameof(AuthorizationOptions.Writer), false },
        new object[] { nameof(AuthorizationOptions.Allow), false }, new object[] { string.Empty, false },
        new object[] { "asdf", false }, new object[] { null, false }
    ];

    private static readonly object[] isWriterAccessMapTestCases =
    [
        new object[] { nameof(AuthorizationOptions.Admin), false },
        new object[] { nameof(AuthorizationOptions.SuperUser), false },
        new object[] { nameof(AuthorizationOptions.SuperReader), false },
        new object[] { nameof(AuthorizationOptions.Writer), true },
        new object[] { nameof(AuthorizationOptions.Allow), false }, new object[] { string.Empty, false },
        new object[] { "asdf", false }, new object[] { null, false }
    ];
}
