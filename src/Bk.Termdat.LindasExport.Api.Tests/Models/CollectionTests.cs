using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Extensions;
using Bk.Termdat.LindasExport.Api.Models;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Bk.Termdat.LindasExport.Api.Tests.Models;

[TestFixture]
internal class CollectionTests
{
    private static readonly Collection _collection = new()
    {
        Code = "PFS17",
        Names = new List<Text>
        {
            new() { Value = "Schweizerische parlamentarische Terminologie", Language = Language.De },
            new() { Value = "Terminologie parlementaire suisse", Language = Language.Fr }
        }
    };

    [Test]
    public void GetTriples_ShouldReturnTriples()
    {
        var graph = new Graph { BaseUri = UriFactory.Create(Uris.TermdatGraph) };

        var triples = _collection.GetTriples(graph, false).ToList();

        Assert.That(triples, Has.Count.EqualTo(5));

        // All should have same subject
        foreach (var triple in triples)
        {
            Assert.That(triple.Subject.AsValuedNode().AsString(), Is.EqualTo(_collection.Uri));
        }

        // Types
        var typeTriple = triples.FirstOrDefault(item => item.Predicate.AsValuedNode().AsString() == Uris.Type);
        Assert.That(typeTriple, Is.Not.Null);
        Assert.That(typeTriple!.Object.AsValuedNode().ToString(), Is.EqualTo(Uris.DefinedTermSet));

        // Identifier
        var identifierTriple = triples.FirstOrDefault(item => item.Predicate.AsValuedNode().AsString() == Uris.Identifier);
        Assert.That(identifierTriple, Is.Not.Null);
        Assert.That(identifierTriple!.Object.AsValuedNode().AsString(), Is.EqualTo(_collection.Code));

        // Names
        var nameTriples = triples.Where(item => item.Predicate.AsValuedNode().AsString() == Uris.Name).ToList();
        Assert.That(nameTriples, Has.Count.EqualTo(2));
        Assert.That(nameTriples.First().Object.AsValuedNode().ToString(), Is.EqualTo($"{_collection.Names!.First().Value}@{_collection.Names!.First().Language.ToLowerCaseStringValue()}"));
        Assert.That(nameTriples.Last().Object.AsValuedNode().ToString(), Is.EqualTo($"{_collection.Names!.Last().Value}@{_collection.Names!.Last().Language.ToLowerCaseStringValue()}"));
    }
}
