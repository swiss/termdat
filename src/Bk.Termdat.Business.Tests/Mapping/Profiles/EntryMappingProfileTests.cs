using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Mapping.Actions;
using Bk.Termdat.Business.Mapping.Profiles;
using Bk.Termdat.Business.Mapping.Resolvers;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
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

internal class EntryMappingProfileTests
{
    private IMapper mapper;
    private Entry entry;
    private Entry referencedEntry1;
    private Entry referencedEntry2;
    private Entry referencedEntry3;
    private Entry referencedEntry4;
    private EntryLanguage entryLanguage;
    private EntryTemplate entryTemplate;
    private readonly PrivateOptions optionsMock = new();
    private readonly IOptions<PrivateOptions> privateOptionsMock = Substitute.For<IOptions<PrivateOptions>>();
    private readonly IAuthorizationService authorizationServiceMock = Substitute.For<IAuthorizationService>();
    private readonly INoteSubCodeTranslationService noteSubCodeTranslationService = Substitute.For<INoteSubCodeTranslationService>();
    private readonly ICultureService cultureServiceMock = Substitute.For<ICultureService>();

    [SetUp]
    public void SetUp()
    {
        privateOptionsMock.ClearSubstitute();
        authorizationServiceMock.ClearSubstitute();
        cultureServiceMock.ClearSubstitute();

        cultureServiceMock.GetTranslation(Arg.Any<IEnumerable<IEntityNameTranslation>>()).Returns(x =>
        {
            var translations = x.Arg<IEnumerable<IEntityNameTranslation>>();
            return translations.FirstOrDefault(t => t.Language.IsoCode == "current")?.Name;
        });
        cultureServiceMock.GetCurrentIsoCode().ReturnsForAnyArgs("current");

        optionsMock.AchOfficeId = 10;
        optionsMock.ConceptUriFormat = "https://example.com/{0}";
        privateOptionsMock.Value.Returns(optionsMock);

        authorizationServiceMock.IsAdmin().Returns(true);
        authorizationServiceMock.IsSuperUser().Returns(false);
        authorizationServiceMock.IsWriter().Returns(false);

        noteSubCodeTranslationService.GetTooltips(Arg.Any<string>())
            .Returns(new List<NoteSubCodeTooltip>
            {
                new() { Code = "USG", Tooltip = "Usg tooltip" },
                new() { Code = "EXP", Tooltip = "Exp tooltip" }
            });

        var config = new MapperConfiguration(c =>
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => cultureServiceMock);
            services.AddScoped(_ => privateOptionsMock);
            services.AddScoped(_ => authorizationServiceMock);
            services.AddScoped(_ => noteSubCodeTranslationService);
            services.AddTransient<EntryWatchlistValueResolver>();
            services.AddTransient(typeof(TranslationResolver<,>));
            services.AddTransient(typeof(LanguageTranslationResolver<,>));
            services.AddTransient<EntrySummaryIsMainOfficeResolver>();
            services.AddTransient<EntryDetailIsMainOfficeResolver>();
            services.AddTransient<WatchlistEntryIsMainOfficeResolver>();
            services.AddTransient<EntryCanBeAddedToWatchListResolver>();
            services.AddTransient<EntryCanDeleteResolver>();
            services.AddTransient<EntryCanRestoreResolver>();
            services.AddTransient<EntryCanEditCollectionResolver>();
            services.AddTransient<EntryCanEditResolver>();
            services.AddTransient<EntryLockedResolver>();
            services.AddTransient<EntryLockedByResolver>();
            services.AddTransient<EntryDetailConceptUriResolver>();
            services.AddTransient<EntryDetailRelatedCollectionsResolver>();
            services.AddTransient<EntryDetailIdentificationResolver>();
            services.AddTransient<EntryDetailCommentResolver>();
            services.AddTransient<EntryDetailAdminInfosResolver>();
            services.AddTransient<EntryDetailCreatedByResolver>();
            services.AddTransient<EntryDetailModifiedByResolver>();
            services.AddTransient<EntryLanguageNoteTooltipResolver>();
            services.AddTransient<EntryLanguageDetailAuthorizationAction>();
            services.AddTransient<EntryWatchlistGlossariesResolver>();
            services.AddTransient<EntryWatchlistAdminInfosResolver>();
            var provider = services.BuildServiceProvider();

            c.ConstructServicesUsing(provider.GetRequiredService);
            c.AddProfile(new EntryMappingProfile());
            c.AddProfile(new SharedMappingProfile());
        }, LoggerFactory.Create(_ => { }));
        mapper = config.CreateMapper();

        referencedEntry1 = GenerateReferencedEntry(2);
        referencedEntry2 = GenerateReferencedEntry(3);
        referencedEntry3 = GenerateReferencedEntry(4);
        referencedEntry3.Deleted = true;
        referencedEntry4 = GenerateReferencedEntry(5);

        entry = new Entry
        {
            Id = 1,
            OfficeId = 10,
            CollectionId = 100,
            Reliability = DomainModels.Reliability.Approved,
            Status = DomainModels.Status.Validated,
            Identification = 100,
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "CreatedBy",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "ModifiedBy",
            Comment = "Comment",
            Deleted = true,
            Locked = DateTime.UtcNow.AddMinutes(-5),
            LockedBy = "test@test.ch",
            AdminInfos = [new AdminInfo() { Code = "adminInfo1" }, new AdminInfo() { Code = "adminInfo2" }],
            Glossaries = new List<Glossary>()
            {
                new()
                {
                    Id = 1,
                    Code = "Glo1",
                    Translations = new List<GlossaryTranslation> {
                        new()
                        {
                            Name = "Default Translation",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current Translation",
                            Language = new()
                                {
                                IsoCode = "current"
                            }
                        }
                    }
                }
            },
            Office = new()
            {
                Id = 10,
                Code = "Office",
                Translations = new List<OfficeTranslation> {
                    new()
                    {
                        Name = "Default Translation",
                        Language = new()
                        {
                            IsoCode = "default"
                        }
                    },
                    new()
                    {
                        Name = "Current Translation",
                        Language = new()
                        {
                            IsoCode = "current"
                        }
                    }
                }
            },
            Collection = new Collection()
            {
                Id = 100,
                Code = "Collection",
                Translations = new List<CollectionTranslation> {
                    new()
                    {
                        Name = "Default Translation",
                        Language = new()
                        {
                            IsoCode = "default"
                        }
                    },
                    new()
                    {
                        Name = "Current Translation",
                        Language = new()
                        {
                            IsoCode = "current"
                        }
                    }
                }
            },
            RelatedCollections = new List<Collection>
            {
                new()
                {
                    Id = 101,
                    Code = "RelatedCollection",
                    Translations = new List<CollectionTranslation>
                    {
                        new()
                        {
                            Name = "Default Translation",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current Translation",
                            Language = new()
                            {
                                IsoCode = "current"
                            }
                        }
                    }
                },
                new()
                {
                    Id = 102,
                    Code = "RelatedCollection2",
                    Deleted = true,
                    Translations = new List<CollectionTranslation>
                    {
                        new()
                        {
                            Name = "Default Translation2",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current Translation2",
                            Language = new()
                            {
                                IsoCode = "current"
                            }
                        }
                    }
                },
            },
            LenochClassifications = new List<LenochClassification>
            {
                new()
                {
                    Id = 200,
                    Code = "Domain",
                    Translations = new List<LenochClassificationTranslation>()
                    {
                        new()
                        {
                            Name = "Default Translation",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current Translation",
                            Language = new()
                            {
                                IsoCode = "current"
                            }
                        }
                    }
                }
            },
            Picture = new()
            {
                File = new()
                {
                    Value = Encoding.ASCII.GetBytes("TEST"),
                },
                EntryId = 1,
                ContentType = "ContentType",
                EntityTag = "EntityTag",
                FileExtension = "FileExtension",
                Translations = new List<PictureTranslation>
                {
                    new() { Id = 10001, Description = "Description DE", Source = "Source DE", Language = new() { IsoCode = IsoCode.De.ToString() }},
                    new() { Id = 10002, Description = "Description FR", Source = "Source FR", Language = new() { IsoCode = IsoCode.Fr.ToString() }},
                    new() { Id = 10003, Description = "Description IT", Source = "Source IT", Language = new() { IsoCode = IsoCode.It.ToString() }},
                    new() { Id = 10004, Description = "Description RM", Source = "Source RM", Language = new() { IsoCode = IsoCode.Rm.ToString() }},
                    new() { Id = 10005, Description = "Description EN", Source = "Source EN", Language = new() { IsoCode = IsoCode.En.ToString() }},
                }
            },
            Translations = new List<EntryLanguage>
            {
                new() { Id = 1, EntryId = 1, LanguageId = 1, Sequence = 1, Abbreviation = "kg", Definition = "Masseinheit, die gleich der Masse des Internationalen Kilogrammprototyps ist.", Name = "Kilogramm", Phraseology = null, Terminus = "Einheit der Masse"},
                new() { Id = 2, EntryId = 1, LanguageId = 2, Sequence = 1, Abbreviation = "kg", Definition = "Unité de mesure égale à la masse du prototype international du kilogramme. ", Name = "kilogramme", Phraseology = null, Terminus = "unité de masse"},
                new() { Id = 3, EntryId = 1, LanguageId = 3, Sequence = 1, Abbreviation = "kg", Definition = "Unità di misura pari alla massa del prototipo internazionale del chilogrammo.", Name = "chilogrammo", Phraseology = null, Terminus = "unità di massa"},
            },
            ConceptSystemPosition = new()
            {
                Id = 1000,
                Value = "ConceptSystemPosition"
            },
            References = new List<EntryReference>
            {
                new() { Id = 1, EntryId = 1, ReferencedEntryId = 2, Type = DomainModels.ReferenceType.General, ReferencedEntry = referencedEntry1 },
                new() { Id = 2, EntryId = 1, ReferencedEntryId = 3, Type = DomainModels.ReferenceType.None, ReferencedEntry = referencedEntry2 },
                new() { Id = 3, EntryId = 1, ReferencedEntryId = 4, Type = DomainModels.ReferenceType.Antonym, ReferencedEntry = referencedEntry3 },
            },
            ReferencedByEntries = new List<EntryReference>
            {
                new() { Id = 4, ReferencedEntryId = 1, EntryId = 4, Type = DomainModels.ReferenceType.Antonym, Entry = referencedEntry3 },
                new() { Id = 5, ReferencedEntryId = 1, EntryId = 5, Type = DomainModels.ReferenceType.Specific, Entry = referencedEntry4 }
            }
        };

        entryLanguage = new EntryLanguage
        {
            Id = 123,
            EntryId = 456,
            Entry = new()
            {
                Id = 4
            },
            LanguageId = 1,
            Language = new()
            {
                Id = 2,
                IsoCode = "current"
            },
            Sequence = 1,
            Terminus = "Terminus",
            TerminusSource = "TerminusSource",
            Name = "Name",
            NameSource = "NameSource",
            Abbreviation = "Abbreviation",
            AbbreviationSource = "AbbreviationSource",
            Phraseology = "Phraseology",
            PhraseologySource = "PhraseologySource",
            Definition = "Definition",
            DefinitionSource = "DefinitionSource",
            Note = "USG: Note; EXP: Note2",
            NoteSource = "NoteSource",
            Context = "Context",
            ContextSource = "ContextSource",
            Country = "Country",
            Metadata = "Metadata",
            MetadataSource = "MetadataSource",
            IsRare = true,
            IsObsolete = true,
            ShouldAvoid = true,
        };

        entryTemplate = new EntryTemplate
        {
            Id = 1,
            Name = "MyTemplate",
            Template = "\"Identification\":abc,\"Comment\":\"CommentTest\",\"Subjects\":[{\"Name\":\"SubjectNameTest\"}]",
            UserAccountId = 1,
        };
    }

    [Test]
    public void ValidateMappingConfiguration()
    {
        var mapperConfig = mapper.ConfigurationProvider;
        mapperConfig.AssertConfigurationIsValid();
    }

    [Test]
    public void ShouldMapEntryDetailForAdmin()
    {
        authorizationServiceMock.IsAdmin().Returns(true);
        authorizationServiceMock.IsSuperUser().Returns(false);
        authorizationServiceMock.IsWriter().Returns(false);
        authorizationServiceMock.CanEdit(Arg.Any<Entry>()).Returns(true);

        var result = mapper.Map<EntryDetail>(entry);

        AssertGeneralEntryDetail(entry, result);

        Assert.That(result.CreatedBy, Is.EqualTo(entry.CreatedBy));
        Assert.That(result.ModifiedBy, Is.EqualTo(entry.ModifiedBy));
        Assert.That(result.Comment, Is.EqualTo(entry.Comment));
        Assert.That(result.Identification, Is.EqualTo(entry.Identification));

        Assert.That(result.Locked, Is.EqualTo(entry.Locked));
        Assert.That(result.LockedBy, Is.EqualTo(entry.LockedBy));

        Assert.That(result.AdminInfos, Has.Count.EqualTo(2));
        Assert.That(result.RelatedCollections, Is.Not.Null);
        Assert.That(result.RelatedCollections.Count, Is.EqualTo(1));
        Assert.That(result.RelatedCollections.All(rc => rc.Id == 101));
    }

    [Test]
    public void ShouldMapEntryDetailForSuperUser()
    {
        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsSuperUser().Returns(true);
        authorizationServiceMock.IsWriter().Returns(false);
        authorizationServiceMock.CanEdit(Arg.Any<Entry>()).Returns(true);

        var result = mapper.Map<EntryDetail>(entry);

        AssertGeneralEntryDetail(entry, result);

        Assert.That(result.CreatedBy, Is.EqualTo(entry.CreatedBy));
        Assert.That(result.ModifiedBy, Is.EqualTo(entry.ModifiedBy));
        Assert.That(result.Comment, Is.EqualTo(entry.Comment));
        Assert.That(result.Identification, Is.EqualTo(entry.Identification));

        Assert.That(result.Locked, Is.EqualTo(entry.Locked));
        Assert.That(result.LockedBy, Is.EqualTo(entry.LockedBy));

        Assert.That(result.RelatedCollections, Is.Not.Null);
        Assert.That(result.RelatedCollections.Count, Is.EqualTo(1));
        Assert.That(result.RelatedCollections.All(rc => rc.Id == 101));
    }

    [Test]
    public void ShouldMapEntryDetailForWriter()
    {
        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsSuperUser().Returns(false);
        authorizationServiceMock.IsWriter().Returns(true);
        authorizationServiceMock.CanEdit(Arg.Any<Entry>()).Returns(true);

        var result = mapper.Map<EntryDetail>(entry);

        AssertGeneralEntryDetail(entry, result);

        Assert.That(result.CreatedBy, Is.EqualTo(entry.CreatedBy));
        Assert.That(result.ModifiedBy, Is.EqualTo(entry.ModifiedBy));
        Assert.That(result.Comment, Is.EqualTo(entry.Comment));
        Assert.That(result.RelatedCollections, Is.Null);
        Assert.That(result.Identification, Is.Null);

        Assert.That(result.Locked, Is.EqualTo(entry.Locked));
        Assert.That(result.LockedBy, Is.EqualTo(entry.LockedBy));
    }

    [Test]
    public void Map_EntryDetailForSuperReader_ShouldMap()
    {
        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsSuperUser().Returns(false);
        authorizationServiceMock.IsWriter().Returns(false);
        authorizationServiceMock.IsSuperReader().Returns(true);
        authorizationServiceMock.CanEdit(Arg.Any<Entry>()).Returns(false);

        var result = mapper.Map<EntryDetail>(entry);

        AssertGeneralEntryDetail(entry, result);

        Assert.That(result.CreatedBy, Is.Null);
        Assert.That(result.ModifiedBy, Is.Null);
        Assert.That(result.Comment, Is.EqualTo(entry.Comment));
        Assert.That(result.AdminInfos, Has.Count.EqualTo(2));
        Assert.That(result.RelatedCollections, Is.Null);
        Assert.That(result.Identification, Is.Null);

        Assert.That(result.Locked, Is.Null);
        Assert.That(result.LockedBy, Is.Null);
    }

    [Test]
    public void ShouldMapEntryDetail_WithConceptUri()
    {
        entry.Collection.ExportLindas = true;
        entry.Collection.Active = true;
        entry.Collection.Deleted = false;

        var result = mapper.Map<EntryDetail>(entry);

        Assert.That(result.ConceptUri, Is.EqualTo("https://example.com/1"));
    }

    [Test]
    public void ShouldMapEntryDetail_WithoutConceptUri()
    {
        entry.Collection.ExportLindas = false;
        entry.Collection.Active = true;
        entry.Collection.Deleted = false;

        var result = mapper.Map<EntryDetail>(entry);

        Assert.That(result.ConceptUri, Is.Null);
    }

    [Test]
    public void Map_ForUnauthorized_ShouldNotMapLockInfos()
    {
        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsSuperUser().Returns(false);
        authorizationServiceMock.IsWriter().Returns(false);
        authorizationServiceMock.CanEdit(Arg.Any<Entry>()).Returns(false);

        var result = mapper.Map<EntryDetail>(entry);

        AssertGeneralEntryDetail(entry, result);

        Assert.That(result.Locked, Is.Null);
        Assert.That(result.LockedBy, Is.Null);
    }

    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    [TestCase(false, false, true)]
    public void ShouldMapEntryModificationDetail(bool isAdmin, bool isSuperUser, bool isWriter)
    {
        authorizationServiceMock.IsAdmin().Returns(isAdmin);
        authorizationServiceMock.IsSuperUser().Returns(isSuperUser);
        authorizationServiceMock.IsWriter().Returns(isWriter);

        var result = mapper.Map<EntryModificationDetail>(entry);

        AssertGeneralEntryModificationDetail(entry, result);
    }

    [Test]
    public void ShouldMapEntryIsMainOffice()
    {
        var result = mapper.Map<EntryDetail>(entry);
        Assert.That(result.IsMainOffice, Is.True);

        var resultSummary = mapper.Map<EntrySummary>(entry);
        Assert.That(resultSummary.IsMainOffice, Is.True);

        optionsMock.AchOfficeId = 11;

        var result2 = mapper.Map<EntryDetail>(entry);
        Assert.That(result2.IsMainOffice, Is.False);

        var resultSummary2 = mapper.Map<EntrySummary>(entry);
        Assert.That(resultSummary2.IsMainOffice, Is.False);
    }

    [Test]
    public void ShouldMapPictureForEntry()
    {
        var result = mapper.Map<EntryDetail>(entry);

        Assert.That(result.Picture, Is.Not.Null);
        Assert.That(result.Picture.EntryId, Is.EqualTo(entry.Picture.EntryId));
        Assert.That(result.Picture.EntityTag, Is.EqualTo(entry.Picture.EntityTag));
        Assert.That(result.Picture.DescriptionDe, Is.EqualTo("Description DE"));
        Assert.That(result.Picture.DescriptionFr, Is.EqualTo("Description FR"));
        Assert.That(result.Picture.DescriptionIt, Is.EqualTo("Description IT"));
        Assert.That(result.Picture.DescriptionEn, Is.EqualTo("Description EN"));
        Assert.That(result.Picture.DescriptionRm, Is.EqualTo("Description RM"));
        Assert.That(result.Picture.SourceDe, Is.EqualTo("Source DE"));
        Assert.That(result.Picture.SourceFr, Is.EqualTo("Source FR"));
        Assert.That(result.Picture.SourceIt, Is.EqualTo("Source IT"));
        Assert.That(result.Picture.SourceEn, Is.EqualTo("Source EN"));
        Assert.That(result.Picture.SourceRm, Is.EqualTo("Source RM"));
    }

    [Test]
    public void ShouldMapEntryLanguageDetailForAdmin()
    {
        authorizationServiceMock.IsAdmin().Returns(true);
        authorizationServiceMock.IsSuperUser().Returns(false);
        authorizationServiceMock.IsWriter().Returns(false);

        var result = mapper.Map<EntryLanguageDetail>(entryLanguage);

        AssertGenerateEntryLanguageDetail(entryLanguage, result);
        Assert.That(result.Metadata, Is.EqualTo(entryLanguage.Metadata));
        Assert.That(result.MetadataSource, Is.EqualTo(entryLanguage.MetadataSource));
    }

    [Test]
    public void ShouldMapEntryLanguageDetailForSuperUser()
    {
        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsSuperUser().Returns(true);
        authorizationServiceMock.IsWriter().Returns(false);

        var result = mapper.Map<EntryLanguageDetail>(entryLanguage);

        AssertGenerateEntryLanguageDetail(entryLanguage, result);
        Assert.That(result.Metadata, Is.EqualTo(entryLanguage.Metadata));
        Assert.That(result.MetadataSource, Is.EqualTo(entryLanguage.MetadataSource));
    }

    [Test]
    public void ShouldMapEntryLanguageDetailForWriter()
    {
        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsSuperUser().Returns(false);
        authorizationServiceMock.IsWriter().Returns(true);

        var result = mapper.Map<EntryLanguageDetail>(entryLanguage);

        AssertGenerateEntryLanguageDetail(entryLanguage, result);
        Assert.That(result.Metadata, Is.EqualTo(entryLanguage.Metadata));
        Assert.That(result.MetadataSource, Is.EqualTo(entryLanguage.MetadataSource));
    }

    [Test]
    public void Map_ToEntryLanguageDetail_ShouldMapNoteSubCodeTooltips()
    {
        authorizationServiceMock.IsAdmin().Returns(false);
        authorizationServiceMock.IsSuperUser().Returns(false);
        authorizationServiceMock.IsWriter().Returns(false);

        var result = mapper.Map<EntryLanguageDetail>(entryLanguage);

        Assert.That(result.NoteTooltips, Has.Count.EqualTo(2));
        Assert.That(result.NoteTooltips.First().Code, Is.EqualTo("USG"));
        Assert.That(result.NoteTooltips.Last().Code, Is.EqualTo("EXP"));
    }

    [Test]
    public void ShouldMapEntryCreate()
    {
        var faker = new Faker<EntryCreate>();
        var entryCreate = faker.Generate();
        entryCreate.ExternalSystemId = "42";

        var result = mapper.Map<Entry>(entryCreate);

        AssertEntryModification(result, entryCreate);
        Assert.That(result.ExternalSystemId, Is.EqualTo(entryCreate.ExternalSystemId));
    }

    [Test]
    public void ShouldMapEntryUpdate()
    {
        var faker = new Faker<EntryUpdate>();
        var entryUpdate = faker.Generate();

        var result = mapper.Map<Entry>(entryUpdate);

        AssertEntryModification(result, entryUpdate);
    }

    [Test]
    public void Map_ShouldMapToWatchlistEntry()
    {
        entry.ConceptSystemPosition = new()
        {
            Id = 1,
            Value = "ConceptSystem",
        };
        var translations = entry.Translations.ToList();
        translations[0].Language = new Language()
        {
            IsoCode = "current"
        };
        translations[1].Language = new Language()
        {
            IsoCode = "default"
        };
        translations[2].Language = new Language()
        {
            IsoCode = "other"
        };

        var result = mapper.Map<WatchlistEntry>(entry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryId, Is.EqualTo(entry.Id));
        Assert.That(result.Identification, Is.EqualTo(entry.Identification));
        Assert.That(result.Status, Is.EqualTo((ResourceModels.Status)entry.Status));
        Assert.That(result.Deleted, Is.EqualTo(entry.Deleted));
        Assert.That(result.Collection, Is.Not.Null);
        Assert.That(result.Collection, Is.InstanceOf<CollectionSummary>());
        Assert.That(result.ConceptSystemPosition, Is.Not.Null);
        Assert.That(result.ConceptSystemPosition, Is.InstanceOf<ConceptSystemPositionSummary>());
        Assert.That(result.Value, Is.EqualTo("Einheit der Masse"));
        Assert.That(result.Glossaries, Is.EqualTo("Glo1"));
        Assert.That(result.AdminInfos, Is.EqualTo("adminInfo1, adminInfo2"));

        translations[0].Terminus = null;
        var resultName = mapper.Map<WatchlistEntry>(entry);
        Assert.That(resultName.Value, Is.EqualTo("Kilogramm"));

        translations[0].Name = null;
        var resultAbbreviation = mapper.Map<WatchlistEntry>(entry);
        Assert.That(resultAbbreviation.Value, Is.EqualTo("kg"));

        translations[0].Abbreviation = null;
        translations[0].Phraseology = "Phraseology";
        var resultPhraseology = mapper.Map<WatchlistEntry>(entry);
        Assert.That(resultPhraseology.Value, Is.EqualTo("Phraseology"));

        translations[0].Language.IsoCode = "default";
        translations[1].Language.IsoCode = "current";
        var resultFromOtherLanguage = mapper.Map<WatchlistEntry>(entry);
        Assert.That(resultFromOtherLanguage.Value, Is.EqualTo("unité de masse"));
    }

    [Test]
    public void ShouldMapEntrySummary()
    {
        var result = mapper.Map<EntrySummary>(entry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(entry.Id));
        Assert.That((int)result.Status, Is.EqualTo((int)entry.Status));
        Assert.That(result.Deleted, Is.EqualTo(entry.Deleted));
        Assert.That(result.IsMainOffice, Is.True);
        Assert.That(result.Terms, Is.Not.Null);
        Assert.That(result.Terms.Count, Is.EqualTo(3));
    }

    [Test]
    public void ShouldMapEntryReference()
    {
        var entryModificationReference = new EntryModificationReference
        {
            Id = 1,
            Type = ResourceModels.ReferenceType.General,
            EntryId = 2,
        };

        var result = mapper.Map<EntryReference>(entryModificationReference);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(entryModificationReference.Id));
        Assert.That(result.EntryId, Is.EqualTo(default(int)));
        Assert.That(result.Type, Is.EqualTo((DomainModels.ReferenceType)entryModificationReference.Type));
        Assert.That(result.ReferencedEntryId, Is.EqualTo(entryModificationReference.EntryId));
    }

    [Test]
    public void ShouldMapEntryReferenceDetail()
    {
        var translation = entry.Translations.ToList();
        translation[0].Phraseology = "Phraseology";

        var result = mapper.Map<EntryReferenceDetail>(entry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(default(int)));
        Assert.That(result.EntryId, Is.EqualTo(entry.Id));
        Assert.That(result.Type, Is.Null);
        Assert.That(result.Languages, Is.Not.Null);
        Assert.That(result.Languages.Count, Is.EqualTo(3));

        Assert.That(result.Languages[0].LanguageId, Is.EqualTo(translation[0].LanguageId));
        Assert.That(result.Languages[0].Text, Is.EqualTo(translation[0].Terminus));

        translation[0].Terminus = null;
        result = mapper.Map<EntryReferenceDetail>(entry);
        Assert.That(result.Languages[0].LanguageId, Is.EqualTo(translation[0].LanguageId));
        Assert.That(result.Languages[0].Text, Is.EqualTo(translation[0].Name));

        translation[0].Name = null;
        result = mapper.Map<EntryReferenceDetail>(entry);
        Assert.That(result.Languages[0].LanguageId, Is.EqualTo(translation[0].LanguageId));
        Assert.That(result.Languages[0].Text, Is.EqualTo(translation[0].Phraseology));

        translation[0].Phraseology = null;
        result = mapper.Map<EntryReferenceDetail>(entry);
        Assert.That(result.Languages[0].LanguageId, Is.EqualTo(translation[0].LanguageId));
        Assert.That(result.Languages[0].Text, Is.EqualTo(translation[0].Abbreviation));
    }

    [Test]
    public void ShouldMapEntryReferenceToResourceModels()
    {
        var shouldNotBeMapped = GenerateReferencedEntryLanguage(referencedEntry1.Id, 1);
        shouldNotBeMapped.Sequence = 2;
        var shouldNotBeMapped2 = GenerateReferencedEntryLanguage(referencedEntry1.Id, 2);
        shouldNotBeMapped2.Sequence = 2;
        referencedEntry1.Translations.Add(shouldNotBeMapped);
        referencedEntry1.Translations.Add(shouldNotBeMapped2);
        referencedEntry4.Translations.Add(shouldNotBeMapped);
        referencedEntry4.Translations.Add(shouldNotBeMapped2);

        var entryReferenceDetail = mapper.Map<EntryReferenceDetail>(entry.References.First());
        Assert.That(entryReferenceDetail, Is.Not.Null);
        Assert.That(entryReferenceDetail.Id, Is.EqualTo(1));
        Assert.That(entryReferenceDetail.EntryId, Is.EqualTo(2));
        Assert.That(entryReferenceDetail.Type, Is.EqualTo(ResourceModels.ReferenceType.General));
        Assert.That(entryReferenceDetail.Languages, Is.Not.Null);
        Assert.That(entryReferenceDetail.Languages.Count, Is.EqualTo(4));
        var languages = entryReferenceDetail.Languages.ToList();
        Assert.That(languages[0].LanguageId, Is.EqualTo(1));
        Assert.That(languages[0].Text, Is.EqualTo("Referenced Entry 2 Language 1"));
        Assert.That(languages[1].LanguageId, Is.EqualTo(2));
        Assert.That(languages[1].Text, Is.EqualTo("Referenced Entry 2 Language 2"));
        Assert.That(languages[2].LanguageId, Is.EqualTo(3));
        Assert.That(languages[2].Text, Is.EqualTo("Referenced Entry 2 Language 3"));
        Assert.That(languages[3].LanguageId, Is.EqualTo(4));
        Assert.That(languages[3].Text, Is.EqualTo("Referenced Entry 2 Language 4"));

        var entryReferencedByDetail = mapper.Map<EntryReferencedByDetail>(entry.ReferencedByEntries.First());
        Assert.That(entryReferencedByDetail, Is.Not.Null);
        Assert.That(entryReferencedByDetail.EntryId, Is.EqualTo(4));
        Assert.That(entryReferencedByDetail.Type, Is.EqualTo(ResourceModels.ReferenceType.Antonym));
        var languages2 = entryReferencedByDetail.Languages.ToList();
        Assert.That(languages2[0].LanguageId, Is.EqualTo(1));
        Assert.That(languages2[0].Text, Is.EqualTo("Referenced Entry 4 Language 1"));
        Assert.That(languages2[1].LanguageId, Is.EqualTo(2));
        Assert.That(languages2[1].Text, Is.EqualTo("Referenced Entry 4 Language 2"));
        Assert.That(languages2[2].LanguageId, Is.EqualTo(3));
        Assert.That(languages2[2].Text, Is.EqualTo("Referenced Entry 4 Language 3"));
        Assert.That(languages2[3].LanguageId, Is.EqualTo(4));
        Assert.That(languages2[3].Text, Is.EqualTo("Referenced Entry 4 Language 4"));
    }

    [TestCase(true, true, true, true, true, true, true, true)]
    [TestCase(false, true, true, true, false, true, true, true)]
    [TestCase(false, false, true, true, false, false, true, true)]
    [TestCase(false, false, false, true, false, false, false, true)]
    [TestCase(false, false, false, false, false, false, false, false)]
    public void Map_WithAuthorizationConfiguration_ReturnsActionsCorrectly(bool canEdit, bool canEditCollection, bool canDelete, bool canRestore, bool expectedEdit, bool expectedCanEditCollection, bool expectedDelete, bool expectedRestore)
    {
        authorizationServiceMock.CanEdit(Arg.Any<Entry>()).Returns(canEdit);
        authorizationServiceMock.HasAccessToCollection(Arg.Any<int>()).Returns(canEditCollection);
        authorizationServiceMock.CanDelete(Arg.Any<Entry>()).Returns(canDelete);
        authorizationServiceMock.CanRestore(Arg.Any<Entry>()).Returns(canRestore);

        var result = mapper.Map<EntryDetail>(entry);

        Assert.That(result.CanEdit, Is.EqualTo(expectedEdit));
        Assert.That(result.CanEditCollection, Is.EqualTo(expectedCanEditCollection));
        Assert.That(result.CanDelete, Is.EqualTo(expectedDelete));
        Assert.That(result.CanRestore, Is.EqualTo(expectedRestore));
    }

    [Test]
    public void Map_ToEntrySummary_ReturnedSummaryCanBeAddedToWatchList()
    {
        authorizationServiceMock.CanAddToWatchList(entry).Returns(true);

        var mappedSummary = mapper.Map<EntrySummary>(entry);

        Assert.Multiple(() =>
        {
            Assert.That(mappedSummary, Is.Not.Null);
            Assert.That(mappedSummary.CanBeAddedToWatchList, Is.True);
        });
    }

    [Test]
    public void Map_ToEntrySummary_ReturnedSummaryCannotBeAddedToWatchList()
    {
        authorizationServiceMock.CanEdit(entry).Returns(false);

        var mappedSummary = mapper.Map<EntrySummary>(entry);

        Assert.Multiple(() =>
        {
            Assert.That(mappedSummary, Is.Not.Null);
            Assert.That(mappedSummary.CanBeAddedToWatchList, Is.False);
        });
    }

    [Test]
    public void Map_FromEntryTemplateToEntrySummary_MapsCorrectly()
    {
        var result = mapper.Map<EntryTemplateSummary>(entryTemplate);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(entryTemplate.Id));
            Assert.That(result.TemplateName, Is.EqualTo(entryTemplate.Name));
        });
    }

    private static void AssertEntryModification(Entry result, IEntryModification entryModification)
    {
        Assert.That((int)result.Status, Is.EqualTo((int)entryModification.Status));
        Assert.That((int)result.Reliability, Is.EqualTo((int)entryModification.Reliability));
        Assert.That(result.Comment, Is.EqualTo(entryModification.Comment));
        Assert.That(result.AdminInfos, Is.EqualTo(entryModification.AdminInfos));
        Assert.That(result.Identification, Is.EqualTo(entryModification.Identification));
        Assert.That(result.OfficeId, Is.EqualTo(entryModification.OfficeId));
        Assert.That(result.CollectionId, Is.EqualTo(entryModification.CollectionId));
        Assert.That(result.ConceptSystemPositionId, Is.EqualTo(entryModification.ConceptSystemPositionId));
        foreach (var entryLanguageTranslation in result.Translations)
        {
            var languageDetail = entryModification.LanguageDetails.Single(el =>
                el.LanguageId == entryLanguageTranslation.LanguageId && el.Sequence == entryLanguageTranslation.Sequence);

            Assert.That(entryLanguageTranslation.Id, Is.EqualTo(languageDetail.Id));
            Assert.That(entryLanguageTranslation.Terminus, Is.EqualTo(languageDetail.Terminus));
            Assert.That(entryLanguageTranslation.TerminusSource, Is.EqualTo(languageDetail.TerminusSource));
            Assert.That(entryLanguageTranslation.Name, Is.EqualTo(languageDetail.Name));
            Assert.That(entryLanguageTranslation.NameSource, Is.EqualTo(languageDetail.NameSource));
            Assert.That(entryLanguageTranslation.Phraseology, Is.EqualTo(languageDetail.Phraseology));
            Assert.That(entryLanguageTranslation.PhraseologySource, Is.EqualTo(languageDetail.PhraseologySource));
            Assert.That(entryLanguageTranslation.Abbreviation, Is.EqualTo(languageDetail.Abbreviation));
            Assert.That(entryLanguageTranslation.AbbreviationSource, Is.EqualTo(languageDetail.AbbreviationSource));
            Assert.That(entryLanguageTranslation.Definition, Is.EqualTo(languageDetail.Definition));
            Assert.That(entryLanguageTranslation.DefinitionSource, Is.EqualTo(languageDetail.DefinitionSource));
            Assert.That(entryLanguageTranslation.Note, Is.EqualTo(languageDetail.Note));
            Assert.That(entryLanguageTranslation.NoteSource, Is.EqualTo(languageDetail.NameSource));
            Assert.That(entryLanguageTranslation.Context, Is.EqualTo(languageDetail.Context));
            Assert.That(entryLanguageTranslation.ContextSource, Is.EqualTo(languageDetail.ContextSource));
            Assert.That(entryLanguageTranslation.Country, Is.EqualTo(languageDetail.Country));
            Assert.That(entryLanguageTranslation.Metadata, Is.EqualTo(languageDetail.Metadata));
            Assert.That(entryLanguageTranslation.MetadataSource, Is.EqualTo(languageDetail.MetadataSource));
        }
    }

    private static void AssertIEntryDetail(Entry domainEntry, IEntryDetail resourceEntry)
    {
        Assert.That(resourceEntry, Is.Not.Null);
        Assert.That(resourceEntry.Id, Is.EqualTo(domainEntry.Id));
        Assert.That((int)resourceEntry.Status, Is.EqualTo((int)domainEntry.Status));
        Assert.That((int)resourceEntry.Reliability, Is.EqualTo((int)domainEntry.Reliability));
        Assert.That(resourceEntry.Deleted, Is.EqualTo(domainEntry.Deleted));

        Assert.That(resourceEntry.Collection, Is.Not.Null);
        Assert.That(resourceEntry.Collection.Id, Is.EqualTo(domainEntry.Collection.Id));
        Assert.That(resourceEntry.Collection.Code, Is.EqualTo(domainEntry.Collection.Code));
        Assert.That(resourceEntry.Collection.Name, Is.EqualTo("Current Translation"));

        Assert.That(resourceEntry.Office, Is.Not.Null);
        Assert.That(resourceEntry.Office, Is.InstanceOf<OfficeSummary>());
        Assert.That(resourceEntry.Office.Id, Is.EqualTo(domainEntry.Office.Id));
        Assert.That(resourceEntry.Office.Code, Is.EqualTo(domainEntry.Office.Code));
        Assert.That(resourceEntry.Office.Name, Is.EqualTo("Current Translation"));

        Assert.That(resourceEntry.LenochClassifications, Is.Not.Null);
        Assert.That(resourceEntry.LenochClassifications.Count, Is.EqualTo(1));

        Assert.That(resourceEntry.ConceptSystemPosition, Is.Not.Null);
        Assert.That(resourceEntry.ConceptSystemPosition.Id, Is.EqualTo(domainEntry.ConceptSystemPosition.Id));
        Assert.That(resourceEntry.ConceptSystemPosition.Value, Is.EqualTo(domainEntry.ConceptSystemPosition.Value));

        Assert.That(resourceEntry.Picture, Is.Not.Null);
        Assert.That(resourceEntry.Picture, Is.InstanceOf<PictureDetail>());
    }

    private static void AssertGeneralEntryDetail(Entry domainEntry, EntryDetail resourceEntry)
    {
        AssertIEntryDetail(domainEntry, resourceEntry);

        Assert.That(resourceEntry.Created, Is.EqualTo(domainEntry.Created));
        Assert.That(resourceEntry.Modified, Is.EqualTo(domainEntry.Modified));
        Assert.That(resourceEntry.IsMainOffice, Is.True);

        Assert.That(resourceEntry.LanguageDetails, Is.Not.Null);
        Assert.That(resourceEntry.LanguageDetails.Count, Is.EqualTo(3));

        Assert.That(resourceEntry.ReferencedBy, Is.Not.Null);
        Assert.That(resourceEntry.ReferencedBy.Count, Is.EqualTo(1));
        Assert.That(resourceEntry.ReferencedBy[0].EntryId, Is.EqualTo(5));
    }

    private static void AssertGeneralEntryModificationDetail(Entry domainEntry, EntryModificationDetail resourceEntry)
    {
        AssertIEntryDetail(domainEntry, resourceEntry);

        Assert.That(resourceEntry.References, Is.Not.Null);
        Assert.That(resourceEntry.References.Count, Is.EqualTo(3));
        Assert.That(resourceEntry.References[0].EntryId, Is.EqualTo(3));
        Assert.That(resourceEntry.References[1].EntryId, Is.EqualTo(2));
        Assert.That(resourceEntry.References[2].EntryId, Is.EqualTo(4));

        Assert.That(resourceEntry.LanguageDetails, Is.Not.Null);
        Assert.That(resourceEntry.LanguageDetails.Count, Is.EqualTo(3));
    }

    private static void AssertGenerateEntryLanguageDetail(EntryLanguage language, EntryLanguageDetail detail)
    {
        Assert.That(detail, Is.Not.Null);
        Assert.That(detail.Id, Is.EqualTo(language.Id));
        Assert.That(detail.LanguageId, Is.EqualTo(language.LanguageId));
        Assert.That(detail.Sequence, Is.EqualTo(language.Sequence));
        Assert.That(detail.Terminus, Is.EqualTo(language.Terminus));
        Assert.That(detail.TerminusSource, Is.EqualTo(language.TerminusSource));
        Assert.That(detail.Name, Is.EqualTo(language.Name));
        Assert.That(detail.NameSource, Is.EqualTo(language.NameSource));
        Assert.That(detail.Abbreviation, Is.EqualTo(language.Abbreviation));
        Assert.That(detail.AbbreviationSource, Is.EqualTo(language.AbbreviationSource));
        Assert.That(detail.Phraseology, Is.EqualTo(language.Phraseology));
        Assert.That(detail.PhraseologySource, Is.EqualTo(language.PhraseologySource));
        Assert.That(detail.Definition, Is.EqualTo(language.Definition));
        Assert.That(detail.DefinitionSource, Is.EqualTo(language.DefinitionSource));
        Assert.That(detail.Note, Is.EqualTo(language.Note));
        Assert.That(detail.NoteSource, Is.EqualTo(language.NoteSource));
        Assert.That(detail.Context, Is.EqualTo(language.Context));
        Assert.That(detail.ContextSource, Is.EqualTo(language.ContextSource));
        Assert.That(detail.Country, Is.EqualTo(language.Country));
        Assert.That(detail.IsRare, Is.EqualTo(language.IsRare));
        Assert.That(detail.IsObsolete, Is.EqualTo(language.IsObsolete));
        Assert.That(detail.ShouldAvoid, Is.EqualTo(language.ShouldAvoid));
        Assert.That(detail.ExternalSystemId, Is.EqualTo(language.ExternalSystemId));
    }

    private static Entry GenerateReferencedEntry(int entryId)
    {
        return new Entry
        {
            Id = entryId,
            Translations = new List<EntryLanguage>
            {
                GenerateReferencedEntryLanguage(entryId, 1),
                GenerateReferencedEntryLanguage(entryId, 2),
                GenerateReferencedEntryLanguage(entryId, 3),
                GenerateReferencedEntryLanguage(entryId, 4),
            }
        };
    }

    private static EntryLanguage GenerateReferencedEntryLanguage(int entryId, int languageId)
    {
        return new()
        {
            EntryId = entryId,
            LanguageId = languageId,
            Language = new()
            {
                Id = languageId,
                SortIndex = languageId
            },
            Sequence = 1,
            Name = $"Referenced Entry {entryId} Language {languageId}"
        };
    }
}
