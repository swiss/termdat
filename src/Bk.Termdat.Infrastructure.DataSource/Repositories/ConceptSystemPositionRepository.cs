using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class ConceptSystemPositionRepository : IConceptSystemPositionRepository
{
    private readonly DataContext context;

    public ConceptSystemPositionRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<ConceptSystemPosition> GetById(int id)
    {
        return await context.ConceptSystemPositions.FindAsync(id);
    }

    public async Task<IEnumerable<ConceptSystemPosition>> GetByCollectionId(int collectionId)
    {
        var conceptSystem = await context.ConceptSystems
            .SingleOrDefaultAsync(c => c.CollectionId == collectionId);

        if (conceptSystem == null)
        {
            return Array.Empty<ConceptSystemPosition>().ToList();
        }

        return await context.ConceptSystemPositions
            .Where(cs => cs.ConceptSystemId == conceptSystem.Id)
            .OrderBy(cs => cs.SortIndex)
            .ToListAsync();
    }

}
