using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Extensions;
using VDS.RDF;

namespace Bk.Termdat.LindasExport.Api.Models;

public class Entry
{
    public required int Id { get; init; }
    public DateTime Modified { get; init; }
    public Status Status { get; init; }
    public required Office Office { get; init; }
    public required Reliability Reliability { get; init; }
    public ICollection<Glossary> Glossaries { get; set; } = new List<Glossary>();
    public ICollection<Domain> Domains { get; init; } = new List<Domain>();
    public ICollection<string> AdminInfoCodeItems { get; init; } = new List<string>();
    public ICollection<EntryPart> Parts { get; init; } = new List<EntryPart>();
    public ICollection<Reference> References { get; init; } = new List<Reference>();

    public IEnumerable<Triple> GetTriples(IGraph graph, Collection collection, bool addCubeLinkData)
    {
        var entrySubject = graph.CreateUriNode(UriFactory.Create(string.Format(Uris.TermdatEntryUriFormat, Id)));

        // DateModified
        yield return new Triple(
            entrySubject,
            graph.CreateUriNode(UriFactory.Create(Uris.DateModified)),
            graph.CreateLiteralNode(Modified.ToString("O"), UriFactory.Create("xsd:dateTime")));

        // AdminInfos
        foreach (var adminInfoCode in AdminInfoCodeItems)
        {
            var adminInfoSubject = graph.CreateUriNode(UriFactory.Create(string.Format(
                Uris.TermdatEntryAdditionUriFormat,
                Uris.TermdatAdministrativeNoteUriAddition, adminInfoCode)));

            yield return new Triple(
                entrySubject,
                graph.CreateUriNode(UriFactory.Create(Uris.InDefinedTermSet)),
                adminInfoSubject);

            yield return new Triple(
                adminInfoSubject,
                graph.CreateUriNode(UriFactory.Create(Uris.Type)),
                graph.CreateUriNode(UriFactory.Create(Uris.AdministrativeNoteType)));
        }

        // Status
        if (Status == Status.InProgress)
        {
            yield return new Triple(
                entrySubject,
                graph.CreateUriNode(UriFactory.Create(Uris.Type)),
                graph.CreateUriNode(UriFactory.Create(Uris.InProgressType)));
        }
        else if (Status == Status.Validated)
        {
            yield return new Triple(
                entrySubject,
                graph.CreateUriNode(UriFactory.Create(Uris.Type)),
                graph.CreateUriNode(UriFactory.Create(Uris.ValidatedEntryType)));
        }

        // Cube Link data
        if (addCubeLinkData)
        {
            yield return new Triple(
                entrySubject,
                graph.CreateUriNode(UriFactory.Create(Uris.Type)),
                graph.CreateUriNode(UriFactory.Create(Uris.ObservationType)));

            yield return new Triple(
                entrySubject,
                graph.CreateUriNode(UriFactory.Create(Uris.ObservedBy)),
                graph.CreateUriNode(UriFactory.Create(Uris.ObservedByObject)));

            yield return new Triple(
                graph.CreateUriNode(UriFactory.Create(Uris.DataSetUriObservation)),
                graph.CreateUriNode(UriFactory.Create(Uris.Observation)),
                entrySubject);
        }

        // Office
        foreach (var triple in Office.GetTriples(graph, entrySubject))
        {
            yield return triple;
        }

        // Glossaries
        foreach (var glossary in Glossaries)
        {
            foreach (var triple in glossary.GetTriples(graph, entrySubject))
            {
                yield return triple;
            }
        }

        // Aka Subjects
        foreach (var domain in Domains)
        {
            foreach (var triple in domain.GetTriples(graph, entrySubject))
            {
                yield return triple;
            }
        }

        // Reliability
        foreach (var triple in Reliability.GetTriples(graph, entrySubject))
        {
            yield return triple;
        }

        var mainParts = Parts.Where(p => p.Sequence == 1).ToList();
        var hasPartPredicate = graph.CreateUriNode(UriFactory.Create(Uris.HasPart));
        const string firstPosition = "1";

        // Determine main entry type using the following weighting: Name > Term > Phraseology > Abbreviation
        var mainEntryType = GetMainEntryType(mainParts);

        // Main entry metadata like Type, URL etc
        foreach (var triple in GetMetaDataTriples(graph, entrySubject, mainEntryType, firstPosition, Id.ToString(),
                     collection.Uri))
        {
            yield return triple;
        }

        // References
        foreach (var reference in References)
        {
            yield return reference.GetTriple(graph, entrySubject);
        }

        // Check for main entry child of type: Term
        IUriNode? termSubject = null;
        if (mainEntryType != EntryType.Term && mainParts.Any(p => p.Term is not null))
        {
            var termSubjectUri = string.Format(Uris.TermdatEntryAdditionUriFormat, Id, Uris.TermdatTermUriAddition);
            termSubject = graph.CreateUriNode(UriFactory.Create(termSubjectUri));

            foreach (var triple in GetMetaDataTriples(graph, termSubject, EntryType.Term, firstPosition))
            {
                yield return triple;
            }

            yield return new Triple(
                entrySubject,
                hasPartPredicate,
                graph.CreateUriNode(UriFactory.Create(termSubjectUri)));
        }

        // Check for main entry child of type: Phraseology
        IUriNode? phraseologySubject = null;
        if (mainEntryType != EntryType.Phraseology && mainParts.Any(p => p.Phraseology is not null))
        {
            var phraseologySubjectUri =
                string.Format(Uris.TermdatEntryAdditionUriFormat, Id, Uris.TermdatPhraseologyUriAddition);
            phraseologySubject = graph.CreateUriNode(UriFactory.Create(phraseologySubjectUri));

            foreach (var triple in GetMetaDataTriples(graph, phraseologySubject, EntryType.Phraseology, firstPosition))
            {
                yield return triple;
            }

            yield return new Triple(
                entrySubject,
                hasPartPredicate,
                graph.CreateUriNode(UriFactory.Create(phraseologySubjectUri)));
        }

        // Check for main entry child of type: Abbreviation
        IUriNode? abbreviationSubject = null;
        if (mainEntryType != EntryType.Abbreviation && mainParts.Any(p => p.Abbreviation is not null) &&
            mainParts.Any(p => p.AbbreviationCitation is not null))
        {
            var abbreviationSubjectUri =
                string.Format(Uris.TermdatEntryAdditionUriFormat, Id, Uris.TermdatAbbreviationUriAddition);
            abbreviationSubject = graph.CreateUriNode(UriFactory.Create(abbreviationSubjectUri));

            foreach (var triple in GetMetaDataTriples(graph, abbreviationSubject, EntryType.Abbreviation,
                         firstPosition))
            {
                yield return triple;
            }

            yield return new Triple(
                entrySubject,
                hasPartPredicate,
                graph.CreateUriNode(UriFactory.Create(abbreviationSubjectUri)));
        }

        // Get actual content of main entry and main entry children as triples
        foreach (var mainPart in mainParts)
        {
            var triplesByType = mainPart.GetTriplesByType(graph, entrySubject, mainEntryType);
            foreach (var triple in triplesByType)
            {
                yield return triple;
            }

            if (termSubject is not null)
            {
                var termTriples = mainPart.GetTriplesByType(graph, termSubject, EntryType.Term);
                foreach (var triple in termTriples)
                {
                    yield return triple;
                }
            }

            if (phraseologySubject is not null)
            {
                var termTriples = mainPart.GetTriplesByType(graph, phraseologySubject, EntryType.Phraseology);
                foreach (var triple in termTriples)
                {
                    yield return triple;
                }
            }

            if (abbreviationSubject is not null)
            {
                var termTriples = mainPart.GetTriplesByType(graph, abbreviationSubject, EntryType.Abbreviation);
                foreach (var triple in termTriples)
                {
                    yield return triple;
                }
            }
        }

        // SubParts that are not Sequence = 1 are synonyms
        var subParts = Parts.Where(p => p.Sequence != 1).OrderBy(x => x.Sequence).ThenBy(x => x.Language).ToList();

        foreach (var subPart in subParts)
        {
            var language = subPart.Language.ToLowerCaseStringValue();
            var subPartUri = string.Format(Uris.TermdatEntryPartUriFormat, Id, subPart.Sequence, language);

            // Determine sub part typ
            var subPartType = subPart.GetEntryType();

            // Some synonyms only have context (probably legacy data) so everything else is null and those entries must be skipped
            if (subPartType == EntryType.None)
            {
                continue;
            }

            var subPartSubject = graph.CreateUriNode(UriFactory.Create(subPartUri));
            yield return new Triple(
                entrySubject,
                hasPartPredicate,
                graph.CreateUriNode(UriFactory.Create(subPartUri)));

            foreach (var triple in GetMetaDataTriples(graph, subPartSubject, subPartType, subPart.Sequence.ToString(),
                         isSynonym: true))
            {
                yield return triple;
            }

            foreach (var triple in subPart.GetTriplesByType(graph, subPartSubject, subPartType))
            {
                yield return triple;
            }

            // Check for sub entry child of type: Term
            if (subPart.Term is not null && subPartType != EntryType.Term)
            {
                foreach (var triple in GetSubPartAdditionByType(graph, subPart, EntryType.Term, subPartSubject))
                {
                    yield return triple;
                }
            }

            // Check for sub entry child of type: Phraseology
            if (subPart.Phraseology is not null && subPartType != EntryType.Phraseology)
            {
                foreach (var triple in GetSubPartAdditionByType(graph, subPart, EntryType.Phraseology, subPartSubject))
                {
                    yield return triple;
                }
            }

            // Check for sub entry child of type: Abbreviation
            if (subPart.Abbreviation is not null && subPart.AbbreviationCitation is not null &&
                subPartType != EntryType.Abbreviation)
            {
                foreach (var triple in GetSubPartAdditionByType(graph, subPart, EntryType.Abbreviation, subPartSubject))
                {
                    yield return triple;
                }
            }
        }
    }

    private IEnumerable<Triple> GetSubPartAdditionByType(IGraph graph, EntryPart subPart, EntryType entryType,
        INode subPartSubject)
    {
        var hasPartPredicate = graph.CreateUriNode(UriFactory.Create(Uris.HasPart));
        var termSubPartUri = string.Format(Uris.TermdatEntryPartAdditionUriFormat, Id, subPart.Sequence,
            entryType.GetUriAddition(), subPart.Language.ToLowerCaseStringValue());
        var termSubPartSubject = graph.CreateUriNode(UriFactory.Create(termSubPartUri));

        yield return new Triple(
            subPartSubject,
            hasPartPredicate,
            graph.CreateUriNode(UriFactory.Create(termSubPartUri)));

        foreach (var triple in GetMetaDataTriples(graph, termSubPartSubject, entryType, subPart.Sequence.ToString(),
                     isSynonym: true))
        {
            yield return triple;
        }

        foreach (var triple in subPart.GetTriplesByType(graph, termSubPartSubject, entryType))
        {
            yield return triple;
        }
    }

    private static IEnumerable<Triple> GetMetaDataTriples(INodeFactory graph, INode subject, EntryType type,
        string position, string? id = null, string? collectionUri = null, bool isSynonym = false)
    {
        // Type: DefinedTerm
        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.Type)),
            graph.CreateUriNode(UriFactory.Create(Uris.DefinedTerm)));

        // Type: Entry Type (Term, Abbreviation, Phraseology, Name, Definition)
        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.Type)),
            graph.CreateUriNode(UriFactory.Create(type.GetUri())));

        if (isSynonym)
        {
            // Type: Synonym
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.Type)),
                graph.CreateUriNode(UriFactory.Create(Uris.Synonym)));
        }

        // Identifier
        if (id is not null)
        {
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.Identifier)),
                graph.CreateLiteralNode(id));

            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.Url)),
                graph.CreateLiteralNode(string.Format(Uris.TermdatLinkFormat, id), UriFactory.Create("xsd:anyURI")));
        }

        // TermSet
        if (collectionUri is not null)
        {
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.InDefinedTermSet)),
                graph.CreateUriNode(UriFactory.Create(collectionUri)));
        }

        // Position
        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.Position)),
            graph.CreateLiteralNode(position));
    }

    private static EntryType GetMainEntryType(IReadOnlyCollection<EntryPart> mainParts)
    {
        var type = EntryType.Abbreviation;
        if (mainParts.Any(p => p.Name is not null))
        {
            type = EntryType.Name;
        }
        else if (mainParts.Any(p => p.Term is not null))
        {
            type = EntryType.Term;
        }
        else if (mainParts.Any(p => p.Phraseology is not null))
        {
            type = EntryType.Phraseology;
        }

        return type;
    }
}
