using System.Reflection;
using Bk.Termdat.LindasExport.Api.Configuration;
using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Interfaces;
using Bk.Termdat.LindasExport.Api.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using Collection = Bk.Termdat.LindasExport.Api.Models.Collection;

namespace Bk.Termdat.LindasExport.Api.Services;

public class TermdatGraphService : ITermdatGraphService
{
    private readonly LindasExportOptions _lindasExportOptions;
    private readonly ILogger<TermdatGraphService> _logger;
    private readonly IReadOnlyDictionary<string, IAsyncStorageProvider> _storageProviders;
    private readonly TermdatGraphOptions _termdatGraphOptions;

    public TermdatGraphService(ILogger<TermdatGraphService> logger, IOptions<LindasExportOptions> lindasExportOptions,
        IOptions<TermdatGraphOptions> termdatGraphOptions, ISparqlClientFactory sparqlClientFactory)
    {
        _logger = logger;
        _termdatGraphOptions = termdatGraphOptions.Value;
        _lindasExportOptions = lindasExportOptions.Value;
        _storageProviders = sparqlClientFactory.GetStorageProviders();
    }

    public async Task DeleteGraph(CancellationToken ct)
    {
        foreach (var target in _storageProviders.Keys.OrderBy(k => k))
        {
            try
            {
                var storageProvider = _storageProviders[target];
                await storageProvider.DeleteGraphAsync(Uris.TermdatGraph, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting graph on target {TargetName}", target);
            }
        }
    }

    public async Task AddCollection(Collection collection, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var graph = new Graph { BaseUri = UriFactory.Create(Uris.TermdatGraph) };

        var triples = collection.GetTriples(graph, _lindasExportOptions.AddCubeLinkData);

        var chunks = triples.Chunk(_termdatGraphOptions.ChunkSize);
        var current = 0;
        foreach (var chunk in chunks)
        {
            current++;
            foreach (var target in _storageProviders.Keys.OrderBy(k => k))
            {
                try
                {
                    _logger.LogInformation(
                        "Updating graph for collection Code={CollectionCode} on target {TargetName}. Chunk: {Current}",
                        collection.Code, target, current);

                    var storageProvider = _storageProviders[target];
                    await storageProvider.UpdateGraphAsync(Uris.TermdatGraph, chunk, [], ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating graph on target {TargetName}. Chunk: {Current}", target,
                        current);
                }
            }
        }
    }

    public async Task AddDataSet(CancellationToken ct)
    {
        var graph = new Graph();
        var triples = DataSet.GetTriples(graph).ToList();
        graph.Assert(triples);

        if (_lindasExportOptions.AddCubeLinkData)
        {
            var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
            await using var cubeLinkStream = fileProvider
                .GetFileInfo(Path.Combine("StaticLindasData", "DatasetAndConstraint.ttl")).CreateReadStream();
            using var cubeLinkStreamReader = new StreamReader(cubeLinkStream);

            using var cubeLinkGraph = new Graph();
            var turtleParser = new TurtleParser();
            turtleParser.Load(cubeLinkGraph, cubeLinkStreamReader);

            triples.AddRange(cubeLinkGraph.Triples.ToList());

            triples.Add(new Triple(
                graph.CreateUriNode(UriFactory.Create(Uris.DataSetUriObservation)),
                graph.CreateUriNode(UriFactory.Create(Uris.Type)),
                graph.CreateUriNode(UriFactory.Create(Uris.ObservationSet))));
        }

        foreach (var target in _storageProviders.Keys.OrderBy(k => k))
        {
            try
            {
                _logger.LogInformation("Updating graph data set on target {TargetName}", target);
                var storageProvider = _storageProviders[target];
                await storageProvider.UpdateGraphAsync(Uris.TermdatGraph, triples, [], ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating graph on target {TargetName}", target);
            }
        }
    }
}
