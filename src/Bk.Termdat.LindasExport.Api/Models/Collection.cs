using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Extensions;
using VDS.RDF;

namespace Bk.Termdat.LindasExport.Api.Models;

public class Collection
{
    public required string Code { get; init; }
    public string Uri => string.Format(Uris.TermdatCollectionUriFormat, Code);
    public ICollection<Text> Names { get; init; } = new List<Text>();
    public ICollection<Entry> Entries { get; init; } = new List<Entry>();

    public IEnumerable<Triple> GetTriples(IGraph graph, bool addCubeLinkData)
    {
        var collectionSubject = graph.CreateUriNode(UriFactory.Create(Uri));

        // Type
        yield return new Triple(
            collectionSubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Type)),
            graph.CreateUriNode(UriFactory.Create(Uris.DefinedTermSet)));

        yield return new Triple(
            collectionSubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Type)),
            graph.CreateUriNode(UriFactory.Create(Uris.TerminologyCollectionType)));

        // Identifier
        yield return new Triple(
            collectionSubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Identifier)),
            graph.CreateLiteralNode(Code));

        // Names
        if (Names is not null && Names.Any())
        {
            var namePredicate = graph.CreateUriNode(UriFactory.Create(Uris.Name));
            foreach (var name in Names.Where(item => !string.IsNullOrWhiteSpace(item.Value)))
            {
                yield return new Triple(collectionSubject, namePredicate,
                    graph.CreateLiteralNode(name.Value, name.Language.ToLowerCaseStringValue()));
            }
        }

        // Entries
        if (Entries is not null)
        {
            foreach (var entry in Entries)
            {
                foreach (var triple in entry.GetTriples(graph, this, addCubeLinkData))
                {
                    yield return triple;
                }
            }
        }
    }
}
