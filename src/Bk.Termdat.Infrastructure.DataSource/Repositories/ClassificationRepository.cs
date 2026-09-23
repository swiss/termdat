using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class ClassificationRepository : IClassificationRepository
{
    private readonly DataContext context;

    public ClassificationRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Classification>> GetAll()
    {
        return await context.Classifications
            .Include(e => e.Translations)
            .ThenInclude(t => t.Language)
            .OrderBy(c => c.Code)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<ClassificationWithEntryCount>> GetAllDetails()
    {
        return await context.Classifications
            .Include(t => t.Translations)
            .ThenInclude(l => l.Language)
            .Include(c => c.Collections)
            .AsNoTracking()
            .AsSplitQuery()
            .Select(c => new ClassificationWithEntryCount
            {
                Classification = c,
                EntryCount = Convert.ToInt32(c.Collections.Sum(e => e.Entries.Count))
            }).ToListAsync();
    }

    public async Task<Classification> GetById(int id)
    {
        return await context.Classifications
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
