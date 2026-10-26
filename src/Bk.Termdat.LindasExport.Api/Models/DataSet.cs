using System.Globalization;
using Bk.Termdat.LindasExport.Api.Constants;
using VDS.RDF;

namespace Bk.Termdat.LindasExport.Api.Models;

public static class DataSet
{
    public static IEnumerable<Triple> GetTriples(IGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        return GetTriplesIterator(graph);
    }

    private static IEnumerable<Triple> GetTriplesIterator(IGraph graph)
    {
        var subject = graph.CreateUriNode(UriFactory.Create(Uris.DataSetUri));

        foreach (var type in new[] { Uris.RdfsDataSet, Uris.W3DataSet, Uris.DataSetSubject, Uris.LindasDataSet })
        {
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.Type)),
                graph.CreateUriNode(UriFactory.Create(type)));
        }

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.SparqlEndpoint)),
            graph.CreateUriNode(UriFactory.Create(Uris.AdminQuery)));

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.ExampleResource)),
            graph.CreateUriNode(UriFactory.Create(Uris.ExampleResourceUrl)));

        foreach (var (title, language) in new[]
                 {
                     (DataSetConstants.TitleDe, DataSetConstants.LanguageDe),
                     (DataSetConstants.TitleFr, DataSetConstants.LanguageFr),
                     (DataSetConstants.TitleIt, DataSetConstants.LanguageIt),
                     (DataSetConstants.TitleEn, DataSetConstants.LanguageEn),
                     (DataSetConstants.TitleRm, DataSetConstants.LanguageRm)
                 })
        {
            yield return new Triple(subject,
                graph.CreateUriNode(UriFactory.Create(Uris.Name)),
                graph.CreateLiteralNode(title, language));
        }

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.Url)),
            graph.CreateUriNode(UriFactory.Create(Uris.TermdatUrl)));

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.Creator)),
            graph.CreateUriNode(UriFactory.Create(Uris.CreatorUrl)));

        foreach (var (description, language) in new[]
                 {
                     (DataSetConstants.DescriptionDe, DataSetConstants.LanguageDe),
                     (DataSetConstants.DescriptionFr, DataSetConstants.LanguageFr),
                     (DataSetConstants.DescriptionIt, DataSetConstants.LanguageIt),
                     (DataSetConstants.DescriptionEn, DataSetConstants.LanguageEn),
                     (DataSetConstants.DescriptionRm, DataSetConstants.LanguageRm)
                 })
        {
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(Uris.Description)),
                graph.CreateLiteralNode(description, language));
        }

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.Publisher)),
            graph.CreateUriNode(UriFactory.Create(Uris.Lindas)));

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.ContactPoint)),
            graph.CreateUriNode(UriFactory.Create(Uris.ContactPointUrl)));

        foreach (var datePredicate in new[] { Uris.DateCreated, Uris.DatePublished })
        {
            yield return new Triple(
                subject,
                graph.CreateUriNode(UriFactory.Create(datePredicate)),
                graph.CreateLiteralNode(DataSetConstants.DateValue, UriFactory.Create(Uris.Date)));
        }

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.DateModified)),
            graph.CreateLiteralNode(DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), UriFactory.Create(Uris.Date)));

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.LandingPage)),
            graph.CreateUriNode(UriFactory.Create(Uris.LandingPageUrl)));

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.Access)),
            graph.CreateUriNode(UriFactory.Create(Uris.AccessUrl)));

        yield return new Triple(
            graph.CreateUriNode(UriFactory.Create(Uris.Void)),
            graph.CreateUriNode(UriFactory.Create(Uris.DataSetPredicate)),
            subject);
    }
}
