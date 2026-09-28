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
    private readonly LindasExportOptions lindasExportOptions;
    private readonly ILogger<TermdatGraphService> logger;
    private readonly IReadOnlyDictionary<string, IAsyncStorageProvider> storageProviders;
    private readonly TermdatGraphOptions termdatGraphOptions;
    private readonly ILindasOperationExecutor lindasOperationExecutor;
    private readonly IChunkDelay chunkDelay;

    public TermdatGraphService(
        ILogger<TermdatGraphService> logger,
        IOptions<LindasExportOptions> lindasExportOptions,
        IOptions<TermdatGraphOptions> termdatGraphOptions,
        ISparqlClientFactory sparqlClientFactory,
        ILindasOperationExecutor lindasOperationExecutor,
        IChunkDelay chunkDelay)
    {
        this.logger = logger;
        this.termdatGraphOptions = termdatGraphOptions.Value;
        this.lindasExportOptions = lindasExportOptions.Value;
        storageProviders = sparqlClientFactory.GetStorageProviders();
        this.lindasOperationExecutor = lindasOperationExecutor;
        this.chunkDelay = chunkDelay;
    }

    public IReadOnlyCollection<string> Targets => storageProviders.Keys.OrderBy(k => k).ToArray();

    public Task DeleteGraph(string target, CancellationToken ct)
    {
        logger.LogInformation("Deleting graph on target {TargetName}", target);
        var storageProvider = storageProviders[target];
        return lindasOperationExecutor.Execute(
            target,
            "DeleteGraph",
            () => storageProvider.DeleteGraphAsync(Uris.TermdatGraph, ct),
            ct);
    }

    public async Task AddCollection(string target, Collection collection, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var graph = new Graph { BaseUri = UriFactory.Create(Uris.TermdatGraph) };

        var triples = collection.GetTriples(graph, lindasExportOptions.AddCubeLinkData);

        var chunks = triples.Chunk(termdatGraphOptions.ChunkSize);
        var current = 0;
        foreach (var chunk in chunks)
        {
            current++;

            logger.LogInformation(
                "Updating graph for collection Code={CollectionCode} on target {TargetName}. Chunk: {Current}",
                collection.Code, target, current);

            var storageProvider = storageProviders[target];
            await lindasOperationExecutor.Execute(
                target,
                $"AddCollection:{collection.Code}:Chunk:{current}",
                () => storageProvider.UpdateGraphAsync(Uris.TermdatGraph, chunk, [], ct),
                ct);
            await chunkDelay.Wait(ct);
        }
    }

    public async Task AddDataSet(string target, CancellationToken ct)
    {
        var graph = new Graph();
        var triples = DataSet.GetTriples(graph).ToList();
        graph.Assert(triples);

        if (lindasExportOptions.AddCubeLinkData)
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

        logger.LogInformation("Updating graph data set on target {TargetName}", target);
        var storageProvider = storageProviders[target];
        await lindasOperationExecutor.Execute(
            target,
            "AddDataSet",
            () => storageProvider.UpdateGraphAsync(Uris.TermdatGraph, triples, [], ct),
            ct);
    }
}
