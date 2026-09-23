using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.LindasExport.Api.Extensions;
using Bk.Termdat.LindasExport.Api.Interfaces;
using Bk.Termdat.LindasExport.Api.Models;
using Collection = Bk.Termdat.LindasExport.Api.Models.Collection;
using Domain = Bk.Termdat.LindasExport.Api.Models.Domain;
using Entry = Bk.Termdat.LindasExport.Api.Models.Entry;
using Glossary = Bk.Termdat.LindasExport.Api.Models.Glossary;
using Office = Bk.Termdat.LindasExport.Api.Models.Office;
using Reliability = Bk.Termdat.LindasExport.Api.Models.Reliability;
using Text = Bk.Termdat.LindasExport.Api.Models.Text;

namespace Bk.Termdat.LindasExport.Api.Mapping;

public class LindasMapper : ILindasMapper
{
    public Collection Map(Business.Domain.Models.Collection collection)
    {
        var sparqlCollection = new Collection
        {
            Code = collection.Code,
            Names = collection.Translations.FilterSupportedLanguages()
                .Select(translation => new Text
                {
                    Language = translation.Language.ToSparqlLanguage(),
                    Value = translation.Name
                }).ToList(),
            Entries = collection.Entries
            .Where(e => !e.Deleted)
            .Select(Map).ToList()
        };

        return sparqlCollection;
    }

    private static Entry Map(Business.Domain.Models.Entry entry)
    {
        var mapped = new Entry
        {
            Id = entry.Id,
            Parts = MapParts(entry.Translations).ToList(),
            Modified = entry.Modified,
            AdminInfoCodeItems = entry.AdminInfos?.Select(a => a.Code).ToList() ?? new List<string>(),
            Status = entry.Status,
            Office = new Office
            {
                Code = entry.Office.Code,
                Names = entry.Office.Translations.FilterSupportedLanguages()
                    .Select(translation => new Text
                    {
                        Language = translation.Language.ToSparqlLanguage(),
                        Value = translation.Name
                    }).ToList()
            },
            Reliability = new Reliability
            {
                Code = ((int)entry.Reliability).ToString(),
                Names = Reliability.GetNamesByReliability(entry.Reliability),
            },
            Glossaries = entry.Glossaries.Select(glossary =>
            {
                return new Glossary
                {
                    Code = glossary.Code,
                    Names = glossary.Translations.FilterSupportedLanguages()
                        .Select(translation => new Text
                        {
                            Language = translation.Language.ToSparqlLanguage(),
                            Value = translation.Name
                        }).ToList()
                };
            }).ToList(),
            Domains = MapSubjectsAndDomains(entry.Subjects, entry.Domains),
            References = entry.References.Select(reference => new Reference
            {
                Id = reference.ReferencedEntryId,
                Type = reference.Type.ToSparqlReferenceType()
            }).ToList()
        };

        return mapped;
    }

    private static ICollection<Domain> MapSubjectsAndDomains(ICollection<Subject> subjects,
        ICollection<Business.Domain.Models.Domain> domains)
    {
        var result = subjects.Select(subject =>
        {
            return new Domain
            {
                Id = subject.Id,
                Names = subject.Translations.FilterSupportedLanguages()
                    .Select(translation => new Text
                    {
                        Language = translation.Language.ToSparqlLanguage(),
                        Value = translation.Name
                    }).ToList()
            };
        }).ToList();

        foreach (var domain in domains)
        {
            var subject = result.Single(s => s.Id == domain.SubjectId);

            subject.ChildDomains.Add(new Domain
            {
                Id = domain.Id,
                Names = domain.Translations.FilterSupportedLanguages()
                    .Select(translation => new Text
                    {
                        Language = translation.Language.ToSparqlLanguage(),
                        Value = translation.Name
                    }).ToList()
            });
        }

        return result;
    }

    private static IEnumerable<EntryPart> MapParts(IEnumerable<EntryLanguage> entryLanguages)
    {
        return entryLanguages.Where(entryLanguage =>
                entryLanguage.Name is not null || entryLanguage.Terminus is not null ||
                entryLanguage.Abbreviation is not null || entryLanguage.Phraseology is not null)
            .FilterSupportedLanguages()
            .Select(entryLanguage => new EntryPart
            {
                Sequence = entryLanguage.Sequence,
                Language = entryLanguage.Language.ToSparqlLanguage(),
                Term = entryLanguage.Terminus,
                TermCitation = entryLanguage.TerminusSource,
                Name = entryLanguage.Name,
                NameCitation = entryLanguage.NameSource,
                Abbreviation = entryLanguage.Abbreviation,
                AbbreviationCitation = entryLanguage.AbbreviationSource,
                Phraseology = entryLanguage.Phraseology,
                PhraseologyCitation = entryLanguage.PhraseologySource,
                Definition = entryLanguage.Definition,
                DefinitionCitation = entryLanguage.DefinitionSource,
                Deprecated = entryLanguage.IsObsolete || entryLanguage.ShouldAvoid,
                Note = entryLanguage.Note,
                NoteCitation = entryLanguage.NoteSource,
                Context = entryLanguage.Context,
                Country = entryLanguage.Country
            });
    }
}
