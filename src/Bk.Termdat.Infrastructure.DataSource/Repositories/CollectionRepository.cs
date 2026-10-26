using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly DataContext context;

    public CollectionRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<Collection> GetById(int id)
    {
        return await context.Collections
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Include(c => c.ConceptSystem)
            .Include(c => c.Classification)
            .SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CollectionWithEntryCount> GetByIdForDelete(int id)
    {
        return await context.Collections
            .Where(c => c.Id == id)
            .Select(c => new CollectionWithEntryCount
            {
                Collection = c,
                EntryCount = c.Entries.Count
            })
            .SingleOrDefaultAsync();
    }

    public void Delete(Collection collection)
    {
        context.Remove(collection);
    }

    public async Task UpdateDeletedFlag(Collection collection, bool deleted, string modifiedBy)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var modified = DateTime.UtcNow;
        collection.Deleted = deleted;
        collection.Modified = modified;
        collection.ModifiedBy = modifiedBy;

        // SqlInterpolatedAsync is used because of its much better performance vs ef core change tracking for updating 10k+ entries
        await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE data.entries SET deleted = {deleted}, modified = {modified}, modified_by = {modifiedBy} WHERE collection_id = {collection.Id}");
    }

    public async Task<IEnumerable<Collection>> GetAll()
    {
        return await context.Collections
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Include(c => c.ConceptSystem)
            .OrderBy(c => c.Code)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Collection>> GetAccessibleActiveAndInactive(IEnumerable<int> allowedInactiveIds)
    {
        return await context.Collections
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Include(c => c.ConceptSystem)
            .Include(c => c.UserAccounts)
            .Where(c => !c.Deleted && (c.Active || allowedInactiveIds.Contains(c.Id)))
            .OrderBy(c => c.Code)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<Collection>> GetAllActive()
    {
        return await context.Collections
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Include(c => c.ConceptSystem)
            .Where(c => c.Active && !c.Deleted)
            .OrderBy(c => c.Code)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Collection>> GetAuthorizedByIds(IEnumerable<int> authorizedCollectionIds)
    {
        var collectionIdList = authorizedCollectionIds.ToList();

        return await context.Collections
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Include(c => c.ConceptSystem)
            .Where(c => c.Active && !c.Deleted && collectionIdList.Contains(c.Id))
            .OrderBy(c => c.Code)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Collection>> GetAllActiveApi()
    {
        return await context.Collections
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Where(c => c.ActiveApi && !c.Deleted)
            .OrderBy(c => c.Code)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<CollectionWithEntryCount>> GetAllDetails()
    {
        return await context.Collections
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Include(c => c.ConceptSystem)
            .Include(c => c.Classification)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(c => c.Language)
            .OrderBy(c => c.Code)
            .AsNoTracking()
            .AsSplitQuery()
            .Select(c => new CollectionWithEntryCount
            {
                Collection = c,
                EntryCount = c.Entries.Count
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<Collection>> GetByIds(IEnumerable<int> collectionIds)
    {
        var collectionIdList = collectionIds.ToList();

        return await context.Collections
            .Where(c => collectionIdList.Contains(c.Id))
            .Include(e => e.Translations)
                .ThenInclude(t => t.Language)
            .Include(c => c.ConceptSystem)
            .Include(c => c.Classification)
            .ToListAsync();
    }

    public async Task Create(Collection collection)
    {
        await context.Collections.AddAsync(collection);
    }

    public void Update(Collection existing, Collection update)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(update);

        context.Entry(existing).CurrentValues.SetValues(update);

        UpdateTranslationHelper.UpdateTranslations(existing.Translations, update.Translations, existing.Id, "Collection");
    }

    public async Task<IEnumerable<int>> GetIdsForLindasExport()
    {
        return await context.Collections
            .AsNoTracking()
            .Where(c => c.Active && !c.Deleted && c.ExportLindas)
            .OrderBy(c => c.Code)
            .Select(c => c.Id)
            .ToListAsync();
    }

    public async Task<Collection?> GetByIdForLindasExport(int id)
    {
        return await context.Collections
            .Include(c => c.Translations)
                .ThenInclude(t => t.Language)
            .Include(c => c.Entries)
                .ThenInclude(e => e.Translations)
                    .ThenInclude(t => t.Language)
            .Include(c => c.Entries)
                .ThenInclude(e => e.References)
            .Include(c => c.Entries)
                .ThenInclude(o => o.Office)
                    .ThenInclude(e => e.Translations)
                        .ThenInclude(t => t.Language)
            .Include(c => c.Entries)
                .ThenInclude(o => o.Glossaries)
                    .ThenInclude(e => e.Translations)
                        .ThenInclude(t => t.Language)
            .Include(c => c.Entries)
                .ThenInclude(o => o.Subjects)
                    .ThenInclude(e => e.Translations)
                        .ThenInclude(t => t.Language)
            .Include(c => c.Entries)
                .ThenInclude(o => o.Domains)
                    .ThenInclude(e => e.Translations)
                        .ThenInclude(t => t.Language)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
