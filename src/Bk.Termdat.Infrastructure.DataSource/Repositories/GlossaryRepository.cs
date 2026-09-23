using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class GlossaryRepository : IGlossaryRepository
{
    private readonly DataContext context;

    public GlossaryRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Glossary>> GetAll()
    {
        return await context.Glossaries
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .OrderBy(c => c.Code)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Glossary>> GetAuthorizedByIds(IEnumerable<int> authorizedGlossaryIds)
    {
        var glossaryIdList = authorizedGlossaryIds.ToList();

        return await context.Glossaries
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Where(c => glossaryIdList.Contains(c.Id))
            .OrderBy(c => c.Code)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<GlossaryWithEntryCount>> GetAllDetails()
    {
        return await context.Glossaries
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .OrderBy(c => c.Code)
            .AsNoTracking()
            .AsSplitQuery()
            .Select(g => new GlossaryWithEntryCount
            {
                Glossary = g,
                EntryCount = g.Entries.Count
            })
            .ToListAsync();

    }

    public async Task<IEnumerable<Glossary>> GetByIds(IEnumerable<int> glossaryIds)
    {
        var glossaryIdList = glossaryIds.ToList();

        return await context.Glossaries
            .Where(c => glossaryIdList.Contains(c.Id))
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .ToListAsync();
    }

    public async Task<Glossary> GetById(int id)
    {
        return await context.Glossaries
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task Create(Glossary glossary)
    {
        await context.Glossaries.AddAsync(glossary);
    }

    public void Update(Glossary existing, Glossary update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);

        UpdateTranslationHelper.UpdateTranslations(existing.Translations, update.Translations, existing.Id, "Glossary");
    }

    public void Delete(Glossary glossary)
    {
        context.Remove(glossary);
    }
}
