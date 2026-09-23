using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Extensions;
using VDS.RDF;

namespace Bk.Termdat.LindasExport.Api.Models;

public class Glossary
{
    public required string Code { get; init; }
    public ICollection<Text> Names { get; init; } = new List<Text>();

    public IEnumerable<Triple> GetTriples(IGraph graph, IUriNode subject)
    {
        var glossarySubject = graph.CreateUriNode(UriFactory.Create(string.Format(Uris.TermdatEntryAdditionUriFormat,
            Uris.TermdatGlossaryUriAddition, Code)));

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.InDefinedTermSet)),
            glossarySubject);

        // Type
        yield return new Triple(
            glossarySubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Type)),
            graph.CreateUriNode(UriFactory.Create(Uris.DefinedTermSet)));

        yield return new Triple(
            glossarySubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Type)),
            graph.CreateUriNode(UriFactory.Create(Uris.GlossaryType)));

        // Identifier
        yield return new Triple(
            glossarySubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Identifier)),
            graph.CreateLiteralNode(Code));

        // Names
        if (Names is not null && Names.Any())
        {
            var namePredicate = graph.CreateUriNode(UriFactory.Create(Uris.Name));
            foreach (var name in Names.Where(item => !string.IsNullOrWhiteSpace(item.Value)))
            {
                yield return new Triple(glossarySubject, namePredicate,
                    graph.CreateLiteralNode(name.Value, name.Language.ToLowerCaseStringValue()));
            }
        }
    }
}
