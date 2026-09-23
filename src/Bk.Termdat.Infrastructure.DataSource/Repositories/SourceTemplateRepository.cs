using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class SourceTemplateRepository : ISourceTemplateRepository
{
    private readonly DataContext context;

    public SourceTemplateRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<SourceTemplate>> GetAll(int userId)
    {
        return await context.SourceTemplates
            .Include(s => s.User)
            .Where(s => s.UserId == userId)
            .OrderBy(o => o.Id)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<SourceTemplate> GetById(int id)
    {
        return await context.SourceTemplates
            .Include(s => s.User)
            .AsSplitQuery()
            .SingleOrDefaultAsync(s => s.Id == id);
    }

    public async Task Create(SourceTemplate sourceTemplate)
    {
        await context.SourceTemplates.AddAsync(sourceTemplate);
    }

    public void Update(SourceTemplate existing, SourceTemplate update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);
    }

    public void Delete(SourceTemplate sourceTemplate)
    {
        context.SourceTemplates.Remove(sourceTemplate);
    }
}
