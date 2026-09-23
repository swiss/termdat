using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.LindasExport.Api.Extensions;
using Bk.Termdat.LindasExport.Api.Interfaces;
using Bk.Termdat.LindasExport.Api.Mapping;

namespace Bk.Termdat.LindasExport.Api.Tests.Mapping;

[TestFixture]
internal class LindasMapperTests
{
    private static readonly Entry entry = new()
    {
        Id = 42,
        Modified = DateTime.Now,
        AdminInfos =
            new List<AdminInfo>
            {
                new() { Code = "AdminInfoA" }, new() { Code = "AdminInfoB" }, new() { Code = "AdminInfoC" }
            },
        Status = Status.Validated,
        Office = new Office
        {
            Code = "A",
            Translations =
                new List<OfficeTranslation>
                {
                    new() { Name = "A-Name-DE", Language = new Language { IsoCode = "DE" } },
                    new() { Name = "A-Name-FR", Language = new Language { IsoCode = "FR" } },
                    new() { Name = "A-Name-NotSupported", Language = new Language { IsoCode = "XY" } }
                }
        },
        Glossaries = new List<Glossary>
        {
            new()
            {
                Code = "B",
                Translations =
                    new List<GlossaryTranslation>
                    {
                        new() { Name = "B-Name-DE", Language = new Language { IsoCode = "DE" } },
                        new() { Name = "B-Name-FR", Language = new Language { IsoCode = "FR" } },
                        new() { Name = "B-Name-NotSupported", Language = new Language { IsoCode = "XY" } }
                    }
            },
            new()
            {
                Code = "C",
                Translations = new List<GlossaryTranslation>
                {
                    new() { Name = "C-Name-DE", Language = new Language { IsoCode = "DE" } },
                    new() { Name = "C-Name-FR", Language = new Language { IsoCode = "FR" } },
                    new() { Name = "C-Name-NotSupported", Language = new Language { IsoCode = "XY" } }
                }
            }
        },
        Reliability = Reliability.Tentative,
        Subjects = new List<Subject>
        {
            new()
            {
                Id = 42,
                Translations =
                    new List<SubjectTranslation>
                    {
                        new() { Name = "42-Subject-Name-DE", Language = new Language { IsoCode = "DE" } },
                        new() { Name = "42-Subject-Name-FR", Language = new Language { IsoCode = "FR" } },
                        new() { Name = "42-Subject-Name-NotSupported", Language = new Language { IsoCode = "XY" } }
                    }
            },
            new()
            {
                Id = 666,
                Translations =
                    new List<SubjectTranslation>
                    {
                        new() { Name = "666-Subject-Name-DE", Language = new Language { IsoCode = "DE" } },
                        new() { Name = "666-Subject-Name-FR", Language = new Language { IsoCode = "FR" } }
                    }
            }
        },
        Domains = new List<Domain>
        {
            new()
            {
                Id = 11,
                SubjectId = 42,
                Translations =
                    new List<DomainTranslation>
                    {
                        new() { Name = "11-Domain-Name-DE", Language = new Language { IsoCode = "DE" } },
                        new() { Name = "11-Domain-Name-FR", Language = new Language { IsoCode = "FR" } },
                        new() { Name = "11-Domain-Name-NotSupported", Language = new Language { IsoCode = "XY" } }
                    }
            },
            new()
            {
                Id = 22,
                SubjectId = 42,
                Translations =
                    new List<DomainTranslation>
                    {
                        new() { Name = "22-Domain-Name-DE", Language = new Language { IsoCode = "DE" } }
                    }
            },
            new()
            {
                Id = 6,
                SubjectId = 666,
                Translations =
                    new List<DomainTranslation>
                    {
                        new() { Name = "6-Domain-Name-EN", Language = new Language { IsoCode = "EN" } }
                    }
            }
        },
        References =
            new List<EntryReference>
            {
                new() { Type = ReferenceType.Antonym, ReferencedEntryId = 1337 },
                new() { Type = ReferenceType.None, ReferencedEntryId = 1338 }
            },
        Translations = new List<EntryLanguage>
        {
            new()
            {
                Sequence = 1,
                Language = new Language { IsoCode = "DE" },
                Terminus = "test_entry_term_1",
                TerminusSource = "test_entry_term_citation_1",
                Name = "test_entry_name_1",
                NameSource = "test_entry_name_citation_1",
                Abbreviation = "test_entry_abbreviation_1",
                AbbreviationSource = "test_entry_abbreviation_citation_1",
                Phraseology = "test_entry_phraseology_1",
                PhraseologySource = "test_entry_phraseology_citation_1",
                Definition = "test_entry_definition_1",
                DefinitionSource = "test_entry_definition_citation_1",
                Note = "test_entry_note_1",
                NoteSource = "test_entry_note_citation_1",
                Context = "test_entry_context_1",
                Country = "test_entry_country_1"
            },
            new()
            {
                Sequence = 2,
                Language = new Language { IsoCode = "FR" },
                Terminus = "test_entry_term_2",
                TerminusSource = "test_entry_term_citation_2",
                Name = "test_entry_name_2",
                NameSource = "test_entry_name_citation_2",
                Abbreviation = "test_entry_abbreviation_2",
                AbbreviationSource = "test_entry_abbreviation_citation_2",
                Phraseology = "test_entry_phraseology_2",
                PhraseologySource = "test_entry_phraseology_citation_2",
                Definition = "test_entry_definition_2",
                IsObsolete = true
            },
            new() { Sequence = 3, Language = new Language { IsoCode = "FR" }, Context = "should_not_be_mapped" }
        }
    };

    private static readonly Entry deletedEntry = new()
    {
        Id = 42,
        Modified = DateTime.Now,
        Deleted = true,
    };

    private readonly Collection collection = new()
    {
        Code = "test_collection_code",
        Translations = new List<CollectionTranslation>
        {
            new() { Language = new Language { IsoCode = "DE" }, Name = "test_collection_name_1" },
            new() { Language = new Language { IsoCode = "RN" }, Name = "test_collection_name_2" },
            new() { Language = new Language { IsoCode = "EN" }, Name = "test_collection_name_3" }
        },
        Entries = new List<Entry> { entry, deletedEntry }
    };

    private readonly ILindasMapper lindasMapper = new LindasMapper();

    [Test]
    public void Map_Collection_ShouldMap()
    {
        var result = lindasMapper.Map(collection);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Code, Is.EqualTo(collection.Code));

            // Collection names
            Assert.That(result.Names, Is.Not.Empty);
            var collectionNames = result.Names!.ToList();
            Assert.That(collectionNames, Has.Count.EqualTo(2));
            Assert.That(collectionNames.First().Language, Is.EqualTo(Api.Models.Language.De));
            Assert.That(collectionNames.First().Value, Is.EqualTo(collection.Translations.First().Name));
            Assert.That(collectionNames.Last().Language, Is.EqualTo(Api.Models.Language.En));
            Assert.That(collectionNames.Last().Value, Is.EqualTo(collection.Translations.Last().Name));

            // Entries
            Assert.That(result.Entries, Is.Not.Empty);
            Assert.That(result.Entries.Count, Is.EqualTo(1));
            var sparqlEntry = result.Entries!.First();
            Assert.That(sparqlEntry.Id, Is.EqualTo(entry.Id));
            Assert.That(sparqlEntry.Modified, Is.EqualTo(entry.Modified));
            Assert.That(sparqlEntry.AdminInfoCodeItems, Has.Count.EqualTo(3));
            Assert.That(sparqlEntry.AdminInfoCodeItems.First(), Is.EqualTo("AdminInfoA"));
            Assert.That(sparqlEntry.Status, Is.EqualTo(entry.Status));
            Assert.That(sparqlEntry.Parts, Has.Count.EqualTo(2));
            Assert.That(sparqlEntry.References, Has.Count.EqualTo(2));

            // Office
            Assert.That(sparqlEntry.Office, Is.Not.Null);
            Assert.That(sparqlEntry.Office.Code, Is.EqualTo(entry.Office.Code));
            Assert.That(sparqlEntry.Office.Names, Has.Count.EqualTo(2));
            Assert.That(sparqlEntry.Office.Names.First().Value, Is.EqualTo(entry.Office.Translations.First().Name));
            Assert.That(sparqlEntry.Office.Names.First().Language, Is.EqualTo(Api.Models.Language.De));

            // Reliability
            Assert.That(sparqlEntry.Reliability, Is.Not.Null);
            Assert.That(sparqlEntry.Reliability.Code, Is.EqualTo(((int)entry.Reliability).ToString()));
            Assert.That(sparqlEntry.Reliability.Names, Has.Count.EqualTo(4));

            // Glossaries
            Assert.That(sparqlEntry.Glossaries, Is.Not.Null);
            Assert.That(sparqlEntry.Glossaries, Has.Count.EqualTo(2));
            Assert.That(sparqlEntry.Glossaries.First().Code, Is.EqualTo(entry.Glossaries.First().Code));
            Assert.That(sparqlEntry.Glossaries.First().Names, Has.Count.EqualTo(2));
            Assert.That(sparqlEntry.Glossaries.First().Names.First().Value,
                Is.EqualTo(entry.Glossaries.First().Translations.First().Name));
            Assert.That(sparqlEntry.Glossaries.First().Names.First().Language, Is.EqualTo(Api.Models.Language.De));

            // Domains (aka Subjects)
            Assert.That(sparqlEntry.Domains, Is.Not.Null);
            Assert.That(sparqlEntry.Domains, Has.Count.EqualTo(2));
            Assert.That(sparqlEntry.Domains.First().Id, Is.EqualTo(entry.Subjects.First().Id));
            Assert.That(sparqlEntry.Domains.First().Names, Has.Count.EqualTo(2));
            Assert.That(sparqlEntry.Domains.First().Names.First().Value,
                Is.EqualTo(entry.Subjects.First().Translations.First().Name));
            Assert.That(sparqlEntry.Domains.First().Names.First().Language, Is.EqualTo(Api.Models.Language.De));

            // ChildDomains (aka Domains)
            var childDomains = sparqlEntry.Domains.First().ChildDomains;
            Assert.That(childDomains, Has.Count.EqualTo(2));
            Assert.That(childDomains.First().Id, Is.EqualTo(entry.Domains.First().Id));
            Assert.That(childDomains.First().Names, Has.Count.EqualTo(2));
            Assert.That(childDomains.First().Names.First().Value,
                Is.EqualTo(entry.Domains.First().Translations.First().Name));
            Assert.That(childDomains.First().Names.First().Language, Is.EqualTo(Api.Models.Language.De));

            // References
            var reference1 = sparqlEntry.References.First();
            var reference2 = sparqlEntry.References.Last();

            Assert.That(reference1.Id, Is.EqualTo(entry.References.First().ReferencedEntryId));
            Assert.That(reference1.Type, Is.EqualTo(entry.References.First().Type.ToSparqlReferenceType()));
            Assert.That(reference2.Id, Is.EqualTo(entry.References.Last().ReferencedEntryId));
            Assert.That(reference2.Type, Is.EqualTo(entry.References.Last().Type.ToSparqlReferenceType()));

            // Parts
            var part1 = sparqlEntry.Parts.First();
            var part2 = sparqlEntry.Parts.Last();
            var entryLanguage1 = entry.Translations.ElementAt(0);
            var entryLanguage2 = entry.Translations.ElementAt(1);

            Assert.That(part1.Sequence, Is.EqualTo(entryLanguage1.Sequence));
            Assert.That(part1.Language, Is.EqualTo(entryLanguage1.Language.ToSparqlLanguage()));
            Assert.That(part1.Term, Is.EqualTo(entryLanguage1.Terminus));
            Assert.That(part1.TermCitation, Is.EqualTo(entryLanguage1.TerminusSource));
            Assert.That(part1.Name, Is.EqualTo(entryLanguage1.Name));
            Assert.That(part1.NameCitation, Is.EqualTo(entryLanguage1.NameSource));
            Assert.That(part1.Abbreviation, Is.EqualTo(entryLanguage1.Abbreviation));
            Assert.That(part1.AbbreviationCitation, Is.EqualTo(entryLanguage1.AbbreviationSource));
            Assert.That(part1.Phraseology, Is.EqualTo(entryLanguage1.Phraseology));
            Assert.That(part1.PhraseologyCitation, Is.EqualTo(entryLanguage1.PhraseologySource));
            Assert.That(part1.Definition, Is.EqualTo(entryLanguage1.Definition));
            Assert.That(part1.DefinitionCitation, Is.EqualTo(entryLanguage1.DefinitionSource));
            Assert.That(part1.Note, Is.EqualTo(entryLanguage1.Note));
            Assert.That(part1.NoteCitation, Is.EqualTo(entryLanguage1.NoteSource));
            Assert.That(part1.Context, Is.EqualTo(entryLanguage1.Context));
            Assert.That(part1.Country, Is.EqualTo(entryLanguage1.Country));
            Assert.That(part1.Deprecated, Is.False);

            Assert.That(part2.Sequence, Is.EqualTo(entryLanguage2.Sequence));
            Assert.That(part2.Language, Is.EqualTo(entryLanguage2.Language.ToSparqlLanguage()));
            Assert.That(part2.Term, Is.EqualTo(entryLanguage2.Terminus));
            Assert.That(part2.TermCitation, Is.EqualTo(entryLanguage2.TerminusSource));
            Assert.That(part2.Name, Is.EqualTo(entryLanguage2.Name));
            Assert.That(part2.NameCitation, Is.EqualTo(entryLanguage2.NameSource));
            Assert.That(part2.Abbreviation, Is.EqualTo(entryLanguage2.Abbreviation));
            Assert.That(part2.AbbreviationCitation, Is.EqualTo(entryLanguage2.AbbreviationSource));
            Assert.That(part2.Phraseology, Is.EqualTo(entryLanguage2.Phraseology));
            Assert.That(part2.PhraseologyCitation, Is.EqualTo(entryLanguage2.PhraseologySource));
            Assert.That(part2.Definition, Is.EqualTo(entryLanguage2.Definition));
            Assert.That(part2.Deprecated, Is.True);
        });
    }
}
