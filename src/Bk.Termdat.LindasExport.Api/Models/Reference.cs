using System.Globalization;
using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Extensions;
using VDS.RDF;

namespace Bk.Termdat.LindasExport.Api.Models;

public class Reference
{
    public required int Id { get; init; }
    public required ReferenceType Type { get; init; }

    public Triple GetTriple(IGraph graph, IUriNode subject)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var referencePredicate = graph.CreateUriNode(UriFactory.Create(Type.GetUri()));
        return new Triple(subject, referencePredicate, graph.CreateUriNode(UriFactory.Create(string.Format(CultureInfo.InvariantCulture, Uris.TermdatEntryUriFormat, Id))));
    }
}
