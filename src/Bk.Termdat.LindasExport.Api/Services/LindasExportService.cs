using Bk.Termdat.Business.Repositories;
using Bk.Termdat.LindasExport.Api.Interfaces;

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

    public async Task ExportCollections(CancellationToken ct)
    {
        await termdatGraphService.DeleteGraph(ct);

        await termdatGraphService.AddDataSet(ct);

        var collectionIds = (await collectionRepository.GetIdsForLindasExport()).ToList();

        var current = 1;
        var total = collectionIds.Count;
        foreach (var id in collectionIds)
        {
            ct.ThrowIfCancellationRequested();

            logger.LogInformation("Exporting collection {Current}/{Total} - CollectionId={CollectionId}", current++, total, id);

            var collection = await collectionRepository.GetByIdForLindasExport(id);
            if (collection is not null)
            {
                var sparqlCollection = lindasMapper.Map(collection);
                await termdatGraphService.AddCollection(sparqlCollection, ct);
            }
        }
    }
}
