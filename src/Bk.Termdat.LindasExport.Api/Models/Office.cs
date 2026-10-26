using System.Globalization;
using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Extensions;
using VDS.RDF;

namespace Bk.Termdat.LindasExport.Api.Models;

#pragma warning disable CA1724 // Office is a domain entity name.
public class Office
{
#pragma warning restore CA1724
    public required string Code { get; init; }
    public ICollection<Text> Names { get; init; } = new List<Text>();

    public IEnumerable<Triple> GetTriples(IGraph graph, IUriNode subject)
    {
        ArgumentNullException.ThrowIfNull(graph);

        return GetTriplesIterator(graph, subject);
    }

    private IEnumerable<Triple> GetTriplesIterator(IGraph graph, IUriNode subject)
    {
        var officeSubject = graph.CreateUriNode(UriFactory.Create(string.Format(CultureInfo.InvariantCulture, Uris.TermdatEntryAdditionUriFormat,
            Uris.TermdatTerminologyOfficeUriAddition, Code)));

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.InDefinedTermSet)),
            officeSubject);

        // Type
        yield return new Triple(
            officeSubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Type)),
            graph.CreateUriNode(UriFactory.Create(Uris.DefinedTermSet)));

        yield return new Triple(
            officeSubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Type)),
            graph.CreateUriNode(UriFactory.Create(Uris.TerminologyOfficeType)));

        // Identifier
        yield return new Triple(
            officeSubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Identifier)),
            graph.CreateLiteralNode(Code));

        // Names
        if (Names is not null && Names.Count > 0)
        {
            var namePredicate = graph.CreateUriNode(UriFactory.Create(Uris.Name));
            foreach (var name in Names.Where(item => !string.IsNullOrWhiteSpace(item.Value)))
            {
                yield return new Triple(officeSubject, namePredicate,
                    graph.CreateLiteralNode(name.Value, name.Language.ToLowerCaseStringValue()));
            }
        }
    }
}
