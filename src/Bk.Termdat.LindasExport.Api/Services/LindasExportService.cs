using Bk.Termdat.Business.Repositories;
using Bk.Termdat.LindasExport.Api.Interfaces;
using Bk.Termdat.LindasExport.Api.Models;

namespace Bk.Termdat.LindasExport.Api.Services;

public class LindasExportService : ILindasExportService
{
    private readonly ILogger<LindasExportService> logger;
    private readonly ICollectionRepository collectionRepository;
    private readonly ITermdatGraphService termdatGraphService;
    private readonly ILindasMapper lindasMapper;

    public LindasExportService(ILogger<LindasExportService> logger, ICollectionRepository collectionRepository, ITermdatGraphService termdatGraphService, ILindasMapper lindasMapper)
    {
        this.logger = logger;
        this.collectionRepository = collectionRepository;
        this.termdatGraphService = termdatGraphService;
        this.lindasMapper = lindasMapper;
    }

    public async Task<LindasExportResult> ExportCollections(CancellationToken ct)
    {
        var successfulTargets = new List<string>();
        var failedTargets = new List<string>();
        var collectionIds = (await collectionRepository.GetIdsForLindasExport()).ToList();

        foreach (var target in termdatGraphService.Targets)
        {
            try
            {
                await ExportTarget(target, collectionIds, ct);
                successfulTargets.Add(target);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                failedTargets.Add(target);
                logger.LogError(ex, "LINDAS export failed for target {TargetName}", target);
            }
        }

        return new LindasExportResult(successfulTargets, failedTargets);
    }

    private async Task ExportTarget(string target, IReadOnlyCollection<int> collectionIds, CancellationToken ct)
    {
        await termdatGraphService.DeleteGraph(target, ct);
        await termdatGraphService.AddDataSet(target, ct);

        var current = 1;
        foreach (var id in collectionIds)
        {
            ct.ThrowIfCancellationRequested();

            logger.LogInformation(
                "Exporting collection {Current}/{Total} to {TargetName} - CollectionId={CollectionId}",
                current++, collectionIds.Count, target, id);

            var collection = await collectionRepository.GetByIdForLindasExport(id);
            if (collection is null)
            {
                continue;
            }

            var sparqlCollection = lindasMapper.Map(collection);
            await termdatGraphService.AddCollection(target, sparqlCollection, ct);
        }
    }
}
