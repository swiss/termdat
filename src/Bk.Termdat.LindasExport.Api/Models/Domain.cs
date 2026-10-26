using System.Globalization;
using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Extensions;
using Swiss.FCh.Sparql.Extensions;
using VDS.RDF;

namespace Bk.Termdat.LindasExport.Api.Models;

#pragma warning disable CA1724 // Domain is a domain entity name.
public class Domain
{
#pragma warning restore CA1724
    public required int Id { get; init; }
    public ICollection<Text> Names { get; init; } = new List<Text>();
    public ICollection<Domain> ChildDomains { get; init; } = new List<Domain>();

    public IEnumerable<Triple> GetTriples(IGraph graph, IUriNode subject, int? parentDomainId = null)
    {
        ArgumentNullException.ThrowIfNull(graph);
        return GetTriplesIterator(graph, subject, parentDomainId);
    }

    private IEnumerable<Triple> GetTriplesIterator(IGraph graph, IUriNode subject, int? parentDomainId = null)
    {
        var domainSubject = graph.CreateUriNode(UriFactory.Create(string.Format(CultureInfo.InvariantCulture,
            Uris.TermdatEntryAdditionUriFormat,
            parentDomainId.HasValue ? Uris.TermdatSubDomainUriAddition : Uris.TermdatDomainUriAddition, Id)));

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.TermDomain)),
            domainSubject);

        yield return new Triple(
            domainSubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Type)),
            graph.CreateUriNode(UriFactory.Create(parentDomainId.HasValue ? Uris.TermSubDomainType : Uris.TermDomainType)));

        // HasPart, IsPartOf hierarchy
        if (parentDomainId.HasValue)
        {
            var parentDomainSubject = graph.CreateUriNode(UriFactory.Create(string.Format(CultureInfo.InvariantCulture,
                Uris.TermdatEntryAdditionUriFormat,
                Uris.TermdatDomainUriAddition, parentDomainId)));

            yield return new Triple(
                parentDomainSubject,
                graph.CreateUriNode(UriFactory.Create(Uris.HasPart)),
                domainSubject);

            yield return new Triple(
                domainSubject,
                graph.CreateUriNode(UriFactory.Create(Uris.IsPartOf)),
                parentDomainSubject);
        }

        // Identifier
        yield return new Triple(
            domainSubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Identifier)),
            graph.CreateLiteralNode(Id.ToString(CultureInfo.InvariantCulture)));

        // Names
        if (Names is not null && Names.Count > 0)
        {
            var namePredicate = graph.CreateUriNode(UriFactory.Create(Uris.Name));
            foreach (var name in Names.Where(item => !string.IsNullOrWhiteSpace(item.Value)))
            {
                yield return new Triple(domainSubject, namePredicate,
                    graph.CreateLiteralNode(name.Value.AsSanitizedXmlString(), name.Language.ToLowerCaseStringValue()));
            }
        }

        // Aka Domains
        foreach (var domain in ChildDomains)
        {
            foreach (var triple in domain.GetTriples(graph, subject, Id))
            {
                yield return triple;
            }
        }
    }
}
