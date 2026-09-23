using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class DomainRepository : IDomainRepository
{
    private readonly DataContext context;

    public DomainRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Domain>> GetAll()
    {
        return await context.Domains
            .Include(d => d.Translations)
                .ThenInclude(t => t.Language)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<DomainWithEntryCount>> GetAllDetails()
    {
        return await context.Domains
            .Include(d => d.Translations)
                .ThenInclude(t => t.Language)
            .AsNoTracking()
            .AsSplitQuery()
            .Select(d => new DomainWithEntryCount
            {
                Domain = d,
                EntryCount = d.Entries.Count
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<Domain>> GetByIds(IEnumerable<int> domainIds)
    {
        var domainIdList = domainIds.ToList();

        return await context.Domains
            .Where(d => domainIdList.Contains(d.Id))
            .Include(d => d.Translations)
                .ThenInclude(t => t.Language)
            .ToListAsync();
    }

    public async Task<Domain> GetById(int id)
    {
        return await context.Domains
            .Include(d => d.Translations)
                .ThenInclude(t => t.Language)
            .SingleOrDefaultAsync(d => d.Id == id);
    }

    public async Task Create(Domain domain)
    {
        await context.Domains.AddAsync(domain);
    }

    public void Update(Domain existing, Domain update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);

        UpdateTranslationHelper.UpdateTranslations(existing.Translations, update.Translations, existing.Id, nameof(Domain));
    }

    public void Delete(Domain domain)
    {
        context.Remove(domain);
    }

    public async Task<int> GetEntryCountById(int id)
    {
        return await context.Entries.CountAsync(e => e.Domains.Any(d => d.Id == id));
    }
}
