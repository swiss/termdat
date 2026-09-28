using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Extensions;
using Swiss.FCh.Sparql.Extensions;
using VDS.RDF;

namespace Bk.Termdat.LindasExport.Api.Models;

public class EntryPart
{
    public required int Sequence { get; init; }
    public required Language Language { get; init; }

    public string? Term { get; init; }
    public string? TermCitation { get; init; }
    public string? Name { get; init; }
    public string? NameCitation { get; init; }
    public string? Abbreviation { get; init; }
    public string? AbbreviationCitation { get; init; }
    public string? Phraseology { get; init; }
    public string? PhraseologyCitation { get; init; }
    public string? Definition { get; init; }
    public string? DefinitionCitation { get; init; }
    public string? Note { get; init; }
    public string? NoteCitation { get; init; }
    public string? Context { get; init; }
    public string? Country { get; init; }
    public bool Deprecated { get; init; } = false;

    public EntryType GetEntryType()
    {
        if (Name is not null)
        {
            return EntryType.Name;
        }

        if (Term is not null)
        {
            return EntryType.Term;
        }

        if (Phraseology is not null)
        {
            return EntryType.Phraseology;
        }

        if (Abbreviation is not null)
        {
            return EntryType.Abbreviation;
        }

        return EntryType.None;
    }

    public IEnumerable<Triple> GetTriplesByType(IGraph graph, IUriNode subject, EntryType type)
    {
        var name = GetTextByType(type);
        var citation = GetCitationByType(type);
        var language = Language.ToLowerCaseStringValue();

        if (name is not null)
        {
            var namePredicate = graph.CreateUriNode(UriFactory.Create(Uris.Name));
            yield return new Triple(
                subject,
                namePredicate,
                graph.CreateLiteralNode(name.AsSanitizedXmlString(), language));
        }

        if (citation is not null)
        {
            var citationPredicate = graph.CreateUriNode(UriFactory.Create(Uris.Citation));
            yield return new Triple(
                subject,
                citationPredicate,
                graph.CreateLiteralNode(citation.AsSanitizedXmlString(), language));
        }

        if (type is not EntryType.Abbreviation && Abbreviation is not null)
        {
            var alternateNamePredicate = graph.CreateUriNode(UriFactory.Create(Uris.AlternateName));
            yield return new Triple(
                subject,
                alternateNamePredicate,
                graph.CreateLiteralNode(Abbreviation.AsSanitizedXmlString(), language));
        }

        if (Definition is not null)
        {
            var descriptionPredicate = graph.CreateUriNode(UriFactory.Create(Uris.Description));
            yield return new Triple(
                subject,
                descriptionPredicate,
                graph.CreateLiteralNode(Definition.AsSanitizedXmlString(), language));
        }

        if (DefinitionCitation is not null)
        {
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.DescriptionCitationType)),
                graph.CreateLiteralNode(DefinitionCitation.AsSanitizedXmlString(), language));
        }

        if (Deprecated)
        {
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.Type)),
                graph.CreateUriNode(UriFactory.Create(Uris.Deprecated)));
        }

        if (Note is not null)
        {
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.TermNoteType)),
                graph.CreateLiteralNode(Note.AsSanitizedXmlString(), language));
        }

        if (NoteCitation is not null)
        {
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.TermNoteCitationType)),
                graph.CreateLiteralNode(NoteCitation.AsSanitizedXmlString(), language));
        }

        if (Context is not null)
        {
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.LinguisticContextType)),
                graph.CreateLiteralNode(Context.AsSanitizedXmlString()));
        }

        if (Country is not null)
        {
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.ApplicableCountryType)),
                graph.CreateLiteralNode(Country.AsSanitizedXmlString(), language));
        }
    }

    public string? GetTextByType(EntryType type)
    {
        return type switch
        {
            EntryType.Name => Name,
            EntryType.Term => Term,
            EntryType.Phraseology => Phraseology,
            EntryType.Abbreviation => Abbreviation,
            EntryType.None or _ => null
        };
    }

    public string? GetCitationByType(EntryType type)
    {
        return type switch
        {
            EntryType.Name => NameCitation,
            EntryType.Term => TermCitation,
            EntryType.Phraseology => PhraseologyCitation,
            EntryType.Abbreviation => AbbreviationCitation,
            EntryType.None or _ => null
        };
    }
}
