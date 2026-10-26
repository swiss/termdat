using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Bk.Termdat.Business.Mapping.Actions;
using Bk.Termdat.Business.Mapping.Profiles;
using Bk.Termdat.Business.Mapping.Resolvers;
using Bk.Termdat.Business.Resource.Models.Export;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Tests.Mapping.Profiles;

[TestFixture]
internal class ExportMappingProfileTests
{
    private IMapper mapper;
    private ITermdatUrlService termdatUrlServiceMock;
    private readonly ICultureService cultureServiceMock = Substitute.For<ICultureService>();
    private DomainModels.Entry entry;
    private DomainModels.Entry referencedEntry1;
    private DomainModels.Entry referencedEntry2;
    private DomainModels.Entry referencedEntry3;
    private DomainModels.Entry referencedEntry4;

    [SetUp]
    public void SetUp()
    {
        termdatUrlServiceMock = Substitute.For<ITermdatUrlService>();
        termdatUrlServiceMock.GetTermdatUrl(default).ReturnsForAnyArgs("https://localhost/test");

        cultureServiceMock.ClearSubstitute();
        cultureServiceMock.GetTranslation(Arg.Any<IEnumerable<DomainModels.IEntityNameTranslation>>()).Returns(x =>
        {
            var translations = x.Arg<IEnumerable<DomainModels.IEntityNameTranslation>>();
            return translations.FirstOrDefault(t => t.Language.IsoCode == "current")?.Name;
        });
        cultureServiceMock.GetCurrentIsoCode().ReturnsForAnyArgs("current");

        var services = new ServiceCollection();
        services.AddScoped(_ => cultureServiceMock);
        services.AddScoped(_ => termdatUrlServiceMock);
        services.AddTransient<WatchlistExportEntryValueResolver>();
        services.AddTransient<OfficeDisplayNameResolver>();
        services.AddTransient<CollectionDisplayNameResolver>();
        services.AddTransient<RelatedCollectionsResolver>();
        services.AddTransient<GlossariesResolver>();
        services.AddTransient<TermdatClassificationsResolver>();
        services.AddTransient<ExportWatchlistEntryLanguageItemAction>();
        var provider = services.BuildServiceProvider();

        using var loggerFactory = LoggerFactory.Create(_ => { });
        var config = new MapperConfiguration(c =>
        {
            c.ConstructServicesUsing(provider.GetRequiredService);
            c.AddProfile(new ExportMappingProfile());
        }, loggerFactory);
        mapper = config.CreateMapper();

        referencedEntry1 = GenerateReferencedEntry(2);
        referencedEntry2 = GenerateReferencedEntry(3);
        referencedEntry3 = GenerateReferencedEntry(4);
        referencedEntry3.Deleted = true;
        referencedEntry4 = GenerateReferencedEntry(5);

        entry = new DomainModels.Entry
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
            AdminInfo = "AdminInfo",
            Comment = "Comment",
            Deleted = true,
            Office = new()
            {
                Id = 10,
                Code = "Office",
                Translations = new List<DomainModels.OfficeTranslation>
                {
                    new()
                    {
                        Name = "Default Office Translation",
                        Language = new()
                        {
                            IsoCode = "default"
                        }
                    },
                    new()
                    {
                        Name = "Current Office Translation",
                        Language = new()
                        {
                            IsoCode = "current"
                        }
                    }
                }
            },
            Collection = new DomainModels.Collection()
            {
                Id = 100,
                Code = "Collection",
                Translations = new List<DomainModels.CollectionTranslation>
                {
                    new()
                    {
                        Name = "Default Collection Translation",
                        Language = new()
                        {
                            IsoCode = "default"
                        }
                    },
                    new()
                    {
                        Name = "Current Collection Translation",
                        Language = new()
                        {
                            IsoCode = "current"
                        }
                    }
                }
            },
            RelatedCollections = new List<DomainModels.Collection>
            {
                new()
                {
                    Id = 101,
                    Code = "RelatedCollection",
                    Translations = new List<DomainModels.CollectionTranslation>
                    {
                        new()
                        {
                            Name = "Default 101 Collection Translation",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current 101 Collection Translation",
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
                    Translations = new List<DomainModels.CollectionTranslation>
                    {
                        new()
                        {
                            Name = "Default 102 Collection Translation",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current 102 Collection Translation",
                            Language = new()
                            {
                                IsoCode = "current"
                            }
                        }
                    }
                },
            },
            Glossaries = new List<DomainModels.Glossary>
            {
                new()
                {
                    Code = "Test Glossary Code",
                    Translations = new List<DomainModels.GlossaryTranslation>()
                    {
                        new()
                        {
                            Name = "Default Glossary Translation",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current Glossary Translation",
                            Language = new()
                            {
                                IsoCode = "current"
                            }
                        }
                    }
                }
            },
            LenochClassifications = new List<DomainModels.LenochClassification>
            {
                new()
                {
                    Id = 200,
                    Code = "Domain",
                    Translations = new List<DomainModels.LenochClassificationTranslation>()
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
                Translations = new List<DomainModels.PictureTranslation>
                {
                    new()
                    {
                        Id = 10001, Description = "Description DE", Source = "Source DE",
                        Language = new() {IsoCode = IsoCode.De.ToString()}
                    },
                    new()
                    {
                        Id = 10002, Description = "Description FR", Source = "Source FR",
                        Language = new() {IsoCode = IsoCode.Fr.ToString()}
                    },
                    new()
                    {
                        Id = 10003, Description = "Description IT", Source = "Source IT",
                        Language = new() {IsoCode = IsoCode.It.ToString()}
                    },
                    new()
                    {
                        Id = 10004, Description = "Description RM", Source = "Source RM",
                        Language = new() {IsoCode = IsoCode.Rm.ToString()}
                    },
                    new()
                    {
                        Id = 10005, Description = "Description EN", Source = "Source EN",
                        Language = new() {IsoCode = IsoCode.En.ToString()}
                    },
                }
            },
            Translations = new List<DomainModels.EntryLanguage>
            {
                new()
                {
                    Id = 1,
                    EntryId = 1,
                    LanguageId = 1,
                    Sequence = 1,
                    Terminus = "Einheit der Masse",
                    TerminusSource = "Terminus Quelle",
                    Abbreviation = "kg",
                    AbbreviationSource = "Some Link [here](http://some.link)",
                    Definition = "Masseinheit, die gleich der Masse des Internationalen Kilogrammprototyps ist.",
                    Name = "Kilogramm",
                    Phraseology = null,
                    Language = new()
                    {
                        Id = 1,
                        SortIndex = 1,
                        IsoCode = "current",
                    },
                },
                new()
                {
                    Id = 2, EntryId = 1, LanguageId = 2, Sequence = 1, Abbreviation = "kg",
                    Definition = "Unité de mesure égale à la masse du prototype international du kilogramme. ",
                    Name = "kilogramme", Phraseology = null, Terminus = "unité de masse", Language = new()
                    {
                        Id = 2,
                        SortIndex = 2,
                        IsoCode = "default",
                    },
                },
                new()
                {
                    Id = 3, EntryId = 1, LanguageId = 3, Sequence = 1, Abbreviation = "kg",
                    Definition = "Unità di misura pari alla massa del prototipo internazionale del chilogrammo.",
                    Name = "chilogrammo", Phraseology = null, Terminus = "unità di massa", Language = new()
                    {
                        Id = 3,
                        SortIndex = 3,
                        IsoCode = "other",
                    },
                },
            },
            ConceptSystemPosition = new()
            {
                Id = 1000,
                Value = "ConceptSystemPosition"
            },
            References = new List<DomainModels.EntryReference>
            {
                new()
                {
                    Id = 1, EntryId = 1, ReferencedEntryId = 2, Type = DomainModels.ReferenceType.General,
                    ReferencedEntry = referencedEntry1
                },
                new()
                {
                    Id = 2, EntryId = 1, ReferencedEntryId = 3, Type = DomainModels.ReferenceType.None,
                    ReferencedEntry = referencedEntry2
                },
                new()
                {
                    Id = 3, EntryId = 1, ReferencedEntryId = 4, Type = DomainModels.ReferenceType.Antonym,
                    ReferencedEntry = referencedEntry3
                },
            },
            ReferencedByEntries = new List<DomainModels.EntryReference>
            {
                new()
                {
                    Id = 4, ReferencedEntryId = 1, EntryId = 4, Type = DomainModels.ReferenceType.Antonym,
                    Entry = referencedEntry3
                },
                new()
                {
                    Id = 5, ReferencedEntryId = 1, EntryId = 5, Type = DomainModels.ReferenceType.Specific,
                    Entry = referencedEntry4
                }
            },
            Subjects = new List<DomainModels.Subject>
            {
                new()
                {
                    Id = 1,
                    Translations = new List<DomainModels.SubjectTranslation>
                    {
                        new()
                        {
                            Name = "Default Subject 1 Translation",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current Subject 1 Translation",
                            Language = new()
                            {
                                IsoCode = "current"
                            }
                        }
                    }
                },
                new()
                {
                    Id = 2,
                    Translations = new List<DomainModels.SubjectTranslation>
                    {
                        new()
                        {
                            Name = "Default Subject 2 Translation",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current Subject 2 Translation",
                            Language = new()
                            {
                                IsoCode = "current"
                            }
                        }
                    }
                }
            },
            Domains = new List<DomainModels.Domain>
            {
                new()
                {
                    Id = 42,
                    SubjectId = 2,
                    Translations = new List<DomainModels.DomainTranslation>
                    {
                        new()
                        {
                            Name = "Default Domain 42 Translation",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current Domain 42 Translation",
                            Language = new()
                            {
                                IsoCode = "current"
                            }
                        }
                    }
                },
                new()
                {
                    Id = 43,
                    SubjectId = 2,
                    Translations = new List<DomainModels.DomainTranslation>
                    {
                        new()
                        {
                            Name = "Default Domain 43 Translation",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current Domain 43 Translation",
                            Language = new()
                            {
                                IsoCode = "current"
                            }
                        }
                    }
                },
                new()
                {
                    Id = 44,
                    SubjectId = 2,
                    Translations = new List<DomainModels.DomainTranslation>
                    {
                        new()
                        {
                            Name = "Default Domain 44 Translation",
                            Language = new()
                            {
                                IsoCode = "default"
                            }
                        },
                        new()
                        {
                            Name = "Current Domain 44 Translation",
                            Language = new()
                            {
                                IsoCode = "current"
                            }
                        }
                    }
                }
            }
        };
    }

    [Test]
    public void ShouldMapExportEntryProperties()
    {
        var result = mapper.Map<WatchlistExportEntry>(entry);

        Assert.That(result.Value, Is.EqualTo("Einheit der Masse"));
        Assert.That(result.EntryId, Is.EqualTo(entry.Id));
        Assert.That(result.CreatedKey, Is.EqualTo(BackendTexts.EntryCreated));
        Assert.That(result.Created, Is.Not.Null);
        Assert.That(result.CreatedBy, Is.EqualTo(entry.CreatedBy));
        Assert.That(result.ModifiedKey, Is.EqualTo(BackendTexts.EntryModified));
        Assert.That(result.Modified, Is.Not.Null);
        Assert.That(result.ModifiedBy, Is.EqualTo(entry.ModifiedBy));
        Assert.That(result.Office, Is.EqualTo("Office - Current Office Translation"));
        Assert.That(result.Collection, Is.EqualTo("Collection - Current Collection Translation"));
        Assert.That(result.RelatedCollectionsKey, Is.EqualTo(BackendTexts.EntryRelatedCollections));
        Assert.That(result.RelatedCollections, Is.EqualTo("Current 101 Collection Translation, Current 102 Collection Translation"));
        Assert.That(result.GlossariesKey, Is.EqualTo(BackendTexts.Glossaries));
        Assert.That(result.Glossaries, Is.EqualTo("Current Glossary Translation"));
        Assert.That(result.AdminInfosKey, Is.EqualTo(BackendTexts.EntryAdminInfo));
        Assert.That(result.AdminInfos, Is.Null);
        Assert.That(result.ConceptPositionKey, Is.EqualTo(BackendTexts.EntryConceptSystemPosition));
        Assert.That(result.ConceptPosition, Is.EqualTo(entry.ConceptSystemPosition.Value));
        Assert.That(result.CommentKey, Is.EqualTo(BackendTexts.EntryComment));
        Assert.That(result.Comment, Is.EqualTo(entry.Comment));
        Assert.That(result.StatusId, Is.EqualTo((int)entry.Status));
        Assert.That(result.Status, Is.EqualTo(BackendTexts.StatusValidated));
        Assert.That(result.Reliability, Is.EqualTo($"5 - {BackendTexts.ReliabilityApproved}"));
    }

    [Test]
    public void ShouldMapPictureForEntry()
    {
        var result = mapper.Map<WatchlistExportEntry>(entry);

        Assert.That(result.Picture, Is.Not.Null);
        Assert.That(result.Picture, Is.EqualTo(Convert.ToBase64String(Encoding.ASCII.GetBytes("TEST"))));
    }

    [Test]
    public void ShouldMapTermdatClassifications()
    {
        var result = mapper.Map<WatchlistExportEntry>(entry);

        Assert.That(result.TermdatClassifications.Count, Is.EqualTo(2));

        var firstClassification = result.TermdatClassifications[0];
        Assert.That(firstClassification.Subject, Is.EqualTo("Current Subject 1 Translation"));
        Assert.That(firstClassification.Domains, Is.Empty);

        var secondClassification = result.TermdatClassifications[1];
        Assert.That(secondClassification.Subject, Is.EqualTo("Current Subject 2 Translation"));
        Assert.That(secondClassification.Domains.Count, Is.EqualTo(3));
        Assert.That(secondClassification.Domains[0], Is.EqualTo("Current Domain 42 Translation"));
        Assert.That(secondClassification.Domains[1], Is.EqualTo("Current Domain 43 Translation"));
        Assert.That(secondClassification.Domains[2], Is.EqualTo("Current Domain 44 Translation"));
    }

    [Test]
    public void ShouldMapLanguageItems()
    {
        var result = mapper.Map<WatchlistExportEntry>(entry);

        Assert.That(result.LanguageItems.Count, Is.EqualTo(3));
        Assert.That(result.LanguageItems[0].IsoCode, Is.EqualTo("current"));
        Assert.That(result.LanguageItems[1].IsoCode, Is.EqualTo("default"));
        Assert.That(result.LanguageItems[2].IsoCode, Is.EqualTo("other"));
    }

    [Test]
    public void ShouldMapLanguageItemDetail()
    {
        var result = mapper.Map<WatchlistExportEntry>(entry);

        Assert.That(result.LanguageItems.Count, Is.EqualTo(3));
        Assert.That(result.LanguageItems[0].IsoCode, Is.EqualTo("current"));
        Assert.That(result.LanguageItems[0].Details.Count, Is.EqualTo(1));

        var detail = result.LanguageItems[0].Details[0];

        Assert.That(detail.TerminusKey, Is.EqualTo(BackendTexts.EntryLanguageTerminus));
        Assert.That(detail.Terminus, Is.EqualTo("Einheit der Masse"));
        Assert.That(detail.TerminusSource, Is.EqualTo("Terminus Quelle"));

        Assert.That(detail.NameKey, Is.EqualTo(BackendTexts.EntryLanguageName));
        Assert.That(detail.Name, Is.EqualTo("Kilogramm"));
        Assert.That(detail.NameSource, Is.Null);

        Assert.That(detail.AbbreviationKey, Is.EqualTo(BackendTexts.EntryLanguageAbbreviation));
        Assert.That(detail.Abbreviation, Is.EqualTo("kg"));
        Assert.That(detail.AbbreviationSource, Is.EqualTo("Some Link <a href=\"http://some.link\">here</a>"));

        Assert.That(detail.PhraseologyKey, Is.EqualTo(BackendTexts.EntryLanguagePhraseology));
        Assert.That(detail.Phraseology, Is.Null);
        Assert.That(detail.PhraseologySource, Is.Null);

        Assert.That(detail.DefinitionKey, Is.EqualTo(BackendTexts.EntryLanguageDefinition));
        Assert.That(detail.Definition, Is.EqualTo("Masseinheit, die gleich der Masse des Internationalen Kilogrammprototyps ist."));
        Assert.That(detail.DefinitionSource, Is.Null);

        Assert.That(detail.NoteKey, Is.EqualTo(BackendTexts.EntryLanguageNote));
        Assert.That(detail.Note, Is.Null);
        Assert.That(detail.NoteSource, Is.Null);

        Assert.That(detail.ContextKey, Is.EqualTo(BackendTexts.EntryLanguageContext));
        Assert.That(detail.Context, Is.Null);
        Assert.That(detail.ContextSource, Is.Null);

        Assert.That(detail.MetadataKey, Is.EqualTo(BackendTexts.EntryLanguageMetadata));
        Assert.That(detail.Metadata, Is.Null);
    }

    [Test]
    public void ShouldMapReferencesWithLinks()
    {
        var result = mapper.Map<WatchlistExportEntry>(entry);

        Assert.That(result.LanguageItems.Count, Is.EqualTo(3));
        Assert.That(result.LanguageItems[0].IsoCode, Is.EqualTo("current"));
        Assert.That(result.LanguageItems[0].Details.Count, Is.EqualTo(1));

        var detail = result.LanguageItems[0].Details[0];

        Assert.That(detail.ReferencesKey, Is.EqualTo(BackendTexts.EntryLanguageReference));
        Assert.That(detail.References, Is.EqualTo("<a href=\"https://localhost/test\">Referenced Entry 3 Language 1</a>; GEN: <a href=\"https://localhost/test\">Referenced Entry 2 Language 1</a>"));
    }

    private static DomainModels.Entry GenerateReferencedEntry(int entryId)
    {
        return new DomainModels.Entry
        {
            Id = entryId,
            Translations = new List<DomainModels.EntryLanguage>
            {
                GenerateReferencedEntryLanguage(entryId, 1),
                GenerateReferencedEntryLanguage(entryId, 2),
                GenerateReferencedEntryLanguage(entryId, 3),
                GenerateReferencedEntryLanguage(entryId, 4),
            }
        };
    }

    private static DomainModels.EntryLanguage GenerateReferencedEntryLanguage(int entryId, int languageId)
    {
        return new()
        {
            EntryId = entryId,
            LanguageId = languageId,
            Language = new()
            {
                Id = languageId,
                SortIndex = languageId,
                IsoCode = "current",
            },
            Sequence = 1,
            Name = $"Referenced Entry {entryId} Language {languageId}"
        };
    }
}
