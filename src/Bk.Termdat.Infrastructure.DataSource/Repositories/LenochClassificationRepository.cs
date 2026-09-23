using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class LenochClassificationRepository : ILenochClassificationRepository
{
    private readonly DataContext context;

    public LenochClassificationRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<LenochClassification>> GetAll()
    {
        return await context.LenochClassifications
            .Include(lc => lc.Translations)
                .ThenInclude(t => t.Language)
            .OrderBy(lc => lc.Code)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<LenochClassification>> GetByIds(IEnumerable<int> lenochClassificationIds)
    {
        var ids = lenochClassificationIds.ToList();

        return await context.LenochClassifications
            .Include(lc => lc.Translations)
                .ThenInclude(t => t.Language)
            .Where(d => ids.Contains(d.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<EntityWithEntryCount<LenochClassification>>> GetAllDetails()
    {
        return await context.LenochClassifications
            .Include(lc => lc.Translations)
                .ThenInclude(t => t.Language)
            .OrderBy(lc => lc.Code)
            .AsNoTracking()
            .AsSplitQuery()
            .Select(lc => new EntityWithEntryCount<LenochClassification>
            {
                Entity = lc,
                EntryCount = lc.Entries.Count
            })
            .ToListAsync();
    }
}
