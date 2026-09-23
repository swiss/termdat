using System;
using System.Collections.Generic;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bogus;

namespace Bk.Termdat.Infrastructure.Search.Tests;

public class TestHelpers
{
    public TestHelpers()
    {
        Randomizer.Seed = new Random(8675309);
    }

    public IList<IndexEntry> GenerateIndexEntries(int count = 4000)
    {
        var i = 0;

        var faker = new Faker<IndexEntry>()
            .RuleFor(e => e.EntryId, _ => i++ % 1000)
            .RuleFor(e => e.LanguageId, f => f.PickRandom(new List<int> { 2, 3, 6, 7 }))
            .RuleFor(e => e.LenochClassifications, new[] { 200, 300 })
            .RuleFor(e => e.CollectionId, f => f.PickRandom(new List<int> { 68, 10301, 10299, 10297, 10292 }))
            .RuleFor(e => e.RelatedCollections, new[] { 200, 300 })
            .RuleFor(e => e.OfficeId, f => f.PickRandom(new List<int> { 1, 3, 10, 11, 12, 13, 14 }))
            .RuleFor(e => e.Glossaries, new[] { 500, 600 })
            .RuleFor(e => e.CollectionIsActive, true)
            .RuleFor(e => e.References, f => f.Random.Bool(0.15f) && i > 0 && i < count ? new[] { i - 1, i + 1 } : Array.Empty<int>())
            .RuleFor(e => e.Status, f => f.PickRandom<Status>())
            .RuleFor(e => e.Reliability, f => f.PickRandom<Reliability>())
            .RuleFor(e => e.AdminInfos, new[] { 1, 3, 4, 6, 33, 333 })
            .RuleFor(e => e.Name, f => f.Hacker.Noun())
            .RuleFor(e => e.NameSource, f => f.Lorem.Sentence(f.Random.Int(5, 10)))
            .RuleFor(e => e.Phraseology, f => f.Hacker.Phrase())
            .RuleFor(e => e.PhraseologySource, f => f.Lorem.Sentence(f.Random.Int(5, 10)))
            .RuleFor(e => e.Abbreviation, f => f.Hacker.Abbreviation())
            .RuleFor(e => e.AbbreviationSource, f => f.Lorem.Sentence(f.Random.Int(5, 10)))
            .RuleFor(e => e.Comment, f => f.Lorem.Sentence(f.Random.Int(1, 5)))
            .RuleFor(e => e.Country, f => f.Address.Country())
            .RuleFor(e => e.Note, f => f.Lorem.Sentence(f.Random.Int(5, 10)))
            .RuleFor(e => e.NoteSource, f => f.Lorem.Sentence(f.Random.Int(5, 10)))
            .RuleFor(e => e.Metadata, f => f.Lorem.Sentence(f.Random.Int(5, 10)))
            .RuleFor(e => e.MetadataSource, f => f.Lorem.Sentence(f.Random.Int(5, 10)))
            .RuleFor(e => e.Context, f => f.Lorem.Sentence(f.Random.Int(1, 5)))
            .RuleFor(e => e.ContextSource, f => f.Lorem.Sentence(f.Random.Int(1, 5)))
            .RuleFor(e => e.Terminus, f => f.Lorem.Sentence(f.Random.Int(1, 5)))
            .RuleFor(e => e.TerminusSource, f => f.Lorem.Sentence(f.Random.Int(1, 5)))
            .RuleFor(e => e.Definition, f => f.Lorem.Sentence(f.Random.Int(1, 5)))
            .RuleFor(e => e.DefinitionSource, f => f.Lorem.Sentence(f.Random.Int(1, 5)))
            .RuleFor(e => e.Modified, f => f.PickRandom(new List<DateTime> { DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-380), DateTime.UtcNow.AddDays(-800), }))
            .RuleFor(e => e.CreatedBy, f => f.PickRandom(new List<string> { "test@test.ch", "sergio@test.ch", "pascal@test.ch", "michael@test.ch" }))
            .RuleFor(e => e.ModifiedBy, f => f.PickRandom(new List<string> { "test@test.ch", "sergio@test.ch", "pascal@test.ch", "michael@test.ch" }))
            .RuleFor(e => e.LanguageCount, f => f.Random.Int(1, 10));

        return faker.Generate(count);
    }

    public static IList<IndexEntry> GetStaticIndexEntries()
    {
        return new List<IndexEntry>
        {
            new()
            {
                EntryId = 1,
                SequenceId = 1,
                LanguageId = 2,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 10 },
                Domains = new []{ 100, 101 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "UNIQUE",
                TerminusSource = "Gilliéron"
            },
            new()
            {
                EntryId = 1,
                SequenceId = 1,
                LanguageId = 3,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 10 },
                Domains = new []{ 100, 101 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Name = "UNIQUE Name",
                TerminusSource = "Gilliéron"
            },
            new()
            {
                EntryId = 1,
                SequenceId = 2,
                LanguageId = 3,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 10 },
                Domains = new []{ 100, 101 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Phraseology = "UNIQUE Phraseology",
            },
            new()
            {
                EntryId = 2,
                SequenceId = 1,
                LanguageId = 2,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 11 },
                Domains = new []{ 102 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "UNIQUE Terminus",
                TerminusSource = "Gilliéron"
            },
            new()
            {
                EntryId = 3,
                SequenceId = 1,
                LanguageId = 2,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 10 },
                Domains = new []{ 100 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "PARTIALUNIQUE",
                TerminusSource = "Gilliéron"
            },
            new()
            {
                EntryId = 3,
                SequenceId = 2,
                LanguageId = 2,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Subjects = new []{ 10 },
                Domains = new []{ 100 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "UNIQUEPARTIAL",
                TerminusSource = "Giîlîéron"
            },
            new()
            {
                EntryId = 4,
                SequenceId = 1,
                LanguageId = 6,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Subjects = new []{ 11 },
                Domains = new []{ 102 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "UNIQUE Approved",
            },
            new()
            {
                EntryId = 4,
                SequenceId = 1,
                LanguageId = 6,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 11 },
                Domains = new []{ 102 },
                Status = Status.Validated,
                Reliability = Reliability.Tentative,
                Terminus = "UNIQUE Tentative",
                TerminusSource = "Gillieron"
            },
            new()
            {
                EntryId = 4,
                SequenceId = 1,
                LanguageId = 6,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 11 },
                Domains = new []{ 102 },
                Status = Status.Validated,
                Reliability = Reliability.FirstDraft,
                Terminus = "UNIQUE FirstDraft",
            },
            new()
            {
                EntryId = 4,
                SequenceId = 1,
                LanguageId = 6,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 11 },
                Domains = new []{ 102 },
                Status = Status.Validated,
                Reliability = Reliability.Verified,
                Terminus = "UNIQUE Verified",
                Name = "Verified name ",
                Abbreviation = "Verified abbr",
                Phraseology = "Verified phrase",
                Definition = "Verified def",
                Note = "Verified note",
                Context = "Verified ctx",
            },
            new()
            {
                EntryId = 4,
                SequenceId = 1,
                LanguageId = 6,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 11 },
                Domains = new []{ 102 },
                Status = Status.Validated,
                Reliability = Reliability.Translated,
                Terminus = "UNIQUE Translated",
            },
            new()
            {
                EntryId = 5,
                SequenceId = 5,
                LanguageId = 7,
                OfficeId = 3,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 11 },
                Domains = new []{ 103 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "uNiQuE Office3",
            },
            new()
            {
                EntryId = 5,
                SequenceId = 5,
                LanguageId = 7,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 11 },
                Domains = new []{ 103 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "uNiQuE Office1",
            },
            new()
            {
                EntryId = 6,
                SequenceId = 5,
                LanguageId = 8,
                OfficeId = 3,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 12 },
                Status = Status.InProgress,
                Reliability = Reliability.Approved,
                Terminus = "UNIQUE InProgress",
                Comment = "Very bad contribution",
            },
            new()
            {
                EntryId = 6,
                SequenceId = 5,
                LanguageId = 8,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 12 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "UNIQUE Validated",
                Comment = "Very bad contribution",
            },
            new()
            {
                EntryId = 9,
                SequenceId = 1,
                LanguageId = 10,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 13 },
                Domains = new []{ 104 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "SUPERUNIQUE",
                Comment = "Very bad contribution",
            },
            new()
            {
                EntryId = 9,
                SequenceId = 2,
                LanguageId = 10,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 13 },
                Domains = new []{ 104 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "SUPERUNIQUE",
                Comment = "Very bad contribution",
            },
            new()
            {
                EntryId = 10,
                SequenceId = 1,
                LanguageId = 10,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 13 },
                Domains = new []{ 104 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "PREFERRED 1",
                Comment = "Excellent contribution",
            },
            new()
            {
                EntryId = 10,
                SequenceId = 2,
                LanguageId = 10,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 13 },
                Domains = new []{ 104 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "preferred 2",
                Comment = "Excellent contribution",
            },
            new()
            {
                EntryId = 10,
                SequenceId = 3,
                LanguageId = 10,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 13 },
                Domains = new []{ 104 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "PrEfErReD 3",
                Comment = "Excellent contribution",
            },
            new()
            {
                EntryId = 10,
                SequenceId = 3,
                LanguageId = 10,
                OfficeId = 1,
                CollectionId = 68,
                CollectionIsActive = true,
                RelatedCollections = new [] { 300 },
                LenochClassifications = new []{ 200 },
                Glossaries = new []{ 300 },
                Subjects = new []{ 13 },
                Domains = new []{ 104 },
                Status = Status.Validated,
                Reliability = Reliability.Approved,
                Terminus = "Hundehalterinnen und -halter",
                Comment = "Excellent contribution",
            },
        };
    }
}
