using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Models;
using VDS.RDF;

namespace Bk.Termdat.LindasExport.Api.Tests.Models;

[TestFixture]
internal class DataSetTests
{
    [Test]
    public void GetTriples_ShouldSucceed()
    {
        var graph = new Graph();
        var triples = DataSet.GetTriples(graph).ToList();

        graph.Assert(triples);

        var dataSetTriples = graph.GetTriplesWithSubject(graph.CreateUriNode(UriFactory.Create(Uris.DataSetUri))).ToList();
        var typeTriples = graph.GetTriplesWithPredicate(graph.CreateUriNode(UriFactory.Create(Uris.Type))).ToList();
        var sparqlEndpointTriple = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.SparqlEndpoint)).ToList();
        var exampleResourceTriple = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.ExampleResource)).ToList();
        var nameTriples = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.Name)).ToList();
        var urlTriple = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.Url)).ToList();
        var creatorTriple = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.Creator)).ToList();
        var descriptionTriples = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.Description)).ToList();
        var publisherTriple = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.Publisher)).ToList();
        var contactPointTriple = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.ContactPoint)).ToList();
        var dateCreatedTriple = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.DateCreated)).ToList();
        var datePublishedTriple = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.DatePublished)).ToList();
        var dateModifiedTriple = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.DateModified)).ToList();
        var landingPageTriple = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.LandingPage)).ToList();
        var accessUrlTriple = graph.GetTriplesWithPredicate(UriFactory.Create(Uris.Access)).ToList();
        var voidTriple = graph.GetTriplesWithSubject(UriFactory.Create(Uris.Void)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(dataSetTriples, Has.Count.EqualTo(25));
            Assert.That(triples, Has.Count.EqualTo(dataSetTriples.Count + voidTriple.Count));

            Assert.That(typeTriples, Has.Count.EqualTo(4));
            Assert.That(typeTriples.Select(x => x.Object.ToString()),
                Is.EquivalentTo(new[] { Uris.RdfsDataSet, Uris.W3DataSet, Uris.DataSetSubject, Uris.LindasDataSet }));

            Assert.That(sparqlEndpointTriple, Has.Count.EqualTo(1));
            Assert.That(sparqlEndpointTriple.First().Object.ToString(), Is.EqualTo(Uris.AdminQuery));

            Assert.That(exampleResourceTriple, Has.Count.EqualTo(1));
            Assert.That(exampleResourceTriple.First().Object.ToString(), Is.EqualTo(Uris.ExampleResourceUrl));

            Assert.That(nameTriples, Has.Count.EqualTo(5));

            Assert.That(nameTriples.Select(x => x.Object.ToString()),
                Is.EquivalentTo(new[]
                {
                    $"{DataSetConstants.TitleDe}@{DataSetConstants.LanguageDe}",
                    $"{DataSetConstants.TitleFr}@{DataSetConstants.LanguageFr}",
                    $"{DataSetConstants.TitleIt}@{DataSetConstants.LanguageIt}",
                    $"{DataSetConstants.TitleEn}@{DataSetConstants.LanguageEn}",
                    $"{DataSetConstants.TitleRm}@{DataSetConstants.LanguageRm}"
                }));

            Assert.That(urlTriple, Has.Count.EqualTo(1));
            Assert.That(urlTriple.First().Object.ToString(), Is.EqualTo(Uris.TermdatUrl));

            Assert.That(creatorTriple, Has.Count.EqualTo(1));
            Assert.That(creatorTriple.First().Object.ToString(), Is.EqualTo(Uris.CreatorUrl));

            Assert.That(descriptionTriples, Has.Count.EqualTo(5));
            Assert.That(descriptionTriples.Select(x => x.Object.ToString()),
                Is.EquivalentTo(new[]
                {
                    $"{DataSetConstants.DescriptionDe}@{DataSetConstants.LanguageDe}",
                    $"{DataSetConstants.DescriptionFr}@{DataSetConstants.LanguageFr}",
                    $"{DataSetConstants.DescriptionIt}@{DataSetConstants.LanguageIt}",
                    $"{DataSetConstants.DescriptionEn}@{DataSetConstants.LanguageEn}",
                    $"{DataSetConstants.DescriptionRm}@{DataSetConstants.LanguageRm}"
                }));

            Assert.That(publisherTriple, Has.Count.EqualTo(1));
            Assert.That(publisherTriple.First().Object.ToString(), Is.EqualTo(Uris.Lindas));

            Assert.That(contactPointTriple, Has.Count.EqualTo(1));
            Assert.That(contactPointTriple.First().Object.ToString(), Is.EqualTo(Uris.ContactPointUrl));

            Assert.That(dateCreatedTriple, Has.Count.EqualTo(1));
            Assert.That(dateCreatedTriple.First().Object.ToString(), Is.EqualTo($"{DataSetConstants.DateValue}^^{Uris.Date}"));

            Assert.That(datePublishedTriple, Has.Count.EqualTo(1));
            Assert.That(datePublishedTriple.First().Object.ToString(), Is.EqualTo($"{DataSetConstants.DateValue}^^{Uris.Date}"));

            Assert.That(dateModifiedTriple, Has.Count.EqualTo(1));
            Assert.That(dateModifiedTriple.First().Object.ToString(), Is.EqualTo($"{DateTime.Now:yyyy-MM-dd}^^{Uris.Date}"));

            Assert.That(landingPageTriple, Has.Count.EqualTo(1));
            Assert.That(landingPageTriple.First().Object.ToString(), Is.EqualTo(Uris.LandingPageUrl));

            Assert.That(accessUrlTriple, Has.Count.EqualTo(1));
            Assert.That(accessUrlTriple.First().Object.ToString(), Is.EqualTo(Uris.AccessUrl));

            Assert.That(voidTriple, Has.Count.EqualTo(1));
            Assert.That(voidTriple.First().Object.ToString(), Is.EqualTo(Uris.DataSetUri));
        });
    }
}
