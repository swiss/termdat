using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class OfficeRepository : IOfficeRepository
{
    private readonly DataContext context;

    public OfficeRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<Office> GetById(int id)
    {
        return await context.Offices
            .Include(o => o.Translations)
            .ThenInclude(t => t.Language)
            .SingleOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Office>> GetAll()
    {
        return await context.Offices
            .Include(e => e.Translations)
            .ThenInclude(t => t.Language)
            .OrderBy(o => o.Code)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<int> GetEntryCountById(int id)
    {
        return context.Entries.CountAsync(e => e.OfficeId == id);
    }

    public async Task<IEnumerable<OfficeWithEntryCount>> GetAllDetails()
    {
        return await context.Offices
            .Include(e => e.Translations)
            .ThenInclude(t => t.Language)
            .OrderBy(o => o.Code)
            .AsNoTracking()
            .AsSplitQuery()
            .Select(o => new OfficeWithEntryCount
            {
                Office = o,
                EntryCount = Convert.ToInt32(o.Entries.Count)
            })
            .ToListAsync();
    }

    public void Update(Office existing, Office update)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(update);

        context.Entry(existing).CurrentValues.SetValues(update);

        UpdateTranslationHelper.UpdateTranslations(existing.Translations, update.Translations, existing.Id, "Office");
    }

    public async Task Create(Office office)
    {
        await context.Offices.AddAsync(office);
    }

    public void Delete(Office office)
    {
        context.Offices.Remove(office);
    }
}
