using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly DataContext context;

    public SubjectRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Subject>> GetAll()
    {
        return await context.Subjects
            .Include(s => s.Translations)
                .ThenInclude(t => t.Language)
            .Include(s => s.Domains)
                .ThenInclude(d => d.Translations)
                    .ThenInclude(t => t.Language)
            .OrderBy(o => o.Id)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<SubjectWithEntryCount>> GetAllDetails()
    {
        return await context.Subjects
            .Include(s => s.Translations)
                .ThenInclude(t => t.Language)
            .AsNoTracking()
            .AsSplitQuery()
            .Select(s => new SubjectWithEntryCount
            {
                Subject = s,
                EntryCount = s.Entries.Count
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<Subject>> GetByIds(IEnumerable<int> subjectIds)
    {
        var subjectIdList = subjectIds.ToList();

        return await context.Subjects
            .Where(s => subjectIdList.Contains(s.Id))
            .Include(s => s.Translations)
                .ThenInclude(t => t.Language)
            .ToListAsync();
    }

    public async Task<Subject> GetById(int id)
    {
        return await context.Subjects
            .Include(s => s.Translations)
                .ThenInclude(t => t.Language)
            .Include(s => s.Domains)
                .ThenInclude(d => d.Translations)
                    .ThenInclude(t => t.Language)
            .AsSplitQuery()
            .SingleOrDefaultAsync(s => s.Id == id);
    }

    public async Task Create(Subject subject)
    {
        await context.Subjects.AddAsync(subject);
    }

    public void Update(Subject existing, Subject update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);

        UpdateTranslationHelper.UpdateTranslations(existing.Translations, update.Translations, existing.Id, nameof(Subject));
    }

    public void Delete(Subject subject)
    {
        context.Remove(subject);
    }

    public async Task<int> GetEntryCountById(int id)
    {
        return await context.Entries.CountAsync(e => e.Subjects.Any(s => s.Id == id) || e.Domains.Any(d => d.SubjectId == id));
    }
}
