using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Tests;

public static class DatabaseHelper
{
    public const string ExternalSystemId = "dummy-external-system-id";

    public static DataContext PrepareContext(string memoryContext)
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(memoryContext);

        var context = new DataContext(builder.Options);
        context.Database.EnsureCreated();

        ResetContext(context);
        return context;
    }

    private static void ResetContext(DataContext context)
    {
        context.Collections.AddRange(GetCollections());
        context.ConceptSystems.AddRange(GetConceptSystems());
        context.ConceptSystemPositions.AddRange(GetConceptSystemPositions());
        context.LenochClassifications.AddRange(GetLenochClassifications());
        context.Offices.AddRange(GetOffices());
        context.Languages.AddRange(GetLanguages());
        context.Entries.AddRange(GetEntries());
        context.Classifications.AddRange(GetClassifications());
        context.UserAccounts.AddRange(GetUserAccounts());
        context.Pictures.AddRange(GetPictures());
        context.Glossaries.AddRange(GetGlossaries());
        context.EntryReferences.AddRange(GetEntryReferences());
        context.EntryExternalLinks.AddRange(GetEntryExternalLinks());
        context.Subjects.AddRange(GetSubjects());
        context.AdminInfos.AddRange(GetAdminInfos());
        context.LenochClassificationSubjectDomainMappings.AddRange(GetLenochClassificationSubjectDomainMappings());
        context.Statistics.AddRange(GetStatistics());
        context.EntryTemplates.AddRange(GetEntryTemplates());
        context.SourceTemplates.AddRange(GetSourceTemplates());

        context.SaveChanges();

        AddEntryRelations(context);
        AddUserAccountRelations(context);

        context.SaveChanges();
    }

    private static IEnumerable<EntryTemplate> GetEntryTemplates()
    {
        return new List<EntryTemplate>
        {
            new()
            {
                Id = 1,
                Name = "TemplateZ",
                UserAccountId = 1,
                Template = "{\"Identification\": 123,\"Office\": { \"Name\": \"OfficeNameTest\" },\"Domains\": [ { \"Name\": \"DomainNameTest\" } ],\"Comment\": \"CommentTest\"}",
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch"
            },
            new()
            {
                Id = 2,
                Name = "TemplateA",
                UserAccountId = 1,
                Template = "{\"Identification\": 123,\"Office\": { \"Name\": \"OfficeNameTest\" },\"Domains\": [ { \"Name\": \"DomainNameTest\" } ],\"Comment\": \"CommentTest\"}",
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch"
            },
            new()
            {
                Id = 3,
                Name = "TemplateA",
                UserAccountId = 2,
                Template = "{\"Identification\": 123,\"Office\": { \"Name\": \"OfficeNameTest\" },\"Domains\": [ { \"Name\": \"DomainNameTest\" } ],\"Comment\": \"CommentTest\"}",
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test3@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test3@test.ch"
            }
        };
    }

    private static IEnumerable<SourceTemplate> GetSourceTemplates()
    {
        return new List<SourceTemplate>
        {
            new()
            {
                Id = 1,
                TextDe = "Text De1",
                TextFr = "Text Fr1",
                TextIt = "Text It1",
                TextEn = "Text En1",
                TextRm = "Text Rm1",
                UserId = 1,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch"
            },
            new()
            {
                Id = 2,
                TextDe = "Text De2",
                TextFr = "Text Fr2",
                TextIt = "Text It2",
                TextEn = "Text En2",
                TextRm = "Text Rm2",
                UserId = 1,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch"
            },
            new()
            {
                Id = 3,
                TextDe = "Text De2",
                TextFr = "Text Fr2",
                TextIt = "Text It2",
                TextEn = "Text En2",
                TextRm = "Text Rm2",
                UserId = 2,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch"
            }
        };
    }

    private static IEnumerable<Subject> GetSubjects()
    {
        return new List<Subject>
        {
            new() {
                Id = 1,
                Translations = new List<SubjectTranslation>
                {
                    new() { Id = 1, LanguageId = 1, SubjectId = 1, Name = "SUBJECT 1" },
                    new() { Id = 2, LanguageId = 2, SubjectId = 1, Name = "SUBJECT 1 ALT" },
                },
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch",
                Domains = new List<Domain>
                {
                    new()
                    {
                        Id = 1,
                        SubjectId = 1,
                        Created = DateTime.UtcNow.AddDays(-10),
                        CreatedBy = "test@test.ch",
                        Modified = DateTime.UtcNow.AddDays(-5),
                        ModifiedBy = "test@test.ch",
                        Translations = new List<DomainTranslation>
                        {
                            new() { Id = 1, LanguageId = 1, DomainId = 1, Name = "DOMAIN 1" },
                            new() { Id = 2, LanguageId = 2, DomainId = 1, Name = "DOMAIN 1 ALT" },
                        }
                    },
                    new()
                    {
                        Id = 2,
                        SubjectId = 1,
                        Created = DateTime.UtcNow.AddDays(-10),
                        CreatedBy = "test@test.ch",
                        Modified = DateTime.UtcNow.AddDays(-5),
                        ModifiedBy = "test@test.ch",
                        Translations = new List<DomainTranslation>
                        {
                            new() { Id = 3, LanguageId = 1, DomainId = 2, Name = "DOMAIN 2" },
                            new() { Id = 4, LanguageId = 2, DomainId = 2, Name = "DOMAIN 2 ALT" },
                        }
                    },
                }
            },
            new() {
                Id = 2,
                Translations = new List<SubjectTranslation>
                {
                    new() { Id = 3, LanguageId = 1, SubjectId = 2, Name = "SUBJECT 2" },
                    new() { Id = 4, LanguageId = 2, SubjectId = 2, Name = "SUBJECT 2 ALT" },
                },
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch",
                Domains = new List<Domain>
                {
                    new()
                    {
                        Id = 3,
                        SubjectId = 2,
                        Created = DateTime.UtcNow.AddDays(-10),
                        CreatedBy = "test@test.ch",
                        Modified = DateTime.UtcNow.AddDays(-5),
                        ModifiedBy = "test@test.ch",
                        Translations = new List<DomainTranslation>
                        {
                            new() { Id = 5, LanguageId = 1, DomainId = 3, Name = "DOMAIN 3" },
                            new() { Id = 6, LanguageId = 2, DomainId = 3, Name = "DOMAIN 3 ALT" },
                        }
                    },
                    new()
                    {
                        Id = 4,
                        SubjectId = 2,
                        Created = DateTime.UtcNow.AddDays(-10),
                        CreatedBy = "test@test.ch",
                        Modified = DateTime.UtcNow.AddDays(-5),
                        ModifiedBy = "test@test.ch",
                        Translations = new List<DomainTranslation>
                        {
                            new() { Id = 7, LanguageId = 1, DomainId = 4, Name = "DOMAIN 4" },
                            new() { Id = 8, LanguageId = 2, DomainId = 4, Name = "DOMAIN 5 ALT" },
                        }
                    },
                }
            },
            new() {
                Id = 3,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch",
                Translations = new List<SubjectTranslation>
                {
                    new() { Id = 5, LanguageId = 1, SubjectId = 3, Name = "SUBJECT 3" },
                    new() { Id = 6, LanguageId = 2, SubjectId = 3, Name = "SUBJECT 3 ALT" },
                }
            },
        };
    }

    private static IEnumerable<AdminInfo> GetAdminInfos()
    {
        return new List<AdminInfo>
        {
            new() { Id = 111, Code = "Code111", Description = "Description 1", Created = DateTime.UtcNow.AddDays(-10), CreatedBy = "test@test.ch", Modified = DateTime.UtcNow.AddDays(-5), ModifiedBy = "pp" },
            new() { Id = 222, Code = "Code222", Description = "Description 2", Created = DateTime.UtcNow.AddDays(-10), CreatedBy = "test@test.ch", Modified = DateTime.UtcNow.AddDays(-5), ModifiedBy = "pp" },
            new() { Id = 333, Code = "Code333", Description = "Description 3", Created = DateTime.UtcNow.AddDays(-10), CreatedBy = "test@test.ch", Modified = DateTime.UtcNow.AddDays(-5), ModifiedBy = "pp" },
            new() { Id = 444, Code = "Code444", Description = "Description 4", Created = DateTime.UtcNow.AddDays(-10), CreatedBy = "test@test.ch", Modified = DateTime.UtcNow.AddDays(-5), ModifiedBy = "pp" },
        };
    }

    private static IEnumerable<LenochClassificationSubjectDomainMapping> GetLenochClassificationSubjectDomainMappings()
    {
        return new List<LenochClassificationSubjectDomainMapping>
        {
            new() { Id = 1, LenochClassificationId = 1, SubjectId = 1, DomainId = 1 },
            new() { Id = 2, LenochClassificationId = 1, SubjectId = 1, DomainId = 2 },
            new() { Id = 3, LenochClassificationId = 1, SubjectId = 3 },
            new() { Id = 4, LenochClassificationId = 2, SubjectId = 1, DomainId = 2 },
            new() { Id = 5, LenochClassificationId = 2, SubjectId = 2, DomainId = 4 },
            new() { Id = 6, LenochClassificationId = 3, SubjectId = 2, DomainId = 3 },
        };
    }

    private static IEnumerable<Picture> GetPictures()
    {
        return new List<Picture>
        {
            new()
            {
                EntryId = 1,
                ContentType = "image/png",
                FileExtension = "png",
                EntityTag = "GutenTag",
                File = new PictureFile
                {
                    EntryId = 1, Value = Encoding.ASCII.GetBytes("TEST IMAGE")
                },
                Translations = new List<PictureTranslation>
                {
                    new()
                    {
                        Id = 1,
                        Description = "Description Deutsch",
                        Source = "Source Deutsch",
                        LanguageId = 1,
                        EntryId = 1
                    },
                    new()
                    {
                        Id = 2,
                        Description = "Description Französisch",
                        Source = "Source Französisch",
                        LanguageId = 2,
                        EntryId = 1
                    },
                }
            },
            new()
            {
                EntryId = 2,
                ContentType = "image/jpeg",
                FileExtension = "jpg",
                EntityTag = "GutenTag",
                File = new PictureFile
                {
                    EntryId = 2, Value = Encoding.ASCII.GetBytes("TEST IMAGE 2")
                },
                Translations = new List<PictureTranslation>
                {
                    new()
                    {
                        Id = 3,
                        Description = "Description Deutsch 2",
                        Source = "Source Deutsch 2",
                        LanguageId = 1,
                        EntryId = 2
                    },
                    new()
                    {
                        Id = 4,
                        Description = "Description Französisch 2",
                        Source = "Source Französisch 2",
                        LanguageId = 2,
                        EntryId = 2
                    },
                }
            }
        };
    }

    private static IList<Language> GetLanguages()
    {
        return new List<Language>
        {
            new()
            {
                Id = 1,
                IsUiLanguage = true,
                IsSystemLanguage = true,
                IsoCode = "DE",
                SortIndex = 1,
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<LanguageTranslation>
                {
                    new()
                    {
                        Id = 1, Name = "Deutsch", LanguageId = 1, IsoCode = "DE"
                    },
                    new()
                    {
                        Id = 2, Name = "Allemand", LanguageId = 1, IsoCode = "FR"
                    },
                    new()
                    {
                        Id = 3, Name = "Tedesco", LanguageId = 1, IsoCode = "IT"
                    },
                }
            },
            new()
            {
                Id = 2,
                IsUiLanguage = true,
                IsSystemLanguage = true,
                IsoCode = "FR",
                SortIndex = 2,
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<LanguageTranslation>
                {
                    new()
                    {
                        Id = 4, Name = "Französisch", LanguageId = 2, IsoCode = "DE"
                    },
                    new()
                    {
                        Id = 5, Name = "Français", LanguageId = 2, IsoCode = "FR"
                    },
                    new()
                    {
                        Id = 6, Name = "Francese", LanguageId = 2, IsoCode = "IT"
                    },

                }
            },
            new()
            {
                Id = 3,
                IsUiLanguage = false,
                IsSystemLanguage = true,
                IsoCode = "IT",
                SortIndex = 6,
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<LanguageTranslation>
                {
                    new()
                    {
                        Id = 7, Name = "Italienisch", LanguageId = 3, IsoCode = "DE"
                    },
                    new()
                    {
                        Id = 8, Name = "Italien", LanguageId = 3, IsoCode = "FR"
                    },
                    new()
                    {
                        Id = 9, Name = "Italiano", LanguageId = 3, IsoCode = "IT"
                    },
                }
            },
        };
    }

    private static IList<Office> GetOffices()
    {
        return new List<Office>()
        {
            new()
            {
                Id = 1,
                Code = "ACH",
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<OfficeTranslation>
                {
                    new()
                    {
                        Id = 1, LanguageId = 1, OfficeId = 1, Name = "Schweizerische Bundesverwaltung"
                    },
                    new()
                    {
                        Id = 2, LanguageId = 2, OfficeId = 1, Name = "Administration fédérale suisse"
                    },
                    new()
                    {
                        Id = 3, LanguageId = 3, OfficeId = 1, Name = "Amministrazione federale svizzera"
                    }
                }
            },
            new()
            {
                Id = 2,
                Code = "ALX",
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<OfficeTranslation>
                {
                    new()
                    {
                        Id = 4, LanguageId = 1, OfficeId = 2, Name = "Übersetzerinnen und Übersetzer der Bundesverwaltung"
                    },
                    new()
                    {
                        Id = 5, LanguageId = 2, OfficeId = 2, Name = "Traducteurs et traductrices de l'administration fédérale"
                    },
                    new()
                    {
                        Id = 6, LanguageId = 3, OfficeId = 2, Name = "Traduttrici e traduttori dell'Amministrazione federale"
                    }
                }
            },
        };
    }

    private static IList<Entry> GetEntries()
    {
        return new List<Entry>
        {
            new()
            {
                Id = 1,
                OfficeId = 1,
                CollectionId = 1,
                Reliability = Reliability.Approved,
                Comment = "Comment1",
                AdminInfos = new List<AdminInfo>
                {
                    new()
                    {
                        Id = 2,
                        Code = "Trüffel",
                        Description = "Description 1",
                        CreatedBy = "pp",
                        ModifiedBy = "pp"
                    },
                    new()
                    {
                        Id = 3,
                        Code = "Schwammerl",
                        Description = "Description 2",
                        CreatedBy = "pp",
                        ModifiedBy = "pp"
                    },
                },
                Status = Status.Validated,
                Identification = 100,
                ConceptSystemPositionId = 1,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch",
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        Id = 1,
                        EntryId = 1,
                        LanguageId = 1,
                        Sequence = 1,
                        Abbreviation = "kg",
                        Definition = "Masseinheit, die gleich der Masse des Internationalen Kilogrammprototyps ist.",
                        Name = "Kilogramm ",
                        Phraseology = null,
                        Terminus = "Einheit der Masse"
                    },
                    new()
                    {
                        Id = 2,
                        EntryId = 1,
                        LanguageId = 2,
                        Sequence = 1,
                        Abbreviation = "kg",
                        Definition = "Unité de mesure égale à la masse du prototype international du kilogramme. ",
                        Name = "kilogramme ",
                        Phraseology = null,
                        Terminus = "unité de masse"
                    },
                    new()
                    {
                        Id = 3,
                        EntryId = 1,
                        LanguageId = 3,
                        Sequence = 1,
                        Abbreviation = "kg",
                        Definition = "Unità di misura pari alla massa del prototipo internazionale del chilogrammo.",
                        Name = "chilogrammo ",
                        Phraseology = null,
                        Terminus = "unità di massa"
                    },
                }
            },
            new()
            {
                Id = 2,
                OfficeId = 1,
                CollectionId = 2,
                Reliability = Reliability.Translated,
                Comment = "Comment2",
                AdminInfos = new List<AdminInfo>
                {
                    new()
                    {
                        Id = 4,
                        Code = "Steinpilz",
                        Description = "Description 3",
                        CreatedBy = "pp",
                        ModifiedBy = "pp"
                    },
                    new()
                    {
                        Id = 5,
                        Code = "Champignon",
                        Description = "Description 4",
                        CreatedBy = "pp",
                        ModifiedBy = "pp"
                    },
                },

                Status = Status.InProgress,
                Identification = null,
                ConceptSystemPositionId = 1,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test@test.ch",
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        Id = 4,
                        EntryId = 2,
                        LanguageId = 1,
                        Sequence = 1,
                        Abbreviation = "cd",
                        Definition = "Lichtstärke einer Strahlungsquelle, welche monochromatische Strahlung der Frequenz 540 · 10 12 Hertz in eine bestimmte Richtung aussendet, in der die Strahlstärke l/683 Watt pro Steradiant beträgt.",
                        Name = "Candela ",
                        Phraseology = null,
                        Terminus = "Einheit der Lichtstärke",
                        IsObsolete = true,
                        IsRare = true,
                        ShouldAvoid = true
                    },
                    new()
                    {
                        Id = 5,
                        EntryId = 2,
                        LanguageId = 2,
                        Sequence = 1,
                        Abbreviation = "cd",
                        Definition = "Intensité lumineuse, dans une direction donnée, d'une source qui émet un rayonnement monochromatique de fréquence 540 · 10 12 hertz et dont l'intensité énergétique dans cette direction est 1/683 watt par stéradian.",
                        Name = "candela ",
                        Phraseology = null,
                        Terminus = "unité de l'intensité lumineuse"
                    },
                    new()
                    {
                        Id = 6,
                        EntryId = 2,
                        LanguageId = 3,
                        Sequence = 1,
                        Abbreviation = "cd",
                        Definition = "Irraggiamento monocromatico di frequenza 540 · 10 12 hertz la cui intensità energetica in tale direzione è 1/683 watt per steradiante.",
                        Name = "candela ",
                        Phraseology = null,
                        Terminus = "unità dell'intensità luminosa"
                    },
                }
            },
            new()
            {
                Id = 3,
                OfficeId = 2,
                CollectionId = 1,
                Reliability = Reliability.FirstDraft,
                Comment = "Comment3",
                Status = Status.InProgress,
                Identification = 200,
                ConceptSystemPositionId = 2,
                Created = DateTime.UtcNow.AddDays(-20),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-10),
                ModifiedBy = "test@test.ch",
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        Id = 7,
                        EntryId = 3,
                        LanguageId = 1,
                        Sequence = 1,
                        Abbreviation = null,
                        Definition = "Winkel zwischen Ziellinie und dem Mündungshorizont.",
                        Name = null,
                        Phraseology = "Im Lagewinkel kann eine Abweichung, die durch einen extremen Stellungsunterschied FR/Lwfw bedingt ist, toleriert werden [...]",
                        Terminus = "Lagewinkel"
                    },
                    new()
                    {
                        Id = 8,
                        EntryId = 3,
                        LanguageId = 2,
                        Sequence = 1,
                        Abbreviation = null,
                        Definition = null,
                        Name = null,
                        Phraseology = "En site, on peut tolérer une variation qui est provoquée par une différence de position lance eg/RP extrême [...].",
                        Terminus = "site (1)"
                    },
                    new()
                    {
                        Id = 9,
                        EntryId = 3,
                        LanguageId = 2,
                        Sequence = 2,
                        Abbreviation = null,
                        Definition = null,
                        Name = null,
                        Phraseology = null,
                        Terminus = "angle de site (2)"
                    },
                    new()
                    {
                        Id = 10,
                        EntryId = 3,
                        LanguageId = 3,
                        Sequence = 1,
                        Abbreviation = null,
                        Definition = "Angolo formato dalla linea di sito con l'orizzonte di bocca (a). Angolo formato dalla linea di sito con l'orizzonte del pezzo (b).",
                        Name = null,
                        Phraseology = null,
                        Terminus = "angolo di sito"
                    },
                }
            },
            new()
            {
                Id = 4,
                OfficeId = 2,
                CollectionId = 3,
                Reliability = Reliability.Verified,
                Comment = "Comment4",
                Status = Status.Validated,
                Identification = 300,
                Created = DateTime.UtcNow.AddDays(-20),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-10),
                ModifiedBy = "test@test.ch",
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        Id = 11,
                        EntryId = 4,
                        LanguageId = 1,
                        Sequence = 1,
                        Abbreviation = "Auftr",
                        Definition = "Verbindlich zu erfüllende Aufgabe (a). Befehl, der ein in einer bestimmten Zeit und/oder in einem bestimmten Raum zu erreichendes Ziel und gegebenenfalls die von der Führung damit verfolgte Absicht bezeichnet (b).",
                        Name = null,
                        Phraseology = "Im Auftrag wird die zu erfüllende Aufgabe verbindlich vorgeschrieben. Er ist die Grundlage für das Handeln des Führers während der ganzen Dauer einer Aktion.",
                        Terminus = "Auftrag"
                    },
                    new()
                    {
                        Id = 12,
                        EntryId = 4,
                        LanguageId = 2,
                        Sequence = 1,
                        Abbreviation = "miss",
                        Definition = null,
                        Name = null,
                        Phraseology = "La mission définit la tâche qui doit être accomplie",
                        Terminus = "ordre"
                    },
                }
            },
            new()
            {
                Id = 5,
                OfficeId = 2,
                CollectionId = 1,
                Reliability = Reliability.FirstDraft,
                Comment = "Comment3",
                AdminInfos = new List<AdminInfo>
                {
                    new()
                    {
                        Id = 6,
                        Code = "Fusspilz",
                        Description = "Description 5",
                        CreatedBy = "pp",
                        ModifiedBy = "pp"
                    },
                },
                Status = Status.InProgress,
                Identification = 200,
                Created = DateTime.UtcNow.AddDays(-20),
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow.AddDays(-10),
                ModifiedBy = "test@test.ch",
                Translations = new List<EntryLanguage>
                {
                    new()
                    {
                        Id = 13,
                        EntryId = 3,
                        LanguageId = 1,
                        Sequence = 1,
                        Abbreviation = null,
                        Definition = "Winkel zwischen Ziellinie und dem Mündungshorizont.",
                        Name = null,
                        Phraseology = "Im Lagewinkel kann eine Abweichung, die durch einen extremen Stellungsunterschied FR/Lwfw bedingt ist, toleriert werden [...]",
                        Terminus = "Lagewinkel"
                    },
                    new()
                    {
                        Id = 14,
                        EntryId = 3,
                        LanguageId = 2,
                        Sequence = 1,
                        Abbreviation = null,
                        Definition = null,
                        Name = null,
                        Phraseology = "En site, on peut tolérer une variation qui est provoquée par une différence de position lance eg/RP extrême [...].",
                        Terminus = "site (1)"
                    },
                    new()
                    {
                        Id = 15,
                        EntryId = 3,
                        LanguageId = 2,
                        Sequence = 2,
                        Abbreviation = null,
                        Definition = null,
                        Name = null,
                        Phraseology = null,
                        Terminus = "will be deleted in unit test"
                    },
                    new()
                    {
                        Id = 16,
                        EntryId = 3,
                        LanguageId = 3,
                        Sequence = 1,
                        Abbreviation = null,
                        Definition = "Angolo formato dalla linea di sito con l'orizzonte di bocca (a). Angolo formato dalla linea di sito con l'orizzonte del pezzo (b).",
                        Name = null,
                        Phraseology = null,
                        Terminus = "angolo di sito"
                    }
                },
                ExternalSystemId = ExternalSystemId
            }
        };
    }

    private static IList<LenochClassification> GetLenochClassifications()
    {
        return new List<LenochClassification>
        {
            new()
            {
                Id = 1,
                Code = "AD",
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<LenochClassificationTranslation>
                {
                    new()
                    {
                        Id = 1, LanguageId = 1, LenochClassificationId = 1, Name = "ÖFFENTLICHE UND PRIVATE VERWALTUNG"
                    },
                    new()
                    {
                        Id = 2, LanguageId = 2, LenochClassificationId = 1, Name = "ADMINISTRATION PUBLIQUE ET PRIVÉE"
                    },
                    new()
                    {
                        Id = 3, LanguageId = 3, LenochClassificationId = 1, Name = "AMMINISTRAZIONE PUBBLICA E PRIVATA"
                    }
                }
            },
            new()
            {
                Id = 2,
                Code = "ORA",
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<LenochClassificationTranslation>
                {
                    new()
                    {
                        Id = 4, LanguageId = 1, LenochClassificationId = 2, Name = "IBRD, INTERNATIONALE BANK FÜR WIEDERAUFBAU/ENTWICKLUNG (WELTBANK) (auch: ECA/FI3/FIE/JU3)"
                    },
                    new()
                    {
                        Id = 5, LanguageId = 2, LenochClassificationId = 2, Name = "BIRD, BANQUE INTERNATIONALE POUR LA RECONSTRUCTION/DÉVELOPPEMENT (voir également ECA/PO8)"
                    },
                    new()
                    {
                        Id = 6, LanguageId = 3, LenochClassificationId = 2, Name = "BIRS BANCA INTERNAZIONALE DI RICOSTRUZIONE E SVILUPPO"
                    }
                }
            },
            new()
            {
                Id = 3,
                Code = "ORB",
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<LenochClassificationTranslation>
                {
                    new()
                    {
                        Id = 7, LanguageId = 1, LenochClassificationId = 3, Name = "IAEA, INTERNATIONALE ATOMENERGIEAGENTUR (auch: AT3/AT8/DEK/JU3/JU6/PO8)"
                    },
                    new()
                    {
                        Id = 8, LanguageId = 2, LenochClassificationId = 3, Name = "AIEA, AGENCE INTERNATIONALE DE L'ÉNERGIE ATOMIQUE (voir également AT3/AT8/DEK/JU3/JU6/PO8)"
                    },
                    new()
                    {
                        Id = 9, LanguageId = 3, LenochClassificationId = 3, Name = "AIEA AGENZIA INTERNAZIONALE PER L'ENERGIA ATOMICA"
                    }
                }
            },
        };
    }

    private static IList<Collection> GetCollections()
    {
        return new List<Collection>
        {
            new()
            {
                Id = 1,
                Code = "ABC11",
                Active = true,
                ActiveApi = true,
                ExportLindas = true,
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<CollectionTranslation>
                {
                    new()
                    {
                        Id = 1, LanguageId = 1, CollectionId = 1, Name = "ABC-Terminologie des VBS"
                    },
                    new()
                    {
                        Id = 2, LanguageId = 2, CollectionId = 1, Name = "Terminologie NBC du DDPS"
                    },
                    new()
                    {
                        Id = 3, LanguageId = 3, CollectionId = 1, Name = "Terminologia NBC del DDPS"
                    }
                }
            },
            new()
            {
                Id = 2,
                Code = "ABC11",
                Active = false,
                ActiveApi = false,
                ExportLindas = true,
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<CollectionTranslation>
                {
                    new()
                    {
                        Id = 4, LanguageId = 1, CollectionId = 2, Name = "Übersetzungsdatei der schweizerischen Bundesverwaltung (1991-2004, frühere Datenbank ALEXIS)"
                    },
                    new()
                    {
                        Id = 5, LanguageId = 2, CollectionId = 2, Name = "Fichier collectif des traducteurs de l'administration fédérale suisse (1991 - 2004, ALEXIS)"
                    },
                    new()
                    {
                        Id = 6, LanguageId = 3, CollectionId = 2, Name = "File di traduzione dell'Amministrazione federale (1991-2004, vecchia banca dati ALEXIS)"
                    }
                }
            },
            new()
            {
                Id = 3,
                Code = "ABC11",
                Active = false,
                ActiveApi = false,
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<CollectionTranslation>
                {
                    new()
                    {
                        Id = 7, LanguageId = 1, CollectionId = 3, Name = "Test Sammlung"
                    },
                    new()
                    {
                        Id = 8, LanguageId = 2, CollectionId = 3, Name = "Test collection"
                    },
                    new()
                    {
                        Id = 9, LanguageId = 3, CollectionId = 3, Name = "Test collectione"
                    }
                }
            },
        };
    }

    private static IList<ConceptSystem> GetConceptSystems()
    {
        return new List<ConceptSystem>
        {
            new()
            {
                Id = 1,
                Title = "Test 1 Title",
                CollectionId = 1,
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test1@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test2@test.ch"
            },
            new()
            {
                Id = 2,
                Title = "Test 2 Title",
                CollectionId = 2,
                Created = DateTime.UtcNow.AddDays(-11),
                CreatedBy = "test3@test.ch",
                Modified = DateTime.UtcNow.AddDays(-6),
                ModifiedBy = "test4@test.ch"
            },
            new()
            {
                Id = 3,
                Title = "Test 3 Title",
                CollectionId = 3,
                Created = DateTime.UtcNow.AddDays(-12),
                CreatedBy = "test5@test.ch",
                Modified = DateTime.UtcNow.AddDays(-7),
                ModifiedBy = "test6@test.ch"
            }
        };
    }

    private static IList<ConceptSystemPosition> GetConceptSystemPositions()
    {
        return new List<ConceptSystemPosition>
        {
            new()
            {
                Id = 1,
                SortIndex = 1,
                Value= "Value 1: Tram",
                ConceptSystemId = 1
            },
            new()
            {
                Id = 2,
                SortIndex = 0,
                Value= "Value 1.1: Bam",
                ConceptSystemId = 1
            },
            new()
            {
                Id = 3,
                Value= "Value 2: Lam",
                ConceptSystemId = 2
            },
            new()
            {
                Id = 4,
                Value= "Value 2.1: Jam",
                ConceptSystemId = 2
            },
            new()
            {
                Id = 5,
                Value= "Value 2.2: Nam",
                ConceptSystemId = 2
            },
            new()
            {
                Id = 6,
                Value= "Value 2.2.2: Plam",
                ConceptSystemId = 2
            }
        };
    }

    private static IList<UserAccount> GetUserAccounts()
    {
        return new List<UserAccount>
        {
            new()
            {
                Id = 1,
                FirstName = "Anna",
                LastName = "Genie",
                Email = "anna.genie@test.it",
                Role = "Admin",
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test1@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test2@test.ch",
            },
            new()
            {
                Id = 2,
                FirstName = "Franz",
                LastName = "Tester",
                Email = "franz.tester@test.ch",
                Role = "SuperUser",
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test1@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test2@test.ch",
            },
            new()
            {
                Id = 3,
                FirstName = "Franziska",
                LastName = "Testerin",
                Email = "franziska.testerin@test.ch",
                Role = "Writer",
                Created = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "test1@test.ch",
                Modified = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "test2@test.ch",
            },
        };
    }

    private static IList<Classification> GetClassifications()
    {
        return new List<Classification>
        {
            new()
            {
                Id = 100,
                Code = "ABC",
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<ClassificationTranslation>
                {
                    new()
                    {
                        Id = 55, Name = "BCD", LanguageId = 1, ClassificationId = 100
                    },
                    new()
                    {
                        Id = 56, Name = "EFG", LanguageId = 2, ClassificationId = 100
                    },
                    new()
                    {
                        Id = 57, Name = "HIJ", LanguageId = 3, ClassificationId = 100
                    },
                },
                Collections = new List<Collection>
                {
                    new()
                    {
                        Id = 33,
                        Code = "ABC12",
                        CreatedBy = "test@test.ch",
                        ModifiedBy = "test@test.ch",
                        Translations = new List<CollectionTranslation>
                        {
                            new()
                            {
                                Id = 44, LanguageId = 1, CollectionId = 33, Name = "ABC-Terminologie des VBS"
                            },
                            new()
                            {
                                Id = 45, LanguageId = 2, CollectionId = 33, Name = "Terminologie NBC du DDPS"
                            },
                            new()
                            {
                                Id = 46, LanguageId = 3, CollectionId = 33, Name = "Terminologia NBC del DDPS"
                            }
                        }
                    },
                    new()
                    {
                        Id = 34,
                        Code = "ABC13",
                        CreatedBy = "test@test.ch",
                        ModifiedBy = "test@test.ch",
                        Translations = new List<CollectionTranslation>
                        {
                            new()
                            {
                                Id = 47, LanguageId = 1, CollectionId = 34, Name = "Übersetzungsdatei der schweizerischen Bundesverwaltung (1991-2004, frühere Datenbank ALEXIS)"
                            },
                            new()
                            {
                                Id = 48, LanguageId = 2, CollectionId = 34, Name = "Fichier collectif des traducteurs de l'administration fédérale suisse (1991 - 2004, ALEXIS)"
                            },
                            new()
                            {
                                Id = 49, LanguageId = 3, CollectionId = 34, Name = "File di traduzione dell'Amministrazione federale (1991-2004, vecchia banca dati ALEXIS)"
                            }
                        }
                    },
                }
            },
            new()
            {
                Id = 101,
                Code = "KLM",
                                    CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<ClassificationTranslation>
                {
                    new()
                    {
                        Id = 58, Name = "NOP", LanguageId = 1, ClassificationId = 101
                    },
                    new()
                    {
                        Id = 59, Name = "QRS", LanguageId = 2, ClassificationId = 101
                    },
                    new()
                    {
                        Id = 60, Name = "TUV", LanguageId = 3, ClassificationId = 101
                    },
                },
                Collections = new List<Collection>
                {
                    new()
                    {
                        Id = 35,
                        Code = "ABC14",
                        Active = false,
                        CreatedBy = "test@test.ch",
                        ModifiedBy = "test@test.ch",
                        Translations = new List<CollectionTranslation>
                        {
                            new()
                            {
                                Id = 50, LanguageId = 1, CollectionId = 35, Name = "ABC-Terminologie des VBS"
                            },
                            new()
                            {
                                Id = 51, LanguageId = 2, CollectionId = 35, Name = "Terminologie NBC du DDPS"
                            },
                            new()
                            {
                                Id = 52, LanguageId = 3, CollectionId = 35, Name = "Terminologia NBC del DDPS"
                            }
                        }
                    },
                    new()
                    {
                        Id = 36,
                        Code = "ABC15",
                        Active = true,
                        CreatedBy = "test@test.ch",
                        ModifiedBy = "test@test.ch",
                        Translations = new List<CollectionTranslation>
                        {
                            new()
                            {
                                Id = 53, LanguageId = 1, CollectionId = 36, Name = "Übersetzungsdatei der schweizerischen Bundesverwaltung (1991-2004, frühere Datenbank ALEXIS)"
                            },
                            new()
                            {
                                Id = 54, LanguageId = 2, CollectionId = 36, Name = "Fichier collectif des traducteurs de l'administration fédérale suisse (1991 - 2004, ALEXIS)"
                            },
                            new()
                            {
                                Id = 55, LanguageId = 3, CollectionId = 36, Name = "File di traduzione dell'Amministrazione federale (1991-2004, vecchia banca dati ALEXIS)"
                            }
                        }
                    },
                }
            }
        };
    }

    private static IList<Glossary> GetGlossaries()
    {
        return new List<Glossary>
        {
            new()
            {
                Id = 1,
                Code = "Glo1",
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<GlossaryTranslation>
                {
                    new()
                    {
                        Id = 1, LanguageId = 1, GlossaryId = 1, Name = "Glossary 1"
                    },
                    new()
                    {
                        Id = 2, LanguageId = 2, GlossaryId = 1, Name = "Glossaire 1"
                    },
                    new()
                    {
                        Id = 3, LanguageId = 3, GlossaryId = 1, Name = "Glossario 1"
                    }
                }
            },
            new()
            {
                Id = 2,
                Code = "Glo2",
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<GlossaryTranslation>
                {
                    new()
                    {
                        Id = 4, LanguageId = 1, GlossaryId = 2, Name = "Glossary 2"
                    },
                    new()
                    {
                        Id = 5, LanguageId = 2, GlossaryId = 2, Name = "Glossaire 2"
                    },
                    new()
                    {
                        Id = 6, LanguageId = 3, GlossaryId = 2, Name = "Glossario 2"
                    }
                }
            },
            new()
            {
                Id = 3,
                Code = "Glo3",
                CreatedBy = "test@test.ch",
                ModifiedBy = "test@test.ch",
                Translations = new List<GlossaryTranslation>
                {
                    new()
                    {
                        Id = 7, LanguageId = 1, GlossaryId = 3, Name = "Glossary 3"
                    },
                    new()
                    {
                        Id = 8, LanguageId = 2, GlossaryId = 3, Name = "Glossaire 3"
                    },
                    new()
                    {
                        Id = 9, LanguageId = 3, GlossaryId = 3, Name = "Glossario 3"
                    }
                }
            },
        };
    }

    private static IEnumerable<EntryReference> GetEntryReferences()
    {
        return new List<EntryReference>
        {
            new()
            {
                Id = 1,
                EntryId = 1,
                ReferencedEntryId = 2,
                Type = ReferenceType.Antonym
            },
            new()
            {
                Id = 2,
                EntryId = 1,
                ReferencedEntryId = 3,
                Type = ReferenceType.None
            },
            new()
            {
                Id = 3,
                EntryId = 1,
                ReferencedEntryId = 4,
                Type = ReferenceType.Specific
            },
            new()
            {
                Id = 4,
                EntryId = 2,
                ReferencedEntryId = 3,
                Type = ReferenceType.Antonym
            },
            new()
            {
                Id = 5,
                EntryId = 2,
                ReferencedEntryId = 4,
                Type = ReferenceType.None
            },
        };
    }

    private static IEnumerable<EntryExternalLink> GetEntryExternalLinks()
    {
        return [
            new EntryExternalLink
            {
                Url = "https://example.com",
                EntryId = 1,
                Id = 1,
                Type = ExternalLinkType.Fedlex
            }
        ];
    }

    private static IList<Statistic> GetStatistics()
    {
        return new List<Statistic>
        {
            new()
            {
                Id = 1,
                SearchText = "Rübezahl",
                SearchFields = "AB, Na, Md, PH, VE",
                Languages = "DE, FR, IT, RM, EN",
                NumberOfSearches = 71,
                DateLastSearch = DateTime.UtcNow.AddDays(-3),
                Created = DateTime.UtcNow,
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow,
                ModifiedBy = "test@test.ch",
            },
            new()
            {
                Id = 2,
                SearchText = "Gargamel",
                SearchFields = "AB, Na, PH, VE",
                Languages = "DE",
                NumberOfSearches = 3,
                DateLastSearch = DateTime.UtcNow.AddDays(-11),
                Created = DateTime.UtcNow,
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow,
                ModifiedBy = "test@test.ch",
            },
            new()
            {
                Id = 3,
                SearchText = "Rantanplan",
                SearchFields = "Na, Md, PH, VE",
                Languages = "DE, FR",
                NumberOfSearches = 6,
                DateLastSearch = DateTime.UtcNow.AddDays(-5),
                Created = DateTime.UtcNow,
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow,
                ModifiedBy = "test@test.ch",
            },
            new()
            {
                Id = 4,
                SearchText = "Döppeköche",
                SearchFields = "AB, Na, Md, PH, VE",
                Languages = "DE, FR, IT, EN",
                NumberOfSearches = 7,
                DateLastSearch = DateTime.UtcNow.AddDays(-44),
                Created = DateTime.UtcNow,
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow,
                ModifiedBy = "test@test.ch",
            },
            new()
            {
                Id = 5,
                SearchText = "Friedenspanzer",
                SearchFields = "AB, Na, Md",
                Languages = "DE, FR, IT",
                NumberOfSearches = 18,
                DateLastSearch = DateTime.UtcNow.AddDays(-2),
                Created = DateTime.UtcNow,
                CreatedBy = "test@test.ch",
                Modified = DateTime.UtcNow,
                ModifiedBy = "test@test.ch",
            },
        };
    }



    private static void AddEntryRelations(DataContext context)
    {
        var entries = context.Entries.ToList();
        var lenochClassifications = context.LenochClassifications.ToList();
        var collections = context.Collections.ToList();
        var glossaries = context.Glossaries.ToList();
        var subjects = context.Subjects.Include(s => s.Domains).OrderBy(s => s.Id).ToList();

        entries[0].LenochClassifications = new List<LenochClassification>
        {
            lenochClassifications[0], lenochClassifications[1]
        };
        entries[0].RelatedCollections = new List<Collection>
        {
            collections[1]
        };
        entries[0].Glossaries = new List<Glossary>
        {
            glossaries[0]
        };
        entries[0].Subjects = new List<Subject>
        {
            subjects[0], subjects[1]
        };
        entries[0].Domains = new List<Domain>
        {
            subjects[0].Domains.ToList()[0], subjects[0].Domains.ToList()[1],
        };

        entries[1].LenochClassifications = new List<LenochClassification>
        {
            lenochClassifications[1]
        };
        entries[1].Glossaries = new List<Glossary>
        {
            glossaries[0]
        };
        entries[1].Subjects = new List<Subject>
        {
            subjects[1]
        };
        entries[1].Domains = new List<Domain>
        {
            subjects[1].Domains.ToList()[0]
        };

        entries[2].LenochClassifications = new List<LenochClassification>
        {
            lenochClassifications[0], lenochClassifications[1], lenochClassifications[2]
        };
        entries[2].RelatedCollections = new List<Collection>
        {
            collections[0], collections[1]
        };
        entries[2].Glossaries = new List<Glossary>
        {
            glossaries[1], glossaries[2]
        };
        entries[2].Subjects = new List<Subject>
        {
            subjects[2]
        };

        entries[3].LenochClassifications = new List<LenochClassification>
        {
            lenochClassifications[1], lenochClassifications[2]
        };
        entries[3].Subjects = new List<Subject>
        {
            subjects[1]
        };
    }

    private static void AddUserAccountRelations(DataContext context)
    {
        var userAccounts = context.UserAccounts.ToList();
        var collections = context.Collections.ToList();
        var glossaries = context.Glossaries.ToList();

        userAccounts[0].AuthorizedCollections = new List<Collection>
        {
            collections[0], collections[1]
        };
        userAccounts[0].AuthorizedGlossaries = new List<Glossary>
        {
            glossaries[0], glossaries[1]
        };
    }
}
