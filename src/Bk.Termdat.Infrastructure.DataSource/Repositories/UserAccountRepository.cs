using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly DataContext context;

    public UserAccountRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<UserAccount>> GetAll()
    {
        return await context.UserAccounts
            .OrderBy(u => u.Email)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<UserAccount>> GetAllDetailed()
    {
        return await context.UserAccounts
            .Include(u => u.AuthorizedCollections)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(u => u.AuthorizedGlossaries)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .OrderBy(u => u.Email)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<UserAccount> GetDetail(int id)
    {
        return await context.UserAccounts
            .Include(y => y.AuthorizedCollections)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(y => y.AuthorizedGlossaries)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .AsNoTracking()
            .AsSplitQuery()
            .SingleOrDefaultAsync(c => c.Id == id);
    }

    public Task<UserAccount> GetByEmail(string email)
    {
        return context.UserAccounts
            .SingleOrDefaultAsync(ua => ua.Email == email.ToLowerInvariant());
    }

    public Task<UserAccount> GetDetailByEmail(string email)
    {
        return context.UserAccounts
            .Include(y => y.AuthorizedCollections)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(y => y.AuthorizedGlossaries)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .AsNoTracking()
            .AsSplitQuery()
            .SingleOrDefaultAsync(ua => ua.Email == email.ToLowerInvariant());
    }

    public Task<UserAccount> GetDetailByEmailForUpdate(string email)
    {
        return context.UserAccounts
            .Include(y => y.AuthorizedCollections)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(y => y.AuthorizedGlossaries)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .AsSplitQuery()
            .SingleOrDefaultAsync(ua => ua.Email == email.ToLowerInvariant());
    }

    public Task<UserAccount> GetDetailByIdForUpdate(int id)
    {
        return context.UserAccounts
            .Include(y => y.AuthorizedCollections)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .Include(y => y.AuthorizedGlossaries)
                .ThenInclude(c => c.Translations)
                    .ThenInclude(t => t.Language)
            .AsSplitQuery()
            .SingleOrDefaultAsync(ua => ua.Id == id);
    }

    public async Task Create(UserAccount userAccount)
    {
        await context.UserAccounts.AddAsync(userAccount);
    }

    public void Update(UserAccount existing, UserAccount update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);
    }

    public void UpdateWithRelations(UserAccount existing, UserAccount update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);
        UpdateManyToManyRelation(existing.AuthorizedCollections, update.AuthorizedCollections);
        UpdateManyToManyRelation(existing.AuthorizedGlossaries, update.AuthorizedGlossaries);
    }

    private static void UpdateManyToManyRelation<T>(ICollection<T> existingEntities, ICollection<T> updatedEntities)
      where T : Entity
    {
        var existingEntityIds = existingEntities.Select(c => c.Id).ToArray();
        var updatedEntityIds = updatedEntities.Select(c => c.Id).ToArray();
        var addEntityIds = updatedEntityIds.Except(existingEntityIds);
        var removeEntityIds = existingEntityIds.Except(updatedEntityIds);
        foreach (var removeEntityId in removeEntityIds)
        {
            var removeEntity = existingEntities.Single(c => c.Id == removeEntityId);
            existingEntities.Remove(removeEntity);
        }

        foreach (var addEntityId in addEntityIds)
        {
            var addEntity = updatedEntities.Single(c => c.Id == addEntityId);
            existingEntities.Add(addEntity);
        }
    }
}
