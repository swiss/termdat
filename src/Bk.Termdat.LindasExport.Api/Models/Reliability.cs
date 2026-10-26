using System.Globalization;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Extensions;
using VDS.RDF;

namespace Bk.Termdat.LindasExport.Api.Models;

public class Reliability
{
    public required string Code { get; init; }
    public ICollection<Text> Names { get; init; } = new List<Text>();

    public IEnumerable<Triple> GetTriples(IGraph graph, IUriNode subject)
    {
        ArgumentNullException.ThrowIfNull(graph);

        return GetTriplesIterator(graph, subject);
    }

    private IEnumerable<Triple> GetTriplesIterator(IGraph graph, IUriNode subject)
    {
        var reliabilityCodeSubject = graph.CreateUriNode(UriFactory.Create(string.Format(CultureInfo.InvariantCulture,
            Uris.TermdatEntryAdditionUriFormat,
            Uris.TermdatReliabilityCodeUriAddition, Code)));

        yield return new Triple(
            subject,
            graph.CreateUriNode(UriFactory.Create(Uris.ReliabilityCodeType)),
            reliabilityCodeSubject);

        // Identifier
        yield return new Triple(
            reliabilityCodeSubject,
            graph.CreateUriNode(UriFactory.Create(Uris.Identifier)),
            graph.CreateLiteralNode(Code));

        // Names
        if (Names is not null && Names.Count > 0)
        {
            var namePredicate = graph.CreateUriNode(UriFactory.Create(Uris.Name));
            foreach (var name in Names.Where(item => !string.IsNullOrWhiteSpace(item.Value)))
            {
                yield return new Triple(reliabilityCodeSubject, namePredicate,
                    graph.CreateLiteralNode(name.Value, name.Language.ToLowerCaseStringValue()));
            }
        }
    }

    public static ICollection<Text> GetNamesByReliability(Business.Domain.Models.Reliability reliability)
    {
        var resourceName = reliability switch
        {
            Business.Domain.Models.Reliability.FirstDraft => nameof(BackendTexts.ReliabilityFirstDraft),
            Business.Domain.Models.Reliability.Tentative => nameof(BackendTexts.ReliabilityTentative),
            Business.Domain.Models.Reliability.Translated => nameof(BackendTexts.ReliabilityTranslated),
            Business.Domain.Models.Reliability.Verified => nameof(BackendTexts.ReliabilityVerified),
            Business.Domain.Models.Reliability.Approved => nameof(BackendTexts.ReliabilityApproved),
            _ => ""
        };

        var result = new List<Text>();

        foreach (var language in Enum.GetValues<Language>())
        {
            result.Add(new Text
            {
                Value = BackendTexts.ResourceManager.GetString(resourceName,
                    CultureInfo.GetCultureInfo(Enum.GetName(language)!)) ?? "",
                Language = language
            });
        }

        return result;
    }
}
