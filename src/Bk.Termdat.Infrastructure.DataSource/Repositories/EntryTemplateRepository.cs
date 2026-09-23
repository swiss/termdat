using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class EntryTemplateRepository : IEntryTemplateRepository
{
    private readonly DataContext context;

    public EntryTemplateRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<EntryTemplate>> GetForUser(int userId)
    {
        return await context.EntryTemplates
            .Where(et => et.UserAccountId == userId)
            .OrderBy(et => et.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<EntryTemplate> GetById(int id)
    {
        return await context.EntryTemplates
            .SingleOrDefaultAsync(et => et.Id == id);
    }

    public async Task<bool> TemplateNameExistsForUser(int userId, string templateName)
    {
        return await context.EntryTemplates.AnyAsync(et =>
            et.UserAccountId == userId && et.Name.ToUpper() == templateName.ToUpper());
    }

    public async Task Create(EntryTemplate entryTemplate)
    {
        await context.EntryTemplates.AddAsync(entryTemplate);
    }

    public void Update(EntryTemplate entryTemplate)
    {
        context.EntryTemplates.Update(entryTemplate);
    }

    public void Delete(EntryTemplate entryTemplate)
    {
        context.EntryTemplates.Remove(entryTemplate);
    }
}
