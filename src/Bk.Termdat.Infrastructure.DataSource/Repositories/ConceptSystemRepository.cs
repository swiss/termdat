using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class ConceptSystemRepository : IConceptSystemRepository
{
    private readonly DataContext context;

    public ConceptSystemRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<ConceptSystem>> GetAll()
    {
        return await context.ConceptSystems.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<ConceptSystemWithEntryCount>> GetAllDetails()
    {
        return await context.ConceptSystems
            .Include(cs => cs.Collection)
            .OrderBy(cs => cs.Id)
            .AsNoTracking()
            .Select(cs => new ConceptSystemWithEntryCount
            {
                ConceptSystem = cs,
                EntryCount = cs.ConceptSystemPositions.Sum(csp => csp.Entries.Count)
            })
            .ToListAsync();
    }

    public Task<ConceptSystem> GetById(int id)
    {
        return context.ConceptSystems.SingleOrDefaultAsync(c => c.Id == id);
    }

    public Task<ConceptSystem> GetByIdForEdit(int id)
    {
        return context.ConceptSystems
            .Include(c => c.ConceptSystemPositions)
            .Include(c => c.Collection)
            .SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task Create(ConceptSystem conceptSystem)
    {
        await context.ConceptSystems.AddAsync(conceptSystem);
    }

    public void Update(ConceptSystem existing, ConceptSystem update)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(update);

        context.Entry(existing).CurrentValues.SetValues(update);
        existing.Collection = update.Collection;

        UpdatePositions(existing, update);
    }

    public void Delete(ConceptSystem conceptSystem)
    {
        context.Remove(conceptSystem);
    }

    private void UpdatePositions(ConceptSystem existing, ConceptSystem update)
    {
        foreach (var updatePosition in update.ConceptSystemPositions)
        {
            updatePosition.ConceptSystemId = update.Id;

            var existingPosition = updatePosition.Id > 0 ? existing.ConceptSystemPositions.FirstOrDefault(p => p.Id == updatePosition.Id) : null;

            if (existingPosition == null)
            {
                existing.ConceptSystemPositions.Add(updatePosition);
            }
            else
            {
                context.Entry(existingPosition).CurrentValues.SetValues(updatePosition);
            }
        }

        foreach (var existingPosition in existing.ConceptSystemPositions.Where(p => update.ConceptSystemPositions.All(t => t.Id != p.Id)))
        {
            context.Remove(existingPosition);
        }
    }

}
